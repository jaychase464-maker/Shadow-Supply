using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterAppearance : MonoBehaviour
    {
        [SerializeField] private CharacterPartDatabase database;
        [SerializeField] private List<CharacterPartMarker> parts =
            new List<CharacterPartMarker>();

        private readonly Dictionary<CharacterSlot, CharacterPartMarker>
            equippedBySlot =
                new Dictionary<CharacterSlot, CharacterPartMarker>();

        public event Action Changed;

        public CharacterPartDatabase Database => database;

        private void Awake()
        {
            RebuildPartCache();
        }

        public void Configure(
            CharacterPartDatabase partDatabase
        )
        {
            database = partDatabase;
            RebuildPartCache();
        }

        public void RebuildPartCache()
        {
            parts.Clear();

            parts.AddRange(
                GetComponentsInChildren<CharacterPartMarker>(true)
            );

            equippedBySlot.Clear();

            foreach (CharacterPartMarker part in parts)
            {
                if (
                    part != null &&
                    part.IsVisible
                )
                {
                    equippedBySlot[part.Slot] = part;
                }
            }
        }

        public bool EquipPart(
            string partId
        )
        {
            CharacterPartMarker requestedPart =
                FindPart(partId);

            if (requestedPart == null)
            {
                return false;
            }

            foreach (CharacterPartMarker part in parts)
            {
                if (
                    part != null &&
                    part.Slot == requestedPart.Slot
                )
                {
                    part.SetVisible(
                        part == requestedPart
                    );
                }
            }

            equippedBySlot[requestedPart.Slot] =
                requestedPart;

            Changed?.Invoke();
            return true;
        }

        public bool SetSlotVisible(
            CharacterSlot slot,
            bool visible
        )
        {
            bool changed = false;

            foreach (CharacterPartMarker part in parts)
            {
                if (
                    part != null &&
                    part.Slot == slot
                )
                {
                    part.SetVisible(visible);
                    changed = true;
                }
            }

            if (changed)
            {
                Changed?.Invoke();
            }

            return changed;
        }

        public string GetEquippedPartId(
            CharacterSlot slot
        )
        {
            return
                equippedBySlot.TryGetValue(
                    slot,
                    out CharacterPartMarker part
                ) &&
                part != null
                    ? part.PartId
                    : string.Empty;
        }

        private CharacterPartMarker FindPart(
            string partId
        )
        {
            if (string.IsNullOrWhiteSpace(partId))
            {
                return null;
            }

            foreach (CharacterPartMarker part in parts)
            {
                if (
                    part != null &&
                    string.Equals(
                        part.PartId,
                        partId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return part;
                }
            }

            return null;
        }
    }
}
