using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Character
{
    [CreateAssetMenu(
        fileName = "DB_CharacterParts",
        menuName = "Shadow Supply/Character/Part Database"
    )]
    public sealed class CharacterPartDatabase : ScriptableObject
    {
        [SerializeField] private List<CharacterPartDefinition> definitions =
            new List<CharacterPartDefinition>();

        private readonly Dictionary<string, CharacterPartDefinition> lookup =
            new Dictionary<string, CharacterPartDefinition>(
                StringComparer.Ordinal
            );

        private bool lookupBuilt;

        public IReadOnlyList<CharacterPartDefinition> Definitions =>
            definitions;

        public void SetDefinitions(
            IEnumerable<CharacterPartDefinition> newDefinitions
        )
        {
            definitions.Clear();

            if (newDefinitions != null)
            {
                foreach (
                    CharacterPartDefinition definition
                    in newDefinitions
                )
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

        public bool TryGetDefinition(
            string partId,
            out CharacterPartDefinition definition
        )
        {
            definition = null;
            BuildLookup();

            if (string.IsNullOrWhiteSpace(partId))
            {
                return false;
            }

            return lookup.TryGetValue(
                partId,
                out definition
            );
        }

        private void OnEnable()
        {
            lookupBuilt = false;
        }

        private void BuildLookup()
        {
            if (lookupBuilt)
            {
                return;
            }

            lookup.Clear();

            foreach (
                CharacterPartDefinition definition
                in definitions
            )
            {
                if (definition == null)
                {
                    continue;
                }

                definition.EnsurePersistentId();

                if (
                    !lookup.ContainsKey(
                        definition.PartId
                    )
                )
                {
                    lookup.Add(
                        definition.PartId,
                        definition
                    );
                }
            }

            lookupBuilt = true;
        }
    }
}
