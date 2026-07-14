using UnityEngine;

namespace ShadowSupply.Properties
{
    [DisallowMultipleComponent]
    public sealed class StarterGarageProperty : MonoBehaviour
    {
        [SerializeField] private string propertyId =
            "starter-garage-01";
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform deliveryDrop;
        [SerializeField] private StarterGarageDoor overheadDoor;
        [SerializeField] private StarterGarageDoor entryDoor;

        public string PropertyId => propertyId;
        public Transform PlayerSpawn => playerSpawn;
        public Transform DeliveryDrop => deliveryDrop;
        public StarterGarageDoor OverheadDoor => overheadDoor;
        public StarterGarageDoor EntryDoor => entryDoor;

        public void Configure(
            Transform spawnPoint,
            Transform deliveryPoint,
            StarterGarageDoor garageDoor,
            StarterGarageDoor manDoor
        )
        {
            playerSpawn = spawnPoint;
            deliveryDrop = deliveryPoint;
            overheadDoor = garageDoor;
            entryDoor = manDoor;
        }
    }
}
