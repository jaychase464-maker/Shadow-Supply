using System;
using UnityEngine;

namespace ShadowSupply.Economy
{
    public sealed class PlayerWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int cleanCash = 1200;
        [SerializeField, Min(0)] private int dirtyCash;

        public event Action Changed;

        public int CleanCash => cleanCash;
        public int DirtyCash => dirtyCash;

        public bool CanAfford(int amount)
        {
            return amount >= 0 && cleanCash >= amount;
        }

        public bool TrySpendCleanCash(int amount)
        {
            if (amount < 0 || cleanCash < amount)
            {
                return false;
            }

            cleanCash -= amount;
            Changed?.Invoke();
            return true;
        }

        public void AddCleanCash(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            cleanCash += amount;
            Changed?.Invoke();
        }

        public void AddDirtyCash(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            dirtyCash += amount;
            Changed?.Invoke();
        }

        public void Restore(int restoredCleanCash, int restoredDirtyCash)
        {
            cleanCash = Mathf.Max(0, restoredCleanCash);
            dirtyCash = Mathf.Max(0, restoredDirtyCash);
            Changed?.Invoke();
        }

        public void ConfigureStartingCash(int amount)
        {
            cleanCash = Mathf.Max(0, amount);
            Changed?.Invoke();
        }
    }
}
