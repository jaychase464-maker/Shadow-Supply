using System;
using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Production
{
    [Serializable]
    public sealed class ProductionIngredient
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField] private bool consumed = true;

        public ItemDefinition Item => item;
        public int Quantity => quantity;
        public bool Consumed => consumed;

        public ProductionIngredient()
        {
        }

        public ProductionIngredient(
            ItemDefinition ingredientItem,
            int requiredQuantity,
            bool consumeOnStart
        )
        {
            item = ingredientItem;
            quantity = Mathf.Max(1, requiredQuantity);
            consumed = consumeOnStart;
        }
    }
}
