using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Electrical;
using ShadowSupply.Production;
using ShadowSupply.Properties;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class
        Milestone8InteractionLightingHotfixSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/" +
            "Dev_Playground.unity";

        private const string GarageRootName =
            "StarterGarage_Property";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/Lighting";

        private const string DiffuserMaterialPath =
            MaterialFolder +
            "/MAT_Garage_FluorescentDiffuser.mat";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply v0.8.5 Workbench Target + Lighting Hotfix"
        )]
        public static void ApplyHotfix()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );

            GameObject garage =
                GameObject.Find(GarageRootName);

            PoweredWorkbenchProduction workbench =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >();

            StarterGarageLightSwitch lightSwitch =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        StarterGarageLightSwitch
                    >();

            ElectricalPanel panel =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ElectricalPanel
                    >();

            ElectricalLightLoad lightLoad =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ElectricalLightLoad
                    >();

            if (
                !scene.IsValid() ||
                garage == null ||
                workbench == null ||
                lightSwitch == null ||
                panel == null
            )
            {
                ShowError(
                    "The confirmed starter garage, powered workbench, " +
                    "light switch, or electrical panel was not found."
                );

                return;
            }

            EnsureFolder(MaterialFolder);

            Material diffuserMaterial =
                GetOrCreateDiffuserMaterial();

            workbench.EnsureInteractionTarget();

            List<Light> rebuiltLights =
                new List<Light>();

            Transform[] fixtures =
                garage.GetComponentsInChildren<
                    Transform
                >(true)
                .Where(
                    target =>
                        target != null &&
                        target.name.StartsWith(
                            "Fluorescent Fixture",
                            StringComparison.Ordinal
                        )
                )
                .OrderBy(
                    target => target.name
                )
                .ToArray();

            foreach (Transform fixture in fixtures)
            {
                RebuildFixture(
                    fixture,
                    diffuserMaterial,
                    rebuiltLights
                );
            }

            if (rebuiltLights.Count == 0)
            {
                ShowError(
                    "No fluorescent garage fixtures were found."
                );

                return;
            }

            lightSwitch.Configure(
                "Garage Lights",
                rebuiltLights.ToArray(),
                true
            );

            lightSwitch.ConfigurePower(
                panel,
                "garage-lighting"
            );

            if (lightLoad != null)
            {
                lightLoad.Configure(
                    panel,
                    "garage-lighting",
                    240,
                    rebuiltLights.ToArray()
                );
            }

            EditorUtility.SetDirty(workbench);
            EditorUtility.SetDirty(lightSwitch);
            EditorUtility.SetDirty(panel);

            if (lightLoad != null)
            {
                EditorUtility.SetDirty(lightLoad);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                workbench.gameObject;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "v0.8.5 applied.\n\n" +
                "• A broad trigger target now covers the usable " +
                "workbench surface.\n" +
                "• Garage lights reapply their electrical state " +
                "during startup.\n" +
                "• Each fluorescent fixture now has three downward " +
                "light sources positioned directly beneath its diffuser.\n" +
                "• Diffusers glow only while their actual lights are on.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate v0.8.5 Workbench Target + Lighting"
        )]
        public static void ValidateHotfix()
        {
            PoweredWorkbenchProduction workbench =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >();

            Transform interactionTarget =
                workbench != null
                    ? workbench.transform.Find(
                        "Production Interaction Target"
                    )
                    : null;

            StarterGarageFluorescentFixture[] fixtures =
                UnityEngine.Object
                    .FindObjectsByType<
                        StarterGarageFluorescentFixture
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            Light[] fixtureLights =
                UnityEngine.Object
                    .FindObjectsByType<Light>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    )
                    .Where(
                        light =>
                            light != null &&
                            light.name.StartsWith(
                                "Fixture Light",
                                StringComparison.Ordinal
                            )
                    )
                    .ToArray();

            bool valid =
                workbench != null &&
                interactionTarget != null &&
                interactionTarget
                    .GetComponent<BoxCollider>() != null &&
                fixtures.Length == 4 &&
                fixtureLights.Length == 12;

            string report =
                "Workbench target: " +
                (
                    interactionTarget != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nTarget collider: " +
                (
                    interactionTarget != null &&
                    interactionTarget
                        .GetComponent<BoxCollider>() != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nFluorescent fixtures: " +
                fixtures.Length +
                " / 4" +
                "\nFixture light sources: " +
                fixtureLights.Length +
                " / 12";

            if (valid)
            {
                Debug.Log(
                    "[v0.8.5] WORKBENCH TARGET + " +
                    "LIGHTING READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "v0.8.5 Validation",
                    "WORKBENCH TARGET + LIGHTING READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[v0.8.5] VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "v0.8.5 Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static void RebuildFixture(
            Transform fixture,
            Material diffuserMaterial,
            List<Light> rebuiltLights
        )
        {
            List<GameObject> oldSources =
                new List<GameObject>();

            for (
                int index = 0;
                index < fixture.childCount;
                index++
            )
            {
                Transform child =
                    fixture.GetChild(index);

                if (
                    child.name.StartsWith(
                        "Fluorescent Light Source",
                        StringComparison.Ordinal
                    ) ||
                    child.name.StartsWith(
                        "Fixture Light",
                        StringComparison.Ordinal
                    )
                )
                {
                    oldSources.Add(child.gameObject);
                }
            }

            foreach (GameObject oldSource in oldSources)
            {
                UnityEngine.Object.DestroyImmediate(
                    oldSource
                );
            }

            Renderer diffuser =
                FindChildRecursive(
                    fixture,
                    "Diffuser"
                )?.GetComponent<Renderer>();

            if (diffuser != null)
            {
                diffuser.sharedMaterial =
                    diffuserMaterial;

                EditorUtility.SetDirty(diffuser);
            }

            List<Light> fixtureLights =
                new List<Light>();

            float[] positions =
            {
                -0.43f,
                0f,
                0.43f
            };

            for (
                int index = 0;
                index < positions.Length;
                index++
            )
            {
                GameObject source =
                    new GameObject(
                        $"Fixture Light {index + 1}"
                    );

                source.transform.SetParent(
                    fixture,
                    false
                );

                source.transform.localPosition =
                    new Vector3(
                        positions[index],
                        -0.105f,
                        0f
                    );

                source.transform.localRotation =
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f
                    );

                Light light =
                    source.AddComponent<Light>();

                light.type = LightType.Spot;
                light.range = 6.4f;
                light.spotAngle = 108f;
                light.innerSpotAngle = 76f;
                light.intensity = 2.5f;
                light.color =
                    new Color(
                        1f,
                        0.94f,
                        0.78f
                    );

                light.shadows =
                    LightShadows.Soft;

                fixtureLights.Add(light);
                rebuiltLights.Add(light);
            }

            StarterGarageFluorescentFixture controller =
                fixture.GetComponent<
                    StarterGarageFluorescentFixture
                >();

            if (controller == null)
            {
                controller =
                    fixture.gameObject.AddComponent<
                        StarterGarageFluorescentFixture
                    >();
            }

            controller.Configure(
                diffuser,
                fixtureLights.ToArray()
            );

            EditorUtility.SetDirty(controller);
        }

        private static Material
            GetOrCreateDiffuserMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(DiffuserMaterialPath);

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
                            "MAT_Garage_FluorescentDiffuser"
                    };

                AssetDatabase.CreateAsset(
                    material,
                    DiffuserMaterialPath
                );
            }

            Color baseColor =
                new Color(
                    0.86f,
                    0.84f,
                    0.72f
                );

            Color emission =
                new Color(
                    2.8f,
                    2.55f,
                    1.85f
                );

            if (
                material.HasProperty(
                    "_BaseColor"
                )
            )
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

            if (
                material.HasProperty(
                    "_EmissionColor"
                )
            )
            {
                material.SetColor(
                    "_EmissionColor",
                    emission
                );

                material.EnableKeyword(
                    "_EMISSION"
                );
            }

            if (
                material.HasProperty(
                    "_Smoothness"
                )
            )
            {
                material.SetFloat(
                    "_Smoothness",
                    0.28f
                );
            }

            EditorUtility.SetDirty(material);
            return material;
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
                "[v0.8.5] " + message
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }
    }
}
