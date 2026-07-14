using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Production;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone8BInteractiveProductionSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/" +
            "Dev_Playground.unity";

        private const string GarageRootName =
            "StarterGarage_Property";

        private const string AssemblyRootName =
            "Interactive Production Surface";

        private const string MaterialFolder =
            "Assets/_Project/Art/Materials/Production";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 8B Interactive Production Upgrade"
        )]
        public static void ApplyInteractiveProduction()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );

            PoweredWorkbenchProduction production =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >();

            ProductionWorkbenchHUD hud =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ProductionWorkbenchHUD
                    >();

            GameObject garage =
                GameObject.Find(GarageRootName);

            if (
                !scene.IsValid() ||
                production == null ||
                hud == null ||
                garage == null
            )
            {
                ShowError(
                    "The confirmed Milestone 8B production " +
                    "workbench was not found."
                );

                return;
            }

            EnsureFolder(MaterialFolder);

            Transform workbench =
                production.transform;

            RemoveExistingAssemblySurface(
                workbench
            );

            LocalBounds localBounds =
                CalculateLocalWorkbenchBounds(
                    workbench
                );

            bool rotateSurface =
                localBounds.size.z >
                localBounds.size.x;

            float longDimension =
                rotateSurface
                    ? localBounds.size.z
                    : localBounds.size.x;

            float shortDimension =
                rotateSurface
                    ? localBounds.size.x
                    : localBounds.size.z;

            float surfaceWidth =
                Mathf.Clamp(
                    longDimension - 0.12f,
                    1.35f,
                    2.15f
                );

            float surfaceDepth =
                Mathf.Clamp(
                    shortDimension - 0.08f,
                    0.56f,
                    0.9f
                );

            GameObject assemblyRoot =
                new GameObject(
                    AssemblyRootName
                );

            assemblyRoot.transform.SetParent(
                workbench,
                false
            );

            assemblyRoot.transform.localPosition =
                new Vector3(
                    localBounds.center.x,
                    localBounds.max.y + 0.025f,
                    localBounds.center.z
                );

            assemblyRoot.transform.localRotation =
                rotateSurface
                    ? Quaternion.Euler(
                        0f,
                        90f,
                        0f
                    )
                    : Quaternion.identity;

            GameObject surface =
                new GameObject(
                    "Assembly Surface"
                );

            surface.transform.SetParent(
                assemblyRoot.transform,
                false
            );

            GameObject partsRoot =
                new GameObject(
                    "Spawned Assembly Parts"
                );

            partsRoot.transform.SetParent(
                assemblyRoot.transform,
                false
            );

            GameObject packageRoot =
                BuildOpenPackage(
                    assemblyRoot.transform,
                    surfaceWidth,
                    out Transform packageInterior,
                    out BoxCollider dropZone,
                    out Transform lidPivot,
                    out Collider lidCollider,
                    out Renderer lidRenderer
                );

            Camera assemblyCamera =
                BuildAssemblyCamera(
                    assemblyRoot.transform,
                    surfaceWidth,
                    surfaceDepth
                );

            InteractiveAssemblyController controller =
                assemblyRoot.AddComponent<
                    InteractiveAssemblyController
                >();

            controller.Configure(
                production,
                assemblyCamera,
                surface.transform,
                new Vector2(
                    surfaceWidth,
                    surfaceDepth
                ),
                partsRoot.transform,
                packageRoot.transform,
                packageInterior,
                dropZone,
                lidPivot,
                lidCollider,
                lidRenderer
            );

            GameObject outputVisual =
                FindChildRecursive(
                    workbench,
                    "Ready Sealed Package"
                )?.gameObject;

            production.Configure(
                production.WorkbenchId,
                production.Inventory,
                production.PowerConnection,
                production.RecipeDatabase,
                outputVisual,
                controller
            );

            Transform oldTray =
                FindChildRecursive(
                    workbench,
                    "Output Tray"
                );

            if (oldTray != null)
            {
                oldTray.gameObject.SetActive(false);
            }

            packageRoot.SetActive(false);
            partsRoot.SetActive(false);
            assemblyCamera.enabled = false;

            EditorUtility.SetDirty(
                production
            );

            EditorUtility.SetDirty(
                controller
            );

            EditorUtility.SetDirty(
                hud
            );

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                assemblyRoot;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "The production system is now manual and " +
                "interactive.\n\n" +
                "Consumed ingredients spawn as draggable 3D parts. " +
                "Reusable tools must be clicked. After every step is " +
                "complete, the player must click the package lid to " +
                "close and seal the output.\n\n" +
                "This framework is recipe-driven and becomes the " +
                "required production path for future recipes.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 8B Interactive Production"
        )]
        public static void ValidateInteractiveProduction()
        {
            PoweredWorkbenchProduction production =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PoweredWorkbenchProduction
                    >();

            InteractiveAssemblyController controller =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        InteractiveAssemblyController
                    >();

            ProductionWorkbenchHUD hud =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ProductionWorkbenchHUD
                    >();

            bool valid =
                production != null &&
                production.AssemblyController != null &&
                controller != null &&
                controller.AssemblyCamera != null &&
                hud != null &&
                SaveManager.CurrentSaveVersion == 7;

            string report =
                "Production workbench: " +
                (
                    production != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nInteractive controller: " +
                (
                    controller != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nAssembly camera: " +
                (
                    controller != null &&
                    controller.AssemblyCamera != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nProduction HUD: " +
                (
                    hud != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nSave schema: " +
                SaveManager.CurrentSaveVersion +
                " / 7";

            if (valid)
            {
                Debug.Log(
                    "[Milestone8B] INTERACTIVE PRODUCTION READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Interactive Production Validation",
                    "INTERACTIVE PRODUCTION READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone8B] INTERACTIVE PRODUCTION " +
                    "VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Interactive Production Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static GameObject BuildOpenPackage(
            Transform parent,
            float surfaceWidth,
            out Transform packageInterior,
            out BoxCollider dropZone,
            out Transform lidPivot,
            out Collider lidCollider,
            out Renderer lidRenderer
        )
        {
            Material packageMaterial =
                CreateMaterial(
                    "MAT_InteractivePackage",
                    new Color(
                        0.19f,
                        0.14f,
                        0.09f
                    ),
                    0.1f,
                    0f
                );

            Material edgeMaterial =
                CreateMaterial(
                    "MAT_InteractivePackageEdge",
                    new Color(
                        0.075f,
                        0.06f,
                        0.045f
                    ),
                    0.12f,
                    0f
                );

            GameObject root =
                new GameObject(
                    "Interactive Open Package"
                );

            root.transform.SetParent(
                parent,
                false
            );

            root.transform.localPosition =
                new Vector3(
                    surfaceWidth * 0.27f,
                    0.025f,
                    0f
                );

            CreateLocalCube(
                root.transform,
                "Package Base",
                new Vector3(
                    0f,
                    0.018f,
                    0f
                ),
                new Vector3(
                    0.56f,
                    0.035f,
                    0.38f
                ),
                packageMaterial,
                false
            );

            CreateLocalCube(
                root.transform,
                "Package Left Wall",
                new Vector3(
                    -0.275f,
                    0.095f,
                    0f
                ),
                new Vector3(
                    0.03f,
                    0.18f,
                    0.38f
                ),
                edgeMaterial,
                false
            );

            CreateLocalCube(
                root.transform,
                "Package Right Wall",
                new Vector3(
                    0.275f,
                    0.095f,
                    0f
                ),
                new Vector3(
                    0.03f,
                    0.18f,
                    0.38f
                ),
                edgeMaterial,
                false
            );

            CreateLocalCube(
                root.transform,
                "Package Front Wall",
                new Vector3(
                    0f,
                    0.095f,
                    -0.185f
                ),
                new Vector3(
                    0.56f,
                    0.18f,
                    0.03f
                ),
                edgeMaterial,
                false
            );

            CreateLocalCube(
                root.transform,
                "Package Rear Wall",
                new Vector3(
                    0f,
                    0.095f,
                    0.185f
                ),
                new Vector3(
                    0.56f,
                    0.18f,
                    0.03f
                ),
                edgeMaterial,
                false
            );

            GameObject interior =
                new GameObject(
                    "Package Interior"
                );

            interior.transform.SetParent(
                root.transform,
                false
            );

            packageInterior =
                interior.transform;

            GameObject zone =
                new GameObject(
                    "Package Drop Zone"
                );

            zone.transform.SetParent(
                root.transform,
                false
            );

            zone.transform.localPosition =
                new Vector3(
                    0f,
                    0.12f,
                    0f
                );

            dropZone =
                zone.AddComponent<BoxCollider>();

            dropZone.isTrigger = true;
            dropZone.size =
                new Vector3(
                    0.48f,
                    0.26f,
                    0.3f
                );

            GameObject pivot =
                new GameObject(
                    "Package Lid Pivot"
                );

            pivot.transform.SetParent(
                root.transform,
                false
            );

            pivot.transform.localPosition =
                new Vector3(
                    0f,
                    0.18f,
                    0.19f
                );

            lidPivot = pivot.transform;

            GameObject lid =
                CreateLocalCube(
                    lidPivot,
                    "Package Lid",
                    new Vector3(
                        0f,
                        0.015f,
                        -0.19f
                    ),
                    new Vector3(
                        0.57f,
                        0.035f,
                        0.38f
                    ),
                    packageMaterial,
                    true
                );

            lid.AddComponent<AssemblyLidHandle>();

            lidCollider =
                lid.GetComponent<Collider>();

            lidRenderer =
                lid.GetComponent<Renderer>();

            lidPivot.localRotation =
                Quaternion.Euler(
                    -108f,
                    0f,
                    0f
                );

            return root;
        }

        private static Camera BuildAssemblyCamera(
            Transform parent,
            float surfaceWidth,
            float surfaceDepth
        )
        {
            GameObject cameraObject =
                new GameObject(
                    "Interactive Assembly Camera"
                );

            cameraObject.transform.SetParent(
                parent,
                false
            );

            cameraObject.transform.localPosition =
                new Vector3(
                    0f,
                    1.55f,
                    0f
                );

            cameraObject.transform.localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );

            Camera camera =
                cameraObject.AddComponent<Camera>();

            camera.orthographic = true;
            camera.orthographicSize =
                Mathf.Max(
                    surfaceDepth * 0.75f,
                    surfaceWidth / 3.2f
                ) +
                0.12f;

            camera.nearClipPlane = 0.02f;
            camera.farClipPlane = 4f;
            camera.depth = 20f;
            camera.enabled = false;

            return camera;
        }

        private static LocalBounds
            CalculateLocalWorkbenchBounds(
                Transform workbench
            )
        {
            Renderer[] renderers =
                workbench.GetComponentsInChildren<
                    Renderer
                >(true);

            bool initialized = false;
            Bounds localBounds =
                new Bounds();

            foreach (Renderer renderer in renderers)
            {
                if (
                    renderer == null ||
                    renderer.name.IndexOf(
                        "Power Indicator",
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0 ||
                    renderer.name.IndexOf(
                        "Production Output",
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0 ||
                    HasAncestorNamed(
                        renderer.transform,
                        "Workbench Production Output"
                    ) ||
                    HasAncestorNamed(
                        renderer.transform,
                        AssemblyRootName
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
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z)
                };

                foreach (Vector3 corner in corners)
                {
                    Vector3 local =
                        workbench.InverseTransformPoint(
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
                        localBounds.Encapsulate(local);
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

            return new LocalBounds(
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

        private static void RemoveExistingAssemblySurface(
            Transform workbench
        )
        {
            Transform existing =
                FindChildRecursive(
                    workbench,
                    AssemblyRootName
                );

            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    existing.gameObject
                );
            }

            InteractiveAssemblyController[] controllers =
                workbench.GetComponentsInChildren<
                    InteractiveAssemblyController
                >(true);

            foreach (
                InteractiveAssemblyController controller
                in controllers
            )
            {
                if (controller != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        controller
                    );
                }
            }
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

            cube.transform.localScale =
                size;

            Renderer renderer =
                cube.GetComponent<Renderer>();

            renderer.sharedMaterial =
                material;

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

        private static void EnsureFolder(
            string path
        )
        {
            if (
                AssetDatabase.IsValidFolder(path)
            )
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            string name =
                Path.GetFileName(path);

            if (
                !string.IsNullOrWhiteSpace(parent)
            )
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                name
            );
        }

        private static void ShowError(
            string message
        )
        {
            Debug.LogError(
                "[InteractiveProduction] " +
                message
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
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
