using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int slotCount = 24;
        [SerializeField, Min(1)] private int hotbarSize = 8;
        [SerializeField] private List<InventorySlot> slots =
            new List<InventorySlot>();

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
            return index >= 0 && index < slots.Count
                ? slots[index]
                : null;
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

            ItemStack incoming =
                new ItemStack(item, quantity, quality, condition);

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

                int amountForSlot =
                    Mathf.Min(item.MaximumStack, remaining);

                slot.Set(
                    new ItemStack(
                        item,
                        amountForSlot,
                        quality,
                        condition
                    )
                );

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

            int removed =
                Mathf.Min(quantity, slot.Stack.Quantity);

            ItemStack result =
                slot.Stack.Clone(removed);

            slot.Stack.Remove(removed);
            Changed?.Invoke();
            return result;
        }


        public int CountItem(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            InitializeSlots();
            int total = 0;

            foreach (InventorySlot slot in slots)
            {
                if (
                    slot != null &&
                    !slot.IsEmpty &&
                    slot.Stack.Item == item
                )
                {
                    total += slot.Stack.Quantity;
                }
            }

            return total;
        }

        public bool TryRemoveItem(
            ItemDefinition item,
            int quantity,
            out ItemStack removedStack
        )
        {
            removedStack = null;

            if (
                item == null ||
                quantity <= 0 ||
                CountItem(item) < quantity
            )
            {
                return false;
            }

            InitializeSlots();

            ItemQuality quality = ItemQuality.Standard;
            float condition = 1f;
            int remaining = quantity;
            bool capturedMetadata = false;

            for (
                int index = slots.Count - 1;
                index >= 0 && remaining > 0;
                index--
            )
            {
                InventorySlot slot = slots[index];

                if (
                    slot == null ||
                    slot.IsEmpty ||
                    slot.Stack.Item != item
                )
                {
                    continue;
                }

                if (!capturedMetadata)
                {
                    quality = slot.Stack.Quality;
                    condition = slot.Stack.Condition;
                    capturedMetadata = true;
                }

                int removed =
                    slot.Stack.Remove(remaining);

                remaining -= removed;
            }

            if (remaining > 0)
            {
                Debug.LogError(
                    $"[PlayerInventory] Failed to remove the requested " +
                    $"quantity of '{item.DisplayName}'.",
                    this
                );
                return false;
            }

            removedStack =
                new ItemStack(
                    item,
                    quantity,
                    quality,
                    condition
                );

            Changed?.Invoke();
            return true;
        }

        public bool HasSpaceFor(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            InitializeSlots();
            int capacity = 0;

            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty)
                {
                    capacity += item.MaximumStack;
                }
                else if (slot.Stack.Item == item)
                {
                    capacity +=
                        item.MaximumStack -
                        slot.Stack.Quantity;
                }

                if (capacity >= quantity)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasEmptySlot()
        {
            InitializeSlots();

            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetTotalQuantity()
        {
            InitializeSlots();
            int total = 0;

            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    total += slot.Stack.Quantity;
                }
            }

            return total;
        }

        public bool SwapSlots(int firstIndex, int secondIndex)
        {
            InitializeSlots();

            if (
                firstIndex < 0 ||
                firstIndex >= slots.Count ||
                secondIndex < 0 ||
                secondIndex >= slots.Count
            )
            {
                return false;
            }

            if (firstIndex == secondIndex)
            {
                return true;
            }

            ItemStack first =
                slots[firstIndex].IsEmpty
                    ? null
                    : slots[firstIndex].Stack.Clone();

            ItemStack second =
                slots[secondIndex].IsEmpty
                    ? null
                    : slots[secondIndex].Stack.Clone();

            slots[firstIndex].Set(second);
            slots[secondIndex].Set(first);

            Changed?.Invoke();
            return true;
        }

        public int SplitStack(int sourceIndex)
        {
            InitializeSlots();

            InventorySlot source = GetSlot(sourceIndex);

            if (
                source == null ||
                source.IsEmpty ||
                source.Stack.Quantity < 2
            )
            {
                return -1;
            }

            int emptyIndex = FindFirstEmptySlot();

            if (emptyIndex < 0)
            {
                return -1;
            }

            int amountToMove =
                source.Stack.Quantity / 2;

            ItemStack splitStack =
                source.Stack.Clone(amountToMove);

            source.Stack.Remove(amountToMove);
            slots[emptyIndex].Set(splitStack);

            Changed?.Invoke();
            return emptyIndex;
        }

        public void RestoreSlots(
            IReadOnlyList<ItemStack> restoredStacks
        )
        {
            InitializeSlots();

            for (int i = 0; i < slots.Count; i++)
            {
                ItemStack restored =
                    restoredStacks != null &&
                    i < restoredStacks.Count
                        ? restoredStacks[i]
                        : null;

                if (restored == null || restored.IsEmpty)
                {
                    slots[i].Clear();
                }
                else
                {
                    slots[i].Set(restored);
                }
            }

            Changed?.Invoke();
        }

        public void ClearAll()
        {
            InitializeSlots();

            foreach (InventorySlot slot in slots)
            {
                slot.Clear();
            }

            Changed?.Invoke();
        }

        private int FindFirstEmptySlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    return i;
                }
            }

            return -1;
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
                slots.RemoveRange(
                    slotCount,
                    slots.Count - slotCount
                );
            }

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i] ??= new InventorySlot();
            }
        }
    }
}
