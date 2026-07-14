using System.Collections.Generic;
using ShadowSupply.Core;
using ShadowSupply.Interaction;
using ShadowSupply.Player;
using ShadowSupply.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class ShadowSupplyProjectSetup
    {
        private const string SceneFolder = "Assets/_Project/Scenes/Development";
        private const string MaterialFolder = "Assets/_Project/Art/Materials/Development";
        private const string ScenePath = SceneFolder + "/Dev_Playground.unity";

        [MenuItem("Shadow Supply/Setup/Create Milestone 1 Playground")]
        public static void CreateMilestoneOnePlayground()
        {
            if (!EditorUtility.DisplayDialog(
                    "Create Milestone 1 Playground",
                    "This will create or replace the development playground scene.",
                    "Create",
                    "Cancel"))
            {
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Scenes");
            EnsureFolder(SceneFolder);
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material groundMaterial = GetOrCreateMaterial(
                MaterialFolder + "/MAT_DevGround.mat",
                new Color(0.11f, 0.12f, 0.13f)
            );

            Material wallMaterial = GetOrCreateMaterial(
                MaterialFolder + "/MAT_DevWall.mat",
                new Color(0.19f, 0.2f, 0.21f)
            );

            Material interactableMaterial = GetOrCreateMaterial(
                MaterialFolder + "/MAT_DevInteractable.mat",
                new Color(0.18f, 0.32f, 0.42f)
            );

            CreateEnvironment(groundMaterial, wallMaterial);
            GameObject player = CreatePlayer();
            CreateRuntimeServices();
            CreateTestInteractable(interactableMaterial);
            CreateLighting();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Selection.activeGameObject = player;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 1 playground created.\n\n" +
                "Open Dev_Playground and press Play.\n" +
                "WASD: Move\nShift: Sprint\nCtrl/C: Crouch\n" +
                "Space: Jump\nE: Interact\nEscape: Unlock cursor",
                "Done"
            );
        }

        private static void CreateEnvironment(Material groundMaterial, Material wallMaterial)
        {
            GameObject environmentRoot = new GameObject("Environment");

            CreateCube("Ground", environmentRoot.transform, new Vector3(0f, -0.25f, 0f), new Vector3(30f, 0.5f, 30f), groundMaterial);
            CreateCube("Wall_North", environmentRoot.transform, new Vector3(0f, 2f, 15f), new Vector3(30f, 4f, 0.5f), wallMaterial);
            CreateCube("Wall_South", environmentRoot.transform, new Vector3(0f, 2f, -15f), new Vector3(30f, 4f, 0.5f), wallMaterial);
            CreateCube("Wall_East", environmentRoot.transform, new Vector3(15f, 2f, 0f), new Vector3(0.5f, 4f, 30f), wallMaterial);
            CreateCube("Wall_West", environmentRoot.transform, new Vector3(-15f, 2f, 0f), new Vector3(0.5f, 4f, 30f), wallMaterial);
            CreateCube("Platform", environmentRoot.transform, new Vector3(5f, 0.35f, 5f), new Vector3(4f, 0.7f, 4f), wallMaterial);
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0.05f, -4f);
            player.tag = "Player";

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 50f;

            GameObject cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform);
            cameraObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 75f;
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();

            player.AddComponent<FirstPersonController>();
            player.AddComponent<PlayerInteractor>();
            return player;
        }

        private static void CreateRuntimeServices()
        {
            GameObject services = new GameObject("RuntimeServices");
            services.AddComponent<GameBootstrap>();
            services.AddComponent<InteractionPromptHUD>();
        }

        private static void CreateTestInteractable(Material material)
        {
            GameObject testObject = CreateCube(
                "Test_Interactable",
                null,
                new Vector3(0f, 0.75f, 3f),
                new Vector3(1.5f, 1.5f, 1.5f),
                material
            );

            testObject.AddComponent<TestInteractable>();
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new GameObject("Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.38f, 0.43f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.19f, 0.2f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.07f, 0.075f);
        }

        private static GameObject CreateCube(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static Material GetOrCreateMaterial(string path, Color baseColor)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path)
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }
            else
            {
                material.color = baseColor;
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.2f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool alreadyIncluded = scenes.Exists(scene => scene.path == scenePath);

            if (!alreadyIncluded)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private static void EnsureFolder(string fullPath)
        {
            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(fullPath)?.Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(fullPath);

            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
