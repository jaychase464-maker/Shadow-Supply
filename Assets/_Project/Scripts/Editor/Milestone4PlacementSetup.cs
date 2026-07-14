using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Placement;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone4PlacementSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        private const string DataFolder =
            "Assets/_Project/Data/Placeables/Development";

        private const string DatabaseFolder =
            "Assets/_Project/Data/Databases";

        private const string DatabasePath =
            DatabaseFolder + "/DB_Placeables.asset";

        private const string PrefabFolder =
            "Assets/_Project/Prefabs/Placeables/Development";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/Placement";

        [MenuItem("Shadow Supply/Setup/Apply Milestone 4 Placement")]
        public static void ApplyMilestoneFour()
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
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "The development scene has no Player.",
                    "OK"
                );
                return;
            }

            SaveManager saveManager =
                UnityEngine.Object.FindFirstObjectByType<SaveManager>();

            if (saveManager == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Apply Milestone 3 Save System before Milestone 4.",
                    "OK"
                );
                return;
            }

            EnsureProjectFolders();

            PlaceableDefinition workbench =
                CreateWorkbenchDefinition();

            PlaceableDefinition shelf =
                CreateShelfDefinition();

            PlaceableDefinition cabinet =
                CreateCabinetDefinition();

            PlaceableDatabase database =
                GetOrCreateDatabase(
                    new[]
                    {
                        workbench,
                        shelf,
                        cabinet
                    }
                );

            PlacementController controller =
                player.GetComponent<PlacementController>();

            if (controller == null)
            {
                controller =
                    player.AddComponent<PlacementController>();
            }

            Camera playerCamera =
                player.GetComponentInChildren<Camera>(true);

            controller.Configure(
                playerCamera,
                database
            );

            GameObject services =
                GameObject.Find("RuntimeServices");

            if (services == null)
            {
                services =
                    new GameObject("RuntimeServices");
            }

            PlacementHUD hud =
                services.GetComponent<PlacementHUD>();

            if (hud == null)
            {
                hud =
                    services.AddComponent<PlacementHUD>();
            }

            hud.Configure(controller);
            saveManager.ConfigurePlacementDatabase(database);

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(saveManager);
            EditorUtility.SetDirty(database);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 4 placement applied.\n\n" +
                "B: Enter/exit build mode\n" +
                "1–3: Select furniture\n" +
                "R or Mouse Wheel: Rotate\n" +
                "Left Click: Place\n" +
                "Delete: Remove targeted placed object\n" +
                "Right Click: Exit build mode",
                "Done"
            );
        }

        private static PlaceableDefinition CreateWorkbenchDefinition()
        {
            Material material =
                GetOrCreateMaterial(
                    MaterialFolder + "/MAT_DevWorkbench.mat",
                    new Color(0.22f, 0.25f, 0.27f),
                    0.55f
                );

            GameObject prefab =
                GetOrCreatePrefab(
                    PrefabFolder + "/PREFAB_DevWorkbench.prefab",
                    "Dev Workbench",
                    root =>
                    {
                        CreatePart(
                            root,
                            "Top",
                            new Vector3(0f, 0.82f, 0f),
                            new Vector3(2.2f, 0.14f, 0.9f),
                            material
                        );

                        CreatePart(
                            root,
                            "Backboard",
                            new Vector3(0f, 1.42f, 0.38f),
                            new Vector3(2.2f, 1.05f, 0.08f),
                            material
                        );

                        CreatePart(
                            root,
                            "Leg_FL",
                            new Vector3(-0.92f, 0.4f, -0.33f),
                            new Vector3(0.14f, 0.8f, 0.14f),
                            material
                        );

                        CreatePart(
                            root,
                            "Leg_FR",
                            new Vector3(0.92f, 0.4f, -0.33f),
                            new Vector3(0.14f, 0.8f, 0.14f),
                            material
                        );

                        CreatePart(
                            root,
                            "Leg_BL",
                            new Vector3(-0.92f, 0.4f, 0.33f),
                            new Vector3(0.14f, 0.8f, 0.14f),
                            material
                        );

                        CreatePart(
                            root,
                            "Leg_BR",
                            new Vector3(0.92f, 0.4f, 0.33f),
                            new Vector3(0.14f, 0.8f, 0.14f),
                            material
                        );
                    }
                );

            return GetOrCreateDefinition(
                DataFolder + "/PLACEABLE_DevWorkbench.asset",
                "Workbench",
                "A sturdy starter workbench for future crafting and assembly.",
                prefab,
                new Vector3(0f, 0.96f, 0f),
                new Vector3(2.2f, 1.92f, 0.9f)
            );
        }

        private static PlaceableDefinition CreateShelfDefinition()
        {
            Material material =
                GetOrCreateMaterial(
                    MaterialFolder + "/MAT_DevShelf.mat",
                    new Color(0.16f, 0.19f, 0.21f),
                    0.65f
                );

            GameObject prefab =
                GetOrCreatePrefab(
                    PrefabFolder + "/PREFAB_DevShelf.prefab",
                    "Dev Storage Shelf",
                    root =>
                    {
                        for (int index = 0; index < 4; index++)
                        {
                            CreatePart(
                                root,
                                $"Shelf_{index + 1}",
                                new Vector3(
                                    0f,
                                    0.18f + index * 0.58f,
                                    0f
                                ),
                                new Vector3(1.8f, 0.1f, 0.55f),
                                material
                            );
                        }

                        CreatePart(
                            root,
                            "Post_Left",
                            new Vector3(-0.84f, 1.05f, 0f),
                            new Vector3(0.1f, 2.1f, 0.1f),
                            material
                        );

                        CreatePart(
                            root,
                            "Post_Right",
                            new Vector3(0.84f, 1.05f, 0f),
                            new Vector3(0.1f, 2.1f, 0.1f),
                            material
                        );
                    }
                );

            return GetOrCreateDefinition(
                DataFolder + "/PLACEABLE_DevStorageShelf.asset",
                "Storage Shelf",
                "A metal shelving unit prepared for visible inventory storage.",
                prefab,
                new Vector3(0f, 1.05f, 0f),
                new Vector3(1.8f, 2.1f, 0.55f)
            );
        }

        private static PlaceableDefinition CreateCabinetDefinition()
        {
            Material material =
                GetOrCreateMaterial(
                    MaterialFolder + "/MAT_DevCabinet.mat",
                    new Color(0.12f, 0.26f, 0.3f),
                    0.42f
                );

            GameObject prefab =
                GetOrCreatePrefab(
                    PrefabFolder + "/PREFAB_DevCabinet.prefab",
                    "Dev Utility Cabinet",
                    root =>
                    {
                        CreatePart(
                            root,
                            "Body",
                            new Vector3(0f, 0.75f, 0f),
                            new Vector3(1.15f, 1.5f, 0.55f),
                            material
                        );

                        CreatePart(
                            root,
                            "Door_Left",
                            new Vector3(-0.285f, 0.75f, -0.292f),
                            new Vector3(0.54f, 1.36f, 0.04f),
                            material
                        );

                        CreatePart(
                            root,
                            "Door_Right",
                            new Vector3(0.285f, 0.75f, -0.292f),
                            new Vector3(0.54f, 1.36f, 0.04f),
                            material
                        );
                    }
                );

            return GetOrCreateDefinition(
                DataFolder + "/PLACEABLE_DevUtilityCabinet.asset",
                "Utility Cabinet",
                "A lockable cabinet foundation for tools and restricted supplies.",
                prefab,
                new Vector3(0f, 0.75f, 0f),
                new Vector3(1.15f, 1.5f, 0.55f)
            );
        }

        private static PlaceableDefinition GetOrCreateDefinition(
            string path,
            string displayName,
            string description,
            GameObject prefab,
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

            SerializedProperty idProperty =
                serialized.FindProperty("placeableId");

            if (string.IsNullOrWhiteSpace(idProperty.stringValue))
            {
                idProperty.stringValue =
                    Guid.NewGuid().ToString("N");
            }

            serialized.FindProperty("displayName").stringValue =
                displayName;

            serialized.FindProperty("description").stringValue =
                description;

            serialized.FindProperty("prefab").objectReferenceValue =
                prefab;

            serialized.FindProperty("boundsCenter").vector3Value =
                boundsCenter;

            serialized.FindProperty("boundsSize").vector3Value =
                boundsSize;

            serialized.FindProperty("gridSize").floatValue =
                0.25f;

            serialized.FindProperty("rotationStep").floatValue =
                15f;

            serialized.FindProperty("maximumSlope").floatValue =
                20f;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            definition.EnsurePersistentId();
            EditorUtility.SetDirty(definition);

            return definition;
        }

        private static PlaceableDatabase GetOrCreateDatabase(
            IEnumerable<PlaceableDefinition> definitions
        )
        {
            PlaceableDatabase database =
                AssetDatabase.LoadAssetAtPath<PlaceableDatabase>(
                    DatabasePath
                );

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<PlaceableDatabase>();

                AssetDatabase.CreateAsset(
                    database,
                    DatabasePath
                );
            }

            database.SetDefinitions(
                definitions
                    .Where(definition => definition != null)
                    .OrderBy(definition => definition.DisplayName)
            );

            EditorUtility.SetDirty(database);
            return database;
        }

        private static GameObject GetOrCreatePrefab(
            string path,
            string objectName,
            Action<Transform> builder
        )
        {
            GameObject existing =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (existing != null)
            {
                return existing;
            }

            GameObject root =
                new GameObject(objectName);

            builder?.Invoke(root.transform);

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreatePart(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Material material
        )
        {
            GameObject part =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            part.name = objectName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation =
                Quaternion.identity;
            part.transform.localScale = localScale;

            Renderer renderer =
                part.GetComponent<Renderer>();

            renderer.sharedMaterial = material;
        }

        private static Material GetOrCreateMaterial(
            string path,
            Color baseColor,
            float metallic
        )
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(path);

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
                    baseColor
                );
            }
            else
            {
                material.color = baseColor;
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
                    0.3f
                );
            }

            AssetDatabase.CreateAsset(
                material,
                path
            );

            return material;
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
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Create the development playground first.",
                    "OK"
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
                    "Assets/_Project/Data/Placeables",
                    DataFolder,
                    DatabaseFolder,
                    "Assets/_Project/Prefabs",
                    "Assets/_Project/Prefabs/Placeables",
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
    }
}
