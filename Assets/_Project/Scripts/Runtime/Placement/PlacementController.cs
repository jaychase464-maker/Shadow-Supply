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
        [SerializeField] private HotbarController hotbarController;
        [SerializeField] private PlayerInteractor playerInteractor;

        [Header("Raycast")]
        [SerializeField, Min(1f)] private float placementDistance = 8f;
        [SerializeField] private LayerMask placementMask = ~0;

        [Header("Validation")]
        [SerializeField, Min(0f)] private float surfaceOffset = 0.01f;
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
        private float previewYaw;
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
                RemoveTargetedPlacedObject();
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
            placementCamera = cameraReference;
            database = placeableDatabase;
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
                Mathf.Clamp(
                    selectedDefinitionIndex,
                    0,
                    database.Definitions.Count - 1
                );

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

            previewYaw = 0f;
            CreatePreview();
            SetTransientMessage(
                $"Selected {CurrentDefinition.DisplayName}.",
                2f
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
                    "Remove Placed Object",
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

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                requestedIndex = 0;
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                requestedIndex = 1;
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                requestedIndex = 2;
            }

            if (
                requestedIndex < 0 ||
                requestedIndex >= database.Definitions.Count ||
                requestedIndex == selectedDefinitionIndex
            )
            {
                return;
            }

            selectedDefinitionIndex = requestedIndex;
            previewYaw = 0f;
            CreatePreview();

            SetTransientMessage(
                $"Selected {CurrentDefinition.DisplayName}.",
                2f
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

            previewYaw =
                Mathf.Repeat(
                    previewYaw +
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
                previewValid = false;
                hasSurfaceHit = false;
                validationMessage =
                    "Placement definition is incomplete.";
                ApplyPreviewColor(false);
                PreviewChanged?.Invoke();
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
                previewValid = false;
                hasSurfaceHit = false;
                validationMessage =
                    "Aim at a floor within placement range.";
                ApplyPreviewColor(false);
                PreviewChanged?.Invoke();
                return;
            }

            hasSurfaceHit = true;

            if (
                hit.collider.GetComponentInParent<PlacedObject>() != null
            )
            {
                previewValid = false;
                validationMessage =
                    "Furniture cannot be placed on other furniture.";
                ApplyPreviewTransform(
                    hit.point,
                    Quaternion.Euler(0f, previewYaw, 0f)
                );
                ApplyPreviewColor(false);
                PreviewChanged?.Invoke();
                return;
            }

            float slope =
                Vector3.Angle(
                    hit.normal,
                    Vector3.up
                );

            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    previewYaw,
                    0f
                );

            Vector3 position =
                hit.point +
                hit.normal * surfaceOffset;

            if (snapToGrid)
            {
                position = SnapPosition(
                    position,
                    definition.GridSize
                );
            }

            ApplyPreviewTransform(
                position,
                rotation
            );

            if (slope > definition.MaximumSlope)
            {
                previewValid = false;
                validationMessage =
                    $"Surface is too steep ({slope:0}°).";
                ApplyPreviewColor(false);
                PreviewChanged?.Invoke();
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
                    ? "Valid placement."
                    : "Placement is blocked.";

            ApplyPreviewColor(previewValid);
            PreviewChanged?.Invoke();
        }

        private bool IsPlacementClear(
            PlaceableDefinition definition,
            Vector3 position,
            Quaternion rotation,
            Collider supportCollider
        )
        {
            Vector3 size =
                definition.BoundsSize;

            Vector3 halfExtents =
                size * 0.5f;

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

            PlacedObject placedObject =
                PlacedObject.Spawn(
                    definition,
                    previewObject.transform.position,
                    previewObject.transform.rotation
                );

            if (placedObject == null)
            {
                SetTransientMessage(
                    "Placement failed.",
                    2.5f
                );
                return;
            }

            SetTransientMessage(
                $"Placed {definition.DisplayName}.",
                2.5f
            );

            UpdatePreview();
        }

        private void RemoveTargetedPlacedObject()
        {
            if (placementCamera == null)
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

            if (placedObject == null)
            {
                SetTransientMessage(
                    "No placed object targeted.",
                    2f
                );
                return;
            }

            string objectName =
                placedObject.Definition != null
                    ? placedObject.Definition.DisplayName
                    : placedObject.gameObject.name;

            placedObject.gameObject.SetActive(false);
            Destroy(placedObject.gameObject);

            SetTransientMessage(
                $"Removed {objectName}.",
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

        private static Vector3 SnapPosition(
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
