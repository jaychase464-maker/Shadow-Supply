using System;
using UnityEngine;

namespace ShadowSupply.Character
{
    [CreateAssetMenu(
        fileName = "CHARPART_NewPart",
        menuName = "Shadow Supply/Character/Part Definition"
    )]
    public sealed class CharacterPartDefinition : ScriptableObject
    {
        [SerializeField, HideInInspector] private string partId;
        [SerializeField] private string displayName;
        [SerializeField] private CharacterSlot slot;
        [SerializeField] private CharacterBodyRegion coveredRegions;
        [SerializeField] private string sourceObjectName;
        [SerializeField] private GameObject visualPrefab;

        public string PartId => partId;
        public string DisplayName => displayName;
        public CharacterSlot Slot => slot;
        public CharacterBodyRegion CoveredRegions => coveredRegions;
        public string SourceObjectName => sourceObjectName;
        public GameObject VisualPrefab => visualPrefab;

        public void Configure(
            string stablePartId,
            string partDisplayName,
            CharacterSlot characterSlot,
            CharacterBodyRegion regions,
            string sourceName,
            GameObject prefab
        )
        {
            partId =
                string.IsNullOrWhiteSpace(stablePartId)
                    ? Guid.NewGuid().ToString("N")
                    : stablePartId;

            displayName = partDisplayName;
            slot = characterSlot;
            coveredRegions = regions;
            sourceObjectName = sourceName;
            visualPrefab = prefab;
        }

        public void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(partId))
            {
                partId = Guid.NewGuid().ToString("N");
            }
        }

        private void OnValidate()
        {
            EnsurePersistentId();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }
        }
    }
}
