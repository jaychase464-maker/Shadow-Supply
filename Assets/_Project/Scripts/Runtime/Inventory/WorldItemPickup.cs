using System;
using ShadowSupply.Interaction;
using UnityEngine;

namespace ShadowSupply.Inventory
{
    public sealed class WorldItemPickup :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField, HideInInspector] private string persistentId;
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField] private ItemQuality quality =
            ItemQuality.Standard;
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

        public string PersistentId => persistentId;
        public ItemDefinition Item => item;
        public int Quantity => quantity;
        public ItemQuality Quality => quality;
        public float Condition => condition;

        private void Awake()
        {
            EnsurePersistentId();
        }

        private void OnValidate()
        {
            EnsurePersistentId();
            quantity = Mathf.Max(1, quantity);
            condition = Mathf.Clamp01(condition);
        }

        public void Initialize(
            ItemDefinition definition,
            int amount,
            ItemQuality itemQuality = ItemQuality.Standard,
            float itemCondition = 1f,
            string restoredPersistentId = null
        )
        {
            item = definition;
            quantity = Mathf.Max(1, amount);
            quality = itemQuality;
            condition = Mathf.Clamp01(itemCondition);

            persistentId =
                string.IsNullOrWhiteSpace(restoredPersistentId)
                    ? persistentId
                    : restoredPersistentId;

            EnsurePersistentId();

            if (item != null)
            {
                name =
                    $"Pickup_{item.DisplayName}_{quantity}";
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
            PlayerInventory inventory =
                interactor.GetComponent<PlayerInventory>();

            if (
                inventory == null ||
                item == null ||
                quantity <= 0
            )
            {
                return;
            }

            int remaining = inventory.AddItem(
                item,
                quantity,
                quality,
                condition
            );

            int collected = quantity - remaining;
            quantity = remaining;

            if (collected > 0)
            {
                Debug.Log(
                    $"[Inventory] Picked up " +
                    $"{item.DisplayName} x{collected}.",
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

            GameObject worldObject = CreateWorldObject(
                stack.Item,
                position,
                UnityEngine.Random.rotation
            );

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

            ConfigureDroppedItemPhysics(
                worldObject,
                velocity
            );

            return pickup;
        }

        public static WorldItemPickup SpawnRestored(
            string savedPersistentId,
            ItemDefinition definition,
            int amount,
            ItemQuality itemQuality,
            float itemCondition,
            Vector3 position,
            Quaternion rotation,
            bool hasPhysics,
            Vector3 linearVelocity,
            Vector3 angularVelocity
        )
        {
            if (definition == null || amount <= 0)
            {
                return null;
            }

            GameObject worldObject = CreateWorldObject(
                definition,
                position,
                rotation
            );

            EnsureSolidCollider(worldObject);

            WorldItemPickup pickup =
                worldObject.GetComponent<WorldItemPickup>() ??
                worldObject.AddComponent<WorldItemPickup>();

            pickup.Initialize(
                definition,
                amount,
                itemQuality,
                itemCondition,
                savedPersistentId
            );

            if (hasPhysics)
            {
                ConfigureRestoredPhysics(
                    worldObject,
                    linearVelocity,
                    angularVelocity
                );
            }
            else
            {
                ConfigureStaticPickup(worldObject);
            }

            return pickup;
        }

        private static GameObject CreateWorldObject(
            ItemDefinition definition,
            Vector3 position,
            Quaternion rotation
        )
        {
            GameObject worldObject;

            if (definition.DisplayPrefab != null)
            {
                worldObject = Instantiate(
                    definition.DisplayPrefab,
                    position,
                    rotation
                );
            }
            else
            {
                worldObject = GameObject.CreatePrimitive(
                    definition.FallbackPrimitive
                );

                worldObject.transform.position = position;
                worldObject.transform.rotation = rotation;
                worldObject.transform.localScale =
                    Vector3.one * 0.32f;

                ApplyFallbackColor(
                    worldObject,
                    definition.FallbackColor
                );
            }

            worldObject.transform.SetParent(null, true);
            return worldObject;
        }

        private static void EnsureSolidCollider(
            GameObject worldObject
        )
        {
            Collider[] colliders =
                worldObject.GetComponentsInChildren<Collider>(true);

            bool hasEnabledSolidCollider = false;

            foreach (Collider collider in colliders)
            {
                if (
                    collider != null &&
                    collider.enabled &&
                    !collider.isTrigger
                )
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
            Rigidbody rigidbody =
                GetOrCreateRigidbody(worldObject);

            ConfigureDynamicDefaults(rigidbody);

            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.WakeUp();

            rigidbody.AddForce(
                initialVelocity,
                ForceMode.VelocityChange
            );

            rigidbody.AddTorque(
                UnityEngine.Random.onUnitSphere *
                UnityEngine.Random.Range(
                    MinimumTumbleSpeed,
                    MaximumTumbleSpeed
                ),
                ForceMode.VelocityChange
            );
        }

        private static void ConfigureRestoredPhysics(
            GameObject worldObject,
            Vector3 linearVelocity,
            Vector3 angularVelocity
        )
        {
            Rigidbody rigidbody =
                GetOrCreateRigidbody(worldObject);

            ConfigureDynamicDefaults(rigidbody);
            rigidbody.linearVelocity = linearVelocity;
            rigidbody.angularVelocity = angularVelocity;
            rigidbody.WakeUp();
        }

        private static void ConfigureStaticPickup(
            GameObject worldObject
        )
        {
            Rigidbody rigidbody =
                worldObject.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                return;
            }

            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = true;
        }

        private static Rigidbody GetOrCreateRigidbody(
            GameObject worldObject
        )
        {
            Rigidbody rigidbody =
                worldObject.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                rigidbody =
                    worldObject.AddComponent<Rigidbody>();
            }

            return rigidbody;
        }

        private static void ConfigureDynamicDefaults(
            Rigidbody rigidbody
        )
        {
            rigidbody.mass = DroppedItemMass;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.detectCollisions = true;
            rigidbody.constraints =
                RigidbodyConstraints.None;
            rigidbody.interpolation =
                RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;
            rigidbody.linearDamping =
                DroppedItemLinearDamping;
            rigidbody.angularDamping =
                DroppedItemAngularDamping;
            rigidbody.maxAngularVelocity =
                MaximumTumbleSpeed * 2f;
        }

        private static void ApplyFallbackColor(
            GameObject target,
            Color color
        )
        {
            Renderer renderer =
                target.GetComponentInChildren<Renderer>();

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

        private void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(persistentId))
            {
                persistentId =
                    Guid.NewGuid().ToString("N");
            }
        }
    }
}
