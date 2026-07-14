using ShadowSupply.Electrical;
using ShadowSupply.Interaction;
using ShadowSupply.Inventory;
using ShadowSupply.Player;
using ShadowSupply.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Production
{
    [DisallowMultipleComponent]
    public sealed class ProductionWorkbenchHUD :
        MonoBehaviour
    {
        public static ProductionWorkbenchHUD Instance
        {
            get;
            private set;
        }

        private PoweredWorkbenchProduction workbench;
        private GameObject player;
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
        private bool assemblyView;

        private GUIStyle overlayStyle;
        private GUIStyle panelStyle;
        private GUIStyle headerStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle mutedStyle;
        private GUIStyle goodStyle;
        private GUIStyle badStyle;
        private GUIStyle orangeStyle;
        private GUIStyle buttonStyle;
        private GUIStyle disabledButtonStyle;
        private GUIStyle centeredStyle;

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
            PoweredWorkbenchProduction targetWorkbench,
            GameObject interactingPlayer
        )
        {
            if (
                open ||
                targetWorkbench == null ||
                interactingPlayer == null
            )
            {
                return;
            }

            workbench = targetWorkbench;
            player = interactingPlayer;

            playerCamera =
                interactingPlayer
                    .GetComponentInChildren<Camera>(true);

            controller =
                interactingPlayer.GetComponent<
                    FirstPersonController
                >();

            hotbar =
                interactingPlayer.GetComponent<
                    HotbarController
                >();

            interactor =
                interactingPlayer.GetComponent<
                    PlayerInteractor
                >();

            plugController =
                interactingPlayer.GetComponent<
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

            if (workbench.IsProducing)
            {
                EnterAssemblyView();
            }
        }

        public void Close()
        {
            if (!open)
            {
                return;
            }

            if (
                assemblyView &&
                workbench != null &&
                workbench.AssemblyController != null
            )
            {
                workbench.AssemblyController
                    .Deactivate();
            }

            assemblyView = false;
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

            workbench = null;
            player = null;
            playerCamera = null;
            controller = null;
            hotbar = null;
            interactor = null;
            plugController = null;
            inventoryHud = null;
        }

        private void EnterAssemblyView()
        {
            if (
                workbench == null ||
                workbench.AssemblyController == null
            )
            {
                return;
            }

            assemblyView = true;

            workbench.AssemblyController
                .Activate(playerCamera);
        }

        private void OnGUI()
        {
            if (
                !open ||
                workbench == null
            )
            {
                return;
            }

            EnsureStyles();

            if (assemblyView)
            {
                DrawAssemblyOverlay();
                return;
            }

            DrawRecipeWindow();
        }

        private void DrawRecipeWindow()
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
                    920f,
                    Screen.width - 40f
                );

            float height =
                Mathf.Min(
                    650f,
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
                    window.y + 20f,
                    window.width - 250f,
                    40f
                ),
                "INTERACTIVE WORKBENCH",
                headerStyle
            );

            GUI.Label(
                new Rect(
                    window.x + 30f,
                    window.y + 60f,
                    window.width - 260f,
                    30f
                ),
                "Every recipe must be physically assembled.",
                mutedStyle
            );

            GUI.Label(
                new Rect(
                    window.xMax - 210f,
                    window.y + 28f,
                    180f,
                    30f
                ),
                workbench.HasPower
                    ? "● POWERED"
                    : "● NO POWER",
                workbench.HasPower
                    ? goodStyle
                    : badStyle
            );

            ProductionRecipe recipe =
                GetPrimaryRecipe();

            if (workbench.HasPendingOutput)
            {
                DrawPendingOutput(
                    window
                );
            }
            else if (recipe == null)
            {
                GUI.Label(
                    new Rect(
                        window.x + 32f,
                        window.y + 130f,
                        window.width - 64f,
                        80f
                    ),
                    "No interactive recipe is assigned.",
                    badStyle
                );
            }
            else
            {
                DrawRecipeDetails(
                    window,
                    recipe
                );
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

        private void DrawRecipeDetails(
            Rect window,
            ProductionRecipe recipe
        )
        {
            Rect left =
                new Rect(
                    window.x + 28f,
                    window.y + 110f,
                    window.width * 0.56f,
                    window.height - 200f
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
                panelStyle
            );

            GUI.Box(
                right,
                GUIContent.none,
                panelStyle
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
                    left.y + 56f,
                    left.width - 40f,
                    66f
                ),
                recipe.Description,
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    left.x + 20f,
                    left.y + 132f,
                    left.width - 40f,
                    28f
                ),
                "MATERIALS ON THE TABLE",
                orangeStyle
            );

            float rowY =
                left.y + 166f;

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
                    continue;
                }

                int owned =
                    workbench.CountOwned(
                        ingredient.Item
                    );

                bool enough =
                    owned >= ingredient.Quantity;

                string action =
                    ingredient.Consumed
                        ? "drag into package"
                        : "click to prepare";

                GUI.Label(
                    new Rect(
                        left.x + 26f,
                        rowY,
                        left.width - 170f,
                        28f
                    ),
                    $"• {ingredient.Quantity}× " +
                    $"{ingredient.Item.DisplayName} — " +
                    action,
                    enough
                        ? goodStyle
                        : badStyle
                );

                GUI.Label(
                    new Rect(
                        left.xMax - 130f,
                        rowY,
                        100f,
                        28f
                    ),
                    $"{owned} owned",
                    enough
                        ? goodStyle
                        : badStyle
                );

                rowY += 34f;
            }

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 18f,
                    right.width - 36f,
                    30f
                ),
                "INTERACTION FLOW",
                orangeStyle
            );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 60f,
                    right.width - 36f,
                    190f
                ),
                "1. Start assembly\n\n" +
                "2. Click reusable tools\n\n" +
                "3. Drag each loose part into the box\n\n" +
                "4. Click the lid to close and seal it\n\n" +
                "5. Collect the finished package",
                bodyStyle
            );

            workbench.CanStartRecipe(
                recipe,
                out string reason
            );

            GUI.Label(
                new Rect(
                    right.x + 18f,
                    right.y + 270f,
                    right.width - 36f,
                    90f
                ),
                string.IsNullOrWhiteSpace(reason)
                    ? "All materials are ready."
                    : reason,
                string.IsNullOrWhiteSpace(reason)
                    ? goodStyle
                    : badStyle
            );

            bool canStart =
                workbench.CanStartRecipe(
                    recipe,
                    out string _
                );

            GUI.enabled = canStart;

            if (
                GUI.Button(
                    new Rect(
                        right.x + 18f,
                        right.yMax - 62f,
                        right.width - 36f,
                        42f
                    ),
                    "START INTERACTIVE ASSEMBLY",
                    canStart
                        ? buttonStyle
                        : disabledButtonStyle
                )
            )
            {
                if (
                    workbench.StartRecipe(
                        recipe,
                        out string _
                    )
                )
                {
                    EnterAssemblyView();
                }
            }

            GUI.enabled = true;
        }

        private void DrawPendingOutput(
            Rect window
        )
        {
            Rect card =
                new Rect(
                    window.x + 28f,
                    window.y + 120f,
                    window.width - 56f,
                    window.height - 220f
                );

            GUI.Box(
                card,
                GUIContent.none,
                panelStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 22f,
                    card.width - 48f,
                    36f
                ),
                "PACKAGE COMPLETE",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 78f,
                    card.width - 48f,
                    34f
                ),
                workbench.PendingOutputItem.DisplayName,
                goodStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 120f,
                    card.width - 48f,
                    32f
                ),
                $"Quality: " +
                $"{workbench.PendingOutputQuality}",
                bodyStyle
            );

            GUI.Label(
                new Rect(
                    card.x + 24f,
                    card.y + 157f,
                    card.width - 48f,
                    32f
                ),
                $"Condition: " +
                $"{workbench.PendingOutputCondition:P0}",
                bodyStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        card.x + 24f,
                        card.yMax - 70f,
                        card.width - 48f,
                        46f
                    ),
                    "COLLECT FINISHED PACKAGE",
                    buttonStyle
                )
            )
            {
                workbench.CollectPendingOutput(
                    out string _
                );
            }
        }

        private void DrawAssemblyOverlay()
        {
            InteractiveAssemblyController assembly =
                workbench.AssemblyController;

            if (assembly == null)
            {
                assemblyView = false;
                return;
            }

            GUI.Box(
                new Rect(
                    18f,
                    18f,
                    Mathf.Min(
                        560f,
                        Screen.width - 36f
                    ),
                    124f
                ),
                GUIContent.none,
                panelStyle
            );

            GUI.Label(
                new Rect(
                    36f,
                    30f,
                    500f,
                    30f
                ),
                "MANUAL ASSEMBLY",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    36f,
                    62f,
                    500f,
                    28f
                ),
                assembly.InstructionText,
                workbench.HasPower
                    ? bodyStyle
                    : badStyle
            );

            GUI.Label(
                new Rect(
                    36f,
                    94f,
                    500f,
                    24f
                ),
                $"Steps: " +
                $"{assembly.CompletedStepCount}/" +
                $"{assembly.TotalStepCount}",
                orangeStyle
            );

            if (workbench.HasPendingOutput)
            {
                GUI.Box(
                    new Rect(
                        Screen.width * 0.5f - 210f,
                        Screen.height - 116f,
                        420f,
                        88f
                    ),
                    GUIContent.none,
                    panelStyle
                );

                if (
                    GUI.Button(
                        new Rect(
                            Screen.width * 0.5f - 185f,
                            Screen.height - 96f,
                            370f,
                            46f
                        ),
                        "COLLECT FINISHED PACKAGE",
                        buttonStyle
                    )
                )
                {
                    if (
                        workbench.CollectPendingOutput(
                            out string _
                        )
                    )
                    {
                        Close();
                    }
                }
            }
            else
            {
                GUI.Label(
                    new Rect(
                        22f,
                        Screen.height - 54f,
                        Screen.width - 240f,
                        30f
                    ),
                    "Mouse: drag parts • Click tools • " +
                    "Click lid when highlighted • Esc: exit",
                    centeredStyle
                );
            }

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

        private ProductionRecipe GetPrimaryRecipe()
        {
            if (
                workbench == null ||
                workbench.RecipeDatabase == null ||
                workbench.RecipeDatabase.Recipes.Count == 0
            )
            {
                return null;
            }

            return
                workbench.RecipeDatabase.Recipes[0];
        }

        private void EnsureStyles()
        {
            if (overlayStyle != null)
            {
                return;
            }

            overlayStyle =
                new GUIStyle(GUI.skin.box);

            overlayStyle.normal.background =
                MakeTexture(
                    new Color(
                        0.005f,
                        0.007f,
                        0.009f,
                        0.94f
                    )
                );

            panelStyle =
                new GUIStyle(GUI.skin.box);

            panelStyle.normal.background =
                MakeTexture(
                    new Color(
                        0.045f,
                        0.05f,
                        0.055f,
                        0.96f
                    )
                );

            headerStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 29,
                    fontStyle = FontStyle.Bold
                };

            headerStyle.normal.textColor =
                new Color(
                    1f,
                    0.47f,
                    0.015f
                );

            titleStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 21,
                    fontStyle = FontStyle.Bold
                };

            titleStyle.normal.textColor =
                Color.white;

            bodyStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    wordWrap = true,
                    alignment =
                        TextAnchor.UpperLeft
                };

            bodyStyle.normal.textColor =
                new Color(
                    0.87f,
                    0.89f,
                    0.91f
                );

            mutedStyle =
                new GUIStyle(bodyStyle);

            mutedStyle.normal.textColor =
                new Color(
                    0.58f,
                    0.61f,
                    0.64f
                );

            goodStyle =
                new GUIStyle(bodyStyle);

            goodStyle.normal.textColor =
                new Color(
                    0.34f,
                    1f,
                    0.48f
                );

            badStyle =
                new GUIStyle(bodyStyle);

            badStyle.normal.textColor =
                new Color(
                    1f,
                    0.28f,
                    0.2f
                );

            orangeStyle =
                new GUIStyle(bodyStyle)
                {
                    fontStyle = FontStyle.Bold
                };

            orangeStyle.normal.textColor =
                new Color(
                    1f,
                    0.48f,
                    0.02f
                );

            buttonStyle =
                new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };

            buttonStyle.normal.textColor =
                Color.white;

            buttonStyle.normal.background =
                MakeTexture(
                    new Color(
                        0.82f,
                        0.28f,
                        0.01f
                    )
                );

            buttonStyle.hover.background =
                MakeTexture(
                    new Color(
                        1f,
                        0.43f,
                        0.02f
                    )
                );

            disabledButtonStyle =
                new GUIStyle(buttonStyle);

            disabledButtonStyle.normal.background =
                MakeTexture(
                    new Color(
                        0.16f,
                        0.17f,
                        0.18f
                    )
                );

            disabledButtonStyle.normal.textColor =
                new Color(
                    0.42f,
                    0.44f,
                    0.46f
                );

            centeredStyle =
                new GUIStyle(bodyStyle)
                {
                    alignment =
                        TextAnchor.MiddleLeft
                };

            centeredStyle.normal.textColor =
                new Color(
                    0.85f,
                    0.87f,
                    0.89f
                );
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
