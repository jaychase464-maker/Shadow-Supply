using UnityEngine;

namespace ShadowSupply.Production
{
    public enum CounterfeitPressActionKind
    {
        PrintControl,
        SealControl
    }

    [DisallowMultipleComponent]
    public sealed class CounterfeitPressActionTarget :
        MonoBehaviour
    {
        [SerializeField]
        private CounterfeitPressActionKind actionKind;

        public CounterfeitPressActionKind ActionKind =>
            actionKind;

        public void Configure(
            CounterfeitPressActionKind kind
        )
        {
            actionKind = kind;
        }
    }
}
