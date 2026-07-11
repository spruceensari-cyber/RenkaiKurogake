using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageProceduralAgentRig : MonoBehaviour
    {
        [SerializeField] private KurokageAgentArchetype archetype;
        [SerializeField] private float gaitFrequency = 8.4f;
        [SerializeField] private float walkSwing = 22f;
        [SerializeField] private float runSwing = 34f;
        [SerializeField] private float bodyBob = 0.032f;
        [SerializeField] private float breathingAmount = 0.008f;
        [SerializeField] private float turnLean = 4.5f;

        public KurokageAgentArchetype Archetype => archetype;

        private RenkaiRoundPlayer owner;
        private CharacterController controller;
        private RenkaiFPSController fps;
        private RenkaiTacticalBotAI tacticalBot;
        private KurokageBotWeaponState botWeaponState;

        private Transform modelRoot;
        private Transform pelvis;
        private Transform chest;
        private Transform head;
        private Transform leftUpperArm;
        private Transform rightUpperArm;
        private Transform leftLowerArm;
        private Transform rightLowerArm;
        private Transform leftUpperLeg;
        private Transform rightUpperLeg;
        private Transform leftLowerLeg;
        private Transform rightLowerLeg;
        private Transform leftFoot;
        private Transform rightFoot;
        private Transform ponytailRoot;
        private Transform coatLeft;
        private Transform coatRight;
        private Transform dronePivot;

        private Vector3 modelBasePosition;
        private Quaternion pelvisBaseRotation;
        private Quaternion chestBaseRotation;
        private Quaternion headBaseRotation;
        private Quaternion leftUpperArmBaseRotation;
        private Quaternion rightUpperArmBaseRotation;
        private Quaternion leftLowerArmBaseRotation;
        private Quaternion rightLowerArmBaseRotation;
        private Quaternion leftUpperLegBaseRotation;
        private Quaternion rightUpperLegBaseRotation;
        private Quaternion leftLowerLegBaseRotation;
        private Quaternion rightLowerLegBaseRotation;
        private Quaternion leftFootBaseRotation;
        private Quaternion rightFootBaseRotation;
        private Quaternion ponytailBaseRotation;
        private Quaternion coatLeftBaseRotation;
        private Quaternion coatRightBaseRotation;

        private Vector3 previousForward;
        private float gaitPhase;
        private bool cached;

        public void Configure(KurokageAgentArchetype targetArchetype)
        {
            archetype = targetArchetype;
            CacheRig();
        }

        private void Awake()
        {
            CacheOwners();
            CacheRig();
            previousForward = transform.forward;
        }

        private void OnEnable()
        {
            CacheOwners();
            if (!cached) CacheRig();
        }

        private void LateUpdate()
        {
            if (!cached) CacheRig();
            if (!cached || owner == null || !owner.isAlive) return;

            Vector3 velocity = ResolveVelocity();
            float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            float normalizedSpeed = Mathf.Clamp01(speed / 7.4f);
            bool grounded = controller == null || controller.isGrounded;
            bool crouching = fps != null && fps.IsCrouching;
            bool engaging = tacticalBot != null && tacticalBot.state == RenkaiBotState.Engage;
            bool reloading = botWeaponState != null && botWeaponState.IsReloading;

            float frequency = Mathf.Lerp(gaitFrequency * 0.72f, gaitFrequency * 1.18f, normalizedSpeed);
            gaitPhase += Time.deltaTime * frequency * Mathf.Lerp(0.45f, 1f, normalizedSpeed);
            float stride = Mathf.Sin(gaitPhase);
            float opposite = Mathf.Sin(gaitPhase + Mathf.PI);
            float swing = Mathf.Lerp(walkSwing, runSwing, normalizedSpeed) * normalizedSpeed;

            float breath = Mathf.Sin(Time.time * 1.75f) * breathingAmount;
            float bob = grounded ? Mathf.Abs(Mathf.Sin(gaitPhase * 2f)) * bodyBob * normalizedSpeed : 0f;
            if (modelRoot != null)
                modelRoot.localPosition = modelBasePosition + Vector3.up * (breath + bob - (crouching ? 0.22f : 0f));

            float yawVelocity = SignedTurnVelocity();
            float combatLean = engaging ? -4.5f : 0f;
            float reloadLean = reloading ? 5f : 0f;
            SetLocalRotation(pelvis, pelvisBaseRotation, new Vector3(0f, yawVelocity * 0.22f, -yawVelocity * 0.08f));
            SetLocalRotation(chest, chestBaseRotation, new Vector3(-normalizedSpeed * 2.2f + combatLean + reloadLean, -yawVelocity * 0.36f, yawVelocity * 0.16f));
            SetLocalRotation(head, headBaseRotation, new Vector3(engaging ? 1.5f : 0f, yawVelocity * 0.18f, -yawVelocity * 0.08f));

            if (reloading)
                ApplyReloadPose();
            else if (engaging)
                ApplyWeaponReadyPose(normalizedSpeed);
            else
                ApplyLocomotionArms(stride, opposite, swing, normalizedSpeed);

            if (grounded)
            {
                SetLocalRotation(leftUpperLeg, leftUpperLegBaseRotation, new Vector3(stride * swing, 0f, 0f));
                SetLocalRotation(rightUpperLeg, rightUpperLegBaseRotation, new Vector3(opposite * swing, 0f, 0f));
                SetLocalRotation(leftLowerLeg, leftLowerLegBaseRotation, new Vector3(Mathf.Max(0f, opposite) * 30f * normalizedSpeed, 0f, 0f));
                SetLocalRotation(rightLowerLeg, rightLowerLegBaseRotation, new Vector3(Mathf.Max(0f, stride) * 30f * normalizedSpeed, 0f, 0f));
                SetLocalRotation(leftFoot, leftFootBaseRotation, new Vector3(-Mathf.Max(0f, stride) * 13f * normalizedSpeed, 0f, 0f));
                SetLocalRotation(rightFoot, rightFootBaseRotation, new Vector3(-Mathf.Max(0f, opposite) * 13f * normalizedSpeed, 0f, 0f));
            }
            else
            {
                SetLocalRotation(leftUpperLeg, leftUpperLegBaseRotation, new Vector3(-12f, 0f, 0f));
                SetLocalRotation(rightUpperLeg, rightUpperLegBaseRotation, new Vector3(18f, 0f, 0f));
                SetLocalRotation(leftLowerLeg, leftLowerLegBaseRotation, new Vector3(24f, 0f, 0f));
                SetLocalRotation(rightLowerLeg, rightLowerLegBaseRotation, new Vector3(38f, 0f, 0f));
            }

            AnimateSecondaryMotion(normalizedSpeed, yawVelocity);
        }

        public void ResetRigPose()
        {
            if (!cached) CacheRig();
            if (!cached) return;

            if (modelRoot != null) modelRoot.localPosition = modelBasePosition;
            Restore(pelvis, pelvisBaseRotation);
            Restore(chest, chestBaseRotation);
            Restore(head, headBaseRotation);
            Restore(leftUpperArm, leftUpperArmBaseRotation);
            Restore(rightUpperArm, rightUpperArmBaseRotation);
            Restore(leftLowerArm, leftLowerArmBaseRotation);
            Restore(rightLowerArm, rightLowerArmBaseRotation);
            Restore(leftUpperLeg, leftUpperLegBaseRotation);
            Restore(rightUpperLeg, rightUpperLegBaseRotation);
            Restore(leftLowerLeg, leftLowerLegBaseRotation);
            Restore(rightLowerLeg, rightLowerLegBaseRotation);
            Restore(leftFoot, leftFootBaseRotation);
            Restore(rightFoot, rightFootBaseRotation);
            Restore(ponytailRoot, ponytailBaseRotation);
            Restore(coatLeft, coatLeftBaseRotation);
            Restore(coatRight, coatRightBaseRotation);
        }

        private void ApplyLocomotionArms(float stride, float opposite, float swing, float normalizedSpeed)
        {
            float armSwingScale = archetype == KurokageAgentArchetype.Reiha ? 0.78f : 1f;
            SetLocalRotation(leftUpperArm, leftUpperArmBaseRotation, new Vector3(opposite * swing * armSwingScale, 0f, 0f));
            SetLocalRotation(rightUpperArm, rightUpperArmBaseRotation, new Vector3(stride * swing * armSwingScale, 0f, 0f));
            SetLocalRotation(leftLowerArm, leftLowerArmBaseRotation, new Vector3(Mathf.Max(0f, stride) * 11f * normalizedSpeed, 0f, 0f));
            SetLocalRotation(rightLowerArm, rightLowerArmBaseRotation, new Vector3(Mathf.Max(0f, opposite) * 11f * normalizedSpeed, 0f, 0f));
        }

        private void ApplyWeaponReadyPose(float normalizedSpeed)
        {
            float breathing = Mathf.Sin(Time.time * 3.1f) * 1.1f;
            float movementDip = normalizedSpeed * 3f;
            SetLocalRotation(leftUpperArm, leftUpperArmBaseRotation, new Vector3(-55f + movementDip, -12f, -34f + breathing));
            SetLocalRotation(rightUpperArm, rightUpperArmBaseRotation, new Vector3(-48f + movementDip, 12f, 29f - breathing));
            SetLocalRotation(leftLowerArm, leftLowerArmBaseRotation, new Vector3(-72f, 5f, -8f));
            SetLocalRotation(rightLowerArm, rightLowerArmBaseRotation, new Vector3(-58f, -4f, 7f));
        }

        private void ApplyReloadPose()
        {
            float normalized = botWeaponState != null ? botWeaponState.Reload01 : 0f;
            float lift = Mathf.Sin(normalized * Mathf.PI);
            SetLocalRotation(leftUpperArm, leftUpperArmBaseRotation, new Vector3(-18f - lift * 34f, -24f, -45f));
            SetLocalRotation(rightUpperArm, rightUpperArmBaseRotation, new Vector3(-35f + lift * 12f, 18f, 31f));
            SetLocalRotation(leftLowerArm, leftLowerArmBaseRotation, new Vector3(-88f + lift * 18f, 8f, -12f));
            SetLocalRotation(rightLowerArm, rightLowerArmBaseRotation, new Vector3(-42f, -8f, 12f));
        }

        private void AnimateSecondaryMotion(float normalizedSpeed, float yawVelocity)
        {
            float tailWave = Mathf.Sin(Time.time * 4.1f + gaitPhase * 0.35f) * (4f + normalizedSpeed * 9f);
            if (ponytailRoot != null)
                ponytailRoot.localRotation = ponytailBaseRotation * Quaternion.Euler(6f + normalizedSpeed * 14f, -yawVelocity * 0.55f, tailWave);

            float coatLift = normalizedSpeed * 13f;
            float coatWave = Mathf.Sin(Time.time * 3.2f + gaitPhase * 0.2f) * (2f + normalizedSpeed * 5f);
            if (coatLeft != null)
                coatLeft.localRotation = coatLeftBaseRotation * Quaternion.Euler(coatLift + coatWave, 0f, 2f + yawVelocity * 0.12f);
            if (coatRight != null)
                coatRight.localRotation = coatRightBaseRotation * Quaternion.Euler(coatLift - coatWave, 0f, -2f + yawVelocity * 0.12f);

            if (dronePivot != null)
            {
                dronePivot.localRotation *= Quaternion.Euler(0f, 28f * Time.deltaTime, 0f);
                float y = 1.34f + Mathf.Sin(Time.time * 1.8f) * 0.045f;
                Vector3 p = dronePivot.localPosition;
                p.y = y;
                dronePivot.localPosition = p;
            }
        }

        private Vector3 ResolveVelocity()
        {
            if (controller != null && controller.enabled)
                return controller.velocity;
            return Vector3.zero;
        }

        private float SignedTurnVelocity()
        {
            Vector3 current = transform.forward;
            float signed = Vector3.SignedAngle(previousForward, current, Vector3.up) / Mathf.Max(Time.deltaTime, 0.001f);
            previousForward = current;
            return Mathf.Clamp(signed * 0.02f, -turnLean, turnLean);
        }

        private void CacheOwners()
        {
            owner = GetComponentInParent<RenkaiRoundPlayer>();
            controller = GetComponentInParent<CharacterController>();
            fps = GetComponentInParent<RenkaiFPSController>();
            tacticalBot = GetComponentInParent<RenkaiTacticalBotAI>();
            botWeaponState = GetComponentInParent<KurokageBotWeaponState>();
        }

        private void CacheRig()
        {
            CacheOwners();
            modelRoot = FindDeep(transform, "PROCEDURAL_AGENT_ROOT");
            pelvis = FindDeep(transform, "pelvis");
            chest = FindDeep(transform, "chest");
            head = FindDeep(transform, "head");
            leftUpperArm = FindDeep(transform, "leftupperarm");
            rightUpperArm = FindDeep(transform, "rightupperarm");
            leftLowerArm = FindDeep(transform, "leftlowerarm");
            rightLowerArm = FindDeep(transform, "rightlowerarm");
            leftUpperLeg = FindDeep(transform, "leftupleg");
            rightUpperLeg = FindDeep(transform, "rightupleg");
            leftLowerLeg = FindDeep(transform, "leftleg");
            rightLowerLeg = FindDeep(transform, "rightleg");
            leftFoot = FindDeep(transform, "leftfoot");
            rightFoot = FindDeep(transform, "rightfoot");
            ponytailRoot = FindDeep(transform, "PONYTAIL_ROOT");
            coatLeft = FindDeep(transform, "COAT_PANEL_L");
            coatRight = FindDeep(transform, "COAT_PANEL_R");
            dronePivot = FindDeep(transform, "DRONE_PIVOT");

            if (modelRoot == null || pelvis == null || chest == null || head == null)
            {
                cached = false;
                return;
            }

            modelBasePosition = modelRoot.localPosition;
            pelvisBaseRotation = pelvis.localRotation;
            chestBaseRotation = chest.localRotation;
            headBaseRotation = head.localRotation;
            leftUpperArmBaseRotation = GetRotation(leftUpperArm);
            rightUpperArmBaseRotation = GetRotation(rightUpperArm);
            leftLowerArmBaseRotation = GetRotation(leftLowerArm);
            rightLowerArmBaseRotation = GetRotation(rightLowerArm);
            leftUpperLegBaseRotation = GetRotation(leftUpperLeg);
            rightUpperLegBaseRotation = GetRotation(rightUpperLeg);
            leftLowerLegBaseRotation = GetRotation(leftLowerLeg);
            rightLowerLegBaseRotation = GetRotation(rightLowerLeg);
            leftFootBaseRotation = GetRotation(leftFoot);
            rightFootBaseRotation = GetRotation(rightFoot);
            ponytailBaseRotation = GetRotation(ponytailRoot);
            coatLeftBaseRotation = GetRotation(coatLeft);
            coatRightBaseRotation = GetRotation(coatRight);
            cached = true;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        private static Quaternion GetRotation(Transform t)
        {
            return t != null ? t.localRotation : Quaternion.identity;
        }

        private static void SetLocalRotation(Transform t, Quaternion baseRotation, Vector3 euler)
        {
            if (t != null) t.localRotation = baseRotation * Quaternion.Euler(euler);
        }

        private static void Restore(Transform t, Quaternion rotation)
        {
            if (t != null) t.localRotation = rotation;
        }
    }
}
