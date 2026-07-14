using UnityEngine;

namespace ShadowSupply.Character
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class HumanoidFootAlignment : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Right ankle correction")]
        [SerializeField] private bool correctRightFoot = true;

        [SerializeField, Range(0f, 45f)]
        private float minimumCorrectionAngle = 3f;

        [SerializeField, Range(10f, 120f)]
        private float maximumCorrectionAngle = 80f;

        [SerializeField, Range(-45f, 45f)]
        private float manualYawOffset;

        private Transform leftFoot;
        private Transform leftToes;
        private Transform rightFoot;
        private Transform rightToes;

        private bool referencesResolved;

        public void Configure(
            Animator animatorReference,
            ShadowSupply.Player.FirstPersonController controllerReference
        )
        {
            animator = animatorReference;
            referencesResolved = false;
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            if (!correctRightFoot)
            {
                return;
            }

            if (!referencesResolved)
            {
                ResolveReferences();

                if (!referencesResolved)
                {
                    return;
                }
            }

            Vector3 upAxis =
                animator.transform.up;

            Vector3 leftDirection =
                GetProjectedFootDirection(
                    leftFoot,
                    leftToes,
                    upAxis
                );

            Vector3 rightDirection =
                GetProjectedFootDirection(
                    rightFoot,
                    rightToes,
                    upAxis
                );

            if (
                leftDirection.sqrMagnitude < 0.001f ||
                rightDirection.sqrMagnitude < 0.001f
            )
            {
                return;
            }

            float requiredYaw =
                Vector3.SignedAngle(
                    rightDirection,
                    leftDirection,
                    upAxis
                ) +
                manualYawOffset;

            if (
                Mathf.Abs(requiredYaw) <
                minimumCorrectionAngle
            )
            {
                return;
            }

            requiredYaw =
                Mathf.Clamp(
                    requiredYaw,
                    -maximumCorrectionAngle,
                    maximumCorrectionAngle
                );

            Quaternion correction =
                Quaternion.AngleAxis(
                    requiredYaw,
                    upAxis
                );

            rightFoot.rotation =
                correction *
                rightFoot.rotation;
        }

        private static Vector3 GetProjectedFootDirection(
            Transform foot,
            Transform toes,
            Vector3 upAxis
        )
        {
            if (
                foot == null ||
                toes == null
            )
            {
                return Vector3.zero;
            }

            Vector3 direction =
                toes.position -
                foot.position;

            direction =
                Vector3.ProjectOnPlane(
                    direction,
                    upAxis
                );

            return direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.zero;
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator =
                    GetComponentInChildren<Animator>(true);
            }

            if (
                animator == null ||
                animator.avatar == null ||
                !animator.avatar.isValid ||
                !animator.avatar.isHuman
            )
            {
                referencesResolved = false;
                return;
            }

            leftFoot =
                animator.GetBoneTransform(
                    HumanBodyBones.LeftFoot
                );

            leftToes =
                animator.GetBoneTransform(
                    HumanBodyBones.LeftToes
                );

            rightFoot =
                animator.GetBoneTransform(
                    HumanBodyBones.RightFoot
                );

            rightToes =
                animator.GetBoneTransform(
                    HumanBodyBones.RightToes
                );

            referencesResolved =
                leftFoot != null &&
                leftToes != null &&
                rightFoot != null &&
                rightToes != null;
        }
    }
}
