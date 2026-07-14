using UnityEngine;

namespace ShadowSupply.Electrical
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class PowerCableVisual : MonoBehaviour
    {
        [SerializeField] private Transform cableAnchor;
        [SerializeField] private Transform plugTransform;
        [SerializeField, Min(0.25f)] private float maximumLength =
            3f;
        [SerializeField, Range(4, 32)] private int segments = 16;
        [SerializeField, Min(0f)] private float maximumSag = 0.65f;
        [SerializeField, Min(0.001f)] private float cableWidth =
            0.025f;

        [Header("Floor Following")]
        [SerializeField] private LayerMask floorProbeMask = ~0;
        [SerializeField, Min(0f)] private float floorClearance =
            0.018f;
        [SerializeField, Min(0.1f)] private float floorProbeHeight =
            1.25f;
        [SerializeField, Min(0.1f)] private float floorProbeDepth =
            2.5f;
        [SerializeField, Min(0f)] private float endpointHeightAllowance =
            0.12f;

        private readonly RaycastHit[] floorHitBuffer =
            new RaycastHit[16];

        private LineRenderer lineRenderer;

        private void Awake()
        {
            ResolveRenderer();
        }

        private void LateUpdate()
        {
            DrawCable();
        }

        public void Configure(
            Transform anchor,
            Transform plug,
            float cableLength
        )
        {
            cableAnchor = anchor;
            plugTransform = plug;
            maximumLength = Mathf.Max(0.25f, cableLength);
            ResolveRenderer();
            DrawCable();
        }

        private void ResolveRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer =
                    GetComponent<LineRenderer>();
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount =
                Mathf.Max(4, segments);
            lineRenderer.startWidth = cableWidth;
            lineRenderer.endWidth = cableWidth;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 3;
        }

        private void DrawCable()
        {
            if (
                lineRenderer == null ||
                cableAnchor == null ||
                plugTransform == null
            )
            {
                return;
            }

            int pointCount =
                Mathf.Max(4, segments);

            if (lineRenderer.positionCount != pointCount)
            {
                lineRenderer.positionCount =
                    pointCount;
            }

            Vector3 start =
                cableAnchor.position;

            Vector3 end =
                plugTransform.position;

            float directDistance =
                Vector3.Distance(start, end);

            float slack =
                Mathf.Max(
                    0f,
                    maximumLength - directDistance
                );

            float sag =
                Mathf.Min(
                    maximumSag,
                    slack * 0.38f
                );

            float maximumFloorHeight =
                Mathf.Min(start.y, end.y) +
                endpointHeightAllowance;

            for (
                int index = 0;
                index < pointCount;
                index++
            )
            {
                float t =
                    index /
                    (float)(pointCount - 1);

                Vector3 point =
                    Vector3.Lerp(start, end, t);

                point +=
                    Vector3.down *
                    (
                        4f *
                        t *
                        (1f - t) *
                        sag
                    );

                if (
                    index > 0 &&
                    index < pointCount - 1
                )
                {
                    point =
                        ClampPointAboveFloor(
                            point,
                            maximumFloorHeight
                        );
                }

                lineRenderer.SetPosition(
                    index,
                    point
                );
            }
        }

        private Vector3 ClampPointAboveFloor(
            Vector3 point,
            float maximumFloorHeight
        )
        {
            float originHeight =
                Mathf.Max(
                    point.y + floorProbeHeight,
                    maximumFloorHeight +
                    floorProbeHeight
                );

            Vector3 origin =
                new Vector3(
                    point.x,
                    originHeight,
                    point.z
                );

            float castDistance =
                floorProbeHeight +
                floorProbeDepth +
                Mathf.Max(
                    0f,
                    maximumFloorHeight - point.y
                );

            int hitCount =
                Physics.RaycastNonAlloc(
                    origin,
                    Vector3.down,
                    floorHitBuffer,
                    castDistance,
                    floorProbeMask,
                    QueryTriggerInteraction.Ignore
                );

            bool foundFloor = false;
            float bestFloorHeight =
                float.NegativeInfinity;

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                RaycastHit hit =
                    floorHitBuffer[index];

                if (
                    hit.collider == null ||
                    hit.normal.y < 0.55f ||
                    hit.point.y >
                    maximumFloorHeight
                )
                {
                    continue;
                }

                if (
                    cableAnchor != null &&
                    cableAnchor.parent != null &&
                    hit.collider.transform.IsChildOf(
                        cableAnchor.parent
                    )
                )
                {
                    continue;
                }

                if (
                    plugTransform != null &&
                    hit.collider.transform.IsChildOf(
                        plugTransform
                    )
                )
                {
                    continue;
                }

                if (hit.point.y > bestFloorHeight)
                {
                    bestFloorHeight =
                        hit.point.y;

                    foundFloor = true;
                }
            }

            if (
                foundFloor &&
                point.y <
                bestFloorHeight + floorClearance
            )
            {
                point.y =
                    bestFloorHeight +
                    floorClearance;
            }

            return point;
        }
    }
}
