using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowSupply.Character
{
    public enum FirstPersonBodyRenderMode
    {
        LocalVisible,
        ShadowOnly
    }

    [DisallowMultipleComponent]
    public sealed class FirstPersonBodyVisibilityController :
        MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private FirstPersonBodyRenderMode renderMode;
        [SerializeField, Range(0.0001f, 0.05f)]
        private float localHeadScale = 0.001f;

        private Transform headBone;
        private Vector3 originalHeadScale = Vector3.one;
        private bool headScaleCached;

        private static readonly string[] LocalHiddenRendererNames =
        {
            "Eye",
            "Teeth",
            "Tongue",
            "eye_laach",
            "Buzz_Cut",
            "Chin_Curtain_Sparse"
        };

        public FirstPersonBodyRenderMode RenderMode =>
            renderMode;

        public void Configure(
            Animator animatorReference,
            FirstPersonBodyRenderMode mode
        )
        {
            animator = animatorReference;
            renderMode = mode;
            Apply();
        }

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            if (animator == null)
            {
                animator =
                    GetComponentInChildren<Animator>(true);
            }

            CacheHeadScale();

            Renderer[] renderers =
                GetComponentsInChildren<Renderer>(true);

            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                if (
                    renderMode ==
                    FirstPersonBodyRenderMode.ShadowOnly
                )
                {
                    targetRenderer.enabled = true;
                    targetRenderer.shadowCastingMode =
                        ShadowCastingMode.ShadowsOnly;

                    targetRenderer.receiveShadows = false;
                    continue;
                }

                targetRenderer.shadowCastingMode =
                    ShadowCastingMode.Off;

                targetRenderer.receiveShadows = true;

                targetRenderer.enabled =
                    !ShouldHideRendererLocally(
                        targetRenderer.name
                    );
            }

            Collider[] colliders =
                GetComponentsInChildren<Collider>(true);

            foreach (Collider targetCollider in colliders)
            {
                if (targetCollider != null)
                {
                    targetCollider.enabled = false;
                }
            }

            if (
                headBone != null &&
                headScaleCached
            )
            {
                headBone.localScale =
                    renderMode ==
                    FirstPersonBodyRenderMode.LocalVisible
                        ? originalHeadScale * localHeadScale
                        : originalHeadScale;
            }
        }

        private void CacheHeadScale()
        {
            if (
                animator == null ||
                animator.avatar == null ||
                !animator.avatar.isHuman
            )
            {
                return;
            }

            if (headBone == null)
            {
                headBone =
                    animator.GetBoneTransform(
                        HumanBodyBones.Head
                    );
            }

            if (
                headBone != null &&
                !headScaleCached
            )
            {
                originalHeadScale =
                    headBone.localScale;

                headScaleCached = true;
            }
        }

        private static bool ShouldHideRendererLocally(
            string rendererName
        )
        {
            foreach (
                string hiddenName
                in LocalHiddenRendererNames
            )
            {
                if (
                    string.Equals(
                        rendererName,
                        hiddenName,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    rendererName.IndexOf(
                        hiddenName,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
