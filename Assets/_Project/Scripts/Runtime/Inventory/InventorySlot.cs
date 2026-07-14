using System;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    [Serializable]
    public sealed class InventorySlot
    {
        [SerializeField] private ItemStack stack = new ItemStack();

        public ItemStack Stack => stack;
        public bool IsEmpty => stack == null || stack.IsEmpty;

        public void Set(ItemStack newStack)
        {
            stack = newStack?.Clone() ?? new ItemStack();
        }

        public void Clear()
        {
            stack ??= new ItemStack();
            stack.Clear();
        }
    }
}
