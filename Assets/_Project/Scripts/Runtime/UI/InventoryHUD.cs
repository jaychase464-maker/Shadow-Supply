using System;
using System.Collections.Generic;
using ShadowSupply.Inventory;
using ShadowSupply.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ShadowSupply.UI
{
    /// <summary>
    /// Production-style UI Toolkit inventory presentation inspired by the
    /// Shadow Supply industrial inventory concept.
    ///
    /// The screen is data-driven from PlayerInventory and HotbarController.
    /// It keeps the compact hotbar visible during gameplay and expands into
    /// the full inventory, equipment, and item-details screen when opened.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryHUD : MonoBehaviour
    {
        private const string LayoutResourcePath =
            "ShadowSupply/UI/Inventory/InventoryScreen";

        private const string StyleResourcePath =
            "ShadowSupply/UI/Inventory/InventoryScreen";

        private const string IconResourceRoot =
            "ShadowSupply/UI/Inventory/Icons";

        [Header("Runtime References")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private HotbarController hotbar;
        [SerializeField] private FirstPersonController firstPersonController;

        [Header("Header Values")]
        [Tooltip("Temporary value until the full economy system is implemented.")]
        [SerializeField, Min(0)] private int cash;

        [Tooltip("Temporary value until the full dirty-cash system is implemented.")]
        [SerializeField, Min(0)] private int dirtyCash;

        [Tooltip("Temporary value until the full heat system is implemented.")]
        [SerializeField, Range(0f, 100f)] private float heatPercent;

        [Tooltip("Temporary quantity-based capacity until item weight is implemented.")]
        [SerializeField, Min(1)] private int weightCapacity = 60;

        private UIDocument uiDocument;
        private PanelSettings runtimePanelSettings;
        private VisualTreeAsset layoutAsset;
        private StyleSheet styleAsset;

        private PlayerInventory subscribedInventory;
        private HotbarController subscribedHotbar;

        private VisualElement root;
        private VisualElement inventoryUiRoot;
        private VisualElement inventoryScreen;
        private VisualElement compactHotbarShell;
        private VisualElement inventoryGrid;
        private VisualElement inventoryHotbarGrid;
        private VisualElement compactHotbarGrid;

        private Label inventoryCountLabel;
        private Label cashLabel;
        private Label dirtyCashLabel;
        private Label heatLabel;
        private Label weightLabel;

        private Label detailsNameLabel;
        private Label detailsQuantityLabel;
        private Label detailsCategoryLabel;
        private Label detailsQualityLabel;
        private Label detailsConditionLabel;
        private Label detailsValueLabel;
        private Label detailsDescriptionLabel;
        private Label detailsActionMessageLabel;
        private VisualElement detailsIcon;
        private VisualElement detailsConditionFill;

        private Button useButton;
        private Button dropButton;
        private Button splitButton;
        private Button inspectButton;

        private VisualElement inspectModal;
        private VisualElement inspectIcon;
        private Label inspectNameLabel;
        private Label inspectCategoryLabel;
        private Label inspectDescriptionLabel;
        private Label inspectMetadataLabel;
        private Button inspectCloseButton;

        private readonly Dictionary<ItemDefinition, Texture2D> iconCache =
            new Dictionary<ItemDefinition, Texture2D>();

        private bool uiReady;
        private bool inventoryOpen;
        private bool inspectOpen;
        private bool controllerWasEnabled;
        private bool hotbarWasEnabled;
        private int selectedInventoryIndex;

        public bool IsOpen => inventoryOpen;

        private void Awake()
        {
            ResolveReferences();
            EnsureDocument();
            LoadAssets();
        }

        private void OnEnable()
        {
            ResolveReferences();
            BindDataSources();
            EnsureDocument();
            LoadAssets();

            if (uiDocument != null)
            {
                uiDocument.rootVisualElement.schedule.Execute(BuildUI);
            }
        }

        private void OnDisable()
        {
            UnbindDataSources();

            if (inventoryOpen)
            {
                SetInventoryOpen(false, true);
            }

            uiReady = false;
        }

        private void OnDestroy()
        {
            if (runtimePanelSettings != null)
            {
                Destroy(runtimePanelSettings);
            }
        }

        private void Update()
        {
            ResolveReferences();
            BindDataSources();

            if (!uiReady)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (
                keyboard.tabKey.wasPressedThisFrame ||
                keyboard.iKey.wasPressedThisFrame
            )
            {
                SetInventoryOpen(!inventoryOpen);
                return;
            }

            if (!inventoryOpen)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (inspectOpen)
                {
                    CloseInspectModal();
                }
                else
                {
                    SetInventoryOpen(false);
                }

                return;
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                UseSelectedItem();
            }
            else if (keyboard.qKey.wasPressedThisFrame)
            {
                DropSelectedItem();
            }
            else if (keyboard.rKey.wasPressedThisFrame)
            {
                SplitSelectedStack();
            }

            HandleInventoryHotbarInput(keyboard);
        }

        private void EnsureDocument()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }

            if (uiDocument.panelSettings != null)
            {
                return;
            }

            runtimePanelSettings =
                ScriptableObject.CreateInstance<PanelSettings>();

            runtimePanelSettings.name =
                "Shadow Supply Runtime Inventory Panel";

            runtimePanelSettings.hideFlags =
                HideFlags.HideAndDontSave;

            runtimePanelSettings.scaleMode =
                PanelScaleMode.ScaleWithScreenSize;

            runtimePanelSettings.referenceResolution =
                new Vector2Int(1920, 1080);

            runtimePanelSettings.screenMatchMode =
                PanelScreenMatchMode.MatchWidthOrHeight;

            runtimePanelSettings.match = 0.5f;
            runtimePanelSettings.sortingOrder = 200;

            uiDocument.panelSettings = runtimePanelSettings;
        }

        private void LoadAssets()
        {
            layoutAsset ??=
                Resources.Load<VisualTreeAsset>(
                    LayoutResourcePath
                );

            styleAsset ??=
                Resources.Load<StyleSheet>(
                    StyleResourcePath
                );

            if (layoutAsset == null)
            {
                Debug.LogError(
                    $"[InventoryHUD] Missing UI layout at Resources/{LayoutResourcePath}.uxml",
                    this
                );
            }

            if (styleAsset == null)
            {
                Debug.LogError(
                    $"[InventoryHUD] Missing UI stylesheet at Resources/{StyleResourcePath}.uss",
                    this
                );
            }
        }

        private void BuildUI()
        {
            if (
                uiDocument == null ||
                layoutAsset == null
            )
            {
                return;
            }

            root = uiDocument.rootVisualElement;
            root.Clear();

            root.style.position = Position.Absolute;
            root.style.left = 0f;
            root.style.right = 0f;
            root.style.top = 0f;
            root.style.bottom = 0f;
            root.style.flexGrow = 1f;

            TemplateContainer template = layoutAsset.Instantiate();
            template.name = "shadow-supply-inventory-template";
            template.style.position = Position.Absolute;
            template.style.left = 0f;
            template.style.right = 0f;
            template.style.top = 0f;
            template.style.bottom = 0f;
            template.style.flexGrow = 1f;

            if (styleAsset != null)
            {
                template.styleSheets.Add(styleAsset);
            }

            root.pickingMode = PickingMode.Position;
            template.pickingMode = PickingMode.Position;
            root.Add(template);

            CacheVisualElements();
            ConfigurePointerInteraction();
            ApplyRuntimeStyles();
            RegisterButtonCallbacks();

            selectedInventoryIndex =
                hotbar != null
                    ? hotbar.SelectedIndex
                    : 0;

            uiReady = true;
            SetInventoryOpen(false, true);
            RefreshAll();
        }

        private void CacheVisualElements()
        {
            inventoryUiRoot =
                RequireElement<VisualElement>("inventory-ui-root");

            inventoryScreen =
                RequireElement<VisualElement>("inventory-screen");

            compactHotbarShell =
                RequireElement<VisualElement>("compact-hotbar-shell");

            inventoryGrid =
                RequireElement<VisualElement>("inventory-grid");

            inventoryHotbarGrid =
                RequireElement<VisualElement>("inventory-hotbar-grid");

            compactHotbarGrid =
                RequireElement<VisualElement>("compact-hotbar-grid");

            inventoryCountLabel =
                RequireElement<Label>("inventory-count");

            cashLabel =
                RequireElement<Label>("cash-value");

            dirtyCashLabel =
                RequireElement<Label>("dirty-cash-value");

            heatLabel =
                RequireElement<Label>("heat-value");

            weightLabel =
                RequireElement<Label>("weight-value");

            detailsNameLabel =
                RequireElement<Label>("details-name");

            detailsQuantityLabel =
                RequireElement<Label>("details-quantity");

            detailsCategoryLabel =
                RequireElement<Label>("details-category");

            detailsQualityLabel =
                RequireElement<Label>("details-quality");

            detailsConditionLabel =
                RequireElement<Label>("details-condition");

            detailsValueLabel =
                RequireElement<Label>("details-value");

            detailsDescriptionLabel =
                RequireElement<Label>("details-description");

            detailsActionMessageLabel =
                RequireElement<Label>("details-action-message");

            detailsIcon =
                RequireElement<VisualElement>("details-icon");

            detailsConditionFill =
                RequireElement<VisualElement>("details-condition-fill");

            useButton =
                RequireElement<Button>("details-use-button");

            dropButton =
                RequireElement<Button>("details-drop-button");

            splitButton =
                RequireElement<Button>("details-split-button");

            inspectButton =
                RequireElement<Button>("details-inspect-button");

            inspectModal =
                RequireElement<VisualElement>("inspect-modal");

            inspectIcon =
                RequireElement<VisualElement>("inspect-icon");

            inspectNameLabel =
                RequireElement<Label>("inspect-name");

            inspectCategoryLabel =
                RequireElement<Label>("inspect-category");

            inspectDescriptionLabel =
                RequireElement<Label>("inspect-description");

            inspectMetadataLabel =
                RequireElement<Label>("inspect-metadata");

            inspectCloseButton =
                RequireElement<Button>("inspect-close-button");
        }

        /// <summary>
        /// Ensures the full inventory hierarchy participates in UI Toolkit
        /// picking. The original UXML marked the inventory root as Ignore,
        /// which prevented pointer hits from reaching slot buttons.
        /// </summary>
        private void ConfigurePointerInteraction()
        {
            if (root == null)
            {
                return;
            }

            root.pickingMode = PickingMode.Position;

            if (inventoryUiRoot != null)
            {
                inventoryUiRoot.pickingMode = PickingMode.Position;
            }

            if (inventoryScreen != null)
            {
                inventoryScreen.pickingMode = PickingMode.Position;
                inventoryScreen.BringToFront();
            }

            VisualElement frame =
                root.Q<VisualElement>(
                    className: "inventory-frame"
                );

            if (frame != null)
            {
                frame.pickingMode = PickingMode.Position;
            }

            if (inventoryGrid != null)
            {
                inventoryGrid.pickingMode = PickingMode.Position;
            }

            if (inventoryHotbarGrid != null)
            {
                inventoryHotbarGrid.pickingMode = PickingMode.Position;
            }

            VisualElement backdrop =
                root.Q<VisualElement>(
                    className: "inventory-backdrop"
                );

            if (backdrop != null)
            {
                backdrop.pickingMode = PickingMode.Ignore;
            }

            if (inspectModal != null)
            {
                inspectModal.pickingMode = PickingMode.Position;
            }
        }

        private void RegisterButtonCallbacks()
        {
            useButton.pickingMode = PickingMode.Position;
            dropButton.pickingMode = PickingMode.Position;
            splitButton.pickingMode = PickingMode.Position;
            inspectButton.pickingMode = PickingMode.Position;
            inspectCloseButton.pickingMode = PickingMode.Position;

            useButton.clicked += UseSelectedItem;
            dropButton.clicked += DropSelectedItem;
            splitButton.clicked += SplitSelectedStack;
            inspectButton.clicked += OpenInspectModal;
            inspectCloseButton.clicked += CloseInspectModal;

            inspectModal.RegisterCallback<PointerDownEvent>(
                HandleInspectBackdropPointerDown
            );
        }

        private void HandleInspectBackdropPointerDown(
            PointerDownEvent evt
        )
        {
            if (evt.target == inspectModal)
            {
                CloseInspectModal();
            }
        }

        private T RequireElement<T>(string elementName)
            where T : VisualElement
        {
            T element = root.Q<T>(elementName);

            if (element == null)
            {
                throw new InvalidOperationException(
                    $"Inventory UI element '{elementName}' was not found."
                );
            }

            return element;
        }

        private void SetInventoryOpen(
            bool open,
            bool force = false
        )
        {
            if (!uiReady && !force)
            {
                return;
            }

            if (!force && inventoryOpen == open)
            {
                return;
            }

            inventoryOpen = open;

            if (inventoryScreen != null)
            {
                inventoryScreen.style.display =
                    open
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
            }

            if (compactHotbarShell != null)
            {
                compactHotbarShell.style.display =
                    open
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;
            }

            if (open)
            {
                ConfigurePointerInteraction();

                if (inventoryScreen != null)
                {
                    inventoryScreen.BringToFront();
                }

                root?.Focus();

                controllerWasEnabled =
                    firstPersonController != null &&
                    firstPersonController.enabled;

                hotbarWasEnabled =
                    hotbar != null &&
                    hotbar.enabled;

                if (firstPersonController != null)
                {
                    firstPersonController.enabled = false;
                }

                if (hotbar != null)
                {
                    hotbar.enabled = false;
                }

                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;

                selectedInventoryIndex =
                    hotbar != null
                        ? hotbar.SelectedIndex
                        : selectedInventoryIndex;

                SetActionMessage(
                    "Select an item to view available actions."
                );
            }
            else
            {
                CloseInspectModal();

                if (
                    firstPersonController != null &&
                    controllerWasEnabled
                )
                {
                    firstPersonController.enabled = true;
                }

                if (hotbar != null && hotbarWasEnabled)
                {
                    hotbar.enabled = true;
                }

                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }

            RefreshAll();
        }


        private static readonly Color RuntimeBackground =
            new Color(0.018f, 0.023f, 0.025f, 0.98f);

        private static readonly Color RuntimePanel =
            new Color(0.045f, 0.057f, 0.06f, 0.98f);

        private static readonly Color RuntimePanelRaised =
            new Color(0.07f, 0.082f, 0.084f, 0.98f);

        private static readonly Color RuntimeSlot =
            new Color(0.075f, 0.086f, 0.088f, 0.99f);

        private static readonly Color RuntimeBorder =
            new Color(0.31f, 0.34f, 0.34f, 0.8f);

        private static readonly Color RuntimeAmber =
            new Color(0.95f, 0.52f, 0.08f, 1f);

        private static readonly Color RuntimeAmberDark =
            new Color(0.34f, 0.18f, 0.035f, 0.98f);

        private static readonly Color RuntimeTeal =
            new Color(0.31f, 0.72f, 0.67f, 1f);

        private static readonly Color RuntimeText =
            new Color(0.92f, 0.93f, 0.91f, 1f);

        private static readonly Color RuntimeMutedText =
            new Color(0.63f, 0.66f, 0.65f, 1f);

        /// <summary>
        /// Applies the complete inventory appearance through inline UI Toolkit
        /// styles. The interface no longer depends on USS parsing to establish
        /// its layout, colors, sizing, or presentation.
        /// </summary>
        private void ApplyRuntimeStyles()
        {
            if (root == null)
            {
                return;
            }

            SetFullscreen(root);
            root.style.backgroundColor =
                new StyleColor(Color.clear);

            VisualElement uiRoot =
                root.Q<VisualElement>("inventory-ui-root");

            SetFullscreen(uiRoot);

            if (uiRoot != null)
            {
                uiRoot.style.color =
                    new StyleColor(RuntimeText);
            }

            StyleCompactHotbar();
            StyleInventoryScreen();
            StyleHeader();
            StyleMainColumns();
            StyleEquipmentPanel();
            StyleDetailsPanel();
            StyleFooter();
            StyleInspectModal();
        }

        private void StyleCompactHotbar()
        {
            if (compactHotbarShell == null ||
                compactHotbarGrid == null)
            {
                return;
            }

            compactHotbarShell.style.position =
                Position.Absolute;
            compactHotbarShell.style.left =
                Length.Percent(22f);
            compactHotbarShell.style.right =
                Length.Percent(22f);
            compactHotbarShell.style.bottom = 18f;
            compactHotbarShell.style.height = 100f;
            compactHotbarShell.style.paddingLeft = 10f;
            compactHotbarShell.style.paddingRight = 10f;
            compactHotbarShell.style.paddingTop = 7f;
            compactHotbarShell.style.paddingBottom = 8f;
            compactHotbarShell.style.flexDirection =
                FlexDirection.Column;
            compactHotbarShell.style.backgroundColor =
                new StyleColor(RuntimeBackground);
            SetBorder(
                compactHotbarShell,
                new Color(
                    RuntimeAmber.r,
                    RuntimeAmber.g,
                    RuntimeAmber.b,
                    0.65f
                ),
                1f
            );

            VisualElement compactTitle =
                root.Q<VisualElement>(
                    className: "compact-hotbar-title"
                );

            if (compactTitle != null)
            {
                compactTitle.style.height = 18f;
                compactTitle.style.fontSize = 12f;
                compactTitle.style.color =
                    new StyleColor(RuntimeMutedText);
                compactTitle.style.unityFontStyleAndWeight =
                    FontStyle.Bold;
                compactTitle.style.unityTextAlign =
                    TextAnchor.MiddleLeft;
            }

            compactHotbarGrid.style.flexGrow = 1f;
            compactHotbarGrid.style.flexDirection =
                FlexDirection.Row;
            compactHotbarGrid.style.justifyContent =
                Justify.Center;
            compactHotbarGrid.style.alignItems =
                Align.Center;
        }

        private void StyleInventoryScreen()
        {
            SetFullscreen(inventoryScreen);

            if (inventoryScreen != null)
            {
                inventoryScreen.style.paddingLeft = 16f;
                inventoryScreen.style.paddingRight = 16f;
                inventoryScreen.style.paddingTop = 14f;
                inventoryScreen.style.paddingBottom = 14f;
            }

            VisualElement backdrop =
                root.Q<VisualElement>(
                    className: "inventory-backdrop"
                );

            SetFullscreen(backdrop);

            if (backdrop != null)
            {
                backdrop.style.backgroundColor =
                    new StyleColor(
                        new Color(0.005f, 0.007f, 0.008f, 0.93f)
                    );
            }

            VisualElement frame =
                root.Q<VisualElement>(
                    className: "inventory-frame"
                );

            if (frame != null)
            {
                frame.style.flexGrow = 1f;
                frame.style.flexDirection =
                    FlexDirection.Column;
                frame.style.paddingLeft = 20f;
                frame.style.paddingRight = 20f;
                frame.style.paddingTop = 12f;
                frame.style.paddingBottom = 12f;
                frame.style.backgroundColor =
                    new StyleColor(RuntimeBackground);
                SetBorder(
                    frame,
                    new Color(
                        RuntimeAmber.r,
                        RuntimeAmber.g,
                        RuntimeAmber.b,
                        0.72f
                    ),
                    1f
                );
            }
        }

        private void StyleHeader()
        {
            VisualElement header =
                root.Q<VisualElement>(
                    className: "inventory-header"
                );

            if (header != null)
            {
                header.style.height = 88f;
                header.style.flexShrink = 0f;
                header.style.flexDirection =
                    FlexDirection.Row;
                header.style.alignItems = Align.Center;
                header.style.justifyContent =
                    Justify.SpaceBetween;
                header.style.borderBottomWidth = 1f;
                header.style.borderBottomColor =
                    new StyleColor(
                        new Color(
                            RuntimeAmber.r,
                            RuntimeAmber.g,
                            RuntimeAmber.b,
                            0.5f
                        )
                    );
            }

            VisualElement titleGroup =
                root.Q<VisualElement>(
                    className: "inventory-title-group"
                );

            if (titleGroup != null)
            {
                titleGroup.style.width =
                    Length.Percent(38f);
                titleGroup.style.flexDirection =
                    FlexDirection.Row;
                titleGroup.style.alignItems =
                    Align.Center;
            }

            StyleClass(
                "inventory-chevron",
                element =>
                {
                    element.style.fontSize = 31f;
                    element.style.color =
                        new StyleColor(RuntimeAmber);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    element.style.marginRight = 12f;
                }
            );

            StyleClass(
                "inventory-title",
                element =>
                {
                    element.style.fontSize = 44f;
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            VisualElement stats =
                root.Q<VisualElement>(
                    className: "inventory-stats"
                );

            if (stats != null)
            {
                stats.style.width =
                    Length.Percent(62f);
                stats.style.height = 70f;
                stats.style.flexDirection =
                    FlexDirection.Row;
                stats.style.justifyContent =
                    Justify.FlexEnd;
                stats.style.alignItems =
                    Align.Center;
            }

            StyleClass(
                "header-stat",
                element =>
                {
                    element.style.width =
                        Length.Percent(24f);
                    element.style.height = 58f;
                    element.style.flexDirection =
                        FlexDirection.Row;
                    element.style.alignItems =
                        Align.Center;
                    element.style.paddingLeft = 12f;
                    element.style.borderLeftWidth = 1f;
                    element.style.borderLeftColor =
                        new StyleColor(
                            new Color(
                                RuntimeBorder.r,
                                RuntimeBorder.g,
                                RuntimeBorder.b,
                                0.45f
                            )
                        );
                }
            );

            StyleClass(
                "header-stat-icon",
                element =>
                {
                    element.style.width = 38f;
                    element.style.height = 38f;
                    element.style.marginRight = 10f;
                    element.style.backgroundColor =
                        new StyleColor(RuntimePanelRaised);
                    element.style.color =
                        new StyleColor(RuntimeTeal);
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    element.style.fontSize = 20f;
                    SetBorder(
                        element,
                        new Color(
                            RuntimeTeal.r,
                            RuntimeTeal.g,
                            RuntimeTeal.b,
                            0.55f
                        ),
                        1f
                    );
                }
            );

            StyleClass(
                "header-stat-copy",
                element =>
                {
                    element.style.flexGrow = 1f;
                    element.style.flexDirection =
                        FlexDirection.Column;
                    element.style.justifyContent =
                        Justify.Center;
                }
            );

            StyleClass(
                "header-stat-label",
                element =>
                {
                    element.style.fontSize = 12f;
                    element.style.color =
                        new StyleColor(RuntimeMutedText);
                }
            );

            StyleClass(
                "header-stat-value",
                element =>
                {
                    element.style.fontSize = 23f;
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            StyleClass(
                "heat-value",
                element =>
                {
                    element.style.color =
                        new StyleColor(
                            new Color(0.9f, 0.25f, 0.14f, 1f)
                        );
                }
            );

            StyleClass(
                "weight-value",
                element =>
                {
                    element.style.color =
                        new StyleColor(RuntimeTeal);
                }
            );
        }

        private void StyleMainColumns()
        {
            VisualElement main =
                root.Q<VisualElement>(
                    className: "inventory-main"
                );

            if (main != null)
            {
                main.style.flexGrow = 1f;
                main.style.flexDirection =
                    FlexDirection.Row;
                main.style.marginTop = 10f;
                main.style.marginBottom = 9f;
                main.style.overflow = Overflow.Hidden;
            }

            VisualElement left =
                root.Q<VisualElement>(
                    className: "inventory-left-column"
                );

            if (left != null)
            {
                left.style.width =
                    Length.Percent(64f);
                left.style.flexDirection =
                    FlexDirection.Column;
            }

            VisualElement right =
                root.Q<VisualElement>(
                    className: "inventory-right-column"
                );

            if (right != null)
            {
                right.style.flexGrow = 1f;
                right.style.marginLeft = 12f;
                right.style.flexDirection =
                    FlexDirection.Column;
            }

            StyleClass(
                "industrial-panel",
                element =>
                {
                    element.style.backgroundColor =
                        new StyleColor(RuntimePanel);
                    element.style.paddingLeft = 12f;
                    element.style.paddingRight = 12f;
                    element.style.paddingTop = 10f;
                    element.style.paddingBottom = 10f;
                    SetBorder(
                        element,
                        RuntimeBorder,
                        1f
                    );
                }
            );

            VisualElement gridPanel =
                root.Q<VisualElement>(
                    className: "inventory-grid-panel"
                );

            if (gridPanel != null)
            {
                gridPanel.style.flexGrow = 1f;
                gridPanel.style.overflow =
                    Overflow.Hidden;
            }

            VisualElement panelHeader =
                root.Q<VisualElement>(
                    className: "panel-header-row"
                );

            if (panelHeader != null)
            {
                panelHeader.style.height = 30f;
                panelHeader.style.flexShrink = 0f;
                panelHeader.style.flexDirection =
                    FlexDirection.Row;
                panelHeader.style.alignItems =
                    Align.Center;
            }

            StyleClass(
                "panel-title",
                element =>
                {
                    element.style.fontSize = 18f;
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            StyleClass(
                "panel-count",
                element =>
                {
                    element.style.fontSize = 17f;
                    element.style.color =
                        new StyleColor(RuntimeTeal);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    element.style.marginLeft = 10f;
                }
            );

            if (inventoryGrid != null)
            {
                inventoryGrid.style.flexGrow = 1f;
                inventoryGrid.style.flexDirection =
                    FlexDirection.Row;
                inventoryGrid.style.flexWrap =
                    Wrap.Wrap;
                inventoryGrid.style.alignContent =
                    Align.FlexStart;
                inventoryGrid.style.marginTop = 5f;
                inventoryGrid.style.overflow =
                    Overflow.Hidden;
            }

            VisualElement hotbarPanel =
                root.Q<VisualElement>(
                    className: "inventory-hotbar-panel"
                );

            if (hotbarPanel != null)
            {
                hotbarPanel.style.height = 116f;
                hotbarPanel.style.flexShrink = 0f;
                hotbarPanel.style.marginTop = 10f;
            }

            if (inventoryHotbarGrid != null)
            {
                inventoryHotbarGrid.style.flexGrow = 1f;
                inventoryHotbarGrid.style.flexDirection =
                    FlexDirection.Row;
                inventoryHotbarGrid.style.alignItems =
                    Align.Center;
                inventoryHotbarGrid.style.justifyContent =
                    Justify.SpaceBetween;
                inventoryHotbarGrid.style.marginTop = 6f;
            }
        }

        private void StyleEquipmentPanel()
        {
            VisualElement panel =
                root.Q<VisualElement>(
                    className: "equipment-panel"
                );

            if (panel != null)
            {
                panel.style.height = 235f;
                panel.style.flexShrink = 0f;
                panel.style.flexDirection =
                    FlexDirection.Row;
                panel.style.alignItems = Align.Center;
                panel.style.justifyContent =
                    Justify.SpaceBetween;
            }

            StyleClass(
                "equipment-left-slots",
                element =>
                {
                    element.style.width =
                        Length.Percent(28f);
                    element.style.height =
                        Length.Percent(100f);
                    element.style.justifyContent =
                        Justify.SpaceAround;
                }
            );

            StyleClass(
                "equipment-right-slots",
                element =>
                {
                    element.style.width =
                        Length.Percent(28f);
                    element.style.height =
                        Length.Percent(100f);
                    element.style.justifyContent =
                        Justify.SpaceAround;
                }
            );

            StyleClass(
                "equipment-slot-group",
                element =>
                {
                    element.style.alignItems =
                        Align.Center;
                }
            );

            StyleClass(
                "equipment-label",
                element =>
                {
                    element.style.fontSize = 12f;
                    element.style.color =
                        new StyleColor(RuntimeMutedText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.marginBottom = 4f;
                }
            );

            StyleClass(
                "equipment-slot",
                element =>
                {
                    element.style.width = 80f;
                    element.style.height = 74f;
                    element.style.backgroundColor =
                        new StyleColor(
                            new Color(0.035f, 0.043f, 0.045f, 1f)
                        );
                    SetBorder(
                        element,
                        RuntimeBorder,
                        1f
                    );
                }
            );

            StyleClass(
                "equipment-glyph",
                element =>
                {
                    element.style.position =
                        Position.Absolute;
                    element.style.left = 0f;
                    element.style.right = 0f;
                    element.style.top = 0f;
                    element.style.bottom = 0f;
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.fontSize = 25f;
                    element.style.color =
                        new StyleColor(
                            new Color(0.25f, 0.28f, 0.28f, 1f)
                        );
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            VisualElement doll =
                root.Q<VisualElement>(
                    className: "paper-doll"
                );

            if (doll != null)
            {
                doll.style.width =
                    Length.Percent(36f);
                doll.style.height =
                    Length.Percent(100f);
                doll.style.position =
                    Position.Relative;
            }

            StyleDollPart(
                "doll-head",
                38f,
                38f,
                Length.Percent(50f),
                12f
            );

            VisualElement body =
                root.Q<VisualElement>(
                    className: "doll-body"
                );

            if (body != null)
            {
                body.style.position = Position.Absolute;
                body.style.width = 64f;
                body.style.height = 92f;
                body.style.left =
                    Length.Percent(50f);
                body.style.top = 50f;
                body.style.marginLeft = -32f;
                body.style.backgroundColor =
                    new StyleColor(
                        new Color(0.15f, 0.17f, 0.17f, 1f)
                    );
            }

            StyleClass(
                "doll-arm",
                element =>
                {
                    element.style.position =
                        Position.Absolute;
                    element.style.width = 17f;
                    element.style.height = 92f;
                    element.style.top = 57f;
                    element.style.backgroundColor =
                        new StyleColor(
                            new Color(0.15f, 0.17f, 0.17f, 1f)
                        );
                }
            );

            VisualElement leftArm =
                root.Q<VisualElement>(
                    className: "doll-arm-left"
                );

            if (leftArm != null)
            {
                leftArm.style.left =
                    Length.Percent(28f);
            }

            VisualElement rightArm =
                root.Q<VisualElement>(
                    className: "doll-arm-right"
                );

            if (rightArm != null)
            {
                rightArm.style.right =
                    Length.Percent(28f);
            }

            StyleClass(
                "doll-leg",
                element =>
                {
                    element.style.position =
                        Position.Absolute;
                    element.style.width = 23f;
                    element.style.height = 76f;
                    element.style.top = 137f;
                    element.style.backgroundColor =
                        new StyleColor(
                            new Color(0.15f, 0.17f, 0.17f, 1f)
                        );
                }
            );

            VisualElement leftLeg =
                root.Q<VisualElement>(
                    className: "doll-leg-left"
                );

            if (leftLeg != null)
            {
                leftLeg.style.left =
                    Length.Percent(39f);
            }

            VisualElement rightLeg =
                root.Q<VisualElement>(
                    className: "doll-leg-right"
                );

            if (rightLeg != null)
            {
                rightLeg.style.right =
                    Length.Percent(39f);
            }
        }

        private void StyleDollPart(
            string className,
            float width,
            float height,
            Length left,
            float top
        )
        {
            VisualElement element =
                root.Q<VisualElement>(
                    className: className
                );

            if (element == null)
            {
                return;
            }

            element.style.position = Position.Absolute;
            element.style.width = width;
            element.style.height = height;
            element.style.left = left;
            element.style.top = top;
            element.style.marginLeft = -width * 0.5f;
            element.style.backgroundColor =
                new StyleColor(
                    new Color(0.15f, 0.17f, 0.17f, 1f)
                );
        }

        private void StyleDetailsPanel()
        {
            VisualElement panel =
                root.Q<VisualElement>(
                    className: "details-panel"
                );

            if (panel != null)
            {
                panel.style.flexGrow = 1f;
                panel.style.marginTop = 10f;
                panel.style.borderTopColor =
                    new StyleColor(RuntimeAmber);
            }

            VisualElement titleRow =
                root.Q<VisualElement>(
                    className: "details-title-row"
                );

            if (titleRow != null)
            {
                titleRow.style.height = 70f;
                titleRow.style.flexShrink = 0f;
                titleRow.style.flexDirection =
                    FlexDirection.Row;
                titleRow.style.justifyContent =
                    Justify.SpaceBetween;
                titleRow.style.alignItems =
                    Align.Center;
                titleRow.style.borderBottomWidth = 1f;
                titleRow.style.borderBottomColor =
                    new StyleColor(RuntimeBorder);
            }

            StyleClass(
                "details-title-copy",
                element =>
                {
                    element.style.flexGrow = 1f;
                }
            );

            StyleClass(
                "details-name",
                element =>
                {
                    element.style.fontSize = 27f;
                    element.style.color =
                        new StyleColor(RuntimeAmber);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            StyleClass(
                "details-category",
                element =>
                {
                    element.style.alignSelf =
                        Align.FlexStart;
                    element.style.marginTop = 4f;
                    element.style.paddingLeft = 10f;
                    element.style.paddingRight = 10f;
                    element.style.paddingTop = 3f;
                    element.style.paddingBottom = 3f;
                    element.style.backgroundColor =
                        new StyleColor(
                            new Color(0.12f, 0.31f, 0.28f, 1f)
                        );
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.fontSize = 13f;
                }
            );

            StyleClass(
                "details-quantity",
                element =>
                {
                    element.style.fontSize = 35f;
                    element.style.color =
                        new StyleColor(RuntimeAmber);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            VisualElement content =
                root.Q<VisualElement>(
                    className: "details-content-row"
                );

            if (content != null)
            {
                content.style.height = 270f;
                content.style.flexShrink = 0f;
                content.style.flexDirection =
                    FlexDirection.Row;
                content.style.marginTop = 10f;
                content.style.marginBottom = 10f;
            }

            if (detailsIcon != null)
            {
                detailsIcon.style.width =
                    Length.Percent(44f);
                detailsIcon.style.height =
                    Length.Percent(100f);
                detailsIcon.style.position =
                    Position.Relative;
                detailsIcon.style.backgroundColor =
                    new StyleColor(
                        new Color(0.035f, 0.045f, 0.047f, 1f)
                    );
                SetBorder(
                    detailsIcon,
                    RuntimeBorder,
                    1f
                );
            }

            VisualElement statColumn =
                root.Q<VisualElement>(
                    className: "details-stat-column"
                );

            if (statColumn != null)
            {
                statColumn.style.flexGrow = 1f;
                statColumn.style.marginLeft = 12f;
                statColumn.style.justifyContent =
                    Justify.SpaceAround;
            }

            StyleClass(
                "details-stat-row",
                element =>
                {
                    element.style.minHeight = 56f;
                    element.style.paddingLeft = 8f;
                    element.style.paddingRight = 8f;
                    element.style.paddingTop = 8f;
                    element.style.paddingBottom = 8f;
                    element.style.borderBottomWidth = 1f;
                    element.style.borderBottomColor =
                        new StyleColor(
                            new Color(
                                RuntimeBorder.r,
                                RuntimeBorder.g,
                                RuntimeBorder.b,
                                0.5f
                            )
                        );
                    element.style.justifyContent =
                        Justify.Center;
                }
            );

            StyleClass(
                "details-stat-topline",
                element =>
                {
                    element.style.flexDirection =
                        FlexDirection.Row;
                    element.style.justifyContent =
                        Justify.SpaceBetween;
                }
            );

            StyleClass(
                "details-stat-label",
                element =>
                {
                    element.style.fontSize = 13f;
                    element.style.color =
                        new StyleColor(RuntimeMutedText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            StyleClass(
                "details-stat-value",
                element =>
                {
                    element.style.fontSize = 16f;
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            StyleClass(
                "quality-value",
                element =>
                {
                    element.style.color =
                        new StyleColor(RuntimeTeal);
                }
            );

            VisualElement conditionBar =
                root.Q<VisualElement>(
                    className: "condition-bar"
                );

            if (conditionBar != null)
            {
                conditionBar.style.height = 9f;
                conditionBar.style.marginTop = 8f;
                conditionBar.style.backgroundColor =
                    new StyleColor(
                        new Color(0.12f, 0.14f, 0.14f, 1f)
                    );
            }

            if (detailsConditionFill != null)
            {
                detailsConditionFill.style.height =
                    Length.Percent(100f);
                detailsConditionFill.style.backgroundColor =
                    new StyleColor(RuntimeTeal);
            }

            StyleClass(
                "details-description",
                element =>
                {
                    element.style.flexGrow = 1f;
                    element.style.minHeight = 64f;
                    element.style.paddingTop = 10f;
                    element.style.paddingBottom = 8f;
                    element.style.fontSize = 15f;
                    element.style.color =
                        new StyleColor(
                            new Color(0.78f, 0.8f, 0.78f, 1f)
                        );
                    element.style.whiteSpace =
                        WhiteSpace.Normal;
                }
            );

            StyleClass(
                "details-action-message",
                element =>
                {
                    element.style.height = 28f;
                    element.style.fontSize = 13f;
                    element.style.color =
                        new StyleColor(RuntimeTeal);
                    element.style.unityTextAlign =
                        TextAnchor.MiddleLeft;
                }
            );

            VisualElement actionRow =
                root.Q<VisualElement>(
                    className: "details-action-row"
                );

            if (actionRow != null)
            {
                actionRow.style.height = 58f;
                actionRow.style.flexShrink = 0f;
                actionRow.style.flexDirection =
                    FlexDirection.Row;
                actionRow.style.justifyContent =
                    Justify.SpaceBetween;
                actionRow.style.alignItems =
                    Align.Center;
            }

            StyleClass(
                "details-action-button",
                element =>
                {
                    element.style.width =
                        Length.Percent(23.5f);
                    element.style.height = 48f;
                    element.style.backgroundColor =
                        new StyleColor(RuntimePanelRaised);
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.fontSize = 14f;
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    SetBorder(
                        element,
                        RuntimeBorder,
                        1f
                    );
                }
            );

            StyleClass(
                "primary-action",
                element =>
                {
                    element.style.backgroundColor =
                        new StyleColor(RuntimeAmberDark);
                    element.style.color =
                        new StyleColor(RuntimeAmber);
                    SetBorder(
                        element,
                        RuntimeAmber,
                        1f
                    );
                }
            );
        }

        private void StyleFooter()
        {
            VisualElement footer =
                root.Q<VisualElement>(
                    className: "inventory-footer"
                );

            if (footer != null)
            {
                footer.style.height = 58f;
                footer.style.flexShrink = 0f;
                footer.style.flexDirection =
                    FlexDirection.Row;
                footer.style.alignItems =
                    Align.Center;
                footer.style.borderTopWidth = 1f;
                footer.style.borderTopColor =
                    new StyleColor(
                        new Color(
                            RuntimeAmber.r,
                            RuntimeAmber.g,
                            RuntimeAmber.b,
                            0.4f
                        )
                    );
            }

            StyleClass(
                "footer-hint",
                element =>
                {
                    element.style.flexDirection =
                        FlexDirection.Row;
                    element.style.alignItems =
                        Align.Center;
                    element.style.marginRight = 26f;
                }
            );

            StyleClass(
                "footer-key",
                element =>
                {
                    element.style.minWidth = 42f;
                    element.style.height = 30f;
                    element.style.paddingLeft = 9f;
                    element.style.paddingRight = 9f;
                    element.style.marginRight = 8f;
                    element.style.backgroundColor =
                        new StyleColor(RuntimePanelRaised);
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    SetBorder(
                        element,
                        RuntimeBorder,
                        1f
                    );
                }
            );

            StyleClass(
                "footer-copy",
                element =>
                {
                    element.style.fontSize = 13f;
                    element.style.color =
                        new StyleColor(RuntimeMutedText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );
        }

        private void StyleInspectModal()
        {
            if (inspectModal != null)
            {
                SetFullscreen(inspectModal);
                inspectModal.style.backgroundColor =
                    new StyleColor(
                        new Color(0f, 0f, 0f, 0.84f)
                    );
                inspectModal.style.justifyContent =
                    Justify.Center;
                inspectModal.style.alignItems =
                    Align.Center;
                inspectModal.style.display =
                    DisplayStyle.None;
            }

            VisualElement card =
                root.Q<VisualElement>(
                    className: "inspect-card"
                );

            if (card != null)
            {
                card.style.width = 620f;
                card.style.height = 760f;
                card.style.paddingLeft = 22f;
                card.style.paddingRight = 22f;
                card.style.paddingTop = 18f;
                card.style.paddingBottom = 20f;
                card.style.backgroundColor =
                    new StyleColor(RuntimeBackground);
                SetBorder(
                    card,
                    RuntimeAmber,
                    1f
                );
            }

            StyleClass(
                "inspect-card-header",
                element =>
                {
                    element.style.height = 48f;
                    element.style.flexDirection =
                        FlexDirection.Row;
                    element.style.justifyContent =
                        Justify.SpaceBetween;
                    element.style.alignItems =
                        Align.Center;
                }
            );

            StyleClass(
                "inspect-kicker",
                element =>
                {
                    element.style.fontSize = 13f;
                    element.style.color =
                        new StyleColor(RuntimeMutedText);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                }
            );

            StyleClass(
                "inspect-close-button",
                element =>
                {
                    element.style.width = 42f;
                    element.style.height = 38f;
                    element.style.backgroundColor =
                        new StyleColor(RuntimePanelRaised);
                    element.style.color =
                        new StyleColor(RuntimeText);
                    SetBorder(
                        element,
                        RuntimeBorder,
                        1f
                    );
                }
            );

            if (inspectIcon != null)
            {
                inspectIcon.style.height = 390f;
                inspectIcon.style.position =
                    Position.Relative;
                inspectIcon.style.marginTop = 8f;
                inspectIcon.style.marginBottom = 14f;
                inspectIcon.style.backgroundColor =
                    new StyleColor(RuntimePanel);
                SetBorder(
                    inspectIcon,
                    RuntimeBorder,
                    1f
                );
            }

            StyleClass(
                "inspect-name",
                element =>
                {
                    element.style.fontSize = 32f;
                    element.style.color =
                        new StyleColor(RuntimeAmber);
                    element.style.unityFontStyleAndWeight =
                        FontStyle.Bold;
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                }
            );

            StyleClass(
                "inspect-category",
                element =>
                {
                    element.style.fontSize = 14f;
                    element.style.color =
                        new StyleColor(RuntimeTeal);
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.marginTop = 4f;
                }
            );

            StyleClass(
                "inspect-description",
                element =>
                {
                    element.style.fontSize = 15f;
                    element.style.color =
                        new StyleColor(RuntimeText);
                    element.style.whiteSpace =
                        WhiteSpace.Normal;
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.marginTop = 14f;
                }
            );

            StyleClass(
                "inspect-metadata",
                element =>
                {
                    element.style.fontSize = 13f;
                    element.style.color =
                        new StyleColor(RuntimeMutedText);
                    element.style.unityTextAlign =
                        TextAnchor.MiddleCenter;
                    element.style.marginTop = 16f;
                }
            );
        }

        private void ApplySlotRuntimeStyle(
            Button button,
            Label indexLabel,
            VisualElement icon,
            Label nameLabel,
            Label quantityLabel,
            SlotPresentation presentation,
            bool isUiSelected,
            bool isEquipped,
            bool hasItem
        )
        {
            button.style.position = Position.Relative;
            button.style.flexShrink = 0f;
            button.style.paddingLeft = 5f;
            button.style.paddingRight = 5f;
            button.style.paddingTop = 5f;
            button.style.paddingBottom = 5f;
            button.style.backgroundColor =
                new StyleColor(
                    isUiSelected || isEquipped
                        ? RuntimeAmberDark
                        : RuntimeSlot
                );

            SetBorder(
                button,
                isUiSelected || isEquipped
                    ? RuntimeAmber
                    : RuntimeBorder,
                isUiSelected || isEquipped
                    ? 2f
                    : 1f
            );

            switch (presentation)
            {
                case SlotPresentation.Inventory:
                    button.style.width =
                        Length.Percent(15.25f);
                    button.style.height = 128f;
                    button.style.marginLeft = 4f;
                    button.style.marginRight = 4f;
                    button.style.marginTop = 4f;
                    button.style.marginBottom = 4f;
                    break;

                case SlotPresentation.InventoryHotbar:
                    button.style.width =
                        Length.Percent(11.7f);
                    button.style.height = 78f;
                    button.style.marginLeft = 2f;
                    button.style.marginRight = 2f;
                    break;

                case SlotPresentation.CompactHotbar:
                    button.style.width = 78f;
                    button.style.height = 66f;
                    button.style.marginLeft = 3f;
                    button.style.marginRight = 3f;
                    break;
            }

            indexLabel.style.position = Position.Absolute;
            indexLabel.style.left = 6f;
            indexLabel.style.top = 4f;
            indexLabel.style.fontSize =
                presentation == SlotPresentation.CompactHotbar
                    ? 11f
                    : 12f;
            indexLabel.style.color =
                new StyleColor(
                    isEquipped
                        ? RuntimeAmber
                        : RuntimeMutedText
                );
            indexLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            indexLabel.pickingMode = PickingMode.Ignore;

            icon.style.position = Position.Absolute;
            icon.style.left = 8f;
            icon.style.right = 8f;
            icon.style.top =
                presentation == SlotPresentation.Inventory
                    ? 22f
                    : 14f;
            icon.style.bottom =
                presentation == SlotPresentation.Inventory
                    ? 34f
                    : 10f;
            icon.style.overflow = Overflow.Hidden;
            icon.style.backgroundColor =
                new StyleColor(
                    hasItem
                        ? new Color(0.04f, 0.05f, 0.052f, 1f)
                        : new Color(0.035f, 0.04f, 0.042f, 1f)
                );
            icon.pickingMode = PickingMode.Ignore;

            nameLabel.style.position = Position.Absolute;
            nameLabel.style.left = 5f;
            nameLabel.style.right = 5f;
            nameLabel.style.bottom = 4f;
            nameLabel.style.height = 25f;
            nameLabel.style.fontSize = 11f;
            nameLabel.style.color =
                new StyleColor(RuntimeText);
            nameLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            nameLabel.style.unityTextAlign =
                TextAnchor.MiddleCenter;
            nameLabel.style.whiteSpace =
                WhiteSpace.Normal;
            nameLabel.pickingMode = PickingMode.Ignore;

            quantityLabel.style.position =
                Position.Absolute;
            quantityLabel.style.right = 6f;
            quantityLabel.style.top = 4f;
            quantityLabel.style.fontSize =
                presentation == SlotPresentation.CompactHotbar
                    ? 11f
                    : 12f;
            quantityLabel.style.color =
                new StyleColor(RuntimeAmber);
            quantityLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            quantityLabel.pickingMode = PickingMode.Ignore;

            if (presentation != SlotPresentation.Inventory)
            {
                nameLabel.style.display =
                    DisplayStyle.None;
            }
        }

        private void StyleClass(
            string className,
            Action<VisualElement> styleAction
        )
        {
            if (root == null ||
                string.IsNullOrWhiteSpace(className) ||
                styleAction == null)
            {
                return;
            }

            root.Query<VisualElement>(
                className: className
            ).ForEach(styleAction);
        }

        private static void SetFullscreen(
            VisualElement element
        )
        {
            if (element == null)
            {
                return;
            }

            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;
            element.style.flexGrow = 1f;
        }

        private static void SetBorder(
            VisualElement element,
            Color color,
            float width
        )
        {
            if (element == null)
            {
                return;
            }

            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;

            StyleColor styleColor =
                new StyleColor(color);

            element.style.borderLeftColor = styleColor;
            element.style.borderRightColor = styleColor;
            element.style.borderTopColor = styleColor;
            element.style.borderBottomColor = styleColor;
        }

        private void RefreshAll()
        {
            if (!uiReady)
            {
                return;
            }

            RefreshStats();
            RebuildInventoryGrid();
            RebuildInventoryHotbar();
            RebuildCompactHotbar();
            RefreshDetails();
        }

        private void RefreshStats()
        {
            if (inventory == null)
            {
                return;
            }

            int occupiedSlots = 0;

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                if (!inventory.GetSlot(i).IsEmpty)
                {
                    occupiedSlots++;
                }
            }

            inventoryCountLabel.text =
                $"{occupiedSlots} / {inventory.SlotCount}";

            cashLabel.text = $"${cash:N0}";
            dirtyCashLabel.text = $"${dirtyCash:N0}";
            heatLabel.text = $"{heatPercent:0}%";

            weightLabel.text =
                $"{inventory.GetTotalQuantity()} / {weightCapacity}";
        }

        private void RebuildInventoryGrid()
        {
            inventoryGrid.Clear();

            if (inventory == null)
            {
                return;
            }

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                inventoryGrid.Add(
                    CreateSlotButton(
                        i,
                        SlotPresentation.Inventory
                    )
                );
            }
        }

        private void RebuildInventoryHotbar()
        {
            inventoryHotbarGrid.Clear();

            if (inventory == null)
            {
                return;
            }

            for (int i = 0; i < inventory.HotbarSize; i++)
            {
                inventoryHotbarGrid.Add(
                    CreateSlotButton(
                        i,
                        SlotPresentation.InventoryHotbar
                    )
                );
            }
        }

        private void RebuildCompactHotbar()
        {
            compactHotbarGrid.Clear();

            if (inventory == null)
            {
                return;
            }

            for (int i = 0; i < inventory.HotbarSize; i++)
            {
                compactHotbarGrid.Add(
                    CreateSlotButton(
                        i,
                        SlotPresentation.CompactHotbar
                    )
                );
            }
        }

        private Button CreateSlotButton(
            int index,
            SlotPresentation presentation
        )
        {
            InventorySlot slot = inventory.GetSlot(index);
            Button button = new Button();
            button.text = string.Empty;
            button.focusable = false;
            button.name = $"inventory-slot-{index}";

            switch (presentation)
            {
                case SlotPresentation.Inventory:
                    button.AddToClassList("inventory-slot");
                    break;

                case SlotPresentation.InventoryHotbar:
                    button.AddToClassList("inventory-hotbar-slot");
                    break;

                case SlotPresentation.CompactHotbar:
                    button.AddToClassList("compact-hotbar-slot");
                    button.pickingMode = PickingMode.Ignore;
                    break;
            }

            bool isUiSelected =
                index == selectedInventoryIndex;

            bool isEquipped =
                hotbar != null &&
                index == hotbar.SelectedIndex &&
                index < inventory.HotbarSize;

            SetClass(
                button,
                "is-selected",
                isUiSelected &&
                presentation != SlotPresentation.CompactHotbar
            );

            SetClass(
                button,
                "is-equipped",
                isEquipped
            );

            Label indexLabel = new Label(
                index < inventory.HotbarSize
                    ? (index + 1).ToString()
                    : string.Empty
            );

            indexLabel.AddToClassList("slot-index");
            button.Add(indexLabel);

            VisualElement icon = new VisualElement();
            icon.AddToClassList("slot-icon");
            button.Add(icon);

            Label nameLabel = new Label();
            nameLabel.AddToClassList("slot-name");
            button.Add(nameLabel);

            Label quantityLabel = new Label();
            quantityLabel.AddToClassList("slot-quantity");
            button.Add(quantityLabel);

            if (slot != null && !slot.IsEmpty)
            {
                ItemStack stack = slot.Stack;
                nameLabel.text = stack.Item.DisplayName;
                quantityLabel.text = $"x{stack.Quantity}";

                ApplyItemIcon(
                    icon,
                    stack.Item,
                    presentation == SlotPresentation.CompactHotbar
                );

                button.tooltip =
                    $"{stack.Item.DisplayName}\n" +
                    $"{stack.Quality} • " +
                    $"{Mathf.RoundToInt(stack.Condition * 100f)}%";
            }
            else
            {
                nameLabel.text = string.Empty;
                quantityLabel.text = string.Empty;
                icon.AddToClassList("slot-icon-empty");
            }

            if (presentation != SlotPresentation.CompactHotbar)
            {
                int capturedIndex = index;
                SlotPresentation capturedPresentation = presentation;

                button.pickingMode = PickingMode.Position;
                button.SetEnabled(true);

                button.RegisterCallback<PointerDownEvent>(
                    evt =>
                    {
                        if (evt.button != 0)
                        {
                            return;
                        }

                        SelectInventorySlot(
                            capturedIndex,
                            capturedPresentation
                        );

                        evt.StopImmediatePropagation();
                    },
                    TrickleDown.TrickleDown
                );
            }

            ApplySlotRuntimeStyle(
                button,
                indexLabel,
                icon,
                nameLabel,
                quantityLabel,
                presentation,
                isUiSelected,
                isEquipped,
                slot != null && !slot.IsEmpty
            );

            return button;
        }

        private void SelectInventorySlot(
            int index,
            SlotPresentation presentation
        )
        {
            if (
                inventory == null ||
                index < 0 ||
                index >= inventory.SlotCount
            )
            {
                return;
            }

            selectedInventoryIndex = index;

            if (
                presentation ==
                SlotPresentation.InventoryHotbar
            )
            {
                hotbar?.Select(index);
            }

            SetActionMessage(string.Empty);
            RefreshAll();
        }

        private void ApplyItemIcon(
            VisualElement iconElement,
            ItemDefinition item,
            bool compact
        )
        {
            iconElement.Clear();

            Texture2D texture = GetItemTexture(item);

            if (texture != null)
            {
                Image image = new Image
                {
                    image = texture,
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Ignore
                };

                image.style.position = Position.Absolute;
                image.style.left = 0f;
                image.style.right = 0f;
                image.style.top = 0f;
                image.style.bottom = 0f;

                iconElement.Add(image);
                return;
            }

            Color fallback = item.FallbackColor;
            fallback.a = compact ? 0.72f : 0.9f;

            iconElement.style.backgroundColor =
                new StyleColor(fallback);

            Label abbreviation =
                new Label(GetAbbreviation(item.DisplayName));

            abbreviation.style.position = Position.Absolute;
            abbreviation.style.left = 0f;
            abbreviation.style.right = 0f;
            abbreviation.style.top = 0f;
            abbreviation.style.bottom = 0f;
            abbreviation.style.unityTextAlign =
                TextAnchor.MiddleCenter;
            abbreviation.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            abbreviation.style.fontSize =
                compact ? 13f : 18f;
            abbreviation.style.color =
                new StyleColor(Color.white);
            abbreviation.pickingMode = PickingMode.Ignore;

            iconElement.Add(abbreviation);
        }

        private Texture2D GetItemTexture(ItemDefinition item)
        {
            if (item == null)
            {
                return null;
            }

            if (iconCache.TryGetValue(item, out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture =
                Resources.Load<Texture2D>(
                    $"{IconResourceRoot}/{item.name}"
                );

            if (texture == null)
            {
                string safeDisplayName =
                    item.DisplayName
                        .Replace(" ", string.Empty)
                        .Replace("-", string.Empty);

                texture =
                    Resources.Load<Texture2D>(
                        $"{IconResourceRoot}/ITEM_{safeDisplayName}"
                    );
            }

            iconCache[item] = texture;
            return texture;
        }

        private void RefreshDetails()
        {
            InventorySlot slot =
                inventory != null
                    ? inventory.GetSlot(selectedInventoryIndex)
                    : null;

            bool hasItem =
                slot != null &&
                !slot.IsEmpty;

            useButton?.SetEnabled(hasItem);
            dropButton?.SetEnabled(hasItem);
            inspectButton?.SetEnabled(hasItem);

            splitButton?.SetEnabled(
                hasItem &&
                slot.Stack.Quantity > 1 &&
                inventory.HasEmptySlot()
            );

            if (!hasItem)
            {
                detailsNameLabel.text = "EMPTY SLOT";
                detailsQuantityLabel.text = string.Empty;
                detailsCategoryLabel.text = "No Item";
                detailsQualityLabel.text = "—";
                detailsConditionLabel.text = "—";
                detailsValueLabel.text = "—";

                detailsDescriptionLabel.text =
                    "Select a filled inventory slot to inspect its contents.";

                detailsIcon.style.backgroundImage =
                    StyleKeyword.None;

                detailsIcon.style.backgroundColor =
                    new StyleColor(
                        new Color(0.06f, 0.07f, 0.075f, 1f)
                    );

                detailsConditionFill.style.width =
                    Length.Percent(0f);

                return;
            }

            ItemStack stack = slot.Stack;
            ItemDefinition item = stack.Item;

            detailsNameLabel.text =
                item.DisplayName.ToUpperInvariant();

            detailsQuantityLabel.text =
                $"x{stack.Quantity}";

            detailsCategoryLabel.text =
                SplitCamelCase(item.Category.ToString());

            detailsQualityLabel.text =
                stack.Quality.ToString();

            int conditionPercent =
                Mathf.RoundToInt(stack.Condition * 100f);

            detailsConditionLabel.text =
                $"{conditionPercent}%";

            detailsValueLabel.text =
                $"${item.BaseValue:N0} each";

            detailsDescriptionLabel.text =
                string.IsNullOrWhiteSpace(item.Description)
                    ? "No item description has been assigned."
                    : item.Description;

            detailsConditionFill.style.width =
                Length.Percent(conditionPercent);

            ApplyLargeItemIcon(detailsIcon, item);
        }

        private void ApplyLargeItemIcon(
            VisualElement target,
            ItemDefinition item
        )
        {
            target.Clear();
            target.style.backgroundImage = StyleKeyword.None;

            Texture2D texture = GetItemTexture(item);

            if (texture != null)
            {
                target.style.backgroundColor =
                    new StyleColor(
                        new Color(0.035f, 0.045f, 0.047f, 1f)
                    );

                Image image = new Image
                {
                    image = texture,
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Ignore
                };

                image.style.position = Position.Absolute;
                image.style.left = 8f;
                image.style.right = 8f;
                image.style.top = 8f;
                image.style.bottom = 8f;

                target.Add(image);
                return;
            }

            Color fallback = item.FallbackColor;
            fallback.a = 0.9f;

            target.style.backgroundColor =
                new StyleColor(fallback);

            Label abbreviation =
                new Label(GetAbbreviation(item.DisplayName));

            abbreviation.style.position = Position.Absolute;
            abbreviation.style.left = 0f;
            abbreviation.style.right = 0f;
            abbreviation.style.top = 0f;
            abbreviation.style.bottom = 0f;
            abbreviation.style.unityTextAlign =
                TextAnchor.MiddleCenter;
            abbreviation.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            abbreviation.style.fontSize = 42f;
            abbreviation.style.color =
                new StyleColor(Color.white);
            abbreviation.pickingMode = PickingMode.Ignore;

            target.Add(abbreviation);
        }

        private void UseSelectedItem()
        {
            if (
                inventory == null ||
                hotbar == null
            )
            {
                return;
            }

            InventorySlot slot =
                inventory.GetSlot(selectedInventoryIndex);

            if (slot == null || slot.IsEmpty)
            {
                SetActionMessage("That slot is empty.");
                return;
            }

            string itemName =
                slot.Stack.Item.DisplayName;

            bool equipped =
                hotbar.EquipFromInventorySlot(
                    selectedInventoryIndex
                );

            if (!equipped)
            {
                SetActionMessage(
                    "The item could not be moved to the hotbar."
                );
                return;
            }

            selectedInventoryIndex =
                hotbar.SelectedIndex;

            SetActionMessage(
                $"Equipped {itemName} " +
                $"in hotbar slot {hotbar.SelectedIndex + 1}."
            );

            RefreshAll();
        }

        private void DropSelectedItem()
        {
            if (
                inventory == null ||
                hotbar == null
            )
            {
                return;
            }

            InventorySlot slot =
                inventory.GetSlot(selectedInventoryIndex);

            if (slot == null || slot.IsEmpty)
            {
                SetActionMessage("That slot is empty.");
                return;
            }

            string itemName =
                slot.Stack.Item.DisplayName;

            bool dropped =
                hotbar.DropOneFromSlot(
                    selectedInventoryIndex
                );

            SetActionMessage(
                dropped
                    ? $"Dropped one {itemName}."
                    : "The item could not be dropped."
            );

            RefreshAll();
        }

        private void SplitSelectedStack()
        {
            if (inventory == null)
            {
                return;
            }

            int newSlotIndex =
                inventory.SplitStack(
                    selectedInventoryIndex
                );

            if (newSlotIndex < 0)
            {
                SetActionMessage(
                    "The stack cannot be split or there is no empty slot."
                );
                return;
            }

            SetActionMessage(
                $"Split stack into slot {newSlotIndex + 1}."
            );

            RefreshAll();
        }

        private void OpenInspectModal()
        {
            InventorySlot slot =
                inventory != null
                    ? inventory.GetSlot(selectedInventoryIndex)
                    : null;

            if (
                slot == null ||
                slot.IsEmpty ||
                inspectModal == null
            )
            {
                return;
            }

            ItemStack stack = slot.Stack;
            ItemDefinition item = stack.Item;

            inspectNameLabel.text =
                item.DisplayName.ToUpperInvariant();

            inspectCategoryLabel.text =
                SplitCamelCase(item.Category.ToString());

            inspectDescriptionLabel.text =
                string.IsNullOrWhiteSpace(item.Description)
                    ? "No item description has been assigned."
                    : item.Description;

            inspectMetadataLabel.text =
                $"Quantity x{stack.Quantity}   •   " +
                $"{stack.Quality} quality   •   " +
                $"{Mathf.RoundToInt(stack.Condition * 100f)}% condition   •   " +
                $"${item.BaseValue:N0} each";

            ApplyLargeItemIcon(inspectIcon, item);

            inspectOpen = true;
            inspectModal.pickingMode = PickingMode.Position;
            inspectModal.style.display = DisplayStyle.Flex;

            // The inventory screen is brought forward when opened so its
            // controls receive pointer input. Bring the inspection modal
            // above it each time the modal opens.
            inspectModal.BringToFront();
            inspectModal.MarkDirtyRepaint();

            VisualElement inspectCard =
                inspectModal.Q<VisualElement>(
                    className: "inspect-card"
                );

            inspectCard?.BringToFront();
            inspectCloseButton?.Focus();
        }

        private void CloseInspectModal()
        {
            inspectOpen = false;

            if (inspectModal != null)
            {
                inspectModal.style.display =
                    DisplayStyle.None;
            }

            if (inventoryOpen && inventoryScreen != null)
            {
                inventoryScreen.BringToFront();
                root?.Focus();
            }
        }

        private void SetActionMessage(string message)
        {
            if (detailsActionMessageLabel == null)
            {
                return;
            }

            detailsActionMessageLabel.text =
                string.IsNullOrWhiteSpace(message)
                    ? string.Empty
                    : message;
        }

        private void HandleInventoryHotbarInput(
            Keyboard keyboard
        )
        {
            int requestedIndex = -1;

            if (keyboard.digit1Key.wasPressedThisFrame)
                requestedIndex = 0;
            else if (keyboard.digit2Key.wasPressedThisFrame)
                requestedIndex = 1;
            else if (keyboard.digit3Key.wasPressedThisFrame)
                requestedIndex = 2;
            else if (keyboard.digit4Key.wasPressedThisFrame)
                requestedIndex = 3;
            else if (keyboard.digit5Key.wasPressedThisFrame)
                requestedIndex = 4;
            else if (keyboard.digit6Key.wasPressedThisFrame)
                requestedIndex = 5;
            else if (keyboard.digit7Key.wasPressedThisFrame)
                requestedIndex = 6;
            else if (keyboard.digit8Key.wasPressedThisFrame)
                requestedIndex = 7;

            if (
                requestedIndex >= 0 &&
                inventory != null &&
                requestedIndex < inventory.HotbarSize
            )
            {
                selectedInventoryIndex =
                    requestedIndex;

                hotbar?.Select(requestedIndex);
                RefreshAll();
            }

            Mouse mouse = Mouse.current;

            if (mouse == null || hotbar == null)
            {
                return;
            }

            float scroll = mouse.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) <= 0.01f)
            {
                return;
            }

            hotbar.Select(
                hotbar.SelectedIndex +
                (scroll > 0f ? -1 : 1)
            );

            selectedInventoryIndex =
                hotbar.SelectedIndex;

            RefreshAll();
        }

        private void HandleInventoryChanged()
        {
            if (selectedInventoryIndex >= inventory.SlotCount)
            {
                selectedInventoryIndex = 0;
            }

            RefreshAll();
        }

        private void HandleHotbarSelectionChanged(int index)
        {
            if (!inventoryOpen)
            {
                selectedInventoryIndex = index;
            }

            RefreshAll();
        }

        private void ResolveReferences()
        {
            if (
                inventory != null &&
                hotbar != null &&
                firstPersonController != null
            )
            {
                return;
            }

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                return;
            }

            inventory ??=
                player.GetComponent<PlayerInventory>();

            hotbar ??=
                player.GetComponent<HotbarController>();

            firstPersonController ??=
                player.GetComponent<FirstPersonController>();
        }

        private void BindDataSources()
        {
            if (subscribedInventory != inventory)
            {
                if (subscribedInventory != null)
                {
                    subscribedInventory.Changed -=
                        HandleInventoryChanged;
                }

                subscribedInventory = inventory;

                if (subscribedInventory != null)
                {
                    subscribedInventory.Changed +=
                        HandleInventoryChanged;
                }
            }

            if (subscribedHotbar != hotbar)
            {
                if (subscribedHotbar != null)
                {
                    subscribedHotbar.SelectionChanged -=
                        HandleHotbarSelectionChanged;
                }

                subscribedHotbar = hotbar;

                if (subscribedHotbar != null)
                {
                    subscribedHotbar.SelectionChanged +=
                        HandleHotbarSelectionChanged;
                }
            }
        }

        private void UnbindDataSources()
        {
            if (subscribedInventory != null)
            {
                subscribedInventory.Changed -=
                    HandleInventoryChanged;
            }

            if (subscribedHotbar != null)
            {
                subscribedHotbar.SelectionChanged -=
                    HandleHotbarSelectionChanged;
            }

            subscribedInventory = null;
            subscribedHotbar = null;
        }

        private static void SetClass(
            VisualElement element,
            string className,
            bool enabled
        )
        {
            if (enabled)
            {
                element.AddToClassList(className);
            }
            else
            {
                element.RemoveFromClassList(className);
            }
        }

        private static string GetAbbreviation(
            string displayName
        )
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "?";
            }

            string[] words =
                displayName.Split(
                    new[] { ' ', '-', '_' },
                    StringSplitOptions.RemoveEmptyEntries
                );

            if (words.Length == 1)
            {
                return words[0]
                    .Substring(
                        0,
                        Mathf.Min(3, words[0].Length)
                    )
                    .ToUpperInvariant();
            }

            string abbreviation = string.Empty;

            for (
                int i = 0;
                i < words.Length &&
                abbreviation.Length < 3;
                i++
            )
            {
                abbreviation +=
                    char.ToUpperInvariant(words[i][0]);
            }

            return abbreviation;
        }

        private static string SplitCamelCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];

                if (
                    i > 0 &&
                    char.IsUpper(current) &&
                    !char.IsUpper(value[i - 1])
                )
                {
                    builder.Append(' ');
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private enum SlotPresentation
        {
            Inventory,
            InventoryHotbar,
            CompactHotbar
        }
    }
}
