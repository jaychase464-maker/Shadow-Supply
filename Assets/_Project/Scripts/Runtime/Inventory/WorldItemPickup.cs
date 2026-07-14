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

        private const float DroppedItemMass = 0.6f;
        private const float DroppedItemLinearDamping = 0.08f;
        private const float DroppedItemAngularDamping = 0.05f;
        private const float MinimumTumbleSpeed = 4f;
        private const float MaximumTumbleSpeed = 8f;

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

            GameObject worldObject = CreateWorldObject(stack, position);
            EnsureSolidCollider(worldObject);

            WorldItemPickup pickup =
                worldObject.GetComponent<WorldItemPickup>() ??
                worldObject.AddComponent<WorldItemPickup>();

            pickup.Initialize(
                stack.Item,
                stack.Quantity,
                stack.Quality,
                stack.Condition
            );

            ConfigureDroppedItemPhysics(worldObject, velocity);
            return pickup;
        }

        private static GameObject CreateWorldObject(
            ItemStack stack,
            Vector3 position
        )
        {
            GameObject worldObject;

            if (stack.Item.DisplayPrefab != null)
            {
                worldObject = Instantiate(
                    stack.Item.DisplayPrefab,
                    position,
                    Random.rotation
                );
            }
            else
            {
                worldObject = GameObject.CreatePrimitive(
                    stack.Item.FallbackPrimitive
                );

                worldObject.transform.position = position;
                worldObject.transform.rotation = Random.rotation;
                worldObject.transform.localScale = Vector3.one * 0.32f;
                ApplyFallbackColor(worldObject, stack.Item.FallbackColor);
            }

            worldObject.transform.SetParent(null, true);
            return worldObject;
        }

        private static void EnsureSolidCollider(GameObject worldObject)
        {
            Collider[] colliders =
                worldObject.GetComponentsInChildren<Collider>(true);

            bool hasEnabledSolidCollider = false;

            foreach (Collider collider in colliders)
            {
                if (collider != null && collider.enabled && !collider.isTrigger)
                {
                    hasEnabledSolidCollider = true;
                    break;
                }
            }

            if (!hasEnabledSolidCollider)
            {
                worldObject.AddComponent<BoxCollider>();
            }
        }

        private static void ConfigureDroppedItemPhysics(
            GameObject worldObject,
            Vector3 initialVelocity
        )
        {
            Rigidbody rigidbody = worldObject.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                rigidbody = worldObject.AddComponent<Rigidbody>();
            }

            rigidbody.mass = DroppedItemMass;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.detectCollisions = true;
            rigidbody.constraints = RigidbodyConstraints.None;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;
            rigidbody.linearDamping = DroppedItemLinearDamping;
            rigidbody.angularDamping = DroppedItemAngularDamping;
            rigidbody.maxAngularVelocity = MaximumTumbleSpeed * 2f;

            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.WakeUp();

            rigidbody.AddForce(
                initialVelocity,
                ForceMode.VelocityChange
            );

            rigidbody.AddTorque(
                Random.onUnitSphere *
                Random.Range(MinimumTumbleSpeed, MaximumTumbleSpeed),
                ForceMode.VelocityChange
            );
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
