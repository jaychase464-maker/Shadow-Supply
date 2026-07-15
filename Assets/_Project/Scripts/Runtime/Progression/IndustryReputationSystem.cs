using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Progression
{
    [Serializable]
    public sealed class IndustryReputationEntry
    {
        [SerializeField] private string industryId;
        [SerializeField] private int reputation;

        public string IndustryId => industryId;
        public int Reputation => reputation;

        public IndustryReputationEntry(
            string stableIndustryId,
            int startingReputation
        )
        {
            industryId = stableIndustryId;
            reputation = Mathf.Max(0, startingReputation);
        }

        public void SetReputation(int value)
        {
            reputation = Mathf.Max(0, value);
        }

        public void AddReputation(int amount)
        {
            reputation = Mathf.Max(0, reputation + amount);
        }
    }

    [DisallowMultipleComponent]
    public sealed class IndustryReputationSystem :
        MonoBehaviour
    {
        public const string CounterfeitingIndustryId =
            "industry-counterfeiting";

        [SerializeField]
        private List<IndustryReputationEntry> entries =
            new List<IndustryReputationEntry>();

        public event Action<string, int> ReputationChanged;

        public IReadOnlyList<IndustryReputationEntry> Entries =>
            entries;

        public int GetReputation(string industryId)
        {
            IndustryReputationEntry entry =
                FindEntry(industryId);

            return entry != null
                ? entry.Reputation
                : 0;
        }

        public void AddReputation(
            string industryId,
            int amount
        )
        {
            if (
                string.IsNullOrWhiteSpace(industryId) ||
                amount == 0
            )
            {
                return;
            }

            IndustryReputationEntry entry =
                FindEntry(industryId);

            if (entry == null)
            {
                entry =
                    new IndustryReputationEntry(
                        industryId,
                        0
                    );

                entries.Add(entry);
            }

            entry.AddReputation(amount);

            ReputationChanged?.Invoke(
                industryId,
                entry.Reputation
            );

            Debug.Log(
                $"[IndustryReputation] {industryId}: " +
                $"{entry.Reputation} reputation.",
                this
            );
        }

        public void SetReputation(
            string industryId,
            int value
        )
        {
            if (string.IsNullOrWhiteSpace(industryId))
            {
                return;
            }

            IndustryReputationEntry entry =
                FindEntry(industryId);

            if (entry == null)
            {
                entry =
                    new IndustryReputationEntry(
                        industryId,
                        value
                    );

                entries.Add(entry);
            }
            else
            {
                entry.SetReputation(value);
            }

            ReputationChanged?.Invoke(
                industryId,
                entry.Reputation
            );
        }

        public void ResetAll()
        {
            entries.Clear();
            ReputationChanged?.Invoke(
                CounterfeitingIndustryId,
                0
            );
        }

        public string GetCounterfeitingState()
        {
            int value =
                GetReputation(
                    CounterfeitingIndustryId
                );

            if (value < 8)
            {
                return "Unknown Printer";
            }

            if (value < 20)
            {
                return "Street Counterfeiter";
            }

            if (value < 40)
            {
                return "Reliable Printer";
            }

            if (value < 70)
            {
                return "Skilled Counterfeiter";
            }

            return "Master Printer";
        }

        public float GetCounterfeitingQualityBonus()
        {
            int value =
                GetReputation(
                    CounterfeitingIndustryId
                );

            if (value >= 70)
            {
                return 0.6f;
            }

            if (value >= 40)
            {
                return 0.4f;
            }

            if (value >= 20)
            {
                return 0.22f;
            }

            if (value >= 8)
            {
                return 0.1f;
            }

            return 0f;
        }

        private IndustryReputationEntry FindEntry(
            string industryId
        )
        {
            if (
                entries == null ||
                string.IsNullOrWhiteSpace(industryId)
            )
            {
                return null;
            }

            foreach (
                IndustryReputationEntry entry
                in entries
            )
            {
                if (
                    entry != null &&
                    string.Equals(
                        entry.IndustryId,
                        industryId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
