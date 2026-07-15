using ShadowSupply.Electrical;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using ShadowSupply.Player;
using ShadowSupply.Progression;
using ShadowSupply.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Production
{
    [DisallowMultipleComponent]
    public sealed class CounterfeitPressHUD :
        MonoBehaviour
    {
        public static CounterfeitPressHUD Instance
        {
            get;
            private set;
        }

        private CounterfeitPressStation station;
        private Camera playerCamera;
        private FirstPersonController controller;
        private HotbarController hotbar;
        private PlayerInteractor interactor;
        private PlayerPlugController plugController;
        private InventoryHUD inventoryHud;

        private bool controllerWasEnabled;
        private bool hotbarWasEnabled;
        private bool interactorWasEnabled;
        private bool plugControllerWasEnabled;
        private bool inventoryHudWasEnabled;
        private bool open;
        private bool processView;

        private GUIStyle overlayStyle;
        private GUIStyle panelStyle;
        private GUIStyle cardStyle;
        private GUIStyle headerStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle mutedStyle;
        private GUIStyle orangeStyle;
        private GUIStyle goodStyle;
        private GUIStyle badStyle;
        private GUIStyle buttonStyle;
        private GUIStyle disabledButtonStyle;
        private GUIStyle calloutActionStyle;
        private GUIStyle calloutTargetStyle;
        private GUIStyle calloutTargetBoxStyle;

        public bool IsOpen => open;

        private void Awake()
        {
            if (
                Instance != null &&
                Instance != this
            )
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (open)
            {
                Close();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (
                open &&
                Keyboard.current != null &&
                Keyboard.current.escapeKey
                    .wasPressedThisFrame
            )
            {
                Close();
            }
        }

        public void Open(
            CounterfeitPressStation targetStation,
            GameObject player
        )
        {
            if (
                open ||
                targetStation == null ||
                player == null
            )
            {
                return;
            }

            station = targetStation;

            playerCamera =
                player.GetComponentInChildren<
                    Camera
                >(true);

            controller =
                player.GetComponent<
                    FirstPersonController
                >();

            hotbar =
                player.GetComponent<
                    HotbarController
                >();

            interactor =
                player.GetComponent<
                    PlayerInteractor
                >();

            plugController =
                player.GetComponent<
                    PlayerPlugController
                >();

            inventoryHud =
                FindFirstObjectByType<
                    InventoryHUD
                >();

            controllerWasEnabled =
                controller != null &&
                controller.enabled;

            hotbarWasEnabled =
                hotbar != null &&
                hotbar.enabled;

            interactorWasEnabled =
                interactor != null &&
                interactor.enabled;

            plugControllerWasEnabled =
                plugController != null &&
                plugController.enabled;

            inventoryHudWasEnabled =
                inventoryHud != null &&
                inventoryHud.enabled;

            if (controller != null)
            {
                controller.enabled = false;
            }

            if (hotbar != null)
            {
                hotbar.enabled = false;
            }

            if (interactor != null)
            {
                interactor.enabled = false;
            }

            if (plugController != null)
            {
                plugController.enabled = false;
            }

            if (inventoryHud != null)
            {
                inventoryHud.enabled = false;
            }

            Cursor.lockState =
                CursorLockMode.None;
            Cursor.visible = true;
            open = true;

            if (station.ProcessActive)
            {
                EnterProcessView();
            }
        }

        public void Close()
        {
            if (!open)
            {
                return;
            }

            if (
                processView &&
                station != null &&
                station.InteractionController != null
            )
            {
                station.InteractionController
                    .Deactivate();
            }

            processView = false;
            open = false;

            if (
                controller != null &&
                controllerWasEnabled
            )
            {
                controller.enabled = true;
            }

            if (
                hotbar != null &&
                hotbarWasEnabled
            )
            {
                hotbar.enabled = true;
            }

            if (
                interactor != null &&
                interactorWasEnabled
            )
            {
                interactor.enabled = true;
            }

            if (
                plugController != null &&
                plugControllerWasEnabled
            )
            {
                plugController.enabled = true;
            }

            if (
                inventoryHud != null &&
                inventoryHudWasEnabled
            )
            {
                inventoryHud.enabled = true;
            }

            Cursor.lockState =
                CursorLockMode.Locked;
            Cursor.visible = false;

            station = null;
            playerCamera = null;
            controller = null;
            hotbar = null;
            interactor = null;
            plugController = null;
            inventoryHud = null;
        }

        private void EnterProcessView()
        {
            if (
                station == null ||
                station.InteractionController == null
            )
            {
                return;
            }

            processView = true;

            station.InteractionController
                .Activate(playerCamera);
        }

        private void OnGUI()
        {
            if (
                !open ||
                station == null
            )
            {
                return;
            }

            EnsureStyles();

            if (processView)
            {
                DrawProcessOverlay();
            }
            else
            {
                DrawStationWindow();
            }
        }

        private void DrawStationWindow()
        {
            GUI.Box(
                new Rect(
                    0f,
                    0f,
                    Screen.width,
                    Screen.height
                ),
                GUIContent.none,
                overlayStyle
            );

            float width =
                Mathf.Min(
                    960f,
                    Screen.width - 40f
                );

            float height =
                Mathf.Min(
                    680f,
                    Screen.height - 40f
                );

            Rect window =
                new Rect(
                    (Screen.width - width) * 0.5f,
                    (Screen.height - height) * 0.5f,
                    width,
                    height
                );

            GUI.Box(
                window,
                GUIContent.none,
                panelStyle
            );

            GUI.Label(
                new Rect(
                    window.x + 28f,
                    window.y + 18f,
                    window.width - 270f,
                    42f
                ),
                "IMPRINT PRESS",
                headerStyle
            );

            GUI.Label(
                new Rect(
                    window.x + 30f,
                    window.y + 58f,
                    window.width - 280f,
                    28f
                ),
                "Opening Counterfeiting Branch",
                mutedStyle
            );

            GUI.Label(
                new Rect(
                    window.xMax - 235f,
                    window.y + 28f,
                    200f,
                    30f
                ),
                station.HasPower
                    ? "● POWERED"
                    : "● NO POWER",
                station.HasPower
                    ? goodStyle
                    : badStyle
            );

            if (station.HasPendingOutput)
            {
                DrawPendingOutput(window);
            }
            else
            {
                DrawRecipe(window);
            }

            if (
                GUI.Button(
                    new Rect(
                        window.xMax - 180f,
                        window.yMax - 58f,
                        150f,
                        36f
                    ),
                    "CLOSE",
                    buttonStyle
                )
            )
            {
                Close();
            }
        }

        private void DrawRecipe(Rect window)
        {
            CounterfeitRecipeDefinition recipe =
                station.Recipe;

            if (recipe == null)
            {
                GUI.Label(
                    new Rect(
                        window.x + 30f,
                        window.y + 120f,
                        window.width - 60f,
                        70f
                    ),
                    "No counterfeit recipe is configured.",
                    badStyle
                );
                return;
            }

            Rect left =
                new Rect(
                    window.x + 28f,
                    window.y + 108f,
                    window.width * 0.58f,
                    window.height - 194f
                );

            Rect right =
                new Rect(
                    left.xMax + 18f,
                    left.y,
                    window.xMax - left.xMax - 46f,
                    left.height
                );

            GUI.Box(
                left,
                GUIContent.none,
                cardStyle
            );

            GUI.Box(
                right,
                GUIContent.none,
                cardStyle
            );

            GUI.Label(
                new Rect(
                    left.x + 20f,
                    left.y + 18f,
                    left.width - 40f,
                    34f
                ),
                recipe.DisplayName,
                titleStyle
            );

            GUI.Label(
                new Rect(
                    left.x + 20f,
                    left.y + 58f,
                    left.width - 40f,
                    70f
                ),
                recipe.Description,
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    left.x + 20f,
                    left.y + 142f,
                    left.width - 40f,
                    28f
                ),
                "REQUIRED MATERIALS",
                orangeStyle
            );

            float rowY =
                left.y + 180f;

            DrawRequirement(
                left,
                ref rowY,
                recipe.BlankNoteStock,
                true
            );

            DrawRequirement(
                left,
                ref rowY,
                recipe.PigmentCapsule,
                true
            );

            DrawRequirement(
                left,
                ref rowY,
                recipe.SecurityFilm,
                true
            );

            DrawRequirement(
                left,
                ref rowY,
                recipe.PackagingMaterial,
                true
            );

            DrawRequirement(
                left,
                ref rowY,
                recipe.BasicToolkit,
                false
            );

            IndustryReputationSystem reputation =
                station.ReputationSystem;

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 18f,
                    right.width - 36f,
                    28f
                ),
                "COUNTERFEITING REPUTATION",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 55f,
                    right.width - 36f,
                    30f
                ),
                reputation != null
                    ? reputation
                        .GetCounterfeitingState()
                    : "Unknown Printer",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 92f,
                    right.width - 36f,
                    30f
                ),
                reputation != null
                    ? reputation.GetReputation(
                        IndustryReputationSystem
                            .CounterfeitingIndustryId
                      ) +
                      " reputation"
                    : "0 reputation",
                mutedStyle
            );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 148f,
                    right.width - 36f,
                    28f
                ),
                "MANUAL PROCESS",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 184f,
                    right.width - 36f,
                    212f
                ),
                "1. Load note stock\n" +
                "2. Load pigment\n" +
                "3. Align security film\n" +
                "4. Hold the print control\n" +
                "5. Move the printed sheet\n" +
                "6. Trim it with the toolkit\n" +
                "7. Package the cut stack\n" +
                "8. Add wrapping\n" +
                "9. Seal the bundle",
                bodyStyle
            );

            bool canStart =
                station.CanStart(
                    out string reason
                );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.yMax - 126f,
                    right.width - 36f,
                    60f
                ),
                canStart
                    ? "All materials are ready."
                    : reason,
                canStart
                    ? goodStyle
                    : badStyle
            );

            GUI.enabled = canStart;

            if (
                GUI.Button(
                    new Rect(
                        right.x + 18f,
                        right.yMax - 60f,
                        right.width - 36f,
                        42f
                    ),
                    "START COUNTERFEIT RUN",
                    canStart
                        ? buttonStyle
                        : disabledButtonStyle
                )
            )
            {
                if (
                    station.StartProduction(
                        out string _
                    )
                )
                {
                    EnterProcessView();
                }
            }

            GUI.enabled = true;
        }

        private void DrawRequirement(
            Rect area,
            ref float y,
            ItemDefinition item,
            bool consumed
        )
        {
            if (item == null)
            {
                return;
            }

            int owned =
                station.CountOwned(item);

            GUI.Label(
                new Rect(
                    area.x + 24f,
                    y,
                    area.width - 180f,
                    27f
                ),
                $"• 1× {item.DisplayName} " +
                (
                    consumed
                        ? "(consumed)"
                        : "(reusable)"
                ),
                owned > 0
                    ? goodStyle
                    : badStyle
            );

            GUI.Label(
                new Rect(
                    area.xMax - 130f,
                    y,
                    100f,
                    27f
                ),
                $"{owned} owned",
                owned > 0
                    ? goodStyle
                    : badStyle
            );

            y += 35f;
        }

        private void DrawPendingOutput(Rect window)
        {
            Rect card =
                new Rect(
                    window.x + 28f,
                    window.y + 112f,
                    window.width - 56f,
                    window.height - 214f
                );

            GUI.Box(
                card,
                GUIContent.none,
                cardStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 22f,
                    card.width - 48f,
                    36f
                ),
                "COUNTERFEIT RUN COMPLETE",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 82f,
                    card.width - 48f,
                    34f
                ),
                station.PendingOutputItem != null
                    ? station
                        .PendingOutputItem.DisplayName
                    : "Finished output",
                goodStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 130f,
                    card.width - 48f,
                    30f
                ),
                $"Quality: " +
                $"{station.PendingOutputQuality}",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 168f,
                    card.width - 48f,
                    30f
                ),
                $"Condition: " +
                $"{station.PendingOutputCondition:P0}",
                bodyStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        card.x + 24f,
                        card.yMax - 68f,
                        card.width - 48f,
                        44f
                    ),
                    "COLLECT REPLICA BUNDLE",
                    buttonStyle
                )
            )
            {
                station.CollectPendingOutput(
                    out string _
                );
            }
        }

        private void DrawProcessOverlay()
        {
            GUI.Box(
                new Rect(
                    18f,
                    18f,
                    Mathf.Min(
                        630f,
                        Screen.width - 36f
                    ),
                    154f
                ),
                GUIContent.none,
                panelStyle
            );

            GUI.Label(
                new Rect(
                    36f,
                    30f,
                    560f,
                    30f
                ),
                "COUNTERFEIT PRODUCTION",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    36f,
                    64f,
                    570f,
                    38f
                ),
                station.CurrentInstruction,
                station.HasPower
                    ? bodyStyle
                    : badStyle
            );

            GUI.Label(
                new Rect(
                    36f,
                    106f,
                    280f,
                    25f
                ),
                $"Step: " +
                $"{Mathf.Min(station.CurrentStep + 1, 9)}/9",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    300f,
                    106f,
                    220f,
                    25f
                ),
                $"Mistakes: " +
                $"{station.MistakeCount}",
                station.MistakeCount > 0
                    ? badStyle
                    : goodStyle
            );

            if (station.CurrentStep == 3)
            {
                DrawPrintProgress();
            }

            DrawProcessCallouts();
            DrawProcessLegend();

            if (station.HasPendingOutput)
            {
                GUI.Box(
                    new Rect(
                        Screen.width * 0.5f - 220f,
                        Screen.height - 116f,
                        440f,
                        88f
                    ),
                    GUIContent.none,
                    panelStyle
                );

                if (
                    GUI.Button(
                        new Rect(
                            Screen.width * 0.5f - 195f,
                            Screen.height - 96f,
                            390f,
                            46f
                        ),
                        "COLLECT REPLICA BUNDLE",
                        buttonStyle
                    )
                )
                {
                    if (
                        station.CollectPendingOutput(
                            out string _
                        )
                    )
                    {
                        Close();
                    }
                }
            }

            GUI.Label(
                new Rect(
                    22f,
                    Screen.height - 54f,
                    Screen.width - 220f,
                    30f
                ),
                "Mouse: drag materials • Hold PRINT • " +
                "Click toolkit and SEAL • Esc: exit view",
                mutedStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        Screen.width - 172f,
                        24f,
                        148f,
                        38f
                    ),
                    "EXIT VIEW",
                    buttonStyle
                )
            )
            {
                Close();
            }
        }


        private void DrawProcessCallouts()
        {
            CounterfeitPressInteractionController controller =
                station != null
                    ? station.InteractionController
                    : null;

            if (
                controller == null ||
                !controller.TryGetStepCallouts(
                    out string actionLabel,
                    out Vector2 actionPosition,
                    out string targetLabel,
                    out Vector2 targetPosition
                )
            )
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(actionLabel))
            {
                DrawCallout(
                    actionPosition,
                    actionLabel,
                    true
                );
            }

            if (!string.IsNullOrWhiteSpace(targetLabel))
            {
                DrawCallout(
                    targetPosition,
                    targetLabel,
                    false
                );
            }
        }

        private void DrawCallout(
            Vector2 position,
            string label,
            bool action
        )
        {
            const float width = 190f;
            const float height = 50f;

            float x =
                Mathf.Clamp(
                    position.x - width * 0.5f,
                    8f,
                    Screen.width - width - 8f
                );

            float yOffset =
                action
                    ? -64f
                    : 20f;

            float y =
                Mathf.Clamp(
                    position.y + yOffset,
                    178f,
                    Screen.height - height - 68f
                );

            Rect box =
                new Rect(
                    x,
                    y,
                    width,
                    height
                );

            GUI.Box(
                box,
                GUIContent.none,
                action
                    ? buttonStyle
                    : panelStyle
            );

            GUI.Label(
                new Rect(
                    box.x + 7f,
                    box.y + 5f,
                    box.width - 14f,
                    box.height - 10f
                ),
                label,
                action
                    ? calloutActionStyle
                    : calloutTargetStyle
            );

            GUI.Box(
                new Rect(
                    position.x - 6f,
                    position.y - 6f,
                    12f,
                    12f
                ),
                GUIContent.none,
                action
                    ? buttonStyle
                    : calloutTargetBoxStyle
            );
        }

        private void DrawProcessLegend()
        {
            const float width = 278f;

            Rect legend =
                new Rect(
                    Screen.width - width - 18f,
                    78f,
                    width,
                    92f
                );

            GUI.Box(
                legend,
                GUIContent.none,
                panelStyle
            );

            GUI.Label(
                new Rect(
                    legend.x + 12f,
                    legend.y + 10f,
                    legend.width - 24f,
                    24f
                ),
                "VISUAL GUIDE",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    legend.x + 12f,
                    legend.y + 36f,
                    legend.width - 24f,
                    48f
                ),
                "DRAG THIS = required material\n" +
                "DROP HERE = correct work area",
                mutedStyle
            );
        }

        private void DrawPrintProgress()
        {
            Rect background =
                new Rect(
                    36f,
                    136f,
                    560f,
                    12f
                );

            GUI.Box(
                background,
                GUIContent.none,
                cardStyle
            );

            Rect fill =
                new Rect(
                    background.x,
                    background.y,
                    background.width *
                    station.PrintProgress,
                    background.height
                );

            GUI.Box(
                fill,
                GUIContent.none,
                buttonStyle
            );
        }

        private void EnsureStyles()
        {
            if (overlayStyle != null)
            {
                return;
            }

            overlayStyle =
                BoxStyle(
                    new Color(
                        0.005f,
                        0.007f,
                        0.009f,
                        0.94f
                    )
                );

            panelStyle =
                BoxStyle(
                    new Color(
                        0.038f,
                        0.043f,
                        0.048f,
                        0.98f
                    )
                );

            cardStyle =
                BoxStyle(
                    new Color(
                        0.07f,
                        0.075f,
                        0.08f,
                        0.98f
                    )
                );

            headerStyle =
                LabelStyle(
                    30,
                    FontStyle.Bold,
                    new Color(
                        1f,
                        0.48f,
                        0.015f
                    )
                );

            titleStyle =
                LabelStyle(
                    21,
                    FontStyle.Bold,
                    Color.white
                );

            bodyStyle =
                LabelStyle(
                    15,
                    FontStyle.Normal,
                    new Color(
                        0.88f,
                        0.9f,
                        0.92f
                    )
                );

            bodyStyle.wordWrap = true;

            mutedStyle =
                new GUIStyle(bodyStyle);

            mutedStyle.normal.textColor =
                new Color(
                    0.58f,
                    0.62f,
                    0.66f
                );

            orangeStyle =
                new GUIStyle(bodyStyle);

            orangeStyle.fontStyle =
                FontStyle.Bold;

            orangeStyle.normal.textColor =
                new Color(
                    1f,
                    0.5f,
                    0.02f
                );

            goodStyle =
                new GUIStyle(bodyStyle);

            goodStyle.fontStyle =
                FontStyle.Bold;

            goodStyle.normal.textColor =
                new Color(
                    0.32f,
                    1f,
                    0.48f
                );

            badStyle =
                new GUIStyle(bodyStyle);

            badStyle.fontStyle =
                FontStyle.Bold;

            badStyle.normal.textColor =
                new Color(
                    1f,
                    0.28f,
                    0.2f
                );

            buttonStyle =
                ButtonStyle(
                    new Color(
                        0.82f,
                        0.28f,
                        0.01f
                    )
                );

            disabledButtonStyle =
                ButtonStyle(
                    new Color(
                        0.15f,
                        0.16f,
                        0.17f
                    )
                );

            disabledButtonStyle
                .normal.textColor =
                    new Color(
                        0.42f,
                        0.44f,
                        0.46f
                    );

            calloutActionStyle =
                new GUIStyle(bodyStyle)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };

            calloutActionStyle.normal.textColor =
                Color.white;

            calloutTargetStyle =
                new GUIStyle(calloutActionStyle);

            calloutTargetStyle.normal.textColor =
                new Color(
                    1f,
                    0.64f,
                    0.08f
                );

            calloutTargetBoxStyle =
                BoxStyle(
                    new Color(
                        1f,
                        0.54f,
                        0.02f,
                        1f
                    )
                );
        }

        private static GUIStyle BoxStyle(
            Color color
        )
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.box);

            style.normal.background =
                MakeTexture(color);

            return style;
        }

        private static GUIStyle LabelStyle(
            int size,
            FontStyle fontStyle,
            Color color
        )
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = size,
                    fontStyle = fontStyle,
                    alignment =
                        TextAnchor.UpperLeft
                };

            style.normal.textColor = color;
            return style;
        }

        private static GUIStyle ButtonStyle(
            Color color
        )
        {
            GUIStyle style =
                new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };

            style.normal.textColor =
                Color.white;

            style.normal.background =
                MakeTexture(color);

            style.hover.background =
                MakeTexture(
                    new Color(
                        Mathf.Clamp01(
                            color.r * 1.18f
                        ),
                        Mathf.Clamp01(
                            color.g * 1.18f
                        ),
                        Mathf.Clamp01(
                            color.b * 1.18f
                        ),
                        color.a
                    )
                );

            return style;
        }

        private static Texture2D MakeTexture(
            Color color
        )
        {
            Texture2D texture =
                new Texture2D(1, 1)
                {
                    hideFlags =
                        HideFlags.HideAndDontSave
                };

            texture.SetPixel(
                0,
                0,
                color
            );

            texture.Apply();
            return texture;
        }
    }
}
