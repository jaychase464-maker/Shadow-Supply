using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 7.25f;
        [SerializeField, Min(0.1f)] private float crouchSpeed = 2.4f;
        [SerializeField, Min(0.1f)] private float acceleration = 18f;
        [SerializeField, Min(0.1f)] private float airControl = 5f;
        [SerializeField, Min(0.1f)] private float jumpHeight = 1.15f;
        [SerializeField] private float gravity = -24f;

        [Header("Look")]
        [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Min(1f)] private float gamepadLookSpeed = 150f;
        [SerializeField] private float minimumPitch = -85f;
        [SerializeField] private float maximumPitch = 85f;

        [Header("Crouch")]
        [SerializeField, Min(0.5f)] private float standingHeight = 1.8f;
        [SerializeField, Min(0.5f)] private float crouchingHeight = 1.15f;
        [SerializeField, Min(0.1f)] private float crouchTransitionSpeed = 10f;
        [SerializeField] private LayerMask obstructionMask = ~0;

        private const int ClearanceBufferSize = 32;

        private CharacterController characterController;

        private readonly Collider[] clearanceHits =
            new Collider[ClearanceBufferSize];

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction cursorAction;

        private Vector3 horizontalVelocity;
        private Vector2 currentMoveInput;
        private float verticalVelocity;
        private float cameraPitch;
        private float standingCameraLocalY;
        private bool wantsToCrouch;
        private bool inputInitialized;

        public bool IsCursorLocked =>
            Cursor.lockState == CursorLockMode.Locked;

        public bool IsCrouching { get; private set; }
        public float CameraPitch => cameraPitch;
        public Vector2 MoveInput => currentMoveInput;

        public float CrouchBlend =>
            characterController == null
                ? IsCrouching
                    ? 1f
                    : 0f
                : 1f - Mathf.InverseLerp(
                    crouchingHeight,
                    standingHeight,
                    characterController.height
                );
        public float VerticalVelocity => verticalVelocity;

        public float HorizontalSpeed =>
            new Vector2(
                horizontalVelocity.x,
                horizontalVelocity.z
            ).magnitude;

        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float CrouchSpeed => crouchSpeed;

        public bool IsGrounded =>
            characterController != null &&
            characterController.isGrounded;

        public bool IsSprinting =>
            !IsCrouching &&
            currentMoveInput.sqrMagnitude > 0.01f &&
            sprintAction != null &&
            sprintAction.IsPressed();

        public float NormalizedMovementSpeed =>
            Mathf.Clamp01(
                HorizontalSpeed /
                Mathf.Max(0.1f, sprintSpeed)
            );

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();

            if (cameraTransform == null)
            {
                Camera childCamera =
                    GetComponentInChildren<Camera>(true);

                if (childCamera != null)
                {
                    cameraTransform = childCamera.transform;
                }
            }

            if (cameraTransform == null)
            {
                Debug.LogError(
                    "[FirstPersonController] A child Camera is required.",
                    this
                );

                enabled = false;
                return;
            }

            standingCameraLocalY =
                cameraTransform.localPosition.y;

            characterController.height = standingHeight;
            characterController.center =
                Vector3.up * (standingHeight * 0.5f);

            BuildInputActions();
            SetCursorLocked(true);
        }

        private void OnEnable()
        {
            if (!inputInitialized)
            {
                return;
            }

            moveAction.Enable();
            lookAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            crouchAction.Enable();
            cursorAction.Enable();
        }

        private void OnDisable()
        {
            currentMoveInput = Vector2.zero;

            if (!inputInitialized)
            {
                return;
            }

            moveAction.Disable();
            lookAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();
            crouchAction.Disable();
            cursorAction.Disable();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            lookAction?.Dispose();
            jumpAction?.Dispose();
            sprintAction?.Dispose();
            crouchAction?.Dispose();
            cursorAction?.Dispose();
        }

        private void Update()
        {
            if (cursorAction.WasPressedThisFrame())
            {
                SetCursorLocked(!IsCursorLocked);
            }

            HandleCrouch();
            HandleMovement();

            if (IsCursorLocked)
            {
                HandleLook();
            }
        }

        public void SetViewRotation(float yaw, float pitch)
        {
            cameraPitch = Mathf.Clamp(
                pitch,
                minimumPitch,
                maximumPitch
            );

            transform.rotation =
                Quaternion.Euler(0f, yaw, 0f);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation =
                    Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void BuildInputActions()
        {
            moveAction =
                new InputAction("Move", InputActionType.Value);

            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            moveAction.AddBinding("<Gamepad>/leftStick");

            lookAction =
                new InputAction("Look", InputActionType.Value);

            lookAction.AddBinding("<Mouse>/delta");
            lookAction.AddBinding("<Gamepad>/rightStick");

            jumpAction = new InputAction(
                "Jump",
                InputActionType.Button,
                "<Keyboard>/space"
            );

            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            sprintAction = new InputAction(
                "Sprint",
                InputActionType.Button,
                "<Keyboard>/leftShift"
            );

            sprintAction.AddBinding("<Gamepad>/leftStickPress");

            crouchAction = new InputAction(
                "Crouch",
                InputActionType.Button,
                "<Keyboard>/leftCtrl"
            );

            crouchAction.AddBinding("<Keyboard>/c");
            crouchAction.AddBinding("<Gamepad>/buttonEast");

            cursorAction = new InputAction(
                "Toggle Cursor",
                InputActionType.Button,
                "<Keyboard>/escape"
            );

            inputInitialized = true;
        }

        private void HandleMovement()
        {
            currentMoveInput =
                moveAction.ReadValue<Vector2>();

            Vector3 desiredDirection =
                transform.right * currentMoveInput.x +
                transform.forward * currentMoveInput.y;

            desiredDirection =
                Vector3.ClampMagnitude(desiredDirection, 1f);

            float targetSpeed = IsCrouching
                ? crouchSpeed
                : sprintAction.IsPressed()
                    ? sprintSpeed
                    : walkSpeed;

            Vector3 desiredVelocity =
                desiredDirection * targetSpeed;

            float control =
                characterController.isGrounded
                    ? acceleration
                    : airControl;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                control * Time.deltaTime
            );

            if (
                characterController.isGrounded &&
                verticalVelocity < 0f
            )
            {
                verticalVelocity = -2f;
            }

            if (
                characterController.isGrounded &&
                !IsCrouching &&
                jumpAction.WasPressedThisFrame()
            )
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 finalVelocity =
                horizontalVelocity +
                Vector3.up * verticalVelocity;

            characterController.Move(
                finalVelocity * Time.deltaTime
            );
        }

        private void HandleLook()
        {
            Vector2 lookInput =
                lookAction.ReadValue<Vector2>();

            bool usingMouse =
                lookAction.activeControl?.device is Mouse;

            float yaw;
            float pitch;

            if (usingMouse)
            {
                yaw =
                    lookInput.x * mouseSensitivity;

                pitch =
                    lookInput.y * mouseSensitivity;
            }
            else
            {
                yaw =
                    lookInput.x *
                    gamepadLookSpeed *
                    Time.deltaTime;

                pitch =
                    lookInput.y *
                    gamepadLookSpeed *
                    Time.deltaTime;
            }

            cameraPitch = Mathf.Clamp(
                cameraPitch - pitch,
                minimumPitch,
                maximumPitch
            );

            cameraTransform.localRotation =
                Quaternion.Euler(cameraPitch, 0f, 0f);

            transform.Rotate(
                Vector3.up,
                yaw,
                Space.World
            );
        }

        private void HandleCrouch()
        {
            if (crouchAction.WasPressedThisFrame())
            {
                wantsToCrouch = !wantsToCrouch;
            }

            if (
                !wantsToCrouch &&
                IsCrouching &&
                !HasStandingClearance()
            )
            {
                wantsToCrouch = true;
            }

            float targetHeight =
                wantsToCrouch
                    ? crouchingHeight
                    : standingHeight;

            float nextHeight = Mathf.MoveTowards(
                characterController.height,
                targetHeight,
                crouchTransitionSpeed * Time.deltaTime
            );

            characterController.height = nextHeight;
            characterController.center =
                Vector3.up * (nextHeight * 0.5f);

            float crouchRatio = Mathf.InverseLerp(
                crouchingHeight,
                standingHeight,
                nextHeight
            );

            Vector3 cameraLocalPosition =
                cameraTransform.localPosition;

            cameraLocalPosition.y = Mathf.Lerp(
                crouchingHeight - 0.12f,
                standingCameraLocalY,
                crouchRatio
            );

            cameraTransform.localPosition =
                cameraLocalPosition;

            IsCrouching =
                nextHeight < standingHeight - 0.02f;
        }

        private bool HasStandingClearance()
        {
            if (characterController == null)
            {
                return true;
            }

            float radius = Mathf.Max(
                characterController.radius -
                characterController.skinWidth -
                0.01f,
                0.05f
            );

            float currentHeight = Mathf.Max(
                characterController.height,
                crouchingHeight
            );

            float lowerCenterHeight = Mathf.Max(
                currentHeight - radius + 0.03f,
                radius + 0.08f
            );

            float upperCenterHeight =
                standingHeight - radius - 0.02f;

            if (upperCenterHeight <= lowerCenterHeight)
            {
                return true;
            }

            Vector3 bottom =
                transform.position +
                Vector3.up * lowerCenterHeight;

            Vector3 top =
                transform.position +
                Vector3.up * upperCenterHeight;

            int hitCount =
                Physics.OverlapCapsuleNonAlloc(
                    bottom,
                    top,
                    radius,
                    clearanceHits,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore
                );

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                Collider hit =
                    clearanceHits[index];

                clearanceHits[index] = null;

                if (
                    hit == null ||
                    !hit.enabled ||
                    hit == characterController
                )
                {
                    continue;
                }

                Transform hitTransform =
                    hit.transform;

                if (
                    hitTransform == transform ||
                    hitTransform.IsChildOf(transform)
                )
                {
                    continue;
                }

                if (
                    hit.bounds.max.y <=
                    transform.position.y + 0.08f
                )
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            Cursor.visible = !locked;
        }
    }
}
