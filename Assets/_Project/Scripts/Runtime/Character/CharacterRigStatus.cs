using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterRigStatus : MonoBehaviour
    {
        [SerializeField] private bool rigReady;
        [SerializeField, TextArea(3, 8)] private string statusMessage;

        public bool RigReady => rigReady;
        public string StatusMessage => statusMessage;

        public void Configure(
            bool ready,
            string message
        )
        {
            rigReady = ready;
            statusMessage = message;
        }
    }
}
