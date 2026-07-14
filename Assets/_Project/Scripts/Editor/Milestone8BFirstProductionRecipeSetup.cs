using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Electrical;
using ShadowSupply.Inventory;
using ShadowSupply.Production;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class
        Milestone8BFirstProductionRecipeSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/" +
            "Dev_Playground.unity";

        private const string GarageRootName =
            "StarterGarage_Property";

        private const string ProductionRootName =
            "Milestone8B_ProductionSystem";

        private const string ItemDataFolder =
            "Assets/_Project/Data/Items/Production";

        private const string RecipeDataFolder =
            "Assets/_Project/Data/Production/Recipes";

        private const string DatabaseFolder =
            "Assets/_Project/Data/Databases";

        private const string ItemDatabasePath =
            DatabaseFolder + "/DB_Items.asset";

        private const string RecipeDatabasePath =
            DatabaseFolder +
            "/DB_ProductionRecipes.asset";

        private const string PrefabFolder =
            "Assets/_Project/Prefabs/Production";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/Production";

        private const string PackagingItemPath =
            ItemDataFolder +
            "/ITEM_PackagingMaterial.asset";

        private const string OutputItemPath =
            ItemDataFolder +
            "/ITEM_SealedHardwarePackage.asset";

        private const string RecipePath =
            RecipeDataFolder +
            "/RECIPE_SealedHardwarePackage.asset";

        private const string PackagingPrefabPath =
            PrefabFolder +
            "/PREFAB_PackagingMaterial.prefab";

        private const string OutputPrefabPath =
            PrefabFolder +
            "/PREFAB_SealedHardwarePackage.prefab";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 8B First Production Recipe"
        )]
        public static void ApplyMilestoneEightB()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );

            if (!scene.IsValid())
            {
                ShowError(
                    "Dev_Playground could not be opened."
                );

                return;
            }

            GameObject garage =
                GameObject.Find(GarageRootName);

            PlayerInventory inventory =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PlayerInventory
                    >();

            MachinePowerConnection machinePower =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        MachinePowerConnection
                    >();

            SaveManager saveManager =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        SaveManager
                    >();

            if (
                garage == null ||
                inventory == null ||
                machinePower == null ||
                saveManager == null ||
                SaveManager.CurrentSaveVersion < 5
            )
            {
                ShowError(
                    "Milestone 8B requires the confirmed starter " +
                    "garage, inventory, save system, and physical " +
                    "garage power from Milestone 8A."
                );

                return;
            }

            EnsureFolders();

            ItemDatabase itemDatabase =
                AssetDatabase.LoadAssetAtPath<
                    ItemDatabase
                >(ItemDatabasePath);

            if (itemDatabase == null)
            {
                ShowError(
                    "DB_Items.asset was not found. " +
                    "Reapply the inventory setup first."
                );

                return;
            }

            ItemDefinition metalComponents =
                FindItemByDisplayName(
                    itemDatabase,
                    "Metal Components"
                );

            ItemDefinition polymerHousing =
                FindItemByDisplayName(
                    itemDatabase,
                    "Polymer Housing"
                );

            ItemDefinition basicToolkit =
                FindItemByDisplayName(
                    itemDatabase,
                    "Basic Toolkit"
                );

            List<string> missingItems =
                new List<string>();

            if (metalComponents == null)
            {
                missingItems.Add(
                    "Metal Components"
                );
            }

            if (polymerHousing == null)
            {
                missingItems.Add(
                    "Polymer Housing"
                );
            }

            if (basicToolkit == null)
            {
                missingItems.Add(
                    "Basic Toolkit"
                );
            }

            if (missingItems.Count > 0)
            {
                ShowError(
                    "Required starter items were not found:\n\n" +
                    string.Join(
                        "\n",
                        missingItems
                    )
                );

                return;
            }

            ProductionMaterials materials =
                CreateMaterials();

            GameObject packagingPrefab =
                CreatePackagingPrefab(materials);

            GameObject outputPrefab =
                CreateOutputPrefab(materials);

            ItemDefinition packagingMaterial =
                GetOrCreateItem(
                    PackagingItemPath,
                    "production-packaging-material",
                    "Packaging Material",
                    "Folded wrap, tape, and seals used to " +
                    "prepare finished goods for delivery.",
                    ItemCategory.Material,
                    20,
                    8,
                    packagingPrefab,
                    new Vector3(
                        0.43f,
                        -0.38f,
                        0.72f
                    ),
                    new Vector3(
                        6f,
                        -20f,
                        0f
                    ),
                    Vector3.one * 0.3f
                );

            ItemDefinition sealedPackage =
                GetOrCreateItem(
                    OutputItemPath,
                    "product-sealed-hardware-package",
                    "Sealed Hardware Package",
                    "A compact sealed package assembled at a " +
                    "powered workbench. Ready for a buyer order.",
                    ItemCategory.Product,
                    10,
                    180,
                    outputPrefab,
                    new Vector3(
                        0.43f,
                        -0.36f,
                        0.7f
                    ),
                    new Vector3(
                        8f,
                        -18f,
                        0f
                    ),
                    Vector3.one * 0.34f
                );

            UpdateItemDatabase(
                itemDatabase
            );

            ProductionRecipe recipe =
                GetOrCreateRecipe(
                    metalComponents,
                    polymerHousing,
                    basicToolkit,
                    packagingMaterial,
                    sealedPackage
                );

            ProductionRecipeDatabase recipeDatabase =
                GetOrCreateRecipeDatabase(recipe);

            RemovePreviousProductionSetup(
                garage
            );

            GameObject productionRoot =
                new GameObject(
                    ProductionRootName
                );

            productionRoot.transform.SetParent(
                garage.transform,
                false
            );

            ProductionWorkbenchHUD hud =
                productionRoot.AddComponent<
                    ProductionWorkbenchHUD
                >();

            Transform workbench =
                FindChildRecursive(
                    garage.transform,
                    "Included Workbench"
                );

            if (workbench == null)
            {
                UnityEngine.Object.DestroyImmediate(
                    productionRoot
                );

                ShowError(
                    "Included Workbench was not found."
                );

                return;
            }

            GameObject outputVisual =
                BuildWorkbenchOutputTray(
                    workbench,
                    outputPrefab,
                    materials
                );

            PoweredWorkbenchProduction production =
                workbench.gameObject.AddComponent<
                    PoweredWorkbenchProduction
                >();

            production.Configure(
                "starter-workbench",
                inventory,
                machinePower,
                recipeDatabase,
                outputVisual
            );

            EnsureMinimumInventory(
                inventory,
                metalComponents,
                4
            );

            EnsureMinimumInventory(
                inventory,
                polymerHousing,
                2
            );

            EnsureMinimumInventory(
                inventory,
                basicToolkit,
                1
            );

            EnsureMinimumInventory(
                inventory,
                packagingMaterial,
                3
            );

            EditorUtility.SetDirty(inventory);
            EditorUtility.SetDirty(production);
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(recipe);
            EditorUtility.SetDirty(recipeDatabase);
            EditorUtility.SetDirty(itemDatabase);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                workbench.gameObject;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 8B first production recipe applied.\n\n" +
                "The powered starter workbench can now assemble a " +
                "Sealed Hardware Package.\n\n" +
                "A small development supply of components, " +
                "packaging material, and one toolkit was ensured " +
                "in the player inventory for testing.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 8B First Production Recipe"
        )]
        public static void ValidateMilestoneEightB()
        {
            PoweredWorkbenchProduction workbench =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >();

            ProductionWorkbenchHUD hud =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ProductionWorkbenchHUD
                    >();

            ProductionRecipeDatabase recipeDatabase =
                AssetDatabase.LoadAssetAtPath<
                    ProductionRecipeDatabase
                >(RecipeDatabasePath);

            ItemDatabase itemDatabase =
                AssetDatabase.LoadAssetAtPath<
                    ItemDatabase
                >(ItemDatabasePath);

            ItemDefinition packaging =
                itemDatabase != null
                    ? FindItemByDisplayName(
                        itemDatabase,
                        "Packaging Material"
                    )
                    : null;

            ItemDefinition output =
                itemDatabase != null
                    ? FindItemByDisplayName(
                        itemDatabase,
                        "Sealed Hardware Package"
                    )
                    : null;

            bool valid =
                workbench != null &&
                workbench.PowerConnection != null &&
                workbench.RecipeDatabase != null &&
                hud != null &&
                recipeDatabase != null &&
                recipeDatabase.Recipes.Count == 1 &&
                packaging != null &&
                output != null &&
                SaveManager.CurrentSaveVersion == 6;

            string report =
                "Powered workbench: " +
                (
                    workbench != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nPower connection: " +
                (
                    workbench != null &&
                    workbench.PowerConnection != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nRecipe database: " +
                (
                    recipeDatabase != null
                        ? recipeDatabase.Recipes.Count +
                          " recipe(s)"
                        : "MISSING"
                ) +
                "\nProduction HUD: " +
                (
                    hud != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nPackaging Material: " +
                (
                    packaging != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nSealed Hardware Package: " +
                (
                    output != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nSave schema: " +
                SaveManager.CurrentSaveVersion +
                " / 6";

            if (valid)
            {
                Debug.Log(
                    "[Milestone8B] FIRST PRODUCTION " +
                    "RECIPE READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8B Validation",
                    "FIRST PRODUCTION RECIPE READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone8B] VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8B Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static ProductionRecipe
            GetOrCreateRecipe(
                ItemDefinition metalComponents,
                ItemDefinition polymerHousing,
                ItemDefinition basicToolkit,
                ItemDefinition packagingMaterial,
                ItemDefinition sealedPackage
            )
        {
            ProductionRecipe recipe =
                AssetDatabase.LoadAssetAtPath<
                    ProductionRecipe
                >(RecipePath);

            if (recipe == null)
            {
                recipe =
                    ScriptableObject.CreateInstance<
                        ProductionRecipe
                    >();

                AssetDatabase.CreateAsset(
                    recipe,
                    RecipePath
                );
            }

            recipe.Configure(
                "recipe-sealed-hardware-package",
                "Sealed Hardware Package",
                "Combine a reinforced housing with metal " +
                "components, then wrap and seal the finished unit.",
                8f,
                true,
                new[]
                {
                    new ProductionIngredient(
                        metalComponents,
                        2,
                        true
                    ),
                    new ProductionIngredient(
                        polymerHousing,
                        1,
                        true
                    ),
                    new ProductionIngredient(
                        packagingMaterial,
                        1,
                        true
                    ),
                    new ProductionIngredient(
                        basicToolkit,
                        1,
                        false
                    )
                },
                sealedPackage,
                1
            );

            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static ProductionRecipeDatabase
            GetOrCreateRecipeDatabase(
                ProductionRecipe recipe
            )
        {
            ProductionRecipeDatabase database =
                AssetDatabase.LoadAssetAtPath<
                    ProductionRecipeDatabase
                >(RecipeDatabasePath);

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<
                        ProductionRecipeDatabase
                    >();

                AssetDatabase.CreateAsset(
                    database,
                    RecipeDatabasePath
                );
            }

            database.SetRecipes(
                new[]
                {
                    recipe
                }
            );

            EditorUtility.SetDirty(database);
            return database;
        }

        private static ItemDefinition GetOrCreateItem(
            string path,
            string stableItemId,
            string displayName,
            string description,
            ItemCategory category,
            int maximumStack,
            int baseValue,
            GameObject displayPrefab,
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

            serialized.FindProperty(
                "itemId"
            ).stringValue =
                stableItemId;

            serialized.FindProperty(
                "displayName"
            ).stringValue =
                displayName;

            serialized.FindProperty(
                "description"
            ).stringValue =
                description;

            serialized.FindProperty(
                "category"
            ).enumValueIndex =
                (int)category;

            serialized.FindProperty(
                "maximumStack"
            ).intValue =
                Mathf.Max(
                    1,
                    maximumStack
                );

            serialized.FindProperty(
                "baseValue"
            ).intValue =
                Mathf.Max(
                    0,
                    baseValue
                );

            serialized.FindProperty(
                "displayPrefab"
            ).objectReferenceValue =
                displayPrefab;

            serialized.FindProperty(
                "heldLocalPosition"
            ).vector3Value =
                heldPosition;

            serialized.FindProperty(
                "heldLocalEulerAngles"
            ).vector3Value =
                heldEuler;

            serialized.FindProperty(
                "heldLocalScale"
            ).vector3Value =
                heldScale;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            item.EnsurePersistentId();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void UpdateItemDatabase(
            ItemDatabase database
        )
        {
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
                        path =>
                            AssetDatabase
                                .LoadAssetAtPath<
                                    ItemDefinition
                                >(path)
                    )
                    .Where(
                        item => item != null
                    )
                    .Distinct()
                    .ToList();

            database.SetItems(allItems);
            EditorUtility.SetDirty(database);
        }

        private static ItemDefinition
            FindItemByDisplayName(
                ItemDatabase database,
                string displayName
            )
        {
            if (database == null)
            {
                return null;
            }

            ItemDefinition direct =
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

            if (direct != null)
            {
                return direct;
            }

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:ItemDefinition",
                    new[]
                    {
                        "Assets/_Project/Data/Items"
                    }
                );

            foreach (string guid in guids)
            {
                ItemDefinition item =
                    AssetDatabase.LoadAssetAtPath<
                        ItemDefinition
                    >(
                        AssetDatabase
                            .GUIDToAssetPath(guid)
                    );

                if (
                    item != null &&
                    string.Equals(
                        item.DisplayName,
                        displayName,
                        StringComparison
                            .OrdinalIgnoreCase
                    )
                )
                {
                    return item;
                }
            }

            return null;
        }

        private static void EnsureMinimumInventory(
            PlayerInventory inventory,
            ItemDefinition item,
            int minimumQuantity
        )
        {
            int current =
                inventory.CountItem(item);

            if (current >= minimumQuantity)
            {
                return;
            }

            inventory.AddItem(
                item,
                minimumQuantity - current,
                ItemQuality.Standard,
                1f
            );
        }

        private static GameObject
            BuildWorkbenchOutputTray(
                Transform workbench,
                GameObject outputPrefab,
                ProductionMaterials materials
            )
        {
            Bounds bounds =
                CalculateBounds(
                    workbench.gameObject
                );

            GameObject tray =
                new GameObject(
                    "Workbench Production Output"
                );

            tray.transform.SetParent(
                workbench,
                true
            );

            tray.transform.position =
                new Vector3(
                    bounds.center.x,
                    bounds.max.y + 0.035f,
                    bounds.center.z
                );

            tray.transform.rotation =
                workbench.rotation;

            CreateLocalCube(
                tray.transform,
                "Output Tray",
                Vector3.zero,
                new Vector3(
                    0.5f,
                    0.035f,
                    0.34f
                ),
                materials.darkMetal,
                false
            );

            GameObject outputVisual =
                PrefabUtility.InstantiatePrefab(
                    outputPrefab,
                    tray.transform
                ) as GameObject;

            if (outputVisual == null)
            {
                outputVisual =
                    UnityEngine.Object.Instantiate(
                        outputPrefab,
                        tray.transform
                    );
            }

            outputVisual.name =
                "Ready Sealed Package";

            outputVisual.transform.localPosition =
                new Vector3(
                    0f,
                    0.13f,
                    0f
                );

            outputVisual.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    12f,
                    0f
                );

            outputVisual.transform.localScale =
                Vector3.one;

            RemoveAllColliders(
                outputVisual
            );

            outputVisual.SetActive(false);
            return outputVisual;
        }

        private static GameObject
            CreatePackagingPrefab(
                ProductionMaterials materials
            )
        {
            GameObject root =
                new GameObject(
                    "PREFAB_PackagingMaterial"
                );

            CreateLocalCube(
                root.transform,
                "Folded Wrap",
                Vector3.zero,
                new Vector3(
                    0.34f,
                    0.1f,
                    0.24f
                ),
                materials.cardboard,
                false
            );

            CreateLocalCube(
                root.transform,
                "Tape Band",
                new Vector3(
                    0f,
                    0.056f,
                    0f
                ),
                new Vector3(
                    0.07f,
                    0.018f,
                    0.25f
                ),
                materials.orange,
                false
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PackagingPrefabPath
                );

            UnityEngine.Object.DestroyImmediate(
                root
            );

            return prefab;
        }

        private static GameObject CreateOutputPrefab(
            ProductionMaterials materials
        )
        {
            GameObject root =
                new GameObject(
                    "PREFAB_SealedHardwarePackage"
                );

            CreateLocalCube(
                root.transform,
                "Package Body",
                Vector3.zero,
                new Vector3(
                    0.36f,
                    0.18f,
                    0.26f
                ),
                materials.package,
                false
            );

            CreateLocalCube(
                root.transform,
                "Long Seal",
                new Vector3(
                    0f,
                    0.096f,
                    0f
                ),
                new Vector3(
                    0.075f,
                    0.018f,
                    0.27f
                ),
                materials.orange,
                false
            );

            CreateLocalCube(
                root.transform,
                "Cross Seal",
                new Vector3(
                    0f,
                    0.097f,
                    0f
                ),
                new Vector3(
                    0.37f,
                    0.019f,
                    0.06f
                ),
                materials.orange,
                false
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    OutputPrefabPath
                );

            UnityEngine.Object.DestroyImmediate(
                root
            );

            return prefab;
        }

        private static ProductionMaterials CreateMaterials()
        {
            return new ProductionMaterials
            {
                cardboard =
                    CreateMaterial(
                        "MAT_Production_Cardboard",
                        new Color(
                            0.39f,
                            0.28f,
                            0.17f
                        ),
                        0.08f,
                        0f
                    ),

                package =
                    CreateMaterial(
                        "MAT_Production_Package",
                        new Color(
                            0.14f,
                            0.15f,
                            0.16f
                        ),
                        0.18f,
                        0.18f
                    ),

                orange =
                    CreateMaterial(
                        "MAT_Production_SealOrange",
                        new Color(
                            0.9f,
                            0.25f,
                            0.015f
                        ),
                        0.26f,
                        0.1f
                    ),

                darkMetal =
                    CreateMaterial(
                        "MAT_Production_Tray",
                        new Color(
                            0.08f,
                            0.09f,
                            0.1f
                        ),
                        0.28f,
                        0.7f
                    )
            };
        }

        private static Material CreateMaterial(
            string name,
            Color color,
            float smoothness,
            float metallic
        )
        {
            string path =
                MaterialFolder + "/" +
                name + ".mat";

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
                        name = name
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

        private static GameObject CreateLocalCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size,
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

            cube.transform.localScale = size;

            Renderer renderer =
                cube.GetComponent<Renderer>();

            renderer.sharedMaterial = material;

            if (!keepCollider)
            {
                Collider collider =
                    cube.GetComponent<Collider>();

                UnityEngine.Object.DestroyImmediate(
                    collider
                );
            }

            return cube;
        }

        private static Bounds CalculateBounds(
            GameObject target
        )
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<
                    Renderer
                >(true);

            List<Renderer> furnitureRenderers =
                renderers
                    .Where(
                        renderer =>
                            renderer != null &&
                            renderer.name.IndexOf(
                                "Power Indicator",
                                StringComparison.OrdinalIgnoreCase
                            ) < 0 &&
                            renderer.name.IndexOf(
                                "Production Output",
                                StringComparison.OrdinalIgnoreCase
                            ) < 0
                    )
                    .ToList();

            if (furnitureRenderers.Count == 0)
            {
                return new Bounds(
                    target.transform.position,
                    Vector3.one
                );
            }

            Bounds bounds =
                furnitureRenderers[0].bounds;

            for (
                int index = 1;
                index < furnitureRenderers.Count;
                index++
            )
            {
                bounds.Encapsulate(
                    furnitureRenderers[index].bounds
                );
            }

            return bounds;
        }

        private static void RemovePreviousProductionSetup(
            GameObject garage
        )
        {
            Transform previousRoot =
                FindChildRecursive(
                    garage.transform,
                    ProductionRootName
                );

            if (previousRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    previousRoot.gameObject
                );
            }

            PoweredWorkbenchProduction[] production =
                garage.GetComponentsInChildren<
                    PoweredWorkbenchProduction
                >(true);

            foreach (
                PoweredWorkbenchProduction component
                in production
            )
            {
                UnityEngine.Object.DestroyImmediate(
                    component
                );
            }

            DestroyNamedChildren(
                garage.transform,
                "Workbench Production Output"
            );
        }

        private static void DestroyNamedChildren(
            Transform root,
            string requestedName
        )
        {
            List<GameObject> matches =
                new List<GameObject>();

            CollectNamedChildren(
                root,
                requestedName,
                matches
            );

            foreach (GameObject match in matches)
            {
                if (match != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        match
                    );
                }
            }
        }

        private static void CollectNamedChildren(
            Transform current,
            string requestedName,
            List<GameObject> results
        )
        {
            if (current == null)
            {
                return;
            }

            for (
                int index = 0;
                index < current.childCount;
                index++
            )
            {
                Transform child =
                    current.GetChild(index);

                if (
                    string.Equals(
                        child.name,
                        requestedName,
                        StringComparison.Ordinal
                    )
                )
                {
                    results.Add(child.gameObject);
                    continue;
                }

                CollectNamedChildren(
                    child,
                    requestedName,
                    results
                );
            }
        }

        private static void RemoveAllColliders(
            GameObject target
        )
        {
            foreach (
                Collider collider
                in target.GetComponentsInChildren<
                    Collider
                >(true)
            )
            {
                UnityEngine.Object.DestroyImmediate(
                    collider
                );
            }
        }

        private static Transform FindChildRecursive(
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
                Transform match =
                    FindChildRecursive(
                        child,
                        requestedName
                    );

                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void EnsureFolders()
        {
            foreach (
                string folder
                in new[]
                {
                    "Assets/_Project",
                    "Assets/_Project/Data",
                    "Assets/_Project/Data/Items",
                    ItemDataFolder,
                    "Assets/_Project/Data/Production",
                    RecipeDataFolder,
                    DatabaseFolder,
                    "Assets/_Project/Prefabs",
                    PrefabFolder,
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
            string path
        )
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(path);

            if (
                !string.IsNullOrWhiteSpace(parent)
            )
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                folderName
            );
        }

        private static void ShowError(
            string message
        )
        {
            Debug.LogError(
                "[Milestone8B] " + message
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }

        private sealed class ProductionMaterials
        {
            public Material cardboard;
            public Material package;
            public Material orange;
            public Material darkMetal;
        }
    }
}
