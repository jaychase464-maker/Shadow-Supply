using System;
using ShadowSupply.Economy;
using ShadowSupply.Inventory;
using ShadowSupply.Placement;
using UnityEngine;

namespace ShadowSupply.Delivery
{
    public sealed class FurnitureDeliverySystem :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerWallet wallet;
        [SerializeField]
        private Transform deliveryPoint;
        [SerializeField]
        private GameObject deliveryCratePrefab;
        [SerializeField, Min(0.2f)]
        private float crateSpacing = 1.25f;

        public event Action Changed;
        public event Action<string, bool>
            StatusChanged;

        public PlayerWallet Wallet => wallet;
        public Transform DeliveryPoint =>
            deliveryPoint;

        public void Configure(
            PlayerWallet playerWallet,
            Transform point,
            GameObject cratePrefab
        )
        {
            wallet = playerWallet;
            deliveryPoint = point;
            deliveryCratePrefab = cratePrefab;
        }

        public bool TryOrder(
            PlaceableDefinition definition
        )
        {
            if (
                definition == null ||
                definition.InventoryItem == null
            )
            {
                Publish(
                    "This catalog entry has no delivery item.",
                    false
                );
                return false;
            }

            if (wallet == null)
            {
                Publish(
                    "Furniture ordering is unavailable.",
                    false
                );
                return false;
            }

            int price =
                Mathf.Max(
                    0,
                    definition.PurchasePrice
                );

            if (
                !wallet.TrySpendCleanCash(price)
            )
            {
                Publish(
                    $"Not enough clean cash for " +
                    $"{definition.DisplayName}.",
                    false
                );
                return false;
            }

            FurnitureDeliveryCrate crate =
                SpawnDelivery(
                    definition.InventoryItem,
                    1,
                    null,
                    GetNextDeliveryPosition(),
                    Quaternion.identity
                );

            if (crate == null)
            {
                wallet.AddCleanCash(price);

                Publish(
                    "Delivery creation failed. " +
                    "Payment refunded.",
                    false
                );
                return false;
            }

            Publish(
                $"Ordered {definition.DisplayName} " +
                $"for ${price:N0}.",
                true
            );

            Changed?.Invoke();
            return true;
        }

        public FurnitureDeliveryCrate
            SpawnItemDelivery(
                ItemDefinition item,
                int quantity,
                string sourceName = null
            )
        {
            if (
                item == null ||
                quantity <= 0
            )
            {
                Publish(
                    "Item delivery data is invalid.",
                    false
                );
                return null;
            }

            FurnitureDeliveryCrate crate =
                SpawnDelivery(
                    item,
                    quantity,
                    null,
                    GetNextDeliveryPosition(),
                    Quaternion.identity
                );

            if (crate == null)
            {
                Publish(
                    $"Could not create delivery for " +
                    $"{item.DisplayName}.",
                    false
                );
                return null;
            }

            string source =
                string.IsNullOrWhiteSpace(
                    sourceName
                )
                    ? "Supplier"
                    : sourceName;

            Publish(
                $"{source} delivered " +
                $"{item.DisplayName} x{quantity}.",
                true
            );

            Changed?.Invoke();
            return crate;
        }

        public FurnitureDeliveryCrate
            SpawnRestoredDelivery(
                string persistentId,
                ItemDefinition item,
                int quantity,
                Vector3 position,
                Quaternion rotation
            )
        {
            return
                SpawnDelivery(
                    item,
                    quantity,
                    persistentId,
                    position,
                    rotation
                );
        }

        public void ClearCurrentDeliveries()
        {
            FurnitureDeliveryCrate[] crates =
                FindObjectsByType<
                    FurnitureDeliveryCrate
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (
                FurnitureDeliveryCrate crate
                in crates
            )
            {
                if (crate == null)
                {
                    continue;
                }

                crate.gameObject.SetActive(false);
                Destroy(crate.gameObject);
            }

            Changed?.Invoke();
        }

        private FurnitureDeliveryCrate
            SpawnDelivery(
                ItemDefinition item,
                int quantity,
                string persistentId,
                Vector3 position,
                Quaternion rotation
            )
        {
            if (
                item == null ||
                deliveryCratePrefab == null
            )
            {
                return null;
            }

            GameObject instance =
                Instantiate(
                    deliveryCratePrefab,
                    position,
                    rotation
                );

            instance.SetActive(true);

            FurnitureDeliveryCrate crate =
                instance.GetComponent<
                    FurnitureDeliveryCrate
                >() ??
                instance.AddComponent<
                    FurnitureDeliveryCrate
                >();

            crate.Initialize(
                persistentId,
                item,
                quantity
            );

            return crate;
        }

        private Vector3 GetNextDeliveryPosition()
        {
            Vector3 basePosition =
                deliveryPoint != null
                    ? deliveryPoint.position
                    : transform.position;

            FurnitureDeliveryCrate[] crates =
                FindObjectsByType<
                    FurnitureDeliveryCrate
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            int index =
                crates != null
                    ? crates.Length
                    : 0;

            int row = index / 3;
            int column = index % 3;

            return
                basePosition +
                Vector3.right *
                column *
                crateSpacing +
                Vector3.forward *
                row *
                crateSpacing;
        }

        private void Publish(
            string message,
            bool success
        )
        {
            if (success)
            {
                Debug.Log(
                    $"[FurnitureDeliverySystem] " +
                    message,
                    this
                );
            }
            else
            {
                Debug.LogWarning(
                    $"[FurnitureDeliverySystem] " +
                    message,
                    this
                );
            }

            StatusChanged?.Invoke(
                message,
                success
            );
        }
    }
}
