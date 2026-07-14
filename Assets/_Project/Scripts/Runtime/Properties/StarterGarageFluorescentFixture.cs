using UnityEngine;

namespace ShadowSupply.Properties
{
    [DisallowMultipleComponent]
    public sealed class StarterGarageFluorescentFixture :
        MonoBehaviour
    {
        [SerializeField] private Renderer diffuserRenderer;
        [SerializeField] private Light[] lightSources;
        [SerializeField] private Color enabledBaseColor =
            new Color(1f, 0.97f, 0.84f);
        [SerializeField] private Color disabledBaseColor =
            new Color(0.52f, 0.54f, 0.5f);
        [SerializeField] private Color enabledEmission =
            new Color(2.8f, 2.55f, 1.85f);

        private bool lastVisualState;
        private bool initialized;

        public void Configure(
            Renderer diffuser,
            Light[] lights
        )
        {
            diffuserRenderer = diffuser;
            lightSources = lights;
            initialized = false;
            RefreshVisual();
        }

        private void OnEnable()
        {
            initialized = false;
            RefreshVisual();
        }

        private void LateUpdate()
        {
            bool currentState =
                AnySourceEnabled();

            if (
                !initialized ||
                currentState != lastVisualState
            )
            {
                ApplyVisual(currentState);
            }
        }

        private void RefreshVisual()
        {
            ApplyVisual(
                AnySourceEnabled()
            );
        }

        private bool AnySourceEnabled()
        {
            if (lightSources == null)
            {
                return false;
            }

            foreach (Light source in lightSources)
            {
                if (
                    source != null &&
                    source.enabled &&
                    source.gameObject.activeInHierarchy &&
                    source.intensity > 0.001f
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyVisual(bool enabledState)
        {
            lastVisualState = enabledState;
            initialized = true;

            if (diffuserRenderer == null)
            {
                return;
            }

            MaterialPropertyBlock block =
                new MaterialPropertyBlock();

            diffuserRenderer.GetPropertyBlock(block);

            Color baseColor =
                enabledState
                    ? enabledBaseColor
                    : disabledBaseColor;

            Color emission =
                enabledState
                    ? enabledEmission
                    : Color.black;

            block.SetColor(
                "_BaseColor",
                baseColor
            );

            block.SetColor(
                "_Color",
                baseColor
            );

            block.SetColor(
                "_EmissionColor",
                emission
            );

            diffuserRenderer.SetPropertyBlock(block);
        }
    }
}
