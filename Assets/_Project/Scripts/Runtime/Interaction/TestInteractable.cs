using UnityEngine;

namespace ShadowSupply.Interaction
{
    [RequireComponent(typeof(Renderer))]
    public sealed class TestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string objectName = "Test Crate";
        [SerializeField] private Color inactiveColor = new Color(0.18f, 0.32f, 0.42f);
        [SerializeField] private Color activeColor = new Color(0.95f, 0.48f, 0.12f);

        private Renderer cachedRenderer;
        private bool active;

        public string InteractionPrompt => active
            ? $"Deactivate {objectName}"
            : $"Interact with {objectName}";

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            ApplyColor();
        }

        public bool CanInteract(GameObject interactor)
        {
            return interactor != null;
        }

        public void Interact(GameObject interactor)
        {
            active = !active;
            ApplyColor();
            Debug.Log($"[TestInteractable] {objectName} is now {(active ? "active" : "inactive")}.", this);
        }

        private void ApplyColor()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            Material material = cachedRenderer.material;
            Color color = active ? activeColor : inactiveColor;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.color = color;
            }
        }
    }
}
