using System;
using System.Collections.Generic;
using System.IO;
using ShadowSupply.Electrical;
using ShadowSupply.Player;
using ShadowSupply.Properties;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace ShadowSupply.Editor
{
    public static class Milestone8PhysicalGaragePowerSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/" +
            "Dev_Playground.unity";

        private const string GarageRootName =
            "StarterGarage_Property";

        private const string ElectricalRootName =
            "Milestone8_PhysicalElectricalSystem";

        private const string MaterialRoot =
            "Assets/_Project/Art/Electrical/Materials";

        private const string LightingCircuitId =
            "garage-lighting";

        private const string WorkCircuitId =
            "garage-work";

        private const string OfficeCircuitId =
            "garage-office";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 8A Physical Garage Power"
        )]
        public static void ApplyMilestoneEightA()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );

            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Dev_Playground could not be opened.",
                    "OK"
                );

                return;
            }

            GameObject garage =
                GameObject.Find(GarageRootName);

            FirstPersonController player =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        FirstPersonController
                    >();

            if (
                garage == null ||
                player == null
            )
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Milestone 8A requires the confirmed " +
                    "starter garage and player controller.",
                    "OK"
                );

                return;
            }

            EnsureFolders();
            RemovePreviousElectricalSetup(
                garage,
                player
            );

            ElectricalMaterials materials =
                CreateMaterials();

            GameObject electricalRoot =
                new GameObject(ElectricalRootName);

            electricalRoot.transform.SetParent(
                garage.transform,
                false
            );

            ElectricalGridSystem grid =
                electricalRoot.AddComponent<
                    ElectricalGridSystem
                >();

            PlayerPlugController plugController =
                player.gameObject.AddComponent<
                    PlayerPlugController
                >();

            plugController.Configure(
                player.GetComponentInChildren<
                    Camera
                >(true)
            );

            ElectricalPanel panel =
                ConfigurePanel(
                    garage,
                    materials
                );

            List<PowerOutlet> outlets =
                ConfigureOutlets(
                    garage,
                    panel,
                    materials
                );

            MachinePowerConnection workbench =
                ConfigureWorkbenchPower(
                    garage,
                    electricalRoot,
                    materials
                );

            ConfigureLightingCircuit(
                garage,
                electricalRoot,
                panel
            );

            ElectricalStatusHUD hud =
                electricalRoot.AddComponent<
                    ElectricalStatusHUD
                >();

            hud.Configure(
                panel,
                workbench
            );

            grid.ForceRecalculate();

            EditorUtility.SetDirty(
                electricalRoot
            );

            EditorUtility.SetDirty(
                player
            );

            EditorUtility.SetDirty(
                panel
            );

            foreach (PowerOutlet outlet in outlets)
            {
                EditorUtility.SetDirty(outlet);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                electricalRoot;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 8A physical garage power applied.\n\n" +
                "Test flow:\n" +
                "1. Look at the workbench plug and press E.\n" +
                "2. Carry it to either socket on a wall outlet.\n" +
                "3. Look at a socket and press E to connect.\n" +
                "4. Press R while carrying a plug to release it.\n" +
                "5. Use the panel breakers to cut or restore power.\n\n" +
                "The cable is visually simulated with controlled " +
                "sag and a maximum reach.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 8A Physical Garage Power"
        )]
        public static void ValidateMilestoneEightA()
        {
            ElectricalPanel panel =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ElectricalPanel
                    >();

            PowerOutlet[] outlets =
                UnityEngine.Object
                    .FindObjectsByType<PowerOutlet>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            OutletSocket[] sockets =
                UnityEngine.Object
                    .FindObjectsByType<OutletSocket>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            PowerPlug[] plugs =
                UnityEngine.Object
                    .FindObjectsByType<PowerPlug>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            MachinePowerConnection[] machines =
                UnityEngine.Object
                    .FindObjectsByType<
                        MachinePowerConnection
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            PlayerPlugController playerController =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PlayerPlugController
                    >();

            ElectricalGridSystem grid =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        ElectricalGridSystem
                    >();

            bool valid =
                panel != null &&
                outlets.Length == 6 &&
                sockets.Length == 12 &&
                plugs.Length == 1 &&
                machines.Length == 1 &&
                playerController != null &&
                grid != null &&
                SaveManager.CurrentSaveVersion == 5;

            string report =
                "Panel: " +
                (panel != null ? "OK" : "MISSING") +
                "\nOutlets: " +
                outlets.Length +
                " / 6" +
                "\nSockets: " +
                sockets.Length +
                " / 12" +
                "\nPlugs: " +
                plugs.Length +
                " / 1" +
                "\nPowered machines: " +
                machines.Length +
                " / 1" +
                "\nPlayer plug controller: " +
                (playerController != null ? "OK" : "MISSING") +
                "\nGrid system: " +
                (grid != null ? "OK" : "MISSING") +
                "\nSave schema: " +
                SaveManager.CurrentSaveVersion +
                " / 5";

            if (valid)
            {
                Debug.Log(
                    "[Milestone8] PHYSICAL GARAGE POWER READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8A Validation",
                    "PHYSICAL GARAGE POWER READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone8] PHYSICAL GARAGE POWER " +
                    "VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8A Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static ElectricalPanel ConfigurePanel(
            GameObject garage,
            ElectricalMaterials materials
        )
        {
            Transform panelTransform =
                FindChildRecursive(
                    garage.transform,
                    "Main Breaker Panel"
                );

            if (panelTransform == null)
            {
                throw new InvalidOperationException(
                    "Main Breaker Panel was not found."
                );
            }

            ElectricalPanel panel =
                panelTransform.gameObject
                    .AddComponent<ElectricalPanel>();

            panel.Configure(
                "starter-garage-panel",
                3200,
                new[]
                {
                    new ElectricalCircuit(
                        LightingCircuitId,
                        "Lighting",
                        600,
                        true
                    ),
                    new ElectricalCircuit(
                        WorkCircuitId,
                        "Work Outlets",
                        1600,
                        true
                    ),
                    new ElectricalCircuit(
                        OfficeCircuitId,
                        "Office Outlets",
                        1000,
                        true
                    )
                }
            );

            GameObject controls =
                new GameObject(
                    "Breaker Controls"
                );

            controls.transform.SetParent(
                panelTransform,
                false
            );

            controls.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -0.16f
                );

            CreateLocalCube(
                controls.transform,
                "Breaker Backplate",
                Vector3.zero,
                new Vector3(
                    0.46f,
                    0.92f,
                    0.045f
                ),
                materials.breakerPlate,
                false
            );

            CreateBreakerSwitch(
                controls.transform,
                panel,
                true,
                string.Empty,
                "Main Breaker",
                new Vector3(0f, 0.3f, -0.05f),
                materials
            );

            CreateBreakerSwitch(
                controls.transform,
                panel,
                false,
                WorkCircuitId,
                "Work Breaker",
                new Vector3(0f, 0.1f, -0.05f),
                materials
            );

            CreateBreakerSwitch(
                controls.transform,
                panel,
                false,
                OfficeCircuitId,
                "Office Breaker",
                new Vector3(0f, -0.1f, -0.05f),
                materials
            );

            CreateBreakerSwitch(
                controls.transform,
                panel,
                false,
                LightingCircuitId,
                "Lighting Breaker",
                new Vector3(0f, -0.3f, -0.05f),
                materials
            );

            return panel;
        }

        private static void CreateBreakerSwitch(
            Transform parent,
            ElectricalPanel panel,
            bool controlsMain,
            string circuitId,
            string displayName,
            Vector3 localPosition,
            ElectricalMaterials materials
        )
        {
            GameObject root =
                new GameObject(displayName);

            root.transform.SetParent(
                parent,
                false
            );

            root.transform.localPosition =
                localPosition;

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center = Vector3.zero;
            collider.size =
                new Vector3(
                    0.36f,
                    0.16f,
                    0.12f
                );

            CreateLocalCube(
                root.transform,
                "Breaker Slot",
                Vector3.zero,
                new Vector3(
                    0.31f,
                    0.13f,
                    0.035f
                ),
                materials.breakerSlot,
                false
            );

            GameObject lever =
                CreateLocalCube(
                    root.transform,
                    "Lever",
                    new Vector3(
                        0f,
                        0f,
                        -0.045f
                    ),
                    new Vector3(
                        0.09f,
                        0.12f,
                        0.055f
                    ),
                    materials.breakerLever,
                    false
                );

            CircuitBreakerSwitch breaker =
                root.AddComponent<
                    CircuitBreakerSwitch
                >();

            breaker.Configure(
                panel,
                controlsMain,
                circuitId,
                displayName,
                lever.transform
            );
        }

        private static List<PowerOutlet>
            ConfigureOutlets(
                GameObject garage,
                ElectricalPanel panel,
                ElectricalMaterials materials
            )
        {
            List<PowerOutlet> configured =
                new List<PowerOutlet>();

            for (int index = 1; index <= 6; index++)
            {
                Transform outletTransform =
                    FindChildRecursive(
                        garage.transform,
                        $"Outlet {index}"
                    );

                if (outletTransform == null)
                {
                    Debug.LogWarning(
                        $"[Milestone8] Outlet {index} " +
                        "was not found."
                    );

                    continue;
                }

                string circuitId =
                    index == 4 || index == 5
                        ? OfficeCircuitId
                        : WorkCircuitId;

                OutletSocket upper =
                    CreateSocket(
                        outletTransform,
                        0,
                        new Vector3(
                            -0.025f,
                            0.035f,
                            -0.055f
                        ),
                        materials
                    );

                OutletSocket lower =
                    CreateSocket(
                        outletTransform,
                        1,
                        new Vector3(
                            0.025f,
                            -0.035f,
                            -0.055f
                        ),
                        materials
                    );

                PowerOutlet outlet =
                    outletTransform.gameObject
                        .AddComponent<PowerOutlet>();

                outlet.Configure(
                    $"garage-outlet-{index:00}",
                    panel,
                    circuitId,
                    new[]
                    {
                        upper,
                        lower
                    }
                );

                configured.Add(outlet);
            }

            return configured;
        }

        private static OutletSocket CreateSocket(
            Transform outletRoot,
            int socketIndex,
            Vector3 localPosition,
            ElectricalMaterials materials
        )
        {
            GameObject socketObject =
                new GameObject(
                    socketIndex == 0
                        ? "Electrical Socket Upper"
                        : "Electrical Socket Lower"
                );

            socketObject.transform.SetParent(
                outletRoot,
                false
            );

            socketObject.transform.localPosition =
                localPosition;

            socketObject.transform.localRotation =
                Quaternion.identity;

            BoxCollider collider =
                socketObject.AddComponent<BoxCollider>();

            collider.size =
                new Vector3(
                    0.065f,
                    0.065f,
                    0.045f
                );

            GameObject snapPoint =
                new GameObject("Plug Snap Point");

            snapPoint.transform.SetParent(
                socketObject.transform,
                false
            );

            snapPoint.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -0.04f
                );

            snapPoint.transform.localRotation =
                Quaternion.identity;

            GameObject highlight =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            highlight.name = "Socket Highlight";
            highlight.transform.SetParent(
                socketObject.transform,
                false
            );

            highlight.transform.localPosition =
                Vector3.zero;

            highlight.transform.localScale =
                Vector3.one * 0.075f;

            Renderer renderer =
                highlight.GetComponent<Renderer>();

            renderer.sharedMaterial =
                materials.socketHighlight;

            renderer.enabled = false;

            Collider highlightCollider =
                highlight.GetComponent<Collider>();

            UnityEngine.Object.DestroyImmediate(
                highlightCollider
            );

            OutletSocket socket =
                socketObject.AddComponent<
                    OutletSocket
                >();

            socket.Configure(
                null,
                socketIndex,
                snapPoint.transform,
                renderer
            );

            return socket;
        }

        private static MachinePowerConnection
            ConfigureWorkbenchPower(
                GameObject garage,
                GameObject electricalRoot,
                ElectricalMaterials materials
            )
        {
            Transform workbench =
                FindChildRecursive(
                    garage.transform,
                    "Included Workbench"
                );

            if (workbench == null)
            {
                throw new InvalidOperationException(
                    "Included Workbench was not found."
                );
            }

            Bounds bounds =
                CalculateBounds(workbench.gameObject);

            GameObject anchor =
                new GameObject(
                    "Workbench Cable Anchor"
                );

            anchor.transform.SetParent(
                workbench,
                true
            );

            anchor.transform.position =
                new Vector3(
                    bounds.min.x + 0.05f,
                    bounds.min.y + 0.32f,
                    bounds.center.z
                );

            GameObject indicator =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            indicator.name =
                "Workbench Power Indicator";

            indicator.transform.SetParent(
                workbench,
                true
            );

            indicator.transform.position =
                new Vector3(
                    bounds.center.x,
                    bounds.max.y + 0.05f,
                    bounds.min.z + 0.08f
                );

            indicator.transform.localScale =
                Vector3.one * 0.09f;

            Renderer indicatorRenderer =
                indicator.GetComponent<Renderer>();

            indicatorRenderer.sharedMaterial =
                materials.indicator;

            UnityEngine.Object.DestroyImmediate(
                indicator.GetComponent<Collider>()
            );

            MachinePowerConnection machine =
                workbench.gameObject.AddComponent<
                    MachinePowerConnection
                >();

            GameObject plug =
                CreatePlug(
                    electricalRoot.transform,
                    materials,
                    new Vector3(
                        -5.1f,
                        0.18f,
                        2.25f
                    )
                );

            PowerCableVisual cable =
                CreateCable(
                    electricalRoot.transform,
                    anchor.transform,
                    plug.transform,
                    materials
                );

            PowerPlug powerPlug =
                plug.AddComponent<PowerPlug>();

            Rigidbody body =
                plug.GetComponent<Rigidbody>();

            Collider collider =
                plug.GetComponent<Collider>();

            powerPlug.Configure(
                "starter-workbench-plug",
                "Workbench",
                machine,
                anchor.transform,
                3.2f,
                body,
                collider,
                cable
            );

            machine.Configure(
                "starter-workbench",
                "Workbench",
                850,
                powerPlug,
                indicatorRenderer
            );

            return machine;
        }

        private static GameObject CreatePlug(
            Transform parent,
            ElectricalMaterials materials,
            Vector3 position
        )
        {
            GameObject plug =
                new GameObject(
                    "Workbench Power Plug"
                );

            plug.transform.SetParent(
                parent,
                false
            );

            plug.transform.position = position;
            plug.transform.rotation =
                Quaternion.Euler(
                    0f,
                    90f,
                    0f
                );

            BoxCollider collider =
                plug.AddComponent<BoxCollider>();

            collider.size =
                new Vector3(
                    0.18f,
                    0.12f,
                    0.22f
                );

            Rigidbody body =
                plug.AddComponent<Rigidbody>();

            body.mass = 0.2f;
            body.linearDamping = 2.5f;
            body.angularDamping = 4f;
            body.interpolation =
                RigidbodyInterpolation.Interpolate;

            CreateLocalCube(
                plug.transform,
                "Plug Body",
                Vector3.zero,
                new Vector3(
                    0.16f,
                    0.09f,
                    0.17f
                ),
                materials.plugBody,
                false
            );

            CreateLocalCube(
                plug.transform,
                "Left Prong",
                new Vector3(
                    -0.035f,
                    0f,
                    0.12f
                ),
                new Vector3(
                    0.022f,
                    0.022f,
                    0.09f
                ),
                materials.plugMetal,
                false
            );

            CreateLocalCube(
                plug.transform,
                "Right Prong",
                new Vector3(
                    0.035f,
                    0f,
                    0.12f
                ),
                new Vector3(
                    0.022f,
                    0.022f,
                    0.09f
                ),
                materials.plugMetal,
                false
            );

            return plug;
        }

        private static PowerCableVisual CreateCable(
            Transform parent,
            Transform anchor,
            Transform plug,
            ElectricalMaterials materials
        )
        {
            GameObject cable =
                new GameObject(
                    "Workbench Power Cable"
                );

            cable.transform.SetParent(
                parent,
                false
            );

            LineRenderer line =
                cable.AddComponent<LineRenderer>();

            line.sharedMaterial =
                materials.cable;

            line.textureMode =
                LineTextureMode.Tile;

            line.shadowCastingMode =
                ShadowCastingMode.On;

            line.receiveShadows = true;

            PowerCableVisual visual =
                cable.AddComponent<
                    PowerCableVisual
                >();

            visual.Configure(
                anchor,
                plug,
                3.2f
            );

            return visual;
        }

        private static void ConfigureLightingCircuit(
            GameObject garage,
            GameObject electricalRoot,
            ElectricalPanel panel
        )
        {
            StarterGarageLightSwitch lightSwitch =
                garage.GetComponentInChildren<
                    StarterGarageLightSwitch
                >(true);

            List<Light> lights =
                new List<Light>();

            foreach (
                Light targetLight
                in garage.GetComponentsInChildren<
                    Light
                >(true)
            )
            {
                if (
                    targetLight != null &&
                    targetLight.name.IndexOf(
                        "Fluorescent Light Source",
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    lights.Add(targetLight);
                }
            }

            if (lightSwitch != null)
            {
                lightSwitch.Configure(
                    "Garage Lights",
                    lights.ToArray(),
                    true
                );

                lightSwitch.ConfigurePower(
                    panel,
                    LightingCircuitId
                );
            }

            ElectricalLightLoad load =
                electricalRoot.AddComponent<
                    ElectricalLightLoad
                >();

            load.Configure(
                panel,
                LightingCircuitId,
                240,
                lights.ToArray()
            );
        }

        private static void RemovePreviousElectricalSetup(
            GameObject garage,
            FirstPersonController player
        )
        {
            Transform existingRoot =
                FindChildRecursive(
                    garage.transform,
                    ElectricalRootName
                );

            if (existingRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    existingRoot.gameObject
                );
            }

            DestroyComponentsInChildren<
                ElectricalPanel
            >(garage);

            DestroyComponentsInChildren<
                PowerOutlet
            >(garage);

            DestroyComponentsInChildren<
                OutletSocket
            >(garage);

            DestroyComponentsInChildren<
                MachinePowerConnection
            >(garage);

            DestroyComponentsInChildren<
                CircuitBreakerSwitch
            >(garage);

            DestroyNamedChildren(
                garage.transform,
                "Breaker Controls"
            );

            DestroyNamedChildren(
                garage.transform,
                "Electrical Socket Upper"
            );

            DestroyNamedChildren(
                garage.transform,
                "Electrical Socket Lower"
            );

            DestroyNamedChildren(
                garage.transform,
                "Workbench Cable Anchor"
            );

            DestroyNamedChildren(
                garage.transform,
                "Workbench Power Indicator"
            );

            PlayerPlugController controller =
                player.GetComponent<
                    PlayerPlugController
                >();

            if (controller != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    controller
                );
            }
        }

        private static void DestroyComponentsInChildren<T>(
            GameObject rootObject
        )
            where T : Component
        {
            T[] components =
                rootObject.GetComponentsInChildren<T>(
                    true
                );

            foreach (T component in components)
            {
                if (component != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        component
                    );
                }
            }
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

        private static Bounds CalculateBounds(
            GameObject target
        )
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<
                    Renderer
                >(true);

            if (renderers.Length == 0)
            {
                return new Bounds(
                    target.transform.position,
                    Vector3.one
                );
            }

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

        private static ElectricalMaterials
            CreateMaterials()
        {
            return new ElectricalMaterials
            {
                breakerPlate =
                    CreateColorMaterial(
                        "MAT_Electrical_BreakerPlate",
                        new Color(
                            0.18f,
                            0.2f,
                            0.21f
                        ),
                        0.3f,
                        0.65f
                    ),

                breakerSlot =
                    CreateColorMaterial(
                        "MAT_Electrical_BreakerSlot",
                        new Color(
                            0.03f,
                            0.035f,
                            0.04f
                        ),
                        0.18f,
                        0.3f
                    ),

                breakerLever =
                    CreateColorMaterial(
                        "MAT_Electrical_BreakerLever",
                        new Color(
                            0.72f,
                            0.16f,
                            0.08f
                        ),
                        0.2f,
                        0.25f
                    ),

                plugBody =
                    CreateColorMaterial(
                        "MAT_Electrical_PlugBody",
                        new Color(
                            0.035f,
                            0.04f,
                            0.045f
                        ),
                        0.25f,
                        0.15f
                    ),

                plugMetal =
                    CreateColorMaterial(
                        "MAT_Electrical_PlugMetal",
                        new Color(
                            0.68f,
                            0.7f,
                            0.64f
                        ),
                        0.42f,
                        0.78f
                    ),

                socketHighlight =
                    CreateEmissiveMaterial(
                        "MAT_Electrical_SocketHighlight",
                        new Color(
                            1f,
                            0.36f,
                            0.02f
                        )
                    ),

                indicator =
                    CreateEmissiveMaterial(
                        "MAT_Electrical_Indicator",
                        new Color(
                            1f,
                            0.12f,
                            0.08f
                        )
                    ),

                cable =
                    CreateCableMaterial()
            };
        }

        private static Material CreateColorMaterial(
            string name,
            Color color,
            float smoothness,
            float metallic
        )
        {
            string path =
                MaterialRoot + "/" +
                name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(path);

            if (material == null)
            {
                material =
                    new Material(GetLitShader())
                    {
                        name = name
                    };

                AssetDatabase.CreateAsset(
                    material,
                    path
                );
            }

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

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat(
                    "_Smoothness",
                    smoothness
                );
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat(
                    "_Metallic",
                    metallic
                );
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material
            CreateEmissiveMaterial(
                string name,
                Color color
            )
        {
            Material material =
                CreateColorMaterial(
                    name,
                    color,
                    0.35f,
                    0.15f
                );

            if (
                material.HasProperty(
                    "_EmissionColor"
                )
            )
            {
                material.SetColor(
                    "_EmissionColor",
                    color * 2f
                );

                material.EnableKeyword(
                    "_EMISSION"
                );
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateCableMaterial()
        {
            string path =
                MaterialRoot + "/" +
                "MAT_Electrical_Cable.mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(path);

            if (material == null)
            {
                Shader shader =
                    Shader.Find(
                        "Universal Render Pipeline/Unlit"
                    ) ??
                    Shader.Find("Unlit/Color") ??
                    GetLitShader();

                material =
                    new Material(shader)
                    {
                        name =
                            "MAT_Electrical_Cable"
                    };

                AssetDatabase.CreateAsset(
                    material,
                    path
                );
            }

            Color color =
                new Color(
                    0.015f,
                    0.017f,
                    0.02f
                );

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

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader GetLitShader()
        {
            return
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                ) ??
                Shader.Find("Standard");
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

        private static void EnsureFolders()
        {
            foreach (
                string folder
                in new[]
                {
                    "Assets/_Project",
                    "Assets/_Project/Art",
                    "Assets/_Project/Art/Electrical",
                    MaterialRoot
                }
            )
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string path)
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

            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                folderName
            );
        }

        private sealed class ElectricalMaterials
        {
            public Material breakerPlate;
            public Material breakerSlot;
            public Material breakerLever;
            public Material plugBody;
            public Material plugMetal;
            public Material socketHighlight;
            public Material indicator;
            public Material cable;
        }
    }
}
