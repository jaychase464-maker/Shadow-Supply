using System;
using ShadowSupply.SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.UI
{
    public sealed class SaveSystemHUD : MonoBehaviour
    {
        [SerializeField] private SaveManager saveManager;
        [SerializeField, Min(0.5f)] private float statusDuration = 4f;

        private SaveSlotMetadata[] metadata;
        private string statusMessage;
        private bool statusSuccess;
        private float statusUntil;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle normalStyle;
        private GUIStyle selectedStyle;
        private GUIStyle successStyle;
        private GUIStyle errorStyle;

        private void Awake()
        {
            saveManager ??=
                GetComponent<SaveManager>();

            saveManager ??=
                SaveManager.Instance;

            RefreshMetadata();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(SaveManager manager)
        {
            Unsubscribe();
            saveManager = manager;
            RefreshMetadata();

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Update()
        {
            if (saveManager == null)
            {
                saveManager = SaveManager.Instance;

                if (saveManager == null)
                {
                    return;
                }

                Subscribe();
                RefreshMetadata();
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                saveManager.SetActiveSlot(1);
            }
            else if (
                keyboard.f2Key.wasPressedThisFrame &&
                saveManager.SlotCount >= 2
            )
            {
                saveManager.SetActiveSlot(2);
            }
            else if (
                keyboard.f3Key.wasPressedThisFrame &&
                saveManager.SlotCount >= 3
            )
            {
                saveManager.SetActiveSlot(3);
            }

            if (keyboard.f5Key.wasPressedThisFrame)
            {
                saveManager.SaveActiveSlot();
            }

            if (keyboard.f9Key.wasPressedThisFrame)
            {
                saveManager.LoadActiveSlot();
            }
        }

        private void OnGUI()
        {
            if (saveManager == null)
            {
                return;
            }

            EnsureStyles();

            const float width = 280f;
            float height =
                66f +
                saveManager.SlotCount * 24f;

            Rect panel = new Rect(
                Screen.width - width - 18f,
                18f,
                width,
                height
            );

            GUI.Box(
                panel,
                GUIContent.none,
                panelStyle
            );

            GUI.Label(
                new Rect(
                    panel.x + 14f,
                    panel.y + 8f,
                    panel.width - 28f,
                    26f
                ),
                "SAVE SYSTEM",
                titleStyle
            );

            for (
                int slot = 1;
                slot <= saveManager.SlotCount;
                slot++
            )
            {
                Rect row = new Rect(
                    panel.x + 14f,
                    panel.y + 34f + (slot - 1) * 24f,
                    panel.width - 28f,
                    22f
                );

                bool selected =
                    slot == saveManager.ActiveSlot;

                GUI.Label(
                    row,
                    FormatSlot(slot),
                    selected
                        ? selectedStyle
                        : normalStyle
                );
            }

            GUI.Label(
                new Rect(
                    panel.x + 14f,
                    panel.yMax - 25f,
                    panel.width - 28f,
                    20f
                ),
                "F1–F3 Select  •  F5 Save  •  F9 Load",
                normalStyle
            );

            if (
                !string.IsNullOrWhiteSpace(statusMessage) &&
                Time.unscaledTime <= statusUntil
            )
            {
                Rect statusRect = new Rect(
                    panel.x,
                    panel.yMax + 8f,
                    panel.width,
                    32f
                );

                GUI.Label(
                    statusRect,
                    statusMessage,
                    statusSuccess
                        ? successStyle
                        : errorStyle
                );
            }
        }

        private string FormatSlot(int slot)
        {
            SaveSlotMetadata slotMetadata =
                metadata != null &&
                slot - 1 < metadata.Length
                    ? metadata[slot - 1]
                    : null;

            if (slotMetadata == null)
            {
                return $"F{slot}  SLOT {slot}  —  EMPTY";
            }

            string timestamp = "Saved";

            if (
                DateTime.TryParse(
                    slotMetadata.savedAtUtc,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out DateTime parsed
                )
            )
            {
                timestamp =
                    parsed.ToLocalTime()
                        .ToString("MMM d  h:mm tt");
            }

            return
                $"F{slot}  SLOT {slot}  —  {timestamp}";
        }

        private void Subscribe()
        {
            if (saveManager == null)
            {
                return;
            }

            saveManager.StatusChanged -= HandleStatus;
            saveManager.SlotsChanged -= RefreshMetadata;
            saveManager.ActiveSlotChanged -= HandleActiveSlotChanged;

            saveManager.StatusChanged += HandleStatus;
            saveManager.SlotsChanged += RefreshMetadata;
            saveManager.ActiveSlotChanged += HandleActiveSlotChanged;
        }

        private void Unsubscribe()
        {
            if (saveManager == null)
            {
                return;
            }

            saveManager.StatusChanged -= HandleStatus;
            saveManager.SlotsChanged -= RefreshMetadata;
            saveManager.ActiveSlotChanged -= HandleActiveSlotChanged;
        }

        private void HandleStatus(
            string message,
            bool success
        )
        {
            statusMessage = message;
            statusSuccess = success;
            statusUntil =
                Time.unscaledTime + statusDuration;

            RefreshMetadata();
        }

        private void HandleActiveSlotChanged(int slot)
        {
            RefreshMetadata();
        }

        private void RefreshMetadata()
        {
            if (saveManager == null)
            {
                metadata = Array.Empty<SaveSlotMetadata>();
                return;
            }

            metadata =
                new SaveSlotMetadata[saveManager.SlotCount];

            for (
                int slot = 1;
                slot <= saveManager.SlotCount;
                slot++
            )
            {
                saveManager.TryGetMetadata(
                    slot,
                    out metadata[slot - 1]
                );
            }
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle = CreateBoxStyle(
                new Color(
                    0.025f,
                    0.03f,
                    0.035f,
                    0.94f
                )
            );

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor =
                        new Color(
                            0.95f,
                            0.58f,
                            0.13f
                        )
                }
            };

            normalStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal =
                {
                    textColor =
                        new Color(
                            0.72f,
                            0.76f,
                            0.79f
                        )
                }
            };

            selectedStyle =
                new GUIStyle(normalStyle)
                {
                    fontStyle = FontStyle.Bold,
                    normal =
                    {
                        textColor =
                            new Color(
                                0.98f,
                                0.64f,
                                0.18f
                            )
                    }
                };

            successStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor =
                        new Color(
                            0.35f,
                            0.92f,
                            0.64f
                        )
                }
            };

            errorStyle =
                new GUIStyle(successStyle)
                {
                    normal =
                    {
                        textColor =
                            new Color(
                                1f,
                                0.38f,
                                0.32f
                            )
                    }
                };
        }

        private static GUIStyle CreateBoxStyle(
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

            return new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = texture
                }
            };
        }
    }
}
