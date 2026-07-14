using System;
using System.Collections.Generic;
using ShadowSupply.Electrical;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using UnityEngine;

namespace ShadowSupply.Production
{
    [DisallowMultipleComponent]
    public sealed class PoweredWorkbenchProduction :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string workbenchId =
            "starter-workbench";
        [SerializeField] private PlayerInventory inventory;
        [SerializeField]
            private MachinePowerConnection powerConnection;
        [SerializeField]
            private ProductionRecipeDatabase recipeDatabase;
        [SerializeField] private GameObject readyOutputVisual;
        [SerializeField]
            private InteractiveAssemblyController
                assemblyController;

        private readonly List<int> completedStepIndices =
            new List<int>();

        private ProductionRecipe activeRecipe;
        private ItemQuality activeOutputQuality =
            ItemQuality.Standard;
        private float activeOutputCondition = 1f;

        private ItemDefinition pendingOutputItem;
        private int pendingOutputQuantity;
        private ItemQuality pendingOutputQuality =
            ItemQuality.Standard;
        private float pendingOutputCondition = 1f;

        private string statusMessage;
        private float statusUntil;

        private const string InteractionTargetName =
            "Production Interaction Target";

        public event Action StateChanged;

        public string WorkbenchId => workbenchId;
        public PlayerInventory Inventory => inventory;
        public MachinePowerConnection PowerConnection =>
            powerConnection;
        public ProductionRecipeDatabase RecipeDatabase =>
            recipeDatabase;
        public InteractiveAssemblyController
            AssemblyController => assemblyController;
        public ProductionRecipe ActiveRecipe =>
            activeRecipe;
        public bool IsProducing => activeRecipe != null;
        public float RemainingSeconds => 0f;
        public float ActiveDuration => 0f;
        public ItemQuality ActiveOutputQuality =>
            activeOutputQuality;
        public float ActiveOutputCondition =>
            activeOutputCondition;
        public IReadOnlyList<int> CompletedStepIndices =>
            completedStepIndices;

        public bool HasPendingOutput =>
            pendingOutputItem != null &&
            pendingOutputQuantity > 0;

        public ItemDefinition PendingOutputItem =>
            pendingOutputItem;
        public int PendingOutputQuantity =>
            pendingOutputQuantity;
        public ItemQuality PendingOutputQuality =>
            pendingOutputQuality;
        public float PendingOutputCondition =>
            pendingOutputCondition;

        public bool HasPower =>
            powerConnection != null &&
            powerConnection.IsPowered;

        public string StatusMessage =>
            Time.unscaledTime <= statusUntil
                ? statusMessage
                : string.Empty;

        public float Progress01
        {
            get
            {
                int total =
                    GetAssemblyStepCount(
                        activeRecipe
                    );

                return total <= 0
                    ? 0f
                    : Mathf.Clamp01(
                        completedStepIndices.Count /
                        (float)total
                    );
            }
        }

        public string InteractionPrompt
        {
            get
            {
                if (HasPendingOutput)
                {
                    return
                        $"Use workbench — " +
                        $"{pendingOutputItem.DisplayName} ready";
                }

                if (IsProducing)
                {
                    return HasPower
                        ? "Continue interactive assembly"
                        : "Continue assembly — no power";
                }

                return HasPower
                    ? "Use powered workbench"
                    : "Use workbench — no power";
            }
        }

        private void Awake()
        {
            EnsureInteractionTarget();
        }

        private void OnEnable()
        {
            EnsureInteractionTarget();
        }

        public void Configure(
            string stableWorkbenchId,
            PlayerInventory playerInventory,
            MachinePowerConnection machinePower,
            ProductionRecipeDatabase recipes,
            GameObject outputVisual,
            InteractiveAssemblyController
                interactiveController = null
        )
        {
            workbenchId = stableWorkbenchId;
            inventory = playerInventory;
            powerConnection = machinePower;
            recipeDatabase = recipes;
            readyOutputVisual = outputVisual;

            if (interactiveController != null)
            {
                assemblyController =
                    interactiveController;
            }

            EnsureInteractionTarget();
            RefreshOutputVisual();
        }

        public void SetAssemblyController(
            InteractiveAssemblyController controller
        )
        {
            assemblyController = controller;
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                enabled &&
                gameObject.activeInHierarchy &&
                inventory != null &&
                recipeDatabase != null;
        }

        public void Interact(GameObject interactor)
        {
            ProductionWorkbenchHUD hud =
                ProductionWorkbenchHUD.Instance;

            if (hud == null)
            {
                PublishStatus(
                    "Production interface is unavailable.",
                    3f
                );

                return;
            }

            hud.Open(
                this,
                interactor
            );
        }

        public bool CanStartRecipe(
            ProductionRecipe recipe,
            out string reason
        )
        {
            if (recipe == null)
            {
                reason = "No recipe selected.";
                return false;
            }

            if (inventory == null)
            {
                reason =
                    "Player inventory is unavailable.";
                return false;
            }

            if (assemblyController == null)
            {
                reason =
                    "Interactive assembly surface is unavailable.";
                return false;
            }

            if (IsProducing)
            {
                reason =
                    "Finish the active assembly first.";
                return false;
            }

            if (HasPendingOutput)
            {
                reason =
                    "Collect the finished package first.";
                return false;
            }

            if (
                recipe.RequiresPower &&
                !HasPower
            )
            {
                reason =
                    "The workbench has no power.";
                return false;
            }

            foreach (
                ProductionIngredient ingredient
                in recipe.Ingredients
            )
            {
                if (
                    ingredient == null ||
                    ingredient.Item == null
                )
                {
                    reason =
                        "The recipe has an invalid requirement.";
                    return false;
                }

                int owned =
                    inventory.CountItem(
                        ingredient.Item
                    );

                if (owned < ingredient.Quantity)
                {
                    reason =
                        $"Need {ingredient.Quantity}× " +
                        $"{ingredient.Item.DisplayName} " +
                        $"({owned} owned).";

                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        public bool StartRecipe(
            ProductionRecipe recipe,
            out string reason
        )
        {
            if (!CanStartRecipe(recipe, out reason))
            {
                PublishStatus(reason, 4f);
                return false;
            }

            CaptureIngredientQuality(
                recipe,
                out ItemQuality outputQuality,
                out float outputCondition
            );

            foreach (
                ProductionIngredient ingredient
                in recipe.Ingredients
            )
            {
                if (
                    ingredient == null ||
                    !ingredient.Consumed
                )
                {
                    continue;
                }

                if (
                    !inventory.TryRemoveItem(
                        ingredient.Item,
                        ingredient.Quantity,
                        out ItemStack _
                    )
                )
                {
                    reason =
                        "Materials changed before assembly " +
                        "could begin.";

                    PublishStatus(reason, 4f);
                    return false;
                }
            }

            activeRecipe = recipe;
            activeOutputQuality = outputQuality;
            activeOutputCondition = outputCondition;
            completedStepIndices.Clear();

            reason = string.Empty;

            PublishStatus(
                $"Interactive assembly started: " +
                $"{recipe.DisplayName}.",
                3f
            );

            StateChanged?.Invoke();
            return true;
        }

        public bool MarkAssemblyStepComplete(
            int stepIndex
        )
        {
            if (
                activeRecipe == null ||
                stepIndex < 0 ||
                stepIndex >=
                GetAssemblyStepCount(activeRecipe) ||
                completedStepIndices.Contains(
                    stepIndex
                )
            )
            {
                return false;
            }

            completedStepIndices.Add(stepIndex);
            completedStepIndices.Sort();

            PublishStatus(
                $"Assembly step " +
                $"{completedStepIndices.Count}/" +
                $"{GetAssemblyStepCount(activeRecipe)} complete.",
                1.5f
            );

            StateChanged?.Invoke();
            return true;
        }

        public void FinalizeInteractiveAssembly()
        {
            if (
                activeRecipe == null ||
                completedStepIndices.Count <
                GetAssemblyStepCount(activeRecipe)
            )
            {
                PublishStatus(
                    "All parts must be placed before closing.",
                    3f
                );

                return;
            }

            if (
                activeRecipe.RequiresPower &&
                !HasPower
            )
            {
                PublishStatus(
                    "Restore power before sealing the package.",
                    3f
                );

                return;
            }

            pendingOutputItem =
                activeRecipe.OutputItem;
            pendingOutputQuantity =
                activeRecipe.OutputQuantity;
            pendingOutputQuality =
                activeOutputQuality;
            pendingOutputCondition =
                activeOutputCondition;

            string completedName =
                activeRecipe.DisplayName;

            activeRecipe = null;
            completedStepIndices.Clear();

            RefreshOutputVisual();
            assemblyController?.NotifyOutputReady();

            PublishStatus(
                $"{completedName} is ready to collect.",
                5f
            );

            StateChanged?.Invoke();
        }

        public bool CollectPendingOutput(
            out string reason
        )
        {
            if (!HasPendingOutput)
            {
                reason =
                    "There is no finished output to collect.";
                return false;
            }

            if (
                inventory == null ||
                !inventory.HasSpaceFor(
                    pendingOutputItem,
                    pendingOutputQuantity
                )
            )
            {
                reason =
                    "Inventory does not have enough space.";
                PublishStatus(reason, 4f);
                return false;
            }

            int remaining =
                inventory.AddItem(
                    pendingOutputItem,
                    pendingOutputQuantity,
                    pendingOutputQuality,
                    pendingOutputCondition
                );

            if (remaining > 0)
            {
                reason =
                    "The finished package could not be collected.";
                PublishStatus(reason, 4f);
                return false;
            }

            string collectedName =
                pendingOutputItem.DisplayName;

            pendingOutputItem = null;
            pendingOutputQuantity = 0;
            pendingOutputQuality =
                ItemQuality.Standard;
            pendingOutputCondition = 1f;

            RefreshOutputVisual();

            reason = string.Empty;

            PublishStatus(
                $"Collected {collectedName}.",
                4f
            );

            StateChanged?.Invoke();
            return true;
        }

        public int CountOwned(ItemDefinition item)
        {
            return
                inventory != null &&
                item != null
                    ? inventory.CountItem(item)
                    : 0;
        }

        public void RestoreProductionState(
            string activeRecipeId,
            bool hasPendingOutput,
            string pendingItemId,
            int restoredPendingQuantity,
            ItemQuality restoredOutputQuality,
            float restoredOutputCondition,
            IReadOnlyList<int> restoredCompletedSteps,
            ItemDatabase itemDatabase
        )
        {
            activeRecipe = null;
            completedStepIndices.Clear();
            pendingOutputItem = null;
            pendingOutputQuantity = 0;

            activeOutputQuality =
                restoredOutputQuality;
            activeOutputCondition =
                Mathf.Clamp01(
                    restoredOutputCondition
                );

            if (
                !string.IsNullOrWhiteSpace(
                    activeRecipeId
                ) &&
                recipeDatabase != null &&
                recipeDatabase.TryGetRecipe(
                    activeRecipeId,
                    out ProductionRecipe restoredRecipe
                )
            )
            {
                activeRecipe = restoredRecipe;

                int maximumStep =
                    GetAssemblyStepCount(
                        restoredRecipe
                    );

                if (restoredCompletedSteps != null)
                {
                    foreach (
                        int stepIndex
                        in restoredCompletedSteps
                    )
                    {
                        if (
                            stepIndex >= 0 &&
                            stepIndex < maximumStep &&
                            !completedStepIndices.Contains(
                                stepIndex
                            )
                        )
                        {
                            completedStepIndices.Add(
                                stepIndex
                            );
                        }
                    }
                }

                completedStepIndices.Sort();
            }

            if (
                hasPendingOutput &&
                itemDatabase != null &&
                itemDatabase.TryGetItem(
                    pendingItemId,
                    out ItemDefinition restoredItem
                )
            )
            {
                pendingOutputItem = restoredItem;
                pendingOutputQuantity =
                    Mathf.Max(
                        1,
                        restoredPendingQuantity
                    );
                pendingOutputQuality =
                    restoredOutputQuality;
                pendingOutputCondition =
                    Mathf.Clamp01(
                        restoredOutputCondition
                    );
            }

            RefreshOutputVisual();
            assemblyController?.SyncFromWorkbench();
            StateChanged?.Invoke();
        }

        public int GetAssemblyStepCount()
        {
            return GetAssemblyStepCount(
                activeRecipe
            );
        }

        private static int GetAssemblyStepCount(
            ProductionRecipe recipe
        )
        {
            if (recipe == null)
            {
                return 0;
            }

            int total = 0;

            foreach (
                ProductionIngredient ingredient
                in recipe.Ingredients
            )
            {
                if (
                    ingredient != null &&
                    ingredient.Item != null
                )
                {
                    total +=
                        Mathf.Max(
                            1,
                            ingredient.Quantity
                        );
                }
            }

            return total;
        }

        public void EnsureInteractionTarget()
        {
            Transform existing =
                transform.Find(InteractionTargetName);

            GameObject target =
                existing != null
                    ? existing.gameObject
                    : new GameObject(
                        InteractionTargetName
                    );

            target.transform.SetParent(
                transform,
                false
            );

            Bounds localBounds =
                CalculateLocalFurnitureBounds();

            target.transform.localPosition =
                new Vector3(
                    localBounds.center.x,
                    localBounds.max.y + 0.13f,
                    localBounds.center.z
                );

            target.transform.localRotation =
                Quaternion.identity;

            BoxCollider collider =
                target.GetComponent<BoxCollider>();

            if (collider == null)
            {
                collider =
                    target.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.center = Vector3.zero;
            collider.size =
                new Vector3(
                    Mathf.Max(
                        1.25f,
                        localBounds.size.x * 0.94f
                    ),
                    0.42f,
                    Mathf.Max(
                        0.62f,
                        localBounds.size.z * 0.94f
                    )
                );
        }

        private Bounds CalculateLocalFurnitureBounds()
        {
            Renderer[] renderers =
                GetComponentsInChildren<Renderer>(true);

            bool initialized = false;
            Bounds localBounds = new Bounds();

            foreach (Renderer renderer in renderers)
            {
                if (
                    renderer == null ||
                    HasAncestorNamed(
                        renderer.transform,
                        "Interactive Production Surface"
                    ) ||
                    HasAncestorNamed(
                        renderer.transform,
                        "Workbench Production Output"
                    ) ||
                    renderer.name.IndexOf(
                        "Power Indicator",
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                {
                    continue;
                }

                Bounds world = renderer.bounds;
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
                        transform.InverseTransformPoint(corner);

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
                        new Vector3(0f, 0.8f, 0f),
                        new Vector3(2f, 1f, 0.8f)
                    );
            }

            return localBounds;
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

        private void CaptureIngredientQuality(
            ProductionRecipe recipe,
            out ItemQuality quality,
            out float condition
        )
        {
            float qualityTotal = 0f;
            float conditionTotal = 0f;
            int sampledUnits = 0;

            foreach (
                ProductionIngredient ingredient
                in recipe.Ingredients
            )
            {
                if (
                    ingredient == null ||
                    ingredient.Item == null ||
                    !ingredient.Consumed
                )
                {
                    continue;
                }

                int remainingToSample =
                    ingredient.Quantity;

                foreach (
                    InventorySlot slot
                    in inventory.Slots
                )
                {
                    if (
                        remainingToSample <= 0 ||
                        slot == null ||
                        slot.IsEmpty ||
                        slot.Stack.Item !=
                        ingredient.Item
                    )
                    {
                        continue;
                    }

                    int sampled =
                        Mathf.Min(
                            remainingToSample,
                            slot.Stack.Quantity
                        );

                    qualityTotal +=
                        (int)slot.Stack.Quality *
                        sampled;

                    conditionTotal +=
                        slot.Stack.Condition *
                        sampled;

                    sampledUnits += sampled;
                    remainingToSample -= sampled;
                }
            }

            if (sampledUnits <= 0)
            {
                quality = ItemQuality.Standard;
                condition = 1f;
                return;
            }

            float averageQuality =
                qualityTotal / sampledUnits;

            float averageCondition =
                conditionTotal / sampledUnits;

            float qualityScore =
                averageQuality +
                Mathf.Lerp(
                    -0.35f,
                    0.45f,
                    averageCondition
                );

            if (qualityScore < 0.75f)
            {
                quality = ItemQuality.Poor;
            }
            else if (qualityScore < 1.75f)
            {
                quality = ItemQuality.Standard;
            }
            else if (qualityScore < 2.75f)
            {
                quality = ItemQuality.Good;
            }
            else if (qualityScore < 3.75f)
            {
                quality = ItemQuality.Excellent;
            }
            else
            {
                quality = ItemQuality.Masterwork;
            }

            condition =
                Mathf.Clamp01(
                    averageCondition *
                    0.98f +
                    0.02f
                );
        }

        private void RefreshOutputVisual()
        {
            if (readyOutputVisual != null)
            {
                readyOutputVisual.SetActive(
                    HasPendingOutput
                );
            }
        }

        private void PublishStatus(
            string message,
            float duration
        )
        {
            statusMessage = message;
            statusUntil =
                Time.unscaledTime +
                Mathf.Max(
                    0.1f,
                    duration
                );

            Debug.Log(
                $"[Production] {message}",
                this
            );

            StateChanged?.Invoke();
        }
    }
}
