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
    public sealed class BuyerRelationshipHUD : MonoBehaviour
    {
        public static BuyerRelationshipHUD Instance { get; private set; }

        private BuyerNPC buyer;
        private FirstPersonController controller;
        private HotbarController hotbar;
        private PlayerInteractor interactor;
        private PlayerPlugController plugController;
        private InventoryHUD inventoryHud;

        private bool controllerEnabled;
        private bool hotbarEnabled;
        private bool interactorEnabled;
        private bool plugEnabled;
        private bool inventoryHudEnabled;
        private bool open;

        private GUIStyle overlay;
        private GUIStyle panel;
        private GUIStyle card;
        private GUIStyle header;
        private GUIStyle title;
        private GUIStyle body;
        private GUIStyle muted;
        private GUIStyle orange;
        private GUIStyle good;
        private GUIStyle bad;
        private GUIStyle button;
        private GUIStyle secondary;
        private GUIStyle disabled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
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
                Keyboard.current.escapeKey.wasPressedThisFrame
            )
            {
                Close();
            }
        }

        public void Open(BuyerNPC target, GameObject player)
        {
            if (open || target == null || player == null)
            {
                return;
            }

            buyer = target;
            controller = player.GetComponent<FirstPersonController>();
            hotbar = player.GetComponent<HotbarController>();
            interactor = player.GetComponent<PlayerInteractor>();
            plugController = player.GetComponent<PlayerPlugController>();
            inventoryHud = FindFirstObjectByType<InventoryHUD>();

            controllerEnabled = controller != null && controller.enabled;
            hotbarEnabled = hotbar != null && hotbar.enabled;
            interactorEnabled = interactor != null && interactor.enabled;
            plugEnabled =
                plugController != null && plugController.enabled;
            inventoryHudEnabled =
                inventoryHud != null && inventoryHud.enabled;

            if (controller != null) controller.enabled = false;
            if (hotbar != null) hotbar.enabled = false;
            if (interactor != null) interactor.enabled = false;
            if (plugController != null) plugController.enabled = false;
            if (inventoryHud != null) inventoryHud.enabled = false;

            Cursor.lockState = CursorLockMode.None;
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

            if (controller != null && controllerEnabled)
                controller.enabled = true;
            if (hotbar != null && hotbarEnabled)
                hotbar.enabled = true;
            if (interactor != null && interactorEnabled)
                interactor.enabled = true;
            if (plugController != null && plugEnabled)
                plugController.enabled = true;
            if (inventoryHud != null && inventoryHudEnabled)
                inventoryHud.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            buyer = null;
            controller = null;
            hotbar = null;
            interactor = null;
            plugController = null;
            inventoryHud = null;
        }

        private void OnGUI()
        {
            if (!open || buyer == null)
            {
                return;
            }

            EnsureStyles();

            GUI.Box(
                new Rect(0f, 0f, Screen.width, Screen.height),
                GUIContent.none,
                overlay
            );

            float width = Mathf.Min(1040f, Screen.width - 40f);
            float height = Mathf.Min(700f, Screen.height - 40f);

            Rect window = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height
            );

            GUI.Box(window, GUIContent.none, panel);

            GUI.Label(
                new Rect(window.x + 28f, window.y + 18f, 620f, 42f),
                buyer.DisplayName.ToUpperInvariant(),
                header
            );

            GUI.Label(
                new Rect(window.x + 30f, window.y + 58f, 620f, 28f),
                buyer.DistrictName + " • Persistent Buyer Contact",
                muted
            );

            GUI.Label(
                new Rect(window.xMax - 250f, window.y + 28f, 215f, 30f),
                buyer.GetRelationshipState(),
                orange
            );

            Rect relationshipCard = new Rect(
                window.x + 28f,
                window.y + 104f,
                315f,
                window.height - 202f
            );

            Rect conversationCard = new Rect(
                window.x + 361f,
                window.y + 104f,
                window.width - 389f,
                window.height - 202f
            );

            GUI.Box(relationshipCard, GUIContent.none, card);
            GUI.Box(conversationCard, GUIContent.none, card);

            DrawRelationship(relationshipCard);
            DrawConversation(conversationCard);

            string status = buyer.StatusMessage;

            if (!string.IsNullOrWhiteSpace(status))
            {
                GUI.Label(
                    new Rect(
                        window.x + 30f,
                        window.yMax - 74f,
                        window.width - 230f,
                        30f
                    ),
                    status,
                    orange
                );
            }

            GUI.Label(
                new Rect(
                    window.x + 30f,
                    window.yMax - 40f,
                    window.width - 230f,
                    24f
                ),
                "Hold the requested package in the hotbar before handoff.",
                muted
            );

            if (
                GUI.Button(
                    new Rect(
                        window.xMax - 180f,
                        window.yMax - 62f,
                        150f,
                        38f
                    ),
                    "CLOSE",
                    button
                )
            )
            {
                Close();
            }
        }

        private void DrawRelationship(Rect area)
        {
            GUI.Label(
                new Rect(area.x + 20f, area.y + 18f, 275f, 30f),
                "RELATIONSHIP",
                title
            );

            DrawRelationshipLine(
                area,
                62f,
                "PERSONAL RAPPORT",
                buyer.GetRapportState()
            );

            DrawRelationshipLine(
                area,
                120f,
                "PROFESSIONAL TRUST",
                buyer.GetTrustState()
            );

            DrawRelationshipLine(
                area,
                178f,
                "PROFESSIONAL RESPECT",
                buyer.GetRespectState()
            );

            GUI.Label(
                new Rect(area.x + 20f, area.y + 250f, 275f, 25f),
                "ORDER HISTORY",
                orange
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 284f, 250f, 26f),
                $"Completed: {buyer.SuccessfulOrders}",
                good
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 316f, 250f, 26f),
                $"Failed: {buyer.FailedOrders}",
                buyer.FailedOrders > 0 ? bad : muted
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 348f, 250f, 26f),
                $"Declined: {buyer.DeclinedOrders}",
                muted
            );

            GUI.Label(
                new Rect(area.x + 20f, area.y + 404f, 275f, 25f),
                "NETWORK",
                orange
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 438f, 250f, 76f),
                buyer.GetReferralState(),
                buyer.ReferralUnlocked ? good : muted
            );
        }

        private void DrawRelationshipLine(
            Rect area,
            float y,
            string label,
            string value
        )
        {
            GUI.Label(
                new Rect(area.x + 20f, area.y + y, 275f, 20f),
                label,
                muted
            );

            GUI.Label(
                new Rect(area.x + 20f, area.y + y + 22f, 275f, 28f),
                value,
                body
            );
        }

        private void DrawConversation(Rect area)
        {
            if (!buyer.IntroductionCompleted)
            {
                DrawIntroduction(area);
                return;
            }

            switch (buyer.OrderState)
            {
                case BuyerOrderState.Active:
                    DrawActiveOrder(area);
                    break;

                case BuyerOrderState.Completed:
                    DrawResult(
                        area,
                        "ORDER COMPLETE",
                        $"Payment received: ${buyer.LastReward} dirty cash",
                        "\"Clean enough. On time. That matters more than talking.\"",
                        true
                    );
                    break;

                case BuyerOrderState.Failed:
                    DrawResult(
                        area,
                        "ORDER FAILED",
                        "The agreement was broken. Delivered items were not returned.",
                        "\"I gave you a chance. Don't make me regret giving you another.\"",
                        false
                    );
                    break;

                case BuyerOrderState.Declined:
                    DrawResult(
                        area,
                        "ORDER DECLINED",
                        "Declining is allowed, but it makes the buyer less certain you are available.",
                        "\"Fine. I'll find someone else for now.\"",
                        false
                    );
                    break;

                default:
                    DrawAvailableOrder(area);
                    break;
            }
        }

        private void DrawIntroduction(Rect area)
        {
            GUI.Label(
                new Rect(area.x + 24f, area.y + 20f, area.width - 48f, 34f),
                "FIRST IMPRESSION",
                title
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 70f, area.width - 48f, 108f),
                "\"I heard you've started moving sealed hardware out of " +
                "that garage. I don't care about your story yet. I care " +
                "whether your work is clean and whether you do what you say.\"",
                body
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 195f, area.width - 48f, 25f),
                "Choose your approach:",
                orange
            );

            if (
                GUI.Button(
                    new Rect(area.x + 24f, area.y + 235f, area.width - 48f, 56f),
                    "\"Keep it professional. Tell me what you need.\"",
                    button
                )
            )
            {
                buyer.CompleteIntroductionChoice(0);
            }

            if (
                GUI.Button(
                    new Rect(area.x + 24f, area.y + 305f, area.width - 48f, 56f),
                    "\"How did someone around here hear about me?\"",
                    button
                )
            )
            {
                buyer.CompleteIntroductionChoice(1);
            }

            if (
                GUI.Button(
                    new Rect(area.x + 24f, area.y + 375f, area.width - 48f, 56f),
                    "\"My work speaks for itself. You'll come back.\"",
                    button
                )
            )
            {
                buyer.CompleteIntroductionChoice(2);
            }
        }

        private void DrawAvailableOrder(Rect area)
        {
            BuyerOrderDefinition order = buyer.GetAvailableOrder();

            GUI.Label(
                new Rect(area.x + 24f, area.y + 20f, area.width - 48f, 34f),
                order != null ? "ORDER OFFER" : "NO ACTIVE WORK",
                title
            );

            if (order == null)
            {
                string text = buyer.CooldownRemainingSeconds > 0f
                    ? $"{buyer.DisplayName} is waiting before offering more work.\n\n" +
                      $"Check back in about " +
                      $"{Mathf.CeilToInt(buyer.CooldownRemainingSeconds)} seconds."
                    : "No additional order currently meets the relationship requirements.";

                GUI.Label(
                    new Rect(area.x + 24f, area.y + 80f, area.width - 48f, 120f),
                    text,
                    muted
                );

                return;
            }

            GUI.Label(
                new Rect(area.x + 24f, area.y + 68f, area.width - 48f, 32f),
                order.DisplayName,
                orange
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 108f, area.width - 48f, 72f),
                order.Description,
                body
            );

            DrawTerms(area, order, 195f);

            if (
                GUI.Button(
                    new Rect(
                        area.x + 24f,
                        area.yMax - 118f,
                        area.width * 0.61f,
                        48f
                    ),
                    "ACCEPT ORDER",
                    button
                )
            )
            {
                buyer.AcceptAvailableOrder(out string _);
            }

            if (
                GUI.Button(
                    new Rect(
                        area.x + area.width * 0.65f,
                        area.yMax - 118f,
                        area.width * 0.31f,
                        48f
                    ),
                    "DECLINE",
                    secondary
                )
            )
            {
                buyer.DeclineAvailableOrder(out string _);
            }
        }

        private void DrawActiveOrder(Rect area)
        {
            BuyerOrderDefinition order = buyer.ActiveOrder;

            GUI.Label(
                new Rect(area.x + 24f, area.y + 20f, area.width - 48f, 34f),
                "ACTIVE ORDER",
                title
            );

            if (order == null)
            {
                GUI.Label(
                    new Rect(area.x + 24f, area.y + 80f, area.width - 48f, 60f),
                    "The active order definition is missing.",
                    bad
                );
                return;
            }

            GUI.Label(
                new Rect(area.x + 24f, area.y + 68f, area.width - 48f, 32f),
                order.DisplayName,
                orange
            );

            DrawTerms(area, order, 115f);

            GUI.Label(
                new Rect(area.x + 24f, area.y + 300f, area.width - 48f, 28f),
                $"Delivered: {buyer.DeliveredQuantity}/{order.Quantity}",
                good
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 336f, area.width - 48f, 28f),
                $"Deadline: {buyer.FormatDeadline()}",
                buyer.RemainingDeadlineSeconds < 60f ? bad : orange
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 380f, area.width - 48f, 72f),
                GetHeldStatus(order),
                body
            );

            bool canDeliver = CanDeliver(order);
            GUI.enabled = canDeliver;

            if (
                GUI.Button(
                    new Rect(
                        area.x + 24f,
                        area.yMax - 118f,
                        area.width * 0.61f,
                        48f
                    ),
                    "HAND OVER HELD PACKAGE",
                    canDeliver ? button : disabled
                )
            )
            {
                buyer.TryHandOverHeldItem(out string _);
            }

            GUI.enabled = true;

            if (
                GUI.Button(
                    new Rect(
                        area.x + area.width * 0.65f,
                        area.yMax - 118f,
                        area.width * 0.31f,
                        48f
                    ),
                    "CANCEL",
                    secondary
                )
            )
            {
                buyer.CancelActiveOrder(out string _);
            }
        }

        private void DrawResult(
            Rect area,
            string resultTitle,
            string result,
            string quote,
            bool success
        )
        {
            GUI.Label(
                new Rect(area.x + 24f, area.y + 20f, area.width - 48f, 34f),
                resultTitle,
                title
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 82f, area.width - 48f, 72f),
                result,
                success ? good : bad
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + 176f, area.width - 48f, 90f),
                quote,
                body
            );

            GUI.Label(
                new Rect(area.x + 24f, area.yMax - 110f, area.width - 48f, 50f),
                buyer.CooldownRemainingSeconds > 0f
                    ? $"Another opportunity may appear in about " +
                      $"{Mathf.CeilToInt(buyer.CooldownRemainingSeconds)} seconds."
                    : "Another opportunity may now be available.",
                orange
            );
        }

        private void DrawTerms(
            Rect area,
            BuyerOrderDefinition order,
            float y
        )
        {
            GUI.Label(
                new Rect(area.x + 24f, area.y + y, area.width - 48f, 27f),
                $"Product: {order.Quantity}× {order.RequestedItem.DisplayName}",
                body
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + y + 35f, area.width - 48f, 27f),
                $"Minimum quality: {order.MinimumQuality}",
                body
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + y + 70f, area.width - 48f, 27f),
                $"Base payment: ${order.BaseDirtyCashReward} dirty cash",
                body
            );

            GUI.Label(
                new Rect(area.x + 24f, area.y + y + 105f, area.width - 48f, 27f),
                $"Deadline: {Mathf.CeilToInt(order.DeadlineSeconds / 60f)} minutes",
                body
            );
        }

        private string GetHeldStatus(BuyerOrderDefinition order)
        {
            if (
                hotbar == null ||
                hotbar.SelectedSlot == null ||
                hotbar.SelectedSlot.IsEmpty
            )
            {
                return
                    "Held item: none. Close this screen, select the package " +
                    "in the hotbar, then speak to the buyer again.";
            }

            ItemStack held = hotbar.SelectedSlot.Stack;

            if (held.Item != order.RequestedItem)
            {
                return
                    $"Held item: {held.Item.DisplayName}. " +
                    $"Required: {order.RequestedItem.DisplayName}.";
            }

            if ((int)held.Quality < (int)order.MinimumQuality)
            {
                return
                    $"Held package quality: {held.Quality}. " +
                    $"Required: {order.MinimumQuality} or better.";
            }

            return
                $"Held package: {held.Quality} quality, " +
                $"{held.Condition:P0} condition.";
        }

        private bool CanDeliver(BuyerOrderDefinition order)
        {
            if (
                hotbar == null ||
                hotbar.SelectedSlot == null ||
                hotbar.SelectedSlot.IsEmpty
            )
            {
                return false;
            }

            ItemStack held = hotbar.SelectedSlot.Stack;

            return
                held.Item == order.RequestedItem &&
                (int)held.Quality >= (int)order.MinimumQuality;
        }

        private void EnsureStyles()
        {
            if (overlay != null)
            {
                return;
            }

            overlay = BoxStyle(new Color(0.005f, 0.007f, 0.009f, 0.94f));
            panel = BoxStyle(new Color(0.038f, 0.043f, 0.048f, 0.98f));
            card = BoxStyle(new Color(0.07f, 0.075f, 0.08f, 0.98f));

            header = LabelStyle(
                30,
                FontStyle.Bold,
                new Color(1f, 0.48f, 0.015f)
            );

            title = LabelStyle(22, FontStyle.Bold, Color.white);
            body = LabelStyle(
                15,
                FontStyle.Normal,
                new Color(0.88f, 0.9f, 0.92f)
            );
            body.wordWrap = true;

            muted = new GUIStyle(body);
            muted.normal.textColor =
                new Color(0.58f, 0.62f, 0.66f);

            orange = new GUIStyle(body);
            orange.fontStyle = FontStyle.Bold;
            orange.normal.textColor =
                new Color(1f, 0.5f, 0.02f);

            good = new GUIStyle(body);
            good.fontStyle = FontStyle.Bold;
            good.normal.textColor =
                new Color(0.32f, 1f, 0.48f);

            bad = new GUIStyle(body);
            bad.fontStyle = FontStyle.Bold;
            bad.normal.textColor =
                new Color(1f, 0.28f, 0.2f);

            button = ButtonStyle(new Color(0.82f, 0.28f, 0.01f));
            secondary = ButtonStyle(new Color(0.2f, 0.21f, 0.22f));
            disabled = ButtonStyle(new Color(0.15f, 0.16f, 0.17f));
            disabled.normal.textColor =
                new Color(0.42f, 0.44f, 0.46f);
        }

        private static GUIStyle BoxStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = MakeTexture(color);
            return style;
        }

        private static GUIStyle LabelStyle(
            int size,
            FontStyle fontStyle,
            Color color
        )
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = fontStyle,
                alignment = TextAnchor.UpperLeft
            };

            style.normal.textColor = color;
            return style;
        }

        private static GUIStyle ButtonStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            style.normal.textColor = Color.white;
            style.normal.background = MakeTexture(color);
            style.hover.background = MakeTexture(color * 1.18f);
            return style;
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
