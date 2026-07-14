using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Character;
using ShadowSupply.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone6RiggedPlayerSetup
    {
        private const string RiggedModelPath =
            "Assets/_Project/Art/Characters/Player/Rigged/" +
            "PlayerCharacter_Rigged.fbx";

        private const string MaterialFolder =
            "Assets/_Project/Art/Characters/Player/Materials";

        private const string PrefabFolder =
            "Assets/_Project/Prefabs/Characters/Player";

        private const string GameplayPrefabPath =
            PrefabFolder +
            "/PREFAB_PlayerCharacter_Rigged.prefab";

        private const string DataFolder =
            "Assets/_Project/Data/Character/Player";

        private const string DatabasePath =
            DataFolder + "/DB_PlayerCharacterParts.asset";

        private const string VisualRootName =
            "PlayerCharacterVisuals";

        private const float TargetHeight = 1.8f;

        [MenuItem(
            "Shadow Supply/Setup/Apply Milestone 6B Rigged Player Integration"
        )]
        public static void ApplyRiggedPlayerIntegration()
        {
            EnsureFolders();

            Avatar avatar =
                ConfigureRiggedModelImporter();

            if (
                avatar == null ||
                !avatar.isValid ||
                !avatar.isHuman
            )
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Unity could not generate a valid Humanoid Avatar " +
                    "from PlayerCharacter_Rigged.fbx.\n\n" +
                    "Open the FBX Rig tab and confirm:\n" +
                    "Animation Type: Humanoid\n" +
                    "Avatar Definition: Create From This Model\n\n" +
                    "Then inspect Configure Avatar for any red bones.",
                    "OK"
                );

                return;
            }

            CharacterPartDatabase database =
                EnsureCharacterPartDatabase();

            GameObject gameplayPrefab =
                BuildGameplayPrefab(
                    avatar,
                    database
                );

            if (gameplayPrefab == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "The gameplay character prefab could not be built.",
                    "OK"
                );

                return;
            }

            FirstPersonController controller =
                UnityEngine.Object.FindFirstObjectByType<
                    FirstPersonController
                >();

            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "No FirstPersonController was found in the open scene.",
                    "OK"
                );

                return;
            }

            InstallCharacterOnPlayer(
                controller,
                gameplayPrefab
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.MarkSceneDirty(
                SceneManager.GetActiveScene()
            );

            ValidateRiggedPlayer();

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "The rigged player character has been installed.\n\n" +
                "The local body is visible in first person with its head " +
                "removed from the camera view. A second shadows-only body " +
                "preserves the complete character shadow.\n\n" +
                "This pass uses procedural placeholder locomotion until " +
                "production animation clips are added.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/Validate Rigged Player Integration"
        )]
        public static void ValidateRiggedPlayer()
        {
            GameObject model =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    RiggedModelPath
                );

            Avatar avatar =
                AssetDatabase.LoadAllAssetsAtPath(
                    RiggedModelPath
                )
                .OfType<Avatar>()
                .FirstOrDefault();

            int skinnedRendererCount = 0;
            int boneCount = 0;

            if (model != null)
            {
                GameObject instance =
                    PrefabUtility.InstantiatePrefab(
                        model
                    ) as GameObject;

                if (instance == null)
                {
                    instance =
                        UnityEngine.Object.Instantiate(
                            model
                        );
                }

                instance.hideFlags =
                    HideFlags.HideAndDontSave;

                SkinnedMeshRenderer[] skinned =
                    instance.GetComponentsInChildren<SkinnedMeshRenderer>(
                        true
                    );

                skinnedRendererCount =
                    skinned.Length;

                Animator animator =
                    instance.GetComponentInChildren<Animator>(true);

                if (
                    animator != null &&
                    animator.avatar != null &&
                    animator.avatar.isHuman
                )
                {
                    foreach (
                        HumanBodyBones bone
                        in Enum.GetValues(
                            typeof(HumanBodyBones)
                        )
                    )
                    {
                        if (bone == HumanBodyBones.LastBone)
                        {
                            continue;
                        }

                        if (
                            animator.GetBoneTransform(bone) != null
                        )
                        {
                            boneCount++;
                        }
                    }
                }

                UnityEngine.Object.DestroyImmediate(
                    instance
                );
            }

            PlayerCharacterRuntime runtime =
                UnityEngine.Object.FindFirstObjectByType<
                    PlayerCharacterRuntime
                >();

            bool valid =
                model != null &&
                avatar != null &&
                avatar.isValid &&
                avatar.isHuman &&
                skinnedRendererCount > 0 &&
                runtime != null;

            string report =
                valid
                    ? "RIGGED PLAYER READY\n\n" +
                      $"Skinned renderers: {skinnedRendererCount}\n" +
                      $"Mapped humanoid bones: {boneCount}\n" +
                      $"Avatar: {avatar.name}\n" +
                      "Player runtime installed: Yes"
                    : "RIGGED PLAYER INCOMPLETE\n\n" +
                      $"Model found: {model != null}\n" +
                      $"Valid humanoid avatar: " +
                      $"{avatar != null && avatar.isValid && avatar.isHuman}\n" +
                      $"Skinned renderers: {skinnedRendererCount}\n" +
                      $"Player runtime installed: {runtime != null}";

            Debug.Log(
                $"[Milestone6B] {report}"
            );

            EditorUtility.DisplayDialog(
                "Rigged Player Validation",
                report,
                "OK"
            );
        }

        private static Avatar ConfigureRiggedModelImporter()
        {
            ModelImporter importer =
                AssetImporter.GetAtPath(
                    RiggedModelPath
                ) as ModelImporter;

            if (importer == null)
            {
                return null;
            }

            bool changed = false;

            if (
                importer.animationType !=
                ModelImporterAnimationType.Human
            )
            {
                importer.animationType =
                    ModelImporterAnimationType.Human;

                changed = true;
            }

            if (
                importer.avatarSetup !=
                ModelImporterAvatarSetup.CreateFromThisModel
            )
            {
                importer.avatarSetup =
                    ModelImporterAvatarSetup.CreateFromThisModel;

                changed = true;
            }

            if (importer.importAnimation)
            {
                importer.importAnimation = false;
                changed = true;
            }

            if (!importer.importBlendShapes)
            {
                importer.importBlendShapes = true;
                changed = true;
            }

            if (importer.optimizeGameObjects)
            {
                importer.optimizeGameObjects = false;
                changed = true;
            }

            if (importer.importCameras)
            {
                importer.importCameras = false;
                changed = true;
            }

            if (importer.importLights)
            {
                importer.importLights = false;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAllAssetsAtPath(
                RiggedModelPath
            )
            .OfType<Avatar>()
            .FirstOrDefault(
                candidate =>
                    candidate != null &&
                    candidate.isValid &&
                    candidate.isHuman
            );
        }

        private static GameObject BuildGameplayPrefab(
            Avatar avatar,
            CharacterPartDatabase database
        )
        {
            GameObject model =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    RiggedModelPath
                );

            if (model == null)
            {
                return null;
            }

            GameObject root =
                new GameObject(
                    "PlayerCharacter_Rigged"
                );

            GameObject visual =
                PrefabUtility.InstantiatePrefab(
                    model
                ) as GameObject;

            if (visual == null)
            {
                visual =
                    UnityEngine.Object.Instantiate(
                        model
                    );
            }

            UnpackPrefabInstance(visual);

            visual.name = "RiggedModel";
            visual.transform.SetParent(
                root.transform,
                false
            );

            RemoveColliders(visual);
            AssignExistingCharacterMaterials(visual);
            NormalizeCharacter(visual);

            Animator animator =
                visual.GetComponentInChildren<Animator>(true);

            if (animator == null)
            {
                animator =
                    visual.AddComponent<Animator>();
            }

            animator.avatar = avatar;
            animator.applyRootMotion = false;
            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;

            CreateModularPartGroups(
                visual.transform
            );

            ConfigurePartMarkers(
                visual.transform
            );

            CharacterSocketRegistry sockets =
                root.AddComponent<CharacterSocketRegistry>();

            CreateBoneSockets(
                animator,
                sockets
            );

            PlayerCharacterAppearance appearance =
                root.AddComponent<PlayerCharacterAppearance>();

            appearance.Configure(database);
            appearance.RebuildPartCache();

            CharacterRigStatus rigStatus =
                root.AddComponent<CharacterRigStatus>();

            rigStatus.Configure(
                true,
                "Valid Unity Humanoid rig generated from the " +
                "Mixamo skeleton."
            );

            ProceduralHumanoidLocomotion locomotion =
                root.AddComponent<ProceduralHumanoidLocomotion>();

            locomotion.Configure(
                animator,
                null
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    GameplayPrefabPath
                );

            UnityEngine.Object.DestroyImmediate(root);

            return prefab;
        }

        private static void InstallCharacterOnPlayer(
            FirstPersonController controller,
            GameObject gameplayPrefab
        )
        {
            Transform playerTransform =
                controller.transform;

            Transform existing =
                playerTransform.Find(
                    VisualRootName
                );

            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    existing.gameObject
                );
            }

            GameObject visualRoot =
                new GameObject(
                    VisualRootName
                );

            visualRoot.transform.SetParent(
                playerTransform,
                false
            );

            GameObject localBody =
                InstantiateCharacterPrefab(
                    gameplayPrefab,
                    visualRoot.transform,
                    "LocalBody"
                );

            GameObject shadowBody =
                InstantiateCharacterPrefab(
                    gameplayPrefab,
                    visualRoot.transform,
                    "ShadowBody"
                );

            ConfigureSceneBody(
                localBody,
                controller,
                FirstPersonBodyRenderMode.LocalVisible
            );

            ConfigureSceneBody(
                shadowBody,
                controller,
                FirstPersonBodyRenderMode.ShadowOnly
            );

            PlayerCharacterRuntime runtime =
                controller.GetComponent<PlayerCharacterRuntime>();

            if (runtime == null)
            {
                runtime =
                    controller.gameObject.AddComponent<PlayerCharacterRuntime>();
            }

            runtime.Configure(
                localBody.GetComponent<PlayerCharacterAppearance>(),
                shadowBody.GetComponent<PlayerCharacterAppearance>(),
                localBody.GetComponent<CharacterSocketRegistry>(),
                localBody.GetComponent<
                    FirstPersonBodyVisibilityController
                >(),
                shadowBody.GetComponent<
                    FirstPersonBodyVisibilityController
                >()
            );

            EditorUtility.SetDirty(
                controller.gameObject
            );
        }

        private static GameObject InstantiateCharacterPrefab(
            GameObject prefab,
            Transform parent,
            string instanceName
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

            instance.name = instanceName;
            instance.transform.localPosition =
                Vector3.zero;

            instance.transform.localRotation =
                Quaternion.identity;

            instance.transform.localScale =
                Vector3.one;

            return instance;
        }

        private static void ConfigureSceneBody(
            GameObject body,
            FirstPersonController controller,
            FirstPersonBodyRenderMode mode
        )
        {
            Animator animator =
                body.GetComponentInChildren<Animator>(true);

            ProceduralHumanoidLocomotion locomotion =
                body.GetComponent<ProceduralHumanoidLocomotion>();

            if (locomotion == null)
            {
                locomotion =
                    body.AddComponent<ProceduralHumanoidLocomotion>();
            }

            locomotion.Configure(
                animator,
                controller
            );

            FirstPersonBodyVisibilityController visibility =
                body.GetComponent<
                    FirstPersonBodyVisibilityController
                >();

            if (visibility == null)
            {
                visibility =
                    body.AddComponent<
                        FirstPersonBodyVisibilityController
                    >();
            }

            visibility.Configure(
                animator,
                mode
            );

            FirstPersonBodyAlignment alignment =
                body.GetComponent<
                    FirstPersonBodyAlignment
                >();

            if (
                mode ==
                FirstPersonBodyRenderMode.LocalVisible
            )
            {
                if (alignment == null)
                {
                    alignment =
                        body.AddComponent<
                            FirstPersonBodyAlignment
                        >();
                }

                alignment.Configure(controller);
            }
            else if (alignment != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    alignment
                );

                body.transform.localPosition =
                    Vector3.zero;
            }

            HumanoidFootAlignment footAlignment =
                body.GetComponent<
                    HumanoidFootAlignment
                >();

            if (footAlignment == null)
            {
                footAlignment =
                    body.AddComponent<
                        HumanoidFootAlignment
                    >();
            }

            footAlignment.Configure(
                animator,
                controller
            );
        }

        private static void CreateModularPartGroups(
            Transform visualRoot
        )
        {
            Transform baseBody =
                CreateGroup(
                    visualRoot,
                    "Part_BaseBody"
                );

            MoveNamedChild(
                visualRoot,
                baseBody,
                "Body"
            );

            MoveNamedChild(
                visualRoot,
                baseBody,
                "Eye"
            );

            MoveNamedChild(
                visualRoot,
                baseBody,
                "Teeth"
            );

            MoveNamedChild(
                visualRoot,
                baseBody,
                "Tongue"
            );

            MoveNamedChild(
                visualRoot,
                baseBody,
                "eye_laach"
            );

            Transform hair =
                CreateGroup(
                    visualRoot,
                    "Part_Hair"
                );

            MoveNamedChild(
                visualRoot,
                hair,
                "Buzz_Cut"
            );

            Transform facialHair =
                CreateGroup(
                    visualRoot,
                    "Part_FacialHair"
                );

            MoveNamedChild(
                visualRoot,
                facialHair,
                "Chin_Curtain_Sparse"
            );

            Transform underwear =
                CreateGroup(
                    visualRoot,
                    "Part_Underwear"
                );

            MoveNamedChild(
                visualRoot,
                underwear,
                "Briefs_Morph"
            );
        }

        private static void ConfigurePartMarkers(
            Transform visualRoot
        )
        {
            ConfigureMarker(
                visualRoot.Find("Part_BaseBody"),
                "player-base-body-v1",
                "Base Body",
                CharacterSlot.BaseBody,
                CharacterBodyRegion.Head |
                CharacterBodyRegion.Neck |
                CharacterBodyRegion.Torso |
                CharacterBodyRegion.Arms |
                CharacterBodyRegion.Hands |
                CharacterBodyRegion.Hips |
                CharacterBodyRegion.Legs |
                CharacterBodyRegion.Feet
            );

            ConfigureMarker(
                visualRoot.Find("Part_Hair"),
                "player-hair-buzz-cut-v1",
                "Buzz Cut",
                CharacterSlot.Hair,
                CharacterBodyRegion.Head
            );

            ConfigureMarker(
                visualRoot.Find("Part_FacialHair"),
                "player-facial-hair-chin-curtain-v1",
                "Chin Curtain",
                CharacterSlot.FacialHair,
                CharacterBodyRegion.Head
            );

            ConfigureMarker(
                visualRoot.Find("Part_Underwear"),
                "player-underwear-briefs-v1",
                "Briefs",
                CharacterSlot.Underwear,
                CharacterBodyRegion.Hips
            );
        }

        private static void ConfigureMarker(
            Transform target,
            string partId,
            string displayName,
            CharacterSlot slot,
            CharacterBodyRegion regions
        )
        {
            if (target == null)
            {
                return;
            }

            CharacterPartMarker marker =
                target.GetComponent<CharacterPartMarker>();

            if (marker == null)
            {
                marker =
                    target.gameObject.AddComponent<CharacterPartMarker>();
            }

            marker.Configure(
                partId,
                displayName,
                slot,
                regions
            );
        }

        private static void CreateBoneSockets(
            Animator animator,
            CharacterSocketRegistry registry
        )
        {
            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.Head,
                HumanBodyBones.Head
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.Back,
                HumanBodyBones.Chest
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.Chest,
                HumanBodyBones.Chest
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.LeftHand,
                HumanBodyBones.LeftHand
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.RightHand,
                HumanBodyBones.RightHand
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.Hips,
                HumanBodyBones.Hips
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.LeftHip,
                HumanBodyBones.LeftUpperLeg
            );

            CreateBoneSocket(
                animator,
                registry,
                CharacterSocket.RightHip,
                HumanBodyBones.RightUpperLeg
            );
        }

        private static void CreateBoneSocket(
            Animator animator,
            CharacterSocketRegistry registry,
            CharacterSocket socket,
            HumanBodyBones bone
        )
        {
            Transform boneTransform =
                animator.GetBoneTransform(bone);

            if (boneTransform == null)
            {
                return;
            }

            GameObject socketObject =
                new GameObject(
                    "Socket_" + socket
                );

            socketObject.transform.SetParent(
                boneTransform,
                false
            );

            socketObject.transform.localPosition =
                Vector3.zero;

            socketObject.transform.localRotation =
                Quaternion.identity;

            socketObject.transform.localScale =
                Vector3.one;

            registry.Configure(
                socket,
                socketObject.transform
            );
        }

        private static void AssignExistingCharacterMaterials(
            GameObject visual
        )
        {
            Dictionary<string, Material> materials =
                new Dictionary<string, Material>(
                    StringComparer.OrdinalIgnoreCase
                )
                {
                    ["Std_Skin_Head"] =
                        LoadMaterial("MAT_Player_SkinHead"),
                    ["Std_Skin_Body"] =
                        LoadMaterial("MAT_Player_SkinBody"),
                    ["Std_Skin_Arm"] =
                        LoadMaterial("MAT_Player_SkinArms"),
                    ["Std_Skin_Leg"] =
                        LoadMaterial("MAT_Player_SkinLegs"),
                    ["Std_Nails"] =
                        LoadMaterial("MAT_Player_Nails"),
                    ["Std_Eye_L"] =
                        LoadMaterial("MAT_Player_EyeLeft"),
                    ["Std_Eye_R"] =
                        LoadMaterial("MAT_Player_EyeRight"),
                    ["Std_Cornea_L"] =
                        LoadMaterial("MAT_Player_Cornea"),
                    ["Std_Cornea_R"] =
                        LoadMaterial("MAT_Player_Cornea"),
                    ["Std_Upper_Teeth"] =
                        LoadMaterial("MAT_Player_UpperTeeth"),
                    ["Std_Lower_Teeth"] =
                        LoadMaterial("MAT_Player_LowerTeeth"),
                    ["Std_Tongue"] =
                        LoadMaterial("MAT_Player_Tongue"),
                    ["Briefs"] =
                        LoadMaterial("MAT_Player_Briefs"),
                    ["Hair_Transparency"] =
                        LoadMaterial("MAT_Player_Hair"),
                    ["Scalp_Transparency"] =
                        LoadMaterial("MAT_Player_Scalp"),
                    ["Beard_Transparency"] =
                        LoadMaterial("MAT_Player_Beard"),
                    ["Std_Eyelash"] =
                        LoadMaterial("MAT_Player_Eyelashes")
                };

            foreach (
                Renderer targetRenderer
                in visual.GetComponentsInChildren<Renderer>(true)
            )
            {
                Material[] assigned =
                    targetRenderer.sharedMaterials;

                for (
                    int index = 0;
                    index < assigned.Length;
                    index++
                )
                {
                    Material sourceMaterial =
                        assigned[index];

                    if (sourceMaterial == null)
                    {
                        continue;
                    }

                    foreach (
                        KeyValuePair<string, Material> pair
                        in materials
                    )
                    {
                        if (
                            pair.Value != null &&
                            sourceMaterial.name.EndsWith(
                                pair.Key,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        {
                            assigned[index] = pair.Value;
                            break;
                        }
                    }
                }

                targetRenderer.sharedMaterials =
                    assigned;
            }
        }

        private static Material LoadMaterial(
            string assetName
        )
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                MaterialFolder + "/" +
                assetName + ".mat"
            );
        }

        private static CharacterPartDatabase
            EnsureCharacterPartDatabase()
        {
            EnsurePartDefinition(
                "CHARPART_BaseBody.asset",
                "player-base-body-v1",
                "Base Body",
                CharacterSlot.BaseBody,
                CharacterBodyRegion.Head |
                CharacterBodyRegion.Neck |
                CharacterBodyRegion.Torso |
                CharacterBodyRegion.Arms |
                CharacterBodyRegion.Hands |
                CharacterBodyRegion.Hips |
                CharacterBodyRegion.Legs |
                CharacterBodyRegion.Feet,
                "Part_BaseBody"
            );

            CharacterPartDefinition hair =
                EnsurePartDefinition(
                    "CHARPART_BuzzCut.asset",
                    "player-hair-buzz-cut-v1",
                    "Buzz Cut",
                    CharacterSlot.Hair,
                    CharacterBodyRegion.Head,
                    "Part_Hair"
                );

            CharacterPartDefinition facialHair =
                EnsurePartDefinition(
                    "CHARPART_ChinCurtain.asset",
                    "player-facial-hair-chin-curtain-v1",
                    "Chin Curtain",
                    CharacterSlot.FacialHair,
                    CharacterBodyRegion.Head,
                    "Part_FacialHair"
                );

            CharacterPartDefinition underwear =
                EnsurePartDefinition(
                    "CHARPART_Briefs.asset",
                    "player-underwear-briefs-v1",
                    "Briefs",
                    CharacterSlot.Underwear,
                    CharacterBodyRegion.Hips,
                    "Part_Underwear"
                );

            CharacterPartDefinition baseBody =
                AssetDatabase.LoadAssetAtPath<
                    CharacterPartDefinition
                >(
                    DataFolder +
                    "/CHARPART_BaseBody.asset"
                );

            CharacterPartDatabase database =
                AssetDatabase.LoadAssetAtPath<CharacterPartDatabase>(
                    DatabasePath
                );

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<
                        CharacterPartDatabase
                    >();

                AssetDatabase.CreateAsset(
                    database,
                    DatabasePath
                );
            }

            database.SetDefinitions(
                new[]
                {
                    baseBody,
                    hair,
                    facialHair,
                    underwear
                }
            );

            EditorUtility.SetDirty(database);
            return database;
        }

        private static CharacterPartDefinition EnsurePartDefinition(
            string fileName,
            string partId,
            string displayName,
            CharacterSlot slot,
            CharacterBodyRegion regions,
            string sourceObjectName
        )
        {
            string path =
                DataFolder + "/" + fileName;

            CharacterPartDefinition definition =
                AssetDatabase.LoadAssetAtPath<
                    CharacterPartDefinition
                >(path);

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<
                        CharacterPartDefinition
                    >();

                AssetDatabase.CreateAsset(
                    definition,
                    path
                );
            }

            definition.Configure(
                partId,
                displayName,
                slot,
                regions,
                sourceObjectName,
                null
            );

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void NormalizeCharacter(
            GameObject visual
        )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds =
                CalculateBounds(renderers);

            float multiplier =
                TargetHeight /
                Mathf.Max(
                    0.001f,
                    bounds.size.y
                );

            visual.transform.localScale *=
                multiplier;

            renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            bounds =
                CalculateBounds(renderers);

            visual.transform.position +=
                new Vector3(
                    -bounds.center.x,
                    -bounds.min.y,
                    -bounds.center.z
                );
        }

        private static Bounds CalculateBounds(
            Renderer[] renderers
        )
        {
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

        private static void RemoveColliders(
            GameObject root
        )
        {
            foreach (
                Collider targetCollider
                in root.GetComponentsInChildren<Collider>(true)
            )
            {
                UnityEngine.Object.DestroyImmediate(
                    targetCollider
                );
            }
        }

        private static Transform CreateGroup(
            Transform parent,
            string name
        )
        {
            Transform existing =
                parent.Find(name);

            if (existing != null)
            {
                return existing;
            }

            GameObject group =
                new GameObject(name);

            group.transform.SetParent(
                parent,
                false
            );

            return group.transform;
        }

        private static void MoveNamedChild(
            Transform searchRoot,
            Transform destination,
            string objectName
        )
        {
            Transform target =
                FindChildRecursive(
                    searchRoot,
                    objectName
                );

            if (
                target == null ||
                target == destination ||
                target.IsChildOf(destination)
            )
            {
                return;
            }

            target.SetParent(
                destination,
                true
            );
        }

        private static Transform FindChildRecursive(
            Transform root,
            string objectName
        )
        {
            foreach (Transform child in root)
            {
                if (
                    string.Equals(
                        child.name,
                        objectName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return child;
                }

                Transform nested =
                    FindChildRecursive(
                        child,
                        objectName
                    );

                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void UnpackPrefabInstance(
            GameObject instance
        )
        {
            if (
                instance == null ||
                !PrefabUtility.IsPartOfPrefabInstance(instance)
            )
            {
                return;
            }

            GameObject root =
                PrefabUtility.GetOutermostPrefabInstanceRoot(
                    instance
                );

            if (root == null)
            {
                return;
            }

            PrefabUtility.UnpackPrefabInstance(
                root,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction
            );
        }

        private static void EnsureFolders()
        {
            foreach (
                string folder
                in new[]
                {
                    "Assets/_Project",
                    "Assets/_Project/Art",
                    "Assets/_Project/Art/Characters",
                    "Assets/_Project/Art/Characters/Player",
                    "Assets/_Project/Art/Characters/Player/Rigged",
                    "Assets/_Project/Prefabs",
                    "Assets/_Project/Prefabs/Characters",
                    PrefabFolder,
                    "Assets/_Project/Data",
                    "Assets/_Project/Data/Character",
                    DataFolder
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
