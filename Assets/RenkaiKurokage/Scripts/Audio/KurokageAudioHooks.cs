using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageAudioHooks : MonoBehaviour
    {
        [Header("Weapon")]
        public AudioClip rifleFire;
        public AudioClip pistolFire;
        public AudioClip bladeSlash;
        public AudioClip emptyClick;
        public AudioClip reloadStart;
        public AudioClip reloadFinish;

        [Header("Combat")]
        public AudioClip bodyHit;
        public AudioClip headshot;

        [Header("Abilities")]
        public AudioClip dash;
        public AudioClip leap;
        public AudioClip decoy;
        public AudioClip ultimate;

        [Header("Objective / Match")]
        public AudioClip zodiacPickup;
        public AudioClip linkStart;
        public AudioClip severStart;
        public AudioClip synchronizationComplete;
        public AudioClip roundStart;
        public AudioClip roundWin;
        public AudioClip roundLoss;

        private AudioSource source;
        private RenkaiWeaponController weapon;
        private KairiAbilityController abilities;
        private int lastRifleAmmo;
        private int lastPistolAmmo;
        private bool wasReloading;
        private float previousQ;
        private float previousE;
        private float previousC;
        private float previousX;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null)
                source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.spatialBlend = 0f;

            weapon = GetComponent<RenkaiWeaponController>();
            abilities = GetComponent<KairiAbilityController>();

            if (weapon != null)
            {
                lastRifleAmmo = weapon.rifleAmmo;
                lastPistolAmmo = weapon.pistolAmmo;
            }
        }

        private void OnEnable()
        {
            KurokageGameEvents.RoundBanner += OnRoundBanner;
            KurokageGameEvents.RoundEnded += OnRoundEnded;
        }

        private void OnDisable()
        {
            KurokageGameEvents.RoundBanner -= OnRoundBanner;
            KurokageGameEvents.RoundEnded -= OnRoundEnded;
        }

        private void Update()
        {
            MonitorWeapon();
            MonitorAbilities();
        }

        private void MonitorWeapon()
        {
            if (weapon == null) return;

            if (weapon.rifleAmmo < lastRifleAmmo)
                Play(rifleFire);
            if (weapon.pistolAmmo < lastPistolAmmo)
                Play(pistolFire);

            lastRifleAmmo = weapon.rifleAmmo;
            lastPistolAmmo = weapon.pistolAmmo;

            bool reloadHeld = Input.GetKey(KeyCode.R) && weapon.slot != RenkaiWeaponSlot.Sword;
            if (reloadHeld && !wasReloading)
                Play(reloadStart);
            if (!reloadHeld && wasReloading)
                Play(reloadFinish);
            wasReloading = reloadHeld;
        }

        private void MonitorAbilities()
        {
            if (abilities == null) return;

            float q = abilities.QCooldown01;
            float e = abilities.ECooldown01;
            float c = abilities.CCooldown01;
            float x = abilities.XCooldown01;

            if (previousQ <= 0.001f && q > 0.001f) Play(dash);
            if (previousE <= 0.001f && e > 0.001f) Play(decoy);
            if (previousC <= 0.001f && c > 0.001f) Play(leap);
            if (previousX <= 0.001f && x > 0.001f) Play(ultimate);

            previousQ = q;
            previousE = e;
            previousC = c;
            previousX = x;
        }

        private void OnRoundBanner(string message)
        {
            if (message.Contains("ENGAGE")) Play(roundStart);
        }

        private void OnRoundEnded(RenkaiTeam winner, string reason)
        {
            RenkaiRoundPlayer player = GetComponent<RenkaiRoundPlayer>();
            if (player == null) return;
            Play(player.team == winner ? roundWin : roundLoss);
        }

        public void PlayBodyHit() => Play(bodyHit);
        public void PlayHeadshot() => Play(headshot);
        public void PlayBladeSlash() => Play(bladeSlash);
        public void PlayZodiacPickup() => Play(zodiacPickup);
        public void PlayLinkStart() => Play(linkStart);
        public void PlaySeverStart() => Play(severStart);
        public void PlaySynchronizationComplete() => Play(synchronizationComplete);

        private void Play(AudioClip clip)
        {
            if (clip == null || source == null) return;
            source.PlayOneShot(clip);
        }
    }
}
