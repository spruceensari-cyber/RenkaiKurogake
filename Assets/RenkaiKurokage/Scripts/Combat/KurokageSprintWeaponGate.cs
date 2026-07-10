using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageSprintWeaponGate : MonoBehaviour
    {
        [SerializeField] private RenkaiFPSController fps;
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private RenkaiRoundPlayer player;
        [SerializeField] private float rifleReadyDelay = 0.145f;
        [SerializeField] private float pistolReadyDelay = 0.105f;
        [SerializeField] private float bladeReadyDelay = 0.08f;

        public bool WeaponReady => weapon != null && !weapon.IsFireLocked && Time.time >= readyAt;
        public float Ready01
        {
            get
            {
                if (fps != null && fps.IsSprinting) return 0f;
                if (Time.time >= readyAt) return 1f;
                float duration = Mathf.Max(0.01f, lastReadyDelay);
                return 1f - Mathf.Clamp01((readyAt - Time.time) / duration);
            }
        }

        private float readyAt;
        private float lastReadyDelay;
        private bool wasSprinting;

        private void Awake()
        {
            if (fps == null) fps = GetComponent<RenkaiFPSController>();
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (player == null) player = GetComponent<RenkaiRoundPlayer>();
        }

        private void OnDisable()
        {
            if (weapon != null)
                weapon.SetExternalFireLock(false);
        }

        private void Update()
        {
            if (fps == null || weapon == null) return;
            if (player != null && !player.isAlive)
            {
                weapon.SetExternalFireLock(true);
                return;
            }

            bool sprinting = fps.IsSprinting;
            if (sprinting)
            {
                weapon.SetExternalFireLock(true);
                wasSprinting = true;
                return;
            }

            if (wasSprinting)
            {
                lastReadyDelay = ResolveDelay();
                readyAt = Time.time + lastReadyDelay;
                wasSprinting = false;
            }

            weapon.SetExternalFireLock(Time.time < readyAt);
        }

        public void ResetGate()
        {
            readyAt = 0f;
            lastReadyDelay = 0f;
            wasSprinting = false;
            if (weapon != null)
                weapon.SetExternalFireLock(false);
        }

        private float ResolveDelay()
        {
            if (weapon == null) return rifleReadyDelay;
            if (weapon.slot == RenkaiWeaponSlot.Pistol) return pistolReadyDelay;
            if (weapon.slot == RenkaiWeaponSlot.Sword) return bladeReadyDelay;
            return rifleReadyDelay;
        }
    }
}
