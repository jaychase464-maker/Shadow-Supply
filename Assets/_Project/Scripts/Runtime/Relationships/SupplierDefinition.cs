using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Relationships
{
    [CreateAssetMenu(
        fileName = "SUPPLIER_NewSupplier",
        menuName =
            "Shadow Supply/Relationships/Supplier Definition"
    )]
    public sealed class SupplierDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string supplierId;
        [SerializeField] private string displayName =
            "New Supplier";
        [SerializeField] private string districtId =
            "blackwater-south";
        [SerializeField] private string districtName =
            "Blackwater South";
        [SerializeField, TextArea(3, 8)]
        private string biography;

        [Header("Referral Gate")]
        [SerializeField] private string requiredBuyerId =
            "buyer-mara-voss";
        [SerializeField] private string requiredReferralName =
            "Mara Voss referral";

        [Header("Initial Relationship")]
        [SerializeField] private int initialRapport;
        [SerializeField] private int initialTrust;
        [SerializeField] private int initialRespect;

        [Header("Stock")]
        [SerializeField]
        private List<SupplierStockDefinition> stock =
            new List<SupplierStockDefinition>();
        [SerializeField, Min(10f)]
        private float restockSeconds = 120f;
        [SerializeField, Min(0.1f)]
        private float deliveryDelaySeconds = 8f;

        public string SupplierId => supplierId;
        public string DisplayName => displayName;
        public string DistrictId => districtId;
        public string DistrictName => districtName;
        public string Biography => biography;
        public string RequiredBuyerId => requiredBuyerId;
        public string RequiredReferralName =>
            requiredReferralName;
        public int InitialRapport => initialRapport;
        public int InitialTrust => initialTrust;
        public int InitialRespect => initialRespect;
        public IReadOnlyList<SupplierStockDefinition> Stock =>
            stock;
        public float RestockSeconds => restockSeconds;
        public float DeliveryDelaySeconds =>
            deliveryDelaySeconds;

        public void Configure(
            string stableSupplierId,
            string readableName,
            string stableDistrictId,
            string readableDistrict,
            string readableBiography,
            string referralBuyerId,
            string readableReferralRequirement,
            int startingRapport,
            int startingTrust,
            int startingRespect,
            IEnumerable<SupplierStockDefinition>
                stockDefinitions,
            float restockDelay,
            float deliveryDelay
        )
        {
            supplierId = stableSupplierId;
            displayName = readableName;
            districtId = stableDistrictId;
            districtName = readableDistrict;
            biography = readableBiography;
            requiredBuyerId = referralBuyerId;
            requiredReferralName =
                readableReferralRequirement;
            initialRapport = startingRapport;
            initialTrust = startingTrust;
            initialRespect = startingRespect;
            stock =
                stockDefinitions != null
                    ? new List<SupplierStockDefinition>(
                        stockDefinitions
                    )
                    : new List<SupplierStockDefinition>();
            restockSeconds =
                Mathf.Max(10f, restockDelay);
            deliveryDelaySeconds =
                Mathf.Max(0.1f, deliveryDelay);
        }

        private void OnValidate()
        {
            restockSeconds =
                Mathf.Max(10f, restockSeconds);
            deliveryDelaySeconds =
                Mathf.Max(
                    0.1f,
                    deliveryDelaySeconds
                );
        }
    }
}
