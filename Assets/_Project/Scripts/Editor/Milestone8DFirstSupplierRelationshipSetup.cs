using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShadowSupply.Delivery;
using ShadowSupply.Economy;
using ShadowSupply.Inventory;
using ShadowSupply.Relationships;
using ShadowSupply.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowSupply.Editor
{
    public static class
        Milestone8DFirstSupplierRelationshipSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/" +
            "Dev_Playground.unity";

        private const string GarageName =
            "StarterGarage_Property";

        private const string RootName =
            "Milestone8D_FirstSupplierRelationship";

        private const string ItemDatabasePath =
            "Assets/_Project/Data/Databases/" +
            "DB_Items.asset";

        private const string RelationshipDataRoot =
            "Assets/_Project/Data/Relationships";

        private const string SupplierFolder =
            RelationshipDataRoot +
            "/Suppliers";

        private const string StockFolder =
            RelationshipDataRoot +
            "/SupplierStock";

        private const string SupplierPath =
            SupplierFolder +
            "/SUPPLIER_EliasMercer.asset";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 8D First Supplier Relationship"
        )]
        public static void ApplyMilestoneEightD()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single
                );

            GameObject garage =
                GameObject.Find(GarageName);

            PlayerWallet wallet =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        PlayerWallet
                    >();

            FurnitureDeliverySystem deliverySystem =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        FurnitureDeliverySystem
                    >();

            BuyerNPC mara =
                FindBuyer(
                    "buyer-mara-voss"
                );

            SaveManager saveManager =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        SaveManager
                    >();

            ItemDatabase itemDatabase =
                AssetDatabase.LoadAssetAtPath<
                    ItemDatabase
                >(ItemDatabasePath);

            if (
                !scene.IsValid() ||
                garage == null ||
                wallet == null ||
                deliverySystem == null ||
                mara == null ||
                saveManager == null ||
                itemDatabase == null
            )
            {
                ShowError(
                    "Milestone 8D requires the confirmed " +
                    "starter garage, Mara buyer relationship, " +
                    "wallet, item database, delivery system, " +
                    "and save manager."
                );
                return;
            }

            ItemDefinition packaging =
                FindItem(
                    itemDatabase,
                    "production-packaging-material",
                    "Packaging Material"
                );

            ItemDefinition polymer =
                FindItem(
                    itemDatabase,
                    string.Empty,
                    "Polymer Housing"
                );

            ItemDefinition metal =
                FindItem(
                    itemDatabase,
                    string.Empty,
                    "Metal Components"
                );

            ItemDefinition toolkit =
                FindItem(
                    itemDatabase,
                    string.Empty,
                    "Basic Toolkit"
                );

            List<string> missing =
                new List<string>();

            if (packaging == null)
            {
                missing.Add(
                    "Packaging Material"
                );
            }

            if (polymer == null)
            {
                missing.Add(
                    "Polymer Housing"
                );
            }

            if (metal == null)
            {
                missing.Add(
                    "Metal Components"
                );
            }

            if (toolkit == null)
            {
                missing.Add(
                    "Basic Toolkit"
                );
            }

            if (missing.Count > 0)
            {
                ShowError(
                    "Required production items were not found:\n\n" +
                    string.Join("\n", missing)
                );
                return;
            }

            EnsureFolder(SupplierFolder);
            EnsureFolder(StockFolder);

            SupplierStockDefinition packagingStock =
                CreateStock(
                    StockFolder +
                    "/STOCK_Elias_Packaging.asset",
                    "elias-stock-packaging",
                    "Packaging Material",
                    "Basic wrap, tape, and seals. " +
                    "Elias is willing to sell this immediately.",
                    packaging,
                    8,
                    12,
                    0,
                    -100,
                    -100,
                    -100
                );

            SupplierStockDefinition polymerStock =
                CreateStock(
                    StockFolder +
                    "/STOCK_Elias_Polymer.asset",
                    "elias-stock-polymer",
                    "Polymer Housing",
                    "Consistent molded housings. " +
                    "Available after Elias sees one clean payment.",
                    polymer,
                    5,
                    32,
                    1,
                    -100,
                    4,
                    1
                );

            SupplierStockDefinition metalStock =
                CreateStock(
                    StockFolder +
                    "/STOCK_Elias_Metal.asset",
                    "elias-stock-metal",
                    "Metal Components",
                    "Higher-value components kept off the " +
                    "public catalog until the account proves reliable.",
                    metal,
                    8,
                    40,
                    2,
                    -100,
                    7,
                    3
                );

            SupplierStockDefinition toolkitStock =
                CreateStock(
                    StockFolder +
                    "/STOCK_Elias_Toolkit.asset",
                    "elias-stock-toolkit",
                    "Basic Toolkit",
                    "A complete reusable toolkit. " +
                    "Reserved for established repeat customers.",
                    toolkit,
                    1,
                    180,
                    4,
                    -100,
                    11,
                    5
                );

            SupplierDefinition supplierDefinition =
                AssetDatabase.LoadAssetAtPath<
                    SupplierDefinition
                >(SupplierPath);

            if (supplierDefinition == null)
            {
                supplierDefinition =
                    ScriptableObject.CreateInstance<
                        SupplierDefinition
                    >();

                AssetDatabase.CreateAsset(
                    supplierDefinition,
                    SupplierPath
                );
            }

            supplierDefinition.Configure(
                "supplier-elias-mercer",
                "Elias Mercer",
                "blackwater-south",
                "Blackwater South",
                "A cautious print and materials broker who " +
                "works through referrals. Elias starts with " +
                "small quantities, watches payment history, " +
                "and reserves better inventory for reliable accounts.",
                "buyer-mara-voss",
                "Mara Voss referral",
                0,
                0,
                0,
                new[]
                {
                    packagingStock,
                    polymerStock,
                    metalStock,
                    toolkitStock
                },
                120f,
                8f
            );

            EditorUtility.SetDirty(
                supplierDefinition
            );

            GameObject oldRoot =
                GameObject.Find(RootName);

            if (oldRoot != null)
            {
                UnityEngine.Object
                    .DestroyImmediate(oldRoot);
            }

            GameObject root =
                new GameObject(RootName);

            root.AddComponent<
                SupplierRelationshipHUD
            >();

            GameObject supplierObject =
                new GameObject(
                    "Supplier — Elias Mercer"
                );

            supplierObject.transform.SetParent(
                root.transform,
                false
            );

            supplierObject.transform.position =
                CalculateSupplierPosition(
                    garage,
                    deliverySystem
                );

            Vector3 lookDirection =
                garage.transform.position -
                supplierObject.transform.position;

            lookDirection.y = 0f;

            if (
                lookDirection.sqrMagnitude >
                0.01f
            )
            {
                supplierObject.transform.rotation =
                    Quaternion.LookRotation(
                        lookDirection
                    );
            }

            CapsuleCollider interactionCollider =
                supplierObject.AddComponent<
                    CapsuleCollider
                >();

            interactionCollider.isTrigger = true;
            interactionCollider.center =
                new Vector3(
                    0f,
                    0.92f,
                    0f
                );

            interactionCollider.height = 1.9f;
            interactionCollider.radius = 0.44f;

            CreateSupplierVisual(
                supplierObject.transform
            );

            SupplierNPC supplier =
                supplierObject.AddComponent<
                    SupplierNPC
                >();

            supplier.Configure(
                supplierDefinition,
                wallet,
                deliverySystem
            );

            CreateWorldLabel(
                supplierObject.transform,
                supplier
            );

            EditorUtility.SetDirty(supplier);
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                supplierObject;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 8D applied.\n\n" +
                "Elias Mercer is positioned near the starter " +
                "garage but remains locked until Mara's referral " +
                "is earned.\n\n" +
                "His catalog uses relationship-gated stock, " +
                "clean-cash pricing, limited inventory, restocking, " +
                "delayed physical delivery crates, and save schema 9.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Testing/" +
            "Grant First Supplier Referral (Play Mode)"
        )]
        public static void
            GrantFirstSupplierReferralForTesting()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Shadow Supply",
                    "Enter Play Mode before using the " +
                    "temporary referral override.",
                    "OK"
                );
                return;
            }

            SupplierNPC supplier =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        SupplierNPC
                    >();

            if (supplier == null)
            {
                ShowError(
                    "The first supplier was not found."
                );
                return;
            }

            supplier.GrantReferralForTesting();

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Temporary Play Mode referral granted.\n\n" +
                "This does not modify Mara's relationship " +
                "or persist into the saved scene.",
                "OK"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 8D First Supplier Relationship"
        )]
        public static void ValidateMilestoneEightD()
        {
            SupplierNPC supplier =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        SupplierNPC
                    >();

            SupplierRelationshipHUD hud =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        SupplierRelationshipHUD
                    >();

            FurnitureDeliverySystem deliverySystem =
                UnityEngine.Object
                    .FindFirstObjectByType<
                        FurnitureDeliverySystem
                    >();

            bool valid =
                supplier != null &&
                supplier.Definition != null &&
                supplier.Definition.Stock.Count == 4 &&
                hud != null &&
                deliverySystem != null &&
                SaveManager.CurrentSaveVersion == 9;

            string report =
                "Supplier NPC: " +
                (
                    supplier != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nSupplier definition: " +
                (
                    supplier != null &&
                    supplier.Definition != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nStock definitions: " +
                (
                    supplier != null &&
                    supplier.Definition != null
                        ? supplier.Definition
                            .Stock.Count +
                          " / 4"
                        : "0 / 4"
                ) +
                "\nSupplier HUD: " +
                (
                    hud != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nPhysical delivery system: " +
                (
                    deliverySystem != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nSave schema: " +
                SaveManager.CurrentSaveVersion +
                " / 9";

            if (valid)
            {
                Debug.Log(
                    "[Milestone8D] FIRST SUPPLIER " +
                    "RELATIONSHIP READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8D Validation",
                    "FIRST SUPPLIER RELATIONSHIP READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone8D] VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8D Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static SupplierStockDefinition
            CreateStock(
                string path,
                string stockId,
                string displayName,
                string description,
                ItemDefinition item,
                int maximumStock,
                int basePrice,
                int requiredPurchases,
                int requiredRapport,
                int requiredTrust,
                int requiredRespect
            )
        {
            SupplierStockDefinition stock =
                AssetDatabase.LoadAssetAtPath<
                    SupplierStockDefinition
                >(path);

            if (stock == null)
            {
                stock =
                    ScriptableObject.CreateInstance<
                        SupplierStockDefinition
                    >();

                AssetDatabase.CreateAsset(
                    stock,
                    path
                );
            }

            stock.Configure(
                stockId,
                displayName,
                description,
                item,
                maximumStock,
                basePrice,
                requiredPurchases,
                requiredRapport,
                requiredTrust,
                requiredRespect
            );

            EditorUtility.SetDirty(stock);
            return stock;
        }

        private static ItemDefinition FindItem(
            ItemDatabase database,
            string itemId,
            string displayName
        )
        {
            if (database == null)
            {
                return null;
            }

            if (
                !string.IsNullOrWhiteSpace(
                    itemId
                )
            )
            {
                ItemDefinition byId =
                    database.GetItem(itemId);

                if (byId != null)
                {
                    return byId;
                }
            }

            return
                database.Items.FirstOrDefault(
                    item =>
                        item != null &&
                        string.Equals(
                            item.DisplayName,
                            displayName,
                            StringComparison
                                .OrdinalIgnoreCase
                        )
                );
        }

        private static BuyerNPC FindBuyer(
            string buyerId
        )
        {
            BuyerNPC[] buyers =
                UnityEngine.Object
                    .FindObjectsByType<
                        BuyerNPC
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            return
                buyers.FirstOrDefault(
                    buyer =>
                        buyer != null &&
                        string.Equals(
                            buyer.BuyerId,
                            buyerId,
                            StringComparison.Ordinal
                        )
                );
        }

        private static Vector3
            CalculateSupplierPosition(
                GameObject garage,
                FurnitureDeliverySystem
                    deliverySystem
            )
        {
            Bounds bounds =
                CalculateBounds(garage);

            Vector3 position =
                bounds.center +
                garage.transform.right *
                (
                    Mathf.Max(
                        3.5f,
                        bounds.extents.x +
                        2.8f
                    )
                ) +
                garage.transform.forward *
                1.2f;

            if (
                deliverySystem != null &&
                deliverySystem.DeliveryPoint != null
            )
            {
                Vector3 awayFromDelivery =
                    (
                        position -
                        deliverySystem
                            .DeliveryPoint.position
                    );

                awayFromDelivery.y = 0f;

                if (
                    awayFromDelivery.sqrMagnitude <
                    16f
                )
                {
                    position +=
                        garage.transform.forward *
                        3f;
                }
            }

            Vector3 rayOrigin =
                position +
                Vector3.up * 8f;

            if (
                Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    30f,
                    ~0,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                position.y = hit.point.y;
            }
            else
            {
                position.y = bounds.min.y;
            }

            return position;
        }

        private static void CreateSupplierVisual(
            Transform parent
        )
        {
            GameObject modelPrefab =
                FindSupplierModel();

            if (modelPrefab != null)
            {
                GameObject visual =
                    PrefabUtility
                        .InstantiatePrefab(
                            modelPrefab,
                            parent
                        ) as GameObject;

                if (visual != null)
                {
                    visual.name =
                        "Elias Mercer Visual";

                    visual.transform.localPosition =
                        Vector3.zero;

                    visual.transform.localRotation =
                        Quaternion.Euler(
                            0f,
                            180f,
                            0f
                        );

                    NormalizeVisualHeight(
                        visual,
                        1.8f
                    );

                    foreach (
                        Collider collider
                        in visual
                            .GetComponentsInChildren<
                                Collider
                            >(true)
                    )
                    {
                        UnityEngine.Object
                            .DestroyImmediate(
                                collider
                            );
                    }

                    return;
                }
            }

            Material jacket =
                GetMaterial(
                    "Assets/_Project/Art/Materials/" +
                    "Relationships/" +
                    "MAT_Elias_Jacket.mat",
                    new Color(
                        0.055f,
                        0.075f,
                        0.07f
                    )
                );

            Material skin =
                GetMaterial(
                    "Assets/_Project/Art/Materials/" +
                    "Relationships/" +
                    "MAT_Elias_Skin.mat",
                    new Color(
                        0.42f,
                        0.27f,
                        0.19f
                    )
                );

            Material accent =
                GetMaterial(
                    "Assets/_Project/Art/Materials/" +
                    "Relationships/" +
                    "MAT_Elias_Accent.mat",
                    new Color(
                        0.72f,
                        0.28f,
                        0.025f
                    )
                );

            GameObject body =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );

            body.name =
                "Development Supplier Body";

            body.transform.SetParent(
                parent,
                false
            );

            body.transform.localPosition =
                new Vector3(
                    0f,
                    0.98f,
                    0f
                );

            body.transform.localScale =
                new Vector3(
                    0.54f,
                    0.83f,
                    0.4f
                );

            body.GetComponent<Renderer>()
                .sharedMaterial = jacket;

            UnityEngine.Object.DestroyImmediate(
                body.GetComponent<Collider>()
            );

            GameObject head =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            head.name =
                "Development Supplier Head";

            head.transform.SetParent(
                parent,
                false
            );

            head.transform.localPosition =
                new Vector3(
                    0f,
                    1.79f,
                    0f
                );

            head.transform.localScale =
                new Vector3(
                    0.29f,
                    0.35f,
                    0.29f
                );

            head.GetComponent<Renderer>()
                .sharedMaterial = skin;

            UnityEngine.Object.DestroyImmediate(
                head.GetComponent<Collider>()
            );

            GameObject accentBar =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            accentBar.name =
                "Supplier Jacket Accent";

            accentBar.transform.SetParent(
                parent,
                false
            );

            accentBar.transform.localPosition =
                new Vector3(
                    0f,
                    1.1f,
                    0.38f
                );

            accentBar.transform.localScale =
                new Vector3(
                    0.24f,
                    0.055f,
                    0.02f
                );

            accentBar.GetComponent<Renderer>()
                .sharedMaterial = accent;

            UnityEngine.Object.DestroyImmediate(
                accentBar
                    .GetComponent<Collider>()
            );
        }

        private static GameObject
            FindSupplierModel()
        {
            string[] preferredTerms =
            {
                "supplier_npc",
                "npc_male",
                "male_npc",
                "man_npc"
            };

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:GameObject",
                    new[]
                    {
                        "Assets"
                    }
                );

            foreach (
                string term
                in preferredTerms
            )
            {
                foreach (string guid in guids)
                {
                    string path =
                        AssetDatabase
                            .GUIDToAssetPath(guid);

                    string fileName =
                        Path
                            .GetFileNameWithoutExtension(
                                path
                            );

                    if (
                        fileName.IndexOf(
                            term,
                            StringComparison
                                .OrdinalIgnoreCase
                        ) < 0
                    )
                    {
                        continue;
                    }

                    GameObject candidate =
                        AssetDatabase
                            .LoadAssetAtPath<
                                GameObject
                            >(path);

                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private static void
            NormalizeVisualHeight(
                GameObject visual,
                float targetHeight
            )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<
                    Renderer
                >(true);

            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds =
                renderers[0].bounds;

            for (
                int index = 1;
                index < renderers.Length;
                index++
            )
            {
                bounds.Encapsulate(
                    renderers[index].bounds
                );
            }

            if (
                bounds.size.y <=
                0.0001f
            )
            {
                return;
            }

            visual.transform.localScale *=
                targetHeight /
                bounds.size.y;

            renderers =
                visual.GetComponentsInChildren<
                    Renderer
                >(true);

            bounds =
                renderers[0].bounds;

            for (
                int index = 1;
                index < renderers.Length;
                index++
            )
            {
                bounds.Encapsulate(
                    renderers[index].bounds
                );
            }

            visual.transform.position +=
                Vector3.up *
                (
                    visual.transform.parent
                        .position.y -
                    bounds.min.y
                );
        }

        private static void CreateWorldLabel(
            Transform parent,
            SupplierNPC supplier
        )
        {
            GameObject marker =
                new GameObject(
                    "Supplier World Label"
                );

            marker.transform.SetParent(
                parent,
                false
            );

            marker.transform.localPosition =
                new Vector3(
                    0f,
                    2.25f,
                    0f
                );

            TextMesh text =
                marker.AddComponent<
                    TextMesh
                >();

            text.text =
                "UNKNOWN BROKER\n" +
                "REFERRAL REQUIRED";

            text.anchor =
                TextAnchor.MiddleCenter;

            text.alignment =
                TextAlignment.Center;

            text.characterSize =
                0.075f;

            text.fontSize = 48;

            text.color =
                new Color(
                    0.62f,
                    0.62f,
                    0.62f
                );

            SupplierWorldMarker worldMarker =
                marker.AddComponent<
                    SupplierWorldMarker
                >();

            worldMarker.Configure(
                supplier,
                text
            );
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
                return
                    new Bounds(
                        target.transform.position,
                        new Vector3(
                            8f,
                            4f,
                            8f
                        )
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
                bounds.Encapsulate(
                    renderers[index].bounds
                );
            }

            return bounds;
        }

        private static Material GetMaterial(
            string path,
            Color color
        )
        {
            string folder =
                Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            EnsureFolder(folder);

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
                    new Material(shader);

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

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(
            string path
        )
        {
            if (
                string.IsNullOrWhiteSpace(path) ||
                AssetDatabase.IsValidFolder(path)
            )
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(path);

            EnsureFolder(parent);

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
                "[Milestone8D] " +
                message
            );

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }
    }
}
