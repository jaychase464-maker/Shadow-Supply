using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Relationships
{
    [CreateAssetMenu(
        fileName = "ORDER_NewBuyerOrder",
        menuName = "Shadow Supply/Relationships/Buyer Order"
    )]
    public sealed class BuyerOrderDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string orderId;
        [SerializeField] private string displayName = "New Buyer Order";
        [SerializeField, TextArea(2, 6)] private string description;

        [Header("Product")]
        [SerializeField] private ItemDefinition requestedItem;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField] private ItemQuality minimumQuality =
            ItemQuality.Standard;

        [Header("Terms")]
        [SerializeField, Min(10f)] private float deadlineSeconds = 480f;
        [SerializeField, Min(0)] private int baseDirtyCashReward = 240;
        [SerializeField, Min(0)] private int qualityBonusPerTier = 30;

        [Header("Unlock Requirements")]
        [SerializeField, Min(0)] private int minimumSuccessfulOrders;
        [SerializeField] private int minimumRapport;
        [SerializeField] private int minimumTrust;
        [SerializeField] private int minimumRespect;

        [Header("Success Relationship Change")]
        [SerializeField] private int successRapport = 4;
        [SerializeField] private int successTrust = 8;
        [SerializeField] private int successRespect = 6;

        [Header("Failure Relationship Change")]
        [SerializeField] private int failureRapport = -2;
        [SerializeField] private int failureTrust = -7;
        [SerializeField] private int failureRespect = -3;

        public string OrderId => orderId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemDefinition RequestedItem => requestedItem;
        public int Quantity => quantity;
        public ItemQuality MinimumQuality => minimumQuality;
        public float DeadlineSeconds => deadlineSeconds;
        public int BaseDirtyCashReward => baseDirtyCashReward;
        public int QualityBonusPerTier => qualityBonusPerTier;
        public int MinimumSuccessfulOrders => minimumSuccessfulOrders;
        public int MinimumRapport => minimumRapport;
        public int MinimumTrust => minimumTrust;
        public int MinimumRespect => minimumRespect;
        public int SuccessRapport => successRapport;
        public int SuccessTrust => successTrust;
        public int SuccessRespect => successRespect;
        public int FailureRapport => failureRapport;
        public int FailureTrust => failureTrust;
        public int FailureRespect => failureRespect;

        public void Configure(
            string stableOrderId,
            string readableName,
            string readableDescription,
            ItemDefinition item,
            int requestedQuantity,
            ItemQuality requiredQuality,
            float deadline,
            int baseReward,
            int qualityBonus,
            int requiredSuccesses,
            int requiredRapport,
            int requiredTrust,
            int requiredRespect,
            int rapportSuccess,
            int trustSuccess,
            int respectSuccess,
            int rapportFailure,
            int trustFailure,
            int respectFailure
        )
        {
            orderId = stableOrderId;
            displayName = readableName;
            description = readableDescription;
            requestedItem = item;
            quantity = Mathf.Max(1, requestedQuantity);
            minimumQuality = requiredQuality;
            deadlineSeconds = Mathf.Max(10f, deadline);
            baseDirtyCashReward = Mathf.Max(0, baseReward);
            qualityBonusPerTier = Mathf.Max(0, qualityBonus);
            minimumSuccessfulOrders = Mathf.Max(0, requiredSuccesses);
            minimumRapport = requiredRapport;
            minimumTrust = requiredTrust;
            minimumRespect = requiredRespect;
            successRapport = rapportSuccess;
            successTrust = trustSuccess;
            successRespect = respectSuccess;
            failureRapport = rapportFailure;
            failureTrust = trustFailure;
            failureRespect = respectFailure;
        }

        public bool RequirementsMet(
            int successfulOrders,
            int rapport,
            int trust,
            int respect
        )
        {
            return
                successfulOrders >= minimumSuccessfulOrders &&
                rapport >= minimumRapport &&
                trust >= minimumTrust &&
                respect >= minimumRespect;
        }

        public int CalculateReward(float averageQuality)
        {
            int bonusTiers = Mathf.Max(
                0,
                Mathf.FloorToInt(averageQuality) - (int)minimumQuality
            );

            return
                baseDirtyCashReward +
                bonusTiers * qualityBonusPerTier;
        }

        private void OnValidate()
        {
            quantity = Mathf.Max(1, quantity);
            deadlineSeconds = Mathf.Max(10f, deadlineSeconds);
            baseDirtyCashReward = Mathf.Max(0, baseDirtyCashReward);
            qualityBonusPerTier = Mathf.Max(0, qualityBonusPerTier);
        }
    }
}
