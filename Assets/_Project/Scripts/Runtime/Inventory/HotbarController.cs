using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Inventory
{
    public sealed class HotbarController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private HeldItemDisplay heldItemDisplay;
        [SerializeField] private Transform dropOrigin;
        [SerializeField, Min(0f)] private float dropForwardSpeed = 2.5f;
        [SerializeField, Min(0f)] private float dropUpwardSpeed = 1.2f;

        public event Action<int> SelectionChanged;

        public int SelectedIndex { get; private set; }

        public InventorySlot SelectedSlot =>
            inventory != null
                ? inventory.GetSlot(SelectedIndex)
                : null;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (inventory != null)
            {
                inventory.Changed += HandleInventoryChanged;
            }

            RefreshHeldItem();
        }

        private void Start()
        {
            Select(SelectedIndex);
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.Changed -= HandleInventoryChanged;
            }
        }

        private void Update()
        {
            HandleNumberSelection();
            HandleScrollSelection();

            if (
                Keyboard.current != null &&
                Keyboard.current.qKey.wasPressedThisFrame
            )
            {
                DropOneSelectedItem();
            }
        }

        public void Select(int index)
        {
            if (inventory == null || inventory.HotbarSize <= 0)
            {
                return;
            }

            int wrapped = index % inventory.HotbarSize;

            if (wrapped < 0)
            {
                wrapped += inventory.HotbarSize;
            }

            SelectedIndex = wrapped;
            RefreshHeldItem();
            SelectionChanged?.Invoke(SelectedIndex);
        }

        public bool EquipFromInventorySlot(int inventoryIndex)
        {
            if (
                inventory == null ||
                inventoryIndex < 0 ||
                inventoryIndex >= inventory.SlotCount
            )
            {
                return false;
            }

            InventorySlot source =
                inventory.GetSlot(inventoryIndex);

            if (source == null || source.IsEmpty)
            {
                return false;
            }

            if (inventoryIndex < inventory.HotbarSize)
            {
                Select(inventoryIndex);
                return true;
            }

            int destinationIndex = SelectedIndex;

            if (
                destinationIndex < 0 ||
                destinationIndex >= inventory.HotbarSize
            )
            {
                destinationIndex = 0;
            }

            if (
                !inventory.SwapSlots(
                    inventoryIndex,
                    destinationIndex
                )
            )
            {
                return false;
            }

            Select(destinationIndex);
            return true;
        }

        public void DropOneSelectedItem()
        {
            DropOneFromSlot(SelectedIndex);
        }

        public bool DropOneFromSlot(int slotIndex)
        {
            ResolveReferences();

            if (
                inventory == null ||
                dropOrigin == null
            )
            {
                return false;
            }

            InventorySlot slot =
                inventory.GetSlot(slotIndex);

            if (slot == null || slot.IsEmpty)
            {
                return false;
            }

            ItemStack removed =
                inventory.RemoveFromSlot(slotIndex, 1);

            if (removed == null || removed.IsEmpty)
            {
                return false;
            }

            Vector3 position =
                dropOrigin.position +
                dropOrigin.forward * 0.8f -
                dropOrigin.up * 0.15f;

            Vector3 velocity =
                dropOrigin.forward * dropForwardSpeed +
                Vector3.up * dropUpwardSpeed;

            WorldItemPickup pickup =
                WorldItemPickup.Spawn(
                    removed,
                    position,
                    velocity
                );

            if (pickup == null)
            {
                inventory.AddItem(
                    removed.Item,
                    removed.Quantity,
                    removed.Quality,
                    removed.Condition
                );

                return false;
            }

            RefreshHeldItem();
            return true;
        }

        private void HandleNumberSelection()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) Select(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) Select(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) Select(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) Select(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) Select(4);
            else if (keyboard.digit6Key.wasPressedThisFrame) Select(5);
            else if (keyboard.digit7Key.wasPressedThisFrame) Select(6);
            else if (keyboard.digit8Key.wasPressedThisFrame) Select(7);
        }

        private void HandleScrollSelection()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            float scroll = mouse.scroll.ReadValue().y;

            if (scroll > 0.01f)
            {
                Select(SelectedIndex - 1);
            }
            else if (scroll < -0.01f)
            {
                Select(SelectedIndex + 1);
            }
        }

        private void HandleInventoryChanged()
        {
            RefreshHeldItem();
        }

        private void RefreshHeldItem()
        {
            heldItemDisplay?.Refresh(
                SelectedSlot?.Stack
            );
        }

        private void ResolveReferences()
        {
            inventory ??=
                GetComponent<PlayerInventory>();

            heldItemDisplay ??=
                GetComponent<HeldItemDisplay>();

            if (dropOrigin == null)
            {
                Camera camera =
                    GetComponentInChildren<Camera>(true);

                dropOrigin =
                    camera != null
                        ? camera.transform
                        : transform;
            }
        }
    }
}
