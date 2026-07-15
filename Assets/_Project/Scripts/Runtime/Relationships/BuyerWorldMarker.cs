using UnityEngine;

namespace ShadowSupply.Relationships
{
    [DisallowMultipleComponent]
    public sealed class BuyerWorldMarker : MonoBehaviour
    {
        private Camera targetCamera;

        private void LateUpdate()
        {
            targetCamera ??= Camera.main;

            if (targetCamera == null)
            {
                return;
            }

            Vector3 direction =
                transform.position - targetCamera.transform.position;

            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
