using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Delivery;
using ShadowSupply.Economy;
using ShadowSupply.Inventory;
using ShadowSupply.Placement;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone5FurnitureOwnershipSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        private const string PlaceableDataFolder =
            "Assets/_Project/Data/Placeables/Development";

        private const string ItemDataFolder =
            "Assets/_Project/Data/Items/Furniture";

        private const string DatabaseFolder =
            "Assets/_Project/Data/Databases";

        private const string PlaceableDatabasePath =
            DatabaseFolder + "/DB_Placeables.asset";

        private const string ItemDatabasePath =
            DatabaseFolder + "/DB_Items.asset";

        private const string PrefabFolder =
            "Assets/_Project/Prefabs/Placeables/Furniture";

        private const string DeliveryPrefabFolder =
            "Assets/_Project/Prefabs/Delivery";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/Furniture";

        [MenuItem(
            "Shadow Supply/Setup/Apply Milestone 5 Furniture Ownership"
        )]
        public static void ApplyMilestoneFive()
        {
            Scene scene =
                OpenDevelopmentScene();

            if (!scene.IsValid())
            {
                return;
            }

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                ShowError("The development scene has no Player.");
                return;
            }

            PlayerInventory inventory =
                player.GetComponent<PlayerInventory>();

            PlacementController placementController =
                player.GetComponent<PlacementController>();

            SaveManager saveManager =
                UnityEngine.Object.FindFirstObjectByType<SaveManager>();

            if (
                inventory == null ||
                placementController == null ||
                saveManager == null
            )
            {
                ShowError(
                    "Milestones 2 through 4 must be installed first."
                );
                return;
            }

            EnsureProjectFolders();

            GameObject packagePrefab =
                CreateFurniturePackagePrefab();

            GameObject workbenchPrefab =
                CreateImportedFurniturePrefab(
                    "Workbench",
                    ImportedPath("Workbench/work_bench.fbx"),
                    PrefabFolder + "/PREFAB_Workbench.prefab",
                    CreateMaterial(
                        MaterialFolder + "/MAT_Workbench.mat",
                        ImportedPath(
                            "Workbench/work_bench_Albedo.png"
                        ),
                        ImportedPath(
                            "Workbench/work_bench_Normal.png"
                        ),
                        ImportedPath(
                            "Workbench/work_bench_Metallic.png"
                        ),
                        ImportedPath(
                            "Workbench/work_bench_AO.png"
                        ),
                        0.28f
                    ),
                    new Vector3(2.2f, 1.9f, 0.9f),
                    PlacementSurfaceType.Floor,
                    2.6f,
                    FurnitureColliderProfile.Workbench
                );

            GameObject rackPrefab =
                CreateImportedFurniturePrefab(
                    "Storage Rack",
                    ImportedPath(
                        "PalletRack/" +
                        "Metal_Pallet_Racking_wbslaikga_Mid.fbx"
                    ),
                    PrefabFolder + "/PREFAB_StorageRack.prefab",
                    CreateMaterial(
                        MaterialFolder + "/MAT_StorageRack.mat",
                        ImportedPath(
                            "PalletRack/" +
                            "Metal_Pallet_Racking_wbslaikga_Mid_2K_BaseColor.jpg"
                        ),
                        ImportedPath(
                            "PalletRack/" +
                            "Metal_Pallet_Racking_wbslaikga_Mid_2K_Normal.jpg"
                        ),
                        null,
                        ImportedPath(
                            "PalletRack/" +
                            "Metal_Pallet_Racking_wbslaikga_Mid_2K_AO.jpg"
                        ),
                        0.22f
                    ),
                    new Vector3(2.4f, 2.25f, 0.8f),
                    PlacementSurfaceType.Floor,
                    1f,
                    FurnitureColliderProfile.FullBox
                );

            GameObject deskPrefab =
                CreateImportedFurniturePrefab(
                    "Office Desk",
                    ImportedPath(
                        "OfficeDesk/SM_Med_Office_Desk.fbx"
                    ),
                    PrefabFolder + "/PREFAB_OfficeDesk.prefab",
                    CreateMaterial(
                        MaterialFolder + "/MAT_OfficeDesk.mat",
                        ImportedPath(
                            "OfficeDesk/" +
                            "T_Med_Office_Desk_BaseColor.png"
                        ),
                        ImportedPath(
                            "OfficeDesk/" +
                            "T_Med_Office_Desk_Normal.png"
                        ),
                        ImportedPath(
                            "OfficeDesk/" +
                            "T_Med_Office_Desk_Metallic.png"
                        ),
                        ImportedPath(
                            "OfficeDesk/" +
                            "T_Med_Office_Desk_AO.png"
                        ),
                        0.3f
                    ),
                    new Vector3(2.5f, 1.05f, 1.0f),
                    PlacementSurfaceType.Floor,
                    2.3f,
                    FurnitureColliderProfile.OfficeDesk
                );

            GameObject cabinetPrefab =
                CreateUtilityCabinetFallbackPrefab();

            GameObject cameraPrefab =
                CreateImportedFurniturePrefab(
                    "CCTV Camera",
                    ImportedPath("CCTV/Camera.fbx"),
                    PrefabFolder + "/PREFAB_CCTVCamera.prefab",
                    CreateMaterial(
                        MaterialFolder + "/MAT_CCTVCamera.mat",
                        ImportedPath(
                            "CCTV/CameraMainAlbedo.png"
                        ),
                        ImportedPath(
                            "CCTV/CameraMainNormal.png"
                        ),
                        ImportedPath(
                            "CCTV/CameraMainMetal.png"
                        ),
                        ImportedPath(
                            "CCTV/CameraMainAO.png"
                        ),
                        0.35f
                    ),
                    new Vector3(0.5f, 0.35f, 0.55f),
                    PlacementSurfaceType.Wall,
                    1f,
                    FurnitureColliderProfile.FullBox
                );

            GameObject keypadPrefab =
                CreateImportedFurniturePrefab(
                    "Door Keypad",
                    ImportedPath("Keypad/KeypadDoorLock.fbx"),
                    PrefabFolder + "/PREFAB_DoorKeypad.prefab",
                    CreateMaterial(
                        MaterialFolder + "/MAT_DoorKeypad.mat",
                        ImportedPath(
                            "Keypad/KeypadDoorLockAlbedo.png"
                        ),
                        ImportedPath(
                            "Keypad/KeypadDoorLockNormal.png"
                        ),
                        ImportedPath(
                            "Keypad/KeypadDoorLockMetal.png"
                        ),
                        ImportedPath(
                            "Keypad/KeypadDoorLockAO.png"
                        ),
                        0.38f
                    ),
                    new Vector3(0.22f, 0.34f, 0.08f),
                    PlacementSurfaceType.Wall,
                    1f,
                    FurnitureColliderProfile.FullBox
                );

            FurnitureSetupRecord[] records =
            {
                CreateFurnitureRecord(
                    "Workbench",
                    "A sturdy assembly bench for future crafting and production.",
                    250,
                    "ITEM_WorkbenchPackage",
                    "PLACEABLE_DevWorkbench",
                    workbenchPrefab,
                    packagePrefab,
                    PlacementSurfaceType.Floor,
                    new Vector3(0f, 0.95f, 0f),
                    new Vector3(2.2f, 1.9f, 0.9f)
                ),
                CreateFurnitureRecord(
                    "Storage Rack",
                    "Heavy pallet racking prepared for physical product storage.",
                    210,
                    "ITEM_StorageRackPackage",
                    "PLACEABLE_DevStorageShelf",
                    rackPrefab,
                    packagePrefab,
                    PlacementSurfaceType.Floor,
                    new Vector3(0f, 1.125f, 0f),
                    new Vector3(2.4f, 2.25f, 0.8f)
                ),
                CreateFurnitureRecord(
                    "Office Desk",
                    "A professional desk for computers and paperwork.",
                    185,
                    "ITEM_OfficeDeskPackage",
                    "PLACEABLE_OfficeDesk",
                    deskPrefab,
                    packagePrefab,
                    PlacementSurfaceType.Floor,
                    new Vector3(0f, 0.525f, 0f),
                    new Vector3(2.5f, 1.05f, 1.0f)
                ),
                CreateFurnitureRecord(
                    "Utility Cabinet",
                    "A lockable cabinet for tools and restricted supplies.",
                    160,
                    "ITEM_UtilityCabinetPackage",
                    "PLACEABLE_DevUtilityCabinet",
                    cabinetPrefab,
                    packagePrefab,
                    PlacementSurfaceType.Floor,
                    new Vector3(0f, 0.75f, 0f),
                    new Vector3(1.15f, 1.5f, 0.55f)
                ),
                CreateFurnitureRecord(
                    "CCTV Camera",
                    "A wall-mounted surveillance hardware foundation.",
                    145,
                    "ITEM_CCTVCameraPackage",
                    "PLACEABLE_CCTVCamera",
                    cameraPrefab,
                    packagePrefab,
                    PlacementSurfaceType.Wall,
                    new Vector3(0f, 0f, 0.275f),
                    new Vector3(0.5f, 0.35f, 0.55f)
                ),
                CreateFurnitureRecord(
                    "Door Keypad",
                    "A wall-mounted access-control hardware foundation.",
                    125,
                    "ITEM_DoorKeypadPackage",
                    "PLACEABLE_DoorKeypad",
                    keypadPrefab,
                    packagePrefab,
                    PlacementSurfaceType.Wall,
                    new Vector3(0f, 0f, 0.04f),
                    new Vector3(0.22f, 0.34f, 0.08f)
                )
            };

            PlaceableDatabase placeableDatabase =
                GetOrCreatePlaceableDatabase(
                    records.Select(
                        record => record.placeable
                    )
                );

            UpdateItemDatabase(
                records.Select(
                    record => record.item
                )
            );

            Camera playerCamera =
                player.GetComponentInChildren<Camera>(true);

            placementController.Configure(
                playerCamera,
                placeableDatabase,
                inventory
            );

            GameObject services =
                GameObject.Find("RuntimeServices");

            if (services == null)
            {
                services =
                    new GameObject("RuntimeServices");
            }

            PlayerWallet wallet =
                services.GetComponent<PlayerWallet>();

            if (wallet == null)
            {
                wallet =
                    services.AddComponent<PlayerWallet>();

                wallet.ConfigureStartingCash(1200);
            }

            GameObject milestoneRoot =
                GameObject.Find(
                    "Milestone5_FurnitureOwnership"
                );

            if (milestoneRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    milestoneRoot
                );
            }

            milestoneRoot =
                new GameObject(
                    "Milestone5_FurnitureOwnership"
                );

            Transform deliveryPoint =
                CreateDeliveryPoint(
                    milestoneRoot.transform
                );

            GameObject cratePrefab =
                CreateDeliveryCratePrefab();

            FurnitureDeliverySystem deliverySystem =
                services.GetComponent<FurnitureDeliverySystem>();

            if (deliverySystem == null)
            {
                deliverySystem =
                    services.AddComponent<FurnitureDeliverySystem>();
            }

            deliverySystem.Configure(
                wallet,
                deliveryPoint,
                cratePrefab
            );

            if (
                services.GetComponent<FurnitureShopHUD>() == null
            )
            {
                services.AddComponent<FurnitureShopHUD>();
            }

            CreateSupplierTerminal(
                milestoneRoot.transform,
                deliverySystem
            );

            saveManager.ConfigurePlacementDatabase(
                placeableDatabase
            );

            saveManager.ConfigureEconomyAndDelivery(
                wallet,
                deliverySystem
            );

            EnsureStarterOwnership(
                inventory,
                records[0].item
            );

            EditorUtility.SetDirty(inventory);
            EditorUtility.SetDirty(placementController);
            EditorUtility.SetDirty(wallet);
            EditorUtility.SetDirty(deliverySystem);
            EditorUtility.SetDirty(saveManager);
            EditorUtility.SetDirty(placeableDatabase);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                milestoneRoot;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 5 furniture ownership applied.\n\n" +
                "E at supplier terminal: open furniture shop\n" +
                "E at delivery crate: collect furniture\n" +
                "B: enter build mode\n" +
                "1–6: select owned furniture\n" +
                "Left Click: place and consume one package\n" +
                "Delete: pack targeted furniture into inventory",
                "Done"
            );
        }

        private static FurnitureSetupRecord CreateFurnitureRecord(
            string displayName,
            string description,
            int price,
            string itemAssetName,
            string placeableAssetName,
            GameObject furniturePrefab,
            GameObject packagePrefab,
            PlacementSurfaceType surfaceType,
            Vector3 boundsCenter,
            Vector3 boundsSize
        )
        {
            ItemDefinition item =
                GetOrCreateFurnitureItem(
                    ItemDataFolder + "/" +
                    itemAssetName + ".asset",
                    displayName + " Package",
                    $"A packaged {displayName.ToLowerInvariant()} " +
                    "ready for placement.",
                    price,
                    packagePrefab
                );

            PlaceableDefinition placeable =
                GetOrCreatePlaceable(
                    PlaceableDataFolder + "/" +
                    placeableAssetName + ".asset",
                    displayName,
                    description,
                    price,
                    item,
                    furniturePrefab,
                    surfaceType,
                    boundsCenter,
                    boundsSize
                );

            return new FurnitureSetupRecord
            {
                item = item,
                placeable = placeable
            };
        }

        private static ItemDefinition GetOrCreateFurnitureItem(
            string path,
            string displayName,
            string description,
            int baseValue,
            GameObject packagePrefab
        )
        {
            ItemDefinition item =
                AssetDatabase.LoadAssetAtPath<ItemDefinition>(
                    path
                );

            if (item == null)
            {
                item =
                    ScriptableObject.CreateInstance<ItemDefinition>();

                AssetDatabase.CreateAsset(
                    item,
                    path
                );
            }

            SerializedObject serialized =
                new SerializedObject(item);

            SerializedProperty id =
                serialized.FindProperty("itemId");

            if (string.IsNullOrWhiteSpace(id.stringValue))
            {
                id.stringValue =
                    Guid.NewGuid().ToString("N");
            }

            serialized.FindProperty("displayName").stringValue =
                displayName;

            serialized.FindProperty("description").stringValue =
                description;

            serialized.FindProperty("category").enumValueIndex =
                (int)ItemCategory.Equipment;

            serialized.FindProperty("maximumStack").intValue =
                5;

            serialized.FindProperty("baseValue").intValue =
                baseValue;

            serialized.FindProperty("displayPrefab").objectReferenceValue =
                packagePrefab;

            serialized.FindProperty("heldLocalPosition").vector3Value =
                new Vector3(0.42f, -0.42f, 0.72f);

            serialized.FindProperty("heldLocalEulerAngles").vector3Value =
                new Vector3(8f, -18f, 2f);

            serialized.FindProperty("heldLocalScale").vector3Value =
                Vector3.one * 0.32f;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            item.EnsurePersistentId();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static PlaceableDefinition GetOrCreatePlaceable(
            string path,
            string displayName,
            string description,
            int price,
            ItemDefinition inventoryItem,
            GameObject prefab,
            PlacementSurfaceType surfaceType,
            Vector3 boundsCenter,
            Vector3 boundsSize
        )
        {
            PlaceableDefinition definition =
                AssetDatabase.LoadAssetAtPath<PlaceableDefinition>(
                    path
                );

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<PlaceableDefinition>();

                AssetDatabase.CreateAsset(
                    definition,
                    path
                );
            }

            SerializedObject serialized =
                new SerializedObject(definition);

            SerializedProperty id =
                serialized.FindProperty("placeableId");

            if (string.IsNullOrWhiteSpace(id.stringValue))
            {
                id.stringValue =
                    Guid.NewGuid().ToString("N");
            }

            serialized.FindProperty("displayName").stringValue =
                displayName;

            serialized.FindProperty("description").stringValue =
                description;

            serialized.FindProperty("inventoryItem").objectReferenceValue =
                inventoryItem;

            serialized.FindProperty("purchasePrice").intValue =
                price;

            serialized.FindProperty("prefab").objectReferenceValue =
                prefab;

            serialized.FindProperty("surfaceType").enumValueIndex =
                (int)surfaceType;

            if (
                TryGetPrefabColliderBounds(
                    prefab,
                    out Bounds colliderBounds
                )
            )
            {
                boundsCenter =
                    colliderBounds.center;

                boundsSize =
                    colliderBounds.size;
            }

            serialized.FindProperty("boundsCenter").vector3Value =
                boundsCenter;

            serialized.FindProperty("boundsSize").vector3Value =
                boundsSize;

            serialized.FindProperty("gridSize").floatValue =
                surfaceType == PlacementSurfaceType.Wall
                    ? 0.1f
                    : 0.25f;

            serialized.FindProperty("rotationStep").floatValue =
                surfaceType == PlacementSurfaceType.Wall
                    ? 90f
                    : 15f;

            serialized.FindProperty("maximumSlope").floatValue =
                20f;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            definition.EnsurePersistentId();
            EditorUtility.SetDirty(definition);

            return definition;
        }

        private static PlaceableDatabase GetOrCreatePlaceableDatabase(
            IEnumerable<PlaceableDefinition> definitions
        )
        {
            PlaceableDatabase database =
                AssetDatabase.LoadAssetAtPath<PlaceableDatabase>(
                    PlaceableDatabasePath
                );

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<PlaceableDatabase>();

                AssetDatabase.CreateAsset(
                    database,
                    PlaceableDatabasePath
                );
            }

            database.SetDefinitions(definitions);
            EditorUtility.SetDirty(database);
            return database;
        }

        private static void UpdateItemDatabase(
            IEnumerable<ItemDefinition> furnitureItems
        )
        {
            ItemDatabase database =
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(
                    ItemDatabasePath
                );

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<ItemDatabase>();

                AssetDatabase.CreateAsset(
                    database,
                    ItemDatabasePath
                );
            }

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:ItemDefinition",
                    new[]
                    {
                        "Assets/_Project/Data/Items"
                    }
                );

            List<ItemDefinition> allItems =
                guids
                    .Select(
                        AssetDatabase.GUIDToAssetPath
                    )
                    .Select(
                        assetPath =>
                            AssetDatabase.LoadAssetAtPath<ItemDefinition>(
                                assetPath
                            )
                    )
                    .Where(item => item != null)
                    .Concat(furnitureItems)
                    .Distinct()
                    .ToList();

            database.SetItems(allItems);
            EditorUtility.SetDirty(database);
        }

        private static void EnsureStarterOwnership(
            PlayerInventory inventory,
            ItemDefinition workbenchItem
        )
        {
            if (
                inventory != null &&
                workbenchItem != null &&
                inventory.CountItem(workbenchItem) == 0
            )
            {
                inventory.AddItem(
                    workbenchItem,
                    1,
                    ItemQuality.Standard,
                    1f
                );
            }
        }

        private static GameObject CreateImportedFurniturePrefab(
            string displayName,
            string modelPath,
            string prefabPath,
            Material material,
            Vector3 targetSize,
            PlacementSurfaceType surfaceType,
            float visualScaleCorrection,
            FurnitureColliderProfile colliderProfile
        )
        {
            GameObject model =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    modelPath
                );

            if (model == null)
            {
                Debug.LogWarning(
                    $"[Milestone5] Imported model missing at " +
                    $"'{modelPath}'. Using fallback geometry."
                );

                return CreateFallbackFurniturePrefab(
                    displayName,
                    prefabPath,
                    material,
                    targetSize,
                    surfaceType
                );
            }

            GameObject root =
                new GameObject(displayName);

            GameObject visual =
                PrefabUtility.InstantiatePrefab(
                    model
                ) as GameObject;

            if (visual == null)
            {
                visual =
                    UnityEngine.Object.Instantiate(model);
            }

            visual.name = "Visual";
            visual.transform.SetParent(
                root.transform,
                false
            );

            ApplyMaterialToAllRenderers(
                visual,
                material
            );

            NormalizeVisual(
                visual,
                targetSize,
                surfaceType,
                visualScaleCorrection
            );

            CreateImportedFurnitureColliders(
                root,
                visual,
                colliderProfile
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    prefabPath
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateImportedFurnitureColliders(
            GameObject root,
            GameObject visual,
            FurnitureColliderProfile profile
        )
        {
            foreach (
                Collider existing
                in visual.GetComponentsInChildren<Collider>(true)
            )
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            Renderer[] renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            Bounds worldBounds =
                CalculateRendererBounds(renderers);

            Vector3 center =
                root.transform.InverseTransformPoint(
                    worldBounds.center
                );

            Vector3 size =
                worldBounds.size;

            switch (profile)
            {
                case FurnitureColliderProfile.Workbench:
                    CreateWorkbenchColliders(
                        root,
                        center,
                        size
                    );
                    break;

                case FurnitureColliderProfile.OfficeDesk:
                    CreateOfficeDeskColliders(
                        root,
                        center,
                        size
                    );
                    break;

                default:
                    AddBoxCollider(
                        root,
                        center,
                        size
                    );
                    break;
            }
        }

        private static void CreateWorkbenchColliders(
            GameObject root,
            Vector3 center,
            Vector3 size
        )
        {
            float minimumY =
                center.y - size.y * 0.5f;

            float maximumY =
                center.y + size.y * 0.5f;

            float minimumX =
                center.x - size.x * 0.5f;

            float maximumX =
                center.x + size.x * 0.5f;

            float minimumZ =
                center.z - size.z * 0.5f;

            float maximumZ =
                center.z + size.z * 0.5f;

            float topThickness =
                Mathf.Clamp(
                    size.y * 0.12f,
                    0.1f,
                    0.2f
                );

            AddBoxCollider(
                root,
                new Vector3(
                    center.x,
                    maximumY - topThickness * 0.5f,
                    center.z
                ),
                new Vector3(
                    size.x,
                    topThickness,
                    size.z
                )
            );

            float legHeight =
                Mathf.Max(
                    0.1f,
                    size.y - topThickness
                );

            float legWidth =
                Mathf.Clamp(
                    size.x * 0.055f,
                    0.08f,
                    0.16f
                );

            float legDepth =
                Mathf.Clamp(
                    size.z * 0.11f,
                    0.08f,
                    0.18f
                );

            float legCenterY =
                minimumY + legHeight * 0.5f;

            float leftX =
                minimumX + legWidth * 0.5f;

            float rightX =
                maximumX - legWidth * 0.5f;

            float frontZ =
                minimumZ + legDepth * 0.5f;

            float backZ =
                maximumZ - legDepth * 0.5f;

            Vector3 legSize =
                new Vector3(
                    legWidth,
                    legHeight,
                    legDepth
                );

            AddBoxCollider(
                root,
                new Vector3(leftX, legCenterY, frontZ),
                legSize
            );

            AddBoxCollider(
                root,
                new Vector3(rightX, legCenterY, frontZ),
                legSize
            );

            AddBoxCollider(
                root,
                new Vector3(leftX, legCenterY, backZ),
                legSize
            );

            AddBoxCollider(
                root,
                new Vector3(rightX, legCenterY, backZ),
                legSize
            );
        }

        private static void CreateOfficeDeskColliders(
            GameObject root,
            Vector3 center,
            Vector3 size
        )
        {
            float minimumY =
                center.y - size.y * 0.5f;

            float maximumY =
                center.y + size.y * 0.5f;

            float minimumX =
                center.x - size.x * 0.5f;

            float maximumX =
                center.x + size.x * 0.5f;

            float topThickness =
                Mathf.Clamp(
                    size.y * 0.11f,
                    0.1f,
                    0.18f
                );

            AddBoxCollider(
                root,
                new Vector3(
                    center.x,
                    maximumY - topThickness * 0.5f,
                    center.z
                ),
                new Vector3(
                    size.x,
                    topThickness,
                    size.z
                )
            );

            float pedestalHeight =
                Mathf.Max(
                    0.1f,
                    size.y - topThickness
                );

            float pedestalWidth =
                size.x * 0.255f;

            float pedestalDepth =
                size.z * 0.92f;

            float pedestalCenterY =
                minimumY + pedestalHeight * 0.5f;

            float leftX =
                minimumX + pedestalWidth * 0.5f;

            float rightX =
                maximumX - pedestalWidth * 0.5f;

            Vector3 pedestalSize =
                new Vector3(
                    pedestalWidth,
                    pedestalHeight,
                    pedestalDepth
                );

            AddBoxCollider(
                root,
                new Vector3(
                    leftX,
                    pedestalCenterY,
                    center.z
                ),
                pedestalSize
            );

            AddBoxCollider(
                root,
                new Vector3(
                    rightX,
                    pedestalCenterY,
                    center.z
                ),
                pedestalSize
            );
        }

        private static void AddBoxCollider(
            GameObject root,
            Vector3 center,
            Vector3 size
        )
        {
            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center = center;
            collider.size =
                new Vector3(
                    Mathf.Max(0.02f, size.x),
                    Mathf.Max(0.02f, size.y),
                    Mathf.Max(0.02f, size.z)
                );
        }

        private static bool TryGetPrefabColliderBounds(
            GameObject prefab,
            out Bounds bounds
        )
        {
            bounds = default;

            if (prefab == null)
            {
                return false;
            }

            BoxCollider[] colliders =
                prefab.GetComponents<BoxCollider>();

            if (colliders.Length == 0)
            {
                return false;
            }

            bounds =
                new Bounds(
                    colliders[0].center,
                    colliders[0].size
                );

            for (
                int index = 1;
                index < colliders.Length;
                index++
            )
            {
                BoxCollider collider =
                    colliders[index];

                Vector3 halfSize =
                    collider.size * 0.5f;

                bounds.Encapsulate(
                    collider.center - halfSize
                );

                bounds.Encapsulate(
                    collider.center + halfSize
                );
            }

            return true;
        }

        private static void NormalizeVisual(
            GameObject visual,
            Vector3 targetSize,
            PlacementSurfaceType surfaceType,
            float visualScaleCorrection
        )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds =
                CalculateRendererBounds(renderers);

            Vector3 safeSize =
                new Vector3(
                    Mathf.Max(0.001f, bounds.size.x),
                    Mathf.Max(0.001f, bounds.size.y),
                    Mathf.Max(0.001f, bounds.size.z)
                );

            float widthRatio =
                targetSize.x / safeSize.x;

            float heightRatio =
                targetSize.y / safeSize.y;

            float depthRatio =
                targetSize.z / safeSize.z;

            // Preserve the FBX importer's existing root scale.
            // Replacing localScale directly breaks assets whose importer
            // supplies a scale such as 0.01 or 100.
            float uniformMultiplier =
                Mathf.Min(
                    widthRatio,
                    Mathf.Min(
                        heightRatio,
                        depthRatio
                    )
                );

            if (
                float.IsNaN(uniformMultiplier) ||
                float.IsInfinity(uniformMultiplier) ||
                uniformMultiplier <= 0f
            )
            {
                uniformMultiplier = 1f;
            }

            visualScaleCorrection =
                Mathf.Max(
                    0.01f,
                    visualScaleCorrection
                );

            Vector3 importedScale =
                visual.transform.localScale;

            visual.transform.localScale =
                importedScale *
                uniformMultiplier *
                visualScaleCorrection;

            renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            bounds =
                CalculateRendererBounds(renderers);

            Vector3 offset =
                surfaceType == PlacementSurfaceType.Wall
                    ? new Vector3(
                        -bounds.center.x,
                        -bounds.center.y,
                        -bounds.min.z
                    )
                    : new Vector3(
                        -bounds.center.x,
                        -bounds.min.y,
                        -bounds.center.z
                    );

            visual.transform.position += offset;

            Debug.Log(
                $"[Milestone5] Normalized '{visual.name}' " +
                $"from {safeSize:F3} to {bounds.size:F3} " +
                $"using automatic multiplier " +
                $"{uniformMultiplier:0.######} and calibration " +
                $"{visualScaleCorrection:0.###}."
            );
        }

        private static Bounds CalculateRendererBounds(
            Renderer[] renderers
        )
        {
            Bounds bounds =
                renderers[0].bounds;

            for (
                int index = 1;
                index < renderers.Length;
                index++
            )
            {
                if (renderers[index] != null)
                {
                    bounds.Encapsulate(
                        renderers[index].bounds
                    );
                }
            }

            return bounds;
        }

        private static void ApplyMaterialToAllRenderers(
            GameObject root,
            Material material
        )
        {
            if (
                root == null ||
                material == null
            )
            {
                return;
            }

            foreach (
                Renderer renderer
                in root.GetComponentsInChildren<Renderer>(true)
            )
            {
                int materialCount =
                    Mathf.Max(
                        1,
                        renderer.sharedMaterials.Length
                    );

                Material[] materials =
                    new Material[materialCount];

                for (
                    int index = 0;
                    index < materials.Length;
                    index++
                )
                {
                    materials[index] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static GameObject CreateFallbackFurniturePrefab(
            string displayName,
            string prefabPath,
            Material material,
            Vector3 targetSize,
            PlacementSurfaceType surfaceType
        )
        {
            GameObject root =
                new GameObject(displayName);

            GameObject body =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            body.name = "Fallback Visual";
            body.transform.SetParent(
                root.transform,
                false
            );

            body.transform.localScale =
                targetSize;

            body.transform.localPosition =
                surfaceType == PlacementSurfaceType.Wall
                    ? new Vector3(
                        0f,
                        0f,
                        targetSize.z * 0.5f
                    )
                    : new Vector3(
                        0f,
                        targetSize.y * 0.5f,
                        0f
                    );

            body.GetComponent<Renderer>().sharedMaterial =
                material;

            UnityEngine.Object.DestroyImmediate(
                body.GetComponent<Collider>()
            );

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.size = targetSize;
            collider.center =
                body.transform.localPosition;

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    prefabPath
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateUtilityCabinetFallbackPrefab()
        {
            Material material =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_UtilityCabinet.mat",
                    new Color(0.1f, 0.24f, 0.28f),
                    0.5f
                );

            return CreateFallbackFurniturePrefab(
                "Utility Cabinet",
                PrefabFolder + "/PREFAB_UtilityCabinet.prefab",
                material,
                new Vector3(1.15f, 1.5f, 0.55f),
                PlacementSurfaceType.Floor
            );
        }

        private static GameObject CreateFurniturePackagePrefab()
        {
            string path =
                DeliveryPrefabFolder +
                "/PREFAB_FurniturePackage.prefab";

            GameObject existing =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (existing != null)
            {
                return existing;
            }

            Material cardboard =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_Cardboard.mat",
                    new Color(0.38f, 0.24f, 0.12f),
                    0f
                );

            Material strap =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_PackageStrap.mat",
                    new Color(0.08f, 0.09f, 0.09f),
                    0.1f
                );

            GameObject root =
                new GameObject("Furniture Package");

            CreateCubePart(
                root.transform,
                "Box",
                new Vector3(0f, 0.28f, 0f),
                new Vector3(0.62f, 0.56f, 0.48f),
                cardboard
            );

            CreateCubePart(
                root.transform,
                "Strap_X",
                new Vector3(0f, 0.285f, 0f),
                new Vector3(0.66f, 0.045f, 0.52f),
                strap
            );

            CreateCubePart(
                root.transform,
                "Strap_Z",
                new Vector3(0f, 0.285f, 0f),
                new Vector3(0.08f, 0.6f, 0.52f),
                strap
            );

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center =
                new Vector3(0f, 0.28f, 0f);

            collider.size =
                new Vector3(0.66f, 0.58f, 0.52f);

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateDeliveryCratePrefab()
        {
            string path =
                DeliveryPrefabFolder +
                "/PREFAB_FurnitureDeliveryCrate.prefab";

            GameObject existing =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (existing != null)
            {
                return existing;
            }

            Material wood =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_DeliveryWood.mat",
                    new Color(0.3f, 0.17f, 0.075f),
                    0f
                );

            Material metal =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_DeliveryMetal.mat",
                    new Color(0.1f, 0.11f, 0.11f),
                    0.6f
                );

            GameObject root =
                new GameObject("Furniture Delivery Crate");

            CreateCubePart(
                root.transform,
                "Crate",
                new Vector3(0f, 0.48f, 0f),
                new Vector3(1.05f, 0.96f, 0.86f),
                wood
            );

            CreateCubePart(
                root.transform,
                "Band_A",
                new Vector3(-0.32f, 0.49f, 0f),
                new Vector3(0.08f, 1f, 0.9f),
                metal
            );

            CreateCubePart(
                root.transform,
                "Band_B",
                new Vector3(0.32f, 0.49f, 0f),
                new Vector3(0.08f, 1f, 0.9f),
                metal
            );

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center =
                new Vector3(0f, 0.48f, 0f);

            collider.size =
                new Vector3(1.05f, 0.96f, 0.86f);

            root.AddComponent<FurnitureDeliveryCrate>();

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static Transform CreateDeliveryPoint(
            Transform parent
        )
        {
            GameObject point =
                new GameObject("Furniture Delivery Point");

            point.transform.SetParent(
                parent,
                false
            );

            point.transform.position =
                new Vector3(8f, 0f, -5f);

            GameObject marker =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            marker.name = "Delivery Marker";
            marker.transform.SetParent(
                point.transform,
                false
            );

            marker.transform.localPosition =
                new Vector3(0f, 0.025f, 0f);

            marker.transform.localScale =
                new Vector3(1.4f, 0.025f, 1.4f);

            marker.GetComponent<Renderer>().sharedMaterial =
                CreateSimpleMaterial(
                    MaterialFolder +
                    "/MAT_DeliveryMarker.mat",
                    new Color(0.9f, 0.42f, 0.06f),
                    0f
                );

            UnityEngine.Object.DestroyImmediate(
                marker.GetComponent<Collider>()
            );

            return point.transform;
        }

        private static void CreateSupplierTerminal(
            Transform parent,
            FurnitureDeliverySystem deliverySystem
        )
        {
            GameObject terminalRoot =
                new GameObject("Furniture Supplier Terminal");

            terminalRoot.transform.SetParent(
                parent,
                false
            );

            terminalRoot.transform.position =
                new Vector3(6f, 0f, -8f);

            Material bodyMaterial =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_SupplierTerminal.mat",
                    new Color(0.08f, 0.12f, 0.13f),
                    0.45f
                );

            Material screenMaterial =
                CreateSimpleMaterial(
                    MaterialFolder + "/MAT_SupplierScreen.mat",
                    new Color(0.1f, 0.65f, 0.55f),
                    0.05f
                );

            CreateCubePart(
                terminalRoot.transform,
                "Terminal Body",
                new Vector3(0f, 0.75f, 0f),
                new Vector3(0.9f, 1.5f, 0.6f),
                bodyMaterial
            );

            CreateCubePart(
                terminalRoot.transform,
                "Screen",
                new Vector3(0f, 1f, -0.315f),
                new Vector3(0.66f, 0.48f, 0.04f),
                screenMaterial
            );

            BoxCollider collider =
                terminalRoot.AddComponent<BoxCollider>();

            collider.center =
                new Vector3(0f, 0.75f, 0f);

            collider.size =
                new Vector3(0.9f, 1.5f, 0.6f);

            FurnitureShopTerminal terminal =
                terminalRoot.AddComponent<FurnitureShopTerminal>();

            terminal.Configure(deliverySystem);
        }

        private static Material CreateMaterial(
            string materialPath,
            string baseColorPath,
            string normalPath,
            string metallicPath,
            string aoPath,
            float smoothness
        )
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    materialPath
                );

            if (material != null)
            {
                return material;
            }

            if (!string.IsNullOrWhiteSpace(normalPath))
            {
                TextureImporter importer =
                    AssetImporter.GetAtPath(normalPath)
                        as TextureImporter;

                if (
                    importer != null &&
                    importer.textureType != TextureImporterType.NormalMap
                )
                {
                    importer.textureType =
                        TextureImporterType.NormalMap;

                    importer.SaveAndReimport();
                }
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");

            material =
                new Material(shader)
                {
                    name =
                        Path.GetFileNameWithoutExtension(
                            materialPath
                        )
                };

            Texture2D baseColor =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    baseColorPath
                );

            Texture2D normal =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    normalPath
                );

            Texture2D metallic =
                !string.IsNullOrWhiteSpace(metallicPath)
                    ? AssetDatabase.LoadAssetAtPath<Texture2D>(
                        metallicPath
                    )
                    : null;

            Texture2D ao =
                !string.IsNullOrWhiteSpace(aoPath)
                    ? AssetDatabase.LoadAssetAtPath<Texture2D>(
                        aoPath
                    )
                    : null;

            if (baseColor != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture(
                        "_BaseMap",
                        baseColor
                    );
                }
                else
                {
                    material.mainTexture = baseColor;
                }
            }

            if (
                normal != null &&
                material.HasProperty("_BumpMap")
            )
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture(
                    "_BumpMap",
                    normal
                );
            }

            if (
                metallic != null &&
                material.HasProperty("_MetallicGlossMap")
            )
            {
                material.EnableKeyword(
                    "_METALLICSPECGLOSSMAP"
                );

                material.SetTexture(
                    "_MetallicGlossMap",
                    metallic
                );
            }

            if (
                ao != null &&
                material.HasProperty("_OcclusionMap")
            )
            {
                material.SetTexture(
                    "_OcclusionMap",
                    ao
                );
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat(
                    "_Smoothness",
                    smoothness
                );
            }

            AssetDatabase.CreateAsset(
                material,
                materialPath
            );

            return material;
        }

        private static Material CreateSimpleMaterial(
            string path,
            Color color,
            float metallic
        )
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    path
                );

            if (material != null)
            {
                return material;
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");

            material =
                new Material(shader)
                {
                    name =
                        Path.GetFileNameWithoutExtension(path)
                };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    color
                );
            }
            else
            {
                material.color = color;
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat(
                    "_Metallic",
                    metallic
                );
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat(
                    "_Smoothness",
                    0.25f
                );
            }

            AssetDatabase.CreateAsset(
                material,
                path
            );

            return material;
        }

        private static void CreateCubePart(
            Transform parent,
            string partName,
            Vector3 localPosition,
            Vector3 localScale,
            Material material
        )
        {
            GameObject part =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            part.name = partName;
            part.transform.SetParent(
                parent,
                false
            );

            part.transform.localPosition =
                localPosition;

            part.transform.localScale =
                localScale;

            part.GetComponent<Renderer>().sharedMaterial =
                material;

            UnityEngine.Object.DestroyImmediate(
                part.GetComponent<Collider>()
            );
        }

        private static Scene OpenDevelopmentScene()
        {
            Scene active =
                SceneManager.GetActiveScene();

            if (
                active.IsValid() &&
                active.path == ScenePath
            )
            {
                return active;
            }

            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ScenePath
                );

            if (sceneAsset == null)
            {
                ShowError(
                    "Create the development playground first."
                );

                return default;
            }

            return EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single
            );
        }

        private static void EnsureProjectFolders()
        {
            foreach (
                string folder
                in new[]
                {
                    "Assets/_Project",
                    "Assets/_Project/Data",
                    "Assets/_Project/Data/Items",
                    ItemDataFolder,
                    "Assets/_Project/Data/Placeables",
                    PlaceableDataFolder,
                    DatabaseFolder,
                    "Assets/_Project/Prefabs",
                    "Assets/_Project/Prefabs/Placeables",
                    PrefabFolder,
                    DeliveryPrefabFolder,
                    "Assets/_Project/Art",
                    "Assets/_Project/Art/Materials",
                    MaterialFolder
                }
            )
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(
            string fullPath
        )
        {
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(fullPath)
                    ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(fullPath);

            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                folderName
            );
        }

        private static string ImportedPath(
            string relativePath
        )
        {
            return
                "Assets/_Project/Art/Imported/UserFurniture/" +
                relativePath;
        }

        private static void ShowError(
            string message
        )
        {
            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }

        private enum FurnitureColliderProfile
        {
            FullBox,
            Workbench,
            OfficeDesk
        }

        private sealed class FurnitureSetupRecord
        {
            public ItemDefinition item;
            public PlaceableDefinition placeable;
        }
    }
}
