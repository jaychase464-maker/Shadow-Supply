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
            inventory != null ? inventory.GetSlot(SelectedIndex) : null;

        private void Awake()
        {
            inventory ??= GetComponent<PlayerInventory>();
            heldItemDisplay ??= GetComponent<HeldItemDisplay>();

            if (dropOrigin == null)
            {
                Camera camera = GetComponentInChildren<Camera>(true);
                dropOrigin = camera != null ? camera.transform : transform;
            }
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.Changed += HandleInventoryChanged;
            }
        }

        private void Start()
        {
            Select(0);
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

        public void DropOneSelectedItem()
        {
            if (
                inventory == null ||
                SelectedSlot == null ||
                SelectedSlot.IsEmpty ||
                dropOrigin == null
            )
            {
                return;
            }

            ItemStack removed = inventory.RemoveFromSlot(SelectedIndex, 1);

            if (removed == null || removed.IsEmpty)
            {
                return;
            }

            Vector3 position =
                dropOrigin.position +
                dropOrigin.forward * 0.8f -
                dropOrigin.up * 0.15f;

            Vector3 velocity =
                dropOrigin.forward * dropForwardSpeed +
                Vector3.up * dropUpwardSpeed;

            WorldItemPickup.Spawn(removed, position, velocity);
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
            heldItemDisplay?.Refresh(SelectedSlot?.Stack);
        }
    }
}
