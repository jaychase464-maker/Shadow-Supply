using System;
using System.Collections.Generic;
using System.IO;
using ShadowSupply.Placement;
using ShadowSupply.Player;
using ShadowSupply.Properties;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace ShadowSupply.Editor
{
    public static class Milestone7StarterGarageSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        private const string GarageRootName =
            "StarterGarage_Property";

        private const string TextureRoot =
            "Assets/_Project/Art/StarterGarage/Textures";

        private const string MaterialRoot =
            "Assets/_Project/Art/StarterGarage/Materials";

        private const string SourceRoot =
            "Assets/_Project/Art/StarterGarage/Source";

        private const string SwitchBoxSourcePath =
            SourceRoot + "/SwitchBox.fbx";

        private const string ElectricityMeterSourcePath =
            SourceRoot + "/ElectricityMeter.fbx";

        private const string ModulePrefabRoot =
            "Assets/_Project/Prefabs/StarterGarage/CleanModules";

        private const string WorkbenchPrefabPath =
            "Assets/_Project/Prefabs/Placeables/Furniture/" +
            "PREFAB_Workbench.prefab";

        private const string RackPrefabPath =
            "Assets/_Project/Prefabs/Placeables/Furniture/" +
            "PREFAB_StorageRack.prefab";

        private const string DeskPrefabPath =
            "Assets/_Project/Prefabs/Placeables/Furniture/" +
            "PREFAB_OfficeDesk.prefab";

        private const string CabinetPrefabPath =
            "Assets/_Project/Prefabs/Placeables/Furniture/" +
            "PREFAB_UtilityCabinet.prefab";

        private const string CctvPrefabPath =
            "Assets/_Project/Prefabs/Placeables/Furniture/" +
            "PREFAB_CCTVCamera.prefab";

        private const string KeypadPrefabPath =
            "Assets/_Project/Prefabs/Placeables/Furniture/" +
            "PREFAB_DoorKeypad.prefab";

        [MenuItem(
            "Shadow Supply/Setup/Apply Milestone 7A Modular Starter Garage"
        )]
        public static void ApplyMilestoneSevenA()
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

            FirstPersonController player =
                UnityEngine.Object.FindFirstObjectByType<
                    FirstPersonController
                >();

            if (player == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "No FirstPersonController exists in Dev_Playground.",
                    "OK"
                );

                return;
            }

            EnsureFolders();

            GarageMaterials materials =
                CreateMaterials();

            CleanGarageModules modules =
                CreateCleanModules(materials);

            RemovePreviousGarage();
            RemoveOldEnvironment();

            GameObject root =
                new GameObject(GarageRootName);

            GarageBuildResult result =
                BuildGarage(
                    root.transform,
                    materials,
                    modules
                );

            InstallIncludedFurniture(
                root.transform,
                materials
            );

            InstallUtilities(
                root.transform,
                materials,
                result.interiorLights
            );

            InstallExteriorDressing(
                root.transform,
                materials
            );

            RepositionExistingGameplayObjects(
                result.deliveryDrop,
                result.supplierTerminalPoint
            );

            RemoveDevelopmentBlockers();
            RepositionLooseDevelopmentObjects();

            PositionPlayer(
                player,
                result.playerSpawn
            );

            StarterGarageProperty property =
                root.AddComponent<StarterGarageProperty>();

            property.Configure(
                result.playerSpawn,
                result.deliveryDrop,
                result.overheadDoor,
                result.entryDoor
            );

            EditorUtility.SetDirty(property);
            EditorUtility.SetDirty(player);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = root;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "The starter garage has been rebuilt with cleaned wall " +
                "materials, corrected overhead door behavior, and improved " +
                "utility props.\n\n" +
                "v0.7.6 removes the remaining development blockers, moves " +
                "furniture away from the walls, seats the monitor on the " +
                "desk, tightens the side-door frame, and uses the supplied " +
                "breaker-panel and meter models.",
                "Done"
            );
        }

        private static GarageBuildResult BuildGarage(
            Transform root,
            GarageMaterials materials,
            CleanGarageModules modules
        )
        {
            GameObject structure =
                new GameObject("Structure");

            structure.transform.SetParent(
                root,
                false
            );

            BuildFloorAndLot(
                root,
                materials
            );

            BuildWalls(
                structure.transform,
                modules,
                materials
            );

            BuildRoof(
                structure.transform,
                modules,
                materials
            );

            StarterGarageDoor overheadDoor =
                BuildOverheadDoor(
                    structure.transform,
                    materials
                );

            StarterGarageDoor entryDoor =
                BuildEntryDoor(
                    structure.transform,
                    materials
                );

            List<Light> lights =
                BuildLighting(
                    structure.transform,
                    materials
                );

            Transform playerSpawn =
                CreateMarker(
                    root,
                    "Player Spawn",
                    new Vector3(0f, 0.05f, -10f),
                    Quaternion.identity
                );

            Transform deliveryDrop =
                CreateMarker(
                    root,
                    "Delivery Drop",
                    new Vector3(-3.2f, 0.05f, -7.2f),
                    Quaternion.identity
                );

            Transform supplierTerminalPoint =
                CreateMarker(
                    root,
                    "Supplier Terminal Point",
                    new Vector3(4.85f, 0f, -2.3f),
                    Quaternion.Euler(0f, 180f, 0f)
                );

            return new GarageBuildResult
            {
                playerSpawn = playerSpawn,
                deliveryDrop = deliveryDrop,
                supplierTerminalPoint = supplierTerminalPoint,
                overheadDoor = overheadDoor,
                entryDoor = entryDoor,
                interiorLights = lights
            };
        }

        private static void BuildFloorAndLot(
            Transform root,
            GarageMaterials materials
        )
        {
            GameObject lot =
                CreateCube(
                    root,
                    "Exterior Asphalt Lot",
                    new Vector3(0f, -0.16f, -2f),
                    new Vector3(28f, 0.3f, 24f),
                    materials.asphalt,
                    true
                );

            SetTextureScale(
                lot,
                new Vector2(4f, 4f)
            );

            GameObject slab =
                CreateCube(
                    root,
                    "Interior Concrete Slab",
                    new Vector3(0f, -0.03f, 0f),
                    new Vector3(12f, 0.16f, 8f),
                    materials.concrete,
                    true
                );

            SetTextureScale(
                slab,
                new Vector2(3f, 2f)
            );

            GameObject pad =
                CreateCube(
                    root,
                    "Delivery Concrete Pad",
                    new Vector3(-3.2f, -0.01f, -7.1f),
                    new Vector3(4.6f, 0.18f, 3.7f),
                    materials.concrete,
                    true
                );

            SetTextureScale(
                pad,
                new Vector2(1.5f, 1.5f)
            );
        }

        private static void BuildWalls(
            Transform parent,
            CleanGarageModules modules,
            GarageMaterials materials
        )
        {
            GameObject walls =
                new GameObject("Clean Modular Walls");

            walls.transform.SetParent(
                parent,
                false
            );

            // Rear wall: three exact 4 m modules.
            for (int index = 0; index < 3; index++)
            {
                InstantiateModule(
                    modules.wall4m,
                    walls.transform,
                    $"Rear Wall Module {index + 1}",
                    new Vector3(-4f + index * 4f, 0f, 4f),
                    Quaternion.identity
                );
            }

            // Left wall: two exact 4 m modules.
            for (int index = 0; index < 2; index++)
            {
                InstantiateModule(
                    modules.wall4m,
                    walls.transform,
                    $"Left Wall Module {index + 1}",
                    new Vector3(-6f, 0f, -2f + index * 4f),
                    Quaternion.Euler(0f, 90f, 0f)
                );
            }

            // Right rear wall.
            InstantiateModule(
                modules.wall4m,
                walls.transform,
                "Right Rear Wall Module",
                new Vector3(6f, 0f, 2f),
                Quaternion.Euler(0f, 90f, 0f)
            );

            // Right front wall split around the entry door.
            CreateWallSegment(
                walls.transform,
                "Right Front Wall Segment",
                new Vector3(6f, 2.1f, -3.55f),
                new Vector3(0.25f, 4.2f, 0.9f),
                materials.brick
            );

            CreateWallSegment(
                walls.transform,
                "Right Middle Wall Segment",
                new Vector3(6f, 2.1f, -0.8f),
                new Vector3(0.25f, 4.2f, 1.6f),
                materials.brick
            );

            CreateWallSegment(
                walls.transform,
                "Entry Door Lintel",
                new Vector3(6f, 3.35f, -2.35f),
                new Vector3(0.25f, 1.7f, 1.5f),
                materials.brick
            );

            // Front wall leaves a clean 4 m overhead-door opening.
            InstantiateModule(
                modules.wall4m,
                walls.transform,
                "Front Left Wall Module",
                new Vector3(-4f, 0f, -4f),
                Quaternion.identity
            );

            InstantiateModule(
                modules.wall4m,
                walls.transform,
                "Front Right Wall Module",
                new Vector3(4f, 0f, -4f),
                Quaternion.identity
            );

            CreateWallSegment(
                walls.transform,
                "Overhead Door Lintel",
                new Vector3(0f, 3.82f, -4f),
                new Vector3(4f, 0.76f, 0.25f),
                materials.brick
            );

            // Steel framing around the main opening.
            InstantiateModule(
                modules.beamVertical,
                walls.transform,
                "Left Opening Column",
                new Vector3(-2.12f, 0f, -4.08f),
                Quaternion.identity
            );

            InstantiateModule(
                modules.beamVertical,
                walls.transform,
                "Right Opening Column",
                new Vector3(2.12f, 0f, -4.08f),
                Quaternion.identity
            );

            InstantiateModule(
                modules.beamHorizontal,
                walls.transform,
                "Opening Header Beam",
                new Vector3(0f, 3.58f, -4.08f),
                Quaternion.identity
            );

            CreateWallSegment(
                walls.transform,
                "Left Opening Weather Seal",
                new Vector3(-2.02f, 1.75f, -4.01f),
                new Vector3(0.08f, 3.5f, 0.08f),
                materials.darkMetal
            );

            CreateWallSegment(
                walls.transform,
                "Right Opening Weather Seal",
                new Vector3(2.02f, 1.75f, -4.01f),
                new Vector3(0.08f, 3.5f, 0.08f),
                materials.darkMetal
            );

            CreateWallSegment(
                walls.transform,
                "Top Opening Weather Seal",
                new Vector3(0f, 3.47f, -4.01f),
                new Vector3(4.04f, 0.08f, 0.08f),
                materials.darkMetal
            );
        }

        private static void BuildRoof(
            Transform parent,
            CleanGarageModules modules,
            GarageMaterials materials
        )
        {
            GameObject roof =
                new GameObject("Clean Modular Roof");

            roof.transform.SetParent(
                parent,
                false
            );

            for (int xIndex = 0; xIndex < 3; xIndex++)
            {
                for (int zIndex = 0; zIndex < 2; zIndex++)
                {
                    InstantiateModule(
                        modules.roof4x4,
                        roof.transform,
                        $"Roof Module {xIndex + 1}-{zIndex + 1}",
                        new Vector3(
                            -4f + xIndex * 4f,
                            4.18f,
                            -2f + zIndex * 4f
                        ),
                        Quaternion.identity
                    );

                    GameObject ceilingPanel =
                        CreateCube(
                            roof.transform,
                            $"Ceiling Panel {xIndex + 1}-{zIndex + 1}",
                            new Vector3(
                                -4f + xIndex * 4f,
                                4.03f,
                                -2f + zIndex * 4f
                            ),
                            new Vector3(3.96f, 0.06f, 3.96f),
                            materials.ceiling,
                            false
                        );

                    SetTextureScale(
                        ceilingPanel,
                        new Vector2(1.5f, 1.5f)
                    );
                }
            }

            // Interior cross beams.
            for (int index = 0; index < 4; index++)
            {
                GameObject beam =
                    CreateCube(
                        roof.transform,
                        $"Interior Roof Beam {index + 1}",
                        new Vector3(
                            -6f + index * 4f,
                            3.86f,
                            0f
                        ),
                        new Vector3(0.16f, 0.18f, 8f),
                        materials.darkMetal,
                        false
                    );

                beam.GetComponent<Renderer>().shadowCastingMode =
                    ShadowCastingMode.On;
            }
        }

        private static StarterGarageDoor BuildOverheadDoor(
            Transform parent,
            GarageMaterials materials
        )
        {
            GameObject trackRoot =
                new GameObject("Garage Door Tracks");

            trackRoot.transform.SetParent(
                parent,
                false
            );

            CreateLocalCube(
                trackRoot.transform,
                "Left Vertical Track",
                new Vector3(-2.03f, 1.75f, -4.08f),
                new Vector3(0.09f, 3.5f, 0.1f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                trackRoot.transform,
                "Right Vertical Track",
                new Vector3(2.03f, 1.75f, -4.08f),
                new Vector3(0.09f, 3.5f, 0.1f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                trackRoot.transform,
                "Left Ceiling Track",
                new Vector3(-2.03f, 3.56f, -2.15f),
                new Vector3(0.09f, 0.1f, 3.85f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                trackRoot.transform,
                "Right Ceiling Track",
                new Vector3(2.03f, 3.56f, -2.15f),
                new Vector3(0.09f, 0.1f, 3.85f),
                materials.darkMetal,
                false
            );

            GameObject doorRoot =
                new GameObject("Temporary Overhead Garage Door");

            doorRoot.transform.SetParent(
                parent,
                false
            );

            doorRoot.transform.localPosition =
                new Vector3(0f, 3.48f, -4.16f);

            for (int index = 0; index < 6; index++)
            {
                GameObject panel =
                    CreateLocalCube(
                        doorRoot.transform,
                        $"Door Panel {index + 1}",
                        new Vector3(
                            0f,
                            -0.27f - index * 0.54f,
                            0f
                        ),
                        new Vector3(4.08f, 0.5f, 0.12f),
                        materials.garageDoor,
                        true
                    );

                panel.transform.localRotation =
                    Quaternion.identity;
            }

            StarterGarageDoor door =
                doorRoot.AddComponent<StarterGarageDoor>();

            door.Configure(
                "garage door",
                StarterGarageDoorMotion.Rotate,
                Vector3.zero,
                new Vector3(-90f, 0f, 0f),
                4f,
                true
            );

            AddStarterAsset(
                doorRoot,
                "starter-garage-overhead-door"
            );

            return door;
        }

        private static StarterGarageDoor BuildEntryDoor(
            Transform parent,
            GarageMaterials materials
        )
        {
            GameObject assembly =
                new GameObject("Entry Door Assembly");

            assembly.transform.SetParent(
                parent,
                false
            );

            assembly.transform.position =
                new Vector3(5.86f, 0f, -2.35f);

            assembly.transform.rotation =
                Quaternion.Euler(0f, -90f, 0f);

            CreateDoorFrame(
                assembly.transform,
                materials
            );

            GameObject pivot =
                new GameObject("Entry Door Pivot");

            pivot.transform.SetParent(
                assembly.transform,
                false
            );

            pivot.transform.localPosition =
                new Vector3(-0.58f, 0f, 0f);

            GameObject doorVisual =
                CreateCube(
                    pivot.transform,
                    "Entry Door",
                    pivot.transform.position,
                    new Vector3(1.16f, 2.34f, 0.12f),
                    materials.entryDoor,
                    true
                );

            doorVisual.transform.localPosition =
                new Vector3(0.58f, 1.17f, 0f);

            doorVisual.transform.localRotation =
                Quaternion.identity;

            StarterGarageDoor door =
                pivot.AddComponent<StarterGarageDoor>();

            door.Configure(
                "entry door",
                StarterGarageDoorMotion.Rotate,
                Vector3.zero,
                new Vector3(0f, -105f, 0f),
                5f,
                false
            );

            AddStarterAsset(
                assembly,
                "starter-garage-entry-door"
            );

            return door;
        }

        private static void CreateDoorFrame(
            Transform parent,
            GarageMaterials materials
        )
        {
            CreateLocalCube(
                parent,
                "Entry Frame Left",
                new Vector3(-0.67f, 1.24f, 0f),
                new Vector3(0.12f, 2.48f, 0.2f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                parent,
                "Entry Frame Right",
                new Vector3(0.67f, 1.24f, 0f),
                new Vector3(0.12f, 2.48f, 0.2f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                parent,
                "Entry Frame Top",
                new Vector3(0f, 2.44f, 0f),
                new Vector3(1.46f, 0.16f, 0.2f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                parent,
                "Entry Left Weather Seal",
                new Vector3(-0.59f, 1.17f, -0.07f),
                new Vector3(0.04f, 2.34f, 0.04f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                parent,
                "Entry Right Weather Seal",
                new Vector3(0.59f, 1.17f, -0.07f),
                new Vector3(0.04f, 2.34f, 0.04f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                parent,
                "Entry Top Weather Seal",
                new Vector3(0f, 2.32f, -0.07f),
                new Vector3(1.22f, 0.05f, 0.04f),
                materials.darkMetal,
                false
            );
        }

        private static List<Light> BuildLighting(
            Transform parent,
            GarageMaterials materials
        )
        {
            GameObject lighting =
                new GameObject("Installed Lighting");

            lighting.transform.SetParent(
                parent,
                false
            );

            List<Light> lights =
                new List<Light>();

            Vector3[] positions =
            {
                new Vector3(-3f, 3.82f, -1.7f),
                new Vector3(3f, 3.82f, -1.7f),
                new Vector3(-3f, 3.82f, 1.7f),
                new Vector3(3f, 3.82f, 1.7f)
            };

            for (int index = 0; index < positions.Length; index++)
            {
                GameObject fixture =
                    BuildCeilingLightFixture(
                        lighting.transform,
                        $"Fluorescent Fixture {index + 1}",
                        positions[index],
                        materials
                    );

                GameObject lightAnchor =
                    new GameObject(
                        $"Fluorescent Light Source {index + 1}"
                    );

                lightAnchor.transform.SetParent(
                    fixture.transform,
                    false
                );

                lightAnchor.transform.localPosition =
                    new Vector3(0f, -0.11f, 0f);

                lightAnchor.transform.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);

                Light light =
                    lightAnchor.AddComponent<Light>();

                light.type = LightType.Spot;
                light.range = 8f;
                light.spotAngle = 118f;
                light.innerSpotAngle = 92f;
                light.intensity = 7f;
                light.color =
                    new Color(0.84f, 0.9f, 1f);
                light.shadows = LightShadows.Soft;

                lights.Add(light);

                AddStarterAsset(
                    fixture,
                    $"starter-garage-light-{index + 1}"
                );
            }

            return lights;
        }

        private static void InstallIncludedFurniture(
            Transform parent,
            GarageMaterials materials
        )
        {
            GameObject furniture =
                new GameObject("Included Starter Furniture");

            furniture.transform.SetParent(
                parent,
                false
            );

            CreateIncludedFurniture(
                furniture.transform,
                WorkbenchPrefabPath,
                "Included Workbench",
                "starter-workbench",
                new Vector3(-4.62f, 0f, 1.15f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(2.2f, 0.9f, 0.8f),
                materials.darkMetal
            );

            CreateIncludedFurniture(
                furniture.transform,
                RackPrefabPath,
                "Included Storage Rack",
                "starter-storage-rack",
                new Vector3(-0.9f, 0f, 3.45f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(2.4f, 2.25f, 0.8f),
                materials.darkMetal
            );

            GameObject desk =
                CreateIncludedFurniture(
                    furniture.transform,
                    DeskPrefabPath,
                    "Included Office Desk",
                    "starter-office-desk",
                    new Vector3(4.22f, 0f, 1.15f),
                    Quaternion.Euler(0f, -90f, 0f),
                    new Vector3(2.5f, 1.05f, 1f),
                    materials.darkMetal
                );

            GameObject monitor =
                BuildMonitorOnDesk(
                    furniture.transform,
                    desk,
                    materials
                );

            AddStarterAsset(
                monitor,
                "starter-office-monitor"
            );

            GameObject cctv =
                CreateIncludedFurniture(
                    furniture.transform,
                    CctvPrefabPath,
                    "Installed CCTV Camera",
                    "starter-cctv",
                    new Vector3(5.18f, 3.22f, 3.55f),
                    Quaternion.Euler(8f, -135f, 0f),
                    new Vector3(0.5f, 0.35f, 0.55f),
                    materials.darkMetal
                );

            GameObject interiorKeypad =
                CreateIncludedFurniture(
                    furniture.transform,
                    KeypadPrefabPath,
                    "Interior Door Keypad",
                    "starter-door-keypad-interior",
                    new Vector3(5.78f, 1.3f, -3.45f),
                    Quaternion.Euler(0f, -90f, 0f),
                    new Vector3(0.22f, 0.34f, 0.08f),
                    materials.darkMetal
                );

            GameObject exteriorKeypad =
                CreateIncludedFurniture(
                    furniture.transform,
                    KeypadPrefabPath,
                    "Exterior Door Keypad",
                    "starter-door-keypad-exterior",
                    new Vector3(6.22f, 1.3f, -3.45f),
                    Quaternion.Euler(0f, 90f, 0f),
                    new Vector3(0.22f, 0.34f, 0.08f),
                    materials.darkMetal
                );

            RemovePlacedObjectComponent(cctv);
            RemovePlacedObjectComponent(interiorKeypad);
            RemovePlacedObjectComponent(exteriorKeypad);
        }

        private static void InstallUtilities(
            Transform parent,
            GarageMaterials materials,
            List<Light> lights
        )
        {
            GameObject utilities =
                new GameObject("Installed Utilities");

            utilities.transform.SetParent(
                parent,
                false
            );

            GameObject breaker =
                BuildDetailedBreakerPanel(
                    utilities.transform,
                    new Vector3(5.74f, 1.52f, 0.8f),
                    Quaternion.Euler(0f, 90f, 0f),
                    materials
                );

            AddStarterAsset(
                breaker,
                "starter-breaker-panel"
            );

            GameObject meter =
                CreateImportedUtilityModel(
                    ElectricityMeterSourcePath,
                    utilities.transform,
                    "Exterior Electricity Meter",
                    new Vector3(6.24f, 0.95f, 0.8f),
                    Quaternion.Euler(0f, 90f, 0f),
                    new Vector3(0.42f, 0.62f, 0.24f),
                    materials.electrical
                ) ??
                BuildDetailedElectricMeter(
                    utilities.transform,
                    new Vector3(6.24f, 1.42f, 0.8f),
                    Quaternion.Euler(0f, 90f, 0f),
                    materials
                );

            AddStarterAsset(
                meter,
                "starter-electricity-meter"
            );

            Vector3[] outlets =
            {
                new Vector3(-5.78f, 0.48f, 2.7f),
                new Vector3(-2.3f, 0.48f, 3.86f),
                new Vector3(2.3f, 0.48f, 3.86f),
                new Vector3(5.78f, 0.48f, 2.25f),
                new Vector3(5.78f, 0.48f, -0.1f),
                new Vector3(-5.78f, 0.48f, -1.9f)
            };

            Quaternion[] outletRotations =
            {
                Quaternion.Euler(0f, -90f, 0f),
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.Euler(0f, 90f, 0f),
                Quaternion.Euler(0f, 90f, 0f),
                Quaternion.Euler(0f, -90f, 0f)
            };

            for (int index = 0; index < outlets.Length; index++)
            {
                GameObject outlet =
                    BuildDetailedOutlet(
                        utilities.transform,
                        $"Outlet {index + 1}",
                        outlets[index],
                        outletRotations[index],
                        materials
                    );

                AddStarterAsset(
                    outlet,
                    $"starter-outlet-{index + 1}"
                );
            }

            GameObject switchObject =
                BuildDetailedLightSwitch(
                    utilities.transform,
                    new Vector3(5.78f, 1.3f, -1.25f),
                    Quaternion.Euler(0f, 90f, 0f),
                    materials
                );

            StarterGarageLightSwitch lightSwitch =
                switchObject.AddComponent<
                    StarterGarageLightSwitch
                >();

            lightSwitch.Configure(
                "garage lights",
                lights.ToArray(),
                true
            );

            AddStarterAsset(
                switchObject,
                "starter-light-switch"
            );

            BuildVentilation(
                utilities.transform,
                materials
            );
        }


        private static GameObject BuildCeilingLightFixture(
            Transform parent,
            string objectName,
            Vector3 position,
            GarageMaterials materials
        )
        {
            GameObject fixture =
                new GameObject(objectName);

            fixture.transform.SetParent(
                parent,
                false
            );

            fixture.transform.position = position;

            CreateLocalCube(
                fixture.transform,
                "Housing",
                new Vector3(0f, -0.015f, 0f),
                new Vector3(1.38f, 0.07f, 0.24f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                fixture.transform,
                "Diffuser",
                new Vector3(0f, -0.07f, 0f),
                new Vector3(1.22f, 0.035f, 0.18f),
                materials.lightFixture,
                false
            );

            CreateLocalCube(
                fixture.transform,
                "Left End Cap",
                new Vector3(-0.64f, -0.02f, 0f),
                new Vector3(0.06f, 0.05f, 0.21f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                fixture.transform,
                "Right End Cap",
                new Vector3(0.64f, -0.02f, 0f),
                new Vector3(0.06f, 0.05f, 0.21f),
                materials.darkMetal,
                false
            );

            return fixture;
        }

        private static GameObject BuildMonitorOnDesk(
            Transform parent,
            GameObject desk,
            GarageMaterials materials
        )
        {
            if (desk == null)
            {
                return BuildDetailedOfficeMonitor(
                    parent,
                    new Vector3(4.1f, 1.05f, 1.15f),
                    Quaternion.Euler(0f, 90f, 0f),
                    materials
                );
            }

            Bounds deskBounds =
                CalculateRendererBounds(
                    desk
                );

            Vector3 monitorPosition =
                new Vector3(
                    deskBounds.max.x - 0.22f,
                    deskBounds.max.y + 0.03f,
                    deskBounds.center.z
                );

            return BuildDetailedOfficeMonitor(
                parent,
                monitorPosition,
                Quaternion.Euler(0f, 90f, 0f),
                materials
            );
        }

        private static GameObject BuildDetailedOfficeMonitor(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            GarageMaterials materials
        )
        {
            GameObject monitor =
                new GameObject("Office Monitor");

            monitor.transform.SetParent(
                parent,
                false
            );

            monitor.transform.position = position;
            monitor.transform.rotation = rotation;

            CreateLocalCube(
                monitor.transform,
                "Screen Body",
                new Vector3(0f, 0.24f, 0f),
                new Vector3(0.58f, 0.36f, 0.07f),
                materials.darkMetal,
                true
            );

            CreateLocalCube(
                monitor.transform,
                "Screen Face",
                new Vector3(0f, 0.24f, -0.037f),
                new Vector3(0.5f, 0.28f, 0.012f),
                materials.monitor,
                false
            );

            CreateLocalCube(
                monitor.transform,
                "Monitor Neck",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(0.05f, 0.22f, 0.05f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                monitor.transform,
                "Monitor Base",
                new Vector3(0f, -0.03f, 0.02f),
                new Vector3(0.23f, 0.02f, 0.16f),
                materials.darkMetal,
                false
            );

            return monitor;
        }

        private static GameObject BuildDetailedBreakerPanel(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            GarageMaterials materials
        )
        {
            GameObject panel =
                new GameObject("Main Breaker Panel");

            panel.transform.SetParent(
                parent,
                false
            );

            panel.transform.position = position;
            panel.transform.rotation = rotation;

            CreateLocalCube(
                panel.transform,
                "Cabinet",
                Vector3.zero,
                new Vector3(0.62f, 0.92f, 0.13f),
                materials.electrical,
                true
            );

            CreateLocalCube(
                panel.transform,
                "Front Door",
                new Vector3(0f, 0f, -0.076f),
                new Vector3(0.57f, 0.86f, 0.025f),
                materials.switchBox,
                false
            );

            CreateLocalCube(
                panel.transform,
                "Door Handle",
                new Vector3(0.21f, 0f, -0.098f),
                new Vector3(0.035f, 0.15f, 0.018f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                panel.transform,
                "Warning Label",
                new Vector3(-0.11f, 0.22f, -0.096f),
                new Vector3(0.21f, 0.1f, 0.01f),
                materials.outlet,
                false
            );

            for (int index = 0; index < 4; index++)
            {
                CreateLocalCube(
                    panel.transform,
                    $"Panel Hinge {index + 1}",
                    new Vector3(
                        -0.29f,
                        -0.29f + index * 0.19f,
                        -0.084f
                    ),
                    new Vector3(0.025f, 0.08f, 0.025f),
                    materials.darkMetal,
                    false
                );
            }

            return panel;
        }

        private static GameObject BuildDetailedElectricMeter(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            GarageMaterials materials
        )
        {
            GameObject meter =
                new GameObject("Exterior Electricity Meter");

            meter.transform.SetParent(
                parent,
                false
            );

            meter.transform.position = position;
            meter.transform.rotation = rotation;

            CreateLocalCube(
                meter.transform,
                "Meter Body",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.28f, 0.46f, 0.12f),
                materials.electrical,
                true
            );

            GameObject globe =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            globe.name = "Meter Globe";
            globe.transform.SetParent(
                meter.transform,
                false
            );

            globe.transform.localPosition =
                new Vector3(0f, 0.07f, -0.09f);

            globe.transform.localRotation =
                Quaternion.identity;

            globe.transform.localScale =
                new Vector3(0.2f, 0.2f, 0.2f);

            Renderer globeRenderer =
                globe.GetComponent<Renderer>();

            if (globeRenderer != null)
            {
                globeRenderer.sharedMaterial =
                    materials.outlet;
            }

            Collider globeCollider =
                globe.GetComponent<Collider>();

            if (globeCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    globeCollider
                );
            }

            CreateLocalCube(
                meter.transform,
                "Meter Base",
                new Vector3(0f, -0.12f, 0.02f),
                new Vector3(0.14f, 0.08f, 0.08f),
                materials.darkMetal,
                false
            );

            return meter;
        }

        private static GameObject BuildDetailedOutlet(
            Transform parent,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            GarageMaterials materials
        )
        {
            GameObject outlet =
                new GameObject(objectName);

            outlet.transform.SetParent(
                parent,
                false
            );

            outlet.transform.position = position;
            outlet.transform.rotation = rotation;

            CreateLocalCube(
                outlet.transform,
                "Faceplate",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.12f, 0.16f, 0.03f),
                materials.outlet,
                true
            );

            CreateLocalCube(
                outlet.transform,
                "Left Socket Top",
                new Vector3(-0.02f, 0.03f, -0.017f),
                new Vector3(0.02f, 0.028f, 0.008f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                outlet.transform,
                "Left Socket Bottom",
                new Vector3(-0.02f, -0.03f, -0.017f),
                new Vector3(0.02f, 0.028f, 0.008f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                outlet.transform,
                "Right Socket Top",
                new Vector3(0.02f, 0.03f, -0.017f),
                new Vector3(0.02f, 0.028f, 0.008f),
                materials.darkMetal,
                false
            );

            CreateLocalCube(
                outlet.transform,
                "Right Socket Bottom",
                new Vector3(0.02f, -0.03f, -0.017f),
                new Vector3(0.02f, 0.028f, 0.008f),
                materials.darkMetal,
                false
            );

            return outlet;
        }

        private static GameObject BuildDetailedLightSwitch(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            GarageMaterials materials
        )
        {
            GameObject switchObject =
                new GameObject("Garage Light Switch");

            switchObject.transform.SetParent(
                parent,
                false
            );

            switchObject.transform.position = position;
            switchObject.transform.rotation = rotation;

            CreateLocalCube(
                switchObject.transform,
                "Faceplate",
                new Vector3(0f, 0f, 0f),
                new Vector3(0.11f, 0.18f, 0.03f),
                materials.outlet,
                true
            );

            CreateLocalCube(
                switchObject.transform,
                "Rocker",
                new Vector3(0f, 0f, -0.018f),
                new Vector3(0.05f, 0.1f, 0.012f),
                materials.darkMetal,
                false
            );

            return switchObject;
        }

        private static GameObject CreateImportedUtilityModel(
            string sourcePath,
            Transform parent,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            Vector3 targetSize,
            Material material
        )
        {
            GameObject source =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    sourcePath
                );

            if (source == null)
            {
                return null;
            }

            GameObject wrapper =
                new GameObject(objectName);

            wrapper.transform.SetParent(
                parent,
                false
            );

            wrapper.transform.position = position;
            wrapper.transform.rotation = rotation;

            GameObject visual =
                PrefabUtility.InstantiatePrefab(
                    source,
                    wrapper.transform
                ) as GameObject;

            if (visual == null)
            {
                visual =
                    UnityEngine.Object.Instantiate(
                        source,
                        wrapper.transform
                    );
            }

            visual.name = objectName + " Visual";
            visual.transform.localPosition =
                Vector3.zero;

            visual.transform.localRotation =
                Quaternion.identity;

            visual.transform.localScale =
                Vector3.one;

            UnpackPrefabInstance(visual);
            RemoveAllColliders(visual);
            ApplyMaterialToRenderers(
                visual,
                material
            );

            NormalizeImportedVisual(
                wrapper.transform,
                visual.transform,
                targetSize
            );

            BoxCollider collider =
                wrapper.AddComponent<BoxCollider>();

            collider.size = targetSize;
            collider.center =
                new Vector3(
                    0f,
                    targetSize.y * 0.5f,
                    0f
                );

            return wrapper;
        }

        private static void NormalizeImportedVisual(
            Transform wrapper,
            Transform visual,
            Vector3 targetSize
        )
        {
            Bounds localBounds =
                CalculateLocalRendererBounds(
                    wrapper,
                    visual.gameObject
                );

            Vector3 safeSize =
                new Vector3(
                    Mathf.Max(0.001f, localBounds.size.x),
                    Mathf.Max(0.001f, localBounds.size.y),
                    Mathf.Max(0.001f, localBounds.size.z)
                );

            visual.localScale =
                new Vector3(
                    targetSize.x / safeSize.x,
                    targetSize.y / safeSize.y,
                    targetSize.z / safeSize.z
                );

            localBounds =
                CalculateLocalRendererBounds(
                    wrapper,
                    visual.gameObject
                );

            visual.localPosition +=
                new Vector3(
                    -localBounds.center.x,
                    -localBounds.min.y,
                    -localBounds.center.z
                );
        }

        private static Bounds CalculateLocalRendererBounds(
            Transform localRoot,
            GameObject target
        )
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(
                    true
                );

            if (renderers.Length == 0)
            {
                return new Bounds(
                    Vector3.zero,
                    Vector3.one
                );
            }

            bool initialized = false;
            Bounds bounds =
                new Bounds(
                    Vector3.zero,
                    Vector3.zero
                );

            foreach (Renderer renderer in renderers)
            {
                Bounds worldBounds =
                    renderer.bounds;

                Vector3 center =
                    worldBounds.center;

                Vector3 extents =
                    worldBounds.extents;

                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 worldPoint =
                                center +
                                Vector3.Scale(
                                    extents,
                                    new Vector3(x, y, z)
                                );

                            Vector3 localPoint =
                                localRoot.InverseTransformPoint(
                                    worldPoint
                                );

                            if (!initialized)
                            {
                                bounds =
                                    new Bounds(
                                        localPoint,
                                        Vector3.zero
                                    );

                                initialized = true;
                            }
                            else
                            {
                                bounds.Encapsulate(
                                    localPoint
                                );
                            }
                        }
                    }
                }
            }

            return bounds;
        }

        private static Bounds CalculateRendererBounds(
            GameObject target
        )
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(
                    true
                );

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

        private static void ApplyMaterialToRenderers(
            GameObject target,
            Material material
        )
        {
            if (
                target == null ||
                material == null
            )
            {
                return;
            }

            foreach (
                Renderer renderer
                in target.GetComponentsInChildren<Renderer>(
                    true
                )
            )
            {
                int materialCount =
                    Mathf.Max(
                        1,
                        renderer.sharedMaterials.Length
                    );

                Material[] assigned =
                    new Material[materialCount];

                for (
                    int index = 0;
                    index < assigned.Length;
                    index++
                )
                {
                    assigned[index] = material;
                }

                renderer.sharedMaterials =
                    assigned;
            }
        }

        private static void RemoveAllColliders(
            GameObject target
        )
        {
            foreach (
                Collider collider
                in target.GetComponentsInChildren<Collider>(
                    true
                )
            )
            {
                UnityEngine.Object.DestroyImmediate(
                    collider
                );
            }
        }

        private static void BuildVentilation(
            Transform parent,
            GarageMaterials materials
        )
        {
            CreateCube(
                parent,
                "Main Ceiling Duct",
                new Vector3(0f, 3.55f, 2.65f),
                new Vector3(7.6f, 0.42f, 0.42f),
                materials.vent,
                false
            );

            CreateCube(
                parent,
                "Right Ceiling Duct",
                new Vector3(4.15f, 3.55f, 1.3f),
                new Vector3(0.42f, 0.42f, 3.1f),
                materials.vent,
                false
            );

            CreateCube(
                parent,
                "Wall Vent",
                new Vector3(-5.83f, 3.05f, 2.65f),
                new Vector3(0.18f, 0.7f, 0.7f),
                materials.vent,
                false
            );
        }

        private static void InstallExteriorDressing(
            Transform parent,
            GarageMaterials materials
        )
        {
            GameObject exterior =
                new GameObject("Exterior Dressing");

            exterior.transform.SetParent(
                parent,
                false
            );

            BuildStreetLight(
                exterior.transform,
                materials,
                new Vector3(-8.5f, 0f, -7.5f)
            );

            CreateCube(
                exterior.transform,
                "Exterior Power Box",
                new Vector3(-7f, 0.65f, 1.8f),
                new Vector3(0.7f, 1.3f, 0.5f),
                materials.electrical,
                true
            );

            BuildDumpster(
                exterior.transform,
                materials,
                new Vector3(8.1f, 0f, 2.5f)
            );

            BuildPallet(
                exterior.transform,
                materials,
                new Vector3(-6.9f, 0.08f, -1.4f),
                12f
            );

            BuildPallet(
                exterior.transform,
                materials,
                new Vector3(-6.65f, 0.24f, -1.25f),
                -9f
            );

            BuildBox(
                exterior.transform,
                materials,
                new Vector3(-6.8f, 0.35f, -2.25f),
                new Vector3(0.65f, 0.7f, 0.65f)
            );

            BuildBox(
                exterior.transform,
                materials,
                new Vector3(-6.15f, 0.25f, -2.15f),
                new Vector3(0.5f, 0.5f, 0.5f)
            );

            for (int index = 0; index < 5; index++)
            {
                CreateCube(
                    exterior.transform,
                    $"Front Curb {index + 1}",
                    new Vector3(
                        -8f + index * 4f,
                        0.04f,
                        -11.3f
                    ),
                    new Vector3(3.9f, 0.18f, 0.4f),
                    materials.concrete,
                    true
                );
            }
        }

        private static void BuildStreetLight(
            Transform parent,
            GarageMaterials materials,
            Vector3 basePosition
        )
        {
            GameObject root =
                new GameObject("Exterior Street Light");

            root.transform.SetParent(
                parent,
                false
            );

            root.transform.position = basePosition;

            CreateLocalCube(
                root.transform,
                "Pole",
                new Vector3(0f, 3f, 0f),
                new Vector3(0.14f, 6f, 0.14f),
                materials.darkMetal,
                true
            );

            CreateLocalCube(
                root.transform,
                "Lamp Arm",
                new Vector3(0.45f, 5.85f, 0f),
                new Vector3(0.9f, 0.1f, 0.1f),
                materials.darkMetal,
                false
            );

            GameObject lamp =
                CreateLocalCube(
                    root.transform,
                    "Lamp Head",
                    new Vector3(0.9f, 5.75f, 0f),
                    new Vector3(0.42f, 0.18f, 0.3f),
                    materials.lightFixture,
                    false
                );

            Light light =
                lamp.AddComponent<Light>();

            light.type = LightType.Point;
            light.range = 9f;
            light.intensity = 1.6f;
            light.color =
                new Color(1f, 0.78f, 0.48f);
            light.shadows = LightShadows.Soft;
        }

        private static void BuildDumpster(
            Transform parent,
            GarageMaterials materials,
            Vector3 position
        )
        {
            GameObject dumpster =
                new GameObject("Dumpster");

            dumpster.transform.SetParent(
                parent,
                false
            );

            dumpster.transform.position = position;

            CreateLocalCube(
                dumpster.transform,
                "Dumpster Body",
                new Vector3(0f, 0.7f, 0f),
                new Vector3(2.4f, 1.4f, 1.4f),
                materials.dumpster,
                true
            );

            CreateLocalCube(
                dumpster.transform,
                "Dumpster Lid",
                new Vector3(0f, 1.47f, 0f),
                new Vector3(2.5f, 0.12f, 1.5f),
                materials.darkMetal,
                true
            );
        }

        private static void BuildPallet(
            Transform parent,
            GarageMaterials materials,
            Vector3 position,
            float yaw
        )
        {
            GameObject pallet =
                new GameObject("Wooden Pallet");

            pallet.transform.SetParent(
                parent,
                false
            );

            pallet.transform.position = position;
            pallet.transform.rotation =
                Quaternion.Euler(0f, yaw, 0f);

            for (int index = 0; index < 6; index++)
            {
                CreateLocalCube(
                    pallet.transform,
                    $"Pallet Slat {index + 1}",
                    new Vector3(
                        -0.5f + index * 0.2f,
                        0f,
                        0f
                    ),
                    new Vector3(0.14f, 0.08f, 0.9f),
                    materials.wood,
                    true
                );
            }

            for (int index = 0; index < 3; index++)
            {
                CreateLocalCube(
                    pallet.transform,
                    $"Pallet Runner {index + 1}",
                    new Vector3(
                        0f,
                        -0.08f,
                        -0.35f + index * 0.35f
                    ),
                    new Vector3(1.2f, 0.08f, 0.1f),
                    materials.wood,
                    true
                );
            }
        }

        private static void BuildBox(
            Transform parent,
            GarageMaterials materials,
            Vector3 position,
            Vector3 size
        )
        {
            CreateCube(
                parent,
                "Cardboard Box",
                position,
                size,
                materials.cardboard,
                true
            );
        }

        private static GameObject CreateIncludedFurniture(
            Transform parent,
            string prefabPath,
            string objectName,
            string assetId,
            Vector3 position,
            Quaternion rotation,
            Vector3 fallbackSize,
            Material fallbackMaterial
        )
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    prefabPath
                );

            GameObject instance;

            if (prefab != null)
            {
                instance =
                    PrefabUtility.InstantiatePrefab(
                        prefab,
                        parent
                    ) as GameObject;

                if (instance == null)
                {
                    instance =
                        UnityEngine.Object.Instantiate(
                            prefab,
                            parent
                        );
                }

                instance.name = objectName;
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.localScale = Vector3.one;

                UnpackPrefabInstance(instance);
                RemovePlacedObjectComponent(instance);
            }
            else
            {
                instance =
                    CreateCube(
                        parent,
                        objectName,
                        position +
                        Vector3.up * (fallbackSize.y * 0.5f),
                        fallbackSize,
                        fallbackMaterial,
                        true
                    );

                instance.transform.rotation = rotation;
            }

            AddStarterAsset(
                instance,
                assetId
            );

            return instance;
        }

        private static void RepositionExistingGameplayObjects(
            Transform deliveryDrop,
            Transform supplierTerminalPoint
        )
        {
            GameObject delivery =
                GameObject.Find("Furniture Delivery Point");

            if (delivery != null)
            {
                delivery.transform.position =
                    deliveryDrop.position;

                delivery.transform.rotation =
                    deliveryDrop.rotation;
            }

            GameObject terminal =
                GameObject.Find("Furniture Supplier Terminal");

            if (terminal != null)
            {
                terminal.transform.position =
                    supplierTerminalPoint.position;

                terminal.transform.rotation =
                    supplierTerminalPoint.rotation;
            }

            GameObject preview =
                GameObject.Find("CharacterPreviewStation");

            if (preview != null)
            {
                UnityEngine.Object.DestroyImmediate(preview);
            }
        }


        private static void RemoveDevelopmentBlockers()
        {
            Scene scene =
                SceneManager.GetActiveScene();

            if (!scene.IsValid())
            {
                return;
            }

            List<GameObject> targets =
                new List<GameObject>();

            foreach (
                GameObject rootObject
                in scene.GetRootGameObjects()
            )
            {
                CollectDevelopmentBlockers(
                    rootObject.transform,
                    targets
                );
            }

            foreach (GameObject target in targets)
            {
                if (target != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        target
                    );
                }
            }
        }

        private static void CollectDevelopmentBlockers(
            Transform current,
            List<GameObject> targets
        )
        {
            if (current == null)
            {
                return;
            }

            string normalizedName =
                (current.name ?? string.Empty)
                .Trim()
                .Replace("_", " ")
                .ToLowerInvariant();

            if (
                normalizedName == "test interactable" ||
                normalizedName == "fallback visual"
            )
            {
                targets.Add(
                    current.gameObject
                );

                return;
            }

            for (
                int index = 0;
                index < current.childCount;
                index++
            )
            {
                CollectDevelopmentBlockers(
                    current.GetChild(index),
                    targets
                );
            }
        }

        private static void RepositionLooseDevelopmentObjects()
        {
            Scene scene =
                SceneManager.GetActiveScene();

            if (!scene.IsValid())
            {
                return;
            }

            Vector3[] stagingPoints =
            {
                new Vector3(-4.8f, 0.12f, -6.6f),
                new Vector3(-3.6f, 0.12f, -6.6f),
                new Vector3(-2.4f, 0.12f, -6.6f),
                new Vector3(-1.2f, 0.12f, -6.6f),
                new Vector3(0f, 0.12f, -6.6f)
            };

            string[] tokens =
            {
                "pickup",
                "test cube",
                "vendor",
                "supplier terminal",
                "furniture vendor",
                "test_interactable",
                "fallback visual",
                "fallback_visual"
            };

            int movedCount = 0;

            foreach (
                GameObject rootObject
                in scene.GetRootGameObjects()
            )
            {
                MoveMatchingChildren(
                    rootObject.transform,
                    tokens,
                    stagingPoints,
                    ref movedCount
                );
            }
        }

        private static void MoveMatchingChildren(
            Transform current,
            string[] tokens,
            Vector3[] stagingPoints,
            ref int movedCount
        )
        {
            if (
                current == null ||
                current.name == GarageRootName ||
                IsUnderStarterGarage(current)
            )
            {
                return;
            }

            string currentName =
                current.name ?? string.Empty;

            for (int index = 0; index < tokens.Length; index++)
            {
                if (
                    currentName.IndexOf(
                        tokens[index],
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    int pointIndex =
                        Mathf.Min(
                            movedCount,
                            stagingPoints.Length - 1
                        );

                    current.position =
                        stagingPoints[pointIndex];

                    current.rotation =
                        Quaternion.identity;

                    movedCount++;
                    break;
                }
            }

            for (int child = 0; child < current.childCount; child++)
            {
                MoveMatchingChildren(
                    current.GetChild(child),
                    tokens,
                    stagingPoints,
                    ref movedCount
                );
            }
        }

        private static bool IsUnderStarterGarage(
            Transform current
        )
        {
            Transform walker = current;

            while (walker != null)
            {
                if (walker.name == GarageRootName)
                {
                    return true;
                }

                walker = walker.parent;
            }

            return false;
        }

        private static void PositionPlayer(
            FirstPersonController player,
            Transform spawnPoint
        )
        {
            CharacterController controller =
                player.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
            }

            player.transform.position =
                spawnPoint.position;

            player.SetViewRotation(
                0f,
                0f
            );

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private static CleanGarageModules CreateCleanModules(
            GarageMaterials materials
        )
        {
            return new CleanGarageModules
            {
                wall4m =
                    CreateModulePrefab(
                        "PREFAB_CleanWall_4m",
                        new Vector3(4f, 4.2f, 0.25f),
                        new Vector3(0f, 2.1f, 0f),
                        materials.brick,
                        true
                    ),

                roof4x4 =
                    CreateModulePrefab(
                        "PREFAB_CleanRoof_4x4m",
                        new Vector3(4f, 0.28f, 4f),
                        new Vector3(0f, 0.14f, 0f),
                        materials.roof,
                        true
                    ),

                beamVertical =
                    CreateModulePrefab(
                        "PREFAB_CleanBeam_Vertical",
                        new Vector3(0.18f, 3.6f, 0.18f),
                        new Vector3(0f, 1.8f, 0f),
                        materials.darkMetal,
                        true
                    ),

                beamHorizontal =
                    CreateModulePrefab(
                        "PREFAB_CleanBeam_Horizontal",
                        new Vector3(4.45f, 0.2f, 0.2f),
                        Vector3.zero,
                        materials.darkMetal,
                        true
                    )
            };
        }

        private static GameObject CreateModulePrefab(
            string prefabName,
            Vector3 size,
            Vector3 localCenter,
            Material material,
            bool collider
        )
        {
            GameObject root =
                new GameObject(prefabName);

            GameObject visual =
                CreateLocalCube(
                    root.transform,
                    "Visual",
                    localCenter,
                    size,
                    material,
                    collider
                );

            string path =
                ModulePrefabRoot + "/" +
                prefabName + ".prefab";

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void InstantiateModule(
            GameObject prefab,
            Transform parent,
            string objectName,
            Vector3 position,
            Quaternion rotation
        )
        {
            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    prefab,
                    parent
                ) as GameObject;

            if (instance == null)
            {
                instance =
                    UnityEngine.Object.Instantiate(
                        prefab,
                        parent
                    );
            }

            instance.name = objectName;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.one;
        }

        private static void CreateWallSegment(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 size,
            Material material
        )
        {
            CreateCube(
                parent,
                name,
                position,
                size,
                material,
                true
            );
        }

        private static GarageMaterials CreateMaterials()
        {
            return new GarageMaterials
            {
                brick =
                    CreateTexturedMaterial(
                        "MAT_Garage_Brick",
                        TextureRoot + "/Brick_BaseColor.jpg",
                        TextureRoot + "/Brick_Normal.jpg",
                        0.12f,
                        new Vector2(2f, 2f)
                    ),

                concrete =
                    CreateTexturedMaterial(
                        "MAT_Garage_CrackedConcrete",
                        TextureRoot + "/Concrete_BaseColor.jpg",
                        TextureRoot + "/Concrete_Normal.jpg",
                        0.12f
                    ),

                asphalt =
                    CreateTexturedMaterial(
                        "MAT_Garage_Asphalt",
                        TextureRoot + "/Asphalt_BaseColor.jpg",
                        TextureRoot + "/Asphalt_Normal.jpg",
                        0.08f
                    ),

                roof =
                    CreateTexturedMaterial(
                        "MAT_Garage_PaintedRoof",
                        TextureRoot + "/Roof_BaseColor.jpg",
                        TextureRoot + "/Roof_Normal.jpg",
                        0.16f
                    ),

                ceiling =
                    CreateTexturedMaterial(
                        "MAT_Garage_BeamedCeiling",
                        TextureRoot + "/Ceiling_BaseColor.jpg",
                        TextureRoot + "/Ceiling_Normal.jpg",
                        0.1f
                    ),

                darkMetal =
                    CreateColorMaterial(
                        "MAT_Garage_DarkMetal",
                        new Color(0.11f, 0.13f, 0.15f),
                        0.24f,
                        0.7f
                    ),

                garageDoor =
                    CreateColorMaterial(
                        "MAT_Garage_OverheadDoor",
                        new Color(0.18f, 0.21f, 0.23f),
                        0.28f,
                        0.62f
                    ),

                entryDoor =
                    CreateColorMaterial(
                        "MAT_Garage_EntryDoor",
                        new Color(0.16f, 0.19f, 0.2f),
                        0.25f,
                        0.55f
                    ),

                lightFixture =
                    CreateColorMaterial(
                        "MAT_Garage_LightFixture",
                        new Color(0.84f, 0.86f, 0.82f),
                        0.4f,
                        0.15f
                    ),

                monitor =
                    CreateColorMaterial(
                        "MAT_Garage_Monitor",
                        new Color(0.04f, 0.05f, 0.06f),
                        0.35f,
                        0.2f
                    ),

                electrical =
                    CreateColorMaterial(
                        "MAT_Garage_Electrical",
                        new Color(0.29f, 0.31f, 0.3f),
                        0.22f,
                        0.58f
                    ),

                switchBox =
                    CreateTexturedMaterial(
                        "MAT_Garage_SwitchBox",
                        TextureRoot + "/SwitchBox_BaseColor.jpg",
                        TextureRoot + "/SwitchBox_Normal.png",
                        0.22f
                    ),

                outlet =
                    CreateColorMaterial(
                        "MAT_Garage_Outlet",
                        new Color(0.72f, 0.71f, 0.67f),
                        0.24f,
                        0f
                    ),

                vent =
                    CreateColorMaterial(
                        "MAT_Garage_Vent",
                        new Color(0.38f, 0.4f, 0.4f),
                        0.3f,
                        0.65f
                    ),

                dumpster =
                    CreateColorMaterial(
                        "MAT_Garage_Dumpster",
                        new Color(0.12f, 0.25f, 0.16f),
                        0.12f,
                        0.45f
                    ),

                wood =
                    CreateColorMaterial(
                        "MAT_Garage_Wood",
                        new Color(0.33f, 0.22f, 0.12f),
                        0.12f,
                        0f
                    ),

                cardboard =
                    CreateColorMaterial(
                        "MAT_Garage_Cardboard",
                        new Color(0.48f, 0.33f, 0.18f),
                        0.08f,
                        0f
                    )
            };
        }

        private static Material CreateTexturedMaterial(
            string name,
            string basePath,
            string normalPath,
            float smoothness
        )
        {
            return CreateTexturedMaterial(
                name,
                basePath,
                normalPath,
                smoothness,
                Vector2.one
            );
        }

        private static Material CreateTexturedMaterial(
            string name,
            string basePath,
            string normalPath,
            float smoothness,
            Vector2 tiling
        )
        {
            string path =
                MaterialRoot + "/" + name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    path
                );

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

            Texture2D baseMap =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    basePath
                );

            Texture2D normalMap =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    normalPath
                );

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture(
                    "_BaseMap",
                    baseMap
                );

                material.SetTextureScale(
                    "_BaseMap",
                    tiling
                );
            }
            else
            {
                material.mainTexture = baseMap;
                material.mainTextureScale = tiling;
            }

            if (
                normalMap != null &&
                material.HasProperty("_BumpMap")
            )
            {
                material.SetTexture(
                    "_BumpMap",
                    normalMap
                );

                material.SetTextureScale(
                    "_BumpMap",
                    tiling
                );

                material.EnableKeyword("_NORMALMAP");
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
                material.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateColorMaterial(
            string name,
            Color color,
            float smoothness,
            float metallic
        )
        {
            string path =
                MaterialRoot + "/" + name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    path
                );

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

        private static Shader GetLitShader()
        {
            return
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                ) ??
                Shader.Find("Standard");
        }

        private static GameObject CreateCube(
            Transform parent,
            string name,
            Vector3 worldPosition,
            Vector3 size,
            Material material,
            bool collider
        )
        {
            GameObject cube =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            cube.name = name;
            cube.transform.SetParent(
                parent,
                true
            );

            cube.transform.position = worldPosition;
            cube.transform.rotation = Quaternion.identity;
            cube.transform.localScale = size;

            Renderer renderer =
                cube.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!collider)
            {
                Collider targetCollider =
                    cube.GetComponent<Collider>();

                if (targetCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        targetCollider
                    );
                }
            }

            return cube;
        }

        private static GameObject CreateLocalCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size,
            Material material,
            bool collider
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

            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!collider)
            {
                Collider targetCollider =
                    cube.GetComponent<Collider>();

                if (targetCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        targetCollider
                    );
                }
            }

            return cube;
        }

        private static void SetTextureScale(
            GameObject target,
            Vector2 scale
        )
        {
            Renderer renderer =
                target != null
                    ? target.GetComponent<Renderer>()
                    : null;

            if (
                renderer == null ||
                renderer.sharedMaterial == null
            )
            {
                return;
            }

            MaterialPropertyBlock block =
                new MaterialPropertyBlock();

            renderer.GetPropertyBlock(block);

            block.SetVector(
                "_BaseMap_ST",
                new Vector4(
                    scale.x,
                    scale.y,
                    0f,
                    0f
                )
            );

            renderer.SetPropertyBlock(block);
        }

        private static Transform CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation
        )
        {
            GameObject marker =
                new GameObject(name);

            marker.transform.SetParent(
                parent,
                false
            );

            marker.transform.position = position;
            marker.transform.rotation = rotation;

            return marker.transform;
        }

        private static void AddStarterAsset(
            GameObject target,
            string assetId
        )
        {
            if (target == null)
            {
                return;
            }

            StarterGarageAsset marker =
                target.GetComponent<StarterGarageAsset>();

            if (marker == null)
            {
                marker =
                    target.AddComponent<StarterGarageAsset>();
            }

            marker.Configure(
                assetId,
                false,
                false
            );
        }

        private static void RemovePlacedObjectComponent(
            GameObject target
        )
        {
            if (target == null)
            {
                return;
            }

            PlacedObject placedObject =
                target.GetComponent<PlacedObject>();

            if (placedObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    placedObject
                );
            }
        }

        private static void UnpackPrefabInstance(
            GameObject target
        )
        {
            if (
                target == null ||
                !PrefabUtility.IsPartOfPrefabInstance(target)
            )
            {
                return;
            }

            GameObject root =
                PrefabUtility.GetOutermostPrefabInstanceRoot(
                    target
                );

            if (root != null)
            {
                PrefabUtility.UnpackPrefabInstance(
                    root,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction
                );
            }
        }

        private static void RemovePreviousGarage()
        {
            GameObject existing =
                GameObject.Find(GarageRootName);

            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    existing
                );
            }
        }

        private static void RemoveOldEnvironment()
        {
            GameObject environment =
                GameObject.Find("Environment");

            if (environment != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    environment
                );
            }
        }

        private static void EnsureFolders()
        {
            foreach (
                string folder
                in new[]
                {
                    "Assets/_Project",
                    "Assets/_Project/Art",
                    "Assets/_Project/Art/StarterGarage",
                    SourceRoot,
                    TextureRoot,
                    MaterialRoot,
                    "Assets/_Project/Prefabs",
                    "Assets/_Project/Prefabs/StarterGarage",
                    ModulePrefabRoot
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

            string name =
                Path.GetFileName(path);

            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                name
            );
        }

        private sealed class GarageBuildResult
        {
            public Transform playerSpawn;
            public Transform deliveryDrop;
            public Transform supplierTerminalPoint;
            public StarterGarageDoor overheadDoor;
            public StarterGarageDoor entryDoor;
            public List<Light> interiorLights;
        }

        private sealed class CleanGarageModules
        {
            public GameObject wall4m;
            public GameObject roof4x4;
            public GameObject beamVertical;
            public GameObject beamHorizontal;
        }

        private sealed class GarageMaterials
        {
            public Material brick;
            public Material concrete;
            public Material asphalt;
            public Material roof;
            public Material ceiling;
            public Material darkMetal;
            public Material garageDoor;
            public Material entryDoor;
            public Material lightFixture;
            public Material monitor;
            public Material electrical;
            public Material switchBox;
            public Material outlet;
            public Material vent;
            public Material dumpster;
            public Material wood;
            public Material cardboard;
        }
    }
}
