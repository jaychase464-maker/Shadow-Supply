using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Properties
{
    [DisallowMultipleComponent]
    public sealed class StarterGarageLightSwitch :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string switchName = "Garage Lights";
        [SerializeField] private Light[] controlledLights;
        [SerializeField] private bool startsOn = true;

        private bool lightsOn;

        public string InteractionPrompt =>
            lightsOn
                ? $"Turn off {switchName}"
                : $"Turn on {switchName}";

        private void Awake()
        {
            SetLights(startsOn);
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
            SetLights(startsOn);
        }

        public bool CanInteract(GameObject interactor)
        {
            return enabled && gameObject.activeInHierarchy;
        }

        public void Interact(GameObject interactor)
        {
            SetLights(!lightsOn);
        }

        public void SetLights(bool enabledState)
        {
            lightsOn = enabledState;

            if (controlledLights == null)
            {
                return;
            }

            foreach (Light targetLight in controlledLights)
            {
                if (targetLight != null)
                {
                    targetLight.enabled = enabledState;
                }
            }
        }
    }
}
