using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.SaveSystem
{
    [Serializable]
    public sealed class SaveGameData
    {
        public int saveVersion;
        public string gameVersion;
        public SaveSlotMetadata metadata =
            new SaveSlotMetadata();
        public PlayerSaveData player =
            new PlayerSaveData();
        public InventorySaveData inventory =
            new InventorySaveData();
        public int selectedHotbarIndex;
        public List<WorldItemSaveData> worldItems =
            new List<WorldItemSaveData>();
    }

    [Serializable]
    public sealed class SaveSlotMetadata
    {
        public int slotNumber;
        public string savedAtUtc;
        public string sceneName;
        public double playTimeSeconds;
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public Vector3SaveData position =
            new Vector3SaveData();
        public float yaw;
        public float cameraPitch;
    }

    [Serializable]
    public sealed class InventorySaveData
    {
        public int slotCount;
        public List<InventorySlotSaveData> slots =
            new List<InventorySlotSaveData>();
    }

    [Serializable]
    public sealed class InventorySlotSaveData
    {
        public int slotIndex;
        public string itemId;
        public int quantity;
        public int quality;
        public float condition;
    }

    [Serializable]
    public sealed class WorldItemSaveData
    {
        public string persistentId;
        public string itemId;
        public int quantity;
        public int quality;
        public float condition;
        public Vector3SaveData position =
            new Vector3SaveData();
        public QuaternionSaveData rotation =
            new QuaternionSaveData();
        public bool hasPhysics;
        public Vector3SaveData linearVelocity =
            new Vector3SaveData();
        public Vector3SaveData angularVelocity =
            new Vector3SaveData();
    }

    [Serializable]
    public sealed class Vector3SaveData
    {
        public float x;
        public float y;
        public float z;

        public Vector3SaveData()
        {
        }

        public Vector3SaveData(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public sealed class QuaternionSaveData
    {
        public float x;
        public float y;
        public float z;
        public float w = 1f;

        public QuaternionSaveData()
        {
        }

        public QuaternionSaveData(Quaternion value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
}
