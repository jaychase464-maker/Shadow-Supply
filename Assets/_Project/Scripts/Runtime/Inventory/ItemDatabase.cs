using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    [CreateAssetMenu(
        fileName = "DB_Items",
        menuName = "Shadow Supply/Inventory/Item Database"
    )]
    public sealed class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

        private readonly Dictionary<string, ItemDefinition> lookup =
            new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);

        public IReadOnlyList<ItemDefinition> Items => items;

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        public void SetItems(IEnumerable<ItemDefinition> definitions)
        {
            items.Clear();

            if (definitions != null)
            {
                foreach (ItemDefinition definition in definitions)
                {
                    if (definition != null && !items.Contains(definition))
                    {
                        items.Add(definition);
                    }
                }
            }

            items.Sort(
                (left, right) => string.Compare(
                    left != null ? left.DisplayName : string.Empty,
                    right != null ? right.DisplayName : string.Empty,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            RebuildLookup();
        }

        public bool TryGetItem(string itemId, out ItemDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                definition = null;
                return false;
            }

            if (lookup.Count != items.Count)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(itemId, out definition);
        }

        public ItemDefinition GetItem(string itemId)
        {
            TryGetItem(itemId, out ItemDefinition definition);
            return definition;
        }

        private void RebuildLookup()
        {
            lookup.Clear();

            foreach (ItemDefinition definition in items)
            {
                if (
                    definition == null ||
                    string.IsNullOrWhiteSpace(definition.ItemId)
                )
                {
                    continue;
                }

                if (lookup.ContainsKey(definition.ItemId))
                {
                    Debug.LogError(
                        $"[ItemDatabase] Duplicate item ID " +
                        $"'{definition.ItemId}' found on '{definition.name}'.",
                        this
                    );
                    continue;
                }

                lookup.Add(definition.ItemId, definition);
            }
        }
    }
}
