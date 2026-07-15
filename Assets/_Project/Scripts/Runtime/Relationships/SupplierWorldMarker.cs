using UnityEngine;

namespace ShadowSupply.Relationships
{
    [DisallowMultipleComponent]
    public sealed class SupplierWorldMarker :
        MonoBehaviour
    {
        [SerializeField]
        private SupplierNPC supplier;
        [SerializeField]
        private TextMesh textMesh;

        private Camera targetCamera;
        private bool lastUnlockedState;

        public void Configure(
            SupplierNPC targetSupplier,
            TextMesh targetText
        )
        {
            supplier = targetSupplier;
            textMesh = targetText;
            RefreshText(true);
        }

        private void Awake()
        {
            supplier ??=
                GetComponentInParent<SupplierNPC>();

            textMesh ??=
                GetComponent<TextMesh>();

            RefreshText(true);
        }

        private void LateUpdate()
        {
            targetCamera ??= Camera.main;

            if (targetCamera != null)
            {
                Vector3 direction =
                    transform.position -
                    targetCamera.transform.position;

                if (
                    direction.sqrMagnitude >
                    0.001f
                )
                {
                    transform.rotation =
                        Quaternion.LookRotation(
                            direction
                        );
                }
            }

            RefreshText(false);
        }

        private void RefreshText(
            bool force
        )
        {
            if (
                supplier == null ||
                textMesh == null
            )
            {
                return;
            }

            bool unlocked =
                supplier.ReferralUnlocked;

            if (
                !force &&
                unlocked == lastUnlockedState
            )
            {
                return;
            }

            lastUnlockedState = unlocked;

            textMesh.text =
                unlocked
                    ? supplier.DisplayName
                        .ToUpperInvariant() +
                      "\nSUPPLIER"
                    : "UNKNOWN BROKER\nREFERRAL REQUIRED";

            textMesh.color =
                unlocked
                    ? new Color(
                        1f,
                        0.45f,
                        0.02f
                    )
                    : new Color(
                        0.62f,
                        0.62f,
                        0.62f
                    );
        }
    }
}
