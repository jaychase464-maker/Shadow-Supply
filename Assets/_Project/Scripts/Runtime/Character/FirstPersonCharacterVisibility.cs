using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class FirstPersonCharacterVisibility : MonoBehaviour
    {
        [SerializeField] private Camera localCamera;
        [SerializeField] private string hiddenLayerName =
            "FirstPersonHidden";
        [SerializeField] private List<GameObject> hiddenFromLocalView =
            new List<GameObject>();

        public void Configure(
            Camera cameraReference,
            IEnumerable<GameObject> hiddenObjects
        )
        {
            localCamera = cameraReference;
            hiddenFromLocalView.Clear();

            if (hiddenObjects != null)
            {
                hiddenFromLocalView.AddRange(hiddenObjects);
            }

            Apply();
        }

        public void Apply()
        {
            if (localCamera == null)
            {
                localCamera =
                    GetComponentInParent<Camera>();
            }

            int hiddenLayer =
                LayerMask.NameToLayer(hiddenLayerName);

            if (
                hiddenLayer < 0 ||
                localCamera == null
            )
            {
                return;
            }

            foreach (GameObject target in hiddenFromLocalView)
            {
                if (target != null)
                {
                    SetLayerRecursively(
                        target,
                        hiddenLayer
                    );
                }
            }

            localCamera.cullingMask &=
                ~(1 << hiddenLayer);
        }

        private static void SetLayerRecursively(
            GameObject target,
            int layer
        )
        {
            target.layer = layer;

            foreach (Transform child in target.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(
                        child.gameObject,
                        layer
                    );
                }
            }
        }
    }
}
