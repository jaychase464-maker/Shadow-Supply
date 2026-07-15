using System.Collections.Generic;
using ShadowSupply.Electrical;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using ShadowSupply.Player;
using ShadowSupply.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Relationships
{
    [DisallowMultipleComponent]
    public sealed class SupplierRelationshipHUD :
        MonoBehaviour
    {
        public static SupplierRelationshipHUD Instance
        {
            get;
            private set;
        }

        private readonly Dictionary<string, int>
            selectedQuantities =
                new Dictionary<string, int>();

        private SupplierNPC supplier;
        private FirstPersonController controller;
        private HotbarController hotbar;
        private PlayerInteractor interactor;
        private PlayerPlugController plugController;
        private InventoryHUD inventoryHud;

        private bool controllerWasEnabled;
        private bool hotbarWasEnabled;
        private bool interactorWasEnabled;
        private bool plugControllerWasEnabled;
        private bool inventoryHudWasEnabled;
        private bool open;

        private GUIStyle overlayStyle;
        private GUIStyle windowStyle;
        private GUIStyle cardStyle;
        private GUIStyle headerStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle mutedStyle;
        private GUIStyle orangeStyle;
        private GUIStyle goodStyle;
        private GUIStyle badStyle;
        private GUIStyle buttonStyle;
        private GUIStyle secondaryButtonStyle;
        private GUIStyle disabledButtonStyle;

        public bool IsOpen => open;

        private void Awake()
        {
            if (
                Instance != null &&
                Instance != this
            )
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (open)
            {
                Close();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (
                open &&
                Keyboard.current != null &&
                Keyboard.current.escapeKey
                    .wasPressedThisFrame
            )
            {
                Close();
            }
        }

        public void Open(
            SupplierNPC targetSupplier,
            GameObject player
        )
        {
            if (
                open ||
                targetSupplier == null ||
                player == null
            )
            {
                return;
            }

            supplier = targetSupplier;
            selectedQuantities.Clear();

            controller =
                player.GetComponent<
                    FirstPersonController
                >();

            hotbar =
                player.GetComponent<
                    HotbarController
                >();

            interactor =
                player.GetComponent<
                    PlayerInteractor
                >();

            plugController =
                player.GetComponent<
                    PlayerPlugController
                >();

            inventoryHud =
                FindFirstObjectByType<
                    InventoryHUD
                >();

            controllerWasEnabled =
                controller != null &&
                controller.enabled;

            hotbarWasEnabled =
                hotbar != null &&
                hotbar.enabled;

            interactorWasEnabled =
                interactor != null &&
                interactor.enabled;

            plugControllerWasEnabled =
                plugController != null &&
                plugController.enabled;

            inventoryHudWasEnabled =
                inventoryHud != null &&
                inventoryHud.enabled;

            if (controller != null)
            {
                controller.enabled = false;
            }

            if (hotbar != null)
            {
                hotbar.enabled = false;
            }

            if (interactor != null)
            {
                interactor.enabled = false;
            }

            if (plugController != null)
            {
                plugController.enabled = false;
            }

            if (inventoryHud != null)
            {
                inventoryHud.enabled = false;
            }

            Cursor.lockState =
                CursorLockMode.None;
            Cursor.visible = true;
            open = true;
        }

        public void Close()
        {
            if (!open)
            {
                return;
            }

            open = false;

            if (
                controller != null &&
                controllerWasEnabled
            )
            {
                controller.enabled = true;
            }

            if (
                hotbar != null &&
                hotbarWasEnabled
            )
            {
                hotbar.enabled = true;
            }

            if (
                interactor != null &&
                interactorWasEnabled
            )
            {
                interactor.enabled = true;
            }

            if (
                plugController != null &&
                plugControllerWasEnabled
            )
            {
                plugController.enabled = true;
            }

            if (
                inventoryHud != null &&
                inventoryHudWasEnabled
            )
            {
                inventoryHud.enabled = true;
            }

            Cursor.lockState =
                CursorLockMode.Locked;
            Cursor.visible = false;

            supplier = null;
            controller = null;
            hotbar = null;
            interactor = null;
            plugController = null;
            inventoryHud = null;
            selectedQuantities.Clear();
        }

        private void OnGUI()
        {
            if (
                !open ||
                supplier == null
            )
            {
                return;
            }

            EnsureStyles();

            GUI.Box(
                new Rect(
                    0f,
                    0f,
                    Screen.width,
                    Screen.height
                ),
                GUIContent.none,
                overlayStyle
            );

            float width =
                Mathf.Min(
                    1120f,
                    Screen.width - 40f
                );

            float height =
                Mathf.Min(
                    720f,
                    Screen.height - 40f
                );

            Rect window =
                new Rect(
                    (Screen.width - width) * 0.5f,
                    (Screen.height - height) * 0.5f,
                    width,
                    height
                );

            GUI.Box(
                window,
                GUIContent.none,
                windowStyle
            );

            DrawHeader(window);
            DrawRelationshipCard(window);
            DrawMainCard(window);
            DrawFooter(window);
        }

        private void DrawHeader(Rect window)
        {
            GUI.Label(
                new Rect(
                    window.x + 28f,
                    window.y + 18f,
                    window.width - 300f,
                    42f
                ),
                supplier.DisplayName
                    .ToUpperInvariant(),
                headerStyle
            );

            GUI.Label(
                new Rect(
                    window.x + 30f,
                    window.y + 58f,
                    window.width - 320f,
                    28f
                ),
                supplier.DistrictName +
                " • Print and Materials Broker",
                mutedStyle
            );

            string rightHeader =
                supplier.ReferralUnlocked
                    ? supplier
                        .GetRelationshipState()
                    : "REFERRAL LOCKED";

            GUI.Label(
                new Rect(
                    window.xMax - 265f,
                    window.y + 27f,
                    230f,
                    32f
                ),
                rightHeader,
                supplier.ReferralUnlocked
                    ? orangeStyle
                    : badStyle
            );
        }

        private void DrawRelationshipCard(
            Rect window
        )
        {
            Rect card =
                new Rect(
                    window.x + 28f,
                    window.y + 104f,
                    315f,
                    window.height - 204f
                );

            GUI.Box(
                card,
                GUIContent.none,
                cardStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 20f,
                    card.y + 18f,
                    card.width - 40f,
                    30f
                ),
                "SUPPLIER RELATIONSHIP",
                titleStyle
            );

            DrawRelationshipLine(
                card,
                64f,
                "PERSONAL RAPPORT",
                supplier.GetRapportState()
            );

            DrawRelationshipLine(
                card,
                124f,
                "PAYMENT TRUST",
                supplier.GetTrustState()
            );

            DrawRelationshipLine(
                card,
                184f,
                "BUSINESS RESPECT",
                supplier.GetRespectState()
            );

            GUI.Label(
                new Rect(
                    card.x + 20f,
                    card.y + 254f,
                    card.width - 40f,
                    25f
                ),
                "ACCOUNT HISTORY",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 290f,
                    card.width - 48f,
                    27f
                ),
                $"Completed purchases: " +
                $"{supplier.SuccessfulPurchases}",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 324f,
                    card.width - 48f,
                    27f
                ),
                $"Lifetime spend: " +
                $"${supplier.LifetimeCleanCashSpent:N0}",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 20f,
                    card.y + 382f,
                    card.width - 40f,
                    25f
                ),
                "CURRENT TERMS",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 418f,
                    card.width - 48f,
                    48f
                ),
                $"Price multiplier: " +
                $"{supplier.GetPriceMultiplier():0.00}×",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 466f,
                    card.width - 48f,
                    52f
                ),
                $"Full restock in approximately " +
                $"{Mathf.CeilToInt(supplier.RestockRemainingSeconds)}s",
                mutedStyle
            );
        }

        private void DrawRelationshipLine(
            Rect card,
            float offsetY,
            string label,
            string value
        )
        {
            GUI.Label(
                new Rect(
                    card.x + 20f,
                    card.y + offsetY,
                    card.width - 40f,
                    20f
                ),
                label,
                mutedStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 20f,
                    card.y + offsetY + 23f,
                    card.width - 40f,
                    28f
                ),
                value,
                bodyStyle
            );
        }

        private void DrawMainCard(
            Rect window
        )
        {
            Rect card =
                new Rect(
                    window.x + 361f,
                    window.y + 104f,
                    window.width - 389f,
                    window.height - 204f
                );

            GUI.Box(
                card,
                GUIContent.none,
                cardStyle
            );

            if (!supplier.ReferralUnlocked)
            {
                DrawReferralLocked(card);
                return;
            }

            if (!supplier.IntroductionCompleted)
            {
                DrawIntroduction(card);
                return;
            }

            DrawCatalog(card);
        }

        private void DrawReferralLocked(Rect card)
        {
            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 20f,
                    card.width - 48f,
                    34f
                ),
                "CONTACT NOT AVAILABLE",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 80f,
                    card.width - 48f,
                    128f
                ),
                "Elias does not deal with walk-ins. " +
                "Mara Voss must trust and respect your work " +
                "enough to introduce you first.",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 232f,
                    card.width - 48f,
                    76f
                ),
                supplier.Definition != null
                    ? "Required access: " +
                      supplier.Definition
                          .RequiredReferralName
                    : "A trusted referral is required.",
                badStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 330f,
                    card.width - 48f,
                    100f
                ),
                "Complete Mara's escalating buyer orders. " +
                "Her referral unlocks this supplier without " +
                "using a generic player level.",
                mutedStyle
            );
        }

        private void DrawIntroduction(Rect card)
        {
            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 20f,
                    card.width - 48f,
                    34f
                ),
                "REFERRED INTRODUCTION",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 70f,
                    card.width - 48f,
                    110f
                ),
                "\"Mara says you finish what you start. " +
                "That gets you in the door. It doesn't get " +
                "you my best prices or my private stock.\"",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 198f,
                    card.width - 48f,
                    26f
                ),
                "Choose your opening approach:",
                orangeStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        card.x + 24f,
                        card.y + 240f,
                        card.width - 48f,
                        58f
                    ),
                    "\"Mara sent me. I keep her name clean.\"",
                    buttonStyle
                )
            )
            {
                supplier
                    .CompleteIntroductionChoice(0);
            }

            if (
                GUI.Button(
                    new Rect(
                        card.x + 24f,
                        card.y + 312f,
                        card.width - 48f,
                        58f
                    ),
                    "\"I pay on time. That's all you need to know.\"",
                    buttonStyle
                )
            )
            {
                supplier
                    .CompleteIntroductionChoice(1);
            }

            if (
                GUI.Button(
                    new Rect(
                        card.x + 24f,
                        card.y + 384f,
                        card.width - 48f,
                        58f
                    ),
                    "\"I need volume. Show me what you can move.\"",
                    buttonStyle
                )
            )
            {
                supplier
                    .CompleteIntroductionChoice(2);
            }
        }

        private void DrawCatalog(Rect card)
        {
            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 18f,
                    card.width - 48f,
                    32f
                ),
                "PRIVATE MATERIAL CATALOG",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 52f,
                    card.width - 48f,
                    28f
                ),
                supplier.Wallet != null
                    ? $"Clean cash: " +
                      $"${supplier.Wallet.CleanCash:N0}"
                    : "Wallet unavailable",
                goodStyle
            );

            if (supplier.HasPendingDelivery)
            {
                DrawPendingDelivery(card);
            }

            float startY =
                supplier.HasPendingDelivery
                    ? 142f
                    : 94f;

            if (
                supplier.Definition == null ||
                supplier.Definition.Stock.Count == 0
            )
            {
                GUI.Label(
                    new Rect(
                        card.x + 24f,
                        card.y + startY,
                        card.width - 48f,
                        60f
                    ),
                    "No stock is configured.",
                    badStyle
                );
                return;
            }

            float rowHeight = 98f;

            for (
                int index = 0;
                index <
                supplier.Definition.Stock.Count;
                index++
            )
            {
                SupplierStockDefinition stock =
                    supplier.Definition.Stock[index];

                if (stock == null)
                {
                    continue;
                }

                Rect row =
                    new Rect(
                        card.x + 20f,
                        card.y + startY +
                        index * rowHeight,
                        card.width - 40f,
                        rowHeight - 8f
                    );

                DrawStockRow(row, stock);
            }
        }

        private void DrawPendingDelivery(Rect card)
        {
            SupplierStockDefinition pending =
                supplier.PendingStock;

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 88f,
                    card.width - 48f,
                    44f
                ),
                pending != null
                    ? $"DELIVERY PENDING — " +
                      $"{supplier.PendingQuantity}× " +
                      $"{pending.DisplayName} • " +
                      $"{Mathf.CeilToInt(supplier.PendingDeliverySeconds)}s"
                    : "DELIVERY PENDING",
                orangeStyle
            );
        }

        private void DrawStockRow(
            Rect row,
            SupplierStockDefinition stock
        )
        {
            GUI.Box(
                row,
                GUIContent.none,
                windowStyle
            );

            bool unlocked =
                supplier.IsStockUnlocked(stock);

            int available =
                supplier.GetCurrentStock(stock);

            int unitPrice =
                supplier.GetUnitPrice(stock);

            GUI.Label(
                new Rect(
                    row.x + 14f,
                    row.y + 10f,
                    row.width * 0.43f,
                    27f
                ),
                stock.DisplayName,
                unlocked
                    ? bodyStyle
                    : mutedStyle
            );

            GUI.Label(
                new Rect(
                    row.x + 14f,
                    row.y + 40f,
                    row.width * 0.43f,
                    38f
                ),
                unlocked
                    ? $"Stock {available}/" +
                      $"{stock.MaximumStock} • " +
                      $"${unitPrice:N0} each"
                    : supplier.GetStockLockReason(
                        stock
                    ),
                unlocked
                    ? goodStyle
                    : badStyle
            );

            int selected =
                GetSelectedQuantity(stock);

            Rect minusRect =
                new Rect(
                    row.x + row.width - 270f,
                    row.y + 18f,
                    36f,
                    42f
                );

            Rect countRect =
                new Rect(
                    minusRect.xMax + 6f,
                    row.y + 18f,
                    48f,
                    42f
                );

            Rect plusRect =
                new Rect(
                    countRect.xMax + 6f,
                    row.y + 18f,
                    36f,
                    42f
                );

            Rect orderRect =
                new Rect(
                    plusRect.xMax + 10f,
                    row.y + 18f,
                    118f,
                    42f
                );

            GUI.enabled =
                unlocked &&
                available > 0 &&
                !supplier.HasPendingDelivery;

            if (
                GUI.Button(
                    minusRect,
                    "−",
                    secondaryButtonStyle
                )
            )
            {
                SetSelectedQuantity(
                    stock,
                    selected - 1
                );
            }

            GUI.Box(
                countRect,
                selected.ToString(),
                windowStyle
            );

            if (
                GUI.Button(
                    plusRect,
                    "+",
                    secondaryButtonStyle
                )
            )
            {
                SetSelectedQuantity(
                    stock,
                    selected + 1
                );
            }

            int total =
                selected *
                unitPrice;

            bool canOrder =
                supplier.CanOrder(
                    stock,
                    selected,
                    out string _
                );

            GUI.enabled = canOrder;

            if (
                GUI.Button(
                    orderRect,
                    $"ORDER\n${total:N0}",
                    canOrder
                        ? buttonStyle
                        : disabledButtonStyle
                )
            )
            {
                supplier.TryOrder(
                    stock,
                    selected,
                    out string _
                );
            }

            GUI.enabled = true;
        }

        private int GetSelectedQuantity(
            SupplierStockDefinition stock
        )
        {
            if (
                stock == null ||
                string.IsNullOrWhiteSpace(
                    stock.StockId
                )
            )
            {
                return 1;
            }

            if (
                !selectedQuantities.TryGetValue(
                    stock.StockId,
                    out int quantity
                )
            )
            {
                quantity = 1;
                selectedQuantities[
                    stock.StockId
                ] = quantity;
            }

            int maximum =
                Mathf.Max(
                    1,
                    supplier.GetCurrentStock(stock)
                );

            quantity =
                Mathf.Clamp(
                    quantity,
                    1,
                    maximum
                );

            selectedQuantities[
                stock.StockId
            ] = quantity;

            return quantity;
        }

        private void SetSelectedQuantity(
            SupplierStockDefinition stock,
            int quantity
        )
        {
            if (
                stock == null ||
                string.IsNullOrWhiteSpace(
                    stock.StockId
                )
            )
            {
                return;
            }

            int maximum =
                Mathf.Max(
                    1,
                    supplier.GetCurrentStock(stock)
                );

            selectedQuantities[
                stock.StockId
            ] =
                Mathf.Clamp(
                    quantity,
                    1,
                    maximum
                );
        }

        private void DrawFooter(Rect window)
        {
            string status =
                supplier.StatusMessage;

            if (!string.IsNullOrWhiteSpace(status))
            {
                GUI.Label(
                    new Rect(
                        window.x + 30f,
                        window.yMax - 76f,
                        window.width - 235f,
                        30f
                    ),
                    status,
                    orangeStyle
                );
            }

            GUI.Label(
                new Rect(
                    window.x + 30f,
                    window.yMax - 42f,
                    window.width - 235f,
                    25f
                ),
                "Orders use clean cash and arrive as " +
                "physical crates at the garage delivery pad.",
                mutedStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        window.xMax - 180f,
                        window.yMax - 64f,
                        150f,
                        38f
                    ),
                    "CLOSE",
                    buttonStyle
                )
            )
            {
                Close();
            }
        }

        private void EnsureStyles()
        {
            if (overlayStyle != null)
            {
                return;
            }

            overlayStyle =
                BoxStyle(
                    new Color(
                        0.005f,
                        0.007f,
                        0.009f,
                        0.94f
                    )
                );

            windowStyle =
                BoxStyle(
                    new Color(
                        0.038f,
                        0.043f,
                        0.048f,
                        0.98f
                    )
                );

            cardStyle =
                BoxStyle(
                    new Color(
                        0.07f,
                        0.075f,
                        0.08f,
                        0.98f
                    )
                );

            headerStyle =
                LabelStyle(
                    30,
                    FontStyle.Bold,
                    new Color(
                        1f,
                        0.48f,
                        0.015f
                    )
                );

            titleStyle =
                LabelStyle(
                    21,
                    FontStyle.Bold,
                    Color.white
                );

            bodyStyle =
                LabelStyle(
                    15,
                    FontStyle.Normal,
                    new Color(
                        0.88f,
                        0.9f,
                        0.92f
                    )
                );

            bodyStyle.wordWrap = true;

            mutedStyle =
                new GUIStyle(bodyStyle);

            mutedStyle.normal.textColor =
                new Color(
                    0.58f,
                    0.62f,
                    0.66f
                );

            orangeStyle =
                new GUIStyle(bodyStyle);

            orangeStyle.fontStyle =
                FontStyle.Bold;

            orangeStyle.normal.textColor =
                new Color(
                    1f,
                    0.5f,
                    0.02f
                );

            goodStyle =
                new GUIStyle(bodyStyle);

            goodStyle.fontStyle =
                FontStyle.Bold;

            goodStyle.normal.textColor =
                new Color(
                    0.32f,
                    1f,
                    0.48f
                );

            badStyle =
                new GUIStyle(bodyStyle);

            badStyle.fontStyle =
                FontStyle.Bold;

            badStyle.normal.textColor =
                new Color(
                    1f,
                    0.28f,
                    0.2f
                );

            buttonStyle =
                ButtonStyle(
                    new Color(
                        0.82f,
                        0.28f,
                        0.01f
                    )
                );

            secondaryButtonStyle =
                ButtonStyle(
                    new Color(
                        0.2f,
                        0.21f,
                        0.22f
                    )
                );

            disabledButtonStyle =
                ButtonStyle(
                    new Color(
                        0.15f,
                        0.16f,
                        0.17f
                    )
                );

            disabledButtonStyle
                .normal.textColor =
                    new Color(
                        0.42f,
                        0.44f,
                        0.46f
                    );
        }

        private static GUIStyle BoxStyle(
            Color color
        )
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.box);

            style.normal.background =
                MakeTexture(color);

            return style;
        }

        private static GUIStyle LabelStyle(
            int size,
            FontStyle fontStyle,
            Color color
        )
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = size,
                    fontStyle = fontStyle,
                    alignment =
                        TextAnchor.UpperLeft
                };

            style.normal.textColor = color;
            return style;
        }

        private static GUIStyle ButtonStyle(
            Color color
        )
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };

            style.normal.textColor =
                Color.white;

            style.normal.background =
                MakeTexture(color);

            style.hover.background =
                MakeTexture(
                    new Color(
                        Mathf.Clamp01(
                            color.r * 1.18f
                        ),
                        Mathf.Clamp01(
                            color.g * 1.18f
                        ),
                        Mathf.Clamp01(
                            color.b * 1.18f
                        ),
                        color.a
                    )
                );

            return style;
        }

        private static Texture2D MakeTexture(
            Color color
        )
        {
            Texture2D texture =
                new Texture2D(1, 1)
                {
                    hideFlags =
                        HideFlags.HideAndDontSave
                };

            texture.SetPixel(
                0,
                0,
                color
            );

            texture.Apply();
            return texture;
        }
    }
}
