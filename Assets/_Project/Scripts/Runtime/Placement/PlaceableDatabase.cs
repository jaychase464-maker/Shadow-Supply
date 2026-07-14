using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Placement
{
    [CreateAssetMenu(
        fileName = "DB_Placeables",
        menuName = "Shadow Supply/Placement/Placeable Database"
    )]
    public sealed class PlaceableDatabase : ScriptableObject
    {
        [SerializeField] private List<PlaceableDefinition> definitions =
            new List<PlaceableDefinition>();

        private readonly Dictionary<string, PlaceableDefinition> lookup =
            new Dictionary<string, PlaceableDefinition>(
                StringComparer.Ordinal
            );

        private bool lookupBuilt;

        public IReadOnlyList<PlaceableDefinition> Definitions =>
            definitions;

        public void SetDefinitions(
            IEnumerable<PlaceableDefinition> newDefinitions
        )
        {
            definitions.Clear();

            if (newDefinitions != null)
            {
                foreach (PlaceableDefinition definition in newDefinitions)
                {
                    if (
                        definition != null &&
                        !definitions.Contains(definition)
                    )
                    {
                        definition.EnsurePersistentId();
                        definitions.Add(definition);
                    }
                }
            }

            lookupBuilt = false;
        }

        public PlaceableDefinition GetAt(int index)
        {
            return
                index >= 0 &&
                index < definitions.Count
                    ? definitions[index]
                    : null;
        }

        public bool TryGetDefinition(
            string placeableId,
            out PlaceableDefinition definition
        )
        {
            definition = null;
            BuildLookup();

            if (string.IsNullOrWhiteSpace(placeableId))
            {
                return false;
            }

            return lookup.TryGetValue(
                placeableId,
                out definition
            );
        }

        private void OnEnable()
        {
            lookupBuilt = false;
        }

        private void OnValidate()
        {
            definitions.RemoveAll(definition => definition == null);

            foreach (PlaceableDefinition definition in definitions)
            {
                definition?.EnsurePersistentId();
            }

            lookupBuilt = false;
        }

        private void BuildLookup()
        {
            if (lookupBuilt)
            {
                return;
            }

            lookup.Clear();

            foreach (PlaceableDefinition definition in definitions)
            {
                if (definition == null)
                {
                    continue;
                }

                definition.EnsurePersistentId();

                if (string.IsNullOrWhiteSpace(definition.PlaceableId))
                {
                    continue;
                }

                if (lookup.ContainsKey(definition.PlaceableId))
                {
                    Debug.LogWarning(
                        $"[PlaceableDatabase] Duplicate placeable ID " +
                        $"'{definition.PlaceableId}'.",
                        this
                    );
                    continue;
                }

                lookup.Add(
                    definition.PlaceableId,
                    definition
                );
            }

            lookupBuilt = true;
        }
    }
}
