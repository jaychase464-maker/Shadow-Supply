using System;
using UnityEngine;

namespace ShadowSupply.Placement
{
    public enum PlacementSurfaceType
    {
        Floor
    }

    [CreateAssetMenu(
        fileName = "PLACEABLE_NewObject",
        menuName = "Shadow Supply/Placement/Placeable Definition"
    )]
    public sealed class PlaceableDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, HideInInspector] private string placeableId;
        [SerializeField] private string displayName = "New Placeable";
        [SerializeField, TextArea(2, 5)] private string description;

        [Header("Prefab")]
        [SerializeField] private GameObject prefab;

        [Header("Placement")]
        [SerializeField] private PlacementSurfaceType surfaceType =
            PlacementSurfaceType.Floor;
        [SerializeField] private Vector3 boundsCenter =
            new Vector3(0f, 0.5f, 0f);
        [SerializeField] private Vector3 boundsSize =
            Vector3.one;
        [SerializeField, Min(0.05f)] private float gridSize = 0.25f;
        [SerializeField, Range(1f, 180f)] private float rotationStep = 15f;
        [SerializeField, Range(0f, 60f)] private float maximumSlope = 20f;

        public string PlaceableId => placeableId;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject Prefab => prefab;
        public PlacementSurfaceType SurfaceType => surfaceType;
        public Vector3 BoundsCenter => boundsCenter;
        public Vector3 BoundsSize => boundsSize;
        public float GridSize => gridSize;
        public float RotationStep => rotationStep;
        public float MaximumSlope => maximumSlope;

        public void EnsurePersistentId()
        {
            if (string.IsNullOrWhiteSpace(placeableId))
            {
                placeableId = Guid.NewGuid().ToString("N");
            }
        }

        private void OnValidate()
        {
            EnsurePersistentId();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }

            boundsSize.x = Mathf.Max(0.05f, boundsSize.x);
            boundsSize.y = Mathf.Max(0.05f, boundsSize.y);
            boundsSize.z = Mathf.Max(0.05f, boundsSize.z);
            gridSize = Mathf.Max(0.05f, gridSize);
            rotationStep = Mathf.Clamp(rotationStep, 1f, 180f);
            maximumSlope = Mathf.Clamp(maximumSlope, 0f, 60f);
        }
    }
}
