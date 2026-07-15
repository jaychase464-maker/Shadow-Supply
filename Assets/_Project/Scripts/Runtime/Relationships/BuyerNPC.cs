using System;
using ShadowSupply.Economy;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Relationships
{
    public enum BuyerOrderState
    {
        None,
        Active,
        Completed,
        Failed,
        Declined
    }

    [DisallowMultipleComponent]
    public sealed class BuyerNPC : MonoBehaviour, IInteractable
    {
        [Header("Definition")]
        [SerializeField] private BuyerDefinition definition;

        [Header("Player References")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private HotbarController hotbar;
        [SerializeField] private PlayerWallet wallet;

        [Header("Relationship State")]
        [SerializeField] private bool initialized;
        [SerializeField] private bool introductionCompleted;
        [SerializeField] private int introductionChoice = -1;
        [SerializeField] private int rapport;
        [SerializeField] private int trust;
        [SerializeField] private int respect;
        [SerializeField] private int successfulOrders;
        [SerializeField] private int failedOrders;
        [SerializeField] private int declinedOrders;
        [SerializeField] private bool referralUnlocked;

        [Header("Order State")]
        [SerializeField] private BuyerOrderState orderState;
        [SerializeField] private BuyerOrderDefinition activeOrder;
        [SerializeField] private int deliveredQuantity;
        [SerializeField] private float deliveredQualityTotal;
        [SerializeField] private float deliveredConditionTotal;
        [SerializeField] private float remainingDeadlineSeconds;
        [SerializeField] private float cooldownRemainingSeconds;
        [SerializeField] private int lastReward;

        private string statusMessage;
        private float statusUntil;

        public event Action StateChanged;

        public BuyerDefinition Definition => definition;
        public string BuyerId =>
            definition != null ? definition.BuyerId : string.Empty;
        public string DisplayName =>
            definition != null ? definition.DisplayName : name;
        public string DistrictName =>
            definition != null ? definition.DistrictName : "Unknown";
        public bool IntroductionCompleted => introductionCompleted;
        public int IntroductionChoice => introductionChoice;
        public int Rapport => rapport;
        public int Trust => trust;
        public int Respect => respect;
        public int SuccessfulOrders => successfulOrders;
        public int FailedOrders => failedOrders;
        public int DeclinedOrders => declinedOrders;
        public bool ReferralUnlocked => referralUnlocked;
        public BuyerOrderState OrderState => orderState;
        public BuyerOrderDefinition ActiveOrder => activeOrder;
        public int DeliveredQuantity => deliveredQuantity;
        public float DeliveredQualityTotal => deliveredQualityTotal;
        public float DeliveredConditionTotal => deliveredConditionTotal;
        public float RemainingDeadlineSeconds => remainingDeadlineSeconds;
        public float CooldownRemainingSeconds => cooldownRemainingSeconds;
        public int LastReward => lastReward;
        public PlayerInventory Inventory => inventory;
        public HotbarController Hotbar => hotbar;
        public PlayerWallet Wallet => wallet;
        public string StatusMessage =>
            Time.unscaledTime <= statusUntil ? statusMessage : string.Empty;

        public string InteractionPrompt
        {
            get
            {
                if (
                    orderState == BuyerOrderState.Active &&
                    IsCorrectHeldProduct()
                )
                {
                    return $"Talk to {DisplayName} — delivery ready";
                }

                BuyerOrderDefinition available = GetAvailableOrder();

                if (available != null)
                {
                    return $"Talk to {DisplayName} — order available";
                }

                return $"Talk to {DisplayName}";
            }
        }

        private void Awake()
        {
            ResolvePlayerReferences();
            EnsureInitialized();
        }

        private void Update()
        {
            if (orderState == BuyerOrderState.Active)
            {
                remainingDeadlineSeconds -= Time.deltaTime;

                if (remainingDeadlineSeconds <= 0f)
                {
                    FailActiveOrder("The deadline expired.");
                }

                return;
            }

            if (cooldownRemainingSeconds <= 0f)
            {
                return;
            }

            cooldownRemainingSeconds -= Time.deltaTime;

            if (cooldownRemainingSeconds <= 0f)
            {
                cooldownRemainingSeconds = 0f;
                orderState = BuyerOrderState.None;
                activeOrder = null;
                lastReward = 0;
                StateChanged?.Invoke();
            }
        }

        public void Configure(
            BuyerDefinition buyerDefinition,
            PlayerInventory playerInventory,
            HotbarController hotbarController,
            PlayerWallet playerWallet
        )
        {
            definition = buyerDefinition;
            inventory = playerInventory;
            hotbar = hotbarController;
            wallet = playerWallet;
            initialized = false;
            EnsureInitialized();
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                enabled &&
                gameObject.activeInHierarchy &&
                definition != null;
        }

        public void Interact(GameObject interactor)
        {
            ResolvePlayerReferences();

            BuyerRelationshipHUD hud = BuyerRelationshipHUD.Instance;

            if (hud == null)
            {
                PublishStatus("Buyer interface is unavailable.", 3f);
                return;
            }

            hud.Open(this, interactor);
        }

        public void CompleteIntroductionChoice(int choice)
        {
            if (introductionCompleted)
            {
                return;
            }

            introductionChoice = Mathf.Clamp(choice, 0, 2);
            introductionCompleted = true;

            switch (introductionChoice)
            {
                case 0:
                    ApplyRelationshipChange(
                        1, 2, 2,
                        "You kept the first conversation professional."
                    );
                    break;

                case 1:
                    ApplyRelationshipChange(
                        3, 1, 0,
                        "You showed interest in the neighborhood."
                    );
                    break;

                default:
                    ApplyRelationshipChange(
                        -1, 1, 3,
                        "You made a confident first impression."
                    );
                    break;
            }

            PublishStatus(
                $"{DisplayName} is willing to test your work.",
                5f
            );
        }

        public BuyerOrderDefinition GetAvailableOrder()
        {
            if (
                definition == null ||
                !introductionCompleted ||
                orderState == BuyerOrderState.Active ||
                cooldownRemainingSeconds > 0f
            )
            {
                return null;
            }

            BuyerOrderDefinition selected = null;

            foreach (BuyerOrderDefinition order in definition.Orders)
            {
                if (
                    order != null &&
                    order.RequirementsMet(
                        successfulOrders,
                        rapport,
                        trust,
                        respect
                    )
                )
                {
                    selected = order;
                }
            }

            return selected;
        }

        public bool AcceptAvailableOrder(out string reason)
        {
            BuyerOrderDefinition order = GetAvailableOrder();

            if (order == null)
            {
                reason = "No order is currently available.";
                return false;
            }

            activeOrder = order;
            orderState = BuyerOrderState.Active;
            deliveredQuantity = 0;
            deliveredQualityTotal = 0f;
            deliveredConditionTotal = 0f;
            remainingDeadlineSeconds = order.DeadlineSeconds;
            cooldownRemainingSeconds = 0f;
            lastReward = 0;

            reason = string.Empty;
            PublishStatus($"Accepted: {order.DisplayName}.", 4f);
            StateChanged?.Invoke();
            return true;
        }

        public bool DeclineAvailableOrder(out string reason)
        {
            BuyerOrderDefinition order = GetAvailableOrder();

            if (order == null)
            {
                reason = "No order is currently available.";
                return false;
            }

            activeOrder = order;
            orderState = BuyerOrderState.Declined;
            declinedOrders++;
            cooldownRemainingSeconds =
                definition != null
                    ? Mathf.Max(10f, definition.RepeatOrderCooldown * 0.6f)
                    : 15f;

            ApplyRelationshipChange(
                0, -1, 0,
                "You declined offered work."
            );

            reason = string.Empty;
            PublishStatus($"Declined: {order.DisplayName}.", 4f);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryHandOverHeldItem(out string reason)
        {
            if (
                orderState != BuyerOrderState.Active ||
                activeOrder == null
            )
            {
                reason = "There is no active order.";
                return false;
            }

            if (
                inventory == null ||
                hotbar == null ||
                hotbar.SelectedSlot == null ||
                hotbar.SelectedSlot.IsEmpty
            )
            {
                reason = "Hold the requested product in the hotbar.";
                return false;
            }

            ItemStack held = hotbar.SelectedSlot.Stack;

            if (held.Item != activeOrder.RequestedItem)
            {
                reason =
                    $"Hold {activeOrder.RequestedItem.DisplayName}.";
                return false;
            }

            if ((int)held.Quality < (int)activeOrder.MinimumQuality)
            {
                reason =
                    $"{DisplayName} requires " +
                    $"{activeOrder.MinimumQuality} quality or better.";
                return false;
            }

            ItemStack removed =
                inventory.RemoveFromSlot(hotbar.SelectedIndex, 1);

            if (removed == null || removed.IsEmpty)
            {
                reason = "The held product could not be transferred.";
                return false;
            }

            deliveredQuantity++;
            deliveredQualityTotal += (int)removed.Quality;
            deliveredConditionTotal += removed.Condition;

            PublishStatus(
                $"Handed over 1× {removed.Item.DisplayName} " +
                $"({deliveredQuantity}/{activeOrder.Quantity}).",
                3f
            );

            if (deliveredQuantity >= activeOrder.Quantity)
            {
                CompleteActiveOrder();
            }

            reason = string.Empty;
            StateChanged?.Invoke();
            return true;
        }

        public bool CancelActiveOrder(out string reason)
        {
            if (
                orderState != BuyerOrderState.Active ||
                activeOrder == null
            )
            {
                reason = "There is no active order.";
                return false;
            }

            ApplyRelationshipChange(
                -1, -4, -1,
                "You backed out after accepting the order."
            );

            failedOrders++;
            orderState = BuyerOrderState.Failed;
            remainingDeadlineSeconds = 0f;
            cooldownRemainingSeconds =
                definition != null
                    ? Mathf.Max(20f, definition.RepeatOrderCooldown)
                    : 25f;

            reason = string.Empty;
            PublishStatus(
                "Order cancelled. Delivered items were not returned.",
                5f
            );
            StateChanged?.Invoke();
            return true;
        }

        public void ResetToDefaultState()
        {
            initialized = false;
            EnsureInitialized();
            StateChanged?.Invoke();
        }

        public void RestoreState(
            bool restoredIntroduction,
            int restoredIntroductionChoice,
            int restoredRapport,
            int restoredTrust,
            int restoredRespect,
            int restoredSuccesses,
            int restoredFailures,
            int restoredDeclines,
            bool restoredReferral,
            BuyerOrderState restoredOrderState,
            string restoredOrderId,
            int restoredDeliveredQuantity,
            float restoredQualityTotal,
            float restoredConditionTotal,
            float restoredDeadline,
            float restoredCooldown,
            int restoredReward
        )
        {
            EnsureInitialized();

            introductionCompleted = restoredIntroduction;
            introductionChoice = restoredIntroductionChoice;
            rapport = Mathf.Clamp(restoredRapport, -100, 100);
            trust = Mathf.Clamp(restoredTrust, -100, 100);
            respect = Mathf.Clamp(restoredRespect, -100, 100);
            successfulOrders = Mathf.Max(0, restoredSuccesses);
            failedOrders = Mathf.Max(0, restoredFailures);
            declinedOrders = Mathf.Max(0, restoredDeclines);
            referralUnlocked = restoredReferral;
            orderState = restoredOrderState;
            deliveredQuantity = Mathf.Max(0, restoredDeliveredQuantity);
            deliveredQualityTotal = Mathf.Max(0f, restoredQualityTotal);
            deliveredConditionTotal = Mathf.Max(0f, restoredConditionTotal);
            remainingDeadlineSeconds = Mathf.Max(0f, restoredDeadline);
            cooldownRemainingSeconds = Mathf.Max(0f, restoredCooldown);
            lastReward = Mathf.Max(0, restoredReward);
            activeOrder = FindOrderById(restoredOrderId);

            if (
                orderState == BuyerOrderState.Active &&
                activeOrder == null
            )
            {
                orderState = BuyerOrderState.None;
                deliveredQuantity = 0;
                deliveredQualityTotal = 0f;
                deliveredConditionTotal = 0f;
                remainingDeadlineSeconds = 0f;
            }

            StateChanged?.Invoke();
        }

        public string GetRelationshipState()
        {
            float average = (rapport + trust + respect) / 3f;

            if (average < 0f) return "Suspicious";
            if (average < 6f) return "Acquaintance";
            if (average < 15f) return "Familiar";
            if (average < 28f) return "Trusted Contact";
            if (average < 45f) return "Close Ally";
            return "Loyal Partner";
        }

        public string GetRapportState()
        {
            if (rapport < 0) return "Guarded";
            if (rapport < 6) return "Reserved";
            if (rapport < 15) return "Comfortable";
            if (rapport < 30) return "Friendly";
            return "Personally Close";
        }

        public string GetTrustState()
        {
            if (trust < 0) return "Distrustful";
            if (trust < 7) return "Testing You";
            if (trust < 18) return "Cautiously Reliable";
            if (trust < 32) return "Trusted";
            return "Fully Dependable";
        }

        public string GetRespectState()
        {
            if (respect < 0) return "Dismissive";
            if (respect < 7) return "Unproven";
            if (respect < 18) return "Capable";
            if (respect < 32) return "Respected";
            return "Formidable";
        }

        public string GetReferralState()
        {
            if (definition == null)
            {
                return "No known referral";
            }

            if (referralUnlocked)
            {
                return $"Available: {definition.ReferralDisplayName}";
            }

            return
                $"{definition.ReferralDisplayName} — " +
                "earn more trust and respect";
        }

        public string FormatDeadline()
        {
            int total = Mathf.Max(
                0,
                Mathf.CeilToInt(remainingDeadlineSeconds)
            );

            return $"{total / 60:00}:{total % 60:00}";
        }

        private void CompleteActiveOrder()
        {
            if (activeOrder == null)
            {
                return;
            }

            float averageQuality =
                deliveredQuantity > 0
                    ? deliveredQualityTotal / deliveredQuantity
                    : 0f;

            float averageCondition =
                deliveredQuantity > 0
                    ? deliveredConditionTotal / deliveredQuantity
                    : 0f;

            lastReward = activeOrder.CalculateReward(averageQuality);
            wallet?.AddDirtyCash(lastReward);

            ApplyRelationshipChange(
                activeOrder.SuccessRapport,
                activeOrder.SuccessTrust,
                activeOrder.SuccessRespect +
                (averageCondition >= 0.95f ? 1 : 0),
                "You completed the order as agreed."
            );

            successfulOrders++;
            orderState = BuyerOrderState.Completed;
            remainingDeadlineSeconds = 0f;
            cooldownRemainingSeconds =
                definition != null
                    ? definition.RepeatOrderCooldown
                    : 25f;

            TryUnlockReferral();

            PublishStatus(
                $"Order complete. Received ${lastReward} dirty cash.",
                6f
            );
            StateChanged?.Invoke();
        }

        private void FailActiveOrder(string reason)
        {
            if (
                activeOrder == null ||
                orderState != BuyerOrderState.Active
            )
            {
                return;
            }

            ApplyRelationshipChange(
                activeOrder.FailureRapport,
                activeOrder.FailureTrust,
                activeOrder.FailureRespect,
                "You failed an accepted order."
            );

            failedOrders++;
            orderState = BuyerOrderState.Failed;
            remainingDeadlineSeconds = 0f;
            cooldownRemainingSeconds =
                definition != null
                    ? Mathf.Max(
                        30f,
                        definition.RepeatOrderCooldown * 1.4f
                    )
                    : 40f;

            PublishStatus(
                $"{reason} Delivered items were not returned.",
                6f
            );
            StateChanged?.Invoke();
        }

        private void TryUnlockReferral()
        {
            if (referralUnlocked || definition == null)
            {
                return;
            }

            if (
                successfulOrders >= definition.ReferralSuccessfulOrders &&
                trust >= definition.ReferralTrust &&
                respect >= definition.ReferralRespect
            )
            {
                referralUnlocked = true;
                PublishStatus(
                    $"{DisplayName} is ready to introduce you to " +
                    $"{definition.ReferralDisplayName}.",
                    8f
                );
            }
        }

        private BuyerOrderDefinition FindOrderById(string orderId)
        {
            if (
                definition == null ||
                string.IsNullOrWhiteSpace(orderId)
            )
            {
                return null;
            }

            foreach (BuyerOrderDefinition order in definition.Orders)
            {
                if (
                    order != null &&
                    string.Equals(
                        order.OrderId,
                        orderId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return order;
                }
            }

            return null;
        }

        private bool IsCorrectHeldProduct()
        {
            return
                orderState == BuyerOrderState.Active &&
                activeOrder != null &&
                hotbar != null &&
                hotbar.SelectedSlot != null &&
                !hotbar.SelectedSlot.IsEmpty &&
                hotbar.SelectedSlot.Stack.Item ==
                activeOrder.RequestedItem &&
                (int)hotbar.SelectedSlot.Stack.Quality >=
                (int)activeOrder.MinimumQuality;
        }

        private void ResolvePlayerReferences()
        {
            inventory ??= FindFirstObjectByType<PlayerInventory>();
            hotbar ??= FindFirstObjectByType<HotbarController>();
            wallet ??= FindFirstObjectByType<PlayerWallet>();
        }

        private void EnsureInitialized()
        {
            if (initialized || definition == null)
            {
                return;
            }

            rapport = definition.InitialRapport;
            trust = definition.InitialTrust;
            respect = definition.InitialRespect;
            introductionCompleted = false;
            introductionChoice = -1;
            successfulOrders = 0;
            failedOrders = 0;
            declinedOrders = 0;
            referralUnlocked = false;
            orderState = BuyerOrderState.None;
            activeOrder = null;
            deliveredQuantity = 0;
            deliveredQualityTotal = 0f;
            deliveredConditionTotal = 0f;
            remainingDeadlineSeconds = 0f;
            cooldownRemainingSeconds = 0f;
            lastReward = 0;
            initialized = true;
        }

        private void ApplyRelationshipChange(
            int rapportChange,
            int trustChange,
            int respectChange,
            string context
        )
        {
            rapport = Mathf.Clamp(rapport + rapportChange, -100, 100);
            trust = Mathf.Clamp(trust + trustChange, -100, 100);
            respect = Mathf.Clamp(respect + respectChange, -100, 100);

            Debug.Log(
                $"[BuyerRelationship] {DisplayName}: " +
                $"Rapport {Signed(rapportChange)}, " +
                $"Trust {Signed(trustChange)}, " +
                $"Respect {Signed(respectChange)}. {context}",
                this
            );

            StateChanged?.Invoke();
        }

        private void PublishStatus(string message, float duration)
        {
            statusMessage = message;
            statusUntil =
                Time.unscaledTime + Mathf.Max(0.1f, duration);

            Debug.Log($"[Buyer] {DisplayName}: {message}", this);
            StateChanged?.Invoke();
        }

        private static string Signed(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }
    }
}
