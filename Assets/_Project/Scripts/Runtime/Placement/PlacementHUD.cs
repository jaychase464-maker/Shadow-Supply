using UnityEngine;

namespace ShadowSupply.Placement
{
    public sealed class PlacementHUD : MonoBehaviour
    {
        [SerializeField] private PlacementController placementController;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle statusStyle;
        private GUIStyle bodyStyle;
        private GUIStyle keyStyle;

        public void Configure(
            PlacementController controller
        )
        {
            placementController = controller;
        }

        private void Awake()
        {
            placementController ??=
                FindFirstObjectByType<PlacementController>();
        }

        private void OnGUI()
        {
            if (
                placementController == null ||
                !placementController.BuildModeActive
            )
            {
                return;
            }

            EnsureStyles();

            const float width = 420f;
            const float height = 172f;

            Rect panel = new Rect(
                22f,
                Screen.height - height - 24f,
                width,
                height
            );

            GUI.Box(
                panel,
                GUIContent.none,
                panelStyle
            );

            PlaceableDefinition definition =
                placementController.CurrentDefinition;

            string title =
                definition != null
                    ? definition.DisplayName.ToUpperInvariant()
                    : "PLACEMENT MODE";

            GUI.Label(
                new Rect(
                    panel.x + 18f,
                    panel.y + 12f,
                    panel.width - 36f,
                    30f
                ),
                title,
                titleStyle
            );

            Color previous = GUI.color;

            GUI.color =
                placementController.PreviewValid
                    ? new Color(0.35f, 0.95f, 0.55f)
                    : new Color(1f, 0.38f, 0.25f);

            GUI.Label(
                new Rect(
                    panel.x + 18f,
                    panel.y + 46f,
                    panel.width - 36f,
                    24f
                ),
                placementController.StatusMessage,
                statusStyle
            );

            GUI.color = previous;

            GUI.Label(
                new Rect(
                    panel.x + 18f,
                    panel.y + 76f,
                    panel.width - 36f,
                    45f
                ),
                "1–3 Select  •  R / Mouse Wheel Rotate  •  Left Click Place\n" +
                "Delete Remove Target  •  Right Click / B Exit",
                bodyStyle
            );

            GUI.Box(
                new Rect(
                    panel.x + 18f,
                    panel.yMax - 38f,
                    34f,
                    25f
                ),
                "B",
                keyStyle
            );

            GUI.Label(
                new Rect(
                    panel.x + 60f,
                    panel.yMax - 40f,
                    panel.width - 78f,
                    28f
                ),
                "BUILD MODE ACTIVE",
                bodyStyle
            );
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle =
                CreateBoxStyle(
                    new Color(
                        0.018f,
                        0.023f,
                        0.025f,
                        0.96f
                    )
                );

            titleStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal =
                    {
                        textColor =
                            new Color(0.95f, 0.55f, 0.1f)
                    }
                };

            statusStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };

            bodyStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    normal =
                    {
                        textColor =
                            new Color(0.8f, 0.83f, 0.82f)
                    }
                };

            keyStyle =
                new GUIStyle(GUI.skin.box)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal =
                    {
                        background =
                            MakeTexture(
                                new Color(
                                    0.16f,
                                    0.18f,
                                    0.18f,
                                    1f
                                )
                            ),
                        textColor = Color.white
                    }
                };
        }

        private static GUIStyle CreateBoxStyle(
            Color color
        )
        {
            return new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = MakeTexture(color)
                },
                border =
                    new RectOffset(1, 1, 1, 1)
            };
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

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }
    }
}
