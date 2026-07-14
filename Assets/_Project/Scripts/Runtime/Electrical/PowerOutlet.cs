using System;
using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class PowerOutlet :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string outletId;
        [SerializeField] private ElectricalPanel panel;
        [SerializeField] private string circuitId;
        [SerializeField] private OutletSocket[] sockets;

        public string OutletId => outletId;
        public ElectricalPanel Panel => panel;
        public string CircuitId => circuitId;
        public bool IsPowered =>
            panel != null &&
            panel.IsCircuitPowered(circuitId);
        public OutletSocket[] Sockets => sockets;

        public string InteractionPrompt
        {
            get
            {
                OutletSocket socket =
                    GetBestSocketForCamera();

                return socket != null
                    ? socket.InteractionPrompt
                    : "Outlet";
            }
        }

        public void Configure(
            string stableOutletId,
            ElectricalPanel panelReference,
            string stableCircuitId,
            OutletSocket[] configuredSockets
        )
        {
            outletId = stableOutletId;
            panel = panelReference;
            circuitId = stableCircuitId;
            sockets = configuredSockets;

            if (sockets == null)
            {
                return;
            }

            for (int index = 0; index < sockets.Length; index++)
            {
                sockets[index]?.Configure(
                    this,
                    index,
                    sockets[index].SnapPoint,
                    sockets[index].HighlightRenderer
                );
            }
        }

        public OutletSocket GetSocket(int index)
        {
            return
                sockets != null &&
                index >= 0 &&
                index < sockets.Length
                    ? sockets[index]
                    : null;
        }

        public bool CanInteract(GameObject interactor)
        {
            OutletSocket socket =
                GetBestSocketForInteractor(interactor);

            return
                socket != null &&
                socket.CanInteract(interactor);
        }

        public void Interact(GameObject interactor)
        {
            OutletSocket socket =
                GetBestSocketForInteractor(interactor);

            socket?.Interact(interactor);
        }

        private OutletSocket GetBestSocketForCamera()
        {
            Camera camera = Camera.main;

            if (
                camera == null &&
                PlayerPlugController.Local != null
            )
            {
                camera =
                    PlayerPlugController.Local
                        .GetComponentInChildren<Camera>(true);
            }

            if (camera == null)
            {
                return GetFirstUsableSocket(null);
            }

            return GetBestSocketAlongRay(
                camera.transform.position,
                camera.transform.forward,
                null
            );
        }

        private OutletSocket GetBestSocketForInteractor(
            GameObject interactor
        )
        {
            Camera camera =
                interactor != null
                    ? interactor.GetComponentInChildren<Camera>(true)
                    : null;

            if (camera == null)
            {
                return GetFirstUsableSocket(interactor);
            }

            return GetBestSocketAlongRay(
                camera.transform.position,
                camera.transform.forward,
                interactor
            );
        }

        private OutletSocket GetBestSocketAlongRay(
            Vector3 origin,
            Vector3 direction,
            GameObject interactor
        )
        {
            OutletSocket best = null;
            float bestDistance = float.MaxValue;

            if (sockets == null)
            {
                return null;
            }

            foreach (OutletSocket socket in sockets)
            {
                if (
                    socket == null ||
                    (
                        interactor != null &&
                        !socket.CanInteract(interactor)
                    )
                )
                {
                    continue;
                }

                Vector3 toSocket =
                    socket.SnapPoint.position - origin;

                float forwardDistance =
                    Vector3.Dot(
                        toSocket,
                        direction
                    );

                Vector3 closestPoint =
                    origin +
                    direction *
                    Mathf.Max(0f, forwardDistance);

                float distance =
                    Vector3.Distance(
                        closestPoint,
                        socket.SnapPoint.position
                    );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = socket;
                }
            }

            return best;
        }

        private OutletSocket GetFirstUsableSocket(
            GameObject interactor
        )
        {
            if (sockets == null)
            {
                return null;
            }

            foreach (OutletSocket socket in sockets)
            {
                if (
                    socket != null &&
                    (
                        interactor == null ||
                        socket.CanInteract(interactor)
                    )
                )
                {
                    return socket;
                }
            }

            return null;
        }
    }
}
