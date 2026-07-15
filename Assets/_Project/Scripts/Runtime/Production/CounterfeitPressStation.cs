using System;
using System.Collections.Generic;
using ShadowSupply.Electrical;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using ShadowSupply.Progression;
using UnityEngine;

namespace ShadowSupply.Production
{
    [DisallowMultipleComponent]
    public sealed class CounterfeitPressStation :
        MonoBehaviour,
        IInteractable
    {
        public const int TotalProcessSteps = 9;

        [SerializeField] private string stationId =
            "starter-counterfeit-press";
        [SerializeField] private PlayerInventory inventory;
        [SerializeField]
        private MachinePowerConnection powerConnection;
        [SerializeField]
        private CounterfeitRecipeDefinition recipe;
        [SerializeField]
        private CounterfeitPressInteractionController
            interactionController;
        [SerializeField]
        private IndustryReputationSystem
            reputationSystem;
        [SerializeField]
        private GameObject readyOutputVisual;

        [Header("Runtime")]
        [SerializeField] private bool processActive;
        [SerializeField] private int currentStep;
        [SerializeField] private int mistakeCount;
        [SerializeField] private float printProgress;
        [SerializeField] private float activeBaseQualityScore;
        [SerializeField] private float activeBaseCondition = 1f;

        [SerializeField]
        private ItemDefinition pendingOutputItem;
        [SerializeField] private int pendingOutputQuantity;
        [SerializeField]
        private ItemQuality pendingOutputQuality =
            ItemQuality.Standard;
        [SerializeField]
        private float pendingOutputCondition = 1f;

        private string statusMessage;
        private float statusUntil;

        public event Action StateChanged;

        public string StationId => stationId;
        public PlayerInventory Inventory => inventory;
        public MachinePowerConnection PowerConnection =>
            powerConnection;
        public CounterfeitRecipeDefinition Recipe => recipe;
        public CounterfeitPressInteractionController
            InteractionController =>
                interactionController;
        public IndustryReputationSystem ReputationSystem =>
            reputationSystem;
        public bool ProcessActive => processActive;
        public int CurrentStep => currentStep;
        public int MistakeCount => mistakeCount;
        public float PrintProgress => printProgress;
        public float ActiveBaseQualityScore =>
            activeBaseQualityScore;
        public float ActiveBaseCondition =>
            activeBaseCondition;
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

        public bool HasPendingOutput =>
            pendingOutputItem != null &&
            pendingOutputQuantity > 0;

        public float Progress01 =>
            processActive
                ? Mathf.Clamp01(
                    currentStep /
                    (float)TotalProcessSteps
                )
                : HasPendingOutput
                    ? 1f
                    : 0f;

        public string StatusMessage =>
            Time.unscaledTime <= statusUntil
                ? statusMessage
                : string.Empty;

        public string InteractionPrompt
        {
            get
            {
                if (HasPendingOutput)
                {
                    return
                        $"Use imprint press — " +
                        $"{pendingOutputItem.DisplayName} ready";
                }

                if (processActive)
                {
                    return HasPower
                        ? "Continue counterfeit production"
                        : "Continue production — no power";
                }

                return HasPower
                    ? "Use powered imprint press"
                    : "Use imprint press — no power";
            }
        }

        public string CurrentInstruction
        {
            get
            {
                if (!processActive)
                {
                    return HasPendingOutput
                        ? "Collect the finished replica bundle."
                        : "Start a production run.";
                }

                if (!HasPower)
                {
                    return
                        "Production paused — restore press power.";
                }

                switch (currentStep)
                {
                    case 0:
                        return
                            "Drag Blank Note Stock into the paper tray.";

                    case 1:
                        return
                            "Drag the Pigment Capsule into the pigment bay.";

                    case 2:
                        return
                            "Align the Security Film on the imprint bed.";

                    case 3:
                        return
                            "Hold the illuminated PRINT control until the cycle completes.";

                    case 4:
                        return
                            "Drag the printed sheet onto the cutting mat.";

                    case 5:
                        return
                            "Click the Basic Toolkit to trim the sheet.";

                    case 6:
                        return
                            "Drag the cut note stack into the packaging zone.";

                    case 7:
                        return
                            "Drag Packaging Material over the note stack.";

                    case 8:
                        return
                            "Click SEAL to finish the replica bundle.";

                    default:
                        return "Finalize the production run.";
                }
            }
        }

        private void Awake()
        {
            ResolveReferences();
            RefreshOutputVisual();
        }

        public void Configure(
            string stableStationId,
            PlayerInventory playerInventory,
            MachinePowerConnection machinePower,
            CounterfeitRecipeDefinition
                recipeDefinition,
            CounterfeitPressInteractionController
                controller,
            IndustryReputationSystem
                industryReputation,
            GameObject outputVisual
        )
        {
            stationId = stableStationId;
            inventory = playerInventory;
            powerConnection = machinePower;
            recipe = recipeDefinition;
            interactionController = controller;
            reputationSystem = industryReputation;
            readyOutputVisual = outputVisual;

            if (
                interactionController != null &&
                interactionController.Station != this
            )
            {
                interactionController.SetStation(this);
            }

            RefreshOutputVisual();
        }

        public void SetInteractionController(
            CounterfeitPressInteractionController
                controller
        )
        {
            interactionController = controller;
        }

        public bool CanInteract(GameObject interactor)
        {
            return
                enabled &&
                gameObject.activeInHierarchy &&
                inventory != null &&
                recipe != null &&
                recipe.IsValid();
        }

        public void Interact(GameObject interactor)
        {
            CounterfeitPressHUD hud =
                CounterfeitPressHUD.Instance;

            if (hud == null)
            {
                PublishStatus(
                    "Counterfeit press interface is unavailable.",
                    3f
                );
                return;
            }

            hud.Open(this, interactor);
        }

        public bool CanStart(out string reason)
        {
            ResolveReferences();

            if (recipe == null || !recipe.IsValid())
            {
                reason =
                    "The counterfeit recipe is not configured.";
                return false;
            }

            if (inventory == null)
            {
                reason =
                    "Player inventory is unavailable.";
                return false;
            }

            if (interactionController == null)
            {
                reason =
                    "The physical press surface is unavailable.";
                return false;
            }

            if (processActive)
            {
                reason =
                    "Finish the active production run.";
                return false;
            }

            if (HasPendingOutput)
            {
                reason =
                    "Collect the finished bundle first.";
                return false;
            }

            if (recipe.RequiresPower && !HasPower)
            {
                reason =
                    "The imprint press has no power.";
                return false;
            }

            if (!HasItem(recipe.BlankNoteStock, 1, out reason))
            {
                return false;
            }

            if (!HasItem(recipe.PigmentCapsule, 1, out reason))
            {
                return false;
            }

            if (!HasItem(recipe.SecurityFilm, 1, out reason))
            {
                return false;
            }

            if (!HasItem(recipe.PackagingMaterial, 1, out reason))
            {
                return false;
            }

            if (!HasItem(recipe.BasicToolkit, 1, out reason))
            {
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool StartProduction(out string reason)
        {
            if (!CanStart(out reason))
            {
                PublishStatus(reason, 4f);
                return false;
            }

            CaptureInputQuality(
                out activeBaseQualityScore,
                out activeBaseCondition
            );

            ItemDefinition[] consumed =
            {
                recipe.BlankNoteStock,
                recipe.PigmentCapsule,
                recipe.SecurityFilm,
                recipe.PackagingMaterial
            };

            foreach (ItemDefinition item in consumed)
            {
                if (
                    !inventory.TryRemoveItem(
                        item,
                        1,
                        out ItemStack _
                    )
                )
                {
                    reason =
                        "Materials changed before production began.";
                    PublishStatus(reason, 4f);
                    return false;
                }
            }

            processActive = true;
            currentStep = 0;
            mistakeCount = 0;
            printProgress = 0f;

            interactionController.SyncFromStation();

            reason = string.Empty;

            PublishStatus(
                $"Started {recipe.DisplayName}.",
                4f
            );

            StateChanged?.Invoke();
            return true;
        }

        public bool CompleteStep(
            int expectedStep
        )
        {
            if (
                !processActive ||
                currentStep != expectedStep ||
                expectedStep < 0 ||
                expectedStep >= TotalProcessSteps
            )
            {
                return false;
            }

            if (
                recipe != null &&
                recipe.RequiresPower &&
                !HasPower
            )
            {
                PublishStatus(
                    "Restore press power first.",
                    3f
                );
                return false;
            }

            currentStep++;
            printProgress = 0f;

            PublishStatus(
                $"Counterfeit process step " +
                $"{currentStep}/{TotalProcessSteps} complete.",
                1.5f
            );

            StateChanged?.Invoke();
            return true;
        }

        public void SetPrintProgress(float value)
        {
            if (
                !processActive ||
                currentStep != 3
            )
            {
                printProgress = 0f;
                return;
            }

            printProgress =
                Mathf.Clamp01(value);

            StateChanged?.Invoke();
        }

        public void ShowGuidance(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            PublishStatus(message, 2.5f);
        }

        public void RegisterMistake(string reason)
        {
            if (!processActive)
            {
                return;
            }

            mistakeCount++;

            PublishStatus(
                $"{reason} Quality may be reduced.",
                3f
            );

            StateChanged?.Invoke();
        }

        public bool FinalizeProduction()
        {
            if (
                !processActive ||
                currentStep < TotalProcessSteps
            )
            {
                PublishStatus(
                    "Complete every production step first.",
                    3f
                );
                return false;
            }

            float reputationBonus =
                reputationSystem != null
                    ? reputationSystem
                        .GetCounterfeitingQualityBonus()
                    : 0f;

            float finalScore =
                activeBaseQualityScore +
                reputationBonus -
                mistakeCount * 0.2f;

            pendingOutputQuality =
                QualityFromScore(finalScore);

            pendingOutputCondition =
                Mathf.Clamp01(
                    activeBaseCondition -
                    mistakeCount * 0.035f
                );

            pendingOutputItem =
                recipe.OutputItem;

            pendingOutputQuantity =
                recipe.OutputQuantity;

            int reputationReward =
                Mathf.Max(
                    1,
                    recipe.BaseReputationReward -
                    mistakeCount / 2
                );

            reputationSystem?.AddReputation(
                IndustryReputationSystem
                    .CounterfeitingIndustryId,
                reputationReward
            );

            processActive = false;
            currentStep = 0;
            printProgress = 0f;

            RefreshOutputVisual();
            interactionController?.NotifyOutputReady();

            PublishStatus(
                $"{recipe.DisplayName} finished at " +
                $"{pendingOutputQuality} quality.",
                6f
            );

            StateChanged?.Invoke();
            return true;
        }

        public bool CollectPendingOutput(
            out string reason
        )
        {
            if (!HasPendingOutput)
            {
                reason =
                    "There is no finished counterfeit output.";
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
                    "The finished bundle could not be collected.";
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

        public void ResetToDefaultState()
        {
            processActive = false;
            currentStep = 0;
            mistakeCount = 0;
            printProgress = 0f;
            activeBaseQualityScore = 1f;
            activeBaseCondition = 1f;
            pendingOutputItem = null;
            pendingOutputQuantity = 0;
            pendingOutputQuality =
                ItemQuality.Standard;
            pendingOutputCondition = 1f;

            RefreshOutputVisual();
            interactionController?.SyncFromStation();
            StateChanged?.Invoke();
        }

        public void RestoreState(
            bool restoredProcessActive,
            int restoredCurrentStep,
            int restoredMistakes,
            float restoredPrintProgress,
            float restoredBaseQualityScore,
            float restoredBaseCondition,
            string restoredPendingItemId,
            int restoredPendingQuantity,
            ItemQuality restoredPendingQuality,
            float restoredPendingCondition,
            ItemDatabase itemDatabase
        )
        {
            processActive =
                restoredProcessActive;

            currentStep =
                Mathf.Clamp(
                    restoredCurrentStep,
                    0,
                    TotalProcessSteps
                );

            mistakeCount =
                Mathf.Max(
                    0,
                    restoredMistakes
                );

            printProgress =
                Mathf.Clamp01(
                    restoredPrintProgress
                );

            activeBaseQualityScore =
                Mathf.Max(
                    0f,
                    restoredBaseQualityScore
                );

            activeBaseCondition =
                Mathf.Clamp01(
                    restoredBaseCondition
                );

            pendingOutputItem = null;
            pendingOutputQuantity = 0;
            pendingOutputQuality =
                restoredPendingQuality;
            pendingOutputCondition =
                Mathf.Clamp01(
                    restoredPendingCondition
                );

            if (
                itemDatabase != null &&
                !string.IsNullOrWhiteSpace(
                    restoredPendingItemId
                ) &&
                itemDatabase.TryGetItem(
                    restoredPendingItemId,
                    out ItemDefinition restoredItem
                )
            )
            {
                pendingOutputItem =
                    restoredItem;

                pendingOutputQuantity =
                    Mathf.Max(
                        1,
                        restoredPendingQuantity
                    );
            }

            if (
                processActive &&
                (
                    recipe == null ||
                    !recipe.IsValid()
                )
            )
            {
                processActive = false;
                currentStep = 0;
            }

            RefreshOutputVisual();
            interactionController?.SyncFromStation();
            StateChanged?.Invoke();
        }

        private bool HasItem(
            ItemDefinition item,
            int quantity,
            out string reason
        )
        {
            if (item == null)
            {
                reason =
                    "A recipe item is not configured.";
                return false;
            }

            int owned =
                inventory.CountItem(item);

            if (owned < quantity)
            {
                reason =
                    $"Need {quantity}× " +
                    $"{item.DisplayName} " +
                    $"({owned} owned).";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void CaptureInputQuality(
            out float qualityScore,
            out float condition
        )
        {
            ItemDefinition[] sampledItems =
            {
                recipe.BlankNoteStock,
                recipe.PigmentCapsule,
                recipe.SecurityFilm,
                recipe.PackagingMaterial
            };

            float qualityTotal = 0f;
            float conditionTotal = 0f;
            int sampledCount = 0;

            foreach (
                ItemDefinition sampledItem
                in sampledItems
            )
            {
                foreach (
                    InventorySlot slot
                    in inventory.Slots
                )
                {
                    if (
                        slot == null ||
                        slot.IsEmpty ||
                        slot.Stack.Item != sampledItem
                    )
                    {
                        continue;
                    }

                    qualityTotal +=
                        (int)slot.Stack.Quality;

                    conditionTotal +=
                        slot.Stack.Condition;

                    sampledCount++;
                    break;
                }
            }

            if (sampledCount <= 0)
            {
                qualityScore = 1f;
                condition = 1f;
                return;
            }

            float averageCondition =
                conditionTotal /
                sampledCount;

            qualityScore =
                qualityTotal /
                sampledCount +
                Mathf.Lerp(
                    -0.3f,
                    0.35f,
                    averageCondition
                );

            condition =
                Mathf.Clamp01(
                    averageCondition
                );
        }

        private static ItemQuality QualityFromScore(
            float score
        )
        {
            if (score < 0.75f)
            {
                return ItemQuality.Poor;
            }

            if (score < 1.75f)
            {
                return ItemQuality.Standard;
            }

            if (score < 2.75f)
            {
                return ItemQuality.Good;
            }

            if (score < 3.75f)
            {
                return ItemQuality.Excellent;
            }

            return ItemQuality.Masterwork;
        }

        private void ResolveReferences()
        {
            inventory ??=
                FindFirstObjectByType<
                    PlayerInventory
                >();

            reputationSystem ??=
                FindFirstObjectByType<
                    IndustryReputationSystem
                >();

            if (
                interactionController == null
            )
            {
                interactionController =
                    GetComponentInChildren<
                        CounterfeitPressInteractionController
                    >(true);
            }

            if (
                interactionController != null &&
                interactionController.Station != this
            )
            {
                interactionController
                    .SetStation(this);
            }
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
                $"[CounterfeitPress] {message}",
                this
            );

            StateChanged?.Invoke();
        }
    }
}
