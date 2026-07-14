using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone6PlayerCharacterSetup
    {
        private const string SourceModelPath =
            "Assets/_Project/Art/Characters/Player/Source/" +
            "PlayerCharacter_Source_Unrigged.fbx";

        private const string TextureFolder =
            "Assets/_Project/Art/Characters/Player/Textures";

        private const string MaterialFolder =
            "Assets/_Project/Art/Characters/Player/Materials";

        private const string PrefabFolder =
            "Assets/_Project/Prefabs/Characters/Player";

        private const string DataFolder =
            "Assets/_Project/Data/Character/Player";

        private const string DatabasePath =
            DataFolder + "/DB_PlayerCharacterParts.asset";

        private const string PreviewPrefabPath =
            PrefabFolder +
            "/PREFAB_PlayerCharacter_SourcePreview.prefab";

        private const string HiddenLayerName =
            "FirstPersonHidden";

        private const float TargetCharacterHeight = 1.82f;

        [MenuItem(
            "Shadow Supply/Setup/Apply Milestone 6 Character Foundation"
        )]
        public static void ApplyMilestoneSix()
        {
            EnsureFolders();
            EnsureLayer(HiddenLayerName);
            ConfigureSourceImporters();

            GameObject sourceModel =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    SourceModelPath
                );

            if (sourceModel == null)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "The player-character FBX could not be found at:\n\n" +
                    SourceModelPath,
                    "OK"
                );
                return;
            }

            Dictionary<string, Material> materials =
                CreateCharacterMaterials();

            CharacterPartDatabase database =
                CreateCharacterPartDatabase();

            GameObject previewPrefab =
                CreateSourcePreviewPrefab(
                    sourceModel,
                    materials,
                    database
                );

            CreateOrRefreshPreviewStation(
                previewPrefab
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ValidatePlayerCharacterRig();

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 6 character asset foundation applied.\n\n" +
                "The supplied FBX contains eight modular mesh objects, " +
                "but no skeleton or skin deformers.\n\n" +
                "A preview prefab, materials, modular part database, " +
                "equipment sockets, and first-person visibility foundation " +
                "have been created.\n\n" +
                "The model is intentionally not attached to the moving " +
                "player until a rigged and skinned FBX is supplied.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/Validate Player Character Rig"
        )]
        public static void ValidatePlayerCharacterRig()
        {
            GameObject sourceModel =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    SourceModelPath
                );

            if (sourceModel == null)
            {
                EditorUtility.DisplayDialog(
                    "Player Character Rig Validation",
                    "Source model not found.",
                    "OK"
                );
                return;
            }

            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    sourceModel
                ) as GameObject;

            if (instance == null)
            {
                instance =
                    UnityEngine.Object.Instantiate(
                        sourceModel
                    );
            }

            instance.hideFlags =
                HideFlags.HideAndDontSave;

            SkinnedMeshRenderer[] skinnedRenderers =
                instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            Animator animator =
                instance.GetComponentInChildren<Animator>(true);

            bool hasValidAvatar =
                animator != null &&
                animator.avatar != null &&
                animator.avatar.isValid;

            bool rigReady =
                skinnedRenderers.Length > 0 &&
                hasValidAvatar;

            string report =
                rigReady
                    ? "RIG READY\n\n" +
                      $"Skinned renderers: {skinnedRenderers.Length}\n" +
                      $"Avatar: {animator.avatar.name}\n\n" +
                      "The character can proceed to gameplay integration."
                    : "RIGGING REQUIRED\n\n" +
                      $"Skinned renderers: {skinnedRenderers.Length}\n" +
                      $"Valid humanoid avatar: {hasValidAvatar}\n\n" +
                      "The supplied source FBX contains separate mesh parts, " +
                      "but it does not contain a skeleton, skin weights, or " +
                      "a valid humanoid avatar.\n\n" +
                      "Do not attach this source model directly to the " +
                      "first-person controller. A rigged version is required " +
                      "for walking, crouching, jumping, clothing deformation, " +
                      "and backpacks that follow the spine.";

            UnityEngine.Object.DestroyImmediate(instance);

            Debug.Log(
                $"[Milestone6] {report}"
            );

            EditorUtility.DisplayDialog(
                "Player Character Rig Validation",
                report,
                "OK"
            );
        }

        private static GameObject CreateSourcePreviewPrefab(
            GameObject sourceModel,
            Dictionary<string, Material> materials,
            CharacterPartDatabase database
        )
        {
            GameObject root =
                new GameObject(
                    "PlayerCharacter_SourcePreview"
                );

            GameObject visual =
                PrefabUtility.InstantiatePrefab(
                    sourceModel
                ) as GameObject;

            if (visual == null)
            {
                visual =
                    UnityEngine.Object.Instantiate(
                        sourceModel
                    );
            }

            UnpackModelPrefabInstance(visual);

            visual.name = "SourceModel";
            visual.transform.SetParent(
                root.transform,
                false
            );

            AssignMaterials(
                visual,
                materials
            );

            NormalizeCharacter(
                visual
            );

            Transform baseBody =
                CreateGroup(
                    root.transform,
                    "Part_BaseBody"
                );

            MoveNamedChild(
                visual.transform,
                baseBody,
                "Body"
            );

            MoveNamedChild(
                visual.transform,
                baseBody,
                "Eye"
            );

            MoveNamedChild(
                visual.transform,
                baseBody,
                "Teeth"
            );

            MoveNamedChild(
                visual.transform,
                baseBody,
                "Tongue"
            );

            MoveNamedChild(
                visual.transform,
                baseBody,
                "eye_laach"
            );

            Transform hair =
                CreateGroup(
                    root.transform,
                    "Part_Hair"
                );

            MoveNamedChild(
                visual.transform,
                hair,
                "Buzz_Cut"
            );

            Transform facialHair =
                CreateGroup(
                    root.transform,
                    "Part_FacialHair"
                );

            MoveNamedChild(
                visual.transform,
                facialHair,
                "Chin_Curtain_Sparse"
            );

            Transform underwear =
                CreateGroup(
                    root.transform,
                    "Part_Underwear"
                );

            MoveNamedChild(
                visual.transform,
                underwear,
                "Briefs_Morph"
            );

            ConfigurePartMarker(
                baseBody,
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

            ConfigurePartMarker(
                hair,
                "player-hair-buzz-cut-v1",
                "Buzz Cut",
                CharacterSlot.Hair,
                CharacterBodyRegion.Head
            );

            ConfigurePartMarker(
                facialHair,
                "player-facial-hair-chin-curtain-v1",
                "Chin Curtain",
                CharacterSlot.FacialHair,
                CharacterBodyRegion.Head
            );

            ConfigurePartMarker(
                underwear,
                "player-underwear-briefs-v1",
                "Briefs",
                CharacterSlot.Underwear,
                CharacterBodyRegion.Hips
            );

            CharacterSocketRegistry socketRegistry =
                root.AddComponent<CharacterSocketRegistry>();

            CreateCharacterSockets(
                root.transform,
                socketRegistry
            );

            PlayerCharacterAppearance appearance =
                root.AddComponent<PlayerCharacterAppearance>();

            appearance.Configure(database);

            CharacterRigStatus rigStatus =
                root.AddComponent<CharacterRigStatus>();

            rigStatus.Configure(
                false,
                "This source model is modular but unrigged. " +
                "It is prepared for external rigging and future " +
                "clothing/backpack integration."
            );

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PreviewPrefabPath
                );

            UnityEngine.Object.DestroyImmediate(root);

            return prefab;
        }

        private static CharacterPartDatabase
            CreateCharacterPartDatabase()
        {
            CharacterPartDefinition baseBody =
                CreatePartDefinition(
                    DataFolder + "/CHARPART_BaseBody.asset",
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
                CreatePartDefinition(
                    DataFolder + "/CHARPART_BuzzCut.asset",
                    "player-hair-buzz-cut-v1",
                    "Buzz Cut",
                    CharacterSlot.Hair,
                    CharacterBodyRegion.Head,
                    "Part_Hair"
                );

            CharacterPartDefinition facialHair =
                CreatePartDefinition(
                    DataFolder + "/CHARPART_ChinCurtain.asset",
                    "player-facial-hair-chin-curtain-v1",
                    "Chin Curtain",
                    CharacterSlot.FacialHair,
                    CharacterBodyRegion.Head,
                    "Part_FacialHair"
                );

            CharacterPartDefinition underwear =
                CreatePartDefinition(
                    DataFolder + "/CHARPART_Briefs.asset",
                    "player-underwear-briefs-v1",
                    "Briefs",
                    CharacterSlot.Underwear,
                    CharacterBodyRegion.Hips,
                    "Part_Underwear"
                );

            CharacterPartDatabase database =
                AssetDatabase.LoadAssetAtPath<CharacterPartDatabase>(
                    DatabasePath
                );

            if (database == null)
            {
                database =
                    ScriptableObject.CreateInstance<CharacterPartDatabase>();

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

        private static CharacterPartDefinition CreatePartDefinition(
            string path,
            string partId,
            string displayName,
            CharacterSlot slot,
            CharacterBodyRegion regions,
            string sourceObjectName
        )
        {
            CharacterPartDefinition definition =
                AssetDatabase.LoadAssetAtPath<CharacterPartDefinition>(
                    path
                );

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<CharacterPartDefinition>();

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

        private static Dictionary<string, Material>
            CreateCharacterMaterials()
        {
            Dictionary<string, Material> materials =
                new Dictionary<string, Material>(
                    StringComparer.OrdinalIgnoreCase
                );

            materials["Std_Skin_Head"] =
                CreateLitMaterial(
                    "MAT_Player_SkinHead",
                    TexturePath("Std_Skin_Head_Diffuse.jpg"),
                    TexturePath("Std_Skin_Head_Normal.png"),
                    0.42f
                );

            materials["Std_Skin_Body"] =
                CreateLitMaterial(
                    "MAT_Player_SkinBody",
                    TexturePath("Std_Skin_Body_Diffuse.jpg"),
                    TexturePath("Std_Skin_Body_Normal.png"),
                    0.4f
                );

            materials["Std_Skin_Arm"] =
                CreateLitMaterial(
                    "MAT_Player_SkinArms",
                    TexturePath("Std_Skin_Arm_Diffuse.jpg"),
                    TexturePath("Std_Skin_Arm_Normal.png"),
                    0.4f
                );

            materials["Std_Skin_Leg"] =
                CreateLitMaterial(
                    "MAT_Player_SkinLegs",
                    TexturePath("Std_Skin_Leg_Diffuse.jpg"),
                    TexturePath("Std_Skin_Leg_Normal.png"),
                    0.38f
                );

            materials["Std_Nails"] =
                CreateLitMaterial(
                    "MAT_Player_Nails",
                    TexturePath("Std_Nails_Diffuse.jpg"),
                    TexturePath("Std_Nails_Normal.png"),
                    0.58f
                );

            materials["Std_Eye_L"] =
                CreateLitMaterial(
                    "MAT_Player_EyeLeft",
                    TexturePath("Std_Eye_L_Diffuse.jpg"),
                    TexturePath("Std_Eye_L_Normal.png"),
                    0.82f
                );

            materials["Std_Eye_R"] =
                CreateLitMaterial(
                    "MAT_Player_EyeRight",
                    TexturePath("Std_Eye_R_Diffuse.jpg"),
                    TexturePath("Std_Eye_R_Normal.png"),
                    0.82f
                );

            Material cornea =
                CreateTransparentCorneaMaterial();

            materials["Std_Cornea_L"] = cornea;
            materials["Std_Cornea_R"] = cornea;

            materials["Std_Upper_Teeth"] =
                CreateLitMaterial(
                    "MAT_Player_UpperTeeth",
                    TexturePath("Std_Upper_Teeth_Diffuse.jpg"),
                    TexturePath("Std_Upper_Teeth_Normal.png"),
                    0.68f
                );

            materials["Std_Lower_Teeth"] =
                CreateLitMaterial(
                    "MAT_Player_LowerTeeth",
                    TexturePath("Std_Lower_Teeth_Diffuse.jpg"),
                    TexturePath("Std_Lower_Teeth_Normal.png"),
                    0.68f
                );

            materials["Std_Tongue"] =
                CreateLitMaterial(
                    "MAT_Player_Tongue",
                    TexturePath("Std_Tongue_Diffuse.jpg"),
                    TexturePath("Std_Tongue_Normal.jpg"),
                    0.58f
                );

            materials["Briefs"] =
                CreateLitMaterial(
                    "MAT_Player_Briefs",
                    TexturePath("Briefs_Diffuse.jpg"),
                    TexturePath("Briefs_Normal.jpg"),
                    0.34f
                );

            materials["Hair_Transparency"] =
                CreateCutoutMaterial(
                    "MAT_Player_Hair",
                    TexturePath("Hair_Transparency_RGBA.png"),
                    0.45f
                );

            materials["Scalp_Transparency"] =
                CreateCutoutMaterial(
                    "MAT_Player_Scalp",
                    TexturePath("Scalp_Transparency_RGBA.png"),
                    0.35f
                );

            materials["Beard_Transparency"] =
                CreateCutoutMaterial(
                    "MAT_Player_Beard",
                    TexturePath("Beard_Transparency_RGBA.png"),
                    0.42f
                );

            materials["Std_Eyelash"] =
                CreateCutoutMaterial(
                    "MAT_Player_Eyelashes",
                    TexturePath("Std_Eyelash_RGBA.png"),
                    0.48f
                );

            return materials;
        }

        private static Material CreateLitMaterial(
            string assetName,
            string baseMapPath,
            string normalPath,
            float smoothness
        )
        {
            string materialPath =
                MaterialFolder + "/" +
                assetName + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    materialPath
                );

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
                        name = assetName
                    };

                AssetDatabase.CreateAsset(
                    material,
                    materialPath
                );
            }

            Texture2D baseMap =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    baseMapPath
                );

            Texture2D normal =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    normalPath
                );

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture(
                    "_BaseMap",
                    baseMap
                );
            }
            else
            {
                material.mainTexture = baseMap;
            }

            if (
                normal != null &&
                material.HasProperty("_BumpMap")
            )
            {
                material.SetTexture(
                    "_BumpMap",
                    normal
                );

                material.EnableKeyword("_NORMALMAP");
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat(
                    "_Smoothness",
                    smoothness
                );
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateCutoutMaterial(
            string assetName,
            string rgbaTexturePath,
            float cutoff
        )
        {
            Material material =
                CreateLitMaterial(
                    assetName,
                    rgbaTexturePath,
                    string.Empty,
                    0.35f
                );

            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", cutoff);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue =
                (int)RenderQueue.AlphaTest;

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateTransparentCorneaMaterial()
        {
            string path =
                MaterialFolder +
                "/MAT_Player_Cornea.mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    path
                );

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
                        name = "MAT_Player_Cornea"
                    };

                AssetDatabase.CreateAsset(
                    material,
                    path
                );
            }

            material.SetColor(
                "_BaseColor",
                new Color(0.8f, 0.9f, 1f, 0.16f)
            );

            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Smoothness", 0.95f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue =
                (int)RenderQueue.Transparent;

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AssignMaterials(
            GameObject root,
            Dictionary<string, Material> materials
        )
        {
            foreach (
                Renderer targetRenderer
                in root.GetComponentsInChildren<Renderer>(true)
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
                    Material existing =
                        assigned[index];

                    if (existing == null)
                    {
                        continue;
                    }

                    string materialName =
                        existing.name.Replace(
                            " (Instance)",
                            string.Empty
                        );

                    if (
                        materials.TryGetValue(
                            materialName,
                            out Material replacement
                        )
                    )
                    {
                        assigned[index] = replacement;
                    }
                }

                targetRenderer.sharedMaterials =
                    assigned;

                targetRenderer.shadowCastingMode =
                    ShadowCastingMode.On;

                targetRenderer.receiveShadows = true;
            }
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

            float height =
                Mathf.Max(
                    0.001f,
                    bounds.size.y
                );

            float multiplier =
                TargetCharacterHeight / height;

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

        private static void CreateCharacterSockets(
            Transform root,
            CharacterSocketRegistry registry
        )
        {
            CreateSocket(
                root,
                registry,
                CharacterSocket.Head,
                new Vector3(0f, 1.72f, 0f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.Back,
                new Vector3(0f, 1.36f, 0.13f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.Chest,
                new Vector3(0f, 1.38f, -0.12f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.LeftHand,
                new Vector3(-0.48f, 1.02f, 0f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.RightHand,
                new Vector3(0.48f, 1.02f, 0f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.Hips,
                new Vector3(0f, 0.94f, 0f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.LeftHip,
                new Vector3(-0.18f, 0.92f, 0f)
            );

            CreateSocket(
                root,
                registry,
                CharacterSocket.RightHip,
                new Vector3(0.18f, 0.92f, 0f)
            );
        }

        private static void CreateSocket(
            Transform root,
            CharacterSocketRegistry registry,
            CharacterSocket socket,
            Vector3 localPosition
        )
        {
            GameObject socketObject =
                new GameObject(
                    "Socket_" + socket
                );

            socketObject.transform.SetParent(
                root,
                false
            );

            socketObject.transform.localPosition =
                localPosition;

            registry.Configure(
                socket,
                socketObject.transform
            );
        }

        private static Transform CreateGroup(
            Transform parent,
            string groupName
        )
        {
            GameObject group =
                new GameObject(groupName);

            group.transform.SetParent(
                parent,
                false
            );

            return group.transform;
        }

        private static void UnpackModelPrefabInstance(
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

            GameObject prefabRoot =
                PrefabUtility.GetOutermostPrefabInstanceRoot(
                    instance
                );

            if (prefabRoot == null)
            {
                return;
            }

            PrefabUtility.UnpackPrefabInstance(
                prefabRoot,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction
            );
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

            if (target != null)
            {
                target.SetParent(
                    destination,
                    true
                );
            }
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

        private static void ConfigurePartMarker(
            Transform group,
            string partId,
            string displayName,
            CharacterSlot slot,
            CharacterBodyRegion regions
        )
        {
            CharacterPartMarker marker =
                group.gameObject.AddComponent<CharacterPartMarker>();

            marker.Configure(
                partId,
                displayName,
                slot,
                regions
            );
        }

        private static void CreateOrRefreshPreviewStation(
            GameObject previewPrefab
        )
        {
            if (
                previewPrefab == null ||
                !SceneManager.GetActiveScene().IsValid()
            )
            {
                return;
            }

            GameObject existing =
                GameObject.Find(
                    "CharacterPreviewStation"
                );

            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    existing
                );
            }

            GameObject station =
                new GameObject(
                    "CharacterPreviewStation"
                );

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            station.transform.position =
                player != null
                    ? player.transform.position +
                      player.transform.right * 3.5f +
                      player.transform.forward * 4f
                    : new Vector3(-5f, 0f, -5f);

            GameObject pedestal =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            pedestal.name = "Preview Pedestal";
            pedestal.transform.SetParent(
                station.transform,
                false
            );

            pedestal.transform.localPosition =
                new Vector3(0f, 0.08f, 0f);

            pedestal.transform.localScale =
                new Vector3(0.85f, 0.08f, 0.85f);

            GameObject preview =
                PrefabUtility.InstantiatePrefab(
                    previewPrefab,
                    station.transform
                ) as GameObject;

            if (preview != null)
            {
                preview.transform.localPosition =
                    new Vector3(0f, 0.16f, 0f);

                preview.transform.localRotation =
                    Quaternion.Euler(0f, 180f, 0f);
            }

            EditorSceneManager.MarkSceneDirty(
                SceneManager.GetActiveScene()
            );
        }

        private static void ConfigureSourceImporters()
        {
            ModelImporter modelImporter =
                AssetImporter.GetAtPath(
                    SourceModelPath
                ) as ModelImporter;

            if (modelImporter != null)
            {
                bool changed = false;

                if (
                    modelImporter.animationType !=
                    ModelImporterAnimationType.None
                )
                {
                    modelImporter.animationType =
                        ModelImporterAnimationType.None;

                    changed = true;
                }

                if (!modelImporter.preserveHierarchy)
                {
                    modelImporter.preserveHierarchy = true;
                    changed = true;
                }

                if (modelImporter.importCameras)
                {
                    modelImporter.importCameras = false;
                    changed = true;
                }

                if (modelImporter.importLights)
                {
                    modelImporter.importLights = false;
                    changed = true;
                }

                if (modelImporter.importBlendShapes == false)
                {
                    modelImporter.importBlendShapes = true;
                    changed = true;
                }

                if (changed)
                {
                    modelImporter.SaveAndReimport();
                }
            }

            ConfigureTexture(
                TexturePath("Std_Skin_Head_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Skin_Body_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Skin_Arm_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Skin_Leg_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Nails_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Eye_L_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Eye_R_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Upper_Teeth_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Lower_Teeth_Normal.png"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Std_Tongue_Normal.jpg"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Briefs_Normal.jpg"),
                true,
                false
            );

            ConfigureTexture(
                TexturePath("Hair_Transparency_RGBA.png"),
                false,
                true
            );

            ConfigureTexture(
                TexturePath("Scalp_Transparency_RGBA.png"),
                false,
                true
            );

            ConfigureTexture(
                TexturePath("Beard_Transparency_RGBA.png"),
                false,
                true
            );

            ConfigureTexture(
                TexturePath("Std_Eyelash_RGBA.png"),
                false,
                true
            );
        }

        private static void ConfigureTexture(
            string path,
            bool normalMap,
            bool alphaTransparency
        )
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path)
                as TextureImporter;

            if (importer == null)
            {
                return;
            }

            bool changed = false;

            TextureImporterType requestedType =
                normalMap
                    ? TextureImporterType.NormalMap
                    : TextureImporterType.Default;

            if (importer.textureType != requestedType)
            {
                importer.textureType =
                    requestedType;

                changed = true;
            }

            if (
                importer.alphaIsTransparency !=
                alphaTransparency
            )
            {
                importer.alphaIsTransparency =
                    alphaTransparency;

                changed = true;
            }

            if (importer.maxTextureSize != 2048)
            {
                importer.maxTextureSize = 2048;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static string TexturePath(
            string fileName
        )
        {
            return TextureFolder + "/" + fileName;
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
                    "Assets/_Project/Art/Characters/Player/Source",
                    TextureFolder,
                    MaterialFolder,
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

        private static void EnsureLayer(
            string layerName
        )
        {
            if (LayerMask.NameToLayer(layerName) >= 0)
            {
                return;
            }

            SerializedObject tagManager =
                new SerializedObject(
                    AssetDatabase.LoadAllAssetsAtPath(
                        "ProjectSettings/TagManager.asset"
                    )[0]
                );

            SerializedProperty layers =
                tagManager.FindProperty("layers");

            for (
                int index = 8;
                index < layers.arraySize;
                index++
            )
            {
                SerializedProperty layer =
                    layers.GetArrayElementAtIndex(index);

                if (string.IsNullOrWhiteSpace(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }

            Debug.LogWarning(
                $"[Milestone6] No free Unity layer was available " +
                $"for '{layerName}'."
            );
        }
    }
}
