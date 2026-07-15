using System;
using System.Collections.Generic;
using ShadowSupply.Delivery;
using ShadowSupply.Economy;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using ShadowSupply.SaveSystem;
using UnityEngine;

namespace ShadowSupply.Relationships
{
    [Serializable]
    public sealed class SupplierStockRuntimeState
    {
        [SerializeField] private SupplierStockDefinition definition;
        [SerializeField] private int currentStock;

        public SupplierStockDefinition Definition =>
            definition;
        public int CurrentStock => currentStock;

        public SupplierStockRuntimeState(
            SupplierStockDefinition stockDefinition,
            int quantity
        )
        {
            definition = stockDefinition;
            currentStock = Mathf.Max(0, quantity);
        }

        public void SetStock(int quantity)
        {
            currentStock = Mathf.Max(0, quantity);
        }

        public bool TryRemove(int quantity)
        {
            if (
                quantity <= 0 ||
                currentStock < quantity
            )
            {
                return false;
            }

            currentStock -= quantity;
            return true;
        }

        public void Add(int quantity)
        {
            currentStock =
                Mathf.Max(
                    0,
                    currentStock + quantity
                );
        }
    }

    [DisallowMultipleComponent]
    public sealed class SupplierNPC :
        MonoBehaviour,
        IInteractable
    {
        [Header("Definition")]
        [SerializeField]
        private SupplierDefinition definition;

        [Header("Player and Delivery")]
        [SerializeField]
        private PlayerWallet wallet;
        [SerializeField]
        private FurnitureDeliverySystem deliverySystem;

        [Header("Relationship")]
        [SerializeField] private bool initialized;
        [SerializeField]
        private bool introductionCompleted;
        [SerializeField]
        private int introductionChoice = -1;
        [SerializeField] private int rapport;
        [SerializeField] private int trust;
        [SerializeField] private int respect;
        [SerializeField]
        private int successfulPurchases;
        [SerializeField]
        private int lifetimeCleanCashSpent;

        [Header("Stock")]
        [SerializeField]
        private List<SupplierStockRuntimeState>
            runtimeStock =
                new List<SupplierStockRuntimeState>();
        [SerializeField]
        private float restockRemainingSeconds;

        [Header("Pending Delivery")]
        [SerializeField]
        private SupplierStockDefinition pendingStock;
        [SerializeField]
        private int pendingQuantity;
        [SerializeField]
        private int pendingTotalPrice;
        [SerializeField]
        private float pendingDeliverySeconds;

        private bool developmentReferralOverride;
        private BuyerNPC referralBuyer;
        private float deliveryRetryDelay;
        private string statusMessage;
        private float statusUntil;

        public event Action StateChanged;

        public SupplierDefinition Definition =>
            definition;
        public string SupplierId =>
            definition != null
                ? definition.SupplierId
                : string.Empty;
        public string DisplayName =>
            definition != null
                ? definition.DisplayName
                : name;
        public string DistrictName =>
            definition != null
                ? definition.DistrictName
                : "Unknown District";
        public bool IntroductionCompleted =>
            introductionCompleted;
        public int IntroductionChoice =>
            introductionChoice;
        public int Rapport => rapport;
        public int Trust => trust;
        public int Respect => respect;
        public int SuccessfulPurchases =>
            successfulPurchases;
        public int LifetimeCleanCashSpent =>
            lifetimeCleanCashSpent;
        public IReadOnlyList<
            SupplierStockRuntimeState
        > RuntimeStock => runtimeStock;
        public float RestockRemainingSeconds =>
            restockRemainingSeconds;
        public bool HasPendingDelivery =>
            pendingStock != null &&
            pendingQuantity > 0;
        public SupplierStockDefinition PendingStock =>
            pendingStock;
        public int PendingQuantity =>
            pendingQuantity;
        public int PendingTotalPrice =>
            pendingTotalPrice;
        public float PendingDeliverySeconds =>
            pendingDeliverySeconds;
        public PlayerWallet Wallet => wallet;
        public FurnitureDeliverySystem
            DeliverySystem => deliverySystem;
        public string StatusMessage =>
            Time.unscaledTime <= statusUntil
                ? statusMessage
                : string.Empty;

        public bool ReferralUnlocked
        {
            get
            {
                if (developmentReferralOverride)
                {
                    return true;
                }

                if (
                    definition == null ||
                    string.IsNullOrWhiteSpace(
                        definition.RequiredBuyerId
                    )
                )
                {
                    return true;
                }

                ResolveReferralBuyer();

                return
                    referralBuyer != null &&
                    referralBuyer.ReferralUnlocked;
            }
        }

        public string InteractionPrompt
        {
            get
            {
                if (!ReferralUnlocked)
                {
                    return
                        "Unknown broker — referral required";
                }

                if (!introductionCompleted)
                {
                    return
                        $"Meet {DisplayName}";
                }

                if (HasPendingDelivery)
                {
                    return
                        $"Talk to {DisplayName} — " +
                        "delivery pending";
                }

                return
                    $"Talk to {DisplayName} — materials";
            }
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureInitialized();
        }

        private void Update()
        {
            UpdateRestock();
            UpdatePendingDelivery();
        }

        public void Configure(
            SupplierDefinition supplierDefinition,
            PlayerWallet playerWallet,
            FurnitureDeliverySystem
                furnitureDeliverySystem
        )
        {
            definition = supplierDefinition;
            wallet = playerWallet;
            deliverySystem =
                furnitureDeliverySystem;
            referralBuyer = null;
            initialized = false;
            EnsureInitialized();
        }

        public bool CanInteract(
            GameObject interactor
        )
        {
            return
                enabled &&
                gameObject.activeInHierarchy &&
                definition != null;
        }

        public void Interact(
            GameObject interactor
        )
        {
            ResolveReferences();

            SupplierRelationshipHUD hud =
                SupplierRelationshipHUD.Instance;

            if (hud == null)
            {
                PublishStatus(
                    "Supplier interface is unavailable.",
                    3f
                );
                return;
            }

            hud.Open(
                this,
                interactor
            );
        }

        public void CompleteIntroductionChoice(
            int choice
        )
        {
            if (
                introductionCompleted ||
                !ReferralUnlocked
            )
            {
                return;
            }

            introductionChoice =
                Mathf.Clamp(choice, 0, 2);
            introductionCompleted = true;

            switch (introductionChoice)
            {
                case 0:
                    ApplyRelationshipChange(
                        1,
                        3,
                        1,
                        "You led with Mara's name " +
                        "and kept the introduction clean."
                    );
                    break;

                case 1:
                    ApplyRelationshipChange(
                        0,
                        2,
                        3,
                        "You made it clear that " +
                        "you pay on time."
                    );
                    break;

                default:
                    ApplyRelationshipChange(
                        -1,
                        1,
                        4,
                        "You immediately asked for volume."
                    );
                    break;
            }

            PublishStatus(
                $"{DisplayName} opened a small catalog.",
                5f
            );
        }

        public bool IsStockUnlocked(
            SupplierStockDefinition stock
        )
        {
            return
                stock != null &&
                introductionCompleted &&
                stock.RequirementsMet(
                    successfulPurchases,
                    rapport,
                    trust,
                    respect
                );
        }

        public string GetStockLockReason(
            SupplierStockDefinition stock
        )
        {
            if (stock == null)
            {
                return "Invalid stock entry.";
            }

            if (!ReferralUnlocked)
            {
                return
                    definition != null
                        ? definition.RequiredReferralName
                        : "Referral required";
            }

            if (!introductionCompleted)
            {
                return
                    "Complete the introduction first.";
            }

            List<string> requirements =
                new List<string>();

            if (
                successfulPurchases <
                stock.MinimumSuccessfulPurchases
            )
            {
                requirements.Add(
                    $"{stock.MinimumSuccessfulPurchases} " +
                    "successful purchases"
                );
            }

            if (
                rapport <
                stock.MinimumRapport
            )
            {
                requirements.Add(
                    $"rapport {stock.MinimumRapport}"
                );
            }

            if (
                trust <
                stock.MinimumTrust
            )
            {
                requirements.Add(
                    $"trust {stock.MinimumTrust}"
                );
            }

            if (
                respect <
                stock.MinimumRespect
            )
            {
                requirements.Add(
                    $"respect {stock.MinimumRespect}"
                );
            }

            return
                requirements.Count > 0
                    ? "Requires " +
                      string.Join(
                          ", ",
                          requirements
                      )
                    : string.Empty;
        }

        public int GetCurrentStock(
            SupplierStockDefinition stock
        )
        {
            SupplierStockRuntimeState state =
                FindRuntimeStock(stock);

            return
                state != null
                    ? state.CurrentStock
                    : 0;
        }

        public int GetUnitPrice(
            SupplierStockDefinition stock
        )
        {
            if (stock == null)
            {
                return 0;
            }

            float multiplier = 1.25f;

            if (trust >= 30)
            {
                multiplier = 0.9f;
            }
            else if (trust >= 18)
            {
                multiplier = 1f;
            }
            else if (trust >= 8)
            {
                multiplier = 1.1f;
            }

            if (respect >= 20)
            {
                multiplier -= 0.05f;
            }

            if (rapport < 0)
            {
                multiplier += 0.1f;
            }

            multiplier =
                Mathf.Clamp(
                    multiplier,
                    0.85f,
                    1.4f
                );

            return
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        stock.BaseUnitPrice *
                        multiplier
                    )
                );
        }

        public bool CanOrder(
            SupplierStockDefinition stock,
            int quantity,
            out string reason
        )
        {
            ResolveReferences();

            if (!ReferralUnlocked)
            {
                reason =
                    definition != null
                        ? definition.RequiredReferralName +
                          " is required."
                        : "A referral is required.";
                return false;
            }

            if (!introductionCompleted)
            {
                reason =
                    "Complete the supplier introduction.";
                return false;
            }

            if (HasPendingDelivery)
            {
                reason =
                    "Wait for the current delivery.";
                return false;
            }

            if (stock == null || stock.Item == null)
            {
                reason =
                    "This stock entry is invalid.";
                return false;
            }

            if (!IsStockUnlocked(stock))
            {
                reason =
                    GetStockLockReason(stock);
                return false;
            }

            if (quantity <= 0)
            {
                reason =
                    "Order quantity must be positive.";
                return false;
            }

            int available =
                GetCurrentStock(stock);

            if (available < quantity)
            {
                reason =
                    $"Only {available} currently available.";
                return false;
            }

            if (wallet == null)
            {
                reason =
                    "The player wallet is unavailable.";
                return false;
            }

            int total =
                GetUnitPrice(stock) *
                quantity;

            if (!wallet.CanAfford(total))
            {
                reason =
                    $"Need ${total:N0} clean cash.";
                return false;
            }

            if (deliverySystem == null)
            {
                reason =
                    "Garage delivery service is unavailable.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool TryOrder(
            SupplierStockDefinition stock,
            int quantity,
            out string reason
        )
        {
            if (
                !CanOrder(
                    stock,
                    quantity,
                    out reason
                )
            )
            {
                PublishStatus(reason, 4f);
                return false;
            }

            SupplierStockRuntimeState state =
                FindRuntimeStock(stock);

            int total =
                GetUnitPrice(stock) *
                quantity;

            if (
                state == null ||
                !state.TryRemove(quantity)
            )
            {
                reason =
                    "Supplier stock changed before " +
                    "the order completed.";
                PublishStatus(reason, 4f);
                return false;
            }

            if (
                !wallet.TrySpendCleanCash(total)
            )
            {
                state.Add(quantity);
                reason =
                    "Clean-cash payment failed.";
                PublishStatus(reason, 4f);
                return false;
            }

            pendingStock = stock;
            pendingQuantity = quantity;
            pendingTotalPrice = total;
            pendingDeliverySeconds =
                definition != null
                    ? definition
                        .DeliveryDelaySeconds
                    : 8f;
            deliveryRetryDelay = 0f;

            reason = string.Empty;

            PublishStatus(
                $"Ordered {quantity}× " +
                $"{stock.Item.DisplayName} " +
                $"for ${total:N0}.",
                5f
            );

            StateChanged?.Invoke();
            return true;
        }

        public string GetRelationshipState()
        {
            float average =
                (
                    rapport +
                    trust +
                    respect
                ) /
                3f;

            if (average < 0f)
            {
                return "Suspicious";
            }

            if (average < 6f)
            {
                return "New Contact";
            }

            if (average < 15f)
            {
                return "Working Contact";
            }

            if (average < 28f)
            {
                return "Trusted Supplier";
            }

            if (average < 45f)
            {
                return "Preferred Broker";
            }

            return "Strategic Partner";
        }

        public string GetRapportState()
        {
            if (rapport < 0)
            {
                return "Cold";
            }

            if (rapport < 6)
            {
                return "Business Only";
            }

            if (rapport < 15)
            {
                return "Comfortable";
            }

            if (rapport < 30)
            {
                return "Friendly";
            }

            return "Personally Invested";
        }

        public string GetTrustState()
        {
            if (trust < 0)
            {
                return "Distrustful";
            }

            if (trust < 8)
            {
                return "Testing Payment";
            }

            if (trust < 18)
            {
                return "Reliable Customer";
            }

            if (trust < 30)
            {
                return "Trusted";
            }

            return "Creditworthy";
        }

        public string GetRespectState()
        {
            if (respect < 0)
            {
                return "Dismissive";
            }

            if (respect < 8)
            {
                return "Small-Time";
            }

            if (respect < 18)
            {
                return "Capable";
            }

            if (respect < 30)
            {
                return "Serious Operator";
            }

            return "Major Account";
        }

        public float GetPriceMultiplier()
        {
            if (
                definition == null ||
                definition.Stock.Count == 0
            )
            {
                return 1f;
            }

            SupplierStockDefinition first =
                definition.Stock[0];

            if (
                first == null ||
                first.BaseUnitPrice <= 0
            )
            {
                return 1f;
            }

            return
                GetUnitPrice(first) /
                (float)first.BaseUnitPrice;
        }

        public void GrantReferralForTesting()
        {
            developmentReferralOverride = true;

            PublishStatus(
                "Development referral override enabled.",
                4f
            );

            StateChanged?.Invoke();
        }

        public void ResetToDefaultState()
        {
            initialized = false;
            developmentReferralOverride = false;
            EnsureInitialized();
            StateChanged?.Invoke();
        }

        public void RestoreState(
            bool restoredIntroduction,
            int restoredIntroductionChoice,
            int restoredRapport,
            int restoredTrust,
            int restoredRespect,
            int restoredSuccessfulPurchases,
            int restoredLifetimeSpend,
            float restoredRestockSeconds,
            string restoredPendingStockId,
            int restoredPendingQuantity,
            int restoredPendingPrice,
            float restoredPendingSeconds,
            IReadOnlyList<
                SupplierStockSaveData
            > restoredStock
        )
        {
            EnsureInitialized();

            introductionCompleted =
                restoredIntroduction;
            introductionChoice =
                restoredIntroductionChoice;
            rapport =
                Mathf.Clamp(
                    restoredRapport,
                    -100,
                    100
                );
            trust =
                Mathf.Clamp(
                    restoredTrust,
                    -100,
                    100
                );
            respect =
                Mathf.Clamp(
                    restoredRespect,
                    -100,
                    100
                );
            successfulPurchases =
                Mathf.Max(
                    0,
                    restoredSuccessfulPurchases
                );
            lifetimeCleanCashSpent =
                Mathf.Max(
                    0,
                    restoredLifetimeSpend
                );
            restockRemainingSeconds =
                Mathf.Max(
                    0f,
                    restoredRestockSeconds
                );

            RebuildRuntimeStock(false);

            if (restoredStock != null)
            {
                foreach (
                    SupplierStockSaveData saved
                    in restoredStock
                )
                {
                    if (
                        saved == null ||
                        string.IsNullOrWhiteSpace(
                            saved.stockId
                        )
                    )
                    {
                        continue;
                    }

                    SupplierStockRuntimeState state =
                        FindRuntimeStockById(
                            saved.stockId
                        );

                    state?.SetStock(
                        saved.currentStock
                    );
                }
            }

            pendingStock =
                FindStockDefinitionById(
                    restoredPendingStockId
                );

            pendingQuantity =
                pendingStock != null
                    ? Mathf.Max(
                        0,
                        restoredPendingQuantity
                    )
                    : 0;

            pendingTotalPrice =
                pendingStock != null
                    ? Mathf.Max(
                        0,
                        restoredPendingPrice
                    )
                    : 0;

            pendingDeliverySeconds =
                pendingStock != null
                    ? Mathf.Max(
                        0f,
                        restoredPendingSeconds
                    )
                    : 0f;

            deliveryRetryDelay = 0f;
            StateChanged?.Invoke();
        }

        private void UpdateRestock()
        {
            if (
                !initialized ||
                definition == null
            )
            {
                return;
            }

            restockRemainingSeconds -=
                Time.deltaTime;

            if (restockRemainingSeconds > 0f)
            {
                return;
            }

            foreach (
                SupplierStockRuntimeState state
                in runtimeStock
            )
            {
                if (
                    state?.Definition != null
                )
                {
                    state.SetStock(
                        state.Definition.MaximumStock
                    );
                }
            }

            restockRemainingSeconds =
                definition.RestockSeconds;

            PublishStatus(
                $"{DisplayName}'s stock was refreshed.",
                4f
            );

            StateChanged?.Invoke();
        }

        private void UpdatePendingDelivery()
        {
            if (!HasPendingDelivery)
            {
                return;
            }

            if (pendingDeliverySeconds > 0f)
            {
                pendingDeliverySeconds -=
                    Time.deltaTime;
                return;
            }

            if (deliveryRetryDelay > 0f)
            {
                deliveryRetryDelay -=
                    Time.deltaTime;
                return;
            }

            ResolveReferences();

            if (
                deliverySystem == null ||
                pendingStock == null ||
                pendingStock.Item == null
            )
            {
                deliveryRetryDelay = 2f;
                return;
            }

            FurnitureDeliveryCrate crate =
                deliverySystem
                    .SpawnItemDelivery(
                        pendingStock.Item,
                        pendingQuantity,
                        DisplayName
                    );

            if (crate == null)
            {
                deliveryRetryDelay = 2f;
                return;
            }

            string deliveredName =
                pendingStock.Item.DisplayName;
            int deliveredAmount =
                pendingQuantity;
            int paidAmount =
                pendingTotalPrice;

            pendingStock = null;
            pendingQuantity = 0;
            pendingTotalPrice = 0;
            pendingDeliverySeconds = 0f;
            deliveryRetryDelay = 0f;

            successfulPurchases++;
            lifetimeCleanCashSpent +=
                paidAmount;

            int respectGain =
                deliveredAmount >= 3
                    ? 2
                    : 1;

            ApplyRelationshipChange(
                successfulPurchases % 3 == 0
                    ? 1
                    : 0,
                2,
                respectGain,
                "A paid order was delivered successfully."
            );

            PublishStatus(
                $"{deliveredAmount}× " +
                $"{deliveredName} arrived at " +
                "the garage delivery pad.",
                6f
            );

            StateChanged?.Invoke();
        }

        private void ResolveReferralBuyer()
        {
            if (
                referralBuyer != null &&
                definition != null &&
                string.Equals(
                    referralBuyer.BuyerId,
                    definition.RequiredBuyerId,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            referralBuyer = null;

            if (
                definition == null ||
                string.IsNullOrWhiteSpace(
                    definition.RequiredBuyerId
                )
            )
            {
                return;
            }

            BuyerNPC[] buyers =
                FindObjectsByType<BuyerNPC>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (BuyerNPC buyer in buyers)
            {
                if (
                    buyer != null &&
                    string.Equals(
                        buyer.BuyerId,
                        definition.RequiredBuyerId,
                        StringComparison.Ordinal
                    )
                )
                {
                    referralBuyer = buyer;
                    return;
                }
            }
        }

        private void ResolveReferences()
        {
            wallet ??=
                FindFirstObjectByType<PlayerWallet>();

            deliverySystem ??=
                FindFirstObjectByType<
                    FurnitureDeliverySystem
                >();
        }

        private void EnsureInitialized()
        {
            if (
                initialized ||
                definition == null
            )
            {
                return;
            }

            introductionCompleted = false;
            introductionChoice = -1;
            rapport =
                definition.InitialRapport;
            trust =
                definition.InitialTrust;
            respect =
                definition.InitialRespect;
            successfulPurchases = 0;
            lifetimeCleanCashSpent = 0;
            restockRemainingSeconds =
                definition.RestockSeconds;
            pendingStock = null;
            pendingQuantity = 0;
            pendingTotalPrice = 0;
            pendingDeliverySeconds = 0f;
            deliveryRetryDelay = 0f;

            RebuildRuntimeStock(true);
            initialized = true;
        }

        private void RebuildRuntimeStock(
            bool fillToMaximum
        )
        {
            runtimeStock ??=
                new List<
                    SupplierStockRuntimeState
                >();

            runtimeStock.Clear();

            if (definition == null)
            {
                return;
            }

            foreach (
                SupplierStockDefinition stock
                in definition.Stock
            )
            {
                if (stock == null)
                {
                    continue;
                }

                runtimeStock.Add(
                    new SupplierStockRuntimeState(
                        stock,
                        fillToMaximum
                            ? stock.MaximumStock
                            : 0
                    )
                );
            }
        }

        private SupplierStockRuntimeState
            FindRuntimeStock(
                SupplierStockDefinition stock
            )
        {
            if (stock == null)
            {
                return null;
            }

            foreach (
                SupplierStockRuntimeState state
                in runtimeStock
            )
            {
                if (
                    state != null &&
                    state.Definition == stock
                )
                {
                    return state;
                }
            }

            return null;
        }

        private SupplierStockRuntimeState
            FindRuntimeStockById(
                string stockId
            )
        {
            if (
                string.IsNullOrWhiteSpace(
                    stockId
                )
            )
            {
                return null;
            }

            foreach (
                SupplierStockRuntimeState state
                in runtimeStock
            )
            {
                if (
                    state?.Definition != null &&
                    string.Equals(
                        state.Definition.StockId,
                        stockId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return state;
                }
            }

            return null;
        }

        private SupplierStockDefinition
            FindStockDefinitionById(
                string stockId
            )
        {
            if (
                definition == null ||
                string.IsNullOrWhiteSpace(
                    stockId
                )
            )
            {
                return null;
            }

            foreach (
                SupplierStockDefinition stock
                in definition.Stock
            )
            {
                if (
                    stock != null &&
                    string.Equals(
                        stock.StockId,
                        stockId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return stock;
                }
            }

            return null;
        }

        private void ApplyRelationshipChange(
            int rapportChange,
            int trustChange,
            int respectChange,
            string context
        )
        {
            rapport =
                Mathf.Clamp(
                    rapport + rapportChange,
                    -100,
                    100
                );
            trust =
                Mathf.Clamp(
                    trust + trustChange,
                    -100,
                    100
                );
            respect =
                Mathf.Clamp(
                    respect + respectChange,
                    -100,
                    100
                );

            Debug.Log(
                $"[SupplierRelationship] {DisplayName}: " +
                $"Rapport {Signed(rapportChange)}, " +
                $"Trust {Signed(trustChange)}, " +
                $"Respect {Signed(respectChange)}. " +
                context,
                this
            );

            StateChanged?.Invoke();
        }

        private void PublishStatus(
            string message,
            float duration
        )
        {
            statusMessage = message;
            statusUntil =
                Time.unscaledTime +
                Mathf.Max(0.1f, duration);

            Debug.Log(
                $"[Supplier] {DisplayName}: {message}",
                this
            );

            StateChanged?.Invoke();
        }

        private static string Signed(int value)
        {
            return
                value >= 0
                    ? $"+{value}"
                    : value.ToString();
        }
    }
}
