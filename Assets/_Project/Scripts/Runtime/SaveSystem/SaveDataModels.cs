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
        public List<PlacedObjectSaveData> placedObjects =
            new List<PlacedObjectSaveData>();
        public WalletSaveData wallet =
            new WalletSaveData();
        public List<FurnitureDeliverySaveData> furnitureDeliveries =
            new List<FurnitureDeliverySaveData>();
        public CharacterAppearanceSaveData characterAppearance =
            new CharacterAppearanceSaveData();
        public ElectricalSystemSaveData electrical =
            new ElectricalSystemSaveData();
        public List<ProductionWorkbenchSaveData>
            productionWorkbenches =
                new List<ProductionWorkbenchSaveData>();
        public List<BuyerRelationshipSaveData>
            buyerRelationships =
                new List<BuyerRelationshipSaveData>();
        public List<SupplierRelationshipSaveData>
            supplierRelationships =
                new List<SupplierRelationshipSaveData>();
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
    public sealed class WalletSaveData
    {
        public int cleanCash = 1200;
        public int dirtyCash;
    }

    [Serializable]
    public sealed class FurnitureDeliverySaveData
    {
        public string persistentId;
        public string itemId;
        public int quantity;
        public Vector3SaveData position =
            new Vector3SaveData();
        public QuaternionSaveData rotation =
            new QuaternionSaveData();
    }

    [Serializable]
    public sealed class PlacedObjectSaveData
    {
        public string persistentId;
        public string placeableId;
        public Vector3SaveData position =
            new Vector3SaveData();
        public QuaternionSaveData rotation =
            new QuaternionSaveData();
    }

    [Serializable]
    public sealed class ElectricalSystemSaveData
    {
        public bool mainOn = true;
        public bool mainTripped;
        public bool garageLightsOn = true;
        public List<ElectricalCircuitSaveData> circuits =
            new List<ElectricalCircuitSaveData>();
        public List<PowerPlugSaveData> plugs =
            new List<PowerPlugSaveData>();
    }

    [Serializable]
    public sealed class ElectricalCircuitSaveData
    {
        public string circuitId;
        public bool isOn = true;
        public bool isTripped;
    }

    [Serializable]
    public sealed class PowerPlugSaveData
    {
        public string persistentId;
        public string connectedOutletId;
        public int socketIndex = -1;
        public Vector3SaveData position =
            new Vector3SaveData();
        public QuaternionSaveData rotation =
            new QuaternionSaveData();
    }

    [Serializable]
    public sealed class ProductionWorkbenchSaveData
    {
        public string workbenchId;
        public string activeRecipeId;
        public float remainingSeconds;
        public bool hasPendingOutput;
        public string pendingItemId;
        public int pendingQuantity;
        public int pendingQuality;
        public float pendingCondition = 1f;
        public List<int> completedStepIndices =
            new List<int>();
    }

    [Serializable]
    public sealed class BuyerRelationshipSaveData
    {
        public string buyerId;
        public bool introductionCompleted;
        public int introductionChoice = -1;
        public int rapport;
        public int trust;
        public int respect;
        public int successfulOrders;
        public int failedOrders;
        public int declinedOrders;
        public bool referralUnlocked;
        public int orderState;
        public string activeOrderId;
        public int deliveredQuantity;
        public float deliveredQualityTotal;
        public float deliveredConditionTotal;
        public float remainingDeadlineSeconds;
        public float cooldownRemainingSeconds;
        public int lastReward;
    }

    [Serializable]
    public sealed class SupplierRelationshipSaveData
    {
        public string supplierId;
        public bool introductionCompleted;
        public int introductionChoice = -1;
        public int rapport;
        public int trust;
        public int respect;
        public int successfulPurchases;
        public int lifetimeCleanCashSpent;
        public float restockRemainingSeconds;
        public string pendingStockId;
        public int pendingQuantity;
        public int pendingTotalPrice;
        public float pendingDeliverySeconds;
        public List<SupplierStockSaveData> stock =
            new List<SupplierStockSaveData>();
    }

    [Serializable]
    public sealed class SupplierStockSaveData
    {
        public string stockId;
        public int currentStock;
    }

    [Serializable]
    public sealed class CharacterAppearanceSaveData
    {
        public string baseBodyPartId =
            "player-base-body-v1";
        public string hairPartId =
            "player-hair-buzz-cut-v1";
        public string facialHairPartId =
            "player-facial-hair-chin-curtain-v1";
        public string underwearPartId =
            "player-underwear-briefs-v1";
        public string torsoPartId;
        public string legsPartId;
        public string feetPartId;
        public string glovesPartId;
        public string headwearPartId;
        public string backpackPartId;
        public string chestAccessoryPartId;
        public string hipAccessoryPartId;
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
