using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class ElectricalStatusHUD : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private ElectricalPanel panel;
        [SerializeField] private MachinePowerConnection workbench;

        public void Configure(
            ElectricalPanel panelReference,
            MachinePowerConnection workbenchReference
        )
        {
            panel = panelReference;
            workbench = workbenchReference;
        }

        private void OnGUI()
        {
            if (!visible || panel == null)
            {
                return;
            }

            Rect area =
                new Rect(
                    12f,
                    12f,
                    300f,
                    155f
                );

            GUI.Box(area, string.Empty);

            GUILayout.BeginArea(
                new Rect(
                    area.x + 10f,
                    area.y + 8f,
                    area.width - 20f,
                    area.height - 16f
                )
            );

            GUILayout.Label(
                "<b>GARAGE POWER</b>",
                GetLabelStyle()
            );

            GUILayout.Label(
                $"Main: {GetMainStatus()}  " +
                $"{panel.CurrentMainLoadWatts}/" +
                $"{panel.MainCapacityWatts} W"
            );

            foreach (
                ElectricalCircuit circuit
                in panel.Circuits
            )
            {
                if (circuit == null)
                {
                    continue;
                }

                GUILayout.Label(
                    $"{circuit.DisplayName}: " +
                    $"{GetCircuitStatus(circuit)}  " +
                    $"{circuit.CurrentLoadWatts}/" +
                    $"{circuit.CapacityWatts} W"
                );
            }

            if (workbench != null)
            {
                GUILayout.Label(
                    $"Workbench: " +
                    $"{(workbench.IsPowered ? "POWERED" : "NO POWER")}"
                );
            }

            PlayerPlugController controller =
                PlayerPlugController.Local;

            if (
                controller != null &&
                controller.HeldPlug != null
            )
            {
                GUILayout.Label(
                    "Holding plug — E: connect  R: release"
                );
            }

            GUILayout.EndArea();
        }

        private string GetMainStatus()
        {
            if (panel.MainTripped)
            {
                return "TRIPPED";
            }

            return panel.MainOn
                ? "ON"
                : "OFF";
        }

        private static string GetCircuitStatus(
            ElectricalCircuit circuit
        )
        {
            if (circuit.IsTripped)
            {
                return "TRIPPED";
            }

            return circuit.IsOn
                ? "ON"
                : "OFF";
        }

        private static GUIStyle GetLabelStyle()
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontSize = 13
                };

            return style;
        }
    }
}
