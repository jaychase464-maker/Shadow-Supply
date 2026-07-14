using ShadowSupply.Inventory;
using ShadowSupply.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.UI
{
    public sealed class InventoryHUD : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private HotbarController hotbar;
        [SerializeField] private FirstPersonController firstPersonController;

        private InputAction toggleInventoryAction;
        private bool inventoryOpen;

        private GUIStyle panelStyle;
        private GUIStyle slotStyle;
        private GUIStyle selectedSlotStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallLabelStyle;
        private GUIStyle titleStyle;

        private void Awake()
        {
            ResolveReferences();

            toggleInventoryAction = new InputAction(
                "Toggle Inventory",
                InputActionType.Button,
                "<Keyboard>/tab"
            );

            toggleInventoryAction.AddBinding("<Keyboard>/i");
            toggleInventoryAction.AddBinding("<Gamepad>/select");
        }

        private void OnEnable()
        {
            toggleInventoryAction.Enable();
        }

        private void OnDisable()
        {
            toggleInventoryAction.Disable();

            if (inventoryOpen)
            {
                SetInventoryOpen(false);
            }
        }

        private void OnDestroy()
        {
            toggleInventoryAction?.Dispose();
        }

        private void Update()
        {
            ResolveReferences();

            if (toggleInventoryAction.WasPressedThisFrame())
            {
                SetInventoryOpen(!inventoryOpen);
            }
        }

        private void OnGUI()
        {
            if (inventory == null || hotbar == null)
            {
                return;
            }

            EnsureStyles();
            DrawHotbar();

            if (inventoryOpen)
            {
                DrawInventory();
            }
        }

        private void DrawHotbar()
        {
            int slotCount = inventory.HotbarSize;
            const float slotSize = 72f;
            const float spacing = 7f;

            float totalWidth =
                slotCount * slotSize +
                Mathf.Max(0, slotCount - 1) * spacing;

            float startX = (Screen.width - totalWidth) * 0.5f;
            float y = Screen.height - 92f;

            for (int i = 0; i < slotCount; i++)
            {
                Rect rect = new Rect(
                    startX + i * (slotSize + spacing),
                    y,
                    slotSize,
                    slotSize
                );

                DrawSlot(
                    rect,
                    inventory.GetSlot(i),
                    i,
                    i == hotbar.SelectedIndex
                );
            }
        }

        private void DrawInventory()
        {
            const int columns = 6;
            const float slotSize = 82f;
            const float spacing = 8f;
            const float padding = 22f;
            const float headerHeight = 52f;

            int rows = Mathf.CeilToInt(
                inventory.SlotCount / (float)columns
            );

            float width =
                columns * slotSize +
                (columns - 1) * spacing +
                padding * 2f;

            float height =
                rows * slotSize +
                (rows - 1) * spacing +
                padding * 2f +
                headerHeight;

            Rect panelRect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f - 18f,
                width,
                height
            );

            GUI.Box(panelRect, GUIContent.none, panelStyle);

            Rect titleRect = new Rect(
                panelRect.x + padding,
                panelRect.y + 10f,
                panelRect.width - padding * 2f,
                38f
            );

            GUI.Label(titleRect, "INVENTORY", titleStyle);

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                int column = i % columns;
                int row = i / columns;

                Rect rect = new Rect(
                    panelRect.x + padding + column * (slotSize + spacing),
                    panelRect.y + padding + headerHeight + row * (slotSize + spacing),
                    slotSize,
                    slotSize
                );

                DrawSlot(
                    rect,
                    inventory.GetSlot(i),
                    i,
                    i == hotbar.SelectedIndex
                );
            }

            Rect hintRect = new Rect(
                panelRect.x,
                panelRect.yMax + 8f,
                panelRect.width,
                26f
            );

            GUI.Label(
                hintRect,
                "TAB / I: Close   •   1–8 or Mouse Wheel: Select   •   Q: Drop One",
                smallLabelStyle
            );
        }

        private void DrawSlot(
            Rect rect,
            InventorySlot slot,
            int index,
            bool selected
        )
        {
            GUI.Box(
                rect,
                GUIContent.none,
                selected ? selectedSlotStyle : slotStyle
            );

            Rect numberRect = new Rect(
                rect.x + 6f,
                rect.y + 4f,
                rect.width - 12f,
                18f
            );

            GUI.Label(
                numberRect,
                index < inventory.HotbarSize
                    ? (index + 1).ToString()
                    : string.Empty,
                smallLabelStyle
            );

            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            ItemStack stack = slot.Stack;

            Rect nameRect = new Rect(
                rect.x + 7f,
                rect.y + 22f,
                rect.width - 14f,
                36f
            );

            GUI.Label(
                nameRect,
                stack.Item.DisplayName,
                labelStyle
            );

            Rect quantityRect = new Rect(
                rect.x + 6f,
                rect.yMax - 22f,
                rect.width - 12f,
                18f
            );

            GUI.Label(
                quantityRect,
                $"x{stack.Quantity}  {stack.Quality}",
                smallLabelStyle
            );
        }

        private void SetInventoryOpen(bool open)
        {
            inventoryOpen = open;

            if (firstPersonController != null)
            {
                firstPersonController.enabled = !open;
            }

            Cursor.lockState = open
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            Cursor.visible = open;
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

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                return;
            }

            inventory ??= player.GetComponent<PlayerInventory>();
            hotbar ??= player.GetComponent<HotbarController>();
            firstPersonController ??=
                player.GetComponent<FirstPersonController>();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle = CreateBoxStyle(
                new Color(0.025f, 0.03f, 0.035f, 0.96f)
            );

            slotStyle = CreateBoxStyle(
                new Color(0.085f, 0.095f, 0.105f, 0.97f)
            );

            selectedSlotStyle = CreateBoxStyle(
                new Color(0.62f, 0.28f, 0.055f, 0.98f)
            );

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            smallLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal =
                {
                    textColor = new Color(0.78f, 0.82f, 0.84f)
                }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 23,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.95f, 0.58f, 0.13f)
                }
            };
        }

        private static GUIStyle CreateBoxStyle(Color color)
        {
            Texture2D texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return new GUIStyle(GUI.skin.box)
            {
                normal = { background = texture },
                border = new RectOffset(1, 1, 1, 1)
            };
        }
    }
}
