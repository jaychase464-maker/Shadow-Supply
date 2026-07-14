using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class ElectricalLightLoad : MonoBehaviour
    {
        [SerializeField] private ElectricalPanel panel;
        [SerializeField] private string circuitId;
        [SerializeField, Min(1)] private int demandWatts = 240;
        [SerializeField] private Light[] controlledLights;

        public int DemandWatts => demandWatts;

        public void Configure(
            ElectricalPanel panelReference,
            string stableCircuitId,
            int watts,
            Light[] lights
        )
        {
            panel = panelReference;
            circuitId = stableCircuitId;
            demandWatts = Mathf.Max(1, watts);
            controlledLights = lights;
            ElectricalGridSystem.NotifyChanged();
        }

        public bool TryGetActiveCircuit(
            out ElectricalPanel activePanel,
            out string activeCircuitId
        )
        {
            activePanel = null;
            activeCircuitId = null;

            if (
                panel == null ||
                string.IsNullOrWhiteSpace(circuitId) ||
                !AnyLightEnabled()
            )
            {
                return false;
            }

            activePanel = panel;
            activeCircuitId = circuitId;
            return true;
        }

        private bool AnyLightEnabled()
        {
            if (controlledLights == null)
            {
                return false;
            }

            foreach (Light targetLight in controlledLights)
            {
                if (
                    targetLight != null &&
                    targetLight.enabled
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
