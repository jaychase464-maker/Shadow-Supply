using System;
using System.Collections.Generic;
using System.Linq;
using ShadowSupply.Inventory;
using ShadowSupply.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class Milestone9AVisualIntegrationSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";

        private const string ImportedRoot =
            "Assets/_Project/Art/Imported/Counterfeiting";

        private const string PrefabRoot =
            "Assets/_Project/Prefabs/Production/Counterfeiting/ImportedVisuals";

        private const string MaterialRoot =
            "Assets/_Project/Art/Materials/Counterfeiting/ImportedVisuals";

        private const string ItemDatabasePath =
            "Assets/_Project/Data/Databases/DB_Items.asset";

        private const string SceneVisualRootName =
            "Milestone9A Imported Counterfeiting Visuals";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 9A Visual Model Integration"
        )]
        public static void ApplyVisualIntegration()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single
            );

            CounterfeitPressStation station =
                UnityEngine.Object.FindFirstObjectByType<
                    CounterfeitPressStation
                >(FindObjectsInactive.Include);

            CounterfeitPressInteractionController controller =
                UnityEngine.Object.FindFirstObjectByType<
                    CounterfeitPressInteractionController
                >(FindObjectsInactive.Include);

            ItemDatabase database =
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(
                    ItemDatabasePath
                );

            if (
                !scene.IsValid() ||
                station == null ||
                controller == null ||
                database == null
            )
            {
                ShowError(
                    "Milestone 9A must be installed and its development " +
                    "scene must contain the counterfeit press, process " +
                    "controller, and item database."
                );
                return;
            }

            EnsureFolder(PrefabRoot);
            EnsureFolder(MaterialRoot);

            VisualDescriptor[] descriptors = BuildDescriptors();
            Dictionary<string, GameObject> prefabs =
                new Dictionary<string, GameObject>(
                    StringComparer.Ordinal
                );

            foreach (VisualDescriptor descriptor in descriptors)
            {
                ConfigureModelImporter(descriptor.ModelPath);
                ConfigureTextureImporter(
                    descriptor.BaseColorPath,
                    false,
                    descriptor.MaximumTextureSize
                );
                ConfigureTextureImporter(
                    descriptor.NormalPath,
                    true,
                    descriptor.MaximumTextureSize
                );
            }

            AssetDatabase.Refresh(
                ImportAssetOptions.ForceSynchronousImport
            );

            foreach (VisualDescriptor descriptor in descriptors)
            {
                Material material = CreateOrUpdateMaterial(
                    descriptor
                );

                GameObject prefab = CreateOrUpdateVisualPrefab(
                    descriptor,
                    material
                );

                if (prefab == null)
                {
                    ShowError(
                        $"Could not create imported visual prefab for " +
                        $"{descriptor.DisplayName}."
                    );
                    return;
                }

                prefabs[descriptor.Key] = prefab;
            }

            UpdateInventoryItems(database, prefabs);

            SerializedObject controllerSerialized =
                new SerializedObject(controller);

            SerializedProperty itemVisualSize =
                controllerSerialized.FindProperty(
                    "itemVisualSize"
                );

            if (itemVisualSize != null)
            {
                itemVisualSize.floatValue = 0.22f;
                controllerSerialized
                    .ApplyModifiedPropertiesWithoutUndo();
            }

            controller.ConfigureImportedVisuals(
                prefabs["PrintedReplicaSheet"],
                prefabs["CutNoteStack"]
            );

            BuildSceneVisuals(
                station,
                controller,
                prefabs
            );

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(station);
            EditorUtility.SetDirty(database);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = station.gameObject;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 9A visual integration applied.\n\n" +
                "All ten Meshy models were imported, optimized, " +
                "converted into normalized prefabs, assigned to their " +
                "inventory items, and connected to the physical " +
                "counterfeit-production sequence.\n\n" +
                "Run the visual validation menu next.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 9A Visual Model Integration"
        )]
        public static void ValidateVisualIntegration()
        {
            CounterfeitPressStation station =
                UnityEngine.Object.FindFirstObjectByType<
                    CounterfeitPressStation
                >(FindObjectsInactive.Include);

            CounterfeitPressInteractionController controller =
                UnityEngine.Object.FindFirstObjectByType<
                    CounterfeitPressInteractionController
                >(FindObjectsInactive.Include);

            ItemDatabase database =
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(
                    ItemDatabasePath
                );

            int prefabCount = 0;
            foreach (VisualDescriptor descriptor in BuildDescriptors())
            {
                if (
                    AssetDatabase.LoadAssetAtPath<GameObject>(
                        descriptor.PrefabPath
                    ) != null
                )
                {
                    prefabCount++;
                }
            }

            Transform sceneVisualRoot =
                controller != null
                    ? FindChildRecursive(
                        controller.transform,
                        SceneVisualRootName
                    )
                    : null;

            bool itemPrefabsReady =
                database != null &&
                HasDisplayPrefab(
                    database,
                    "counterfeit-blank-note-stock",
                    "Blank Note Stock"
                ) &&
                HasDisplayPrefab(
                    database,
                    "counterfeit-pigment-capsule",
                    "Pigment Capsule"
                ) &&
                HasDisplayPrefab(
                    database,
                    "counterfeit-security-film",
                    "Security Film"
                ) &&
                HasDisplayPrefab(
                    database,
                    "production-packaging-material",
                    "Packaging Material"
                ) &&
                HasDisplayPrefab(
                    database,
                    string.Empty,
                    "Basic Toolkit"
                ) &&
                HasDisplayPrefab(
                    database,
                    "counterfeit-replica-note-bundle",
                    "Replica Note Bundle"
                );

            bool valid =
                station != null &&
                controller != null &&
                controller.PrintedSheetVisualPrefab != null &&
                controller.CutNoteStackVisualPrefab != null &&
                prefabCount == 10 &&
                itemPrefabsReady &&
                sceneVisualRoot != null;

            string report =
                "Imported visual prefabs: " +
                prefabCount + " / 10\n" +
                "Inventory display prefabs: " +
                (itemPrefabsReady ? "READY" : "INCOMPLETE") + "\n" +
                "Printed sheet process visual: " +
                (
                    controller != null &&
                    controller.PrintedSheetVisualPrefab != null
                        ? "READY"
                        : "MISSING"
                ) + "\n" +
                "Cut note stack process visual: " +
                (
                    controller != null &&
                    controller.CutNoteStackVisualPrefab != null
                        ? "READY"
                        : "MISSING"
                ) + "\n" +
                "Press and cutting-mat scene visuals: " +
                (sceneVisualRoot != null ? "READY" : "MISSING");

            if (valid)
            {
                Debug.Log(
                    "[Milestone9AVisuals] COUNTERFEIT VISUAL " +
                    "INTEGRATION READY\n" + report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 9A Visual Validation",
                    "COUNTERFEIT VISUAL INTEGRATION READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone9AVisuals] VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 9A Visual Validation",
                    "VALIDATION FAILED\n\n" + report,
                    "OK"
                );
            }
        }

        private static VisualDescriptor[] BuildDescriptors()
        {
            return new[]
            {
                Descriptor(
                    "ImprintPress",
                    "Imprint Press",
                    new Vector3(0.62f, 0.26f, 0.40f),
                    2048,
                    0.22f,
                    0.40f,
                    false
                ),
                Descriptor(
                    "BlankNoteStock",
                    "Blank Note Stock",
                    new Vector3(0.24f, 0.07f, 0.16f),
                    1024,
                    0.02f,
                    0.28f,
                    false
                ),
                Descriptor(
                    "PigmentCapsule",
                    "Pigment Capsule",
                    new Vector3(0.11f, 0.17f, 0.11f),
                    1024,
                    0.08f,
                    0.34f,
                    false
                ),
                Descriptor(
                    "SecurityFilm",
                    "Security Film",
                    new Vector3(0.24f, 0.04f, 0.15f),
                    1024,
                    0.00f,
                    0.64f,
                    true
                ),
                Descriptor(
                    "PrintedReplicaSheet",
                    "Printed Replica Sheet",
                    new Vector3(0.30f, 0.04f, 0.18f),
                    1024,
                    0.00f,
                    0.26f,
                    false
                ),
                Descriptor(
                    "CuttingMat",
                    "Cutting Mat",
                    new Vector3(0.54f, 0.035f, 0.30f),
                    1024,
                    0.00f,
                    0.20f,
                    false
                ),
                Descriptor(
                    "CalibrationToolkit",
                    "Calibration Toolkit",
                    new Vector3(0.22f, 0.11f, 0.16f),
                    1536,
                    0.10f,
                    0.34f,
                    false
                ),
                Descriptor(
                    "CutNoteStack",
                    "Cut Note Stack",
                    new Vector3(0.22f, 0.07f, 0.12f),
                    1024,
                    0.00f,
                    0.24f,
                    false
                ),
                Descriptor(
                    "PackagingSet",
                    "Packaging Set",
                    new Vector3(0.22f, 0.09f, 0.16f),
                    1024,
                    0.03f,
                    0.30f,
                    false
                ),
                Descriptor(
                    "ReplicaNoteBundle",
                    "Replica Note Bundle",
                    new Vector3(0.27f, 0.10f, 0.16f),
                    1536,
                    0.02f,
                    0.36f,
                    false
                )
            };
        }

        private static VisualDescriptor Descriptor(
            string key,
            string displayName,
            Vector3 targetSize,
            int maximumTextureSize,
            float metallic,
            float smoothness,
            bool transparent
        )
        {
            string folder = ImportedRoot + "/" + key;

            return new VisualDescriptor(
                key,
                displayName,
                folder + "/" + key + ".fbx",
                folder + "/" + key + "_BaseColor.jpg",
                folder + "/" + key + "_Normal.png",
                MaterialRoot + "/MAT_" + key + ".mat",
                PrefabRoot + "/PREFAB_" + key + ".prefab",
                targetSize,
                maximumTextureSize,
                metallic,
                smoothness,
                transparent
            );
        }

        private static void ConfigureModelImporter(string path)
        {
            ModelImporter importer =
                AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer == null)
            {
                throw new InvalidOperationException(
                    "Missing imported FBX: " + path
                );
            }

            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode =
                ModelImporterMaterialImportMode.None;
            importer.meshCompression =
                ModelImporterMeshCompression.Medium;
            importer.isReadable = false;
            importer.weldVertices = true;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();
        }

        private static void ConfigureTextureImporter(
            string path,
            bool normalMap,
            int maximumTextureSize
        )
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                throw new InvalidOperationException(
                    "Missing imported texture: " + path
                );
            }

            importer.textureType = normalMap
                ? TextureImporterType.NormalMap
                : TextureImporterType.Default;
            importer.sRGBTexture = !normalMap;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = maximumTextureSize;
            importer.textureCompression =
                TextureImporterCompression.CompressedHQ;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();
        }

        private static Material CreateOrUpdateMaterial(
            VisualDescriptor descriptor
        )
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    descriptor.MaterialPath
                );

            if (material == null)
            {
                Shader shader = Shader.Find(
                    "Universal Render Pipeline/Lit"
                ) ?? Shader.Find("Standard");

                material = new Material(shader)
                {
                    name = "MAT_" + descriptor.Key
                };

                AssetDatabase.CreateAsset(
                    material,
                    descriptor.MaterialPath
                );
            }

            Texture2D baseColor =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    descriptor.BaseColorPath
                );

            Texture2D normal =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    descriptor.NormalPath
                );

            material.SetTexture("_BaseMap", baseColor);
            material.SetTexture("_MainTex", baseColor);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1f);
            material.SetFloat("_Metallic", descriptor.Metallic);
            material.SetFloat("_Smoothness", descriptor.Smoothness);
            material.EnableKeyword("_NORMALMAP");

            Color tint = descriptor.Transparent
                ? new Color(0.52f, 0.82f, 0.88f, 0.56f)
                : Color.white;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            if (descriptor.Transparent)
            {
                ConfigureTransparentMaterial(material);
            }
            else
            {
                ConfigureOpaqueMaterial(material);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTransparentMaterial(
            Material material
        )
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat(
                "_SrcBlend",
                (float)BlendMode.SrcAlpha
            );
            material.SetFloat(
                "_DstBlend",
                (float)BlendMode.OneMinusSrcAlpha
            );
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void ConfigureOpaqueMaterial(
            Material material
        )
        {
            material.SetOverrideTag("RenderType", "Opaque");
            material.SetFloat("_Surface", 0f);
            material.SetFloat(
                "_SrcBlend",
                (float)BlendMode.One
            );
            material.SetFloat(
                "_DstBlend",
                (float)BlendMode.Zero
            );
            material.SetFloat("_ZWrite", 1f);
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_SURFACE_TYPE_OPAQUE");
            material.renderQueue = -1;
        }

        private static GameObject CreateOrUpdateVisualPrefab(
            VisualDescriptor descriptor,
            Material material
        )
        {
            GameObject modelAsset =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    descriptor.ModelPath
                );

            if (modelAsset == null)
            {
                return null;
            }

            GameObject root = new GameObject(
                descriptor.DisplayName + " Visual"
            );

            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    modelAsset,
                    root.transform
                ) as GameObject;

            if (instance == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                return null;
            }

            instance.name = descriptor.DisplayName + " Mesh";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            RemovePhysics(instance);
            AssignMaterial(instance, material);
            ChooseBestOrientation(instance, descriptor.TargetSize);
            NormalizeToTargetBounds(
                instance,
                descriptor.TargetSize
            );

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                root,
                descriptor.PrefabPath
            );

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void ChooseBestOrientation(
            GameObject instance,
            Vector3 targetSize
        )
        {
            Quaternion[] candidates =
            {
                Quaternion.identity,
                Quaternion.Euler(90f, 0f, 0f),
                Quaternion.Euler(-90f, 0f, 0f),
                Quaternion.Euler(0f, 90f, 0f),
                Quaternion.Euler(0f, -90f, 0f),
                Quaternion.Euler(0f, 0f, 90f),
                Quaternion.Euler(0f, 0f, -90f)
            };

            Quaternion best = Quaternion.identity;
            float bestScore = float.MaxValue;

            foreach (Quaternion candidate in candidates)
            {
                instance.transform.localRotation = candidate;
                Bounds bounds = CalculateBounds(instance);

                Vector3 size = bounds.size;
                if (
                    size.x <= 0.0001f ||
                    size.y <= 0.0001f ||
                    size.z <= 0.0001f
                )
                {
                    continue;
                }

                float horizontalScore = Mathf.Abs(
                    Mathf.Log(
                        Mathf.Max(0.0001f, size.x / size.z) /
                        Mathf.Max(
                            0.0001f,
                            targetSize.x / targetSize.z
                        )
                    )
                );

                float sourceHeightRatio =
                    size.y / Mathf.Max(size.x, size.z);

                float targetHeightRatio =
                    targetSize.y /
                    Mathf.Max(targetSize.x, targetSize.z);

                float heightScore = Mathf.Abs(
                    Mathf.Log(
                        Mathf.Max(0.0001f, sourceHeightRatio) /
                        Mathf.Max(0.0001f, targetHeightRatio)
                    )
                );

                float score = horizontalScore + heightScore * 1.35f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            instance.transform.localRotation = best;
        }

        private static void NormalizeToTargetBounds(
            GameObject instance,
            Vector3 targetSize
        )
        {
            Bounds bounds = CalculateBounds(instance);
            Vector3 size = bounds.size;

            if (
                size.x <= 0.0001f ||
                size.y <= 0.0001f ||
                size.z <= 0.0001f
            )
            {
                return;
            }

            float horizontalScale = Mathf.Min(
                targetSize.x / size.x,
                targetSize.z / size.z
            );

            float maximumHeight = targetSize.y * 1.6f;
            float heightScale = maximumHeight / size.y;

            float scale = Mathf.Min(
                horizontalScale,
                heightScale
            );

            instance.transform.localScale =
                Vector3.one * Mathf.Max(0.0001f, scale);

            bounds = CalculateBounds(instance);

            instance.transform.position += new Vector3(
                -bounds.center.x,
                -bounds.min.y,
                -bounds.center.z
            );
        }

        private static Bounds CalculateBounds(GameObject target)
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return new Bounds(
                    target.transform.position,
                    Vector3.one
                );
            }

            Bounds bounds = renderers[0].bounds;

            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static void RemovePhysics(GameObject target)
        {
            foreach (
                Rigidbody body in
                target.GetComponentsInChildren<Rigidbody>(true)
            )
            {
                UnityEngine.Object.DestroyImmediate(body);
            }

            foreach (
                Collider collider in
                target.GetComponentsInChildren<Collider>(true)
            )
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void AssignMaterial(
            GameObject target,
            Material material
        )
        {
            foreach (
                Renderer renderer in
                target.GetComponentsInChildren<Renderer>(true)
            )
            {
                int count = Mathf.Max(
                    1,
                    renderer.sharedMaterials.Length
                );

                renderer.sharedMaterials = Enumerable
                    .Repeat(material, count)
                    .ToArray();
            }
        }

        private static void UpdateInventoryItems(
            ItemDatabase database,
            IReadOnlyDictionary<string, GameObject> prefabs
        )
        {
            AssignItemPrefab(
                database,
                "counterfeit-blank-note-stock",
                "Blank Note Stock",
                prefabs["BlankNoteStock"]
            );

            AssignItemPrefab(
                database,
                "counterfeit-pigment-capsule",
                "Pigment Capsule",
                prefabs["PigmentCapsule"]
            );

            AssignItemPrefab(
                database,
                "counterfeit-security-film",
                "Security Film",
                prefabs["SecurityFilm"]
            );

            AssignItemPrefab(
                database,
                "production-packaging-material",
                "Packaging Material",
                prefabs["PackagingSet"]
            );

            AssignItemPrefab(
                database,
                string.Empty,
                "Basic Toolkit",
                prefabs["CalibrationToolkit"]
            );

            AssignItemPrefab(
                database,
                "counterfeit-replica-note-bundle",
                "Replica Note Bundle",
                prefabs["ReplicaNoteBundle"]
            );
        }

        private static void AssignItemPrefab(
            ItemDatabase database,
            string itemId,
            string displayName,
            GameObject prefab
        )
        {
            ItemDefinition item = FindItem(
                database,
                itemId,
                displayName
            );

            if (item == null)
            {
                throw new InvalidOperationException(
                    "Could not find item definition: " + displayName
                );
            }

            SerializedObject serialized =
                new SerializedObject(item);

            SerializedProperty displayPrefab =
                serialized.FindProperty("displayPrefab");

            SerializedProperty heldScale =
                serialized.FindProperty("heldLocalScale");

            if (displayPrefab != null)
            {
                displayPrefab.objectReferenceValue = prefab;
            }

            if (heldScale != null)
            {
                heldScale.vector3Value = Vector3.one;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static ItemDefinition FindItem(
            ItemDatabase database,
            string itemId,
            string displayName
        )
        {
            if (
                database == null ||
                database.Items == null
            )
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(itemId))
            {
                ItemDefinition byId = database.GetItem(itemId);
                if (byId != null)
                {
                    return byId;
                }
            }

            return database.Items.FirstOrDefault(
                item =>
                    item != null &&
                    string.Equals(
                        item.DisplayName,
                        displayName,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        private static bool HasDisplayPrefab(
            ItemDatabase database,
            string itemId,
            string displayName
        )
        {
            ItemDefinition item = FindItem(
                database,
                itemId,
                displayName
            );

            return item != null && item.DisplayPrefab != null;
        }

        private static void BuildSceneVisuals(
            CounterfeitPressStation station,
            CounterfeitPressInteractionController controller,
            IReadOnlyDictionary<string, GameObject> prefabs
        )
        {
            Transform processSurface = FindChildRecursive(
                controller.transform,
                "Counterfeit Process Surface"
            );

            if (processSurface == null)
            {
                throw new InvalidOperationException(
                    "Counterfeit Process Surface was not found."
                );
            }

            Transform existingRoot = FindChildRecursive(
                processSurface,
                SceneVisualRootName
            );

            if (existingRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    existingRoot.gameObject
                );
            }

            GameObject visualRoot = new GameObject(
                SceneVisualRootName
            );

            visualRoot.transform.SetParent(
                processSurface,
                false
            );

            DisableRenderer(
                processSurface,
                "Imprint Press Body"
            );

            DisableRenderer(
                processSurface,
                "Cutting Mat"
            );

            InstantiateSceneVisual(
                prefabs["ImprintPress"],
                visualRoot.transform,
                "Imported Imprint Press",
                new Vector3(0.38f, 0f, 0.17f),
                Quaternion.identity
            );

            InstantiateSceneVisual(
                prefabs["CuttingMat"],
                visualRoot.transform,
                "Imported Cutting Mat",
                new Vector3(-0.25f, 0.024f, -0.22f),
                Quaternion.identity
            );

            MakeGuideZone(
                processSurface,
                "Paper Tray",
                new Color(1f, 0.38f, 0.02f, 0.34f)
            );

            MakeGuideZone(
                processSurface,
                "Pigment Bay",
                new Color(1f, 0.50f, 0.05f, 0.30f)
            );

            MakeGuideZone(
                processSurface,
                "Security Film Bed",
                new Color(0.20f, 0.78f, 0.88f, 0.30f)
            );

            MakeGuideZone(
                processSurface,
                "Packaging Zone",
                new Color(1f, 0.36f, 0.02f, 0.28f)
            );

            ReplaceReadyOutputVisual(
                station,
                prefabs["ReplicaNoteBundle"]
            );

            EditorUtility.SetDirty(visualRoot);
        }

        private static GameObject InstantiateSceneVisual(
            GameObject prefab,
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation
        )
        {
            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    prefab,
                    parent
                ) as GameObject;

            if (instance == null)
            {
                return null;
            }

            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static void ReplaceReadyOutputVisual(
            CounterfeitPressStation station,
            GameObject outputPrefab
        )
        {
            SerializedObject serialized =
                new SerializedObject(station);

            SerializedProperty property =
                serialized.FindProperty("readyOutputVisual");

            GameObject current = property != null
                ? property.objectReferenceValue as GameObject
                : null;

            Transform parent = station.transform;
            Vector3 localPosition = new Vector3(0.62f, 1.05f, 0f);
            Quaternion localRotation = Quaternion.identity;

            if (current != null)
            {
                parent = current.transform.parent;
                localPosition = current.transform.localPosition;
                localRotation = current.transform.localRotation;
                UnityEngine.Object.DestroyImmediate(current);
            }

            GameObject root = new GameObject(
                "Counterfeit Press Output"
            );

            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;
            root.transform.localRotation = localRotation;

            GameObject visual =
                PrefabUtility.InstantiatePrefab(
                    outputPrefab,
                    root.transform
                ) as GameObject;

            if (visual != null)
            {
                visual.name = "Replica Note Bundle Visual";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;
            }

            root.SetActive(false);

            if (property != null)
            {
                property.objectReferenceValue = root;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void DisableRenderer(
            Transform root,
            string childName
        )
        {
            Transform child = FindChildRecursive(root, childName);
            Renderer renderer = child != null
                ? child.GetComponent<Renderer>()
                : null;

            if (renderer != null)
            {
                renderer.enabled = false;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void MakeGuideZone(
            Transform root,
            string childName,
            Color color
        )
        {
            Transform child = FindChildRecursive(root, childName);
            Renderer renderer = child != null
                ? child.GetComponent<Renderer>()
                : null;

            if (renderer == null)
            {
                return;
            }

            string materialPath =
                MaterialRoot + "/MAT_Guide_" +
                childName.Replace(" ", string.Empty) + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    materialPath
                );

            if (material == null)
            {
                Shader shader = Shader.Find(
                    "Universal Render Pipeline/Lit"
                ) ?? Shader.Find("Standard");

                material = new Material(shader)
                {
                    name = "MAT_Guide_" +
                           childName.Replace(" ", string.Empty)
                };

                AssetDatabase.CreateAsset(
                    material,
                    materialPath
                );
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.12f);
            ConfigureTransparentMaterial(material);

            renderer.sharedMaterial = material;
            renderer.enabled = true;

            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(renderer);
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
                Transform found = FindChildRecursive(
                    child,
                    requestedName
                );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (
                string.IsNullOrWhiteSpace(path) ||
                AssetDatabase.IsValidFolder(path)
            )
            {
                return;
            }

            string parent =
                System.IO.Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            string folderName =
                System.IO.Path.GetFileName(path);

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void ShowError(string message)
        {
            Debug.LogError(
                "[Milestone9AVisuals] " + message
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }

        private readonly struct VisualDescriptor
        {
            public readonly string Key;
            public readonly string DisplayName;
            public readonly string ModelPath;
            public readonly string BaseColorPath;
            public readonly string NormalPath;
            public readonly string MaterialPath;
            public readonly string PrefabPath;
            public readonly Vector3 TargetSize;
            public readonly int MaximumTextureSize;
            public readonly float Metallic;
            public readonly float Smoothness;
            public readonly bool Transparent;

            public VisualDescriptor(
                string key,
                string displayName,
                string modelPath,
                string baseColorPath,
                string normalPath,
                string materialPath,
                string prefabPath,
                Vector3 targetSize,
                int maximumTextureSize,
                float metallic,
                float smoothness,
                bool transparent
            )
            {
                Key = key;
                DisplayName = displayName;
                ModelPath = modelPath;
                BaseColorPath = baseColorPath;
                NormalPath = normalPath;
                MaterialPath = materialPath;
                PrefabPath = prefabPath;
                TargetSize = targetSize;
                MaximumTextureSize = maximumTextureSize;
                Metallic = metallic;
                Smoothness = smoothness;
                Transparent = transparent;
            }
        }
    }
}
