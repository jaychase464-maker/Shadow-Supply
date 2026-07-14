using ShadowSupply.Electrical;
using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Properties
{
    [DisallowMultipleComponent]
    public sealed class StarterGarageLightSwitch :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string switchName =
            "Garage Lights";
        [SerializeField] private Light[] controlledLights;
        [SerializeField] private bool startsOn = true;

        [Header("Optional electrical circuit")]
        [SerializeField] private ElectricalPanel powerPanel;
        [SerializeField] private string circuitId =
            "garage-lighting";

        private bool lightsOn;

        public bool IsOn => lightsOn;

        public string InteractionPrompt =>
            lightsOn
                ? $"Turn off {switchName}"
                : $"Turn on {switchName}";

        private void Awake()
        {
            lightsOn = startsOn;
            ApplyLights();
        }

        private void OnEnable()
        {
            if (powerPanel != null)
            {
                powerPanel.PowerChanged += ApplyLights;
            }

            ApplyLights();
        }

        private void OnDisable()
        {
            if (powerPanel != null)
            {
                powerPanel.PowerChanged -= ApplyLights;
            }
        }

        public void Configure(
            string displayName,
            Light[] lights,
            bool startEnabled
        )
        {
            switchName = displayName;
            controlledLights = lights;
            startsOn = startEnabled;
            lightsOn = startEnabled;
            ApplyLights();
        }

        public void ConfigurePower(
            ElectricalPanel panel,
            string stableCircuitId
        )
        {
            if (powerPanel != null)
            {
                powerPanel.PowerChanged -= ApplyLights;
            }

            powerPanel = panel;
            circuitId = stableCircuitId;

            if (
                isActiveAndEnabled &&
                powerPanel != null
            )
            {
                powerPanel.PowerChanged += ApplyLights;
            }

            ApplyLights();
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                enabled &&
                gameObject.activeInHierarchy;
        }

        public void Interact(GameObject interactor)
        {
            SetLights(!lightsOn);
        }

        public void SetLights(bool enabledState)
        {
            lightsOn = enabledState;
            ApplyLights();
        }

        private void ApplyLights()
        {
            bool circuitPowered =
                powerPanel == null ||
                powerPanel.IsCircuitPowered(circuitId);

            bool finalState =
                lightsOn &&
                circuitPowered;

            if (controlledLights == null)
            {
                return;
            }

            foreach (
                Light targetLight
                in controlledLights
            )
            {
                if (targetLight != null)
                {
                    targetLight.enabled = finalState;
                }
            }
        }
    }
}
