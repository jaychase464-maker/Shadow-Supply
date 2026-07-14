using System;
using System.Collections.Generic;
using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Production
{
    [CreateAssetMenu(
        fileName = "RECIPE_NewProductionRecipe",
        menuName =
            "Shadow Supply/Production/Production Recipe"
    )]
    public sealed class ProductionRecipe : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string recipeId;
        [SerializeField] private string displayName =
            "New Production Recipe";
        [SerializeField, TextArea(2, 5)]
        private string description;

        [Header("Process")]
        [SerializeField, Min(0.1f)]
        private float durationSeconds = 8f;
        [SerializeField] private bool requiresPower = true;
        [SerializeField]
        private List<ProductionIngredient> ingredients =
            new List<ProductionIngredient>();

        [Header("Output")]
        [SerializeField] private ItemDefinition outputItem;
        [SerializeField, Min(1)] private int outputQuantity = 1;

        public string RecipeId => recipeId;
        public string DisplayName => displayName;
        public string Description => description;
        public float DurationSeconds => durationSeconds;
        public bool RequiresPower => requiresPower;
        public IReadOnlyList<ProductionIngredient> Ingredients =>
            ingredients;
        public ItemDefinition OutputItem => outputItem;
        public int OutputQuantity => outputQuantity;

        public void Configure(
            string stableRecipeId,
            string recipeDisplayName,
            string recipeDescription,
            float duration,
            bool powerRequired,
            IEnumerable<ProductionIngredient>
                recipeIngredients,
            ItemDefinition resultItem,
            int resultQuantity
        )
        {
            recipeId = stableRecipeId;
            displayName = recipeDisplayName;
            description = recipeDescription;
            durationSeconds = Mathf.Max(0.1f, duration);
            requiresPower = powerRequired;
            ingredients =
                recipeIngredients != null
                    ? new List<ProductionIngredient>(
                        recipeIngredients
                    )
                    : new List<ProductionIngredient>();
            outputItem = resultItem;
            outputQuantity = Mathf.Max(1, resultQuantity);
        }

        private void OnValidate()
        {
            durationSeconds =
                Mathf.Max(0.1f, durationSeconds);
            outputQuantity =
                Mathf.Max(1, outputQuantity);

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }
        }
    }
}
