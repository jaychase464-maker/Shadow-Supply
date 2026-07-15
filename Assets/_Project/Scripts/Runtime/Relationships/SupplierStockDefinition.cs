using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Relationships
{
    [CreateAssetMenu(
        fileName = "STOCK_NewSupplierItem",
        menuName =
            "Shadow Supply/Relationships/Supplier Stock Item"
    )]
    public sealed class SupplierStockDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string stockId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)]
        private string description;

        [Header("Item")]
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(1)] private int maximumStock = 4;
        [SerializeField, Min(1)] private int baseUnitPrice = 20;

        [Header("Relationship Requirements")]
        [SerializeField, Min(0)]
        private int minimumSuccessfulPurchases;
        [SerializeField] private int minimumRapport;
        [SerializeField] private int minimumTrust;
        [SerializeField] private int minimumRespect;

        public string StockId => stockId;
        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? item != null
                    ? item.DisplayName
                    : name
                : displayName;
        public string Description => description;
        public ItemDefinition Item => item;
        public int MaximumStock => maximumStock;
        public int BaseUnitPrice => baseUnitPrice;
        public int MinimumSuccessfulPurchases =>
            minimumSuccessfulPurchases;
        public int MinimumRapport => minimumRapport;
        public int MinimumTrust => minimumTrust;
        public int MinimumRespect => minimumRespect;

        public void Configure(
            string stableStockId,
            string readableName,
            string readableDescription,
            ItemDefinition stockedItem,
            int maxStock,
            int unitPrice,
            int requiredPurchases,
            int requiredRapport,
            int requiredTrust,
            int requiredRespect
        )
        {
            stockId = stableStockId;
            displayName = readableName;
            description = readableDescription;
            item = stockedItem;
            maximumStock = Mathf.Max(1, maxStock);
            baseUnitPrice = Mathf.Max(1, unitPrice);
            minimumSuccessfulPurchases =
                Mathf.Max(0, requiredPurchases);
            minimumRapport = requiredRapport;
            minimumTrust = requiredTrust;
            minimumRespect = requiredRespect;
        }

        public bool RequirementsMet(
            int successfulPurchases,
            int rapport,
            int trust,
            int respect
        )
        {
            return
                successfulPurchases >=
                minimumSuccessfulPurchases &&
                rapport >= minimumRapport &&
                trust >= minimumTrust &&
                respect >= minimumRespect;
        }

        private void OnValidate()
        {
            maximumStock = Mathf.Max(1, maximumStock);
            baseUnitPrice = Mathf.Max(1, baseUnitPrice);
            minimumSuccessfulPurchases =
                Mathf.Max(
                    0,
                    minimumSuccessfulPurchases
                );
        }
    }
}
