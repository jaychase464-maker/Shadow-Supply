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
                layoutAsset == null ||
                styleAsset == null
            )
            {
                return;
            }

            root = uiDocument.rootVisualElement;
            root.Clear();

            layoutAsset.CloneTree(root);

            root.styleSheets.Add(styleAsset);

            CacheVisualElements();
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

        private void RegisterButtonCallbacks()
        {
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

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

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

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            RefreshAll();
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

                button.clicked += () =>
                {
                    selectedInventoryIndex = capturedIndex;

                    if (
                        presentation ==
                        SlotPresentation.InventoryHotbar
                    )
                    {
                        hotbar?.Select(capturedIndex);
                    }

                    SetActionMessage(string.Empty);
                    RefreshAll();
                };
            }

            return button;
        }

        private void ApplyItemIcon(
            VisualElement iconElement,
            ItemDefinition item,
            bool compact
        )
        {
            Texture2D texture = GetItemTexture(item);

            if (texture != null)
            {
                iconElement.style.backgroundImage =
                    new StyleBackground(texture);

                iconElement.AddToClassList("has-item-texture");
                return;
            }

            Color fallback = item.FallbackColor;
            fallback.a = compact ? 0.72f : 0.9f;

            iconElement.style.backgroundColor =
                new StyleColor(fallback);

            Label abbreviation =
                new Label(GetAbbreviation(item.DisplayName));

            abbreviation.AddToClassList(
                compact
                    ? "slot-icon-abbreviation-compact"
                    : "slot-icon-abbreviation"
            );

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

            Texture2D texture = GetItemTexture(item);

            if (texture != null)
            {
                target.style.backgroundImage =
                    new StyleBackground(texture);

                target.style.backgroundColor =
                    new StyleColor(Color.clear);

                return;
            }

            Color fallback = item.FallbackColor;
            fallback.a = 0.9f;

            target.style.backgroundImage =
                StyleKeyword.None;

            target.style.backgroundColor =
                new StyleColor(fallback);

            Label abbreviation =
                new Label(GetAbbreviation(item.DisplayName));

            abbreviation.AddToClassList(
                "details-icon-abbreviation"
            );

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

            if (slot == null || slot.IsEmpty)
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
            inspectModal.style.display = DisplayStyle.Flex;
        }

        private void CloseInspectModal()
        {
            inspectOpen = false;

            if (inspectModal != null)
            {
                inspectModal.style.display =
                    DisplayStyle.None;
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
