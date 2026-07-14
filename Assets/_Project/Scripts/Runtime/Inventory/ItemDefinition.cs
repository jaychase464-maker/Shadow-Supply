using System;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    public enum ItemCategory
    {
        Component,
        Material,
        Tool,
        Product,
        Equipment,
        Miscellaneous
    }

    public enum ItemQuality
    {
        Poor,
        Standard,
        Good,
        Excellent,
        Masterwork
    }

    [CreateAssetMenu(
        fileName = "ITEM_NewItem",
        menuName = "Shadow Supply/Inventory/Item Definition"
    )]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, HideInInspector] private string itemId;
        [SerializeField] private string displayName = "New Item";
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private ItemCategory category = ItemCategory.Miscellaneous;

        [Header("Inventory")]
        [SerializeField, Min(1)] private int maximumStack = 1;
        [SerializeField, Min(0)] private int baseValue;

        [Header("World and Held Display")]
        [SerializeField] private GameObject displayPrefab;
        [SerializeField] private PrimitiveType fallbackPrimitive = PrimitiveType.Cube;
        [SerializeField] private Color fallbackColor = Color.gray;
        [SerializeField] private Vector3 heldLocalPosition = new Vector3(0.42f, -0.36f, 0.72f);
        [SerializeField] private Vector3 heldLocalEulerAngles = new Vector3(12f, -22f, 0f);
        [SerializeField] private Vector3 heldLocalScale = Vector3.one * 0.22f;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public int MaximumStack => maximumStack;
        public int BaseValue => baseValue;
        public GameObject DisplayPrefab => displayPrefab;
        public PrimitiveType FallbackPrimitive => fallbackPrimitive;
        public Color FallbackColor => fallbackColor;
        public Vector3 HeldLocalPosition => heldLocalPosition;
        public Vector3 HeldLocalEulerAngles => heldLocalEulerAngles;
        public Vector3 HeldLocalScale => heldLocalScale;

        public void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = Guid.NewGuid().ToString("N");
            }
        }

        private void OnValidate()
        {
            EnsurePersistentId();
            maximumStack = Mathf.Max(1, maximumStack);
            baseValue = Mathf.Max(0, baseValue);

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }
        }
    }
}
