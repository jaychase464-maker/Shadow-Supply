using ShadowSupply.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class PlayerPlugController : MonoBehaviour
    {
        public static PlayerPlugController Local
        {
            get;
            private set;
        }

        [SerializeField] private Camera playerCamera;
        [SerializeField, Min(0.25f)] private float holdDistance =
            1.1f;
        [SerializeField] private Vector3 holdOffset =
            new Vector3(0.22f, -0.18f, 0f);

        private FirstPersonController firstPersonController;
        private InputAction releaseAction;

        public PowerPlug HeldPlug { get; private set; }

        private void Awake()
        {
            Local = this;
            firstPersonController =
                GetComponent<FirstPersonController>();

            if (playerCamera == null)
            {
                playerCamera =
                    GetComponentInChildren<Camera>(true);
            }

            releaseAction =
                new InputAction(
                    "Release Plug",
                    InputActionType.Button,
                    "<Keyboard>/r"
                );

            releaseAction.AddBinding(
                "<Gamepad>/buttonEast"
            );
        }

        private void OnEnable()
        {
            releaseAction.Enable();
        }

        private void OnDisable()
        {
            releaseAction.Disable();

            if (HeldPlug != null)
            {
                ReleaseHeldPlug();
            }
        }

        private void OnDestroy()
        {
            releaseAction?.Dispose();

            if (Local == this)
            {
                Local = null;
            }
        }

        private void Update()
        {
            if (
                HeldPlug != null &&
                releaseAction.WasPressedThisFrame()
            )
            {
                ReleaseHeldPlug();
            }
        }

        private void LateUpdate()
        {
            if (
                HeldPlug == null ||
                playerCamera == null
            )
            {
                return;
            }

            if (
                firstPersonController != null &&
                !firstPersonController.IsCursorLocked
            )
            {
                return;
            }

            Transform cameraTransform =
                playerCamera.transform;

            Vector3 targetPosition =
                cameraTransform.position +
                cameraTransform.forward * holdDistance +
                cameraTransform.right * holdOffset.x +
                cameraTransform.up * holdOffset.y;

            HeldPlug.SetHeldPose(
                targetPosition,
                cameraTransform.rotation
            );
        }

        public void Configure(Camera cameraReference)
        {
            playerCamera = cameraReference;
        }

        public bool BeginHolding(PowerPlug plug)
        {
            if (plug == null)
            {
                return false;
            }

            if (
                HeldPlug != null &&
                HeldPlug != plug
            )
            {
                ReleaseHeldPlug();
            }

            HeldPlug = plug;
            HeldPlug.EnterHeld(this);
            return true;
        }

        public bool TryConnectHeldPlug(
            OutletSocket socket
        )
        {
            if (
                HeldPlug == null ||
                socket == null
            )
            {
                return false;
            }

            PowerPlug plug = HeldPlug;

            if (!plug.ConnectTo(socket))
            {
                return false;
            }

            HeldPlug = null;
            return true;
        }

        public void ReleaseHeldPlug()
        {
            if (HeldPlug == null)
            {
                return;
            }

            PowerPlug plug = HeldPlug;
            HeldPlug = null;
            plug.ReleaseFromHold();
        }

        internal void ClearHeldReference(
            PowerPlug plug
        )
        {
            if (HeldPlug == plug)
            {
                HeldPlug = null;
            }
        }
    }
}
