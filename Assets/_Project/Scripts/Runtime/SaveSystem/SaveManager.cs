using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ShadowSupply.Character;
using ShadowSupply.Delivery;
using ShadowSupply.Economy;
using ShadowSupply.Electrical;
using ShadowSupply.Inventory;
using ShadowSupply.Player;
using ShadowSupply.Placement;
using ShadowSupply.Properties;
using ShadowSupply.Relationships;
using ShadowSupply.Production;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.SaveSystem
{
    [DefaultExecutionOrder(-500)]
    public sealed class SaveManager : MonoBehaviour
    {
        public const int CurrentSaveVersion = 9;
        public const string CurrentGameVersion =
            "v0.8.7-first-supplier-relationship";

        public static SaveManager Instance { get; private set; }

        [Header("Databases")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private PlaceableDatabase placeableDatabase;

        [Header("Economy and Delivery")]
        [SerializeField] private PlayerWallet playerWallet;
        [SerializeField] private FurnitureDeliverySystem deliverySystem;

        [Header("Scene References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private HotbarController hotbarController;
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private PlayerCharacterRuntime playerCharacterRuntime;

        [Header("Slots")]
        [SerializeField, Range(1, 3)] private int activeSlot = 1;
        [SerializeField, Range(1, 10)] private int slotCount = 3;

        private SaveGameData pendingSceneLoad;

        public event Action<int> ActiveSlotChanged;
        public event Action SlotsChanged;
        public event Action<string, bool> StatusChanged;

        public int ActiveSlot => activeSlot;
        public int SlotCount => slotCount;
        public string SaveDirectoryPath =>
            Path.Combine(
                Application.persistentDataPath,
                "ShadowSupply",
                "Saves"
            );

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            ResolveSceneReferences();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Configure(
            ItemDatabase database,
            Transform player,
            PlayerInventory inventory,
            HotbarController hotbar
        )
        {
            itemDatabase = database;
            playerTransform = player;
            playerInventory = inventory;
            hotbarController = hotbar;
            firstPersonController =
                player != null
                    ? player.GetComponent<FirstPersonController>()
                    : null;
        }

        public void ConfigurePlacementDatabase(
            PlaceableDatabase database
        )
        {
            placeableDatabase = database;
        }

        public void ConfigureEconomyAndDelivery(
            PlayerWallet wallet,
            FurnitureDeliverySystem system
        )
        {
            playerWallet = wallet;
            deliverySystem = system;
        }

        public void SetActiveSlot(int slot)
        {
            int clamped = Mathf.Clamp(
                slot,
                1,
                slotCount
            );

            if (activeSlot == clamped)
            {
                return;
            }

            activeSlot = clamped;
            ActiveSlotChanged?.Invoke(activeSlot);
            PublishStatus(
                $"Selected save slot {activeSlot}.",
                true
            );
        }

        public bool SaveActiveSlot()
        {
            return SaveSlot(activeSlot);
        }

        public bool LoadActiveSlot()
        {
            return LoadSlot(activeSlot);
        }

        public bool SaveSlot(int slot)
        {
            slot = Mathf.Clamp(slot, 1, slotCount);

            if (!ValidateRuntimeReferences())
            {
                PublishStatus(
                    "Save failed: player or inventory reference is missing.",
                    false
                );
                return false;
            }

            if (itemDatabase == null)
            {
                PublishStatus(
                    "Save failed: item database is not assigned.",
                    false
                );
                return false;
            }

            try
            {
                SaveGameData data = CaptureSaveData(slot);
                string json =
                    JsonUtility.ToJson(data, true);

                Directory.CreateDirectory(
                    SaveDirectoryPath
                );

                string path = GetSavePath(slot);
                string temporaryPath = path + ".tmp";
                string backupPath = path + ".bak";

                File.WriteAllText(
                    temporaryPath,
                    json
                );

                if (File.Exists(path))
                {
                    File.Copy(
                        path,
                        backupPath,
                        true
                    );

                    File.Delete(path);
                }

                File.Move(
                    temporaryPath,
                    path
                );

                SlotsChanged?.Invoke();

                PublishStatus(
                    $"Saved slot {slot}.",
                    true
                );

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);

                PublishStatus(
                    $"Save slot {slot} failed.",
                    false
                );

                return false;
            }
        }

        public bool LoadSlot(int slot)
        {
            slot = Mathf.Clamp(slot, 1, slotCount);
            string path = GetSavePath(slot);

            if (!File.Exists(path))
            {
                PublishStatus(
                    $"Save slot {slot} is empty.",
                    false
                );
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);

                SaveGameData data =
                    JsonUtility.FromJson<SaveGameData>(json);

                if (!ValidateSaveData(data, out string error))
                {
                    PublishStatus(error, false);
                    return false;
                }

                MigrateSaveData(data);

                string targetScene =
                    data.metadata != null
                        ? data.metadata.sceneName
                        : string.Empty;

                if (
                    !string.IsNullOrWhiteSpace(targetScene) &&
                    SceneManager.GetActiveScene().name != targetScene
                )
                {
                    pendingSceneLoad = data;
                    SceneManager.LoadScene(targetScene);

                    PublishStatus(
                        $"Loading slot {slot}...",
                        true
                    );

                    return true;
                }

                ApplySaveData(data);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);

                PublishStatus(
                    $"Load slot {slot} failed.",
                    false
                );

                return false;
            }
        }

        public bool DeleteSlot(int slot)
        {
            slot = Mathf.Clamp(slot, 1, slotCount);

            bool deleted = false;

            foreach (string path in new[]
            {
                GetSavePath(slot),
                GetSavePath(slot) + ".bak",
                GetSavePath(slot) + ".tmp"
            })
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                File.Delete(path);
                deleted = true;
            }

            if (deleted)
            {
                SlotsChanged?.Invoke();

                PublishStatus(
                    $"Deleted save slot {slot}.",
                    true
                );
            }

            return deleted;
        }

        public bool TryGetMetadata(
            int slot,
            out SaveSlotMetadata metadata
        )
        {
            metadata = null;
            slot = Mathf.Clamp(slot, 1, slotCount);

            string path = GetSavePath(slot);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                SaveGameData data =
                    JsonUtility.FromJson<SaveGameData>(
                        File.ReadAllText(path)
                    );

                metadata = data?.metadata;
                return metadata != null;
            }
            catch
            {
                return false;
            }
        }

        private SaveGameData CaptureSaveData(int slot)
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            SaveGameData data = new SaveGameData
            {
                saveVersion = CurrentSaveVersion,
                gameVersion = CurrentGameVersion,
                selectedHotbarIndex =
                    hotbarController != null
                        ? hotbarController.SelectedIndex
                        : 0,
                metadata = new SaveSlotMetadata
                {
                    slotNumber = slot,
                    savedAtUtc =
                        DateTime.UtcNow.ToString("O"),
                    sceneName = activeScene.name,
                    playTimeSeconds =
                        Time.realtimeSinceStartupAsDouble
                },
                player = new PlayerSaveData
                {
                    position =
                        new Vector3SaveData(
                            playerTransform.position
                        ),
                    yaw =
                        playerTransform.eulerAngles.y,
                    cameraPitch =
                        firstPersonController != null
                            ? firstPersonController.CameraPitch
                            : 0f
                },
                inventory =
                    CaptureInventory(),
                worldItems =
                    CaptureWorldItems(),
                placedObjects =
                    CapturePlacedObjects(),
                wallet =
                    CaptureWallet(),
                furnitureDeliveries =
                    CaptureFurnitureDeliveries(),
                characterAppearance =
                    CaptureCharacterAppearance(),
                electrical =
                    CaptureElectricalSystem(),
                productionWorkbenches =
                    CaptureProductionWorkbenches(),
                buyerRelationships =
                    CaptureBuyerRelationships(),
                supplierRelationships =
                    CaptureSupplierRelationships()
            };

            return data;
        }

        private InventorySaveData CaptureInventory()
        {
            InventorySaveData data =
                new InventorySaveData
                {
                    slotCount =
                        playerInventory.SlotCount
                };

            for (
                int index = 0;
                index < playerInventory.SlotCount;
                index++
            )
            {
                InventorySlot slot =
                    playerInventory.GetSlot(index);

                InventorySlotSaveData slotData =
                    new InventorySlotSaveData
                    {
                        slotIndex = index
                    };

                if (slot != null && !slot.IsEmpty)
                {
                    ItemStack stack = slot.Stack;

                    slotData.itemId =
                        stack.Item.ItemId;
                    slotData.quantity =
                        stack.Quantity;
                    slotData.quality =
                        (int)stack.Quality;
                    slotData.condition =
                        stack.Condition;
                }

                data.slots.Add(slotData);
            }

            return data;
        }

        private List<WorldItemSaveData> CaptureWorldItems()
        {
            List<WorldItemSaveData> data =
                new List<WorldItemSaveData>();

            WorldItemPickup[] pickups =
                FindObjectsByType<WorldItemPickup>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            foreach (WorldItemPickup pickup in pickups)
            {
                if (
                    pickup == null ||
                    pickup.Item == null ||
                    pickup.Quantity <= 0
                )
                {
                    continue;
                }

                Rigidbody rigidbody =
                    pickup.GetComponent<Rigidbody>();

                bool hasPhysics =
                    rigidbody != null &&
                    !rigidbody.isKinematic;

                data.Add(
                    new WorldItemSaveData
                    {
                        persistentId =
                            pickup.PersistentId,
                        itemId =
                            pickup.Item.ItemId,
                        quantity =
                            pickup.Quantity,
                        quality =
                            (int)pickup.Quality,
                        condition =
                            pickup.Condition,
                        position =
                            new Vector3SaveData(
                                pickup.transform.position
                            ),
                        rotation =
                            new QuaternionSaveData(
                                pickup.transform.rotation
                            ),
                        hasPhysics =
                            hasPhysics,
                        linearVelocity =
                            new Vector3SaveData(
                                hasPhysics
                                    ? rigidbody.linearVelocity
                                    : Vector3.zero
                            ),
                        angularVelocity =
                            new Vector3SaveData(
                                hasPhysics
                                    ? rigidbody.angularVelocity
                                    : Vector3.zero
                            )
                    }
                );
            }

            return data;
        }

        private List<PlacedObjectSaveData> CapturePlacedObjects()
        {
            List<PlacedObjectSaveData> data =
                new List<PlacedObjectSaveData>();

            PlacedObject[] placedObjects =
                FindObjectsByType<PlacedObject>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            foreach (PlacedObject placedObject in placedObjects)
            {
                if (
                    placedObject == null ||
                    placedObject.Definition == null ||
                    string.IsNullOrWhiteSpace(
                        placedObject.Definition.PlaceableId
                    )
                )
                {
                    continue;
                }

                placedObject.EnsurePersistentId();

                data.Add(
                    new PlacedObjectSaveData
                    {
                        persistentId =
                            placedObject.PersistentId,
                        placeableId =
                            placedObject.Definition.PlaceableId,
                        position =
                            new Vector3SaveData(
                                placedObject.transform.position
                            ),
                        rotation =
                            new QuaternionSaveData(
                                placedObject.transform.rotation
                            )
                    }
                );
            }

            return data;
        }

        private WalletSaveData CaptureWallet()
        {
            return new WalletSaveData
            {
                cleanCash =
                    playerWallet != null
                        ? playerWallet.CleanCash
                        : 0,
                dirtyCash =
                    playerWallet != null
                        ? playerWallet.DirtyCash
                        : 0
            };
        }

        private List<FurnitureDeliverySaveData> CaptureFurnitureDeliveries()
        {
            List<FurnitureDeliverySaveData> data =
                new List<FurnitureDeliverySaveData>();

            FurnitureDeliveryCrate[] crates =
                FindObjectsByType<FurnitureDeliveryCrate>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            foreach (FurnitureDeliveryCrate crate in crates)
            {
                if (
                    crate == null ||
                    crate.Item == null ||
                    crate.Quantity <= 0
                )
                {
                    continue;
                }

                crate.EnsurePersistentId();

                data.Add(
                    new FurnitureDeliverySaveData
                    {
                        persistentId =
                            crate.PersistentId,
                        itemId =
                            crate.Item.ItemId,
                        quantity =
                            crate.Quantity,
                        position =
                            new Vector3SaveData(
                                crate.transform.position
                            ),
                        rotation =
                            new QuaternionSaveData(
                                crate.transform.rotation
                            )
                    }
                );
            }

            return data;
        }


        private CharacterAppearanceSaveData
            CaptureCharacterAppearance()
        {
            CharacterAppearanceSaveData data =
                new CharacterAppearanceSaveData();

            if (playerCharacterRuntime == null)
            {
                return data;
            }

            data.baseBodyPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.BaseBody,
                    data.baseBodyPartId
                );

            data.hairPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Hair,
                    data.hairPartId
                );

            data.facialHairPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.FacialHair,
                    data.facialHairPartId
                );

            data.underwearPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Underwear,
                    data.underwearPartId
                );

            data.torsoPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Torso,
                    string.Empty
                );

            data.legsPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Legs,
                    string.Empty
                );

            data.feetPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Feet,
                    string.Empty
                );

            data.glovesPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Gloves,
                    string.Empty
                );

            data.headwearPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Headwear,
                    string.Empty
                );

            data.backpackPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.Backpack,
                    string.Empty
                );

            data.chestAccessoryPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.ChestAccessory,
                    string.Empty
                );

            data.hipAccessoryPartId =
                GetEquippedPartIdOrDefault(
                    CharacterSlot.HipAccessory,
                    string.Empty
                );

            return data;
        }

        private string GetEquippedPartIdOrDefault(
            CharacterSlot slot,
            string fallback
        )
        {
            string partId =
                playerCharacterRuntime.GetEquippedPartId(slot);

            return string.IsNullOrWhiteSpace(partId)
                ? fallback
                : partId;
        }

        private ElectricalSystemSaveData
            CaptureElectricalSystem()
        {
            ElectricalSystemSaveData data =
                new ElectricalSystemSaveData();

            ElectricalPanel panel =
                FindFirstObjectByType<ElectricalPanel>();

            if (panel != null)
            {
                data.mainOn = panel.MainOn;
                data.mainTripped = panel.MainTripped;

                foreach (
                    ElectricalCircuit circuit
                    in panel.Circuits
                )
                {
                    if (circuit == null)
                    {
                        continue;
                    }

                    data.circuits.Add(
                        new ElectricalCircuitSaveData
                        {
                            circuitId =
                                circuit.CircuitId,
                            isOn =
                                circuit.IsOn,
                            isTripped =
                                circuit.IsTripped
                        }
                    );
                }
            }

            StarterGarageLightSwitch lightSwitch =
                FindFirstObjectByType<
                    StarterGarageLightSwitch
                >();

            data.garageLightsOn =
                lightSwitch == null ||
                lightSwitch.IsOn;

            PowerPlug[] plugs =
                FindObjectsByType<PowerPlug>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            foreach (PowerPlug plug in plugs)
            {
                if (plug == null)
                {
                    continue;
                }

                plug.EnsurePersistentId();

                data.plugs.Add(
                    new PowerPlugSaveData
                    {
                        persistentId =
                            plug.PersistentId,
                        connectedOutletId =
                            plug.ConnectedOutletId,
                        socketIndex =
                            plug.ConnectedSocketIndex,
                        position =
                            new Vector3SaveData(
                                plug.transform.position
                            ),
                        rotation =
                            new QuaternionSaveData(
                                plug.transform.rotation
                            )
                    }
                );
            }

            return data;
        }

        private List<ProductionWorkbenchSaveData>
            CaptureProductionWorkbenches()
        {
            List<ProductionWorkbenchSaveData> data =
                new List<ProductionWorkbenchSaveData>();

            PoweredWorkbenchProduction[] workbenches =
                FindObjectsByType<
                    PoweredWorkbenchProduction
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            foreach (
                PoweredWorkbenchProduction workbench
                in workbenches
            )
            {
                if (
                    workbench == null ||
                    string.IsNullOrWhiteSpace(
                        workbench.WorkbenchId
                    )
                )
                {
                    continue;
                }

                bool hasPending =
                    workbench.HasPendingOutput;

                ItemQuality quality =
                    hasPending
                        ? workbench.PendingOutputQuality
                        : workbench.ActiveOutputQuality;

                float condition =
                    hasPending
                        ? workbench.PendingOutputCondition
                        : workbench.ActiveOutputCondition;

                data.Add(
                    new ProductionWorkbenchSaveData
                    {
                        workbenchId =
                            workbench.WorkbenchId,
                        activeRecipeId =
                            workbench.ActiveRecipe != null
                                ? workbench.ActiveRecipe.RecipeId
                                : string.Empty,
                        remainingSeconds =
                            workbench.RemainingSeconds,
                        hasPendingOutput =
                            hasPending,
                        pendingItemId =
                            workbench.PendingOutputItem != null
                                ? workbench.PendingOutputItem.ItemId
                                : string.Empty,
                        pendingQuantity =
                            workbench.PendingOutputQuantity,
                        pendingQuality =
                            (int)quality,
                        pendingCondition =
                            condition,
                        completedStepIndices =
                            new List<int>(
                                workbench.CompletedStepIndices
                            )
                    }
                );
            }

            return data;
        }


        private List<BuyerRelationshipSaveData>
            CaptureBuyerRelationships()
        {
            List<BuyerRelationshipSaveData> data =
                new List<BuyerRelationshipSaveData>();

            BuyerNPC[] buyers =
                FindObjectsByType<BuyerNPC>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (BuyerNPC buyer in buyers)
            {
                if (
                    buyer == null ||
                    string.IsNullOrWhiteSpace(buyer.BuyerId)
                )
                {
                    continue;
                }

                data.Add(
                    new BuyerRelationshipSaveData
                    {
                        buyerId = buyer.BuyerId,
                        introductionCompleted =
                            buyer.IntroductionCompleted,
                        introductionChoice =
                            buyer.IntroductionChoice,
                        rapport = buyer.Rapport,
                        trust = buyer.Trust,
                        respect = buyer.Respect,
                        successfulOrders =
                            buyer.SuccessfulOrders,
                        failedOrders =
                            buyer.FailedOrders,
                        declinedOrders =
                            buyer.DeclinedOrders,
                        referralUnlocked =
                            buyer.ReferralUnlocked,
                        orderState =
                            (int)buyer.OrderState,
                        activeOrderId =
                            buyer.ActiveOrder != null
                                ? buyer.ActiveOrder.OrderId
                                : string.Empty,
                        deliveredQuantity =
                            buyer.DeliveredQuantity,
                        deliveredQualityTotal =
                            buyer.DeliveredQualityTotal,
                        deliveredConditionTotal =
                            buyer.DeliveredConditionTotal,
                        remainingDeadlineSeconds =
                            buyer.RemainingDeadlineSeconds,
                        cooldownRemainingSeconds =
                            buyer.CooldownRemainingSeconds,
                        lastReward =
                            buyer.LastReward
                    }
                );
            }

            return data;
        }

        private List<SupplierRelationshipSaveData>
            CaptureSupplierRelationships()
        {
            List<SupplierRelationshipSaveData> data =
                new List<SupplierRelationshipSaveData>();

            SupplierNPC[] suppliers =
                FindObjectsByType<SupplierNPC>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (
                SupplierNPC supplier
                in suppliers
            )
            {
                if (
                    supplier == null ||
                    string.IsNullOrWhiteSpace(
                        supplier.SupplierId
                    )
                )
                {
                    continue;
                }

                SupplierRelationshipSaveData
                    supplierData =
                        new SupplierRelationshipSaveData
                        {
                            supplierId =
                                supplier.SupplierId,
                            introductionCompleted =
                                supplier
                                    .IntroductionCompleted,
                            introductionChoice =
                                supplier
                                    .IntroductionChoice,
                            rapport =
                                supplier.Rapport,
                            trust =
                                supplier.Trust,
                            respect =
                                supplier.Respect,
                            successfulPurchases =
                                supplier
                                    .SuccessfulPurchases,
                            lifetimeCleanCashSpent =
                                supplier
                                    .LifetimeCleanCashSpent,
                            restockRemainingSeconds =
                                supplier
                                    .RestockRemainingSeconds,
                            pendingStockId =
                                supplier.PendingStock != null
                                    ? supplier
                                        .PendingStock.StockId
                                    : string.Empty,
                            pendingQuantity =
                                supplier.PendingQuantity,
                            pendingTotalPrice =
                                supplier.PendingTotalPrice,
                            pendingDeliverySeconds =
                                supplier
                                    .PendingDeliverySeconds
                        };

                foreach (
                    SupplierStockRuntimeState state
                    in supplier.RuntimeStock
                )
                {
                    if (
                        state?.Definition == null ||
                        string.IsNullOrWhiteSpace(
                            state.Definition.StockId
                        )
                    )
                    {
                        continue;
                    }

                    supplierData.stock.Add(
                        new SupplierStockSaveData
                        {
                            stockId =
                                state.Definition.StockId,
                            currentStock =
                                state.CurrentStock
                        }
                    );
                }

                data.Add(supplierData);
            }

            return data;
        }

        private void ApplySaveData(SaveGameData data)
        {
            ResolveSceneReferences();

            if (!ValidateRuntimeReferences())
            {
                PublishStatus(
                    "Load failed: scene player references were not found.",
                    false
                );
                return;
            }

            MigrateSaveData(data);
            RestorePlayer(data.player);
            RestoreInventory(data.inventory);
            RestorePlacedObjects(data.placedObjects);
            RestoreWorldItems(data.worldItems);
            RestoreWallet(data.wallet);
            RestoreFurnitureDeliveries(data.furnitureDeliveries);
            RestoreCharacterAppearance(data.characterAppearance);
            RestoreElectricalSystem(data.electrical);
            RestoreProductionWorkbenches(
                data.productionWorkbenches
            );
            RestoreBuyerRelationships(
                data.buyerRelationships
            );
            RestoreSupplierRelationships(
                data.supplierRelationships
            );

            if (hotbarController != null)
            {
                hotbarController.Select(
                    data.selectedHotbarIndex
                );
            }

            activeSlot = Mathf.Clamp(
                data.metadata != null
                    ? data.metadata.slotNumber
                    : activeSlot,
                1,
                slotCount
            );

            ActiveSlotChanged?.Invoke(activeSlot);
            SlotsChanged?.Invoke();

            PublishStatus(
                $"Loaded save slot {activeSlot}.",
                true
            );
        }

        private void RestorePlayer(PlayerSaveData data)
        {
            if (data == null)
            {
                return;
            }

            CharacterController characterController =
                playerTransform.GetComponent<CharacterController>();

            bool wasEnabled =
                characterController != null &&
                characterController.enabled;

            if (characterController != null)
            {
                characterController.enabled = false;
            }

            playerTransform.position =
                data.position != null
                    ? data.position.ToVector3()
                    : playerTransform.position;

            if (firstPersonController != null)
            {
                firstPersonController.SetViewRotation(
                    data.yaw,
                    data.cameraPitch
                );
            }
            else
            {
                playerTransform.rotation =
                    Quaternion.Euler(
                        0f,
                        data.yaw,
                        0f
                    );
            }

            if (characterController != null)
            {
                characterController.enabled = wasEnabled;
            }
        }

        private void RestoreInventory(
            InventorySaveData data
        )
        {
            List<ItemStack> restored =
                new List<ItemStack>();

            for (
                int i = 0;
                i < playerInventory.SlotCount;
                i++
            )
            {
                restored.Add(null);
            }

            if (data?.slots != null)
            {
                foreach (
                    InventorySlotSaveData slotData
                    in data.slots
                )
                {
                    if (
                        slotData == null ||
                        slotData.slotIndex < 0 ||
                        slotData.slotIndex >= restored.Count ||
                        string.IsNullOrWhiteSpace(slotData.itemId) ||
                        slotData.quantity <= 0
                    )
                    {
                        continue;
                    }

                    if (
                        !itemDatabase.TryGetItem(
                            slotData.itemId,
                            out ItemDefinition definition
                        )
                    )
                    {
                        Debug.LogWarning(
                            $"[SaveManager] Missing item ID " +
                            $"'{slotData.itemId}' in inventory save.",
                            this
                        );
                        continue;
                    }

                    restored[slotData.slotIndex] =
                        new ItemStack(
                            definition,
                            slotData.quantity,
                            ParseQuality(slotData.quality),
                            slotData.condition
                        );
                }
            }

            playerInventory.RestoreSlots(restored);
        }

        private void RestoreWorldItems(
            List<WorldItemSaveData> data
        )
        {
            WorldItemPickup[] currentPickups =
                FindObjectsByType<WorldItemPickup>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (WorldItemPickup pickup in currentPickups)
            {
                if (pickup == null)
                {
                    continue;
                }

                pickup.gameObject.SetActive(false);
                Destroy(pickup.gameObject);
            }

            if (data == null)
            {
                return;
            }

            foreach (WorldItemSaveData itemData in data)
            {
                if (
                    itemData == null ||
                    itemData.quantity <= 0 ||
                    string.IsNullOrWhiteSpace(itemData.itemId)
                )
                {
                    continue;
                }

                if (
                    !itemDatabase.TryGetItem(
                        itemData.itemId,
                        out ItemDefinition definition
                    )
                )
                {
                    Debug.LogWarning(
                        $"[SaveManager] Missing world item ID " +
                        $"'{itemData.itemId}'.",
                        this
                    );
                    continue;
                }

                WorldItemPickup.SpawnRestored(
                    itemData.persistentId,
                    definition,
                    itemData.quantity,
                    ParseQuality(itemData.quality),
                    itemData.condition,
                    itemData.position != null
                        ? itemData.position.ToVector3()
                        : Vector3.zero,
                    itemData.rotation != null
                        ? itemData.rotation.ToQuaternion()
                        : Quaternion.identity,
                    itemData.hasPhysics,
                    itemData.linearVelocity != null
                        ? itemData.linearVelocity.ToVector3()
                        : Vector3.zero,
                    itemData.angularVelocity != null
                        ? itemData.angularVelocity.ToVector3()
                        : Vector3.zero
                );
            }
        }

        private void RestorePlacedObjects(
            List<PlacedObjectSaveData> data
        )
        {
            PlacedObject[] currentPlacedObjects =
                FindObjectsByType<PlacedObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (
                PlacedObject placedObject
                in currentPlacedObjects
            )
            {
                if (placedObject == null)
                {
                    continue;
                }

                placedObject.gameObject.SetActive(false);
                Destroy(placedObject.gameObject);
            }

            if (data == null || data.Count == 0)
            {
                return;
            }

            if (placeableDatabase == null)
            {
                Debug.LogWarning(
                    "[SaveManager] Placed objects could not be restored " +
                    "because no PlaceableDatabase is assigned.",
                    this
                );
                return;
            }

            foreach (
                PlacedObjectSaveData objectData
                in data
            )
            {
                if (
                    objectData == null ||
                    string.IsNullOrWhiteSpace(
                        objectData.placeableId
                    )
                )
                {
                    continue;
                }

                if (
                    !placeableDatabase.TryGetDefinition(
                        objectData.placeableId,
                        out PlaceableDefinition definition
                    )
                )
                {
                    Debug.LogWarning(
                        $"[SaveManager] Missing placeable ID " +
                        $"'{objectData.placeableId}'.",
                        this
                    );
                    continue;
                }

                PlacedObject.Spawn(
                    definition,
                    objectData.position != null
                        ? objectData.position.ToVector3()
                        : Vector3.zero,
                    objectData.rotation != null
                        ? objectData.rotation.ToQuaternion()
                        : Quaternion.identity,
                    objectData.persistentId
                );
            }
        }


        private void RestoreCharacterAppearance(
            CharacterAppearanceSaveData data
        )
        {
            if (
                playerCharacterRuntime == null ||
                data == null
            )
            {
                return;
            }

            EquipAppearancePart(data.baseBodyPartId);
            EquipAppearancePart(data.hairPartId);
            EquipAppearancePart(data.facialHairPartId);
            EquipAppearancePart(data.underwearPartId);
            EquipAppearancePart(data.torsoPartId);
            EquipAppearancePart(data.legsPartId);
            EquipAppearancePart(data.feetPartId);
            EquipAppearancePart(data.glovesPartId);
            EquipAppearancePart(data.headwearPartId);
            EquipAppearancePart(data.backpackPartId);
            EquipAppearancePart(data.chestAccessoryPartId);
            EquipAppearancePart(data.hipAccessoryPartId);

            playerCharacterRuntime.RefreshVisibility();
        }

        private void EquipAppearancePart(
            string partId
        )
        {
            if (!string.IsNullOrWhiteSpace(partId))
            {
                playerCharacterRuntime.EquipPart(partId);
            }
        }

        private void RestoreElectricalSystem(
            ElectricalSystemSaveData data
        )
        {
            if (data == null)
            {
                return;
            }

            ElectricalPanel panel =
                FindFirstObjectByType<ElectricalPanel>();

            if (panel != null)
            {
                panel.RestoreMainState(
                    data.mainOn,
                    data.mainTripped
                );

                if (data.circuits != null)
                {
                    foreach (
                        ElectricalCircuitSaveData circuitData
                        in data.circuits
                    )
                    {
                        if (
                            circuitData == null ||
                            string.IsNullOrWhiteSpace(
                                circuitData.circuitId
                            )
                        )
                        {
                            continue;
                        }

                        panel.RestoreCircuitState(
                            circuitData.circuitId,
                            circuitData.isOn,
                            circuitData.isTripped
                        );
                    }
                }
            }

            PowerPlug[] currentPlugs =
                FindObjectsByType<PowerPlug>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            Dictionary<string, PowerPlug> plugsById =
                new Dictionary<string, PowerPlug>();

            foreach (PowerPlug plug in currentPlugs)
            {
                if (plug == null)
                {
                    continue;
                }

                plug.EnsurePersistentId();

                plug.RestoreFreePose(
                    plug.transform.position,
                    plug.transform.rotation
                );

                plugsById[plug.PersistentId] = plug;
            }

            PowerOutlet[] currentOutlets =
                FindObjectsByType<PowerOutlet>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            Dictionary<string, PowerOutlet> outletsById =
                new Dictionary<string, PowerOutlet>();

            foreach (PowerOutlet outlet in currentOutlets)
            {
                if (
                    outlet != null &&
                    !string.IsNullOrWhiteSpace(
                        outlet.OutletId
                    )
                )
                {
                    outletsById[outlet.OutletId] =
                        outlet;
                }
            }

            if (data.plugs != null)
            {
                foreach (
                    PowerPlugSaveData plugData
                    in data.plugs
                )
                {
                    if (
                        plugData == null ||
                        string.IsNullOrWhiteSpace(
                            plugData.persistentId
                        ) ||
                        !plugsById.TryGetValue(
                            plugData.persistentId,
                            out PowerPlug plug
                        )
                    )
                    {
                        continue;
                    }

                    Vector3 restoredPosition =
                        plugData.position != null
                            ? plugData.position.ToVector3()
                            : plug.transform.position;

                    Quaternion restoredRotation =
                        plugData.rotation != null
                            ? plugData.rotation.ToQuaternion()
                            : plug.transform.rotation;

                    plug.RestoreFreePose(
                        restoredPosition,
                        restoredRotation
                    );

                    if (
                        string.IsNullOrWhiteSpace(
                            plugData.connectedOutletId
                        ) ||
                        plugData.socketIndex < 0 ||
                        !outletsById.TryGetValue(
                            plugData.connectedOutletId,
                            out PowerOutlet outlet
                        )
                    )
                    {
                        continue;
                    }

                    OutletSocket socket =
                        outlet.GetSocket(
                            plugData.socketIndex
                        );

                    if (socket != null)
                    {
                        plug.ConnectTo(
                            socket,
                            true
                        );
                    }
                }
            }

            StarterGarageLightSwitch lightSwitch =
                FindFirstObjectByType<
                    StarterGarageLightSwitch
                >();

            lightSwitch?.SetLights(
                data.garageLightsOn
            );

            ElectricalGridSystem grid =
                FindFirstObjectByType<
                    ElectricalGridSystem
                >();

            grid?.ForceRecalculate();
        }

        private void RestoreProductionWorkbenches(
            List<ProductionWorkbenchSaveData> data
        )
        {
            PoweredWorkbenchProduction[] currentWorkbenches =
                FindObjectsByType<
                    PoweredWorkbenchProduction
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            Dictionary<
                string,
                PoweredWorkbenchProduction
            > workbenchesById =
                new Dictionary<
                    string,
                    PoweredWorkbenchProduction
                >(StringComparer.Ordinal);

            foreach (
                PoweredWorkbenchProduction workbench
                in currentWorkbenches
            )
            {
                if (
                    workbench == null ||
                    string.IsNullOrWhiteSpace(
                        workbench.WorkbenchId
                    )
                )
                {
                    continue;
                }

                workbenchesById[
                    workbench.WorkbenchId
                ] = workbench;

                workbench.RestoreProductionState(
                    string.Empty,
                    false,
                    string.Empty,
                    0,
                    ItemQuality.Standard,
                    1f,
                    Array.Empty<int>(),
                    itemDatabase
                );
            }

            if (data == null)
            {
                return;
            }

            foreach (
                ProductionWorkbenchSaveData workbenchData
                in data
            )
            {
                if (
                    workbenchData == null ||
                    string.IsNullOrWhiteSpace(
                        workbenchData.workbenchId
                    ) ||
                    !workbenchesById.TryGetValue(
                        workbenchData.workbenchId,
                        out PoweredWorkbenchProduction
                            workbench
                    )
                )
                {
                    continue;
                }

                workbench.RestoreProductionState(
                    workbenchData.activeRecipeId,
                    workbenchData.hasPendingOutput,
                    workbenchData.pendingItemId,
                    workbenchData.pendingQuantity,
                    ParseQuality(
                        workbenchData.pendingQuality
                    ),
                    workbenchData.pendingCondition,
                    workbenchData.completedStepIndices,
                    itemDatabase
                );
            }
        }


        private void RestoreBuyerRelationships(
            List<BuyerRelationshipSaveData> data
        )
        {
            BuyerNPC[] currentBuyers =
                FindObjectsByType<BuyerNPC>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            Dictionary<string, BuyerNPC> buyersById =
                new Dictionary<string, BuyerNPC>(
                    StringComparer.Ordinal
                );

            foreach (BuyerNPC buyer in currentBuyers)
            {
                if (
                    buyer == null ||
                    string.IsNullOrWhiteSpace(buyer.BuyerId)
                )
                {
                    continue;
                }

                buyer.ResetToDefaultState();
                buyersById[buyer.BuyerId] = buyer;
            }

            if (data == null)
            {
                return;
            }

            foreach (BuyerRelationshipSaveData buyerData in data)
            {
                if (
                    buyerData == null ||
                    string.IsNullOrWhiteSpace(buyerData.buyerId) ||
                    !buyersById.TryGetValue(
                        buyerData.buyerId,
                        out BuyerNPC buyer
                    )
                )
                {
                    continue;
                }

                BuyerOrderState restoredOrderState =
                    Enum.IsDefined(
                        typeof(BuyerOrderState),
                        buyerData.orderState
                    )
                        ? (BuyerOrderState)buyerData.orderState
                        : BuyerOrderState.None;

                buyer.RestoreState(
                    buyerData.introductionCompleted,
                    buyerData.introductionChoice,
                    buyerData.rapport,
                    buyerData.trust,
                    buyerData.respect,
                    buyerData.successfulOrders,
                    buyerData.failedOrders,
                    buyerData.declinedOrders,
                    buyerData.referralUnlocked,
                    restoredOrderState,
                    buyerData.activeOrderId,
                    buyerData.deliveredQuantity,
                    buyerData.deliveredQualityTotal,
                    buyerData.deliveredConditionTotal,
                    buyerData.remainingDeadlineSeconds,
                    buyerData.cooldownRemainingSeconds,
                    buyerData.lastReward
                );
            }
        }

        private void RestoreSupplierRelationships(
            List<SupplierRelationshipSaveData> data
        )
        {
            SupplierNPC[] currentSuppliers =
                FindObjectsByType<SupplierNPC>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            Dictionary<string, SupplierNPC>
                suppliersById =
                    new Dictionary<string, SupplierNPC>(
                        StringComparer.Ordinal
                    );

            foreach (
                SupplierNPC supplier
                in currentSuppliers
            )
            {
                if (
                    supplier == null ||
                    string.IsNullOrWhiteSpace(
                        supplier.SupplierId
                    )
                )
                {
                    continue;
                }

                supplier.ResetToDefaultState();

                suppliersById[
                    supplier.SupplierId
                ] = supplier;
            }

            if (data == null)
            {
                return;
            }

            foreach (
                SupplierRelationshipSaveData
                    supplierData
                in data
            )
            {
                if (
                    supplierData == null ||
                    string.IsNullOrWhiteSpace(
                        supplierData.supplierId
                    ) ||
                    !suppliersById.TryGetValue(
                        supplierData.supplierId,
                        out SupplierNPC supplier
                    )
                )
                {
                    continue;
                }

                supplier.RestoreState(
                    supplierData
                        .introductionCompleted,
                    supplierData
                        .introductionChoice,
                    supplierData.rapport,
                    supplierData.trust,
                    supplierData.respect,
                    supplierData
                        .successfulPurchases,
                    supplierData
                        .lifetimeCleanCashSpent,
                    supplierData
                        .restockRemainingSeconds,
                    supplierData.pendingStockId,
                    supplierData.pendingQuantity,
                    supplierData.pendingTotalPrice,
                    supplierData
                        .pendingDeliverySeconds,
                    supplierData.stock
                );
            }
        }

        private static void MigrateSaveData(
            SaveGameData data
        )
        {
            if (data == null)
            {
                return;
            }

            data.metadata ??=
                new SaveSlotMetadata();

            data.player ??=
                new PlayerSaveData();

            data.inventory ??=
                new InventorySaveData();

            data.inventory.slots ??=
                new List<InventorySlotSaveData>();

            data.worldItems ??=
                new List<WorldItemSaveData>();

            data.placedObjects ??=
                new List<PlacedObjectSaveData>();

            data.wallet ??=
                new WalletSaveData();

            data.furnitureDeliveries ??=
                new List<FurnitureDeliverySaveData>();

            data.characterAppearance ??=
                new CharacterAppearanceSaveData();

            data.electrical ??=
                new ElectricalSystemSaveData();

            data.electrical.circuits ??=
                new List<ElectricalCircuitSaveData>();

            data.electrical.plugs ??=
                new List<PowerPlugSaveData>();

            data.productionWorkbenches ??=
                new List<ProductionWorkbenchSaveData>();

            data.buyerRelationships ??=
                new List<BuyerRelationshipSaveData>();

            data.supplierRelationships ??=
                new List<SupplierRelationshipSaveData>();

            foreach (
                ProductionWorkbenchSaveData workbench
                in data.productionWorkbenches
            )
            {
                if (workbench != null)
                {
                    workbench.completedStepIndices ??=
                        new List<int>();
                }
            }

            if (data.saveVersion < 3)
            {
                if (data.wallet.cleanCash <= 0)
                {
                    data.wallet.cleanCash = 1200;
                }
            }

            if (data.saveVersion < 4)
            {
                data.characterAppearance =
                    new CharacterAppearanceSaveData();
            }

            if (data.saveVersion < 5)
            {
                data.electrical =
                    new ElectricalSystemSaveData();
            }

            if (data.saveVersion < 6)
            {
                data.productionWorkbenches =
                    new List<ProductionWorkbenchSaveData>();
            }

            if (data.saveVersion < 7)
            {
                foreach (
                    ProductionWorkbenchSaveData workbench
                    in data.productionWorkbenches
                )
                {
                    if (workbench != null)
                    {
                        workbench.completedStepIndices =
                            new List<int>();
                    }
                }
            }

            if (data.saveVersion < 8)
            {
                data.buyerRelationships =
                    new List<BuyerRelationshipSaveData>();
            }

            if (data.saveVersion < 9)
            {
                data.supplierRelationships =
                    new List<
                        SupplierRelationshipSaveData
                    >();
            }

            foreach (
                SupplierRelationshipSaveData supplier
                in data.supplierRelationships
            )
            {
                if (supplier != null)
                {
                    supplier.stock ??=
                        new List<SupplierStockSaveData>();
                }
            }

            data.saveVersion = 9;
        }

        private void RestoreWallet(
            WalletSaveData data
        )
        {
            if (playerWallet == null)
            {
                return;
            }

            playerWallet.Restore(
                data != null
                    ? data.cleanCash
                    : 1200,
                data != null
                    ? data.dirtyCash
                    : 0
            );
        }

        private void RestoreFurnitureDeliveries(
            List<FurnitureDeliverySaveData> data
        )
        {
            if (deliverySystem == null)
            {
                return;
            }

            deliverySystem.ClearCurrentDeliveries();

            if (data == null)
            {
                return;
            }

            foreach (
                FurnitureDeliverySaveData deliveryData
                in data
            )
            {
                if (
                    deliveryData == null ||
                    deliveryData.quantity <= 0 ||
                    string.IsNullOrWhiteSpace(
                        deliveryData.itemId
                    )
                )
                {
                    continue;
                }

                if (
                    !itemDatabase.TryGetItem(
                        deliveryData.itemId,
                        out ItemDefinition definition
                    )
                )
                {
                    Debug.LogWarning(
                        $"[SaveManager] Missing delivery item ID " +
                        $"'{deliveryData.itemId}'.",
                        this
                    );
                    continue;
                }

                deliverySystem.SpawnRestoredDelivery(
                    deliveryData.persistentId,
                    definition,
                    deliveryData.quantity,
                    deliveryData.position != null
                        ? deliveryData.position.ToVector3()
                        : Vector3.zero,
                    deliveryData.rotation != null
                        ? deliveryData.rotation.ToQuaternion()
                        : Quaternion.identity
                );
            }
        }

        private bool ValidateSaveData(
            SaveGameData data,
            out string error
        )
        {
            if (data == null)
            {
                error =
                    "Load failed: the save file is empty or invalid.";
                return false;
            }

            if (data.saveVersion <= 0)
            {
                error =
                    "Load failed: the save file has no version.";
                return false;
            }

            if (data.saveVersion > CurrentSaveVersion)
            {
                error =
                    $"Load failed: save version " +
                    $"{data.saveVersion} is newer than supported " +
                    $"version {CurrentSaveVersion}.";
                return false;
            }

            error = null;
            return true;
        }

        private bool ValidateRuntimeReferences()
        {
            ResolveSceneReferences();

            return
                playerTransform != null &&
                playerInventory != null;
        }

        private void ResolveSceneReferences()
        {
            if (playerInventory == null)
            {
                playerInventory =
                    FindFirstObjectByType<PlayerInventory>();
            }

            if (
                playerTransform == null &&
                playerInventory != null
            )
            {
                playerTransform =
                    playerInventory.transform;
            }

            if (
                hotbarController == null &&
                playerInventory != null
            )
            {
                hotbarController =
                    playerInventory.GetComponent<HotbarController>();
            }

            if (playerWallet == null)
            {
                playerWallet =
                    FindFirstObjectByType<PlayerWallet>();
            }

            if (deliverySystem == null)
            {
                deliverySystem =
                    FindFirstObjectByType<FurnitureDeliverySystem>();
            }

            if (
                firstPersonController == null &&
                playerTransform != null
            )
            {
                firstPersonController =
                    playerTransform.GetComponent<FirstPersonController>();
            }

            if (playerCharacterRuntime == null)
            {
                playerCharacterRuntime =
                    FindFirstObjectByType<PlayerCharacterRuntime>();
            }
        }

        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode
        )
        {
            playerTransform = null;
            playerInventory = null;
            hotbarController = null;
            firstPersonController = null;
            playerCharacterRuntime = null;

            ResolveSceneReferences();

            if (pendingSceneLoad != null)
            {
                StartCoroutine(
                    ApplyPendingLoadNextFrame()
                );
            }
        }

        private IEnumerator ApplyPendingLoadNextFrame()
        {
            yield return null;

            SaveGameData data = pendingSceneLoad;
            pendingSceneLoad = null;
            ApplySaveData(data);
        }

        private string GetSavePath(int slot)
        {
            return Path.Combine(
                SaveDirectoryPath,
                $"slot_{slot}.json"
            );
        }

        private static ItemQuality ParseQuality(int value)
        {
            return Enum.IsDefined(
                typeof(ItemQuality),
                value
            )
                ? (ItemQuality)value
                : ItemQuality.Standard;
        }

        private void PublishStatus(
            string message,
            bool success
        )
        {
            if (success)
            {
                Debug.Log(
                    $"[SaveManager] {message}",
                    this
                );
            }
            else
            {
                Debug.LogWarning(
                    $"[SaveManager] {message}",
                    this
                );
            }

            StatusChanged?.Invoke(
                message,
                success
            );
        }
    }
}
