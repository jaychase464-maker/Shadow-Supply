using System;
using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    public sealed class PowerPlug :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string persistentId;
        [SerializeField] private string displayName =
            "Machine";
        [SerializeField] private MachinePowerConnection machine;
        [SerializeField] private Transform cableAnchor;
        [SerializeField, Min(0.25f)] private float maximumCableLength =
            3f;
        [SerializeField] private Rigidbody plugBody;
        [SerializeField] private Collider plugCollider;
        [SerializeField] private PowerCableVisual cableVisual;

        private PlayerPlugController holdingController;
        private OutletSocket connectedSocket;

        public string PersistentId => persistentId;
        public string DisplayName => displayName;
        public float MaximumCableLength => maximumCableLength;
        public Transform CableAnchor => cableAnchor;
        public OutletSocket ConnectedSocket => connectedSocket;
        public bool IsConnected => connectedSocket != null;
        public bool IsHeld => holdingController != null;

        public string ConnectedOutletId =>
            connectedSocket?.Outlet?.OutletId;

        public int ConnectedSocketIndex =>
            connectedSocket != null
                ? connectedSocket.SocketIndex
                : -1;

        public string InteractionPrompt =>
            IsConnected
                ? $"Unplug {displayName}"
                : $"Grab {displayName} plug";

        private void Awake()
        {
            EnsurePersistentId();
            ResolveReferences();
        }

        private void OnEnable()
        {
            ElectricalGridSystem.NotifyChanged();
        }

        private void OnDestroy()
        {
            connectedSocket?.Detach(this);
        }

        private void FixedUpdate()
        {
            if (
                IsHeld ||
                IsConnected ||
                plugBody == null ||
                cableAnchor == null
            )
            {
                return;
            }

            Vector3 offset =
                plugBody.position -
                cableAnchor.position;

            if (
                offset.sqrMagnitude <=
                maximumCableLength *
                maximumCableLength
            )
            {
                return;
            }

            Vector3 direction =
                offset.normalized;

            Vector3 clampedPosition =
                cableAnchor.position +
                direction *
                maximumCableLength;

            plugBody.MovePosition(
                clampedPosition
            );

            float outwardSpeed =
                Vector3.Dot(
                    plugBody.linearVelocity,
                    direction
                );

            if (outwardSpeed > 0f)
            {
                plugBody.linearVelocity -=
                    direction *
                    outwardSpeed;
            }
        }

        public void Configure(
            string stablePersistentId,
            string machineDisplayName,
            MachinePowerConnection machineReference,
            Transform anchor,
            float cableLength,
            Rigidbody body,
            Collider interactionCollider,
            PowerCableVisual visual
        )
        {
            persistentId = stablePersistentId;
            displayName = machineDisplayName;
            machine = machineReference;
            cableAnchor = anchor;
            maximumCableLength = Mathf.Max(0.25f, cableLength);
            plugBody = body;
            plugCollider = interactionCollider;
            cableVisual = visual;

            EnsurePersistentId();
            ResolveReferences();

            cableVisual?.Configure(
                cableAnchor,
                transform,
                maximumCableLength
            );
        }

        public void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(persistentId))
            {
                persistentId =
                    Guid.NewGuid().ToString("N");
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                enabled &&
                gameObject.activeInHierarchy &&
                !IsHeld;
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

            if (IsConnected)
            {
                Disconnect();
            }

            controller.BeginHolding(this);
        }

        public bool CanReach(OutletSocket socket)
        {
            if (
                socket == null ||
                cableAnchor == null
            )
            {
                return false;
            }

            return
                Vector3.Distance(
                    cableAnchor.position,
                    socket.SnapPoint.position
                ) <= maximumCableLength + 0.03f;
        }

        public void EnterHeld(
            PlayerPlugController controller
        )
        {
            if (IsConnected)
            {
                Disconnect();
            }

            holdingController = controller;

            if (plugBody != null)
            {
                plugBody.isKinematic = true;
                plugBody.linearVelocity = Vector3.zero;
                plugBody.angularVelocity = Vector3.zero;
            }

            if (plugCollider != null)
            {
                plugCollider.enabled = false;
            }

            ElectricalGridSystem.NotifyChanged();
        }

        public void SetHeldPose(
            Vector3 desiredPosition,
            Quaternion desiredRotation
        )
        {
            if (
                holdingController == null ||
                cableAnchor == null
            )
            {
                return;
            }

            Vector3 offset =
                desiredPosition - cableAnchor.position;

            if (
                offset.sqrMagnitude >
                maximumCableLength *
                maximumCableLength
            )
            {
                offset =
                    offset.normalized *
                    maximumCableLength;
            }

            transform.SetPositionAndRotation(
                cableAnchor.position + offset,
                desiredRotation
            );
        }

        public void ReleaseFromHold()
        {
            holdingController = null;

            if (plugBody != null)
            {
                plugBody.isKinematic = false;
                plugBody.WakeUp();
            }

            if (plugCollider != null)
            {
                plugCollider.enabled = true;
            }

            ElectricalGridSystem.NotifyChanged();
        }

        public bool ConnectTo(
            OutletSocket socket,
            bool immediate = false
        )
        {
            if (
                socket == null ||
                !CanReach(socket) ||
                (
                    socket.IsOccupied &&
                    socket.ConnectedPlug != this
                )
            )
            {
                return false;
            }

            if (
                connectedSocket != null &&
                connectedSocket != socket
            )
            {
                connectedSocket.Detach(this);
            }

            if (!socket.TryAttach(this))
            {
                return false;
            }

            holdingController?.ClearHeldReference(this);
            holdingController = null;
            connectedSocket = socket;

            if (plugBody != null)
            {
                plugBody.isKinematic = true;
                plugBody.linearVelocity = Vector3.zero;
                plugBody.angularVelocity = Vector3.zero;
            }

            if (plugCollider != null)
            {
                plugCollider.enabled = true;
            }

            Transform snap = socket.SnapPoint;

            transform.SetPositionAndRotation(
                snap.position,
                snap.rotation
            );

            ElectricalGridSystem.NotifyChanged();
            machine?.RefreshPoweredState();
            return true;
        }

        public void Disconnect()
        {
            OutletSocket previous =
                connectedSocket;

            connectedSocket = null;
            previous?.Detach(this);

            if (plugBody != null)
            {
                plugBody.isKinematic = false;
            }

            if (plugCollider != null)
            {
                plugCollider.enabled = true;
            }

            ElectricalGridSystem.NotifyChanged();
            machine?.RefreshPoweredState();
        }

        public void RestoreFreePose(
            Vector3 position,
            Quaternion rotation
        )
        {
            holdingController?.ClearHeldReference(this);
            holdingController = null;

            if (connectedSocket != null)
            {
                connectedSocket.Detach(this);
                connectedSocket = null;
            }

            transform.SetPositionAndRotation(
                position,
                rotation
            );

            if (plugBody != null)
            {
                plugBody.isKinematic = false;
                plugBody.linearVelocity = Vector3.zero;
                plugBody.angularVelocity = Vector3.zero;
            }

            if (plugCollider != null)
            {
                plugCollider.enabled = true;
            }

            ElectricalGridSystem.NotifyChanged();
        }

        private void ResolveReferences()
        {
            if (plugBody == null)
            {
                plugBody = GetComponent<Rigidbody>();
            }

            if (plugCollider == null)
            {
                plugCollider = GetComponent<Collider>();
            }

            if (machine == null)
            {
                machine =
                    GetComponentInParent<
                        MachinePowerConnection
                    >();
            }
        }
    }
}
