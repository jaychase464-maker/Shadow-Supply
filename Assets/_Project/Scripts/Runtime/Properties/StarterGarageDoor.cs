using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Properties
{
    public enum StarterGarageDoorMotion
    {
        Slide,
        Rotate
    }

    [DisallowMultipleComponent]
    public sealed class StarterGarageDoor :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string displayName = "Door";
        [SerializeField] private StarterGarageDoorMotion motion =
            StarterGarageDoorMotion.Slide;
        [SerializeField] private Vector3 openLocalPosition;
        [SerializeField] private Vector3 openLocalEulerAngles;
        [SerializeField, Min(0.1f)] private float movementSpeed = 3f;
        [SerializeField] private bool startsOpen;

        [SerializeField, HideInInspector]
        private Vector3 closedLocalPosition;

        [SerializeField, HideInInspector]
        private Vector3 closedLocalEulerAngles;

        [SerializeField, HideInInspector]
        private bool poseConfigured;

        private Quaternion closedLocalRotation;
        private Quaternion openLocalRotation;
        private bool targetOpen;
        private bool initialized;

        public bool IsOpen => targetOpen;

        public string InteractionPrompt =>
            targetOpen
                ? $"Close {displayName}"
                : $"Open {displayName}";

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void Update()
        {
            if (!initialized)
            {
                Initialize();
            }

            float interpolation =
                1f - Mathf.Exp(
                    -movementSpeed * Time.deltaTime
                );

            if (motion == StarterGarageDoorMotion.Slide)
            {
                Vector3 targetPosition =
                    targetOpen
                        ? closedLocalPosition +
                          openLocalPosition
                        : closedLocalPosition;

                transform.localPosition =
                    Vector3.Lerp(
                        transform.localPosition,
                        targetPosition,
                        interpolation
                    );
            }
            else
            {
                Quaternion targetRotation =
                    targetOpen
                        ? openLocalRotation
                        : closedLocalRotation;

                transform.localRotation =
                    Quaternion.Slerp(
                        transform.localRotation,
                        targetRotation,
                        interpolation
                    );
            }
        }

        public void Configure(
            string doorDisplayName,
            StarterGarageDoorMotion doorMotion,
            Vector3 openPositionOffset,
            Vector3 openEulerOffset,
            float speed,
            bool startOpen
        )
        {
            displayName = doorDisplayName;
            motion = doorMotion;
            openLocalPosition = openPositionOffset;
            openLocalEulerAngles = openEulerOffset;
            movementSpeed = Mathf.Max(0.1f, speed);
            startsOpen = startOpen;

            closedLocalPosition =
                transform.localPosition;

            closedLocalEulerAngles =
                transform.localEulerAngles;

            poseConfigured = true;
            initialized = false;

            Initialize();
            SnapToState();
        }

        public bool CanInteract(GameObject interactor)
        {
            return enabled && gameObject.activeInHierarchy;
        }

        public void Interact(GameObject interactor)
        {
            targetOpen = !targetOpen;
        }

        public void SetOpen(bool open, bool immediate)
        {
            targetOpen = open;

            if (immediate)
            {
                SnapToState();
            }
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            if (!poseConfigured)
            {
                closedLocalPosition =
                    transform.localPosition;

                closedLocalEulerAngles =
                    transform.localEulerAngles;

                poseConfigured = true;
            }

            closedLocalRotation =
                Quaternion.Euler(
                    closedLocalEulerAngles
                );

            openLocalRotation =
                closedLocalRotation *
                Quaternion.Euler(openLocalEulerAngles);

            targetOpen = startsOpen;
            initialized = true;
        }

        private void SnapToState()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (motion == StarterGarageDoorMotion.Slide)
            {
                transform.localPosition =
                    targetOpen
                        ? closedLocalPosition +
                          openLocalPosition
                        : closedLocalPosition;
            }
            else
            {
                transform.localRotation =
                    targetOpen
                        ? openLocalRotation
                        : closedLocalRotation;
            }
        }
    }
}
