using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowSupply.Inventory
{
    public sealed class HeldItemDisplay : MonoBehaviour
    {
        [SerializeField] private Transform displayRoot;

        private GameObject currentDisplay;
        private ItemDefinition displayedItem;

        private void Awake()
        {
            EnsureDisplayRoot();
        }

        public void Refresh(ItemStack stack)
        {
            ItemDefinition requestedItem =
                stack == null || stack.IsEmpty ? null : stack.Item;

            if (displayedItem == requestedItem)
            {
                return;
            }

            Clear();
            displayedItem = requestedItem;

            if (displayedItem == null)
            {
                return;
            }

            EnsureDisplayRoot();

            if (displayRoot == null)
            {
                return;
            }

            if (displayedItem.DisplayPrefab != null)
            {
                currentDisplay = Instantiate(
                    displayedItem.DisplayPrefab,
                    displayRoot
                );
            }
            else
            {
                currentDisplay = GameObject.CreatePrimitive(
                    displayedItem.FallbackPrimitive
                );

                currentDisplay.transform.SetParent(displayRoot, false);
                ApplyFallbackAppearance(currentDisplay);
            }

            currentDisplay.name = $"Held_{displayedItem.DisplayName}";
            currentDisplay.transform.localPosition =
                displayedItem.HeldLocalPosition;
            currentDisplay.transform.localRotation =
                Quaternion.Euler(displayedItem.HeldLocalEulerAngles);
            currentDisplay.transform.localScale =
                displayedItem.HeldLocalScale;

            foreach (
                Collider collider in
                currentDisplay.GetComponentsInChildren<Collider>(true)
            )
            {
                Destroy(collider);
            }

            foreach (
                Rigidbody rigidbody in
                currentDisplay.GetComponentsInChildren<Rigidbody>(true)
            )
            {
                Destroy(rigidbody);
            }
        }

        public void Clear()
        {
            if (currentDisplay != null)
            {
                Destroy(currentDisplay);
            }

            currentDisplay = null;
            displayedItem = null;
        }

        private void EnsureDisplayRoot()
        {
            if (displayRoot != null)
            {
                return;
            }

            Camera camera = GetComponentInChildren<Camera>(true);

            if (camera == null)
            {
                Debug.LogError(
                    "[HeldItemDisplay] A child Camera is required.",
                    this
                );
                return;
            }

            GameObject rootObject = new GameObject("HeldItemRoot");
            displayRoot = rootObject.transform;
            displayRoot.SetParent(camera.transform, false);
        }

        private void ApplyFallbackAppearance(GameObject target)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();

            if (renderer == null)
            {
                return;
            }

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Material material = renderer.material;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    displayedItem.FallbackColor
                );
            }
            else if (material.HasProperty("_Color"))
            {
                material.color = displayedItem.FallbackColor;
            }
        }
    }
}
