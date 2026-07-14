using System;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace ShadowSupply.Placement
{
    public sealed class PlacementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera placementCamera;
        [SerializeField] private PlaceableDatabase database;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private HotbarController hotbarController;
        [SerializeField] private PlayerInteractor playerInteractor;

        [Header("Raycast")]
        [SerializeField, Min(1f)] private float placementDistance = 8f;
        [SerializeField] private LayerMask placementMask = ~0;

        [Header("Validation")]
        [SerializeField, Min(0f)] private float surfaceOffset = 0.015f;
        [SerializeField, Range(0f, 0.2f)] private float collisionPadding = 0.035f;
        [SerializeField] private bool snapToGrid = true;

        [Header("Preview Colors")]
        [SerializeField] private Color validPreviewColor =
            new Color(0.18f, 0.9f, 0.42f, 1f);
        [SerializeField] private Color invalidPreviewColor =
            new Color(0.95f, 0.18f, 0.12f, 1f);

        private InputAction toggleBuildAction;
        private InputAction placeAction;
        private InputAction cancelAction;
        private InputAction rotateAction;
        private InputAction removeAction;

        private GameObject previewObject;
        private Renderer[] previewRenderers =
            Array.Empty<Renderer>();
        private MaterialPropertyBlock previewPropertyBlock;

        private int selectedDefinitionIndex;
        private float previewRotationOffset;
        private bool previewValid;
        private bool hasSurfaceHit;
        private bool hotbarWasEnabled;
        private bool interactorWasEnabled;
        private string validationMessage =
            "Press B to enter placement mode.";
        private string transientMessage;
        private float transientMessageUntil;

        public event Action<bool> BuildModeChanged;
        public event Action PreviewChanged;

        public bool BuildModeActive { get; private set; }
        public bool PreviewValid => previewValid;
        public bool HasSurfaceHit => hasSurfaceHit;
        public int SelectedDefinitionIndex => selectedDefinitionIndex;
        public PlaceableDatabase Database => database;
        public PlaceableDefinition CurrentDefinition =>
            database != null
                ? database.GetAt(selectedDefinitionIndex)
                : null;
        public string StatusMessage =>
            Time.unscaledTime < transientMessageUntil
                ? transientMessage
                : validationMessage;

        private void Awake()
        {
            placementCamera ??=
                GetComponentInChildren<Camera>(true);

            inventory ??=
                GetComponent<PlayerInventory>();

            hotbarController ??=
                GetComponent<HotbarController>();

            playerInteractor ??=
                GetComponent<PlayerInteractor>();

            previewPropertyBlock =
                new MaterialPropertyBlock();

            BuildInputActions();
        }

        private void OnEnable()
        {
            toggleBuildAction.Enable();
            placeAction.Enable();
            cancelAction.Enable();
            rotateAction.Enable();
            removeAction.Enable();
        }

        private void OnDisable()
        {
            toggleBuildAction.Disable();
            placeAction.Disable();
            cancelAction.Disable();
            rotateAction.Disable();
            removeAction.Disable();

            ExitBuildMode();
        }

        private void OnDestroy()
        {
            toggleBuildAction?.Dispose();
            placeAction?.Dispose();
            cancelAction?.Dispose();
            rotateAction?.Dispose();
            removeAction?.Dispose();
        }

        private void Update()
        {
            if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
            {
                SetPreviewVisible(false);
                return;
            }

            if (toggleBuildAction.WasPressedThisFrame())
            {
                if (BuildModeActive)
                {
                    ExitBuildMode();
                }
                else
                {
                    EnterBuildMode();
                }
            }

            if (!BuildModeActive)
            {
                return;
            }

            SetPreviewVisible(true);

            if (cancelAction.WasPressedThisFrame())
            {
                ExitBuildMode();
                return;
            }

            HandleDefinitionSelection();
            HandleRotation();
            UpdatePreview();

            if (removeAction.WasPressedThisFrame())
            {
                PackTargetedPlacedObject();
            }

            if (
                placeAction.WasPressedThisFrame() &&
                previewValid
            )
            {
                PlaceCurrentObject();
            }
        }

        public void Configure(
            Camera cameraReference,
            PlaceableDatabase placeableDatabase
        )
        {
            Configure(
                cameraReference,
                placeableDatabase,
                inventory ?? GetComponent<PlayerInventory>()
            );
        }

        public void Configure(
            Camera cameraReference,
            PlaceableDatabase placeableDatabase,
            PlayerInventory playerInventory
        )
        {
            placementCamera = cameraReference;
            database = placeableDatabase;
            inventory =
                playerInventory ??
                GetComponent<PlayerInventory>();
        }

        public int GetOwnedCount(
            PlaceableDefinition definition
        )
        {
            if (definition == null)
            {
                return 0;
            }

            if (!definition.RequiresOwnership)
            {
                return int.MaxValue;
            }

            return
                inventory != null
                    ? inventory.CountItem(
                        definition.InventoryItem
                    )
                    : 0;
        }

        public void EnterBuildMode()
        {
            if (
                BuildModeActive ||
                database == null ||
                database.Definitions.Count == 0
            )
            {
                return;
            }

            BuildModeActive = true;
            selectedDefinitionIndex =
                FindFirstOwnedDefinitionIndex();

            if (selectedDefinitionIndex < 0)
            {
                selectedDefinitionIndex = 0;
            }

            hotbarWasEnabled =
                hotbarController != null &&
                hotbarController.enabled;

            interactorWasEnabled =
                playerInteractor != null &&
                playerInteractor.enabled;

            if (hotbarController != null)
            {
                hotbarController.enabled = false;
            }

            if (playerInteractor != null)
            {
                playerInteractor.enabled = false;
            }

            previewRotationOffset = 0f;
            CreatePreview();

            PlaceableDefinition definition =
                CurrentDefinition;

            SetTransientMessage(
                definition != null
                    ? $"Selected {definition.DisplayName}. " +
                      $"Owned: {GetOwnedCount(definition)}"
                    : "No placeable selected.",
                2.5f
            );

            BuildModeChanged?.Invoke(true);
        }

        public void ExitBuildMode()
        {
            if (!BuildModeActive && previewObject == null)
            {
                return;
            }

            BuildModeActive = false;
            previewValid = false;
            hasSurfaceHit = false;
            validationMessage =
                "Press B to enter placement mode.";

            DestroyPreview();

            if (
                hotbarController != null &&
                hotbarWasEnabled
            )
            {
                hotbarController.enabled = true;
            }

            if (
                playerInteractor != null &&
                interactorWasEnabled
            )
            {
                playerInteractor.enabled = true;
            }

            BuildModeChanged?.Invoke(false);
            PreviewChanged?.Invoke();
        }

        private void BuildInputActions()
        {
            toggleBuildAction =
                new InputAction(
                    "Toggle Placement",
                    InputActionType.Button,
                    "<Keyboard>/b"
                );

            placeAction =
                new InputAction(
                    "Place Object",
                    InputActionType.Button,
                    "<Mouse>/leftButton"
                );

            cancelAction =
                new InputAction(
                    "Cancel Placement",
                    InputActionType.Button,
                    "<Mouse>/rightButton"
                );

            rotateAction =
                new InputAction(
                    "Rotate Placement",
                    InputActionType.Button,
                    "<Keyboard>/r"
                );

            removeAction =
                new InputAction(
                    "Pack Placed Object",
                    InputActionType.Button,
                    "<Keyboard>/delete"
                );

            removeAction.AddBinding("<Keyboard>/backspace");
        }

        private void HandleDefinitionSelection()
        {
            Keyboard keyboard = Keyboard.current;

            if (
                keyboard == null ||
                database == null
            )
            {
                return;
            }

            int requestedIndex = -1;

            if (keyboard.digit1Key.wasPressedThisFrame) requestedIndex = 0;
            else if (keyboard.digit2Key.wasPressedThisFrame) requestedIndex = 1;
            else if (keyboard.digit3Key.wasPressedThisFrame) requestedIndex = 2;
            else if (keyboard.digit4Key.wasPressedThisFrame) requestedIndex = 3;
            else if (keyboard.digit5Key.wasPressedThisFrame) requestedIndex = 4;
            else if (keyboard.digit6Key.wasPressedThisFrame) requestedIndex = 5;

            if (
                requestedIndex < 0 ||
                requestedIndex >= database.Definitions.Count ||
                requestedIndex == selectedDefinitionIndex
            )
            {
                return;
            }

            selectedDefinitionIndex = requestedIndex;
            previewRotationOffset = 0f;
            CreatePreview();

            PlaceableDefinition definition =
                CurrentDefinition;

            SetTransientMessage(
                definition != null
                    ? $"Selected {definition.DisplayName}. " +
                      $"Owned: {GetOwnedCount(definition)}"
                    : "No placeable selected.",
                2.5f
            );
        }

        private void HandleRotation()
        {
            PlaceableDefinition definition =
                CurrentDefinition;

            if (definition == null)
            {
                return;
            }

            float direction = 0f;

            if (rotateAction.WasPressedThisFrame())
            {
                direction =
                    Keyboard.current != null &&
                    (
                        Keyboard.current.leftShiftKey.isPressed ||
                        Keyboard.current.rightShiftKey.isPressed
                    )
                        ? -1f
                        : 1f;
            }

            Mouse mouse = Mouse.current;

            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;

                if (scroll > 0.01f)
                {
                    direction = 1f;
                }
                else if (scroll < -0.01f)
                {
                    direction = -1f;
                }
            }

            if (Mathf.Approximately(direction, 0f))
            {
                return;
            }

            previewRotationOffset =
                Mathf.Repeat(
                    previewRotationOffset +
                    definition.RotationStep * direction,
                    360f
                );
        }

        private void UpdatePreview()
        {
            PlaceableDefinition definition =
                CurrentDefinition;

            if (
                definition == null ||
                definition.Prefab == null ||
                placementCamera == null ||
                previewObject == null
            )
            {
                SetInvalid(
                    "Placement definition is incomplete.",
                    false
                );
                return;
            }

            Ray ray = new Ray(
                placementCamera.transform.position,
                placementCamera.transform.forward
            );

            if (
                !Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    placementDistance,
                    placementMask,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                SetInvalid(
                    "Aim at a valid surface within placement range.",
                    false
                );
                return;
            }

            hasSurfaceHit = true;

            if (
                hit.collider.GetComponentInParent<PlacedObject>() != null
            )
            {
                SetInvalid(
                    "Cannot place an object on another placed object.",
                    true,
                    hit.point,
                    previewObject.transform.rotation
                );
                return;
            }

            if (!IsSurfaceValid(definition, hit.normal))
            {
                SetInvalid(
                    GetSurfaceFailureMessage(definition),
                    true,
                    hit.point,
                    previewObject.transform.rotation
                );
                return;
            }

            Quaternion rotation =
                BuildPreviewRotation(
                    definition,
                    hit.normal
                );

            Vector3 position =
                hit.point +
                hit.normal * surfaceOffset;

            if (snapToGrid)
            {
                position =
                    definition.SurfaceType ==
                    PlacementSurfaceType.Wall
                        ? SnapWallPosition(
                            position,
                            hit.normal,
                            definition.GridSize
                        )
                        : SnapFloorPosition(
                            position,
                            definition.GridSize
                        );
            }

            ApplyPreviewTransform(
                position,
                rotation
            );

            int ownedCount =
                GetOwnedCount(definition);

            if (
                definition.RequiresOwnership &&
                ownedCount <= 0
            )
            {
                SetInvalid(
                    $"You do not own a packaged " +
                    $"{definition.DisplayName}.",
                    true,
                    position,
                    rotation
                );
                return;
            }

            previewValid =
                IsPlacementClear(
                    definition,
                    position,
                    rotation,
                    hit.collider
                );

            validationMessage =
                previewValid
                    ? $"Valid placement. Owned: {ownedCount}"
                    : "Placement is blocked.";

            ApplyPreviewColor(previewValid);
            PreviewChanged?.Invoke();
        }

        private bool IsSurfaceValid(
            PlaceableDefinition definition,
            Vector3 normal
        )
        {
            float upDot =
                Vector3.Dot(
                    normal.normalized,
                    Vector3.up
                );

            switch (definition.SurfaceType)
            {
                case PlacementSurfaceType.Floor:
                    float slope =
                        Vector3.Angle(
                            normal,
                            Vector3.up
                        );

                    return
                        upDot > 0f &&
                        slope <= definition.MaximumSlope;

                case PlacementSurfaceType.Wall:
                    return Mathf.Abs(upDot) <= 0.35f;

                case PlacementSurfaceType.Ceiling:
                    return upDot <= -0.75f;

                default:
                    return false;
            }
        }

        private static string GetSurfaceFailureMessage(
            PlaceableDefinition definition
        )
        {
            switch (definition.SurfaceType)
            {
                case PlacementSurfaceType.Wall:
                    return "This item must be mounted on a wall.";
                case PlacementSurfaceType.Ceiling:
                    return "This item must be mounted on a ceiling.";
                default:
                    return "This item must be placed on a floor.";
            }
        }

        private Quaternion BuildPreviewRotation(
            PlaceableDefinition definition,
            Vector3 surfaceNormal
        )
        {
            switch (definition.SurfaceType)
            {
                case PlacementSurfaceType.Wall:
                    Quaternion wallFacing =
                        Quaternion.LookRotation(
                            surfaceNormal.normalized,
                            Vector3.up
                        );

                    return
                        wallFacing *
                        Quaternion.AngleAxis(
                            previewRotationOffset,
                            Vector3.forward
                        );

                case PlacementSurfaceType.Ceiling:
                    Quaternion ceilingFacing =
                        Quaternion.FromToRotation(
                            Vector3.up,
                            -surfaceNormal.normalized
                        );

                    return
                        ceilingFacing *
                        Quaternion.AngleAxis(
                            previewRotationOffset,
                            Vector3.up
                        );

                default:
                    return Quaternion.Euler(
                        0f,
                        previewRotationOffset,
                        0f
                    );
            }
        }

        private bool IsPlacementClear(
            PlaceableDefinition definition,
            Vector3 position,
            Quaternion rotation,
            Collider supportCollider
        )
        {
            Vector3 halfExtents =
                definition.BoundsSize * 0.5f;

            halfExtents.x =
                Mathf.Max(
                    0.01f,
                    halfExtents.x - collisionPadding
                );

            halfExtents.y =
                Mathf.Max(
                    0.01f,
                    halfExtents.y - collisionPadding
                );

            halfExtents.z =
                Mathf.Max(
                    0.01f,
                    halfExtents.z - collisionPadding
                );

            Vector3 center =
                position +
                rotation * definition.BoundsCenter;

            Collider[] overlaps =
                Physics.OverlapBox(
                    center,
                    halfExtents,
                    rotation,
                    placementMask,
                    QueryTriggerInteraction.Ignore
                );

            foreach (Collider overlap in overlaps)
            {
                if (
                    overlap == null ||
                    overlap == supportCollider ||
                    overlap.transform.IsChildOf(transform) ||
                    (
                        previewObject != null &&
                        overlap.transform.IsChildOf(
                            previewObject.transform
                        )
                    )
                )
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void PlaceCurrentObject()
        {
            PlaceableDefinition definition =
                CurrentDefinition;

            if (
                definition == null ||
                previewObject == null ||
                !previewValid
            )
            {
                return;
            }

            ItemStack consumed = null;

            if (definition.RequiresOwnership)
            {
                if (
                    inventory == null ||
                    !inventory.TryRemoveItem(
                        definition.InventoryItem,
                        1,
                        out consumed
                    )
                )
                {
                    SetTransientMessage(
                        $"You do not own {definition.DisplayName}.",
                        2.5f
                    );
                    return;
                }
            }

            PlacedObject placedObject =
                PlacedObject.Spawn(
                    definition,
                    previewObject.transform.position,
                    previewObject.transform.rotation
                );

            if (placedObject == null)
            {
                if (
                    consumed != null &&
                    inventory != null
                )
                {
                    inventory.AddItem(
                        consumed.Item,
                        consumed.Quantity,
                        consumed.Quality,
                        consumed.Condition
                    );
                }

                SetTransientMessage(
                    "Placement failed. Item returned.",
                    2.5f
                );
                return;
            }

            int remaining =
                GetOwnedCount(definition);

            SetTransientMessage(
                $"Placed {definition.DisplayName}. " +
                $"Owned: {remaining}",
                2.5f
            );

            UpdatePreview();
        }

        private void PackTargetedPlacedObject()
        {
            if (
                placementCamera == null ||
                inventory == null
            )
            {
                return;
            }

            Ray ray = new Ray(
                placementCamera.transform.position,
                placementCamera.transform.forward
            );

            if (
                !Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    placementDistance,
                    placementMask,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                SetTransientMessage(
                    "No placed object targeted.",
                    2f
                );
                return;
            }

            PlacedObject placedObject =
                hit.collider.GetComponentInParent<PlacedObject>();

            if (
                placedObject == null ||
                placedObject.Definition == null
            )
            {
                SetTransientMessage(
                    "No placed object targeted.",
                    2f
                );
                return;
            }

            PlaceableDefinition definition =
                placedObject.Definition;

            if (
                definition.InventoryItem == null ||
                !inventory.HasSpaceFor(
                    definition.InventoryItem,
                    1
                )
            )
            {
                SetTransientMessage(
                    "Inventory is full. Furniture was not packed.",
                    2.5f
                );
                return;
            }

            int remaining =
                inventory.AddItem(
                    definition.InventoryItem,
                    1,
                    ItemQuality.Standard,
                    1f
                );

            if (remaining > 0)
            {
                SetTransientMessage(
                    "Inventory is full. Furniture was not packed.",
                    2.5f
                );
                return;
            }

            placedObject.gameObject.SetActive(false);
            Destroy(placedObject.gameObject);

            SetTransientMessage(
                $"Packed {definition.DisplayName}.",
                2.5f
            );
        }

        private void CreatePreview()
        {
            DestroyPreview();

            PlaceableDefinition definition =
                CurrentDefinition;

            if (
                definition == null ||
                definition.Prefab == null
            )
            {
                return;
            }

            previewObject =
                Instantiate(definition.Prefab);

            previewObject.name =
                $"Preview_{definition.DisplayName}";

            foreach (
                Collider collider
                in previewObject.GetComponentsInChildren<Collider>(true)
            )
            {
                collider.enabled = false;
            }

            foreach (
                Rigidbody body
                in previewObject.GetComponentsInChildren<Rigidbody>(true)
            )
            {
                body.useGravity = false;
                body.isKinematic = true;
                body.detectCollisions = false;
            }

            foreach (
                MonoBehaviour behaviour
                in previewObject.GetComponentsInChildren<MonoBehaviour>(true)
            )
            {
                if (behaviour != this)
                {
                    behaviour.enabled = false;
                }
            }

            previewRenderers =
                previewObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in previewRenderers)
            {
                renderer.shadowCastingMode =
                    ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            ApplyPreviewColor(false);
        }

        private void DestroyPreview()
        {
            if (previewObject != null)
            {
                Destroy(previewObject);
            }

            previewObject = null;
            previewRenderers =
                Array.Empty<Renderer>();
        }

        private void SetInvalid(
            string message,
            bool hasHit,
            Vector3 position = default,
            Quaternion rotation = default
        )
        {
            previewValid = false;
            hasSurfaceHit = hasHit;
            validationMessage = message;

            if (hasHit && previewObject != null)
            {
                ApplyPreviewTransform(
                    position,
                    rotation
                );
            }

            ApplyPreviewColor(false);
            PreviewChanged?.Invoke();
        }

        private void ApplyPreviewTransform(
            Vector3 position,
            Quaternion rotation
        )
        {
            if (previewObject == null)
            {
                return;
            }

            previewObject.transform.SetPositionAndRotation(
                position,
                rotation
            );
        }

        private void ApplyPreviewColor(bool valid)
        {
            Color color =
                valid
                    ? validPreviewColor
                    : invalidPreviewColor;

            foreach (Renderer renderer in previewRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(
                    previewPropertyBlock
                );

                previewPropertyBlock.SetColor(
                    "_BaseColor",
                    color
                );

                previewPropertyBlock.SetColor(
                    "_Color",
                    color
                );

                previewPropertyBlock.SetColor(
                    "_EmissionColor",
                    color * 0.15f
                );

                renderer.SetPropertyBlock(
                    previewPropertyBlock
                );
            }
        }

        private void SetPreviewVisible(bool visible)
        {
            if (
                previewObject != null &&
                previewObject.activeSelf != visible
            )
            {
                previewObject.SetActive(visible);
            }
        }

        private void SetTransientMessage(
            string message,
            float duration
        )
        {
            transientMessage = message;
            transientMessageUntil =
                Time.unscaledTime +
                Mathf.Max(0f, duration);

            PreviewChanged?.Invoke();
        }

        private int FindFirstOwnedDefinitionIndex()
        {
            if (database == null)
            {
                return -1;
            }

            for (
                int index = 0;
                index < database.Definitions.Count;
                index++
            )
            {
                PlaceableDefinition definition =
                    database.GetAt(index);

                if (
                    definition != null &&
                    GetOwnedCount(definition) > 0
                )
                {
                    return index;
                }
            }

            return -1;
        }

        private static Vector3 SnapFloorPosition(
            Vector3 position,
            float gridSize
        )
        {
            float size =
                Mathf.Max(0.05f, gridSize);

            position.x =
                Mathf.Round(position.x / size) * size;

            position.z =
                Mathf.Round(position.z / size) * size;

            return position;
        }

        private static Vector3 SnapWallPosition(
            Vector3 position,
            Vector3 normal,
            float gridSize
        )
        {
            float size =
                Mathf.Max(0.05f, gridSize);

            Vector3 wallNormal =
                normal.normalized;

            Vector3 right =
                Vector3.Cross(
                    Vector3.up,
                    wallNormal
                ).normalized;

            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            float normalDistance =
                Vector3.Dot(
                    position,
                    wallNormal
                );

            float rightDistance =
                Mathf.Round(
                    Vector3.Dot(position, right) / size
                ) * size;

            float verticalDistance =
                Mathf.Round(position.y / size) * size;

            return
                wallNormal * normalDistance +
                right * rightDistance +
                Vector3.up * verticalDistance;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (
                !BuildModeActive ||
                CurrentDefinition == null ||
                previewObject == null
            )
            {
                return;
            }

            Gizmos.color =
                previewValid
                    ? Color.green
                    : Color.red;

            Matrix4x4 previous =
                Gizmos.matrix;

            Gizmos.matrix =
                Matrix4x4.TRS(
                    previewObject.transform.position,
                    previewObject.transform.rotation,
                    Vector3.one
                );

            Gizmos.DrawWireCube(
                CurrentDefinition.BoundsCenter,
                CurrentDefinition.BoundsSize
            );

            Gizmos.matrix = previous;
        }
#endif
    }
}
