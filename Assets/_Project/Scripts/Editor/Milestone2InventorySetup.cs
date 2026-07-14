using System.IO;
using ShadowSupply.Inventory;
using ShadowSupply.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone2InventorySetup
    {
        private const string ItemFolder =
            "Assets/_Project/Data/Items/Development";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/Development/Items";

        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        [MenuItem("Shadow Supply/Setup/Apply Milestone 2 Inventory")]
        public static void ApplyMilestoneTwo()
        {
            Scene scene = SceneManager.GetActiveScene();

            if (!scene.IsValid() || scene.path != ScenePath)
            {
                SceneAsset sceneAsset =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

                if (sceneAsset == null)
                {
                    EditorUtility.DisplayDialog(
                        "Shadow Supply",
                        "Create the Milestone 1 playground first.",
                        "OK"
                    );
                    return;
                }

                scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "The playground does not contain a Player object.",
                    "OK"
                );
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder("Assets/_Project/Data/Items");
            EnsureFolder(ItemFolder);
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/Materials");
            EnsureFolder("Assets/_Project/Art/Materials/Development");
            EnsureFolder(MaterialFolder);

            ItemDefinition metalComponents = GetOrCreateItem(
                "ITEM_MetalComponents",
                "Metal Components",
                "Common metal parts used in early assembly recipes.",
                ItemCategory.Material,
                20,
                12,
                PrimitiveType.Cube,
                new Color(0.36f, 0.39f, 0.42f),
                Vector3.one * 0.19f
            );

            ItemDefinition polymerHousing = GetOrCreateItem(
                "ITEM_PolymerHousing",
                "Polymer Housing",
                "A molded housing used to protect internal components.",
                ItemCategory.Component,
                10,
                18,
                PrimitiveType.Cube,
                new Color(0.10f, 0.34f, 0.52f),
                new Vector3(0.28f, 0.16f, 0.36f)
            );

            ItemDefinition basicToolkit = GetOrCreateItem(
                "ITEM_BasicToolkit",
                "Basic Toolkit",
                "A compact toolkit needed for simple repair and assembly work.",
                ItemCategory.Tool,
                1,
                75,
                PrimitiveType.Capsule,
                new Color(0.90f, 0.42f, 0.08f),
                new Vector3(0.18f, 0.24f, 0.18f)
            );

            ItemDefinition sealedPackage = GetOrCreateItem(
                "ITEM_SealedPackage",
                "Sealed Package",
                "A finished package prepared for delivery to a buyer.",
                ItemCategory.Product,
                5,
                140,
                PrimitiveType.Cube,
                new Color(0.48f, 0.08f, 0.09f),
                new Vector3(0.30f, 0.18f, 0.38f)
            );

            AddPlayerComponents(player);
            AddInventoryHud();
            CreatePickups(
                metalComponents,
                polymerHousing,
                basicToolkit,
                sealedPackage
            );

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 2 inventory applied.\n\n" +
                "E: Pick up items\n" +
                "1–8 / Mouse Wheel: Select hotbar slot\n" +
                "TAB or I: Open inventory\n" +
                "Q: Drop one selected item",
                "Done"
            );
        }

        private static void AddPlayerComponents(GameObject player)
        {
            if (player.GetComponent<PlayerInventory>() == null)
            {
                player.AddComponent<PlayerInventory>();
            }

            if (player.GetComponent<HeldItemDisplay>() == null)
            {
                player.AddComponent<HeldItemDisplay>();
            }

            if (player.GetComponent<HotbarController>() == null)
            {
                player.AddComponent<HotbarController>();
            }
        }

        private static void AddInventoryHud()
        {
            GameObject services = GameObject.Find("RuntimeServices");

            if (services == null)
            {
                services = new GameObject("RuntimeServices");
            }

            if (services.GetComponent<InventoryHUD>() == null)
            {
                services.AddComponent<InventoryHUD>();
            }
        }

        private static void CreatePickups(
            ItemDefinition metalComponents,
            ItemDefinition polymerHousing,
            ItemDefinition basicToolkit,
            ItemDefinition sealedPackage
        )
        {
            GameObject existing = GameObject.Find("Milestone2_Items");

            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject root = new GameObject("Milestone2_Items");

            CreatePickup(
                root.transform,
                metalComponents,
                12,
                new Vector3(-4.5f, 0.35f, 3f)
            );

            CreatePickup(
                root.transform,
                metalComponents,
                16,
                new Vector3(-3.2f, 0.35f, 3f)
            );

            CreatePickup(
                root.transform,
                polymerHousing,
                6,
                new Vector3(-1.8f, 0.35f, 3f)
            );

            CreatePickup(
                root.transform,
                basicToolkit,
                1,
                new Vector3(1.8f, 0.45f, 3f)
            );

            CreatePickup(
                root.transform,
                sealedPackage,
                3,
                new Vector3(3.4f, 0.35f, 3f)
            );

            CreatePickup(
                root.transform,
                sealedPackage,
                4,
                new Vector3(4.7f, 0.35f, 3f)
            );
        }

        private static void CreatePickup(
            Transform parent,
            ItemDefinition item,
            int quantity,
            Vector3 position
        )
        {
            GameObject pickupObject =
                GameObject.CreatePrimitive(item.FallbackPrimitive);

            pickupObject.transform.SetParent(parent);
            pickupObject.transform.position = position;
            pickupObject.transform.localScale =
                item.FallbackPrimitive == PrimitiveType.Capsule
                    ? new Vector3(0.35f, 0.45f, 0.35f)
                    : Vector3.one * 0.55f;

            Renderer renderer = pickupObject.GetComponent<Renderer>();
            renderer.sharedMaterial = GetOrCreateMaterial(
                $"{MaterialFolder}/MAT_{Sanitize(item.DisplayName)}.mat",
                item.FallbackColor
            );

            WorldItemPickup pickup =
                pickupObject.AddComponent<WorldItemPickup>();

            pickup.Initialize(item, quantity);
        }

        private static ItemDefinition GetOrCreateItem(
            string assetName,
            string displayName,
            string description,
            ItemCategory category,
            int maximumStack,
            int baseValue,
            PrimitiveType fallbackPrimitive,
            Color fallbackColor,
            Vector3 heldScale
        )
        {
            string path = $"{ItemFolder}/{assetName}.asset";

            ItemDefinition definition =
                AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<ItemDefinition>();

                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized =
                new SerializedObject(definition);

            serialized.FindProperty("displayName").stringValue =
                displayName;

            serialized.FindProperty("description").stringValue =
                description;

            serialized.FindProperty("category").enumValueIndex =
                (int)category;

            serialized.FindProperty("maximumStack").intValue =
                maximumStack;

            serialized.FindProperty("baseValue").intValue =
                baseValue;

            serialized.FindProperty("fallbackPrimitive").enumValueIndex =
                (int)fallbackPrimitive;

            serialized.FindProperty("fallbackColor").colorValue =
                fallbackColor;

            serialized.FindProperty("heldLocalScale").vector3Value =
                heldScale;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            definition.EnsurePersistentId();
            EditorUtility.SetDirty(definition);

            return definition;
        }

        private static Material GetOrCreateMaterial(
            string path,
            Color baseColor
        )
        {
            Material existing =
                AssetDatabase.LoadAssetAtPath<Material>(path);

            if (existing != null)
            {
                SetMaterialColor(existing, baseColor);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");

            Material material = new Material(shader)
            {
                name = Path.GetFileNameWithoutExtension(path)
            };

            SetMaterialColor(material, baseColor);

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.18f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void SetMaterialColor(
            Material material,
            Color color
        )
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.color = color;
            }
        }

        private static string Sanitize(string value)
        {
            return value.Replace(" ", string.Empty);
        }

        private static void EnsureFolder(string fullPath)
        {
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(fullPath)?.Replace("\\", "/");

            string folderName = Path.GetFileName(fullPath);

            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
