using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Electrical;
using ShadowSupply.Inventory;
using ShadowSupply.Production;
using ShadowSupply.Progression;
using ShadowSupply.Relationships;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class
        Milestone9AOpeningCounterfeitingBranchSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/" +
            "Dev_Playground.unity";

        private const string GarageRootName =
            "StarterGarage_Property";

        private const string MilestoneRootName =
            "Milestone9A_OpeningCounterfeitingBranch";

        private const string ProcessRootName =
            "Counterfeit Production Surface";

        private const string ItemDatabasePath =
            "Assets/_Project/Data/Databases/" +
            "DB_Items.asset";

        private const string ItemFolder =
            "Assets/_Project/Data/Items/" +
            "Counterfeiting";

        private const string RecipeFolder =
            "Assets/_Project/Data/Production/" +
            "Counterfeiting";

        private const string PrefabFolder =
            "Assets/_Project/Prefabs/Production/" +
            "Counterfeiting";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/" +
            "Counterfeiting";

        private const string IconFolder =
            "Assets/_Project/Resources/ShadowSupply/" +
            "UI/Inventory/Icons";

        private const string SupplierFolder =
            "Assets/_Project/Data/Relationships/" +
            "Suppliers";

        private const string StockFolder =
            "Assets/_Project/Data/Relationships/" +
            "SupplierStock";

        private const string BuyerFolder =
            "Assets/_Project/Data/Relationships/" +
            "Buyers";

        private const string OrderFolder =
            "Assets/_Project/Data/Relationships/" +
            "Orders";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 9A Opening Counterfeiting Branch"
        )]
        public static void ApplyMilestoneNineA()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );

            GameObject garage =
                GameObject.Find(GarageRootName);

            PoweredWorkbenchProduction oldProduction =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >(
                        FindObjectsInactive.Include
                    );

            PlayerInventory inventory =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PlayerInventory
                    >(
                        FindObjectsInactive.Include
                    );

            IndustryReputationSystem
                existingReputation =
                    UnityEngine.Object
                        .FindFirstObjectByType<
                            IndustryReputationSystem
                        >(
                            FindObjectsInactive.Include
                        );

            ItemDatabase itemDatabase =
                AssetDatabase.LoadAssetAtPath<
                    ItemDatabase
                >(ItemDatabasePath);

            if (
                !scene.IsValid() ||
                garage == null ||
                oldProduction == null ||
                oldProduction.PowerConnection == null ||
                inventory == null ||
                itemDatabase == null
            )
            {
                ShowError(
                    "Milestone 9A requires the confirmed " +
                    "starter garage, powered interactive workbench, " +
                    "player inventory, and item database."
                );
                return;
            }

            EnsureFolder(ItemFolder);
            EnsureFolder(RecipeFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(IconFolder);
            EnsureFolder(StockFolder);

            Material paperMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_BlankNoteStock.mat",
                    new Color(
                        0.78f,
                        0.74f,
                        0.56f
                    ),
                    0.18f,
                    0f
                );

            Material pigmentMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_PigmentCapsule.mat",
                    new Color(
                        0.13f,
                        0.12f,
                        0.11f
                    ),
                    0.32f,
                    0.08f
                );

            Material filmMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_SecurityFilm.mat",
                    new Color(
                        0.18f,
                        0.58f,
                        0.64f
                    ),
                    0.65f,
                    0.02f
                );

            Material noteMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_ReplicaNote.mat",
                    new Color(
                        0.42f,
                        0.52f,
                        0.34f
                    ),
                    0.24f,
                    0f
                );

            Material bandMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_ReplicaBand.mat",
                    new Color(
                        0.74f,
                        0.48f,
                        0.08f
                    ),
                    0.26f,
                    0.02f
                );

            GameObject blankStockPrefab =
                CreateBlankStockPrefab(
                    paperMaterial
                );

            GameObject pigmentPrefab =
                CreatePigmentPrefab(
                    pigmentMaterial,
                    bandMaterial
                );

            GameObject filmPrefab =
                CreateFilmPrefab(
                    filmMaterial
                );

            GameObject outputPrefab =
                CreateReplicaBundlePrefab(
                    noteMaterial,
                    bandMaterial
                );

            ItemDefinition blankStock =
                CreateOrUpdateItem(
                    ItemFolder +
                    "/ITEM_BlankNoteStock.asset",
                    "counterfeit-blank-note-stock",
                    "Blank Note Stock",
                    "Fictionalized substrate sheets prepared for " +
                    "the starter imprint press.",
                    ItemCategory.Material,
                    20,
                    18,
                    blankStockPrefab,
                    PrimitiveType.Cube,
                    new Color(
                        0.78f,
                        0.74f,
                        0.56f
                    ),
                    new Vector3(
                        0.42f,
                        -0.34f,
                        0.72f
                    ),
                    new Vector3(
                        8f,
                        -18f,
                        2f
                    ),
                    Vector3.one * 0.28f
                );

            ItemDefinition pigment =
                CreateOrUpdateItem(
                    ItemFolder +
                    "/ITEM_PigmentCapsule.asset",
                    "counterfeit-pigment-capsule",
                    "Pigment Capsule",
                    "A sealed fictional pigment capsule used by " +
                    "the imprint press.",
                    ItemCategory.Material,
                    10,
                    28,
                    pigmentPrefab,
                    PrimitiveType.Cylinder,
                    new Color(
                        0.13f,
                        0.12f,
                        0.11f
                    ),
                    new Vector3(
                        0.43f,
                        -0.34f,
                        0.72f
                    ),
                    new Vector3(
                        78f,
                        0f,
                        0f
                    ),
                    Vector3.one * 0.24f
                );

            ItemDefinition film =
                CreateOrUpdateItem(
                    ItemFolder +
                    "/ITEM_SecurityFilm.asset",
                    "counterfeit-security-film",
                    "Security Film",
                    "A fictional optical film aligned over each " +
                    "imprint run.",
                    ItemCategory.Material,
                    10,
                    36,
                    filmPrefab,
                    PrimitiveType.Cube,
                    new Color(
                        0.18f,
                        0.58f,
                        0.64f
                    ),
                    new Vector3(
                        0.42f,
                        -0.34f,
                        0.72f
                    ),
                    new Vector3(
                        8f,
                        -18f,
                        2f
                    ),
                    Vector3.one * 0.27f
                );

            ItemDefinition output =
                CreateOrUpdateItem(
                    ItemFolder +
                    "/ITEM_ReplicaNoteBundle.asset",
                    "counterfeit-replica-note-bundle",
                    "Replica Note Bundle",
                    "A sealed bundle of fictional replica notes. " +
                    "Quality depends on materials, experience, " +
                    "and production mistakes.",
                    ItemCategory.Product,
                    10,
                    260,
                    outputPrefab,
                    PrimitiveType.Cube,
                    new Color(
                        0.42f,
                        0.52f,
                        0.34f
                    ),
                    new Vector3(
                        0.42f,
                        -0.32f,
                        0.7f
                    ),
                    new Vector3(
                        6f,
                        -20f,
                        0f
                    ),
                    Vector3.one * 0.3f
                );

            ItemDefinition packaging =
                FindItem(
                    itemDatabase,
                    "production-packaging-material",
                    "Packaging Material"
                );

            ItemDefinition toolkit =
                FindItem(
                    itemDatabase,
                    string.Empty,
                    "Basic Toolkit"
                );

            if (
                packaging == null ||
                toolkit == null
            )
            {
                ShowError(
                    "Packaging Material or Basic Toolkit " +
                    "was not found. Confirm Milestone 8B first."
                );
                return;
            }

            List<ItemDefinition> allItems =
                itemDatabase.Items
                    .Where(item => item != null)
                    .ToList();

            AddUnique(allItems, blankStock);
            AddUnique(allItems, pigment);
            AddUnique(allItems, film);
            AddUnique(allItems, output);

            itemDatabase.SetItems(allItems);
            EditorUtility.SetDirty(itemDatabase);

            CounterfeitRecipeDefinition recipe =
                AssetDatabase.LoadAssetAtPath<
                    CounterfeitRecipeDefinition
                >(
                    RecipeFolder +
                    "/COUNTERFEIT_RECIPE_" +
                    "ReplicaNoteBundle.asset"
                );

            if (recipe == null)
            {
                recipe =
                    ScriptableObject.CreateInstance<
                        CounterfeitRecipeDefinition
                    >();

                AssetDatabase.CreateAsset(
                    recipe,
                    RecipeFolder +
                    "/COUNTERFEIT_RECIPE_" +
                    "ReplicaNoteBundle.asset"
                );
            }

            recipe.Configure(
                "counterfeit-recipe-replica-note-bundle",
                "Replica Note Bundle",
                "Load fictional note stock, pigment, and " +
                "security film into the powered imprint press. " +
                "Print, trim, package, and seal the finished bundle.",
                blankStock,
                pigment,
                film,
                packaging,
                toolkit,
                output,
                1,
                true,
                1.75f,
                3
            );

            EditorUtility.SetDirty(recipe);

            GameObject milestoneRoot =
                GameObject.Find(
                    MilestoneRootName
                );

            if (milestoneRoot == null)
            {
                milestoneRoot =
                    new GameObject(
                        MilestoneRootName
                    );
            }

            IndustryReputationSystem reputation =
                existingReputation != null
                    ? existingReputation
                    : milestoneRoot
                        .GetComponent<
                            IndustryReputationSystem
                        >();

            if (reputation == null)
            {
                reputation =
                    milestoneRoot.AddComponent<
                        IndustryReputationSystem
                    >();
            }

            CounterfeitPressHUD hud =
                milestoneRoot.GetComponent<
                    CounterfeitPressHUD
                >();

            if (hud == null)
            {
                hud =
                    milestoneRoot.AddComponent<
                        CounterfeitPressHUD
                    >();
            }

            BuildCounterfeitPress(
                oldProduction,
                recipe,
                reputation,
                outputPrefab
            );

            UpdateSupplierCatalog(
                blankStock,
                pigment,
                film,
                packaging,
                toolkit
            );

            UpdateMaraOrders(output);

            EnsureInventoryMinimum(
                inventory,
                blankStock,
                1
            );

            EnsureInventoryMinimum(
                inventory,
                pigment,
                1
            );

            EnsureInventoryMinimum(
                inventory,
                film,
                1
            );

            EnsureInventoryMinimum(
                inventory,
                packaging,
                1
            );

            EnsureInventoryMinimum(
                inventory,
                toolkit,
                1
            );

            EditorUtility.SetDirty(inventory);
            EditorUtility.SetDirty(reputation);
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(milestoneRoot);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                oldProduction.gameObject;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 9A applied.\n\n" +
                "The development hardware recipe has been " +
                "disabled and the starter workbench is now a " +
                "powered fictional imprint press.\n\n" +
                "One starter batch of materials was added. " +
                "Mara now buys Replica Note Bundles, and her " +
                "first successful order unlocks Elias's " +
                "counterfeiting-material catalog.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Testing/" +
            "Grant Counterfeiting Starter Materials (Play Mode)"
        )]
        public static void GrantStarterMaterials()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Enter Play Mode before granting " +
                    "temporary starter materials.",
                    "OK"
                );
                return;
            }

            PlayerInventory inventory =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PlayerInventory
                    >();

            ItemDatabase database =
                AssetDatabase.LoadAssetAtPath<
                    ItemDatabase
                >(ItemDatabasePath);

            if (
                inventory == null ||
                database == null
            )
            {
                ShowError(
                    "Inventory or item database was not found."
                );
                return;
            }

            ItemDefinition packaging =
                FindItem(
                    database,
                    "production-packaging-material",
                    "Packaging Material"
                );

            ItemDefinition toolkit =
                FindItem(
                    database,
                    string.Empty,
                    "Basic Toolkit"
                );

            EnsureInventoryMinimum(
                inventory,
                FindItem(
                    database,
                    "counterfeit-blank-note-stock",
                    "Blank Note Stock"
                ),
                3
            );

            EnsureInventoryMinimum(
                inventory,
                FindItem(
                    database,
                    "counterfeit-pigment-capsule",
                    "Pigment Capsule"
                ),
                3
            );

            EnsureInventoryMinimum(
                inventory,
                FindItem(
                    database,
                    "counterfeit-security-film",
                    "Security Film"
                ),
                3
            );

            EnsureInventoryMinimum(
                inventory,
                packaging,
                3
            );

            EnsureInventoryMinimum(
                inventory,
                toolkit,
                1
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Temporary materials granted for " +
                "three counterfeit production tests.",
                "OK"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 9A Opening Counterfeiting Branch"
        )]
        public static void ValidateMilestoneNineA()
        {
            CounterfeitPressStation station =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        CounterfeitPressStation
                    >(
                        FindObjectsInactive.Include
                    );

            CounterfeitPressInteractionController
                controller =
                    UnityEngine.Object
                        .FindFirstObjectByType<
                            CounterfeitPressInteractionController
                        >(
                            FindObjectsInactive.Include
                        );

            CounterfeitPressHUD hud =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        CounterfeitPressHUD
                    >(
                        FindObjectsInactive.Include
                    );

            IndustryReputationSystem reputation =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        IndustryReputationSystem
                    >(
                        FindObjectsInactive.Include
                    );

            PoweredWorkbenchProduction oldProduction =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >(
                        FindObjectsInactive.Include
                    );

            ItemDatabase database =
                AssetDatabase.LoadAssetAtPath<
                    ItemDatabase
                >(ItemDatabasePath);

            bool itemsReady =
                database != null &&
                database.GetItem(
                    "counterfeit-blank-note-stock"
                ) != null &&
                database.GetItem(
                    "counterfeit-pigment-capsule"
                ) != null &&
                database.GetItem(
                    "counterfeit-security-film"
                ) != null &&
                database.GetItem(
                    "counterfeit-replica-note-bundle"
                ) != null;

            SupplierDefinition supplier =
                AssetDatabase.LoadAssetAtPath<
                    SupplierDefinition
                >(
                    SupplierFolder +
                    "/SUPPLIER_EliasMercer.asset"
                );

            BuyerDefinition mara =
                AssetDatabase.LoadAssetAtPath<
                    BuyerDefinition
                >(
                    BuyerFolder +
                    "/BUYER_MaraVoss.asset"
                );

            bool valid =
                station != null &&
                station.Recipe != null &&
                station.Recipe.IsValid() &&
                controller != null &&
                controller.ProcessCamera != null &&
                hud != null &&
                reputation != null &&
                itemsReady &&
                supplier != null &&
                supplier.Stock.Count == 5 &&
                mara != null &&
                mara.ReferralSuccessfulOrders == 1 &&
                oldProduction != null &&
                !oldProduction.enabled &&
                SaveManager.CurrentSaveVersion == 10;

            string report =
                "Counterfeit press: " +
                (
                    station != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nRecipe: " +
                (
                    station != null &&
                    station.Recipe != null &&
                    station.Recipe.IsValid()
                        ? "OK"
                        : "MISSING"
                ) +
                "\nPhysical process controller: " +
                (
                    controller != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nProcess camera: " +
                (
                    controller != null &&
                    controller.ProcessCamera != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nCounterfeiting items: " +
                (
                    itemsReady
                        ? "4 / 4"
                        : "INCOMPLETE"
                ) +
                "\nElias catalog: " +
                (
                    supplier != null
                        ? supplier.Stock.Count +
                          " / 5"
                        : "0 / 5"
                ) +
                "\nMara referral threshold: " +
                (
                    mara != null
                        ? mara.ReferralSuccessfulOrders +
                          " / 1"
                        : "MISSING"
                ) +
                "\nDevelopment recipe disabled: " +
                (
                    oldProduction != null &&
                    !oldProduction.enabled
                        ? "YES"
                        : "NO"
                ) +
                "\nSave schema: " +
                SaveManager.CurrentSaveVersion +
                " / 10";

            if (valid)
            {
                Debug.Log(
                    "[Milestone9A] OPENING " +
                    "COUNTERFEITING BRANCH READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 9A Validation",
                    "OPENING COUNTERFEITING BRANCH READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone9A] VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 9A Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static void BuildCounterfeitPress(
            PoweredWorkbenchProduction oldProduction,
            CounterfeitRecipeDefinition recipe,
            IndustryReputationSystem reputation,
            GameObject outputPrefab
        )
        {
            Transform workbench =
                oldProduction.transform;

            oldProduction.enabled = false;

            InteractiveAssemblyController
                oldController =
                    oldProduction
                        .AssemblyController;

            if (oldController != null)
            {
                oldController.enabled = false;
            }

            ProductionWorkbenchHUD oldHud =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ProductionWorkbenchHUD
                    >(
                        FindObjectsInactive.Include
                    );

            if (oldHud != null)
            {
                oldHud.enabled = false;
            }

            Transform oldSurface =
                FindChildRecursive(
                    workbench,
                    "Interactive Production Surface"
                );

            if (oldSurface != null)
            {
                UnityEngine.Object
                    .DestroyImmediate(
                        oldSurface.gameObject
                    );
            }

            Transform existing =
                FindChildRecursive(
                    workbench,
                    ProcessRootName
                );

            if (existing != null)
            {
                UnityEngine.Object
                    .DestroyImmediate(
                        existing.gameObject
                    );
            }

            CounterfeitPressStation station =
                workbench.GetComponent<
                    CounterfeitPressStation
                >();

            if (station == null)
            {
                station =
                    workbench.gameObject
                        .AddComponent<
                            CounterfeitPressStation
                        >();
            }

            LocalBounds bounds =
                CalculateLocalBounds(
                    workbench
                );

            bool rotate =
                bounds.size.z >
                bounds.size.x;

            float width =
                Mathf.Clamp(
                    rotate
                        ? bounds.size.z
                        : bounds.size.x,
                    1.75f,
                    2.35f
                );

            float depth =
                Mathf.Clamp(
                    rotate
                        ? bounds.size.x
                        : bounds.size.z,
                    0.72f,
                    1.05f
                );

            GameObject root =
                new GameObject(
                    ProcessRootName
                );

            root.transform.SetParent(
                workbench,
                false
            );

            root.transform.localPosition =
                new Vector3(
                    bounds.center.x,
                    bounds.max.y + 0.028f,
                    bounds.center.z
                );

            root.transform.localRotation =
                rotate
                    ? Quaternion.Euler(
                        0f,
                        90f,
                        0f
                    )
                    : Quaternion.identity;

            GameObject surface =
                new GameObject(
                    "Counterfeit Process Surface"
                );

            surface.transform.SetParent(
                root.transform,
                false
            );

            GameObject partsRoot =
                new GameObject(
                    "Spawned Counterfeit Parts"
                );

            partsRoot.transform.SetParent(
                surface.transform,
                false
            );

            Material trayMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_PressTray.mat",
                    new Color(
                        0.08f,
                        0.09f,
                        0.095f
                    ),
                    0.28f,
                    0.35f
                );

            Material zoneMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_ProcessZone.mat",
                    new Color(
                        0.16f,
                        0.17f,
                        0.18f
                    ),
                    0.24f,
                    0.08f
                );

            Material matMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_CuttingMat.mat",
                    new Color(
                        0.08f,
                        0.22f,
                        0.16f
                    ),
                    0.2f,
                    0.02f
                );

            Material buttonMaterial =
                GetOrCreateMaterial(
                    MaterialFolder +
                    "/MAT_PressControl.mat",
                    new Color(
                        0.66f,
                        0.18f,
                        0.015f
                    ),
                    0.32f,
                    0.05f
                );

            CreateLocalCube(
                surface.transform,
                "Imprint Press Body",
                new Vector3(
                    0.38f,
                    0.09f,
                    0.17f
                ),
                new Vector3(
                    0.62f,
                    0.18f,
                    0.4f
                ),
                trayMaterial,
                false
            );

            ZoneReferences paper =
                CreateZone(
                    surface.transform,
                    "Paper Tray",
                    new Vector3(
                        -0.68f,
                        0.025f,
                        0.22f
                    ),
                    new Vector3(
                        0.34f,
                        0.045f,
                        0.24f
                    ),
                    zoneMaterial
                );

            ZoneReferences pigment =
                CreateZone(
                    surface.transform,
                    "Pigment Bay",
                    new Vector3(
                        -0.28f,
                        0.035f,
                        0.22f
                    ),
                    new Vector3(
                        0.22f,
                        0.07f,
                        0.22f
                    ),
                    zoneMaterial
                );

            ZoneReferences film =
                CreateZone(
                    surface.transform,
                    "Security Film Bed",
                    new Vector3(
                        0.08f,
                        0.025f,
                        0.2f
                    ),
                    new Vector3(
                        0.3f,
                        0.045f,
                        0.24f
                    ),
                    zoneMaterial
                );

            ZoneReferences cutting =
                CreateZone(
                    surface.transform,
                    "Cutting Mat",
                    new Vector3(
                        -0.25f,
                        0.018f,
                        -0.22f
                    ),
                    new Vector3(
                        0.54f,
                        0.035f,
                        0.3f
                    ),
                    matMaterial
                );

            ZoneReferences packaging =
                CreateZone(
                    surface.transform,
                    "Packaging Zone",
                    new Vector3(
                        0.45f,
                        0.018f,
                        -0.22f
                    ),
                    new Vector3(
                        0.48f,
                        0.035f,
                        0.3f
                    ),
                    zoneMaterial
                );

            ActionReferences printControl =
                CreateActionControl(
                    surface.transform,
                    "PRINT Control",
                    CounterfeitPressActionKind
                        .PrintControl,
                    new Vector3(
                        0.75f,
                        0.11f,
                        0.25f
                    ),
                    new Vector3(
                        0.15f,
                        0.06f,
                        0.15f
                    ),
                    buttonMaterial
                );

            ActionReferences sealControl =
                CreateActionControl(
                    surface.transform,
                    "SEAL Control",
                    CounterfeitPressActionKind
                        .SealControl,
                    new Vector3(
                        0.76f,
                        0.045f,
                        -0.24f
                    ),
                    new Vector3(
                        0.18f,
                        0.06f,
                        0.14f
                    ),
                    buttonMaterial
                );

            Camera camera =
                CreateProcessCamera(
                    root.transform,
                    width,
                    depth
                );

            CounterfeitPressInteractionController
                controller =
                    root.AddComponent<
                        CounterfeitPressInteractionController
                    >();

            controller.Configure(
                station,
                camera,
                surface.transform,
                new Vector2(
                    width,
                    depth
                ),
                partsRoot.transform,
                paper.collider,
                paper.snap,
                paper.renderer,
                pigment.collider,
                pigment.snap,
                pigment.renderer,
                film.collider,
                film.snap,
                film.renderer,
                cutting.collider,
                cutting.snap,
                cutting.renderer,
                packaging.collider,
                packaging.snap,
                packaging.renderer,
                printControl.collider,
                printControl.renderer,
                printControl.transform,
                printControl
                    .releasedLocalPosition,
                printControl
                    .pressedLocalPosition,
                sealControl.collider,
                sealControl.renderer
            );

            GameObject outputVisual =
                CreateOutputVisual(
                    workbench,
                    bounds,
                    outputPrefab
                );

            station.Configure(
                "starter-counterfeit-press",
                oldProduction.Inventory,
                oldProduction.PowerConnection,
                recipe,
                controller,
                reputation,
                outputVisual
            );

            station.ResetToDefaultState();

            surface.SetActive(false);
            camera.enabled = false;

            EditorUtility.SetDirty(station);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(oldProduction);
        }

        private static void UpdateSupplierCatalog(
            ItemDefinition blankStock,
            ItemDefinition pigment,
            ItemDefinition film,
            ItemDefinition packaging,
            ItemDefinition toolkit
        )
        {
            SupplierDefinition supplier =
                AssetDatabase.LoadAssetAtPath<
                    SupplierDefinition
                >(
                    SupplierFolder +
                    "/SUPPLIER_EliasMercer.asset"
                );

            if (supplier == null)
            {
                ShowError(
                    "Elias Mercer was not found. " +
                    "Confirm Milestone 8D first."
                );
                return;
            }

            SupplierStockDefinition stockItem =
                CreateSupplierStock(
                    StockFolder +
                    "/STOCK_Elias_BlankNoteStock.asset",
                    "elias-stock-blank-note-stock",
                    "Blank Note Stock",
                    "Fictionalized press stock sold in " +
                    "small starter quantities.",
                    blankStock,
                    8,
                    22,
                    0,
                    -100,
                    -100,
                    -100
                );

            SupplierStockDefinition pigmentItem =
                CreateSupplierStock(
                    StockFolder +
                    "/STOCK_Elias_PigmentCapsule.asset",
                    "elias-stock-pigment-capsule",
                    "Pigment Capsule",
                    "Sealed press pigment. Elias limits " +
                    "quantities until payment history improves.",
                    pigment,
                    6,
                    36,
                    0,
                    -100,
                    -100,
                    -100
                );

            SupplierStockDefinition filmItem =
                CreateSupplierStock(
                    StockFolder +
                    "/STOCK_Elias_SecurityFilm.asset",
                    "elias-stock-security-film",
                    "Security Film",
                    "Fictional optical film for the " +
                    "starter imprint press.",
                    film,
                    6,
                    48,
                    0,
                    -100,
                    -100,
                    -100
                );

            SupplierStockDefinition packagingItem =
                CreateSupplierStock(
                    StockFolder +
                    "/STOCK_Elias_Packaging.asset",
                    "elias-stock-packaging",
                    "Packaging Material",
                    "Basic wrap, bands, and seals.",
                    packaging,
                    10,
                    12,
                    0,
                    -100,
                    -100,
                    -100
                );

            SupplierStockDefinition toolkitItem =
                CreateSupplierStock(
                    StockFolder +
                    "/STOCK_Elias_Toolkit.asset",
                    "elias-stock-toolkit",
                    "Basic Toolkit",
                    "A reusable cutting and calibration kit " +
                    "reserved for repeat customers.",
                    toolkit,
                    1,
                    180,
                    3,
                    -100,
                    8,
                    4
                );

            supplier.Configure(
                "supplier-elias-mercer",
                "Elias Mercer",
                "blackwater-south",
                "Blackwater South",
                "A cautious print and materials broker who " +
                "works through referrals and watches payment history.",
                "buyer-mara-voss",
                "Mara Voss referral",
                0,
                0,
                0,
                new[]
                {
                    stockItem,
                    pigmentItem,
                    filmItem,
                    packagingItem,
                    toolkitItem
                },
                120f,
                8f
            );

            SupplierNPC[] supplierNpcs =
                UnityEngine.Object.FindObjectsByType<
                    SupplierNPC
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (
                SupplierNPC supplierNpc
                in supplierNpcs
            )
            {
                if (
                    supplierNpc == null ||
                    !string.Equals(
                        supplierNpc.SupplierId,
                        "supplier-elias-mercer",
                        StringComparison.Ordinal
                    )
                )
                {
                    continue;
                }

                supplierNpc.Configure(
                    supplier,
                    supplierNpc.Wallet,
                    supplierNpc.DeliverySystem
                );

                EditorUtility.SetDirty(supplierNpc);
            }

            EditorUtility.SetDirty(supplier);
        }

        private static void UpdateMaraOrders(
            ItemDefinition output
        )
        {
            BuyerDefinition mara =
                AssetDatabase.LoadAssetAtPath<
                    BuyerDefinition
                >(
                    BuyerFolder +
                    "/BUYER_MaraVoss.asset"
                );

            BuyerOrderDefinition trial =
                AssetDatabase.LoadAssetAtPath<
                    BuyerOrderDefinition
                >(
                    OrderFolder +
                    "/ORDER_Mara_TrialPackage.asset"
                );

            BuyerOrderDefinition repeat =
                AssetDatabase.LoadAssetAtPath<
                    BuyerOrderDefinition
                >(
                    OrderFolder +
                    "/ORDER_Mara_RepeatRun.asset"
                );

            BuyerOrderDefinition network =
                AssetDatabase.LoadAssetAtPath<
                    BuyerOrderDefinition
                >(
                    OrderFolder +
                    "/ORDER_Mara_NetworkTest.asset"
                );

            if (
                mara == null ||
                trial == null ||
                repeat == null ||
                network == null
            )
            {
                ShowError(
                    "Mara or her order definitions were not found. " +
                    "Confirm Milestone 8C first."
                );
                return;
            }

            trial.Configure(
                "order-mara-trial-package",
                "First Replica Run",
                "Mara wants one sealed replica-note bundle " +
                "to judge whether the player can print clean work.",
                output,
                1,
                ItemQuality.Standard,
                600f,
                320,
                45,
                0,
                -100,
                -100,
                -100,
                4,
                8,
                6,
                -2,
                -7,
                -3
            );

            repeat.Configure(
                "order-mara-repeat-run",
                "Repeat Replica Run",
                "Mara wants two additional bundles after " +
                "the first handoff proves reliable.",
                output,
                2,
                ItemQuality.Standard,
                720f,
                720,
                55,
                1,
                -100,
                8,
                6,
                3,
                8,
                7,
                -2,
                -7,
                -3
            );

            network.Configure(
                "order-mara-network-test",
                "District Sample Run",
                "Mara wants three bundles before she risks " +
                "putting the player's name deeper into her network.",
                output,
                3,
                ItemQuality.Good,
                900f,
                1250,
                70,
                2,
                -100,
                16,
                13,
                3,
                8,
                7,
                -2,
                -7,
                -3
            );

            mara.Configure(
                "buyer-mara-voss",
                "Mara Voss",
                "blackwater-south",
                "Blackwater South",
                "A cautious neighborhood buyer who values " +
                "quiet handoffs, reliable timing, and clean replica work.",
                0,
                0,
                0,
                new[]
                {
                    trial,
                    repeat,
                    network
                },
                25f,
                "contact-print-supply-broker",
                "Print-Supply Broker",
                1,
                8,
                6
            );

            EditorUtility.SetDirty(trial);
            EditorUtility.SetDirty(repeat);
            EditorUtility.SetDirty(network);
            EditorUtility.SetDirty(mara);
        }

        private static SupplierStockDefinition
            CreateSupplierStock(
                string path,
                string stockId,
                string displayName,
                string description,
                ItemDefinition item,
                int maximumStock,
                int basePrice,
                int requiredPurchases,
                int requiredRapport,
                int requiredTrust,
                int requiredRespect
            )
        {
            SupplierStockDefinition stock =
                AssetDatabase.LoadAssetAtPath<
                    SupplierStockDefinition
                >(path);

            if (stock == null)
            {
                stock =
                    ScriptableObject.CreateInstance<
                        SupplierStockDefinition
                    >();

                AssetDatabase.CreateAsset(
                    stock,
                    path
                );
            }

            stock.Configure(
                stockId,
                displayName,
                description,
                item,
                maximumStock,
                basePrice,
                requiredPurchases,
                requiredRapport,
                requiredTrust,
                requiredRespect
            );

            EditorUtility.SetDirty(stock);
            return stock;
        }

        private static ItemDefinition
            CreateOrUpdateItem(
                string path,
                string itemId,
                string displayName,
                string description,
                ItemCategory category,
                int maximumStack,
                int baseValue,
                GameObject displayPrefab,
                PrimitiveType fallbackPrimitive,
                Color fallbackColor,
                Vector3 heldPosition,
                Vector3 heldEuler,
                Vector3 heldScale
            )
        {
            ItemDefinition item =
                AssetDatabase.LoadAssetAtPath<
                    ItemDefinition
                >(path);

            if (item == null)
            {
                item =
                    ScriptableObject.CreateInstance<
                        ItemDefinition
                    >();

                AssetDatabase.CreateAsset(
                    item,
                    path
                );
            }

            SerializedObject serialized =
                new SerializedObject(item);

            SetString(
                serialized,
                "itemId",
                itemId
            );

            SetString(
                serialized,
                "displayName",
                displayName
            );

            SetString(
                serialized,
                "description",
                description
            );

            SetEnum(
                serialized,
                "category",
                (int)category
            );

            SetInt(
                serialized,
                "maximumStack",
                maximumStack
            );

            SetInt(
                serialized,
                "baseValue",
                baseValue
            );

            SetObject(
                serialized,
                "displayPrefab",
                displayPrefab
            );

            SetEnum(
                serialized,
                "fallbackPrimitive",
                (int)fallbackPrimitive
            );

            SetColor(
                serialized,
                "fallbackColor",
                fallbackColor
            );

            SetVector3(
                serialized,
                "heldLocalPosition",
                heldPosition
            );

            SetVector3(
                serialized,
                "heldLocalEulerAngles",
                heldEuler
            );

            SetVector3(
                serialized,
                "heldLocalScale",
                heldScale
            );

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);

            CreateIcon(
                displayName,
                fallbackColor
            );

            return item;
        }

        private static GameObject
            CreateBlankStockPrefab(
                Material material
            )
        {
            string path =
                PrefabFolder +
                "/PREFAB_BlankNoteStock.prefab";

            GameObject root =
                new GameObject(
                    "Blank Note Stock"
                );

            for (
                int index = 0;
                index < 5;
                index++
            )
            {
                CreateLocalCube(
                    root.transform,
                    $"Sheet {index + 1}",
                    new Vector3(
                        0f,
                        index * 0.008f,
                        0f
                    ),
                    new Vector3(
                        0.28f,
                        0.006f,
                        0.17f
                    ),
                    material,
                    false
                );
            }

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject
            CreatePigmentPrefab(
                Material bodyMaterial,
                Material capMaterial
            )
        {
            string path =
                PrefabFolder +
                "/PREFAB_PigmentCapsule.prefab";

            GameObject root =
                new GameObject(
                    "Pigment Capsule"
                );

            GameObject body =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            body.name = "Capsule Body";
            body.transform.SetParent(
                root.transform,
                false
            );

            body.transform.localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );

            body.transform.localScale =
                new Vector3(
                    0.065f,
                    0.1f,
                    0.065f
                );

            body.GetComponent<Renderer>()
                .sharedMaterial = bodyMaterial;

            UnityEngine.Object.DestroyImmediate(
                body.GetComponent<Collider>()
            );

            GameObject cap =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            cap.name = "Capsule Cap";
            cap.transform.SetParent(
                root.transform,
                false
            );

            cap.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    0.11f
                );

            cap.transform.localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );

            cap.transform.localScale =
                new Vector3(
                    0.072f,
                    0.02f,
                    0.072f
                );

            cap.GetComponent<Renderer>()
                .sharedMaterial = capMaterial;

            UnityEngine.Object.DestroyImmediate(
                cap.GetComponent<Collider>()
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject
            CreateFilmPrefab(
                Material material
            )
        {
            string path =
                PrefabFolder +
                "/PREFAB_SecurityFilm.prefab";

            GameObject root =
                new GameObject(
                    "Security Film"
                );

            CreateLocalCube(
                root.transform,
                "Optical Film",
                Vector3.zero,
                new Vector3(
                    0.28f,
                    0.012f,
                    0.17f
                ),
                material,
                false
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject
            CreateReplicaBundlePrefab(
                Material noteMaterial,
                Material bandMaterial
            )
        {
            string path =
                PrefabFolder +
                "/PREFAB_ReplicaNoteBundle.prefab";

            GameObject root =
                new GameObject(
                    "Replica Note Bundle"
                );

            for (
                int index = 0;
                index < 8;
                index++
            )
            {
                CreateLocalCube(
                    root.transform,
                    $"Replica Layer {index + 1}",
                    new Vector3(
                        0f,
                        index * 0.007f,
                        0f
                    ),
                    new Vector3(
                        0.27f,
                        0.006f,
                        0.15f
                    ),
                    noteMaterial,
                    false
                );
            }

            CreateLocalCube(
                root.transform,
                "Bundle Band",
                new Vector3(
                    0f,
                    0.036f,
                    0f
                ),
                new Vector3(
                    0.055f,
                    0.065f,
                    0.16f
                ),
                bandMaterial,
                false
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateOutputVisual(
            Transform workbench,
            LocalBounds bounds,
            GameObject outputPrefab
        )
        {
            Transform existing =
                FindChildRecursive(
                    workbench,
                    "Counterfeit Press Output"
                );

            if (existing != null)
            {
                UnityEngine.Object
                    .DestroyImmediate(
                        existing.gameObject
                    );
            }

            GameObject outputRoot =
                new GameObject(
                    "Counterfeit Press Output"
                );

            outputRoot.transform.SetParent(
                workbench,
                false
            );

            outputRoot.transform.localPosition =
                new Vector3(
                    bounds.max.x - 0.18f,
                    bounds.max.y + 0.12f,
                    bounds.center.z
                );

            GameObject visual =
                PrefabUtility.InstantiatePrefab(
                    outputPrefab,
                    outputRoot.transform
                ) as GameObject;

            if (visual != null)
            {
                visual.transform.localPosition =
                    Vector3.zero;

                visual.transform.localRotation =
                    Quaternion.identity;

                visual.transform.localScale =
                    Vector3.one * 0.65f;
            }

            outputRoot.SetActive(false);
            return outputRoot;
        }

        private static ZoneReferences CreateZone(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size,
            Material material
        )
        {
            GameObject zone =
                CreateLocalCube(
                    parent,
                    name,
                    localPosition,
                    size,
                    material,
                    true
                );

            BoxCollider collider =
                zone.GetComponent<BoxCollider>();

            collider.isTrigger = true;

            Transform snap =
                new GameObject(
                    name + " Snap"
                ).transform;

            snap.SetParent(
                zone.transform,
                false
            );

            snap.localPosition =
                Vector3.zero;

            return
                new ZoneReferences(
                    collider,
                    snap,
                    zone.GetComponent<Renderer>()
                );
        }

        private static ActionReferences
            CreateActionControl(
                Transform parent,
                string name,
                CounterfeitPressActionKind kind,
                Vector3 localPosition,
                Vector3 size,
                Material material
            )
        {
            GameObject control =
                CreateLocalCube(
                    parent,
                    name,
                    localPosition,
                    size,
                    material,
                    true
                );

            CounterfeitPressActionTarget target =
                control.AddComponent<
                    CounterfeitPressActionTarget
                >();

            target.Configure(kind);

            Vector3 pressed =
                localPosition +
                Vector3.down * 0.025f;

            return
                new ActionReferences(
                    control.GetComponent<Collider>(),
                    control.GetComponent<Renderer>(),
                    control.transform,
                    localPosition,
                    pressed
                );
        }

        private static Camera CreateProcessCamera(
            Transform parent,
            float width,
            float depth
        )
        {
            GameObject cameraObject =
                new GameObject(
                    "Counterfeit Process Camera"
                );

            cameraObject.transform.SetParent(
                parent,
                false
            );

            cameraObject.transform.localPosition =
                new Vector3(
                    0f,
                    1.65f,
                    0f
                );

            cameraObject.transform.localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );

            Camera camera =
                cameraObject.AddComponent<
                    Camera
                >();

            camera.orthographic = true;

            camera.orthographicSize =
                Mathf.Max(
                    depth * 0.78f,
                    width / 3f
                ) +
                0.12f;

            camera.nearClipPlane = 0.02f;
            camera.farClipPlane = 4f;
            camera.depth = 20f;
            camera.enabled = false;

            return camera;
        }

        private static GameObject CreateLocalCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider
        )
        {
            GameObject cube =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            cube.name = name;

            cube.transform.SetParent(
                parent,
                false
            );

            cube.transform.localPosition =
                localPosition;

            cube.transform.localRotation =
                Quaternion.identity;

            cube.transform.localScale =
                localScale;

            cube.GetComponent<Renderer>()
                .sharedMaterial = material;

            if (!keepCollider)
            {
                UnityEngine.Object
                    .DestroyImmediate(
                        cube.GetComponent<Collider>()
                    );
            }

            return cube;
        }

        private static Material
            GetOrCreateMaterial(
                string path,
                Color color,
                float smoothness,
                float metallic
            )
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(path);

            if (material == null)
            {
                Shader shader =
                    Shader.Find(
                        "Universal Render Pipeline/Lit"
                    ) ??
                    Shader.Find("Standard");

                material =
                    new Material(shader)
                    {
                        name =
                            Path.GetFileNameWithoutExtension(
                                path
                            )
                    };

                AssetDatabase.CreateAsset(
                    material,
                    path
                );
            }

            if (
                material.HasProperty(
                    "_BaseColor"
                )
            )
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

            if (
                material.HasProperty(
                    "_Smoothness"
                )
            )
            {
                material.SetFloat(
                    "_Smoothness",
                    smoothness
                );
            }

            if (
                material.HasProperty(
                    "_Metallic"
                )
            )
            {
                material.SetFloat(
                    "_Metallic",
                    metallic
                );
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateIcon(
            string displayName,
            Color color
        )
        {
            string fileName =
                "ITEM_" +
                displayName.Replace(
                    " ",
                    string.Empty
                ) +
                ".png";

            string path =
                IconFolder +
                "/" +
                fileName;

            Texture2D texture =
                new Texture2D(
                    128,
                    128,
                    TextureFormat.RGBA32,
                    false
                );

            Color background =
                new Color(
                    0f,
                    0f,
                    0f,
                    0f
                );

            for (
                int y = 0;
                y < 128;
                y++
            )
            {
                for (
                    int x = 0;
                    x < 128;
                    x++
                )
                {
                    bool inside =
                        x >= 20 &&
                        x <= 107 &&
                        y >= 35 &&
                        y <= 92;

                    bool band =
                        x >= 57 &&
                        x <= 70 &&
                        y >= 29 &&
                        y <= 98;

                    Color pixel =
                        inside
                            ? color
                            : background;

                    if (band)
                    {
                        pixel =
                            new Color(
                                0.96f,
                                0.45f,
                                0.03f,
                                1f
                            );
                    }

                    texture.SetPixel(
                        x,
                        y,
                        pixel
                    );
                }
            }

            texture.Apply();

            File.WriteAllBytes(
                path,
                texture.EncodeToPNG()
            );

            UnityEngine.Object
                .DestroyImmediate(texture);

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceUpdate
            );
        }

        private static void
            EnsureInventoryMinimum(
                PlayerInventory inventory,
                ItemDefinition item,
                int minimum
            )
        {
            if (
                inventory == null ||
                item == null
            )
            {
                return;
            }

            int current =
                inventory.CountItem(item);

            int needed =
                Mathf.Max(
                    0,
                    minimum - current
                );

            if (needed <= 0)
            {
                return;
            }

            inventory.AddItem(
                item,
                needed,
                ItemQuality.Standard,
                1f
            );
        }

        private static ItemDefinition FindItem(
            ItemDatabase database,
            string itemId,
            string displayName
        )
        {
            if (database == null)
            {
                return null;
            }

            if (
                !string.IsNullOrWhiteSpace(
                    itemId
                )
            )
            {
                ItemDefinition byId =
                    database.GetItem(itemId);

                if (byId != null)
                {
                    return byId;
                }
            }

            return
                database.Items.FirstOrDefault(
                    item =>
                        item != null &&
                        string.Equals(
                            item.DisplayName,
                            displayName,
                            StringComparison
                                .OrdinalIgnoreCase
                        )
                );
        }

        private static void AddUnique(
            List<ItemDefinition> items,
            ItemDefinition item
        )
        {
            if (
                item != null &&
                !items.Contains(item)
            )
            {
                items.Add(item);
            }
        }

        private static LocalBounds
            CalculateLocalBounds(
                Transform root
            )
        {
            Renderer[] renderers =
                root.GetComponentsInChildren<
                    Renderer
                >(true);

            bool initialized = false;
            Bounds localBounds =
                new Bounds();

            foreach (Renderer renderer in renderers)
            {
                if (
                    renderer == null ||
                    HasAncestorNamed(
                        renderer.transform,
                        ProcessRootName
                    ) ||
                    HasAncestorNamed(
                        renderer.transform,
                        "Workbench Production Output"
                    ) ||
                    HasAncestorNamed(
                        renderer.transform,
                        "Counterfeit Press Output"
                    )
                )
                {
                    continue;
                }

                Bounds world =
                    renderer.bounds;

                Vector3 min = world.min;
                Vector3 max = world.max;

                Vector3[] corners =
                {
                    new Vector3(
                        min.x,
                        min.y,
                        min.z
                    ),
                    new Vector3(
                        min.x,
                        min.y,
                        max.z
                    ),
                    new Vector3(
                        min.x,
                        max.y,
                        min.z
                    ),
                    new Vector3(
                        min.x,
                        max.y,
                        max.z
                    ),
                    new Vector3(
                        max.x,
                        min.y,
                        min.z
                    ),
                    new Vector3(
                        max.x,
                        min.y,
                        max.z
                    ),
                    new Vector3(
                        max.x,
                        max.y,
                        min.z
                    ),
                    new Vector3(
                        max.x,
                        max.y,
                        max.z
                    )
                };

                foreach (Vector3 corner in corners)
                {
                    Vector3 local =
                        root.InverseTransformPoint(
                            corner
                        );

                    if (!initialized)
                    {
                        localBounds =
                            new Bounds(
                                local,
                                Vector3.zero
                            );

                        initialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(
                            local
                        );
                    }
                }
            }

            if (!initialized)
            {
                localBounds =
                    new Bounds(
                        Vector3.up,
                        new Vector3(
                            2f,
                            1f,
                            0.8f
                        )
                    );
            }

            return
                new LocalBounds(
                    localBounds.center,
                    localBounds.size,
                    localBounds.min,
                    localBounds.max
                );
        }

        private static bool HasAncestorNamed(
            Transform target,
            string requestedName
        )
        {
            Transform current = target;

            while (current != null)
            {
                if (
                    string.Equals(
                        current.name,
                        requestedName,
                        StringComparison.Ordinal
                    )
                )
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static Transform
            FindChildRecursive(
                Transform root,
                string requestedName
            )
        {
            if (root == null)
            {
                return null;
            }

            if (
                string.Equals(
                    root.name,
                    requestedName,
                    StringComparison.Ordinal
                )
            )
            {
                return root;
            }

            foreach (Transform child in root)
            {
                Transform result =
                    FindChildRecursive(
                        child,
                        requestedName
                    );

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void EnsureFolder(
            string path
        )
        {
            if (
                string.IsNullOrWhiteSpace(path) ||
                AssetDatabase.IsValidFolder(path)
            )
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(path);

            EnsureFolder(parent);

            AssetDatabase.CreateFolder(
                parent,
                folderName
            );
        }

        private static void SetString(
            SerializedObject serialized,
            string propertyName,
            string value
        )
        {
            SerializedProperty property =
                serialized.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetInt(
            SerializedObject serialized,
            string propertyName,
            int value
        )
        {
            SerializedProperty property =
                serialized.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetEnum(
            SerializedObject serialized,
            string propertyName,
            int value
        )
        {
            SerializedProperty property =
                serialized.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.enumValueIndex = value;
            }
        }

        private static void SetColor(
            SerializedObject serialized,
            string propertyName,
            Color value
        )
        {
            SerializedProperty property =
                serialized.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void SetVector3(
            SerializedObject serialized,
            string propertyName,
            Vector3 value
        )
        {
            SerializedProperty property =
                serialized.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static void SetObject(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value
        )
        {
            SerializedProperty property =
                serialized.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.objectReferenceValue =
                    value;
            }
        }

        private static void ShowError(
            string message
        )
        {
            Debug.LogError(
                "[Milestone9A] " +
                message
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }

        private readonly struct ZoneReferences
        {
            public readonly BoxCollider collider;
            public readonly Transform snap;
            public readonly Renderer renderer;

            public ZoneReferences(
                BoxCollider zoneCollider,
                Transform zoneSnap,
                Renderer zoneRenderer
            )
            {
                collider = zoneCollider;
                snap = zoneSnap;
                renderer = zoneRenderer;
            }
        }

        private readonly struct ActionReferences
        {
            public readonly Collider collider;
            public readonly Renderer renderer;
            public readonly Transform transform;
            public readonly Vector3
                releasedLocalPosition;
            public readonly Vector3
                pressedLocalPosition;

            public ActionReferences(
                Collider actionCollider,
                Renderer actionRenderer,
                Transform actionTransform,
                Vector3 releasedPosition,
                Vector3 pressedPosition
            )
            {
                collider = actionCollider;
                renderer = actionRenderer;
                transform = actionTransform;
                releasedLocalPosition =
                    releasedPosition;
                pressedLocalPosition =
                    pressedPosition;
            }
        }

        private readonly struct LocalBounds
        {
            public readonly Vector3 center;
            public readonly Vector3 size;
            public readonly Vector3 min;
            public readonly Vector3 max;

            public LocalBounds(
                Vector3 boundsCenter,
                Vector3 boundsSize,
                Vector3 boundsMin,
                Vector3 boundsMax
            )
            {
                center = boundsCenter;
                size = boundsSize;
                min = boundsMin;
                max = boundsMax;
            }
        }
    }
}
