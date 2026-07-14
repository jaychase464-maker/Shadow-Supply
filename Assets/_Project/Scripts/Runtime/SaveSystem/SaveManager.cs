using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ShadowSupply.Delivery;
using ShadowSupply.Economy;
using ShadowSupply.Inventory;
using ShadowSupply.Player;
using ShadowSupply.Placement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.SaveSystem
{
    [DefaultExecutionOrder(-500)]
    public sealed class SaveManager : MonoBehaviour
    {
        public const int CurrentSaveVersion = 3;
        public const string CurrentGameVersion =
            "v0.5.0-furniture-ownership";

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
                    CaptureFurnitureDeliveries()
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

            if (data.saveVersion < 3)
            {
                if (data.wallet.cleanCash <= 0)
                {
                    data.wallet.cleanCash = 1200;
                }
            }

            data.saveVersion = 3;
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
