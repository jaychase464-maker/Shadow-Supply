using UnityEngine;

namespace ShadowSupply.UI
{
    public sealed class InteractionPromptHUD : MonoBehaviour
    {
        [SerializeField] private string keyLabel = "E";

        private string promptText;
        private GUIStyle containerStyle;
        private GUIStyle keyStyle;
        private GUIStyle textStyle;

        public void SetPrompt(string prompt)
        {
            promptText = prompt;
        }

        public void ClearPrompt()
        {
            promptText = string.Empty;
        }

        private void OnGUI()
        {
            if (string.IsNullOrWhiteSpace(promptText))
            {
                return;
            }

            EnsureStyles();

            const float width = 390f;
            const float height = 64f;

            Rect container = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height - 145f,
                width,
                height
            );

            GUI.Box(container, GUIContent.none, containerStyle);

            Rect keyRect = new Rect(container.x + 14f, container.y + 12f, 40f, 40f);
            GUI.Box(keyRect, keyLabel, keyStyle);

            Rect textRect = new Rect(
                keyRect.xMax + 14f,
                container.y,
                container.width - 82f,
                container.height
            );

            GUI.Label(textRect, promptText, textStyle);
        }

        private void EnsureStyles()
        {
            if (containerStyle != null)
            {
                return;
            }

            Texture2D containerTexture = MakeTexture(new Color(0.025f, 0.03f, 0.035f, 0.92f));
            Texture2D keyTexture = MakeTexture(new Color(0.92f, 0.55f, 0.12f, 1f));

            containerStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = containerTexture },
                border = new RectOffset(1, 1, 1, 1)
            };

            keyStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    background = keyTexture,
                    textColor = Color.black
                }
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.93f, 0.95f, 0.96f, 1f)
                }
            };
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1)
            {
                name = "Runtime HUD Color",
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
