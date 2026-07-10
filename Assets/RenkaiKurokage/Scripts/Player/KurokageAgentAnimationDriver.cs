using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageAgentAnimationDriver : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private RenkaiTacticalBotAI tacticalBot;
        [SerializeField] private RenkaiRoundPlayer roundPlayer;
        [SerializeField] private Animator animator;
        [SerializeField] private float visualBobAmount = 0.035f;
        [SerializeField] private float visualBobSpeed = 8f;
        [SerializeField] private float leanAmount = 5f;
        [SerializeField] private float smooth = 10f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 lastWorldPosition;
        private float bobTime;

        private void Awake()
        {
            if (controller == null) controller = GetComponentInParent<CharacterController>();
            if (tacticalBot == null) tacticalBot = GetComponentInParent<RenkaiTacticalBotAI>();
            if (roundPlayer == null) roundPlayer = GetComponentInParent<RenkaiRoundPlayer>();
            if (animator == null) animator = GetComponentInChildren<Animator>(true);

            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            lastWorldPosition = transform.position;

            if (animator != null)
                animator.applyRootMotion = false;
        }

        private void LateUpdate()
        {
            Vector3 worldDelta = (transform.position - lastWorldPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastWorldPosition = transform.position;

            float planarSpeed;
            if (controller != null && controller.enabled)
                planarSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
            else
                planarSpeed = new Vector3(worldDelta.x, 0f, worldDelta.z).magnitude;

            bool alive = roundPlayer == null || roundPlayer.isAlive;
            bool moving = alive && planarSpeed > 0.15f;
            bool engaging = tacticalBot != null && tacticalBot.state == RenkaiBotState.Engage;

            if (animator != null)
            {
                animator.speed = moving ? Mathf.Clamp(planarSpeed / 3.8f, 0.8f, 1.45f) : 1f;
                TrySetBool("Moving", moving);
                TrySetBool("Engaging", engaging);
                TrySetFloat("Speed", planarSpeed);
            }

            Vector3 targetPos = baseLocalPosition;
            Quaternion targetRot = baseLocalRotation;

            if (moving)
            {
                bobTime += Time.deltaTime * visualBobSpeed * Mathf.Clamp(planarSpeed / 3.5f, 0.5f, 1.5f);
                targetPos += new Vector3(0f, Mathf.Abs(Mathf.Sin(bobTime)) * visualBobAmount, 0f);

                float localSide = Vector3.Dot(worldDelta.normalized, transform.parent != null ? transform.parent.right : Vector3.right);
                targetRot *= Quaternion.Euler(0f, 0f, -localSide * leanAmount);
            }
            else
            {
                bobTime = 0f;
            }

            if (engaging)
                targetRot *= Quaternion.Euler(-1.5f, 0f, 0f);

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, smooth * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, smooth * Time.deltaTime);
        }

        private void TrySetBool(string parameter, bool value)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == parameter && p.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(parameter, value);
                    return;
                }
            }
        }

        private void TrySetFloat(string parameter, float value)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == parameter && p.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(parameter, value);
                    return;
                }
            }
        }
    }
}
