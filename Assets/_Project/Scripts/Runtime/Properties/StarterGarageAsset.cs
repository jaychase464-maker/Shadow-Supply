using UnityEngine;

namespace ShadowSupply.Properties
{
    [DisallowMultipleComponent]
    public sealed class StarterGarageAsset : MonoBehaviour
    {
        [SerializeField] private string assetId;
        [SerializeField] private bool includedWithProperty = true;
        [SerializeField] private bool canBePacked;
        [SerializeField] private bool canBeSold;

        public string AssetId => assetId;
        public bool IncludedWithProperty => includedWithProperty;
        public bool CanBePacked => canBePacked;
        public bool CanBeSold => canBeSold;

        public void Configure(
            string stableAssetId,
            bool allowPacking,
            bool allowSelling
        )
        {
            assetId = stableAssetId;
            includedWithProperty = true;
            canBePacked = allowPacking;
            canBeSold = allowSelling;
        }
    }
}
