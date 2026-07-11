using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageBotLocalAvoidance : MonoBehaviour
    {
        [SerializeField] private float obstacleProbeDistance = 1.35f;
        [SerializeField] private float obstacleProbeRadius = 0.28f;
        [SerializeField] private float slideAssistSpeed = 1.9f;
        [SerializeField] private float teammateSeparationRadius = 1.15f;
        [SerializeField] private float teammateSeparationSpeed = 1.25f;

        private readonly Collider[] nearby = new Collider[16];
        private CharacterController controller;
        private RenkaiRoundPlayer owner;
        private Vector3 lastPosition;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            owner = GetComponent<RenkaiRoundPlayer>();
            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (owner == null) owner = GetComponent<RenkaiRoundPlayer>();
            if (controller == null || !controller.enabled || owner == null || !owner.isAlive || owner.isHumanPlayer)
            {
                lastPosition = transform.position;
                return;
            }

            Vector3 frameVelocity = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.001f);
            Vector3 planar = Vector3.ProjectOnPlane(frameVelocity, Vector3.up);
            ApplyObstacleSlide(planar);
            ApplyTeammateSeparation();
            lastPosition = transform.position;
        }

        private void ApplyObstacleSlide(Vector3 planarVelocity)
        {
            if (planarVelocity.sqrMagnitude < 0.16f) return;

            Vector3 direction = planarVelocity.normalized;
            Vector3 origin = transform.position + Vector3.up * 0.9f;
            if (!Physics.SphereCast(
                    origin,
                    obstacleProbeRadius,
                    direction,
                    out RaycastHit hit,
                    obstacleProbeDistance,
                    ~0,
                    QueryTriggerInteraction.Ignore))
                return;

            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) return;

            Vector3 slide = Vector3.ProjectOnPlane(direction, hit.normal);
            slide.y = 0f;
            if (slide.sqrMagnitude < 0.04f)
            {
                float side = Mathf.Sign(Vector3.Dot(transform.right, hit.normal));
                if (Mathf.Approximately(side, 0f)) side = 1f;
                slide = transform.right * -side;
            }

            controller.Move(slide.normalized * slideAssistSpeed * Time.deltaTime);
        }

        private void ApplyTeammateSeparation()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position + Vector3.up * 0.9f,
                teammateSeparationRadius,
                nearby,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            Vector3 separation = Vector3.zero;
            int contributors = 0;
            for (int i = 0; i < count; i++)
            {
                Collider candidate = nearby[i];
                if (candidate == null || candidate.transform.IsChildOf(transform)) continue;

                RenkaiRoundPlayer other = candidate.GetComponentInParent<RenkaiRoundPlayer>();
                if (other == null || other == owner || !other.isAlive || other.team != owner.team) continue;

                Vector3 away = transform.position - other.transform.position;
                away.y = 0f;
                float distance = away.magnitude;
                if (distance < 0.01f || distance >= teammateSeparationRadius) continue;

                separation += away.normalized * (1f - distance / teammateSeparationRadius);
                contributors++;
            }

            if (contributors == 0 || separation.sqrMagnitude < 0.001f) return;
            controller.Move(separation.normalized * teammateSeparationSpeed * Time.deltaTime);
        }
    }
}
