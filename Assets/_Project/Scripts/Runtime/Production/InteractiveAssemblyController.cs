using System;
using System.Collections.Generic;
using System.Linq;
using ShadowSupply.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Production
{
    [DisallowMultipleComponent]
    public sealed class InteractiveAssemblyController :
        MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Camera assemblyCamera;
        [SerializeField] private Transform assemblySurface;
        [SerializeField] private Vector2 surfaceSize =
            new Vector2(1.8f, 0.72f);
        [SerializeField] private Transform spawnedPartsRoot;
        [SerializeField] private Transform packageRoot;
        [SerializeField] private Transform packageInterior;
        [SerializeField] private BoxCollider packageDropZone;
        [SerializeField] private Transform lidPivot;
        [SerializeField] private Collider lidCollider;
        [SerializeField] private Renderer lidRenderer;

        [Header("Interaction")]
        [SerializeField, Min(0.01f)]
        private float dragHeight = 0.12f;
        [SerializeField, Min(0.01f)]
        private float ingredientVisualSize = 0.19f;
        [SerializeField, Min(0.1f)]
        private float lidCloseDuration = 0.45f;
        [SerializeField] private Vector3 lidOpenEuler =
            new Vector3(-108f, 0f, 0f);
        [SerializeField] private Vector3 lidClosedEuler =
            Vector3.zero;

        private readonly List<InteractiveAssemblyPart> parts =
            new List<InteractiveAssemblyPart>();

        [SerializeField]
        private PoweredWorkbenchProduction workbench;

        private Camera playerCamera;
        private InteractiveAssemblyPart draggedPart;
        private Plane dragPlane;
        private Vector3 dragOffset;
        private bool viewActive;
        private bool lidClosing;
        private float lidCloseElapsed;

        public bool IsViewActive => viewActive;
        public int CompletedStepCount =>
            parts.Count(part => part.IsCompleted);
        public int TotalStepCount => parts.Count;
        public bool AllStepsComplete =>
            parts.Count > 0 &&
            parts.All(part => part.IsCompleted);
        public bool CanClosePackage =>
            AllStepsComplete &&
            workbench != null &&
            workbench.HasPower &&
            !lidClosing;
        public Camera AssemblyCamera => assemblyCamera;

        public string InstructionText
        {
            get
            {
                if (workbench == null)
                {
                    return "Assembly unavailable.";
                }

                if (!workbench.HasPower)
                {
                    return
                        "Production paused — restore workbench power.";
                }

                if (workbench.HasPendingOutput)
                {
                    return
                        "Package complete — collect it from the workbench.";
                }

                if (lidClosing)
                {
                    return "Closing and sealing package…";
                }

                InteractiveAssemblyPart nextTool =
                    parts.FirstOrDefault(
                        part =>
                            !part.IsCompleted &&
                            part.Role ==
                            AssemblyPartRole.ReusableTool
                    );

                if (nextTool != null)
                {
                    return
                        $"Click {nextTool.Item.DisplayName} " +
                        "to prepare the workstation.";
                }

                if (!AllStepsComplete)
                {
                    return
                        "Drag each loose part into the open package.";
                }

                return
                    "All parts are loaded — click the package lid.";
            }
        }

        private void Awake()
        {
            ResolveWorkbenchReference();
            SetAssemblyObjectsVisible(false);

            if (assemblyCamera != null)
            {
                assemblyCamera.enabled = false;
            }

            if (lidPivot != null)
            {
                lidPivot.localRotation =
                    Quaternion.Euler(lidOpenEuler);
            }
        }

        private void Update()
        {
            AnimateLid();

            if (
                !viewActive ||
                workbench == null ||
                workbench.HasPendingOutput ||
                lidClosing ||
                Mouse.current == null
            )
            {
                return;
            }

            RefreshLidAvailability();

            if (
                !workbench.HasPower
            )
            {
                if (draggedPart != null)
                {
                    ReturnDraggedPart();
                }

                return;
            }

            HandlePointer();
        }

        public void Configure(
            PoweredWorkbenchProduction targetWorkbench,
            Camera cameraReference,
            Transform surfaceReference,
            Vector2 configuredSurfaceSize,
            Transform partsRoot,
            Transform configuredPackageRoot,
            Transform configuredPackageInterior,
            BoxCollider dropZone,
            Transform configuredLidPivot,
            Collider configuredLidCollider,
            Renderer configuredLidRenderer
        )
        {
            workbench = targetWorkbench;
            assemblyCamera = cameraReference;
            assemblySurface = surfaceReference;
            surfaceSize = configuredSurfaceSize;
            spawnedPartsRoot = partsRoot;
            packageRoot = configuredPackageRoot;
            packageInterior =
                configuredPackageInterior;
            packageDropZone = dropZone;
            lidPivot = configuredLidPivot;
            lidCollider = configuredLidCollider;
            lidRenderer = configuredLidRenderer;

            if (assemblyCamera != null)
            {
                assemblyCamera.enabled = false;
            }

            SetAssemblyObjectsVisible(false);
            ResetLidVisual();
        }

        public void Activate(Camera currentPlayerCamera)
        {
            ResolveWorkbenchReference();
            playerCamera = currentPlayerCamera;

            if (playerCamera != null)
            {
                playerCamera.enabled = false;
            }

            if (assemblyCamera != null)
            {
                assemblyCamera.enabled = true;
            }

            viewActive = true;
            SetAssemblyObjectsVisible(
                workbench != null &&
                !workbench.HasPendingOutput
            );

            SyncFromWorkbench();
        }

        public void Deactivate()
        {
            viewActive = false;

            if (draggedPart != null)
            {
                ReturnDraggedPart();
            }

            if (assemblyCamera != null)
            {
                assemblyCamera.enabled = false;
            }

            if (playerCamera != null)
            {
                playerCamera.enabled = true;
            }

            playerCamera = null;
        }

        public void SyncFromWorkbench()
        {
            ResolveWorkbenchReference();
            ClearSpawnedParts();

            if (
                workbench == null ||
                workbench.ActiveRecipe == null ||
                workbench.HasPendingOutput
            )
            {
                SetAssemblyObjectsVisible(false);
                return;
            }

            SetAssemblyObjectsVisible(true);
            ResetLidVisual();

            List<AssemblyStepDescriptor> steps =
                BuildStepDescriptors(
                    workbench.ActiveRecipe
                );

            for (
                int index = 0;
                index < steps.Count;
                index++
            )
            {
                InteractiveAssemblyPart part =
                    CreatePart(
                        index,
                        steps[index]
                    );

                bool completed =
                    workbench.CompletedStepIndices.Contains(
                        index
                    );

                if (completed)
                {
                    ApplyCompletedPose(
                        part,
                        index
                    );
                }

                parts.Add(part);
            }

            RefreshLidAvailability();
        }

        public void NotifyOutputReady()
        {
            if (draggedPart != null)
            {
                ReturnDraggedPart();
            }

            ClearSpawnedParts();
            SetAssemblyObjectsVisible(false);
        }

        private bool ResolveWorkbenchReference()
        {
            if (workbench != null)
            {
                return true;
            }

            workbench =
                GetComponentInParent<
                    PoweredWorkbenchProduction
                >(true);

            if (workbench == null)
            {
                Debug.LogWarning(
                    "[InteractiveProduction] The assembly surface " +
                    "could not resolve its parent powered workbench.",
                    this
                );

                return false;
            }

            if (
                workbench.AssemblyController != this
            )
            {
                workbench.SetAssemblyController(this);
            }

            return true;
        }

        private void HandlePointer()
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            Ray ray =
                assemblyCamera.ScreenPointToRay(
                    screenPosition
                );

            if (
                Mouse.current.leftButton
                    .wasPressedThisFrame
            )
            {
                HandlePointerDown(ray);
            }

            if (
                draggedPart != null &&
                Mouse.current.leftButton.isPressed
            )
            {
                DragPart(ray);
            }

            if (
                Mouse.current.leftButton
                    .wasReleasedThisFrame
            )
            {
                HandlePointerUp();
            }
        }

        private void HandlePointerDown(Ray ray)
        {
            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    5f,
                    ~0,
                    QueryTriggerInteraction.Collide
                );

            Array.Sort(
                hits,
                (left, right) =>
                    left.distance.CompareTo(
                        right.distance
                    )
            );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                InteractiveAssemblyPart part =
                    hit.collider.GetComponentInParent<
                        InteractiveAssemblyPart
                    >();

                if (
                    part != null &&
                    !part.IsCompleted
                )
                {
                    if (
                        part.Role ==
                        AssemblyPartRole.ReusableTool
                    )
                    {
                        CompleteToolStep(part);
                        return;
                    }

                    BeginDragging(
                        part,
                        hit.point
                    );

                    return;
                }

                AssemblyLidHandle lid =
                    hit.collider.GetComponentInParent<
                        AssemblyLidHandle
                    >();

                if (
                    lid != null &&
                    CanClosePackage
                )
                {
                    BeginLidClose();
                    return;
                }
            }
        }

        private void BeginDragging(
            InteractiveAssemblyPart part,
            Vector3 hitPoint
        )
        {
            draggedPart = part;
            draggedPart.SetDragging(true);

            float planeHeight =
                assemblySurface != null
                    ? assemblySurface.position.y +
                      dragHeight
                    : part.transform.position.y;

            dragPlane =
                new Plane(
                    Vector3.up,
                    new Vector3(
                        0f,
                        planeHeight,
                        0f
                    )
                );

            dragOffset =
                part.transform.position -
                hitPoint;
        }

        private void DragPart(Ray ray)
        {
            if (
                draggedPart == null ||
                !dragPlane.Raycast(
                    ray,
                    out float enter
                )
            )
            {
                return;
            }

            Vector3 worldPosition =
                ray.GetPoint(enter) +
                dragOffset;

            worldPosition =
                ClampToSurface(worldPosition);

            draggedPart.transform.position =
                worldPosition;
        }

        private void HandlePointerUp()
        {
            if (draggedPart == null)
            {
                return;
            }

            InteractiveAssemblyPart released =
                draggedPart;

            draggedPart = null;
            released.SetDragging(false);

            if (
                packageDropZone != null &&
                packageDropZone.bounds.Contains(
                    released.transform.position
                )
            )
            {
                CompleteIngredientStep(
                    released
                );
                return;
            }

            released.ResetToStart();
        }

        private void CompleteToolStep(
            InteractiveAssemblyPart part
        )
        {
            if (
                part == null ||
                workbench == null ||
                !workbench.MarkAssemblyStepComplete(
                    part.StepIndex
                )
            )
            {
                return;
            }

            part.CompleteAsTool();
            RefreshLidAvailability();
        }

        private void CompleteIngredientStep(
            InteractiveAssemblyPart part
        )
        {
            if (
                part == null ||
                workbench == null ||
                !workbench.MarkAssemblyStepComplete(
                    part.StepIndex
                )
            )
            {
                part?.ResetToStart();
                return;
            }

            ApplyCompletedPose(
                part,
                part.StepIndex
            );

            RefreshLidAvailability();
        }

        private void ApplyCompletedPose(
            InteractiveAssemblyPart part,
            int stepIndex
        )
        {
            if (part == null)
            {
                return;
            }

            if (
                part.Role ==
                AssemblyPartRole.ReusableTool
            )
            {
                part.CompleteAsTool();
                return;
            }

            Vector3 packedPosition =
                GetPackedLocalPosition(
                    stepIndex
                );

            Quaternion packedRotation =
                Quaternion.Euler(
                    0f,
                    (
                        stepIndex % 3 -
                        1
                    ) *
                    13f,
                    0f
                );

            part.CompleteAsIngredient(
                packageInterior,
                packedPosition,
                packedRotation,
                0.48f
            );
        }

        private Vector3 GetPackedLocalPosition(
            int stepIndex
        )
        {
            int ingredientOrder = 0;

            foreach (
                InteractiveAssemblyPart part
                in parts
            )
            {
                if (
                    part == null ||
                    part.StepIndex >= stepIndex
                )
                {
                    continue;
                }

                if (
                    part.Role ==
                    AssemblyPartRole.Ingredient
                )
                {
                    ingredientOrder++;
                }
            }

            int column =
                ingredientOrder % 2;

            int row =
                ingredientOrder / 2;

            return new Vector3(
                column == 0
                    ? -0.11f
                    : 0.11f,
                0.045f +
                row * 0.035f,
                row % 2 == 0
                    ? -0.055f
                    : 0.055f
            );
        }

        private void BeginLidClose()
        {
            if (!CanClosePackage)
            {
                return;
            }

            lidClosing = true;
            lidCloseElapsed = 0f;

            if (lidCollider != null)
            {
                lidCollider.enabled = false;
            }

            RefreshLidAvailability();
        }

        private void AnimateLid()
        {
            if (
                !lidClosing ||
                lidPivot == null
            )
            {
                return;
            }

            lidCloseElapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    lidCloseElapsed /
                    Mathf.Max(
                        0.05f,
                        lidCloseDuration
                    )
                );

            t =
                t *
                t *
                (3f - 2f * t);

            lidPivot.localRotation =
                Quaternion.Slerp(
                    Quaternion.Euler(
                        lidOpenEuler
                    ),
                    Quaternion.Euler(
                        lidClosedEuler
                    ),
                    t
                );

            if (t < 1f)
            {
                return;
            }

            lidClosing = false;
            workbench?.FinalizeInteractiveAssembly();
        }

        private void RefreshLidAvailability()
        {
            bool ready =
                AllStepsComplete &&
                workbench != null &&
                workbench.HasPower &&
                !lidClosing;

            if (lidCollider != null)
            {
                lidCollider.enabled = ready;
            }

            if (lidRenderer != null)
            {
                MaterialPropertyBlock block =
                    new MaterialPropertyBlock();

                lidRenderer.GetPropertyBlock(block);

                Color color =
                    ready
                        ? new Color(
                            1f,
                            0.38f,
                            0.03f
                        )
                        : Color.white;

                block.SetColor(
                    "_BaseColor",
                    color
                );

                block.SetColor(
                    "_Color",
                    color
                );

                lidRenderer.SetPropertyBlock(
                    block
                );
            }
        }

        private Vector3 ClampToSurface(
            Vector3 worldPosition
        )
        {
            if (assemblySurface == null)
            {
                return worldPosition;
            }

            Vector3 local =
                assemblySurface.InverseTransformPoint(
                    worldPosition
                );

            local.x =
                Mathf.Clamp(
                    local.x,
                    -surfaceSize.x * 0.5f,
                    surfaceSize.x * 0.5f
                );

            local.z =
                Mathf.Clamp(
                    local.z,
                    -surfaceSize.y * 0.5f,
                    surfaceSize.y * 0.5f
                );

            local.y = dragHeight;

            return
                assemblySurface.TransformPoint(local);
        }

        private InteractiveAssemblyPart CreatePart(
            int stepIndex,
            AssemblyStepDescriptor step
        )
        {
            GameObject root =
                new GameObject(
                    $"Assembly Step {stepIndex + 1} — " +
                    $"{step.item.DisplayName}"
                );

            root.transform.SetParent(
                spawnedPartsRoot,
                false
            );

            Vector3 localPosition =
                GetSpawnLocalPosition(
                    stepIndex,
                    step.role
                );

            root.transform.localPosition =
                localPosition;

            root.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    (stepIndex * 37f) % 360f,
                    0f
                );

            GameObject visual =
                CreateItemVisual(
                    step.item,
                    root.transform
                );

            NormalizeVisual(
                visual,
                ingredientVisualSize
            );

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center =
                new Vector3(
                    0f,
                    ingredientVisualSize * 0.25f,
                    0f
                );

            collider.size =
                new Vector3(
                    ingredientVisualSize * 1.25f,
                    ingredientVisualSize * 0.7f,
                    ingredientVisualSize * 1.25f
                );

            InteractiveAssemblyPart part =
                root.AddComponent<
                    InteractiveAssemblyPart
                >();

            part.Configure(
                stepIndex,
                step.item,
                step.role,
                collider,
                root.GetComponentsInChildren<
                    Renderer
                >(true)
            );

            part.SetStartPose(
                root.transform.position,
                root.transform.rotation
            );

            return part;
        }

        private Vector3 GetSpawnLocalPosition(
            int stepIndex,
            AssemblyPartRole role
        )
        {
            if (
                role ==
                AssemblyPartRole.ReusableTool
            )
            {
                return new Vector3(
                    -surfaceSize.x * 0.36f,
                    dragHeight,
                    surfaceSize.y * 0.22f
                );
            }

            int ingredientIndex =
                parts.Count(
                    part =>
                        part.Role ==
                        AssemblyPartRole.Ingredient
                );

            int column =
                ingredientIndex % 2;

            int row =
                ingredientIndex / 2;

            float x =
                -surfaceSize.x * 0.34f +
                column * 0.3f;

            float z =
                -surfaceSize.y * 0.23f +
                row * 0.22f;

            return new Vector3(
                x,
                dragHeight,
                Mathf.Clamp(
                    z,
                    -surfaceSize.y * 0.34f,
                    surfaceSize.y * 0.34f
                )
            );
        }

        private static GameObject CreateItemVisual(
            ItemDefinition item,
            Transform parent
        )
        {
            GameObject visual;

            if (
                item != null &&
                item.DisplayPrefab != null
            )
            {
                visual =
                    Instantiate(
                        item.DisplayPrefab,
                        parent
                    );
            }
            else
            {
                visual =
                    GameObject.CreatePrimitive(
                        item != null
                            ? item.FallbackPrimitive
                            : PrimitiveType.Cube
                    );

                visual.transform.SetParent(
                    parent,
                    false
                );

                Renderer renderer =
                    visual.GetComponent<Renderer>();

                if (
                    renderer != null &&
                    item != null
                )
                {
                    MaterialPropertyBlock block =
                        new MaterialPropertyBlock();

                    renderer.GetPropertyBlock(block);

                    block.SetColor(
                        "_BaseColor",
                        item.FallbackColor
                    );

                    block.SetColor(
                        "_Color",
                        item.FallbackColor
                    );

                    renderer.SetPropertyBlock(block);
                }
            }

            visual.name =
                item != null
                    ? item.DisplayName + " Visual"
                    : "Assembly Part Visual";

            visual.transform.localPosition =
                Vector3.zero;

            visual.transform.localRotation =
                Quaternion.identity;

            foreach (
                Rigidbody body
                in visual.GetComponentsInChildren<
                    Rigidbody
                >(true)
            )
            {
                UnityEngine.Object.Destroy(body);
            }

            foreach (
                Collider collider
                in visual.GetComponentsInChildren<
                    Collider
                >(true)
            )
            {
                UnityEngine.Object.Destroy(collider);
            }

            foreach (
                MonoBehaviour behaviour
                in visual.GetComponentsInChildren<
                    MonoBehaviour
                >(true)
            )
            {
                if (
                    behaviour != null &&
                    behaviour is ShadowSupply.Interaction
                        .IInteractable
                )
                {
                    UnityEngine.Object.Destroy(behaviour);
                }
            }

            return visual;
        }

        private static void NormalizeVisual(
            GameObject visual,
            float targetSize
        )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<
                    Renderer
                >(true);

            if (renderers.Length == 0)
            {
                visual.transform.localScale =
                    Vector3.one *
                    targetSize;

                return;
            }

            Bounds bounds =
                renderers[0].bounds;

            for (
                int index = 1;
                index < renderers.Length;
                index++
            )
            {
                bounds.Encapsulate(
                    renderers[index].bounds
                );
            }

            float largest =
                Mathf.Max(
                    bounds.size.x,
                    bounds.size.y,
                    bounds.size.z
                );

            if (largest <= 0.0001f)
            {
                return;
            }

            float multiplier =
                targetSize / largest;

            visual.transform.localScale *=
                multiplier;

            renderers =
                visual.GetComponentsInChildren<
                    Renderer
                >(true);

            bounds = renderers[0].bounds;

            for (
                int index = 1;
                index < renderers.Length;
                index++
            )
            {
                bounds.Encapsulate(
                    renderers[index].bounds
                );
            }

            visual.transform.position +=
                Vector3.up *
                (
                    visual.transform.parent.position.y -
                    bounds.min.y
                );
        }

        private static List<AssemblyStepDescriptor>
            BuildStepDescriptors(
                ProductionRecipe recipe
            )
        {
            List<AssemblyStepDescriptor> steps =
                new List<AssemblyStepDescriptor>();

            foreach (
                ProductionIngredient ingredient
                in recipe.Ingredients
            )
            {
                if (
                    ingredient == null ||
                    ingredient.Item == null
                )
                {
                    continue;
                }

                AssemblyPartRole role =
                    ingredient.Consumed
                        ? AssemblyPartRole.Ingredient
                        : AssemblyPartRole.ReusableTool;

                for (
                    int index = 0;
                    index < ingredient.Quantity;
                    index++
                )
                {
                    steps.Add(
                        new AssemblyStepDescriptor(
                            ingredient.Item,
                            role
                        )
                    );
                }
            }

            return steps;
        }

        private void ClearSpawnedParts()
        {
            draggedPart = null;
            parts.Clear();

            if (spawnedPartsRoot == null)
            {
                return;
            }

            for (
                int index =
                    spawnedPartsRoot.childCount - 1;
                index >= 0;
                index--
            )
            {
                Destroy(
                    spawnedPartsRoot
                        .GetChild(index)
                        .gameObject
                );
            }

            if (packageInterior != null)
            {
                for (
                    int index =
                        packageInterior.childCount - 1;
                    index >= 0;
                    index--
                )
                {
                    InteractiveAssemblyPart part =
                        packageInterior
                            .GetChild(index)
                            .GetComponent<
                                InteractiveAssemblyPart
                            >();

                    if (part != null)
                    {
                        Destroy(part.gameObject);
                    }
                }
            }
        }

        private void SetAssemblyObjectsVisible(
            bool visible
        )
        {
            if (spawnedPartsRoot != null)
            {
                spawnedPartsRoot.gameObject
                    .SetActive(visible);
            }

            if (packageRoot != null)
            {
                packageRoot.gameObject
                    .SetActive(visible);
            }
        }

        private void ResetLidVisual()
        {
            lidClosing = false;
            lidCloseElapsed = 0f;

            if (lidPivot != null)
            {
                lidPivot.localRotation =
                    Quaternion.Euler(
                        lidOpenEuler
                    );
            }

            RefreshLidAvailability();
        }

        private void ReturnDraggedPart()
        {
            if (draggedPart == null)
            {
                return;
            }

            InteractiveAssemblyPart part =
                draggedPart;

            draggedPart = null;
            part.SetDragging(false);
            part.ResetToStart();
        }

        private readonly struct
            AssemblyStepDescriptor
        {
            public readonly ItemDefinition item;
            public readonly AssemblyPartRole role;

            public AssemblyStepDescriptor(
                ItemDefinition definition,
                AssemblyPartRole partRole
            )
            {
                item = definition;
                role = partRole;
            }
        }
    }
}
