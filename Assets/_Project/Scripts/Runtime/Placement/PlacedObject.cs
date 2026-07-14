using System;
using UnityEngine;

namespace ShadowSupply.Placement
{
    [DisallowMultipleComponent]
    public sealed class PlacedObject : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string persistentId;
        [SerializeField] private PlaceableDefinition definition;

        public string PersistentId => persistentId;
        public PlaceableDefinition Definition => definition;

        private void Awake()
        {
            EnsurePersistentId();
        }

        public void Initialize(
            string id,
            PlaceableDefinition placeableDefinition
        )
        {
            persistentId =
                string.IsNullOrWhiteSpace(id)
                    ? Guid.NewGuid().ToString("N")
                    : id;

            definition = placeableDefinition;

            if (definition != null)
            {
                string shortId =
                    persistentId.Length > 8
                        ? persistentId.Substring(0, 8)
                        : persistentId;

                gameObject.name =
                    $"Placed_{definition.DisplayName}_{shortId}";
            }
        }

        public void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(persistentId))
            {
                persistentId =
                    Guid.NewGuid().ToString("N");
            }
        }

        public static PlacedObject Spawn(
            PlaceableDefinition definition,
            Vector3 position,
            Quaternion rotation,
            string persistentId = null
        )
        {
            if (
                definition == null ||
                definition.Prefab == null
            )
            {
                return null;
            }

            GameObject instance = Instantiate(
                definition.Prefab,
                position,
                rotation
            );

            instance.SetActive(true);

            PlacedObject placedObject =
                instance.GetComponent<PlacedObject>() ??
                instance.AddComponent<PlacedObject>();

            placedObject.Initialize(
                persistentId,
                definition
            );

            foreach (
                Collider collider
                in instance.GetComponentsInChildren<Collider>(true)
            )
            {
                collider.enabled = true;
            }

            foreach (
                Rigidbody body
                in instance.GetComponentsInChildren<Rigidbody>(true)
            )
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
            }

            return placedObject;
        }
    }
}
