using System;
using ShadowSupply.Player;
using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class ProceduralHumanoidLocomotion : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private FirstPersonController controller;

        [Header("Motion")]
        [SerializeField, Min(0.1f)] private float walkCadence = 7f;
        [SerializeField, Min(0.1f)] private float sprintCadence = 10.5f;
        [SerializeField, Range(0f, 1f)] private float legSwing = 0.45f;
        [SerializeField, Range(0f, 1f)] private float armSwing = 0.38f;
        [SerializeField, Range(0f, 1f)] private float crouchAmount = 0.75f;
        [SerializeField, Range(0f, 0.3f)] private float crouchBodyDrop = 0.13f;
        [SerializeField, Range(0f, 1f)] private float lookFollow = 0.18f;
        [SerializeField, Min(0.1f)] private float smoothing = 10f;

        private HumanPoseHandler poseHandler;
        private HumanPose pose;
        private Vector3 baseBodyPosition;
        private Quaternion baseBodyRotation;
        private float smoothedSpeed;
        private float smoothedCrouch;
        private float smoothedAir;
        private bool initialized;

        private int leftArmDownUp = -1;
        private int rightArmDownUp = -1;
        private int leftArmFrontBack = -1;
        private int rightArmFrontBack = -1;
        private int leftUpperLegFrontBack = -1;
        private int rightUpperLegFrontBack = -1;
        private int leftLowerLegStretch = -1;
        private int rightLowerLegStretch = -1;
        private int spineFrontBack = -1;
        private int chestFrontBack = -1;
        private int upperChestFrontBack = -1;

        public void Configure(
            Animator animatorReference,
            FirstPersonController controllerReference
        )
        {
            animator = animatorReference;
            controller = controllerReference;

            if (Application.isPlaying)
            {
                Initialize();
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            poseHandler?.Dispose();
            poseHandler = null;
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                Initialize();

                if (!initialized)
                {
                    return;
                }
            }

            float delta =
                1f - Mathf.Exp(
                    -smoothing * Time.deltaTime
                );

            float targetSpeed =
                controller != null
                    ? controller.NormalizedMovementSpeed
                    : 0f;

            float targetCrouch =
                controller != null &&
                controller.IsCrouching
                    ? 1f
                    : 0f;

            float targetAir =
                controller != null &&
                !controller.IsGrounded
                    ? 1f
                    : 0f;

            smoothedSpeed =
                Mathf.Lerp(
                    smoothedSpeed,
                    targetSpeed,
                    delta
                );

            smoothedCrouch =
                Mathf.Lerp(
                    smoothedCrouch,
                    targetCrouch,
                    delta
                );

            smoothedAir =
                Mathf.Lerp(
                    smoothedAir,
                    targetAir,
                    delta
                );

            poseHandler.GetHumanPose(ref pose);

            float cadence =
                Mathf.Lerp(
                    walkCadence,
                    sprintCadence,
                    controller != null &&
                    controller.IsSprinting
                        ? 1f
                        : 0f
                );

            float phase =
                Time.time * cadence;

            float swing =
                Mathf.Sin(phase) *
                smoothedSpeed *
                (1f - smoothedAir * 0.65f);

            float oppositeSwing = -swing;

            SetMuscle(
                leftArmDownUp,
                -0.72f
            );

            SetMuscle(
                rightArmDownUp,
                -0.72f
            );

            SetMuscle(
                leftArmFrontBack,
                oppositeSwing * armSwing
            );

            SetMuscle(
                rightArmFrontBack,
                swing * armSwing
            );

            float crouchedLegBase =
                smoothedCrouch *
                crouchAmount;

            SetMuscle(
                leftUpperLegFrontBack,
                swing * legSwing +
                crouchedLegBase * 0.35f
            );

            SetMuscle(
                rightUpperLegFrontBack,
                oppositeSwing * legSwing +
                crouchedLegBase * 0.35f
            );

            SetMuscle(
                leftLowerLegStretch,
                crouchedLegBase * 0.72f +
                Mathf.Max(0f, -swing) * 0.18f
            );

            SetMuscle(
                rightLowerLegStretch,
                crouchedLegBase * 0.72f +
                Mathf.Max(0f, swing) * 0.18f
            );

            float pitch =
                controller != null
                    ? controller.CameraPitch / 85f
                    : 0f;

            float spinePitch =
                Mathf.Clamp(
                    -pitch * lookFollow +
                    smoothedCrouch * 0.2f,
                    -0.35f,
                    0.45f
                );

            SetMuscle(
                spineFrontBack,
                spinePitch * 0.35f
            );

            SetMuscle(
                chestFrontBack,
                spinePitch * 0.4f
            );

            SetMuscle(
                upperChestFrontBack,
                spinePitch * 0.25f
            );

            pose.bodyPosition =
                baseBodyPosition +
                Vector3.down *
                (
                    smoothedCrouch *
                    crouchBodyDrop
                );

            pose.bodyRotation =
                baseBodyRotation;

            poseHandler.SetHumanPose(ref pose);
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

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
                return;
            }

            if (controller == null)
            {
                controller =
                    GetComponentInParent<FirstPersonController>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;

            poseHandler =
                new HumanPoseHandler(
                    animator.avatar,
                    animator.transform
                );

            pose =
                new HumanPose
                {
                    muscles =
                        new float[HumanTrait.MuscleCount]
                };

            poseHandler.GetHumanPose(ref pose);
            baseBodyPosition = pose.bodyPosition;
            baseBodyRotation = pose.bodyRotation;

            leftArmDownUp =
                FindMuscle("Left Arm Down-Up");

            rightArmDownUp =
                FindMuscle("Right Arm Down-Up");

            leftArmFrontBack =
                FindMuscle("Left Arm Front-Back");

            rightArmFrontBack =
                FindMuscle("Right Arm Front-Back");

            leftUpperLegFrontBack =
                FindMuscle("Left Upper Leg Front-Back");

            rightUpperLegFrontBack =
                FindMuscle("Right Upper Leg Front-Back");

            leftLowerLegStretch =
                FindMuscle("Left Lower Leg Stretch");

            rightLowerLegStretch =
                FindMuscle("Right Lower Leg Stretch");

            spineFrontBack =
                FindMuscle("Spine Front-Back");

            chestFrontBack =
                FindMuscle("Chest Front-Back");

            upperChestFrontBack =
                FindMuscle("UpperChest Front-Back");

            initialized = true;
        }

        private void SetMuscle(
            int index,
            float value
        )
        {
            if (
                index < 0 ||
                pose.muscles == null ||
                index >= pose.muscles.Length
            )
            {
                return;
            }

            pose.muscles[index] =
                Mathf.Clamp(value, -1f, 1f);
        }

        private static int FindMuscle(
            string muscleName
        )
        {
            return Array.IndexOf(
                HumanTrait.MuscleName,
                muscleName
            );
        }
    }
}
