using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageOriginalAgentPose : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private bool hideForLocalFirstPerson = true;

        private RenkaiRoundPlayer owner;
        private CharacterController controller;
        private Vector3 lastWorldPosition;
        private Quaternion headBaseRotation;
        private Quaternion torsoBaseRotation;
        private Quaternion leftArmBaseRotation;
        private Quaternion rightArmBaseRotation;
        private Quaternion leftLegBaseRotation;
        private Quaternion rightLegBaseRotation;
        private Renderer[] renderers;
        private float gaitTime;
        private bool localVisibilityResolved;

        public void Configure(
            Transform targetHead,
            Transform targetTorso,
            Transform targetLeftArm,
            Transform targetRightArm,
            Transform targetLeftLeg,
            Transform targetRightLeg)
        {
            head = targetHead;
            torso = targetTorso;
            leftArm = targetLeftArm;
            rightArm = targetRightArm;
            leftLeg = targetLeftLeg;
            rightLeg = targetRightLeg;
            CacheBasePose();
        }

        private void Awake()
        {
            owner = GetComponentInParent<RenkaiRoundPlayer>();
            controller = GetComponentInParent<CharacterController>();
            renderers = GetComponentsInChildren<Renderer>(true);
            lastWorldPosition = transform.position;
            CacheBasePose();
        }

        private void LateUpdate()
        {
            ResolveLocalVisibility();

            Vector3 worldDelta = (transform.position - lastWorldPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastWorldPosition = transform.position;

            float speed = controller != null && controller.enabled
                ? new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude
                : new Vector3(worldDelta.x, 0f, worldDelta.z).magnitude;
            bool moving = (owner == null || owner.isAlive) && speed > 0.1f;

            if (moving)
                gaitTime += Time.deltaTime * Mathf.Lerp(5.5f, 9.5f, Mathf.InverseLerp(0.1f, 5.5f, speed));
            else
                gaitTime = Mathf.Lerp(gaitTime, 0f, Time.deltaTime * 6f);

            float swing = moving ? Mathf.Sin(gaitTime) * Mathf.Lerp(10f, 28f, Mathf.InverseLerp(0.1f, 5.5f, speed)) : 0f;
            float counterSwing = moving ? Mathf.Sin(gaitTime + Mathf.PI) * Mathf.Lerp(9f, 24f, Mathf.InverseLerp(0.1f, 5.5f, speed)) : 0f;
            float breathing = Mathf.Sin(Time.time * 2.1f) * 1.2f;

            ApplyRotation(torso, torsoBaseRotation, breathing, 0f, 0f);
            ApplyRotation(head, headBaseRotation, -breathing * 0.35f, 0f, 0f);
            ApplyRotation(leftArm, leftArmBaseRotation, swing, 0f, moving ? -2f : 0f);
            ApplyRotation(rightArm, rightArmBaseRotation, counterSwing, 0f, moving ? 2f : 0f);
            ApplyRotation(leftLeg, leftLegBaseRotation, counterSwing, 0f, 0f);
            ApplyRotation(rightLeg, rightLegBaseRotation, swing, 0f, 0f);
        }

        private void ResolveLocalVisibility()
        {
            if (localVisibilityResolved || !hideForLocalFirstPerson || owner == null || !owner.isHumanPlayer)
                return;

            Camera camera = Camera.main;
            if (camera == null) return;

            localVisibilityResolved = true;
            if (!camera.transform.IsChildOf(owner.transform)) return;

            if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
                if (renderer != null) renderer.enabled = false;
        }

        private void CacheBasePose()
        {
            if (head != null) headBaseRotation = head.localRotation;
            if (torso != null) torsoBaseRotation = torso.localRotation;
            if (leftArm != null) leftArmBaseRotation = leftArm.localRotation;
            if (rightArm != null) rightArmBaseRotation = rightArm.localRotation;
            if (leftLeg != null) leftLegBaseRotation = leftLeg.localRotation;
            if (rightLeg != null) rightLegBaseRotation = rightLeg.localRotation;
        }

        private static void ApplyRotation(Transform target, Quaternion baseRotation, float pitch, float yaw, float roll)
        {
            if (target == null) return;
            target.localRotation = baseRotation * Quaternion.Euler(pitch, yaw, roll);
        }
    }
}
