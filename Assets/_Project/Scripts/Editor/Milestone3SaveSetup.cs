using System.Collections.Generic;
using System.Linq;
using ShadowSupply.Inventory;
using ShadowSupply.SaveSystem;
using ShadowSupply.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone3SaveSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        private const string DatabaseFolder =
            "Assets/_Project/Data/Databases";

        private const string DatabasePath =
            DatabaseFolder + "/DB_Items.asset";

        [MenuItem("Shadow Supply/Setup/Apply Milestone 3 Save System")]
        public static void ApplyMilestoneThree()
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
                    "The playground does not contain a Player object.",
                    "OK"
                );
                return;
            }

            PlayerInventory inventory =
                player.GetComponent<PlayerInventory>();

            HotbarController hotbar =
                player.GetComponent<HotbarController>();

            if (inventory == null || hotbar == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Apply Milestone 2 Inventory before Milestone 3.",
                    "OK"
                );
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Data");
            EnsureFolder(DatabaseFolder);

            ItemDatabase database =
                GetOrCreateItemDatabase();

            GameObject services =
                GameObject.Find("RuntimeServices");

            if (services == null)
            {
                services =
                    new GameObject("RuntimeServices");
            }

            SaveManager saveManager =
                services.GetComponent<SaveManager>();

            if (saveManager == null)
            {
                saveManager =
                    services.AddComponent<SaveManager>();
            }

            saveManager.Configure(
                database,
                player.transform,
                inventory,
                hotbar
            );

            SaveSystemHUD hud =
                services.GetComponent<SaveSystemHUD>();

            if (hud == null)
            {
                hud =
                    services.AddComponent<SaveSystemHUD>();
            }

            hud.Configure(saveManager);

            EditorUtility.SetDirty(saveManager);
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(database);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = services;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 3 save system applied.\n\n" +
                "F1–F3: Select save slot\n" +
                "F5: Save\n" +
                "F9: Load",
                "Done"
            );
        }

        private static Scene OpenDevelopmentScene()
        {
            Scene active =
                SceneManager.GetActiveScene();

            if (active.IsValid() && active.path == ScenePath)
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
                    "Create the Milestone 1 playground first.",
                    "OK"
                );

                return default;
            }

            return EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single
            );
        }

        private static ItemDatabase GetOrCreateItemDatabase()
        {
            ItemDatabase database =
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(
                    DatabasePath
                );

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<ItemDatabase>();

                AssetDatabase.CreateAsset(
                    database,
                    DatabasePath
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

            List<ItemDefinition> definitions =
                guids
                    .Select(
                        AssetDatabase.GUIDToAssetPath
                    )
                    .Select(
                        path =>
                            AssetDatabase.LoadAssetAtPath<ItemDefinition>(
                                path
                            )
                    )
                    .Where(
                        definition =>
                            definition != null
                    )
                    .ToList();

            database.SetItems(definitions);
            return database;
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
                System.IO.Path
                    .GetDirectoryName(fullPath)
                    ?.Replace("\\", "/");

            string folderName =
                System.IO.Path
                    .GetFileName(fullPath);

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
