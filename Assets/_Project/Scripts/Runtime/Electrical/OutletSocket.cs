using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class OutletSocket :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private PowerOutlet outlet;
        [SerializeField] private int socketIndex;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private Renderer highlightRenderer;

        private PowerPlug connectedPlug;

        public PowerOutlet Outlet => outlet;
        public int SocketIndex => socketIndex;
        public Transform SnapPoint =>
            snapPoint != null
                ? snapPoint
                : transform;
        public Renderer HighlightRenderer => highlightRenderer;
        public PowerPlug ConnectedPlug => connectedPlug;
        public bool IsOccupied => connectedPlug != null;

        public string InteractionPrompt
        {
            get
            {
                PlayerPlugController controller =
                    PlayerPlugController.Local;

                if (connectedPlug != null)
                {
                    return $"Unplug {connectedPlug.DisplayName}";
                }

                if (
                    controller != null &&
                    controller.HeldPlug != null
                )
                {
                    return controller.HeldPlug.CanReach(this)
                        ? $"Plug into {GetSocketName()}"
                        : "Cable cannot reach this socket";
                }

                return outlet != null && outlet.IsPowered
                    ? $"{GetSocketName()} — powered"
                    : $"{GetSocketName()} — no power";
            }
        }

        private void Update()
        {
            RefreshHighlight();
        }

        public void Configure(
            PowerOutlet outletReference,
            int index,
            Transform configuredSnapPoint,
            Renderer configuredHighlight
        )
        {
            outlet = outletReference;
            socketIndex = index;
            snapPoint = configuredSnapPoint;
            highlightRenderer = configuredHighlight;
            RefreshHighlight();
        }

        public bool CanInteract(GameObject interactor)
        {
            PlayerPlugController controller =
                interactor != null
                    ? interactor.GetComponent<PlayerPlugController>()
                    : PlayerPlugController.Local;

            if (connectedPlug != null)
            {
                return controller != null;
            }

            return
                controller != null &&
                controller.HeldPlug != null;
        }

        public void Interact(GameObject interactor)
        {
            PlayerPlugController controller =
                interactor != null
                    ? interactor.GetComponent<PlayerPlugController>()
                    : PlayerPlugController.Local;

            if (controller == null)
            {
                return;
            }

            if (connectedPlug != null)
            {
                PowerPlug plug = connectedPlug;

                plug.Disconnect();
                controller.BeginHolding(plug);
                return;
            }

            controller.TryConnectHeldPlug(this);
        }

        public bool TryAttach(PowerPlug plug)
        {
            if (
                plug == null ||
                connectedPlug != null
            )
            {
                return false;
            }

            connectedPlug = plug;
            RefreshHighlight();
            return true;
        }

        public void Detach(PowerPlug plug)
        {
            if (connectedPlug == plug)
            {
                connectedPlug = null;
                RefreshHighlight();
            }
        }

        private string GetSocketName()
        {
            return socketIndex == 0
                ? "upper socket"
                : "lower socket";
        }

        private void RefreshHighlight()
        {
            if (highlightRenderer == null)
            {
                return;
            }

            PlayerPlugController controller =
                PlayerPlugController.Local;

            bool shouldHighlight =
                connectedPlug == null &&
                controller != null &&
                controller.HeldPlug != null &&
                controller.HeldPlug.CanReach(this);

            highlightRenderer.enabled =
                shouldHighlight;
        }
    }
}
