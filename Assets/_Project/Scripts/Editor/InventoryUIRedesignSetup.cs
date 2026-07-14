using System.IO;
using ShadowSupply.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ShadowSupply.Editor
{
    /// <summary>
    /// Adds and configures the UI Toolkit document used by the redesigned
    /// inventory interface without rebuilding the gameplay scene.
    /// </summary>
    public static class InventoryUIRedesignSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        private const string PanelFolder =
            "Assets/_Project/UI/Inventory";

        private const string PanelSettingsPath =
            PanelFolder + "/InventoryPanelSettings.asset";

        [MenuItem(
            "Shadow Supply/Setup/Apply Inventory UI Redesign"
        )]
        public static void ApplyInventoryUIRedesign()
        {
            Scene scene = EnsureDevelopmentSceneOpen();

            if (!scene.IsValid())
            {
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/UI");
            EnsureFolder(PanelFolder);

            GameObject runtimeServices =
                GameObject.Find("RuntimeServices");

            if (runtimeServices == null)
            {
                runtimeServices =
                    new GameObject("RuntimeServices");
            }

            InventoryHUD inventoryHud =
                runtimeServices.GetComponent<InventoryHUD>();

            if (inventoryHud == null)
            {
                inventoryHud =
                    runtimeServices.AddComponent<InventoryHUD>();
            }

            UIDocument uiDocument =
                runtimeServices.GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                uiDocument =
                    runtimeServices.AddComponent<UIDocument>();
            }

            PanelSettings panelSettings =
                GetOrCreatePanelSettings();

            uiDocument.panelSettings = panelSettings;

            EditorUtility.SetDirty(runtimeServices);
            EditorUtility.SetDirty(inventoryHud);
            EditorUtility.SetDirty(uiDocument);
            EditorUtility.SetDirty(panelSettings);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = runtimeServices;

            EditorUtility.DisplayDialog(
                "Shadow Supply Inventory UI",
                "The redesigned inventory interface is installed.\n\n" +
                "TAB / I: Open inventory\n" +
                "Click: Select item\n" +
                "E: Use / equip\n" +
                "Q: Drop one\n" +
                "R: Split stack\n" +
                "Escape: Close inventory or inspection\n\n" +
                "Press Play to test it.",
                "Done"
            );
        }

        private static Scene EnsureDevelopmentSceneOpen()
        {
            Scene current =
                SceneManager.GetActiveScene();

            if (
                current.IsValid() &&
                current.path == ScenePath
            )
            {
                return current;
            }

            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ScenePath
                );

            if (sceneAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Dev_Playground was not found. " +
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

        private static PanelSettings GetOrCreatePanelSettings()
        {
            PanelSettings settings =
                AssetDatabase.LoadAssetAtPath<PanelSettings>(
                    PanelSettingsPath
                );

            if (settings == null)
            {
                settings =
                    ScriptableObject.CreateInstance<PanelSettings>();

                settings.name =
                    "InventoryPanelSettings";

                AssetDatabase.CreateAsset(
                    settings,
                    PanelSettingsPath
                );
            }

            settings.scaleMode =
                PanelScaleMode.ScaleWithScreenSize;

            settings.referenceResolution =
                new Vector2Int(1920, 1080);

            settings.screenMatchMode =
                PanelScreenMatchMode.MatchWidthOrHeight;

            settings.match = 0.5f;
            settings.sortingOrder = 200;

            return settings;
        }

        private static void EnsureFolder(string fullPath)
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
