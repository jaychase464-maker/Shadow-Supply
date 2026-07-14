using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    public sealed class WorldItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField] private ItemQuality quality = ItemQuality.Standard;
        [SerializeField, Range(0f, 1f)] private float condition = 1f;

        public string InteractionPrompt =>
            item == null
                ? "Invalid item"
                : $"Pick up {item.DisplayName} x{quantity}";

        public ItemDefinition Item => item;
        public int Quantity => quantity;
        public ItemQuality Quality => quality;
        public float Condition => condition;

        public void Initialize(
            ItemDefinition definition,
            int amount,
            ItemQuality itemQuality = ItemQuality.Standard,
            float itemCondition = 1f
        )
        {
            item = definition;
            quantity = Mathf.Max(1, amount);
            quality = itemQuality;
            condition = Mathf.Clamp01(itemCondition);

            if (item != null)
            {
                name = $"Pickup_{item.DisplayName}_{quantity}";
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                item != null &&
                quantity > 0 &&
                interactor != null &&
                interactor.GetComponent<PlayerInventory>() != null;
        }

        public void Interact(GameObject interactor)
        {
            PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();

            if (inventory == null || item == null || quantity <= 0)
            {
                return;
            }

            int remaining = inventory.AddItem(item, quantity, quality, condition);
            int collected = quantity - remaining;
            quantity = remaining;

            if (collected > 0)
            {
                Debug.Log(
                    $"[Inventory] Picked up {item.DisplayName} x{collected}.",
                    this
                );
            }

            if (quantity <= 0)
            {
                Destroy(gameObject);
            }
        }

        public static WorldItemPickup Spawn(
            ItemStack stack,
            Vector3 position,
            Vector3 velocity
        )
        {
            if (stack == null || stack.IsEmpty)
            {
                return null;
            }

            GameObject worldObject;

            if (stack.Item.DisplayPrefab != null)
            {
                worldObject = Instantiate(
                    stack.Item.DisplayPrefab,
                    position,
                    Quaternion.identity
                );
            }
            else
            {
                worldObject = GameObject.CreatePrimitive(
                    stack.Item.FallbackPrimitive
                );

                worldObject.transform.position = position;
                worldObject.transform.localScale = Vector3.one * 0.32f;
                ApplyFallbackColor(worldObject, stack.Item.FallbackColor);
            }

            if (worldObject.GetComponentInChildren<Collider>() == null)
            {
                worldObject.AddComponent<BoxCollider>();
            }

            WorldItemPickup pickup =
                worldObject.GetComponent<WorldItemPickup>() ??
                worldObject.AddComponent<WorldItemPickup>();

            pickup.Initialize(
                stack.Item,
                stack.Quantity,
                stack.Quality,
                stack.Condition
            );

            Rigidbody rigidbody =
                worldObject.GetComponent<Rigidbody>() ??
                worldObject.AddComponent<Rigidbody>();

            rigidbody.mass = 0.6f;
            rigidbody.linearVelocity = velocity;
            rigidbody.angularVelocity = Random.insideUnitSphere * 2f;

            return pickup;
        }

        private static void ApplyFallbackColor(
            GameObject target,
            Color color
        )
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();

            if (renderer == null)
            {
                return;
            }

            Material material = renderer.material;

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
