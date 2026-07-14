using System;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Delivery
{
    [DisallowMultipleComponent]
    public sealed class FurnitureDeliveryCrate :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField, HideInInspector] private string persistentId;
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(1)] private int quantity = 1;

        public string PersistentId => persistentId;
        public ItemDefinition Item => item;
        public int Quantity => quantity;

        public string InteractionPrompt =>
            item == null
                ? "Invalid delivery"
                : $"Collect {item.DisplayName} x{quantity}";

        private void Awake()
        {
            EnsurePersistentId();
        }

        public void Initialize(
            string id,
            ItemDefinition deliveredItem,
            int deliveredQuantity
        )
        {
            persistentId =
                string.IsNullOrWhiteSpace(id)
                    ? Guid.NewGuid().ToString("N")
                    : id;

            item = deliveredItem;
            quantity = Mathf.Max(1, deliveredQuantity);

            if (item != null)
            {
                string shortId =
                    persistentId.Length > 8
                        ? persistentId.Substring(0, 8)
                        : persistentId;

                gameObject.name =
                    $"Delivery_{item.DisplayName}_{shortId}";
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                interactor != null &&
                item != null &&
                quantity > 0 &&
                interactor.GetComponent<PlayerInventory>() != null;
        }

        public void Interact(GameObject interactor)
        {
            PlayerInventory inventory =
                interactor.GetComponent<PlayerInventory>();

            if (
                inventory == null ||
                item == null ||
                quantity <= 0
            )
            {
                return;
            }

            int remaining =
                inventory.AddItem(
                    item,
                    quantity,
                    ItemQuality.Standard,
                    1f
                );

            int collected = quantity - remaining;
            quantity = remaining;

            if (collected <= 0)
            {
                Debug.LogWarning(
                    "[FurnitureDeliveryCrate] Inventory is full.",
                    this
                );
                return;
            }

            Debug.Log(
                $"[FurnitureDeliveryCrate] Collected " +
                $"{item.DisplayName} x{collected}.",
                this
            );

            if (quantity <= 0)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        public void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(persistentId))
            {
                persistentId =
                    Guid.NewGuid().ToString("N");
            }
        }
    }
}
