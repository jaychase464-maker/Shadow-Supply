using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static class Milestone8CFirstBuyerRelationshipSetup
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Development/Dev_Playground.unity";
        private const string GarageName = "StarterGarage_Property";
        private const string RootName =
            "Milestone8C_FirstBuyerRelationship";
        private const string DataRoot =
            "Assets/_Project/Data/Relationships";
        private const string BuyerFolder = DataRoot + "/Buyers";
        private const string OrderFolder = DataRoot + "/Orders";
        private const string DatabasePath =
            "Assets/_Project/Data/Databases/DB_Items.asset";
        private const string BuyerPath =
            BuyerFolder + "/BUYER_MaraVoss.asset";

        [MenuItem(
            "Shadow Supply/Setup/" +
            "Apply Milestone 8C First Buyer Relationship"
        )]
        public static void ApplyMilestoneEightC()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single
            );

            GameObject garage = GameObject.Find(GarageName);
            PlayerInventory inventory =
                UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
            HotbarController hotbar =
                UnityEngine.Object.FindFirstObjectByType<HotbarController>();
            PlayerWallet wallet =
                UnityEngine.Object.FindFirstObjectByType<PlayerWallet>();
            SaveManager saveManager =
                UnityEngine.Object.FindFirstObjectByType<SaveManager>();

            ItemDatabase itemDatabase =
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(
                    DatabasePath
                );

            if (
                !scene.IsValid() ||
                garage == null ||
                inventory == null ||
                hotbar == null ||
                wallet == null ||
                saveManager == null ||
                itemDatabase == null
            )
            {
                ShowError(
                    "Milestone 8C requires the confirmed starter garage, " +
                    "inventory, wallet, hotbar, item database, and save system."
                );
                return;
            }

            ItemDefinition package =
                itemDatabase.GetItem(
                    "product-sealed-hardware-package"
                );

            if (package == null)
            {
                package = itemDatabase.Items.FirstOrDefault(
                    item =>
                        item != null &&
                        string.Equals(
                            item.DisplayName,
                            "Sealed Hardware Package",
                            StringComparison.OrdinalIgnoreCase
                        )
                );
            }

            if (package == null)
            {
                ShowError(
                    "The Sealed Hardware Package item was not found. " +
                    "Apply the confirmed Milestone 8B production setup first."
                );
                return;
            }

            EnsureFolder(BuyerFolder);
            EnsureFolder(OrderFolder);

            BuyerOrderDefinition trial = CreateOrder(
                OrderFolder + "/ORDER_Mara_TrialPackage.asset",
                "order-mara-trial-package",
                "Trial Package",
                "Mara wants one clean package as proof that your " +
                "garage can produce something worth buying.",
                package,
                1,
                ItemQuality.Standard,
                600f,
                240,
                35,
                0,
                -100,
                -100,
                -100,
                4,
                8,
                6
            );

            BuyerOrderDefinition repeat = CreateOrder(
                OrderFolder + "/ORDER_Mara_RepeatRun.asset",
                "order-mara-repeat-run",
                "Repeat Run",
                "The first handoff was clean. Mara now wants two " +
                "packages without excuses or delays.",
                package,
                2,
                ItemQuality.Standard,
                720f,
                540,
                40,
                1,
                -100,
                8,
                6,
                3,
                8,
                7
            );

            BuyerOrderDefinition network = CreateOrder(
                OrderFolder + "/ORDER_Mara_NetworkTest.asset",
                "order-mara-network-test",
                "Network Test",
                "Mara is considering putting your name in front of " +
                "another contact. She wants three reliable packages first.",
                package,
                3,
                ItemQuality.Standard,
                900f,
                900,
                50,
                2,
                -100,
                16,
                13,
                3,
                8,
                7
            );

            BuyerDefinition buyerDefinition =
                AssetDatabase.LoadAssetAtPath<BuyerDefinition>(
                    BuyerPath
                );

            if (buyerDefinition == null)
            {
                buyerDefinition =
                    ScriptableObject.CreateInstance<BuyerDefinition>();

                AssetDatabase.CreateAsset(
                    buyerDefinition,
                    BuyerPath
                );
            }

            buyerDefinition.Configure(
                "buyer-mara-voss",
                "Mara Voss",
                "blackwater-south",
                "Blackwater South",
                "A cautious neighborhood buyer who values reliable " +
                "work, quiet handoffs, and people who keep their word.",
                0,
                0,
                0,
                new[] { trial, repeat, network },
                25f,
                "contact-print-supply-broker",
                "Print-Supply Broker",
                3,
                24,
                18
            );

            EditorUtility.SetDirty(buyerDefinition);

            GameObject oldRoot = GameObject.Find(RootName);

            if (oldRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(oldRoot);
            }

            GameObject root = new GameObject(RootName);
            root.AddComponent<BuyerRelationshipHUD>();

            GameObject buyerObject =
                new GameObject("Buyer — Mara Voss");

            buyerObject.transform.SetParent(root.transform, false);
            buyerObject.transform.position =
                CalculateBuyerPosition(garage);

            Vector3 lookDirection =
                garage.transform.position -
                buyerObject.transform.position;

            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                buyerObject.transform.rotation =
                    Quaternion.LookRotation(lookDirection);
            }

            CapsuleCollider interactionCollider =
                buyerObject.AddComponent<CapsuleCollider>();

            interactionCollider.isTrigger = true;
            interactionCollider.center =
                new Vector3(0f, 0.92f, 0f);
            interactionCollider.height = 1.9f;
            interactionCollider.radius = 0.42f;

            CreateBuyerVisual(buyerObject.transform);

            BuyerNPC buyer =
                buyerObject.AddComponent<BuyerNPC>();

            buyer.Configure(
                buyerDefinition,
                inventory,
                hotbar,
                wallet
            );

            CreateWorldLabel(buyerObject.transform);

            EditorUtility.SetDirty(buyer);
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = buyerObject;

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                "Milestone 8C applied.\n\n" +
                "Mara Voss is now positioned outside the starter garage. " +
                "Speak to her to make a first impression, accept the trial " +
                "order, produce the requested package, hold it in the hotbar, " +
                "and physically hand it over.\n\n" +
                "Relationship state, active orders, deadlines, referrals, " +
                "and order history now use save schema 8.",
                "Done"
            );
        }

        [MenuItem(
            "Shadow Supply/Validation/" +
            "Validate Milestone 8C First Buyer Relationship"
        )]
        public static void ValidateMilestoneEightC()
        {
            BuyerNPC buyer =
                UnityEngine.Object.FindFirstObjectByType<BuyerNPC>();

            BuyerRelationshipHUD hud =
                UnityEngine.Object.FindFirstObjectByType<
                    BuyerRelationshipHUD
                >();

            bool valid =
                buyer != null &&
                buyer.Definition != null &&
                buyer.Definition.Orders.Count == 3 &&
                hud != null &&
                SaveManager.CurrentSaveVersion == 8;

            string report =
                "Buyer NPC: " +
                (buyer != null ? "OK" : "MISSING") +
                "\nBuyer definition: " +
                (
                    buyer != null && buyer.Definition != null
                        ? "OK"
                        : "MISSING"
                ) +
                "\nOrder definitions: " +
                (
                    buyer != null && buyer.Definition != null
                        ? buyer.Definition.Orders.Count + " / 3"
                        : "0 / 3"
                ) +
                "\nRelationship HUD: " +
                (hud != null ? "OK" : "MISSING") +
                "\nSave schema: " +
                SaveManager.CurrentSaveVersion +
                " / 8";

            if (valid)
            {
                Debug.Log(
                    "[Milestone8C] FIRST BUYER RELATIONSHIP READY\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8C Validation",
                    "FIRST BUYER RELATIONSHIP READY\n\n" +
                    report,
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Milestone8C] VALIDATION FAILED\n" +
                    report
                );

                EditorUtility.DisplayDialog(
                    "Milestone 8C Validation",
                    "VALIDATION FAILED\n\n" +
                    report,
                    "OK"
                );
            }
        }

        private static BuyerOrderDefinition CreateOrder(
            string path,
            string orderId,
            string displayName,
            string description,
            ItemDefinition requestedItem,
            int quantity,
            ItemQuality quality,
            float deadline,
            int reward,
            int qualityBonus,
            int requiredSuccesses,
            int requiredRapport,
            int requiredTrust,
            int requiredRespect,
            int successRapport,
            int successTrust,
            int successRespect
        )
        {
            BuyerOrderDefinition order =
                AssetDatabase.LoadAssetAtPath<BuyerOrderDefinition>(
                    path
                );

            if (order == null)
            {
                order =
                    ScriptableObject.CreateInstance<
                        BuyerOrderDefinition
                    >();

                AssetDatabase.CreateAsset(order, path);
            }

            order.Configure(
                orderId,
                displayName,
                description,
                requestedItem,
                quantity,
                quality,
                deadline,
                reward,
                qualityBonus,
                requiredSuccesses,
                requiredRapport,
                requiredTrust,
                requiredRespect,
                successRapport,
                successTrust,
                successRespect,
                -2,
                -7,
                -3
            );

            EditorUtility.SetDirty(order);
            return order;
        }

        private static Vector3 CalculateBuyerPosition(
            GameObject garage
        )
        {
            Bounds bounds = CalculateBounds(garage);

            Vector3 position =
                bounds.center -
                garage.transform.forward *
                (Mathf.Max(3f, bounds.extents.z + 2.6f)) +
                garage.transform.right * 1.8f;

            Vector3 rayOrigin =
                position + Vector3.up * 8f;

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

        private static void CreateBuyerVisual(Transform parent)
        {
            GameObject modelPrefab = FindBuyerModel();

            if (modelPrefab != null)
            {
                GameObject visual =
                    PrefabUtility.InstantiatePrefab(
                        modelPrefab,
                        parent
                    ) as GameObject;

                if (visual != null)
                {
                    visual.name = "Mara Voss Visual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation =
                        Quaternion.Euler(0f, 180f, 0f);

                    NormalizeVisualHeight(visual, 1.72f);

                    foreach (
                        Collider collider
                        in visual.GetComponentsInChildren<Collider>(
                            true
                        )
                    )
                    {
                        UnityEngine.Object.DestroyImmediate(collider);
                    }

                    return;
                }
            }

            Material coat = GetMaterial(
                "Assets/_Project/Art/Materials/Relationships/" +
                "MAT_Mara_Coat.mat",
                new Color(0.055f, 0.06f, 0.07f)
            );

            Material skin = GetMaterial(
                "Assets/_Project/Art/Materials/Relationships/" +
                "MAT_Mara_Skin.mat",
                new Color(0.38f, 0.22f, 0.16f)
            );

            Material accent = GetMaterial(
                "Assets/_Project/Art/Materials/Relationships/" +
                "MAT_Mara_Accent.mat",
                new Color(0.68f, 0.18f, 0.04f)
            );

            GameObject body =
                GameObject.CreatePrimitive(PrimitiveType.Capsule);

            body.name = "Development Buyer Body";
            body.transform.SetParent(parent, false);
            body.transform.localPosition =
                new Vector3(0f, 0.95f, 0f);
            body.transform.localScale =
                new Vector3(0.48f, 0.78f, 0.36f);
            body.GetComponent<Renderer>().sharedMaterial = coat;
            UnityEngine.Object.DestroyImmediate(
                body.GetComponent<Collider>()
            );

            GameObject head =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);

            head.name = "Development Buyer Head";
            head.transform.SetParent(parent, false);
            head.transform.localPosition =
                new Vector3(0f, 1.72f, 0f);
            head.transform.localScale =
                new Vector3(0.28f, 0.34f, 0.28f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            UnityEngine.Object.DestroyImmediate(
                head.GetComponent<Collider>()
            );

            GameObject stripe =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            stripe.name = "Buyer Jacket Accent";
            stripe.transform.SetParent(parent, false);
            stripe.transform.localPosition =
                new Vector3(0f, 1.08f, 0.345f);
            stripe.transform.localScale =
                new Vector3(0.22f, 0.055f, 0.02f);
            stripe.GetComponent<Renderer>().sharedMaterial = accent;
            UnityEngine.Object.DestroyImmediate(
                stripe.GetComponent<Collider>()
            );
        }

        private static GameObject FindBuyerModel()
        {
            string[] preferredTerms =
            {
                "buyer_npc_female",
                "npc_female",
                "girl18"
            };

            string[] guids = AssetDatabase.FindAssets(
                "t:GameObject",
                new[] { "Assets" }
            );

            foreach (string term in preferredTerms)
            {
                foreach (string guid in guids)
                {
                    string path =
                        AssetDatabase.GUIDToAssetPath(guid);

                    string fileName =
                        Path.GetFileNameWithoutExtension(path);

                    if (
                        fileName.IndexOf(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        ) < 0
                    )
                    {
                        continue;
                    }

                    GameObject candidate =
                        AssetDatabase.LoadAssetAtPath<GameObject>(
                            path
                        );

                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private static void NormalizeVisualHeight(
            GameObject visual,
            float targetHeight
        )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;

            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            if (bounds.size.y <= 0.0001f)
            {
                return;
            }

            visual.transform.localScale *=
                targetHeight / bounds.size.y;

            renderers =
                visual.GetComponentsInChildren<Renderer>(true);

            bounds = renderers[0].bounds;

            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            visual.transform.position +=
                Vector3.up *
                (visual.transform.parent.position.y - bounds.min.y);
        }

        private static void CreateWorldLabel(Transform parent)
        {
            GameObject marker =
                new GameObject("Buyer World Label");

            marker.transform.SetParent(parent, false);
            marker.transform.localPosition =
                new Vector3(0f, 2.18f, 0f);

            TextMesh text = marker.AddComponent<TextMesh>();
            text.text = "MARA VOSS\nBUYER";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.075f;
            text.fontSize = 48;
            text.color = new Color(1f, 0.45f, 0.02f);

            marker.AddComponent<BuyerWorldMarker>();
        }

        private static Bounds CalculateBounds(GameObject target)
        {
            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return new Bounds(
                    target.transform.position,
                    new Vector3(8f, 4f, 8f)
                );
            }

            Bounds bounds = renderers[0].bounds;

            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static Material GetMaterial(
            string path,
            Color color
        )
        {
            string folder =
                Path.GetDirectoryName(path)?.Replace("\\", "/");

            EnsureFolder(folder);

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                Shader shader =
                    Shader.Find("Universal Render Pipeline/Lit") ??
                    Shader.Find("Standard");

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            EditorUtility.SetDirty(material);
            return material;
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
                Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void ShowError(string message)
        {
            Debug.LogError("[Milestone8C] " + message);

            EditorUtility.DisplayDialog(
                "Shadow Supply",
                message,
                "OK"
            );
        }
    }
}
