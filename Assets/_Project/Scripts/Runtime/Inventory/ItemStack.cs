using System;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    [Serializable]
    public sealed class ItemStack
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(0)] private int quantity;
        [SerializeField] private ItemQuality quality = ItemQuality.Standard;
        [SerializeField, Range(0f, 1f)] private float condition = 1f;

        public ItemDefinition Item => item;
        public int Quantity => quantity;
        public ItemQuality Quality => quality;
        public float Condition => condition;
        public bool IsEmpty => item == null || quantity <= 0;

        public ItemStack()
        {
        }

        public ItemStack(
            ItemDefinition item,
            int quantity,
            ItemQuality quality = ItemQuality.Standard,
            float condition = 1f
        )
        {
            this.item = item;
            this.quantity = Mathf.Max(0, quantity);
            this.quality = quality;
            this.condition = Mathf.Clamp01(condition);
        }

        public bool CanStackWith(ItemStack other)
        {
            return
                other != null &&
                !IsEmpty &&
                !other.IsEmpty &&
                item == other.item &&
                quality == other.quality &&
                Mathf.Abs(condition - other.condition) <= 0.001f;
        }

        public int Add(int amount)
        {
            if (item == null || amount <= 0)
            {
                return amount;
            }

            int available = item.MaximumStack - quantity;
            int accepted = Mathf.Min(available, amount);
            quantity += accepted;
            return amount - accepted;
        }

        public int Remove(int amount)
        {
            int removed = Mathf.Min(Mathf.Max(0, amount), quantity);
            quantity -= removed;

            if (quantity <= 0)
            {
                Clear();
            }

            return removed;
        }

        public void Clear()
        {
            item = null;
            quantity = 0;
            quality = ItemQuality.Standard;
            condition = 1f;
        }

        public ItemStack Clone(int overrideQuantity = -1)
        {
            return new ItemStack(
                item,
                overrideQuantity >= 0 ? overrideQuantity : quantity,
                quality,
                condition
            );
        }
    }
}
