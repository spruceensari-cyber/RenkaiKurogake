using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageBotWeaponPose : MonoBehaviour
    {
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float aimResponsiveness = 14f;
        [SerializeField] private float idleSway = 0.7f;
        [SerializeField] private float recoilReturn = 18f;

        private RenkaiTacticalBotAI ai;
        private KurokageBotWeaponState weaponState;
        private CharacterController controller;
        private Transform magazine;
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalPosition;
        private Quaternion magazineBaseRotation;
        private Vector3 magazineBasePosition;
        private float recoil;

        public Transform Muzzle => muzzle;

        public void Configure(Transform weapon, Transform muzzleSocket)
        {
            weaponRoot = weapon;
            muzzle = muzzleSocket;
            CacheBasePose();
        }

        private void Awake()
        {
            ai = GetComponent<RenkaiTacticalBotAI>();
            weaponState = GetComponent<KurokageBotWeaponState>();
            controller = GetComponent<CharacterController>();
            CacheBasePose();
        }

        private void LateUpdate()
        {
            if (weaponRoot == null) return;
            if (ai == null) ai = GetComponent<RenkaiTacticalBotAI>();
            if (weaponState == null) weaponState = GetComponent<KurokageBotWeaponState>();
            if (controller == null) controller = GetComponent<CharacterController>();

            bool engaging = ai != null && ai.state == RenkaiBotState.Engage;
            bool reloading = weaponState != null && weaponState.IsReloading;
            float reload01 = reloading ? weaponState.Reload01 : 0f;
            float speed = controller != null ? controller.velocity.magnitude : 0f;

            float sway = Mathf.Sin(Time.time * (2.1f + speed * 0.25f)) * idleSway;
            Quaternion targetRotation;
            Vector3 targetPosition;

            if (reloading)
            {
                float lift = Mathf.Sin(reload01 * Mathf.PI);
                float roll = Mathf.Sin(reload01 * Mathf.PI) * 58f;
                targetRotation = baseLocalRotation * Quaternion.Euler(18f + lift * 14f, -8f, roll);
                targetPosition = baseLocalPosition + new Vector3(0.06f, -0.18f * lift, -0.08f);
                AnimateMagazine(reload01);
            }
            else
            {
                targetRotation = baseLocalRotation * Quaternion.Euler(
                    engaging ? -2.5f : 2.0f + sway,
                    engaging ? 0f : sway * 0.35f,
                    engaging ? 0f : sway * 0.25f
                );
                recoil = Mathf.MoveTowards(recoil, 0f, recoilReturn * Time.deltaTime);
                targetRotation *= Quaternion.Euler(-recoil, 0f, 0f);
                targetPosition = baseLocalPosition + new Vector3(0f, 0f, -recoil * 0.004f);
                RestoreMagazine();
            }

            weaponRoot.localRotation = Quaternion.Slerp(
                weaponRoot.localRotation,
                targetRotation,
                aimResponsiveness * Time.deltaTime
            );
            weaponRoot.localPosition = Vector3.Lerp(
                weaponRoot.localPosition,
                targetPosition,
                aimResponsiveness * Time.deltaTime
            );
        }

        public void AddRecoil(float amount = 2.2f)
        {
            recoil = Mathf.Max(recoil, amount);
        }

        private void AnimateMagazine(float normalized)
        {
            if (magazine == null) return;
            float outPhase = Mathf.Clamp01(normalized / 0.42f);
            float inPhase = Mathf.Clamp01((normalized - 0.55f) / 0.35f);
            float detach = normalized < 0.55f ? outPhase : 1f - inPhase;
            magazine.localPosition = magazineBasePosition + new Vector3(0.08f * detach, -0.24f * detach, -0.04f * detach);
            magazine.localRotation = magazineBaseRotation * Quaternion.Euler(0f, 0f, 18f * detach);
        }

        private void RestoreMagazine()
        {
            if (magazine == null) return;
            magazine.localPosition = Vector3.Lerp(magazine.localPosition, magazineBasePosition, 18f * Time.deltaTime);
            magazine.localRotation = Quaternion.Slerp(magazine.localRotation, magazineBaseRotation, 18f * Time.deltaTime);
        }

        private void CacheBasePose()
        {
            if (weaponRoot == null) return;
            baseLocalRotation = weaponRoot.localRotation;
            baseLocalPosition = weaponRoot.localPosition;
            magazine = FindDeep(weaponRoot, "MAGAZINE");
            if (magazine != null)
            {
                magazineBaseRotation = magazine.localRotation;
                magazineBasePosition = magazine.localPosition;
            }
        }

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }
    }
}
