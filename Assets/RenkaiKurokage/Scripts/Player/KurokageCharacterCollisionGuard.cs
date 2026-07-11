using UnityEngine;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageCharacterCollisionGuard : MonoBehaviour
    {
        [SerializeField] private float wallNormalThreshold = 0.68f;
        [SerializeField] private float sweepSkin = 0.035f;
        [SerializeField] private float minimumSweepDistance = 0.035f;
        [SerializeField] private float maximumRecoveryDistance = 0.55f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private readonly RaycastHit[] sweepHits = new RaycastHit[24];
        private readonly Collider[] overlapHits = new Collider[24];

        private CharacterController controller;
        private Vector3 previousPosition;
        private bool initialized;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            CacheCurrentPosition();
        }

        private void OnEnable()
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            CacheCurrentPosition();
        }

        private void LateUpdate()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
            {
                initialized = false;
                return;
            }

            if (!initialized)
            {
                CacheCurrentPosition();
                return;
            }

            PreventHorizontalTunneling();
            ResolveSolidOverlap();
            previousPosition = transform.position;
        }

        public void ResetGuard()
        {
            CacheCurrentPosition();
        }

        private void PreventHorizontalTunneling()
        {
            Vector3 current = transform.position;
            Vector3 travel = current - previousPosition;
            Vector3 horizontalTravel = Vector3.ProjectOnPlane(travel, transform.up);
            float distance = horizontalTravel.magnitude;
            if (distance < minimumSweepDistance) return;

            Vector3 direction = horizontalTravel / distance;
            GetCapsuleAt(previousPosition, out Vector3 top, out Vector3 bottom, out float radius);

            int count = Physics.CapsuleCastNonAlloc(
                bottom,
                top,
                radius,
                direction,
                sweepHits,
                distance + sweepSkin,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );

            float nearestDistance = float.PositiveInfinity;
            bool blocked = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = sweepHits[i];
                Collider other = hit.collider;
                if (!IsBlockingCollider(other)) continue;

                float verticalNormal = Mathf.Abs(Vector3.Dot(hit.normal.normalized, transform.up));
                if (verticalNormal > wallNormalThreshold) continue;

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    blocked = true;
                }
            }

            if (!blocked) return;

            float allowedDistance = Mathf.Max(0f, nearestDistance - sweepSkin);
            Vector3 target = previousPosition + direction * allowedDistance;
            target += transform.up * Vector3.Dot(travel, transform.up);

            Vector3 correction = target - current;
            if (correction.sqrMagnitude > 0.000001f)
                controller.Move(correction);
        }

        private void ResolveSolidOverlap()
        {
            GetCapsuleAt(transform.position, out Vector3 top, out Vector3 bottom, out float radius);
            int count = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius,
                overlapHits,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );

            Vector3 correction = Vector3.zero;
            int correctionCount = 0;

            for (int i = 0; i < count; i++)
            {
                Collider other = overlapHits[i];
                if (!IsBlockingCollider(other)) continue;

                if (!Physics.ComputePenetration(
                        controller,
                        transform.position,
                        transform.rotation,
                        other,
                        other.transform.position,
                        other.transform.rotation,
                        out Vector3 separationDirection,
                        out float separationDistance))
                    continue;

                Vector3 horizontalSeparation = Vector3.ProjectOnPlane(separationDirection * separationDistance, transform.up);
                if (horizontalSeparation.sqrMagnitude < 0.000001f) continue;

                correction += horizontalSeparation;
                correctionCount++;
            }

            if (correctionCount == 0) return;

            correction /= correctionCount;
            correction = Vector3.ClampMagnitude(correction, maximumRecoveryDistance);
            controller.Move(correction + transform.up * 0.002f);
        }

        private bool IsBlockingCollider(Collider other)
        {
            if (other == null || !other.enabled || other.isTrigger) return false;
            if (other == controller) return false;
            if (other.transform.IsChildOf(transform) || transform.IsChildOf(other.transform)) return false;
            return true;
        }

        private void GetCapsuleAt(Vector3 rootPosition, out Vector3 top, out Vector3 bottom, out float radius)
        {
            radius = Mathf.Max(0.05f, controller.radius * 0.94f);
            float halfSegment = Mathf.Max(0f, controller.height * 0.5f - radius);
            Vector3 centerWorld = rootPosition + transform.rotation * controller.center;
            Vector3 up = transform.up;
            top = centerWorld + up * halfSegment;
            bottom = centerWorld - up * halfSegment;
        }

        private void CacheCurrentPosition()
        {
            previousPosition = transform.position;
            initialized = true;
        }
    }
}
