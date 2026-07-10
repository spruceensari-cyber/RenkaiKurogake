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
        public AudioClip armorBreak;

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
        private RenkaiRoundPlayer localPlayer;
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
            localPlayer = GetComponent<RenkaiRoundPlayer>();
        }

        private void OnEnable()
        {
            KurokageGameEvents.RoundBanner += OnRoundBanner;
            KurokageGameEvents.RoundEnded += OnRoundEnded;
            KurokageGameEvents.ArmorBroken += OnArmorBroken;

            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (weapon != null)
            {
                weapon.ShotFired += OnShotFired;
                weapon.ReloadStarted += OnReloadStarted;
                weapon.ReloadFinished += OnReloadFinished;
                weapon.HitConfirmed += OnHitConfirmed;
            }
        }

        private void OnDisable()
        {
            KurokageGameEvents.RoundBanner -= OnRoundBanner;
            KurokageGameEvents.RoundEnded -= OnRoundEnded;
            KurokageGameEvents.ArmorBroken -= OnArmorBroken;

            if (weapon != null)
            {
                weapon.ShotFired -= OnShotFired;
                weapon.ReloadStarted -= OnReloadStarted;
                weapon.ReloadFinished -= OnReloadFinished;
                weapon.HitConfirmed -= OnHitConfirmed;
            }
        }

        private void Update()
        {
            MonitorAbilities();
        }

        private void OnShotFired()
        {
            if (weapon == null) return;
            if (weapon.slot == RenkaiWeaponSlot.Rifle) Play(rifleFire);
            else if (weapon.slot == RenkaiWeaponSlot.Pistol) Play(pistolFire);
            else Play(bladeSlash);
        }

        private void OnReloadStarted() => Play(reloadStart);
        private void OnReloadFinished() => Play(reloadFinish);

        private void OnHitConfirmed(bool isHeadshot)
        {
            Play(isHeadshot ? headshot : bodyHit);
        }

        private void OnArmorBroken(RenkaiRoundPlayer victim, KurokageDamageInfo info)
        {
            if (localPlayer == null) localPlayer = GetComponent<RenkaiRoundPlayer>();
            if (localPlayer == null) return;

            bool localArmorBroken = victim == localPlayer;
            bool localBrokeEnemyArmor = info.Attacker == localPlayer;
            if (localArmorBroken || localBrokeEnemyArmor)
                Play(armorBreak);
        }

        private void MonitorAbilities()
        {
            if (abilities == null)
            {
                abilities = GetComponent<KairiAbilityController>();
                if (abilities == null) return;
            }

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
            if (localPlayer == null) localPlayer = GetComponent<RenkaiRoundPlayer>();
            if (localPlayer == null) return;
            Play(localPlayer.team == winner ? roundWin : roundLoss);
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
