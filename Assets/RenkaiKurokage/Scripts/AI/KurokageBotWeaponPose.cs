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
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalPosition;
        private float recoil;
        private float previousShotTime;

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
            CacheBasePose();
        }

        private void LateUpdate()
        {
            if (weaponRoot == null) return;
            if (ai == null) ai = GetComponent<RenkaiTacticalBotAI>();

            bool engaging = ai != null && ai.state == RenkaiBotState.Engage;
            float speed = GetComponent<CharacterController>() != null
                ? GetComponent<CharacterController>().velocity.magnitude
                : 0f;

            float sway = Mathf.Sin(Time.time * (2.1f + speed * 0.25f)) * idleSway;
            Quaternion targetRotation = baseLocalRotation * Quaternion.Euler(
                engaging ? -2.5f : 2.0f + sway,
                engaging ? 0f : sway * 0.35f,
                engaging ? 0f : sway * 0.25f
            );

            recoil = Mathf.MoveTowards(recoil, 0f, recoilReturn * Time.deltaTime);
            targetRotation *= Quaternion.Euler(-recoil, 0f, 0f);

            weaponRoot.localRotation = Quaternion.Slerp(
                weaponRoot.localRotation,
                targetRotation,
                aimResponsiveness * Time.deltaTime
            );

            Vector3 targetPosition = baseLocalPosition + new Vector3(0f, 0f, -recoil * 0.004f);
            weaponRoot.localPosition = Vector3.Lerp(
                weaponRoot.localPosition,
                targetPosition,
                aimResponsiveness * Time.deltaTime
            );
        }

        public void AddRecoil(float amount = 2.2f)
        {
            recoil = Mathf.Max(recoil, amount);
            previousShotTime = Time.time;
        }

        private void CacheBasePose()
        {
            if (weaponRoot == null) return;
            baseLocalRotation = weaponRoot.localRotation;
            baseLocalPosition = weaponRoot.localPosition;
        }
    }
}
