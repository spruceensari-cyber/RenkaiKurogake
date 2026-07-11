using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageBotAutonomyMotor : MonoBehaviour
    {
        [Header("Jump")]
        [SerializeField] private float jumpHeight = 1.05f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float obstacleProbeDistance = 1.05f;
        [SerializeField] private float minimumJumpInterval = 2.4f;
        [SerializeField] private float maximumJumpInterval = 5.5f;

        [Header("Decision Variation")]
        [SerializeField] private float steeringAmplitude = 0.28f;
        [SerializeField] private float steeringFrequency = 0.85f;
        [SerializeField] private float engageJumpChance = 0.18f;
        [SerializeField] private float patrolJumpChance = 0.06f;

        private CharacterController controller;
        private RenkaiRoundPlayer owner;
        private RenkaiTacticalBotAI ai;
        private float verticalVelocity;
        private float nextJumpDecision;
        private float seed;

        public bool IsAirborne => controller != null && !controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            owner = GetComponent<RenkaiRoundPlayer>();
            ai = GetComponent<RenkaiTacticalBotAI>();
            seed = Random.Range(0f, 100f);
            ScheduleNextJumpDecision();
        }

        private void OnEnable()
        {
            ResetMotor();
        }

        private void Update()
        {
            if (owner != null && !owner.isAlive) return;
            if (controller == null || !controller.enabled) return;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2.5f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            if (controller.isGrounded && Time.time >= nextJumpDecision)
            {
                if (ShouldJump())
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                ScheduleNextJumpDecision();
            }

            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        public Vector3 AdjustPlanarDirection(Vector3 desiredDirection)
        {
            if (desiredDirection.sqrMagnitude < 0.001f) return desiredDirection;
            Vector3 planar = desiredDirection;
            planar.y = 0f;
            planar.Normalize();

            float roleScale = 1f;
            if (ai != null)
            {
                if (ai.role == RenkaiAgentRole.Sentinel) roleScale = 0.45f;
                else if (ai.role == RenkaiAgentRole.Duelist || ai.role == RenkaiAgentRole.Blade) roleScale = 1.25f;
            }

            float wave = Mathf.Sin(Time.time * steeringFrequency + seed) * steeringAmplitude * roleScale;
            Vector3 adjusted = planar + transform.right * wave;
            adjusted.y = 0f;
            return adjusted.normalized;
        }

        public void ResetMotor()
        {
            verticalVelocity = -2.5f;
            ScheduleNextJumpDecision();
        }

        private bool ShouldJump()
        {
            Vector3 planarVelocity = controller.velocity;
            planarVelocity.y = 0f;
            bool moving = planarVelocity.sqrMagnitude > 0.5f;
            if (!moving) return false;

            Vector3 origin = transform.position + Vector3.up * 0.45f;
            Vector3 direction = planarVelocity.normalized;
            bool obstacleAhead = Physics.SphereCast(
                origin,
                Mathf.Max(0.12f, controller.radius * 0.72f),
                direction,
                out RaycastHit hit,
                obstacleProbeDistance,
                ~0,
                QueryTriggerInteraction.Ignore
            ) && hit.transform != transform && !hit.transform.IsChildOf(transform);

            Vector3 head = transform.position + Vector3.up * (controller.height + 0.32f);
            bool ceilingBlocked = Physics.CheckSphere(head, controller.radius * 0.75f, ~0, QueryTriggerInteraction.Ignore);
            if (ceilingBlocked) return false;
            if (obstacleAhead) return true;

            float chance = ai != null && ai.state == RenkaiBotState.Engage ? engageJumpChance : patrolJumpChance;
            if (ai != null && (ai.role == RenkaiAgentRole.Duelist || ai.role == RenkaiAgentRole.Blade))
                chance *= 1.4f;
            return Random.value <= chance;
        }

        private void ScheduleNextJumpDecision()
        {
            nextJumpDecision = Time.time + Random.Range(minimumJumpInterval, maximumJumpInterval);
        }
    }
}
