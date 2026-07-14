using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Production
{
    public enum AssemblyPartRole
    {
        Ingredient,
        ReusableTool
    }

    [DisallowMultipleComponent]
    public sealed class InteractiveAssemblyPart :
        MonoBehaviour
    {
        [SerializeField] private int stepIndex;
        [SerializeField] private ItemDefinition item;
        [SerializeField] private AssemblyPartRole role;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private Renderer[] renderers;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 startScale;
        private bool completed;

        public int StepIndex => stepIndex;
        public ItemDefinition Item => item;
        public AssemblyPartRole Role => role;
        public bool IsCompleted => completed;
        public Collider InteractionCollider =>
            interactionCollider;

        public void Configure(
            int index,
            ItemDefinition definition,
            AssemblyPartRole partRole,
            Collider colliderReference,
            Renderer[] partRenderers
        )
        {
            stepIndex = index;
            item = definition;
            role = partRole;
            interactionCollider = colliderReference;
            renderers = partRenderers;

            startPosition = transform.position;
            startRotation = transform.rotation;
            startScale = transform.localScale;
            completed = false;
            SetTint(Color.white);
        }

        public void SetStartPose(
            Vector3 position,
            Quaternion rotation
        )
        {
            startPosition = position;
            startRotation = rotation;
            transform.SetPositionAndRotation(
                position,
                rotation
            );
        }

        public void ResetToStart()
        {
            completed = false;
            transform.SetPositionAndRotation(
                startPosition,
                startRotation
            );
            transform.localScale = startScale;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = true;
            }

            SetTint(Color.white);
        }

        public void SetDragging(bool dragging)
        {
            if (completed)
            {
                return;
            }

            SetTint(
                dragging
                    ? new Color(1f, 0.52f, 0.08f)
                    : Color.white
            );
        }

        public void CompleteAsTool()
        {
            completed = true;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            transform.localScale =
                startScale * 0.96f;

            SetTint(
                new Color(
                    0.32f,
                    1f,
                    0.46f
                )
            );
        }

        public void CompleteAsIngredient(
            Transform packageInterior,
            Vector3 localPosition,
            Quaternion localRotation,
            float packedScale
        )
        {
            completed = true;

            transform.SetParent(
                packageInterior,
                false
            );

            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale =
                startScale *
                Mathf.Clamp(
                    packedScale,
                    0.2f,
                    1f
                );

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            SetTint(
                new Color(
                    0.82f,
                    0.88f,
                    0.92f
                )
            );
        }

        private void SetTint(Color tint)
        {
            if (renderers == null)
            {
                return;
            }

            bool clearTint =
                Mathf.Approximately(tint.r, 1f) &&
                Mathf.Approximately(tint.g, 1f) &&
                Mathf.Approximately(tint.b, 1f) &&
                Mathf.Approximately(tint.a, 1f);

            foreach (Renderer target in renderers)
            {
                if (target == null)
                {
                    continue;
                }

                if (clearTint)
                {
                    target.SetPropertyBlock(null);
                    continue;
                }

                MaterialPropertyBlock block =
                    new MaterialPropertyBlock();

                target.GetPropertyBlock(block);

                if (
                    target.sharedMaterial != null &&
                    target.sharedMaterial.HasProperty(
                        "_BaseColor"
                    )
                )
                {
                    block.SetColor(
                        "_BaseColor",
                        tint
                    );
                }

                if (
                    target.sharedMaterial != null &&
                    target.sharedMaterial.HasProperty(
                        "_Color"
                    )
                )
                {
                    block.SetColor(
                        "_Color",
                        tint
                    );
                }

                target.SetPropertyBlock(block);
            }
        }
    }
}
