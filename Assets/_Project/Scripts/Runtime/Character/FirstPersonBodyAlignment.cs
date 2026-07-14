using ShadowSupply.Player;
using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class FirstPersonBodyAlignment : MonoBehaviour
    {
        [SerializeField] private FirstPersonController controller;

        [Header("Local body positioning")]
        [SerializeField] private Vector3 standingOffset =
            new Vector3(0f, -0.03f, -0.14f);

        [SerializeField] private Vector3 crouchingOffset =
            new Vector3(0f, -0.10f, -0.38f);

        [SerializeField, Min(0f)]
        private float maximumLookDownRetreat = 0.08f;

        [SerializeField, Min(0.1f)]
        private float transitionSpeed = 14f;

        private bool initialized;

        public void Configure(
            FirstPersonController controllerReference
        )
        {
            controller = controllerReference;
            ApplyImmediately();
        }

        private void Awake()
        {
            ResolveController();
            ApplyImmediately();
        }

        private void OnEnable()
        {
            ResolveController();
            ApplyImmediately();
        }

        private void LateUpdate()
        {
            if (controller == null)
            {
                ResolveController();

                if (controller == null)
                {
                    return;
                }
            }

            Vector3 target =
                CalculateTargetOffset();

            float interpolation =
                1f - Mathf.Exp(
                    -transitionSpeed * Time.deltaTime
                );

            transform.localPosition =
                Vector3.Lerp(
                    transform.localPosition,
                    target,
                    interpolation
                );

            transform.localRotation =
                Quaternion.identity;
        }

        private void ApplyImmediately()
        {
            if (!initialized)
            {
                initialized = true;
            }

            if (controller == null)
            {
                ResolveController();
            }

            transform.localPosition =
                CalculateTargetOffset();

            transform.localRotation =
                Quaternion.identity;
        }

        private Vector3 CalculateTargetOffset()
        {
            float crouchBlend =
                controller != null
                    ? controller.CrouchBlend
                    : 0f;

            Vector3 target =
                Vector3.Lerp(
                    standingOffset,
                    crouchingOffset,
                    crouchBlend
                );

            if (controller != null)
            {
                float lookDownAmount =
                    Mathf.InverseLerp(
                        20f,
                        85f,
                        -controller.CameraPitch
                    );

                target.z -=
                    lookDownAmount *
                    maximumLookDownRetreat;
            }

            return target;
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller =
                    GetComponentInParent<
                        FirstPersonController
                    >();
            }
        }
    }
}
