using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class CircuitBreakerSwitch :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private ElectricalPanel panel;
        [SerializeField] private bool controlsMain;
        [SerializeField] private string circuitId;
        [SerializeField] private string displayName = "Breaker";
        [SerializeField] private Transform leverVisual;
        [SerializeField] private Vector3 onLocalEuler =
            new Vector3(-18f, 0f, 0f);
        [SerializeField] private Vector3 offLocalEuler =
            new Vector3(18f, 0f, 0f);
        [SerializeField] private Vector3 trippedLocalEuler =
            new Vector3(0f, 0f, 0f);

        public string InteractionPrompt
        {
            get
            {
                if (panel == null)
                {
                    return "Breaker unavailable";
                }

                bool tripped;
                bool on;

                if (controlsMain)
                {
                    tripped = panel.MainTripped;
                    on = panel.MainOn;
                }
                else if (
                    panel.TryGetCircuit(
                        circuitId,
                        out ElectricalCircuit circuit
                    )
                )
                {
                    tripped = circuit.IsTripped;
                    on = circuit.IsOn;
                }
                else
                {
                    tripped = false;
                    on = false;
                }

                if (tripped)
                {
                    return $"Reset {displayName}";
                }

                return on
                    ? $"Turn off {displayName}"
                    : $"Turn on {displayName}";
            }
        }

        private void OnEnable()
        {
            if (panel != null)
            {
                panel.PowerChanged += RefreshVisual;
            }

            RefreshVisual();
        }

        private void OnDisable()
        {
            if (panel != null)
            {
                panel.PowerChanged -= RefreshVisual;
            }
        }

        public void Configure(
            ElectricalPanel panelReference,
            bool isMain,
            string stableCircuitId,
            string breakerName,
            Transform lever
        )
        {
            if (panel != null)
            {
                panel.PowerChanged -= RefreshVisual;
            }

            panel = panelReference;
            controlsMain = isMain;
            circuitId = stableCircuitId;
            displayName = breakerName;
            leverVisual = lever;

            if (isActiveAndEnabled && panel != null)
            {
                panel.PowerChanged += RefreshVisual;
            }

            RefreshVisual();
        }

        public bool CanInteract(GameObject interactor)
        {
            return panel != null;
        }

        public void Interact(GameObject interactor)
        {
            if (panel == null)
            {
                return;
            }

            if (controlsMain)
            {
                panel.ToggleMain();
            }
            else
            {
                panel.ToggleCircuit(circuitId);
            }

            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (
                panel == null ||
                leverVisual == null
            )
            {
                return;
            }

            bool tripped;
            bool on;

            if (controlsMain)
            {
                tripped = panel.MainTripped;
                on = panel.MainOn;
            }
            else if (
                panel.TryGetCircuit(
                    circuitId,
                    out ElectricalCircuit circuit
                )
            )
            {
                tripped = circuit.IsTripped;
                on = circuit.IsOn;
            }
            else
            {
                tripped = false;
                on = false;
            }

            leverVisual.localRotation =
                Quaternion.Euler(
                    tripped
                        ? trippedLocalEuler
                        : on
                            ? onLocalEuler
                            : offLocalEuler
                );
        }
    }
}
