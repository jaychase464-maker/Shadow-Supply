using System;
using System.Collections.Generic;
using ShadowSupply.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Production
{
    [DisallowMultipleComponent]
    public sealed class
        CounterfeitPressInteractionController :
            MonoBehaviour
    {
        [Header("Station")]
        [SerializeField]
        private CounterfeitPressStation station;

        [Header("Camera and Surface")]
        [SerializeField]
        private Camera processCamera;
        [SerializeField]
        private Transform processSurface;
        [SerializeField]
        private Vector2 surfaceSize =
            new Vector2(2f, 0.9f);
        [SerializeField]
        private Transform spawnedPartsRoot;

        [Header("Process Zones")]
        [SerializeField]
        private BoxCollider paperTrayZone;
        [SerializeField]
        private Transform paperTraySnap;
        [SerializeField]
        private Renderer paperTrayRenderer;

        [SerializeField]
        private BoxCollider pigmentBayZone;
        [SerializeField]
        private Transform pigmentBaySnap;
        [SerializeField]
        private Renderer pigmentBayRenderer;

        [SerializeField]
        private BoxCollider filmBedZone;
        [SerializeField]
        private Transform filmBedSnap;
        [SerializeField]
        private Renderer filmBedRenderer;

        [SerializeField]
        private BoxCollider cuttingMatZone;
        [SerializeField]
        private Transform cuttingMatSnap;
        [SerializeField]
        private Renderer cuttingMatRenderer;

        [SerializeField]
        private BoxCollider packagingZone;
        [SerializeField]
        private Transform packagingSnap;
        [SerializeField]
        private Renderer packagingRenderer;

        [Header("Controls")]
        [SerializeField]
        private Collider printControlCollider;
        [SerializeField]
        private Renderer printControlRenderer;
        [SerializeField]
        private Transform printControlTransform;
        [SerializeField]
        private Vector3 printControlReleasedPosition;
        [SerializeField]
        private Vector3 printControlPressedPosition;

        [SerializeField]
        private Collider sealControlCollider;
        [SerializeField]
        private Renderer sealControlRenderer;

        [Header("Interaction")]
        [SerializeField, Min(0.01f)]
        private float dragHeight = 0.11f;
        [SerializeField, Min(0.01f)]
        private float itemVisualSize = 0.18f;

        [Header("Imported Process Visuals")]
        [SerializeField]
        private GameObject printedSheetVisualPrefab;
        [SerializeField]
        private GameObject cutNoteStackVisualPrefab;

        private readonly Dictionary<
            CounterfeitPressPartKind,
            CounterfeitPressPart
        > parts =
            new Dictionary<
                CounterfeitPressPartKind,
                CounterfeitPressPart
            >();

        private Camera playerCamera;
        private CounterfeitPressPart draggedPart;
        private Plane dragPlane;
        private Vector3 dragOffset;
        private bool viewActive;
        private bool holdingPrintControl;

        public CounterfeitPressStation Station =>
            station;
        public Camera ProcessCamera =>
            processCamera;
        public bool ViewActive => viewActive;
        public GameObject PrintedSheetVisualPrefab =>
            printedSheetVisualPrefab;
        public GameObject CutNoteStackVisualPrefab =>
            cutNoteStackVisualPrefab;

        private void Awake()
        {
            ResolveStation();

            if (processCamera != null)
            {
                processCamera.enabled = false;
            }

            SetProcessObjectsVisible(false);
            RefreshVisualState();
        }

        private void Update()
        {
            if (
                !viewActive ||
                station == null ||
                Mouse.current == null
            )
            {
                return;
            }

            RefreshHighlights();

            if (!station.HasPower)
            {
                CancelCurrentInteraction();
                return;
            }

            HandlePointer();
        }

        public void Configure(
            CounterfeitPressStation targetStation,
            Camera cameraReference,
            Transform surfaceReference,
            Vector2 configuredSurfaceSize,
            Transform partsRoot,
            BoxCollider configuredPaperTrayZone,
            Transform configuredPaperTraySnap,
            Renderer configuredPaperTrayRenderer,
            BoxCollider configuredPigmentBayZone,
            Transform configuredPigmentBaySnap,
            Renderer configuredPigmentBayRenderer,
            BoxCollider configuredFilmBedZone,
            Transform configuredFilmBedSnap,
            Renderer configuredFilmBedRenderer,
            BoxCollider configuredCuttingMatZone,
            Transform configuredCuttingMatSnap,
            Renderer configuredCuttingMatRenderer,
            BoxCollider configuredPackagingZone,
            Transform configuredPackagingSnap,
            Renderer configuredPackagingRenderer,
            Collider configuredPrintCollider,
            Renderer configuredPrintRenderer,
            Transform configuredPrintTransform,
            Vector3 releasedPosition,
            Vector3 pressedPosition,
            Collider configuredSealCollider,
            Renderer configuredSealRenderer
        )
        {
            station = targetStation;
            processCamera = cameraReference;
            processSurface = surfaceReference;
            surfaceSize = configuredSurfaceSize;
            spawnedPartsRoot = partsRoot;

            paperTrayZone =
                configuredPaperTrayZone;
            paperTraySnap =
                configuredPaperTraySnap;
            paperTrayRenderer =
                configuredPaperTrayRenderer;

            pigmentBayZone =
                configuredPigmentBayZone;
            pigmentBaySnap =
                configuredPigmentBaySnap;
            pigmentBayRenderer =
                configuredPigmentBayRenderer;

            filmBedZone =
                configuredFilmBedZone;
            filmBedSnap =
                configuredFilmBedSnap;
            filmBedRenderer =
                configuredFilmBedRenderer;

            cuttingMatZone =
                configuredCuttingMatZone;
            cuttingMatSnap =
                configuredCuttingMatSnap;
            cuttingMatRenderer =
                configuredCuttingMatRenderer;

            packagingZone =
                configuredPackagingZone;
            packagingSnap =
                configuredPackagingSnap;
            packagingRenderer =
                configuredPackagingRenderer;

            printControlCollider =
                configuredPrintCollider;
            printControlRenderer =
                configuredPrintRenderer;
            printControlTransform =
                configuredPrintTransform;
            printControlReleasedPosition =
                releasedPosition;
            printControlPressedPosition =
                pressedPosition;

            sealControlCollider =
                configuredSealCollider;
            sealControlRenderer =
                configuredSealRenderer;

            if (processCamera != null)
            {
                processCamera.enabled = false;
            }

            if (
                station != null &&
                station.InteractionController != this
            )
            {
                station.SetInteractionController(this);
            }

            SetProcessObjectsVisible(false);
            RefreshVisualState();
        }

        public void SetStation(
            CounterfeitPressStation targetStation
        )
        {
            station = targetStation;
        }

        public void ConfigureImportedVisuals(
            GameObject printedSheetPrefab,
            GameObject cutStackPrefab
        )
        {
            printedSheetVisualPrefab =
                printedSheetPrefab;

            cutNoteStackVisualPrefab =
                cutStackPrefab;

            if (viewActive)
            {
                SyncFromStation();
            }
        }

        public void Activate(
            Camera currentPlayerCamera
        )
        {
            ResolveStation();
            playerCamera = currentPlayerCamera;

            if (playerCamera != null)
            {
                playerCamera.enabled = false;
            }

            if (processCamera != null)
            {
                processCamera.enabled = true;
            }

            viewActive = true;

            SetProcessObjectsVisible(
                station != null &&
                station.ProcessActive &&
                !station.HasPendingOutput
            );

            SyncFromStation();
        }

        public void Deactivate()
        {
            viewActive = false;
            CancelCurrentInteraction();

            if (processCamera != null)
            {
                processCamera.enabled = false;
            }

            if (playerCamera != null)
            {
                playerCamera.enabled = true;
            }

            playerCamera = null;
        }

        public void SyncFromStation()
        {
            ResolveStation();
            ClearParts();

            bool show =
                station != null &&
                station.ProcessActive &&
                !station.HasPendingOutput;

            SetProcessObjectsVisible(show);

            if (!show)
            {
                RefreshVisualState();
                return;
            }

            CreateProcessParts();
            RestoreStepVisuals();
            RefreshVisualState();
            RefreshHighlights();
        }


        public bool TryGetStepCallouts(
            out string actionLabel,
            out Vector2 actionGuiPosition,
            out string targetLabel,
            out Vector2 targetGuiPosition
        )
        {
            actionLabel = string.Empty;
            actionGuiPosition = Vector2.zero;
            targetLabel = string.Empty;
            targetGuiPosition = Vector2.zero;

            if (
                !viewActive ||
                processCamera == null ||
                station == null ||
                !station.ProcessActive
            )
            {
                return false;
            }

            Transform actionTransform = null;
            Transform targetTransform = null;

            switch (station.CurrentStep)
            {
                case 0:
                    actionLabel =
                        "DRAG THIS\nBLANK NOTE STOCK";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .BlankNoteStock
                        );
                    targetLabel =
                        "DROP HERE\nPAPER TRAY";
                    targetTransform =
                        GetTargetTransform(
                            paperTraySnap,
                            paperTrayRenderer
                        );
                    break;

                case 1:
                    actionLabel =
                        "DRAG THIS\nPIGMENT CAPSULE";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .PigmentCapsule
                        );
                    targetLabel =
                        "DROP HERE\nPIGMENT BAY";
                    targetTransform =
                        GetTargetTransform(
                            pigmentBaySnap,
                            pigmentBayRenderer
                        );
                    break;

                case 2:
                    actionLabel =
                        "DRAG THIS\nSECURITY FILM";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .SecurityFilm
                        );
                    targetLabel =
                        "DROP HERE\nFILM BED";
                    targetTransform =
                        GetTargetTransform(
                            filmBedSnap,
                            filmBedRenderer
                        );
                    break;

                case 3:
                    actionLabel =
                        "HOLD THIS\nPRINT";
                    actionTransform =
                        printControlTransform;
                    break;

                case 4:
                    actionLabel =
                        "DRAG THIS\nPRINTED SHEET";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .PrintedSheet
                        );
                    targetLabel =
                        "DROP HERE\nCUTTING MAT";
                    targetTransform =
                        GetTargetTransform(
                            cuttingMatSnap,
                            cuttingMatRenderer
                        );
                    break;

                case 5:
                    actionLabel =
                        "CLICK THIS\nBASIC TOOLKIT";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .BasicToolkit
                        );
                    break;

                case 6:
                    actionLabel =
                        "DRAG THIS\nCUT NOTE STACK";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .CutNoteStack
                        );
                    targetLabel =
                        "DROP HERE\nPACKAGING ZONE";
                    targetTransform =
                        GetTargetTransform(
                            packagingSnap,
                            packagingRenderer
                        );
                    break;

                case 7:
                    actionLabel =
                        "DRAG THIS\nPACKAGING MATERIAL";
                    actionTransform =
                        GetPartTransform(
                            CounterfeitPressPartKind
                                .PackagingMaterial
                        );
                    targetLabel =
                        "DROP HERE\nOVER NOTE STACK";
                    targetTransform =
                        GetTargetTransform(
                            packagingSnap,
                            packagingRenderer
                        );
                    break;

                case 8:
                    actionLabel =
                        "CLICK THIS\nSEAL";
                    actionTransform =
                        sealControlRenderer != null
                            ? sealControlRenderer.transform
                            : sealControlCollider != null
                                ? sealControlCollider.transform
                                : null;
                    break;
            }

            bool hasAction =
                TryProjectToGui(
                    actionTransform,
                    out actionGuiPosition
                );

            bool hasTarget =
                TryProjectToGui(
                    targetTransform,
                    out targetGuiPosition
                );

            if (!hasAction)
            {
                actionLabel = string.Empty;
            }

            if (!hasTarget)
            {
                targetLabel = string.Empty;
            }

            return hasAction || hasTarget;
        }

        private Transform GetPartTransform(
            CounterfeitPressPartKind kind
        )
        {
            CounterfeitPressPart part =
                GetPart(kind);

            return
                part != null &&
                part.gameObject.activeInHierarchy
                    ? part.transform
                    : null;
        }

        private static Transform GetTargetTransform(
            Transform snap,
            Renderer renderer
        )
        {
            if (snap != null)
            {
                return snap;
            }

            return
                renderer != null
                    ? renderer.transform
                    : null;
        }

        private bool TryProjectToGui(
            Transform target,
            out Vector2 guiPosition
        )
        {
            guiPosition = Vector2.zero;

            if (
                target == null ||
                processCamera == null
            )
            {
                return false;
            }

            Vector3 screen =
                processCamera.WorldToScreenPoint(
                    target.position
                );

            if (screen.z <= 0f)
            {
                return false;
            }

            guiPosition =
                new Vector2(
                    screen.x,
                    Screen.height - screen.y
                );

            return
                guiPosition.x >= 0f &&
                guiPosition.x <= Screen.width &&
                guiPosition.y >= 0f &&
                guiPosition.y <= Screen.height;
        }

        public void NotifyOutputReady()
        {
            CancelCurrentInteraction();
            ClearParts();
            SetProcessObjectsVisible(false);
            RefreshVisualState();
        }

        private void HandlePointer()
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            Ray ray =
                processCamera.ScreenPointToRay(
                    screenPosition
                );

            if (
                Mouse.current.leftButton
                    .wasPressedThisFrame
            )
            {
                HandlePointerDown(ray);
            }

            if (
                draggedPart != null &&
                Mouse.current.leftButton.isPressed
            )
            {
                DragPart(ray);
            }

            if (
                holdingPrintControl &&
                Mouse.current.leftButton.isPressed
            )
            {
                ContinuePrintHold(ray);
            }

            if (
                Mouse.current.leftButton
                    .wasReleasedThisFrame
            )
            {
                HandlePointerUp();
            }
        }

        private void HandlePointerDown(Ray ray)
        {
            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    5f,
                    ~0,
                    QueryTriggerInteraction.Collide
                );

            Array.Sort(
                hits,
                (left, right) =>
                    left.distance.CompareTo(
                        right.distance
                    )
            );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                CounterfeitPressPart part =
                    hit.collider
                        .GetComponentInParent<
                            CounterfeitPressPart
                        >();

                if (
                    part != null &&
                    !part.Completed
                )
                {
                    HandlePartPressed(
                        part,
                        hit.point
                    );
                    return;
                }

                CounterfeitPressActionTarget action =
                    hit.collider
                        .GetComponentInParent<
                            CounterfeitPressActionTarget
                        >();

                if (action != null)
                {
                    HandleActionPressed(action);
                    return;
                }
            }
        }

        private void HandlePartPressed(
            CounterfeitPressPart part,
            Vector3 hitPoint
        )
        {
            if (
                station == null ||
                !station.ProcessActive
            )
            {
                return;
            }

            if (
                part.PartKind ==
                CounterfeitPressPartKind
                    .BasicToolkit
            )
            {
                if (station.CurrentStep == 5)
                {
                    CompleteCutStep(part);
                }
                else
                {
                    station.ShowGuidance(
                        "That tool is not needed yet. " +
                        "Follow the DRAG THIS label."
                    );
                }

                return;
            }

            if (!part.Draggable)
            {
                return;
            }

            if (
                !IsExpectedPart(
                    part.PartKind,
                    station.CurrentStep
                )
            )
            {
                station.ShowGuidance(
                    "That is not the material for this step. " +
                    "Use the item marked DRAG THIS."
                );
                return;
            }

            BeginDragging(
                part,
                hitPoint
            );
        }

        private void HandleActionPressed(
            CounterfeitPressActionTarget action
        )
        {
            if (
                station == null ||
                !station.ProcessActive
            )
            {
                return;
            }

            switch (action.ActionKind)
            {
                case CounterfeitPressActionKind
                    .PrintControl:
                    if (station.CurrentStep == 3)
                    {
                        holdingPrintControl = true;
                        SetPrintControlPressed(true);
                    }
                    else
                    {
                        station.ShowGuidance(
                            "PRINT is not active yet. " +
                            "Complete the highlighted material step first."
                        );
                    }
                    break;

                case CounterfeitPressActionKind
                    .SealControl:
                    if (station.CurrentStep == 8)
                    {
                        if (station.CompleteStep(8))
                        {
                            station.FinalizeProduction();
                        }
                    }
                    else
                    {
                        station.ShowGuidance(
                            "SEAL is not active yet. " +
                            "Finish the highlighted packaging step first."
                        );
                    }
                    break;
            }
        }

        private void BeginDragging(
            CounterfeitPressPart part,
            Vector3 hitPoint
        )
        {
            draggedPart = part;
            draggedPart.SetDragging(true);

            float planeHeight =
                processSurface != null
                    ? processSurface.position.y +
                      dragHeight
                    : part.transform.position.y;

            dragPlane =
                new Plane(
                    Vector3.up,
                    new Vector3(
                        0f,
                        planeHeight,
                        0f
                    )
                );

            dragOffset =
                part.transform.position -
                hitPoint;
        }

        private void DragPart(Ray ray)
        {
            if (
                draggedPart == null ||
                !dragPlane.Raycast(
                    ray,
                    out float enter
                )
            )
            {
                return;
            }

            Vector3 worldPosition =
                ray.GetPoint(enter) +
                dragOffset;

            draggedPart.transform.position =
                ClampToSurface(
                    worldPosition
                );
        }

        private void ContinuePrintHold(Ray ray)
        {
            if (
                station == null ||
                station.CurrentStep != 3
            )
            {
                StopPrintHold(true);
                return;
            }

            if (
                !RayHitsAction(
                    ray,
                    CounterfeitPressActionKind
                        .PrintControl
                )
            )
            {
                StopPrintHold(true);
                return;
            }

            float duration =
                station.Recipe != null
                    ? station.Recipe
                        .PrintHoldSeconds
                    : 1.75f;

            float progress =
                station.PrintProgress +
                Time.unscaledDeltaTime /
                Mathf.Max(
                    0.25f,
                    duration
                );

            station.SetPrintProgress(progress);

            if (progress < 1f)
            {
                return;
            }

            station.SetPrintProgress(1f);

            if (station.CompleteStep(3))
            {
                ShowPrintedSheetAtStart();
            }

            StopPrintHold(false);
        }

        private void HandlePointerUp()
        {
            if (draggedPart != null)
            {
                CounterfeitPressPart released =
                    draggedPart;

                draggedPart = null;
                released.SetDragging(false);

                ResolveDraggedPart(released);
            }

            if (holdingPrintControl)
            {
                StopPrintHold(true);
            }
        }

        private void ResolveDraggedPart(
            CounterfeitPressPart part
        )
        {
            if (
                part == null ||
                station == null
            )
            {
                return;
            }

            int step = station.CurrentStep;

            BoxCollider targetZone =
                GetExpectedZone(step);

            CounterfeitPressPartKind expectedKind =
                GetExpectedPartKind(step);

            bool correct =
                part.PartKind == expectedKind &&
                IsInsideDropZone(
                    targetZone,
                    part.transform.position
                );

            if (!correct)
            {
                part.ResetToStart();

                station.RegisterMistake(
                    "The material was placed in the wrong area."
                );

                RefreshHighlights();
                return;
            }

            if (!station.CompleteStep(step))
            {
                part.ResetToStart();
                return;
            }

            SnapPartForCompletedStep(
                part,
                step
            );

            RefreshHighlights();
        }

        private void CompleteCutStep(
            CounterfeitPressPart toolkit
        )
        {
            if (
                station == null ||
                !station.CompleteStep(5)
            )
            {
                return;
            }

            toolkit.MarkClickedComplete();

            CounterfeitPressPart printed =
                GetPart(
                    CounterfeitPressPartKind
                        .PrintedSheet
                );

            if (printed != null)
            {
                printed.gameObject.SetActive(false);
            }

            CounterfeitPressPart cutStack =
                GetPart(
                    CounterfeitPressPartKind
                        .CutNoteStack
                );

            if (cutStack != null)
            {
                cutStack.gameObject.SetActive(true);
                cutStack.ResetToStart();

                if (cuttingMatSnap != null)
                {
                    Vector3 startPosition =
                        cuttingMatSnap.position +
                        Vector3.up * 0.04f;

                    cutStack.transform.position =
                        startPosition;

                    cutStack.SetStartPose(
                        startPosition,
                        cutStack.transform.rotation
                    );
                }
            }

            RefreshHighlights();
        }

        private void SnapPartForCompletedStep(
            CounterfeitPressPart part,
            int completedStep
        )
        {
            switch (completedStep)
            {
                case 0:
                    part.SnapCompleted(
                        paperTraySnap,
                        Vector3.up * 0.025f,
                        Quaternion.identity,
                        0.8f
                    );
                    break;

                case 1:
                    part.SnapCompleted(
                        pigmentBaySnap,
                        Vector3.up * 0.04f,
                        Quaternion.identity,
                        0.8f
                    );
                    break;

                case 2:
                    part.SnapCompleted(
                        filmBedSnap,
                        Vector3.up * 0.025f,
                        Quaternion.identity,
                        0.86f
                    );
                    break;

                case 4:
                    part.SnapCompleted(
                        cuttingMatSnap,
                        Vector3.up * 0.025f,
                        Quaternion.identity,
                        0.9f
                    );
                    break;

                case 6:
                    part.SnapCompleted(
                        packagingSnap,
                        new Vector3(
                            0f,
                            0.035f,
                            0f
                        ),
                        Quaternion.identity,
                        0.86f
                    );
                    break;

                case 7:
                    part.SnapCompleted(
                        packagingSnap,
                        new Vector3(
                            0f,
                            0.075f,
                            0f
                        ),
                        Quaternion.Euler(
                            0f,
                            0f,
                            8f
                        ),
                        0.82f
                    );
                    break;
            }
        }

        private void CreateProcessParts()
        {
            if (
                station == null ||
                station.Recipe == null ||
                spawnedPartsRoot == null
            )
            {
                return;
            }

            CounterfeitRecipeDefinition recipe =
                station.Recipe;

            CreatePart(
                CounterfeitPressPartKind
                    .BlankNoteStock,
                recipe.BlankNoteStock,
                true,
                new Vector3(
                    -0.82f,
                    dragHeight,
                    -0.27f
                ),
                PrimitiveType.Cube,
                new Color(
                    0.82f,
                    0.78f,
                    0.58f
                ),
                new Vector3(
                    0.24f,
                    0.035f,
                    0.16f
                )
            );

            CreatePart(
                CounterfeitPressPartKind
                    .PigmentCapsule,
                recipe.PigmentCapsule,
                true,
                new Vector3(
                    -0.54f,
                    dragHeight,
                    -0.28f
                ),
                PrimitiveType.Cylinder,
                new Color(
                    0.18f,
                    0.16f,
                    0.14f
                ),
                new Vector3(
                    0.1f,
                    0.06f,
                    0.1f
                )
            );

            CreatePart(
                CounterfeitPressPartKind
                    .SecurityFilm,
                recipe.SecurityFilm,
                true,
                new Vector3(
                    -0.24f,
                    dragHeight,
                    -0.28f
                ),
                PrimitiveType.Cube,
                new Color(
                    0.28f,
                    0.66f,
                    0.72f,
                    0.72f
                ),
                new Vector3(
                    0.23f,
                    0.02f,
                    0.15f
                )
            );

            CreatePart(
                CounterfeitPressPartKind
                    .PackagingMaterial,
                recipe.PackagingMaterial,
                true,
                new Vector3(
                    0.69f,
                    dragHeight,
                    -0.29f
                ),
                PrimitiveType.Cube,
                new Color(
                    0.35f,
                    0.24f,
                    0.12f
                ),
                new Vector3(
                    0.2f,
                    0.035f,
                    0.14f
                )
            );

            CreatePart(
                CounterfeitPressPartKind
                    .BasicToolkit,
                recipe.BasicToolkit,
                false,
                new Vector3(
                    0.92f,
                    dragHeight,
                    0.25f
                ),
                PrimitiveType.Cube,
                new Color(
                    0.12f,
                    0.13f,
                    0.14f
                ),
                new Vector3(
                    0.22f,
                    0.07f,
                    0.14f
                )
            );

            CreatePart(
                CounterfeitPressPartKind
                    .PrintedSheet,
                null,
                true,
                new Vector3(
                    -0.02f,
                    dragHeight,
                    0.26f
                ),
                PrimitiveType.Cube,
                new Color(
                    0.66f,
                    0.72f,
                    0.56f
                ),
                new Vector3(
                    0.3f,
                    0.018f,
                    0.18f
                ),
                printedSheetVisualPrefab
            );

            CreatePart(
                CounterfeitPressPartKind
                    .CutNoteStack,
                null,
                true,
                new Vector3(
                    0.15f,
                    dragHeight,
                    0.27f
                ),
                PrimitiveType.Cube,
                new Color(
                    0.58f,
                    0.66f,
                    0.48f
                ),
                new Vector3(
                    0.22f,
                    0.045f,
                    0.12f
                ),
                cutNoteStackVisualPrefab
            );
        }

        private CounterfeitPressPart CreatePart(
            CounterfeitPressPartKind kind,
            ItemDefinition item,
            bool draggable,
            Vector3 localPosition,
            PrimitiveType fallbackPrimitive,
            Color fallbackColor,
            Vector3 fallbackScale,
            GameObject visualOverride = null
        )
        {
            GameObject root =
                new GameObject(
                    $"Process Part — {kind}"
                );

            root.transform.SetParent(
                spawnedPartsRoot,
                false
            );

            root.transform.localPosition =
                localPosition;

            root.transform.localRotation =
                Quaternion.identity;

            GameObject visual =
                CreateVisual(
                    item,
                    fallbackPrimitive,
                    fallbackColor,
                    fallbackScale,
                    root.transform,
                    visualOverride
                );

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            Bounds localBounds =
                CalculateLocalVisualBounds(
                    root.transform,
                    visual
                );

            collider.center =
                localBounds.center;

            collider.size =
                new Vector3(
                    Mathf.Max(
                        0.1f,
                        localBounds.size.x
                    ),
                    Mathf.Max(
                        0.07f,
                        localBounds.size.y
                    ),
                    Mathf.Max(
                        0.1f,
                        localBounds.size.z
                    )
                );

            CounterfeitPressPart part =
                root.AddComponent<
                    CounterfeitPressPart
                >();

            part.Configure(
                kind,
                item,
                draggable,
                collider,
                root.GetComponentsInChildren<
                    Renderer
                >(true)
            );

            part.SetStartPose(
                root.transform.position,
                root.transform.rotation
            );

            parts[kind] = part;
            return part;
        }

        private GameObject CreateVisual(
            ItemDefinition item,
            PrimitiveType fallbackPrimitive,
            Color fallbackColor,
            Vector3 fallbackScale,
            Transform parent,
            GameObject visualOverride
        )
        {
            GameObject visual;

            GameObject preferredPrefab =
                visualOverride != null
                    ? visualOverride
                    : item != null
                        ? item.DisplayPrefab
                        : null;

            if (preferredPrefab != null)
            {
                visual =
                    Instantiate(
                        preferredPrefab,
                        parent
                    );

                if (visualOverride == null)
                {
                    NormalizeVisual(
                        visual,
                        itemVisualSize
                    );
                }
            }
            else
            {
                visual =
                    GameObject.CreatePrimitive(
                        fallbackPrimitive
                    );

                visual.transform.SetParent(
                    parent,
                    false
                );

                visual.transform.localPosition =
                    Vector3.zero;

                visual.transform.localRotation =
                    Quaternion.identity;

                visual.transform.localScale =
                    fallbackScale;

                Renderer renderer =
                    visual.GetComponent<Renderer>();

                if (renderer != null)
                {
                    MaterialPropertyBlock block =
                        new MaterialPropertyBlock();

                    renderer.GetPropertyBlock(block);

                    block.SetColor(
                        "_BaseColor",
                        fallbackColor
                    );

                    block.SetColor(
                        "_Color",
                        fallbackColor
                    );

                    renderer.SetPropertyBlock(block);
                }
            }

            visual.name =
                item != null
                    ? item.DisplayName + " Visual"
                    : "Process Visual";

            foreach (
                Rigidbody body
                in visual.GetComponentsInChildren<
                    Rigidbody
                >(true)
            )
            {
                Destroy(body);
            }

            foreach (
                Collider collider
                in visual.GetComponentsInChildren<
                    Collider
                >(true)
            )
            {
                Destroy(collider);
            }

            return visual;
        }

        private static void NormalizeVisual(
            GameObject visual,
            float targetSize
        )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<
                    Renderer
                >(true);

            if (renderers.Length == 0)
            {
                visual.transform.localScale =
                    Vector3.one *
                    targetSize;
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

            float largest =
                Mathf.Max(
                    bounds.size.x,
                    bounds.size.y,
                    bounds.size.z
                );

            if (largest <= 0.0001f)
            {
                return;
            }

            visual.transform.localScale *=
                targetSize / largest;

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
                    visual.transform.parent.position.y -
                    bounds.min.y
                );
        }

        private void RestoreStepVisuals()
        {
            if (station == null)
            {
                return;
            }

            int step = station.CurrentStep;

            RestoreInputPart(
                CounterfeitPressPartKind
                    .BlankNoteStock,
                step > 0,
                paperTraySnap,
                Vector3.up * 0.025f
            );

            RestoreInputPart(
                CounterfeitPressPartKind
                    .PigmentCapsule,
                step > 1,
                pigmentBaySnap,
                Vector3.up * 0.04f
            );

            RestoreInputPart(
                CounterfeitPressPartKind
                    .SecurityFilm,
                step > 2,
                filmBedSnap,
                Vector3.up * 0.025f
            );

            CounterfeitPressPart toolkit =
                GetPart(
                    CounterfeitPressPartKind
                        .BasicToolkit
                );

            if (
                toolkit != null &&
                step > 5
            )
            {
                toolkit.MarkClickedComplete();
            }

            CounterfeitPressPart packaging =
                GetPart(
                    CounterfeitPressPartKind
                        .PackagingMaterial
                );

            if (
                packaging != null &&
                step > 7
            )
            {
                packaging.SnapCompleted(
                    packagingSnap,
                    new Vector3(
                        0f,
                        0.075f,
                        0f
                    ),
                    Quaternion.Euler(
                        0f,
                        0f,
                        8f
                    ),
                    0.82f
                );
            }

            CounterfeitPressPart printed =
                GetPart(
                    CounterfeitPressPartKind
                        .PrintedSheet
                );

            CounterfeitPressPart cutStack =
                GetPart(
                    CounterfeitPressPartKind
                        .CutNoteStack
                );

            if (printed != null)
            {
                bool visible =
                    step >= 4 &&
                    step <= 5;

                printed.gameObject.SetActive(
                    visible
                );

                if (visible)
                {
                    printed.ResetToStart();

                    if (
                        step == 5 &&
                        cuttingMatSnap != null
                    )
                    {
                        printed.SnapCompleted(
                            cuttingMatSnap,
                            Vector3.up * 0.025f,
                            Quaternion.identity,
                            0.9f
                        );
                    }
                }
            }

            if (cutStack != null)
            {
                bool visible =
                    step >= 6;

                cutStack.gameObject.SetActive(
                    visible
                );

                if (visible)
                {
                    cutStack.ResetToStart();

                    if (
                        step >= 7 &&
                        packagingSnap != null
                    )
                    {
                        cutStack.SnapCompleted(
                            packagingSnap,
                            new Vector3(
                                0f,
                                0.035f,
                                0f
                            ),
                            Quaternion.identity,
                            0.86f
                        );
                    }
                    else if (
                        cuttingMatSnap != null
                    )
                    {
                        Vector3 startPosition =
                            cuttingMatSnap.position +
                            Vector3.up * 0.04f;

                        cutStack.transform.position =
                            startPosition;

                        cutStack.SetStartPose(
                            startPosition,
                            cutStack.transform.rotation
                        );
                    }
                }
            }
        }

        private void RestoreInputPart(
            CounterfeitPressPartKind kind,
            bool completed,
            Transform target,
            Vector3 localOffset
        )
        {
            CounterfeitPressPart part =
                GetPart(kind);

            if (
                part == null ||
                !completed
            )
            {
                return;
            }

            part.SnapCompleted(
                target,
                localOffset,
                Quaternion.identity,
                0.82f
            );
        }

        private void ShowPrintedSheetAtStart()
        {
            CounterfeitPressPart printed =
                GetPart(
                    CounterfeitPressPartKind
                        .PrintedSheet
                );

            if (printed == null)
            {
                return;
            }

            printed.gameObject.SetActive(true);
            printed.ResetToStart();
        }

        private void RefreshHighlights()
        {
            if (
                station == null ||
                !station.ProcessActive
            )
            {
                ClearAllHighlights();
                return;
            }

            int step = station.CurrentStep;

            foreach (
                KeyValuePair<
                    CounterfeitPressPartKind,
                    CounterfeitPressPart
                > pair
                in parts
            )
            {
                if (pair.Value != null)
                {
                    pair.Value.SetExpected(
                        IsExpectedPart(
                            pair.Key,
                            step
                        )
                    );
                }
            }

            SetRendererTint(
                paperTrayRenderer,
                step == 0
            );

            SetRendererTint(
                pigmentBayRenderer,
                step == 1
            );

            SetRendererTint(
                filmBedRenderer,
                step == 2
            );

            SetRendererTint(
                cuttingMatRenderer,
                step == 4 ||
                step == 5
            );

            SetRendererTint(
                packagingRenderer,
                step == 6 ||
                step == 7
            );

            SetRendererTint(
                printControlRenderer,
                step == 3
            );

            SetRendererTint(
                sealControlRenderer,
                step == 8
            );

            if (printControlCollider != null)
            {
                printControlCollider.enabled =
                    step == 3;
            }

            if (sealControlCollider != null)
            {
                sealControlCollider.enabled =
                    step == 8;
            }
        }

        private void RefreshVisualState()
        {
            if (
                printControlTransform != null
            )
            {
                printControlTransform.localPosition =
                    holdingPrintControl
                        ? printControlPressedPosition
                        : printControlReleasedPosition;
            }

            RefreshHighlights();
        }

        private void ClearAllHighlights()
        {
            SetRendererTint(
                paperTrayRenderer,
                false
            );

            SetRendererTint(
                pigmentBayRenderer,
                false
            );

            SetRendererTint(
                filmBedRenderer,
                false
            );

            SetRendererTint(
                cuttingMatRenderer,
                false
            );

            SetRendererTint(
                packagingRenderer,
                false
            );

            SetRendererTint(
                printControlRenderer,
                false
            );

            SetRendererTint(
                sealControlRenderer,
                false
            );
        }

        private static void SetRendererTint(
            Renderer renderer,
            bool highlighted
        )
        {
            if (renderer == null)
            {
                return;
            }

            if (!highlighted)
            {
                renderer.SetPropertyBlock(null);
                return;
            }

            MaterialPropertyBlock block =
                new MaterialPropertyBlock();

            renderer.GetPropertyBlock(block);

            Color color =
                new Color(
                    1f,
                    0.48f,
                    0.025f
                );

            block.SetColor(
                "_BaseColor",
                color
            );

            block.SetColor(
                "_Color",
                color
            );

            block.SetColor(
                "_EmissionColor",
                color * 2f
            );

            renderer.SetPropertyBlock(block);
        }

        private void SetPrintControlPressed(
            bool pressed
        )
        {
            if (
                printControlTransform != null
            )
            {
                printControlTransform.localPosition =
                    pressed
                        ? printControlPressedPosition
                        : printControlReleasedPosition;
            }
        }

        private void StopPrintHold(
            bool resetProgress
        )
        {
            holdingPrintControl = false;
            SetPrintControlPressed(false);

            if (
                resetProgress &&
                station != null &&
                station.CurrentStep == 3
            )
            {
                station.SetPrintProgress(0f);
            }
        }

        private void CancelCurrentInteraction()
        {
            if (draggedPart != null)
            {
                CounterfeitPressPart part =
                    draggedPart;

                draggedPart = null;
                part.SetDragging(false);
                part.ResetToStart();
            }

            if (holdingPrintControl)
            {
                StopPrintHold(true);
            }
        }

        private bool RayHitsAction(
            Ray ray,
            CounterfeitPressActionKind kind
        )
        {
            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    5f,
                    ~0,
                    QueryTriggerInteraction.Collide
                );

            foreach (RaycastHit hit in hits)
            {
                CounterfeitPressActionTarget target =
                    hit.collider != null
                        ? hit.collider
                            .GetComponentInParent<
                                CounterfeitPressActionTarget
                            >()
                        : null;

                if (
                    target != null &&
                    target.ActionKind == kind
                )
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 ClampToSurface(
            Vector3 worldPosition
        )
        {
            if (processSurface == null)
            {
                return worldPosition;
            }

            Vector3 local =
                processSurface
                    .InverseTransformPoint(
                        worldPosition
                    );

            local.x =
                Mathf.Clamp(
                    local.x,
                    -surfaceSize.x * 0.5f,
                    surfaceSize.x * 0.5f
                );

            local.z =
                Mathf.Clamp(
                    local.z,
                    -surfaceSize.y * 0.5f,
                    surfaceSize.y * 0.5f
                );

            local.y = dragHeight;

            return
                processSurface.TransformPoint(local);
        }

        private static bool IsExpectedPart(
            CounterfeitPressPartKind kind,
            int step
        )
        {
            return
                kind ==
                GetExpectedPartKind(step);
        }

        private static CounterfeitPressPartKind
            GetExpectedPartKind(
                int step
            )
        {
            switch (step)
            {
                case 0:
                    return
                        CounterfeitPressPartKind
                            .BlankNoteStock;

                case 1:
                    return
                        CounterfeitPressPartKind
                            .PigmentCapsule;

                case 2:
                    return
                        CounterfeitPressPartKind
                            .SecurityFilm;

                case 4:
                    return
                        CounterfeitPressPartKind
                            .PrintedSheet;

                case 5:
                    return
                        CounterfeitPressPartKind
                            .BasicToolkit;

                case 6:
                    return
                        CounterfeitPressPartKind
                            .CutNoteStack;

                case 7:
                    return
                        CounterfeitPressPartKind
                            .PackagingMaterial;

                default:
                    return
                        (CounterfeitPressPartKind)
                        (-1);
            }
        }


        private static bool IsInsideDropZone(
            BoxCollider targetZone,
            Vector3 worldPosition
        )
        {
            if (targetZone == null)
            {
                return false;
            }

            Vector3 localPoint =
                targetZone.transform
                    .InverseTransformPoint(
                        worldPosition
                    );

            Vector3 localCenter =
                targetZone.center;

            Vector3 halfSize =
                targetZone.size * 0.5f;

            // The draggable part intentionally floats above the table.
            // Drop validation must therefore use the horizontal footprint
            // only, rather than requiring its center to be inside the
            // extremely thin zone collider on the Y axis.
            const float horizontalPadding = 0.12f;

            bool insideX =
                Mathf.Abs(
                    localPoint.x -
                    localCenter.x
                ) <=
                halfSize.x +
                horizontalPadding;

            bool insideZ =
                Mathf.Abs(
                    localPoint.z -
                    localCenter.z
                ) <=
                halfSize.z +
                horizontalPadding;

            return insideX && insideZ;
        }

        private BoxCollider GetExpectedZone(
            int step
        )
        {
            switch (step)
            {
                case 0:
                    return paperTrayZone;

                case 1:
                    return pigmentBayZone;

                case 2:
                    return filmBedZone;

                case 4:
                    return cuttingMatZone;

                case 6:
                case 7:
                    return packagingZone;

                default:
                    return null;
            }
        }

        private CounterfeitPressPart GetPart(
            CounterfeitPressPartKind kind
        )
        {
            parts.TryGetValue(
                kind,
                out CounterfeitPressPart part
            );

            return part;
        }

        private void ClearParts()
        {
            draggedPart = null;
            holdingPrintControl = false;
            parts.Clear();

            if (processSurface != null)
            {
                CounterfeitPressPart[] existingParts =
                    processSurface.GetComponentsInChildren<
                        CounterfeitPressPart
                    >(true);

                foreach (
                    CounterfeitPressPart existing
                    in existingParts
                )
                {
                    if (existing != null)
                    {
                        Destroy(existing.gameObject);
                    }
                }
            }

            if (spawnedPartsRoot == null)
            {
                return;
            }

            for (
                int index =
                    spawnedPartsRoot.childCount - 1;
                index >= 0;
                index--
            )
            {
                Transform child =
                    spawnedPartsRoot.GetChild(index);

                if (
                    child != null &&
                    child.GetComponent<
                        CounterfeitPressPart
                    >() == null
                )
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void SetProcessObjectsVisible(
            bool visible
        )
        {
            if (spawnedPartsRoot != null)
            {
                spawnedPartsRoot.gameObject
                    .SetActive(visible);
            }

            if (processSurface != null)
            {
                processSurface.gameObject
                    .SetActive(visible);
            }
        }

        private void ResolveStation()
        {
            if (station != null)
            {
                return;
            }

            station =
                GetComponentInParent<
                    CounterfeitPressStation
                >(true);

            if (
                station != null &&
                station.InteractionController != this
            )
            {
                station.SetInteractionController(this);
            }
        }

        private static Bounds
            CalculateLocalVisualBounds(
                Transform root,
                GameObject visual
            )
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<
                    Renderer
                >(true);

            if (renderers.Length == 0)
            {
                return
                    new Bounds(
                        Vector3.up * 0.04f,
                        Vector3.one * 0.15f
                    );
            }

            Bounds world =
                renderers[0].bounds;

            for (
                int index = 1;
                index < renderers.Length;
                index++
            )
            {
                world.Encapsulate(
                    renderers[index].bounds
                );
            }

            Vector3 min =
                root.InverseTransformPoint(
                    world.min
                );

            Vector3 max =
                root.InverseTransformPoint(
                    world.max
                );

            Bounds local =
                new Bounds();

            local.SetMinMax(
                Vector3.Min(min, max),
                Vector3.Max(min, max)
            );

            return local;
        }
    }
}
