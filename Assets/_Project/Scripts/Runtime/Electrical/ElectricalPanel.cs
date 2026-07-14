using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [Serializable]
    public sealed class ElectricalCircuit
    {
        [SerializeField] private string circuitId;
        [SerializeField] private string displayName;
        [SerializeField, Min(1)] private int capacityWatts = 1200;
        [SerializeField] private bool startsOn = true;

        [NonSerialized] private bool initialized;
        [NonSerialized] private bool isOn;
        [NonSerialized] private bool isTripped;
        [NonSerialized] private int currentLoadWatts;

        public string CircuitId => circuitId;
        public string DisplayName => displayName;
        public int CapacityWatts => capacityWatts;
        public bool IsOn => isOn;
        public bool IsTripped => isTripped;
        public int CurrentLoadWatts => currentLoadWatts;

        public ElectricalCircuit()
        {
        }

        public ElectricalCircuit(
            string stableId,
            string name,
            int capacity,
            bool startEnabled
        )
        {
            circuitId = stableId;
            displayName = name;
            capacityWatts = Mathf.Max(1, capacity);
            startsOn = startEnabled;
            Initialize();
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            isOn = startsOn;
            isTripped = false;
            currentLoadWatts = 0;
            initialized = true;
        }

        public void SetLoad(int watts)
        {
            Initialize();
            currentLoadWatts = Mathf.Max(0, watts);
        }

        public void SetState(
            bool enabledState,
            bool trippedState
        )
        {
            initialized = true;
            isOn = enabledState;
            isTripped = trippedState;
        }

        public void Toggle()
        {
            Initialize();

            if (isTripped)
            {
                isTripped = false;
                isOn = true;
                return;
            }

            isOn = !isOn;
        }

        public void Trip()
        {
            Initialize();
            isTripped = true;
            isOn = false;
        }

        public void ResetBreaker()
        {
            initialized = true;
            isTripped = false;
            isOn = true;
        }
    }

    [DisallowMultipleComponent]
    public sealed class ElectricalPanel : MonoBehaviour
    {
        [SerializeField] private string panelId =
            "starter-garage-panel";
        [SerializeField, Min(1)] private int mainCapacityWatts =
            3600;
        [SerializeField] private bool mainStartsOn = true;
        [SerializeField] private List<ElectricalCircuit> circuits =
            new List<ElectricalCircuit>();

        private bool initialized;
        private bool mainOn;
        private bool mainTripped;
        private int currentMainLoadWatts;

        public event Action PowerChanged;

        public string PanelId => panelId;
        public int MainCapacityWatts => mainCapacityWatts;
        public bool MainOn => mainOn;
        public bool MainTripped => mainTripped;
        public int CurrentMainLoadWatts => currentMainLoadWatts;
        public IReadOnlyList<ElectricalCircuit> Circuits => circuits;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            ElectricalGridSystem.NotifyChanged();
        }

        public void Configure(
            string stablePanelId,
            int mainCapacity,
            IEnumerable<ElectricalCircuit> configuredCircuits
        )
        {
            panelId = stablePanelId;
            mainCapacityWatts = Mathf.Max(1, mainCapacity);
            circuits =
                configuredCircuits != null
                    ? new List<ElectricalCircuit>(
                        configuredCircuits
                    )
                    : new List<ElectricalCircuit>();

            initialized = false;
            Initialize();
            PublishChanged();
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            mainOn = mainStartsOn;
            mainTripped = false;
            currentMainLoadWatts = 0;

            if (circuits == null)
            {
                circuits = new List<ElectricalCircuit>();
            }

            foreach (ElectricalCircuit circuit in circuits)
            {
                circuit?.Initialize();
            }

            initialized = true;
        }

        public bool TryGetCircuit(
            string circuitId,
            out ElectricalCircuit circuit
        )
        {
            Initialize();

            circuit = circuits.Find(
                entry =>
                    entry != null &&
                    string.Equals(
                        entry.CircuitId,
                        circuitId,
                        StringComparison.Ordinal
                    )
            );

            return circuit != null;
        }

        public bool IsCircuitPowered(string circuitId)
        {
            return
                mainOn &&
                !mainTripped &&
                TryGetCircuit(
                    circuitId,
                    out ElectricalCircuit circuit
                ) &&
                circuit.IsOn &&
                !circuit.IsTripped;
        }

        public void ToggleMain()
        {
            Initialize();

            if (mainTripped)
            {
                mainTripped = false;
                mainOn = true;
            }
            else
            {
                mainOn = !mainOn;
            }

            PublishChanged();
        }

        public void ToggleCircuit(string circuitId)
        {
            if (!TryGetCircuit(circuitId, out ElectricalCircuit circuit))
            {
                return;
            }

            circuit.Toggle();
            PublishChanged();
        }

        public void RestoreMainState(
            bool enabledState,
            bool trippedState
        )
        {
            initialized = true;
            mainOn = enabledState;
            mainTripped = trippedState;
            PublishChanged();
        }

        public void RestoreCircuitState(
            string circuitId,
            bool enabledState,
            bool trippedState
        )
        {
            if (!TryGetCircuit(circuitId, out ElectricalCircuit circuit))
            {
                return;
            }

            circuit.SetState(
                enabledState,
                trippedState
            );

            PublishChanged();
        }

        internal void ResetCalculatedLoads()
        {
            Initialize();
            currentMainLoadWatts = 0;

            foreach (ElectricalCircuit circuit in circuits)
            {
                circuit?.SetLoad(0);
            }
        }

        internal void SetCalculatedLoad(
            string circuitId,
            int watts
        )
        {
            if (!TryGetCircuit(circuitId, out ElectricalCircuit circuit))
            {
                return;
            }

            circuit.SetLoad(watts);
            currentMainLoadWatts += Mathf.Max(0, watts);
        }

        internal bool EvaluateTrips()
        {
            Initialize();

            bool changed = false;

            foreach (ElectricalCircuit circuit in circuits)
            {
                if (
                    circuit == null ||
                    !circuit.IsOn ||
                    circuit.IsTripped
                )
                {
                    continue;
                }

                if (
                    circuit.CurrentLoadWatts >
                    circuit.CapacityWatts
                )
                {
                    circuit.Trip();
                    changed = true;
                }
            }

            if (
                mainOn &&
                !mainTripped &&
                currentMainLoadWatts >
                mainCapacityWatts
            )
            {
                mainTripped = true;
                mainOn = false;
                changed = true;
            }

            if (changed)
            {
                PowerChanged?.Invoke();
            }

            return changed;
        }

        internal void PublishChanged()
        {
            PowerChanged?.Invoke();
            ElectricalGridSystem.NotifyChanged();
        }
    }
}
