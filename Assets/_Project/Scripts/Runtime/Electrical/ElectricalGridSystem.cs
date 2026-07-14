using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DefaultExecutionOrder(-300)]
    [DisallowMultipleComponent]
    public sealed class ElectricalGridSystem : MonoBehaviour
    {
        public static ElectricalGridSystem Instance
        {
            get;
            private set;
        }

        [SerializeField, Min(0.02f)]
        private float refreshInterval = 0.1f;

        private bool dirty = true;
        private float nextRefreshTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (
                !dirty &&
                Time.unscaledTime < nextRefreshTime
            )
            {
                return;
            }

            Recalculate();
        }

        public static void NotifyChanged()
        {
            if (Instance != null)
            {
                Instance.dirty = true;
            }
        }

        public void ForceRecalculate()
        {
            dirty = true;
            Recalculate();
        }

        private void Recalculate()
        {
            dirty = false;
            nextRefreshTime =
                Time.unscaledTime + refreshInterval;

            ElectricalPanel[] panels =
                FindObjectsByType<ElectricalPanel>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            MachinePowerConnection[] machines =
                FindObjectsByType<MachinePowerConnection>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            ElectricalLightLoad[] lightLoads =
                FindObjectsByType<ElectricalLightLoad>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            foreach (ElectricalPanel panel in panels)
            {
                panel?.ResetCalculatedLoads();
            }

            Dictionary<ElectricalPanel, Dictionary<string, int>>
                loadsByPanel =
                    new Dictionary<
                        ElectricalPanel,
                        Dictionary<string, int>
                    >();

            foreach (
                MachinePowerConnection machine
                in machines
            )
            {
                if (
                    machine == null ||
                    !machine.TryGetConnectedCircuit(
                        out ElectricalPanel panel,
                        out string circuitId
                    )
                )
                {
                    continue;
                }

                if (!loadsByPanel.TryGetValue(
                    panel,
                    out Dictionary<string, int> circuitLoads
                ))
                {
                    circuitLoads =
                        new Dictionary<string, int>();

                    loadsByPanel.Add(
                        panel,
                        circuitLoads
                    );
                }

                circuitLoads.TryGetValue(
                    circuitId,
                    out int current
                );

                circuitLoads[circuitId] =
                    current +
                    machine.PowerDemandWatts;
            }

            foreach (
                ElectricalLightLoad lightLoad
                in lightLoads
            )
            {
                if (
                    lightLoad == null ||
                    !lightLoad.TryGetActiveCircuit(
                        out ElectricalPanel panel,
                        out string circuitId
                    )
                )
                {
                    continue;
                }

                if (!loadsByPanel.TryGetValue(
                    panel,
                    out Dictionary<string, int> circuitLoads
                ))
                {
                    circuitLoads =
                        new Dictionary<string, int>();

                    loadsByPanel.Add(
                        panel,
                        circuitLoads
                    );
                }

                circuitLoads.TryGetValue(
                    circuitId,
                    out int current
                );

                circuitLoads[circuitId] =
                    current +
                    lightLoad.DemandWatts;
            }

            foreach (
                KeyValuePair<
                    ElectricalPanel,
                    Dictionary<string, int>
                > panelEntry
                in loadsByPanel
            )
            {
                foreach (
                    KeyValuePair<string, int> circuitEntry
                    in panelEntry.Value
                )
                {
                    panelEntry.Key.SetCalculatedLoad(
                        circuitEntry.Key,
                        circuitEntry.Value
                    );
                }
            }

            bool tripped = false;

            foreach (ElectricalPanel panel in panels)
            {
                if (
                    panel != null &&
                    panel.EvaluateTrips()
                )
                {
                    tripped = true;
                }
            }

            foreach (
                MachinePowerConnection machine
                in machines
            )
            {
                machine?.RefreshPoweredState();
            }

            if (tripped)
            {
                foreach (
                    MachinePowerConnection machine
                    in machines
                )
                {
                    machine?.RefreshPoweredState();
                }
            }
        }
    }
}
