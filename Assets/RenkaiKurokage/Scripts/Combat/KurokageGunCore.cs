using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageGunCore : MonoBehaviour
    {
        [SerializeField] private KurokageWeaponDefinition definition;
        [SerializeField] private KurokageAmmoState ammo;
        [SerializeField] private KurokageRecoil recoil;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shotClip;

        private float nextShotTime;

        public bool CanShoot => definition != null && ammo != null && ammo.Magazine > 0 && Time.time >= nextShotTime;

        public bool TryShootFeedback()
        {
            if (!CanShoot) return false;
            if (!ammo.TryConsumeRound()) return false;

            nextShotTime = Time.time + 60f / Mathf.Max(1f, definition.roundsPerMinute);

            if (muzzleFlash != null) muzzleFlash.Play();
            if (audioSource != null && shotClip != null) audioSource.PlayOneShot(shotClip);
            if (recoil != null) recoil.Kick(definition.verticalKick, definition.horizontalKick);

            return true;
        }
    }
}
