using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using ShadowSupply.Placement;
using ShadowSupply.Player;
using ShadowSupply.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Delivery
{
    public sealed class FurnitureShopHUD : MonoBehaviour
    {
        public static FurnitureShopHUD Instance { get; private set; }

        private FurnitureShopTerminal terminal;
        private FurnitureDeliverySystem deliverySystem;
        private FirstPersonController controller;
        private HotbarController hotbar;
        private PlayerInteractor interactor;
        private InventoryHUD inventoryHud;

        private bool controllerWasEnabled;
        private bool hotbarWasEnabled;
        private bool interactorWasEnabled;
        private bool inventoryHudWasEnabled;
        private bool open;
        private string statusMessage;
        private float statusUntil;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle cardStyle;
        private GUIStyle itemTitleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle cashStyle;
        private GUIStyle statusStyle;
        private GUIStyle buttonStyle;
        private GUIStyle closeStyle;

        public bool IsOpen => open;

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

        public void Open(
            FurnitureShopTerminal shopTerminal,
            GameObject player
        )
        {
            if (
                open ||
                shopTerminal == null ||
                player == null
            )
            {
                return;
            }

            terminal = shopTerminal;
            deliverySystem = terminal.DeliverySystem;

            if (deliverySystem == null)
            {
                return;
            }

            controller =
                player.GetComponent<FirstPersonController>();

            hotbar =
                player.GetComponent<HotbarController>();

            interactor =
                player.GetComponent<PlayerInteractor>();

            inventoryHud =
                FindFirstObjectByType<InventoryHUD>();

            controllerWasEnabled =
                controller != null &&
                controller.enabled;

            hotbarWasEnabled =
                hotbar != null &&
                hotbar.enabled;

            interactorWasEnabled =
                interactor != null &&
                interactor.enabled;

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

            if (inventoryHud != null)
            {
                inventoryHud.enabled = false;
            }

            UnityEngine.Cursor.lockState =
                CursorLockMode.None;

            UnityEngine.Cursor.visible = true;

            open = true;
            statusMessage =
                "Orders arrive immediately at the marked delivery point.";
            statusUntil =
                Time.unscaledTime + 4f;
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
                inventoryHud != null &&
                inventoryHudWasEnabled
            )
            {
                inventoryHud.enabled = true;
            }

            UnityEngine.Cursor.lockState =
                CursorLockMode.Locked;

            UnityEngine.Cursor.visible = false;

            terminal = null;
            deliverySystem = null;
            controller = null;
            hotbar = null;
            interactor = null;
            inventoryHud = null;
        }

        private void OnGUI()
        {
            if (
                !open ||
                deliverySystem == null
            )
            {
                return;
            }

            EnsureStyles();

            GUI.Box(
                new Rect(0f, 0f, Screen.width, Screen.height),
                GUIContent.none,
                panelStyle
            );

            const float width = 980f;
            const float height = 690f;

            Rect window = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height
            );

            GUI.Box(
                window,
                GUIContent.none,
                cardStyle
            );

            GUI.Label(
                new Rect(window.x + 28f, window.y + 20f, 600f, 48f),
                "ROOK'S PROPERTY SUPPLY",
                titleStyle
            );

            GUI.Label(
                new Rect(window.x + 30f, window.y + 69f, 620f, 26f),
                "Furniture, fixtures, and security hardware",
                subtitleStyle
            );

            int cash =
                deliverySystem.Wallet != null
                    ? deliverySystem.Wallet.CleanCash
                    : 0;

            GUI.Label(
                new Rect(window.xMax - 300f, window.y + 26f, 220f, 42f),
                $"CLEAN CASH  ${cash:N0}",
                cashStyle
            );

            if (
                GUI.Button(
                    new Rect(window.xMax - 64f, window.y + 22f, 38f, 38f),
                    "X",
                    closeStyle
                )
            )
            {
                Close();
                return;
            }

            PlacementController controllerInScene =
                FindFirstObjectByType<PlacementController>();

            PlaceableDatabase database =
                controllerInScene != null
                    ? controllerInScene.Database
                    : null;

            if (database == null)
            {
                GUI.Label(
                    new Rect(
                        window.x + 30f,
                        window.y + 130f,
                        window.width - 60f,
                        40f
                    ),
                    "Placeable database unavailable.",
                    bodyStyle
                );
                return;
            }

            const int columns = 2;
            const float cardWidth = 440f;
            const float cardHeight = 142f;
            const float horizontalGap = 24f;
            const float verticalGap = 18f;

            for (
                int index = 0;
                index < database.Definitions.Count;
                index++
            )
            {
                PlaceableDefinition definition =
                    database.GetAt(index);

                if (
                    definition == null ||
                    definition.InventoryItem == null ||
                    definition.PurchasePrice <= 0
                )
                {
                    continue;
                }

                int column = index % columns;
                int row = index / columns;

                Rect itemRect = new Rect(
                    window.x + 30f +
                    column * (cardWidth + horizontalGap),
                    window.y + 120f +
                    row * (cardHeight + verticalGap),
                    cardWidth,
                    cardHeight
                );

                DrawCatalogCard(
                    itemRect,
                    definition
                );
            }

            string footer =
                Time.unscaledTime < statusUntil
                    ? statusMessage
                    : "Collect ordered crates at the delivery marker.";

            GUI.Label(
                new Rect(
                    window.x + 30f,
                    window.yMax - 50f,
                    window.width - 60f,
                    28f
                ),
                footer,
                statusStyle
            );
        }

        private void DrawCatalogCard(
            Rect rect,
            PlaceableDefinition definition
        )
        {
            GUI.Box(
                rect,
                GUIContent.none,
                cardStyle
            );

            GUI.Label(
                new Rect(
                    rect.x + 18f,
                    rect.y + 12f,
                    rect.width - 160f,
                    32f
                ),
                definition.DisplayName.ToUpperInvariant(),
                itemTitleStyle
            );

            GUI.Label(
                new Rect(
                    rect.x + 18f,
                    rect.y + 47f,
                    rect.width - 170f,
                    62f
                ),
                definition.Description,
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    rect.xMax - 138f,
                    rect.y + 14f,
                    120f,
                    30f
                ),
                $"${definition.PurchasePrice:N0}",
                cashStyle
            );

            bool canAfford =
                deliverySystem.Wallet != null &&
                deliverySystem.Wallet.CanAfford(
                    definition.PurchasePrice
                );

            GUI.enabled = canAfford;

            if (
                GUI.Button(
                    new Rect(
                        rect.xMax - 138f,
                        rect.yMax - 54f,
                        120f,
                        36f
                    ),
                    "ORDER",
                    buttonStyle
                )
            )
            {
                bool success =
                    deliverySystem.TryOrder(definition);

                statusMessage =
                    success
                        ? $"{definition.DisplayName} sent to delivery."
                        : $"Unable to order {definition.DisplayName}.";

                statusUntil =
                    Time.unscaledTime + 4f;
            }

            GUI.enabled = true;
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle =
                CreateBoxStyle(
                    new Color(0f, 0f, 0f, 0.88f)
                );

            cardStyle =
                CreateBoxStyle(
                    new Color(
                        0.035f,
                        0.045f,
                        0.048f,
                        0.98f
                    )
                );

            titleStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 31,
                    fontStyle = FontStyle.Bold,
                    normal =
                    {
                        textColor =
                            new Color(0.95f, 0.54f, 0.1f)
                    }
                };

            subtitleStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    normal =
                    {
                        textColor =
                            new Color(0.65f, 0.69f, 0.68f)
                    }
                };

            itemTitleStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    normal =
                    {
                        textColor = Color.white
                    }
                };

            bodyStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    wordWrap = true,
                    normal =
                    {
                        textColor =
                            new Color(0.78f, 0.82f, 0.8f)
                    }
                };

            cashStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight,
                    normal =
                    {
                        textColor =
                            new Color(0.31f, 0.76f, 0.65f)
                    }
                };

            statusStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    normal =
                    {
                        textColor =
                            new Color(0.95f, 0.54f, 0.1f)
                    }
                };

            buttonStyle =
                new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal =
                    {
                        background =
                            MakeTexture(
                                new Color(
                                    0.42f,
                                    0.22f,
                                    0.04f,
                                    1f
                                )
                            ),
                        textColor =
                            new Color(1f, 0.65f, 0.2f)
                    }
                };

            closeStyle =
                new GUIStyle(buttonStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };
        }

        private static GUIStyle CreateBoxStyle(
            Color color
        )
        {
            return new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = MakeTexture(color)
                },
                border =
                    new RectOffset(1, 1, 1, 1)
            };
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

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }
    }
}
