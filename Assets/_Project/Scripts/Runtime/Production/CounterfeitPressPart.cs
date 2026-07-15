using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Production
{
    public enum CounterfeitPressPartKind
    {
        BlankNoteStock,
        PigmentCapsule,
        SecurityFilm,
        PrintedSheet,
        CutNoteStack,
        PackagingMaterial,
        BasicToolkit
    }

    [DisallowMultipleComponent]
    public sealed class CounterfeitPressPart :
        MonoBehaviour
    {
        [SerializeField]
        private CounterfeitPressPartKind partKind;
        [SerializeField]
        private ItemDefinition item;
        [SerializeField]
        private bool draggable = true;
        [SerializeField]
        private Collider interactionCollider;
        [SerializeField]
        private Renderer[] renderers;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 startScale;
        private bool completed;

        public CounterfeitPressPartKind PartKind =>
            partKind;
        public ItemDefinition Item => item;
        public bool Draggable => draggable;
        public bool Completed => completed;

        public void Configure(
            CounterfeitPressPartKind kind,
            ItemDefinition definition,
            bool canDrag,
            Collider colliderReference,
            Renderer[] visualRenderers
        )
        {
            partKind = kind;
            item = definition;
            draggable = canDrag;
            interactionCollider =
                colliderReference;
            renderers = visualRenderers;

            startPosition =
                transform.position;
            startRotation =
                transform.rotation;
            startScale =
                transform.localScale;
            completed = false;

            ClearTint();
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

            transform.localScale =
                startScale;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = true;
            }

            ClearTint();
        }

        public void SetDragging(bool dragging)
        {
            if (completed)
            {
                return;
            }

            if (dragging)
            {
                SetTint(
                    new Color(
                        1f,
                        0.5f,
                        0.04f
                    )
                );
            }
            else
            {
                ClearTint();
            }
        }

        public void SetExpected(bool expected)
        {
            if (completed)
            {
                return;
            }

            if (expected)
            {
                SetTint(
                    new Color(
                        1f,
                        0.72f,
                        0.18f
                    )
                );
            }
            else
            {
                ClearTint();
            }
        }

        public void SnapCompleted(
            Transform target,
            Vector3 localPosition,
            Quaternion localRotation,
            float scaleMultiplier = 0.9f
        )
        {
            completed = true;

            if (target != null)
            {
                transform.SetParent(
                    target,
                    false
                );

                transform.localPosition =
                    localPosition;

                transform.localRotation =
                    localRotation;
            }

            transform.localScale =
                startScale *
                Mathf.Clamp(
                    scaleMultiplier,
                    0.2f,
                    1.2f
                );

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            SetTint(
                new Color(
                    0.48f,
                    0.95f,
                    0.55f
                )
            );
        }

        public void MarkClickedComplete()
        {
            completed = true;

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            SetTint(
                new Color(
                    0.48f,
                    0.95f,
                    0.55f
                )
            );
        }

        public void SetInteractionEnabled(bool enabledState)
        {
            if (interactionCollider != null)
            {
                interactionCollider.enabled =
                    enabledState;
            }
        }

        private void ClearTint()
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer target in renderers)
            {
                if (target != null)
                {
                    target.SetPropertyBlock(null);
                }
            }
        }

        private void SetTint(Color tint)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer target in renderers)
            {
                if (target == null)
                {
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
