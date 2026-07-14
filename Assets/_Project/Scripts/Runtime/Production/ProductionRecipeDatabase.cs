using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Production
{
    [CreateAssetMenu(
        fileName = "DB_ProductionRecipes",
        menuName =
            "Shadow Supply/Production/Recipe Database"
    )]
    public sealed class ProductionRecipeDatabase :
        ScriptableObject
    {
        [SerializeField]
        private List<ProductionRecipe> recipes =
            new List<ProductionRecipe>();

        private readonly Dictionary<
            string,
            ProductionRecipe
        > lookup =
            new Dictionary<
                string,
                ProductionRecipe
            >(StringComparer.Ordinal);

        public IReadOnlyList<ProductionRecipe> Recipes =>
            recipes;

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        public void SetRecipes(
            IEnumerable<ProductionRecipe> definitions
        )
        {
            recipes.Clear();

            if (definitions != null)
            {
                foreach (
                    ProductionRecipe recipe
                    in definitions
                )
                {
                    if (
                        recipe != null &&
                        !recipes.Contains(recipe)
                    )
                    {
                        recipes.Add(recipe);
                    }
                }
            }

            recipes.Sort(
                (left, right) =>
                    string.Compare(
                        left != null
                            ? left.DisplayName
                            : string.Empty,
                        right != null
                            ? right.DisplayName
                            : string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );

            RebuildLookup();
        }

        public bool TryGetRecipe(
            string recipeId,
            out ProductionRecipe recipe
        )
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                recipe = null;
                return false;
            }

            if (lookup.Count != recipes.Count)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(
                recipeId,
                out recipe
            );
        }

        private void RebuildLookup()
        {
            lookup.Clear();

            foreach (
                ProductionRecipe recipe
                in recipes
            )
            {
                if (
                    recipe == null ||
                    string.IsNullOrWhiteSpace(
                        recipe.RecipeId
                    ) ||
                    lookup.ContainsKey(
                        recipe.RecipeId
                    )
                )
                {
                    continue;
                }

                lookup.Add(
                    recipe.RecipeId,
                    recipe
                );
            }
        }
    }
}
