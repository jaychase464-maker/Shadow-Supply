using System;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class MachinePowerConnection :
        MonoBehaviour
    {
        [SerializeField] private string machineId;
        [SerializeField] private string displayName =
            "Machine";
        [SerializeField, Min(1)] private int powerDemandWatts =
            800;
        [SerializeField] private PowerPlug plug;
        [SerializeField] private Renderer statusIndicator;

        private bool isPowered;

        public event Action<bool> PoweredStateChanged;

        public string MachineId => machineId;
        public string DisplayName => displayName;
        public int PowerDemandWatts => powerDemandWatts;
        public PowerPlug Plug => plug;
        public bool IsPowered => isPowered;

        private void Awake()
        {
            RefreshPoweredState();
        }

        private void OnEnable()
        {
            ElectricalGridSystem.NotifyChanged();
        }

        private void OnDisable()
        {
            ElectricalGridSystem.NotifyChanged();
        }

        public void Configure(
            string stableMachineId,
            string machineDisplayName,
            int demandWatts,
            PowerPlug configuredPlug,
            Renderer indicator
        )
        {
            machineId = stableMachineId;
            displayName = machineDisplayName;
            powerDemandWatts =
                Mathf.Max(1, demandWatts);
            plug = configuredPlug;
            statusIndicator = indicator;

            RefreshPoweredState();
            ElectricalGridSystem.NotifyChanged();
        }

        public bool TryGetConnectedCircuit(
            out ElectricalPanel panel,
            out string circuitId
        )
        {
            panel = null;
            circuitId = null;

            OutletSocket socket =
                plug != null
                    ? plug.ConnectedSocket
                    : null;

            PowerOutlet outlet =
                socket != null
                    ? socket.Outlet
                    : null;

            if (
                outlet == null ||
                outlet.Panel == null ||
                string.IsNullOrWhiteSpace(
                    outlet.CircuitId
                )
            )
            {
                return false;
            }

            panel = outlet.Panel;
            circuitId = outlet.CircuitId;
            return true;
        }

        public void RefreshPoweredState()
        {
            bool powered =
                TryGetConnectedCircuit(
                    out ElectricalPanel panel,
                    out string circuitId
                ) &&
                panel.IsCircuitPowered(circuitId);

            if (isPowered == powered)
            {
                RefreshIndicator();
                return;
            }

            isPowered = powered;
            RefreshIndicator();
            PoweredStateChanged?.Invoke(isPowered);
        }

        private void RefreshIndicator()
        {
            if (statusIndicator == null)
            {
                return;
            }

            MaterialPropertyBlock block =
                new MaterialPropertyBlock();

            statusIndicator.GetPropertyBlock(block);

            Color color =
                isPowered
                    ? new Color(0.1f, 1f, 0.25f)
                    : new Color(1f, 0.12f, 0.08f);

            block.SetColor(
                "_BaseColor",
                color
            );

            block.SetColor(
                "_Color",
                color
            );

            block.SetColor(
                "_EmissionColor",
                color * 1.5f
            );

            statusIndicator.SetPropertyBlock(block);
        }
    }
}
