using System;
using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterPartMarker : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string partId;
        [SerializeField] private string displayName;
        [SerializeField] private CharacterSlot slot;
        [SerializeField] private CharacterBodyRegion coveredRegions;
        [SerializeField] private Renderer[] renderers =
            Array.Empty<Renderer>();

        public string PartId => partId;
        public string DisplayName => displayName;
        public CharacterSlot Slot => slot;
        public CharacterBodyRegion CoveredRegions => coveredRegions;
        public bool IsVisible { get; private set; } = true;

        public void Configure(
            string stablePartId,
            string partDisplayName,
            CharacterSlot characterSlot,
            CharacterBodyRegion regions
        )
        {
            partId = stablePartId;
            displayName = partDisplayName;
            slot = characterSlot;
            coveredRegions = regions;

            if (renderers == null || renderers.Length == 0)
            {
                renderers =
                    GetComponentsInChildren<Renderer>(true);
            }
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;

            if (renderers == null)
            {
                return;
            }

            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer != null)
                {
                    targetRenderer.enabled = visible;
                }
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(partId))
            {
                partId = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = gameObject.name;
            }

            renderers =
                GetComponentsInChildren<Renderer>(true);
        }
    }
}
