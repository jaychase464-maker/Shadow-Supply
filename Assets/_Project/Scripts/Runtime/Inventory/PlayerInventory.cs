using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int slotCount = 24;
        [SerializeField, Min(1)] private int hotbarSize = 8;
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

        public event Action Changed;

        public IReadOnlyList<InventorySlot> Slots => slots;
        public int SlotCount => slots.Count;
        public int HotbarSize => Mathf.Min(hotbarSize, slots.Count);

        private void Awake()
        {
            InitializeSlots();
        }

        private void OnValidate()
        {
            slotCount = Mathf.Max(1, slotCount);
            hotbarSize = Mathf.Clamp(hotbarSize, 1, slotCount);
        }

        public InventorySlot GetSlot(int index)
        {
            return index >= 0 && index < slots.Count ? slots[index] : null;
        }

        public int AddItem(
            ItemDefinition item,
            int quantity,
            ItemQuality quality = ItemQuality.Standard,
            float condition = 1f
        )
        {
            if (item == null || quantity <= 0)
            {
                return quantity;
            }

            InitializeSlots();

            ItemStack incoming = new ItemStack(item, quantity, quality, condition);
            int remaining = quantity;

            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                InventorySlot slot = slots[i];

                if (slot.IsEmpty || !slot.Stack.CanStackWith(incoming))
                {
                    continue;
                }

                remaining = slot.Stack.Add(remaining);
            }

            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                InventorySlot slot = slots[i];

                if (!slot.IsEmpty)
                {
                    continue;
                }

                int amountForSlot = Mathf.Min(item.MaximumStack, remaining);
                slot.Set(new ItemStack(item, amountForSlot, quality, condition));
                remaining -= amountForSlot;
            }

            if (remaining != quantity)
            {
                Changed?.Invoke();
            }

            return remaining;
        }

        public ItemStack RemoveFromSlot(int index, int quantity)
        {
            InventorySlot slot = GetSlot(index);

            if (slot == null || slot.IsEmpty || quantity <= 0)
            {
                return null;
            }

            int removed = Mathf.Min(quantity, slot.Stack.Quantity);
            ItemStack result = slot.Stack.Clone(removed);
            slot.Stack.Remove(removed);
            Changed?.Invoke();
            return result;
        }

        public bool HasSpaceFor(
            ItemDefinition item,
            int quantity,
            ItemQuality quality = ItemQuality.Standard,
            float condition = 1f
        )
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            ItemStack incoming = new ItemStack(item, quantity, quality, condition);
            int capacity = 0;

            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty)
                {
                    capacity += item.MaximumStack;
                }
                else if (slot.Stack.CanStackWith(incoming))
                {
                    capacity += item.MaximumStack - slot.Stack.Quantity;
                }

                if (capacity >= quantity)
                {
                    return true;
                }
            }

            return false;
        }

        private void InitializeSlots()
        {
            slots ??= new List<InventorySlot>();

            while (slots.Count < slotCount)
            {
                slots.Add(new InventorySlot());
            }

            if (slots.Count > slotCount)
            {
                slots.RemoveRange(slotCount, slots.Count - slotCount);
            }

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i] ??= new InventorySlot();
            }
        }
    }
}
