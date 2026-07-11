using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageBotPerception : MonoBehaviour
    {
        [Header("Vision")]
        [SerializeField] private Transform eye;
        [SerializeField] private float viewDistance = 42f;
        [SerializeField, Range(30f, 180f)] private float fieldOfView = 105f;
        [SerializeField] private LayerMask sightMask = ~0;
        [SerializeField] private float torsoHeight = 1.10f;
        [SerializeField] private float headHeight = 1.58f;

        [Header("Memory")]
        [SerializeField] private float memoryDuration = 4.2f;

        public bool HasVisibleTarget => visibleTarget != null && visibleTarget.isAlive;
        public RenkaiRoundPlayer VisibleTarget => HasVisibleTarget ? visibleTarget : null;
        public bool HasMemory => Time.time - lastSeenTime <= memoryDuration;
        public Vector3 LastKnownPosition => lastKnownPosition;
        public float LastSeenTime => lastSeenTime;

        private RenkaiRoundPlayer owner;
        private RenkaiRoundPlayer visibleTarget;
        private Vector3 lastKnownPosition;
        private float lastSeenTime = -999f;

        private void Awake()
        {
            owner = GetComponent<RenkaiRoundPlayer>();
            EnsureEye();
        }

        public void Configure(Transform eyeTransform, float distance, float fov, float memory, LayerMask mask)
        {
            eye = eyeTransform;
            viewDistance = Mathf.Max(1f, distance);
            fieldOfView = Mathf.Clamp(fov, 30f, 180f);
            memoryDuration = Mathf.Max(0f, memory);
            sightMask = mask;
            EnsureEye();
        }

        public RenkaiRoundPlayer FindBestVisibleEnemy(RenkaiRoundPlayer[] roster, RenkaiTeam team)
        {
            EnsureEye();
            visibleTarget = null;
            float bestDistance = float.MaxValue;

            if (roster == null) return null;

            foreach (RenkaiRoundPlayer candidate in roster)
            {
                if (candidate == null || !candidate.isAlive || candidate.team == team || candidate == owner)
                    continue;

                float distance = Vector3.Distance(eye.position, candidate.transform.position + Vector3.up * torsoHeight);
                if (distance > viewDistance) continue;
                if (!InsideFov(candidate.transform.position + Vector3.up * torsoHeight)) continue;
                if (!HasDirectLineOfSight(candidate, viewDistance, out _)) continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    visibleTarget = candidate;
                }
            }

            if (visibleTarget != null)
            {
                lastKnownPosition = visibleTarget.transform.position;
                lastSeenTime = Time.time;
            }

            return visibleTarget;
        }

        public bool IsCurrentlyVisible(RenkaiRoundPlayer candidate)
        {
            if (candidate == null || !candidate.isAlive) return false;
            EnsureEye();
            Vector3 targetPoint = candidate.transform.position + Vector3.up * torsoHeight;
            float distance = Vector3.Distance(eye.position, targetPoint);
            return distance <= viewDistance && InsideFov(targetPoint) && HasDirectLineOfSight(candidate, viewDistance, out _);
        }

        public bool TryGetClearShot(RenkaiRoundPlayer candidate, float fireDistance, Vector3 aimPoint, out RaycastHit hit)
        {
            hit = default;
            if (candidate == null || !candidate.isAlive) return false;
            EnsureEye();

            Vector3 origin = eye.position;
            Vector3 toAim = aimPoint - origin;
            float distance = toAim.magnitude;
            if (distance <= 0.01f || distance > fireDistance) return false;

            if (!Physics.Raycast(origin, toAim / distance, out hit, distance + 0.25f, sightMask, QueryTriggerInteraction.Collide))
                return false;

            RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
            return hitPlayer == candidate;
        }

        public void ClearTargetKnowledge()
        {
            visibleTarget = null;
            lastKnownPosition = Vector3.zero;
            lastSeenTime = -999f;
        }

        private bool HasDirectLineOfSight(RenkaiRoundPlayer candidate, float maxDistance, out RaycastHit hit)
        {
            Vector3 origin = eye.position;
            Vector3 torso = candidate.transform.position + Vector3.up * torsoHeight;
            Vector3 head = candidate.transform.position + Vector3.up * headHeight;

            if (RayHitsCandidate(origin, torso, maxDistance, candidate, out hit)) return true;
            if (RayHitsCandidate(origin, head, maxDistance, candidate, out hit)) return true;
            return false;
        }

        private bool RayHitsCandidate(Vector3 origin, Vector3 targetPoint, float maxDistance, RenkaiRoundPlayer candidate, out RaycastHit hit)
        {
            Vector3 delta = targetPoint - origin;
            float distance = delta.magnitude;
            if (distance <= 0.01f || distance > maxDistance)
            {
                hit = default;
                return false;
            }

            if (!Physics.Raycast(origin, delta / distance, out hit, distance + 0.2f, sightMask, QueryTriggerInteraction.Collide))
                return false;

            RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
            return hitPlayer == candidate;
        }

        private bool InsideFov(Vector3 point)
        {
            Vector3 direction = point - eye.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f) return true;
            return Vector3.Angle(transform.forward, direction.normalized) <= fieldOfView * 0.5f;
        }

        private void EnsureEye()
        {
            if (eye != null) return;

            Transform existing = transform.Find("BOT_VISION_EYE");
            if (existing != null)
            {
                eye = existing;
                return;
            }

            GameObject eyeObject = new GameObject("BOT_VISION_EYE");
            eyeObject.transform.SetParent(transform, false);
            eyeObject.transform.localPosition = new Vector3(0f, 1.52f, 0.12f);
            eye = eyeObject.transform;
        }
    }
}
