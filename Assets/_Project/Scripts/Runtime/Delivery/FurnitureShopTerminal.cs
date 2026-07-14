using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Delivery
{
    public sealed class FurnitureShopTerminal :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private FurnitureDeliverySystem deliverySystem;

        public FurnitureDeliverySystem DeliverySystem =>
            deliverySystem;

        public string InteractionPrompt =>
            "Browse furniture supplier";

        public void Configure(
            FurnitureDeliverySystem system
        )
        {
            deliverySystem = system;
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                interactor != null &&
                deliverySystem != null &&
                FurnitureShopHUD.Instance != null;
        }

        public void Interact(GameObject interactor)
        {
            FurnitureShopHUD.Instance?.Open(
                this,
                interactor
            );
        }
    }
}
