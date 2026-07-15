using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Relationships
{
    [CreateAssetMenu(
        fileName = "BUYER_NewBuyer",
        menuName = "Shadow Supply/Relationships/Buyer Definition"
    )]
    public sealed class BuyerDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string buyerId;
        [SerializeField] private string displayName = "New Buyer";
        [SerializeField] private string districtId = "blackwater-south";
        [SerializeField] private string districtName = "Blackwater South";
        [SerializeField, TextArea(3, 8)] private string biography;

        [Header("Initial Relationship")]
        [SerializeField] private int initialRapport;
        [SerializeField] private int initialTrust;
        [SerializeField] private int initialRespect;

        [Header("Orders")]
        [SerializeField] private List<BuyerOrderDefinition> orders =
            new List<BuyerOrderDefinition>();
        [SerializeField, Min(0f)] private float repeatOrderCooldown = 25f;

        [Header("Referral")]
        [SerializeField] private string referralId =
            "contact-print-supply-broker";
        [SerializeField] private string referralDisplayName =
            "Print-Supply Broker";
        [SerializeField, Min(0)] private int referralSuccessfulOrders = 3;
        [SerializeField] private int referralTrust = 24;
        [SerializeField] private int referralRespect = 18;

        public string BuyerId => buyerId;
        public string DisplayName => displayName;
        public string DistrictId => districtId;
        public string DistrictName => districtName;
        public string Biography => biography;
        public int InitialRapport => initialRapport;
        public int InitialTrust => initialTrust;
        public int InitialRespect => initialRespect;
        public IReadOnlyList<BuyerOrderDefinition> Orders => orders;
        public float RepeatOrderCooldown => repeatOrderCooldown;
        public string ReferralId => referralId;
        public string ReferralDisplayName => referralDisplayName;
        public int ReferralSuccessfulOrders => referralSuccessfulOrders;
        public int ReferralTrust => referralTrust;
        public int ReferralRespect => referralRespect;

        public void Configure(
            string stableBuyerId,
            string readableName,
            string stableDistrictId,
            string readableDistrict,
            string readableBiography,
            int startingRapport,
            int startingTrust,
            int startingRespect,
            IEnumerable<BuyerOrderDefinition> buyerOrders,
            float cooldown,
            string unlockedReferralId,
            string unlockedReferralName,
            int successesForReferral,
            int trustForReferral,
            int respectForReferral
        )
        {
            buyerId = stableBuyerId;
            displayName = readableName;
            districtId = stableDistrictId;
            districtName = readableDistrict;
            biography = readableBiography;
            initialRapport = startingRapport;
            initialTrust = startingTrust;
            initialRespect = startingRespect;
            orders = buyerOrders != null
                ? new List<BuyerOrderDefinition>(buyerOrders)
                : new List<BuyerOrderDefinition>();
            repeatOrderCooldown = Mathf.Max(0f, cooldown);
            referralId = unlockedReferralId;
            referralDisplayName = unlockedReferralName;
            referralSuccessfulOrders = Mathf.Max(0, successesForReferral);
            referralTrust = trustForReferral;
            referralRespect = respectForReferral;
        }
    }
}
