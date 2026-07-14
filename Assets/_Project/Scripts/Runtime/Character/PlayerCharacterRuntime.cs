using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterRuntime : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterAppearance localAppearance;
        [SerializeField] private PlayerCharacterAppearance shadowAppearance;
        [SerializeField] private CharacterSocketRegistry localSockets;
        [SerializeField] private FirstPersonBodyVisibilityController
            localVisibility;
        [SerializeField] private FirstPersonBodyVisibilityController
            shadowVisibility;

        public void Configure(
            PlayerCharacterAppearance localBodyAppearance,
            PlayerCharacterAppearance shadowBodyAppearance,
            CharacterSocketRegistry localBodySockets,
            FirstPersonBodyVisibilityController localBodyVisibility,
            FirstPersonBodyVisibilityController shadowBodyVisibility
        )
        {
            localAppearance = localBodyAppearance;
            shadowAppearance = shadowBodyAppearance;
            localSockets = localBodySockets;
            localVisibility = localBodyVisibility;
            shadowVisibility = shadowBodyVisibility;

            RefreshVisibility();
        }

        public string GetEquippedPartId(
            CharacterSlot slot
        )
        {
            return localAppearance != null
                ? localAppearance.GetEquippedPartId(slot)
                : string.Empty;
        }

        public bool EquipPart(
            string partId
        )
        {
            bool localChanged =
                localAppearance != null &&
                localAppearance.EquipPart(partId);

            bool shadowChanged =
                shadowAppearance != null &&
                shadowAppearance.EquipPart(partId);

            RefreshVisibility();

            return localChanged || shadowChanged;
        }

        public Transform GetSocket(
            CharacterSocket socket
        )
        {
            return localSockets != null
                ? localSockets.GetSocket(socket)
                : transform;
        }

        public GameObject AttachEquipment(
            CharacterSocket socket,
            GameObject prefab
        )
        {
            return localSockets != null
                ? localSockets.Attach(
                    socket,
                    prefab
                )
                : null;
        }

        public void RefreshVisibility()
        {
            localVisibility?.Apply();
            shadowVisibility?.Apply();
        }
    }
}
