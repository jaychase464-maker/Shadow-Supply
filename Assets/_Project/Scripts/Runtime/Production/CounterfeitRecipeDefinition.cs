using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Production
{
    [CreateAssetMenu(
        fileName = "COUNTERFEIT_RECIPE_NewRecipe",
        menuName =
            "Shadow Supply/Production/Counterfeit Recipe"
    )]
    public sealed class CounterfeitRecipeDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string recipeId;
        [SerializeField] private string displayName =
            "Replica Note Bundle";
        [SerializeField, TextArea(3, 8)]
        private string description;

        [Header("Consumable Materials")]
        [SerializeField]
        private ItemDefinition blankNoteStock;
        [SerializeField]
        private ItemDefinition pigmentCapsule;
        [SerializeField]
        private ItemDefinition securityFilm;
        [SerializeField]
        private ItemDefinition packagingMaterial;

        [Header("Reusable Tool")]
        [SerializeField]
        private ItemDefinition basicToolkit;

        [Header("Output")]
        [SerializeField]
        private ItemDefinition outputItem;
        [SerializeField, Min(1)]
        private int outputQuantity = 1;

        [Header("Process")]
        [SerializeField]
        private bool requiresPower = true;
        [SerializeField, Min(0.25f)]
        private float printHoldSeconds = 1.75f;
        [SerializeField, Min(1)]
        private int baseReputationReward = 3;

        public string RecipeId => recipeId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemDefinition BlankNoteStock =>
            blankNoteStock;
        public ItemDefinition PigmentCapsule =>
            pigmentCapsule;
        public ItemDefinition SecurityFilm =>
            securityFilm;
        public ItemDefinition PackagingMaterial =>
            packagingMaterial;
        public ItemDefinition BasicToolkit =>
            basicToolkit;
        public ItemDefinition OutputItem => outputItem;
        public int OutputQuantity => outputQuantity;
        public bool RequiresPower => requiresPower;
        public float PrintHoldSeconds =>
            printHoldSeconds;
        public int BaseReputationReward =>
            baseReputationReward;

        public void Configure(
            string stableRecipeId,
            string readableName,
            string readableDescription,
            ItemDefinition stockItem,
            ItemDefinition pigmentItem,
            ItemDefinition filmItem,
            ItemDefinition packagingItem,
            ItemDefinition toolkitItem,
            ItemDefinition resultItem,
            int resultQuantity,
            bool powerRequired,
            float requiredPrintHoldSeconds,
            int reputationReward
        )
        {
            recipeId = stableRecipeId;
            displayName = readableName;
            description = readableDescription;
            blankNoteStock = stockItem;
            pigmentCapsule = pigmentItem;
            securityFilm = filmItem;
            packagingMaterial = packagingItem;
            basicToolkit = toolkitItem;
            outputItem = resultItem;
            outputQuantity = Mathf.Max(1, resultQuantity);
            requiresPower = powerRequired;
            printHoldSeconds =
                Mathf.Max(
                    0.25f,
                    requiredPrintHoldSeconds
                );
            baseReputationReward =
                Mathf.Max(1, reputationReward);
        }

        public bool IsValid()
        {
            return
                !string.IsNullOrWhiteSpace(recipeId) &&
                blankNoteStock != null &&
                pigmentCapsule != null &&
                securityFilm != null &&
                packagingMaterial != null &&
                basicToolkit != null &&
                outputItem != null;
        }

        private void OnValidate()
        {
            outputQuantity =
                Mathf.Max(1, outputQuantity);
            printHoldSeconds =
                Mathf.Max(
                    0.25f,
                    printHoldSeconds
                );
            baseReputationReward =
                Mathf.Max(
                    1,
                    baseReputationReward
                );
        }
    }
}
