using UnityEngine;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    public enum RenkaiTeam
    {
        Attackers,
        Defenders
    }

    public class RenkaiRoundPlayer : MonoBehaviour
    {
        public string agentName = "Agent";
        public RenkaiTeam team = RenkaiTeam.Attackers;
        public bool isHumanPlayer;
        public bool isAlive = true;

        [Header("Health")]
        public float maxHealth = 100f;
        public float health = 100f;

        [Header("Round")]
        public bool noRespawnUntilRoundEnd = true;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private KurokageArmor armor;
        private KurokageAgentDeathPresentation deathPresentation;

        private void Awake()
        {
            armor = GetComponent<KurokageArmor>();
            deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
            RememberSpawn();
        }

        public void RememberSpawn()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void ResetForRound()
        {
            isAlive = true;
            health = maxHealth;
            gameObject.SetActive(true);

            if (armor == null) armor = GetComponent<KurokageArmor>();
            if (armor != null) armor.ResetArmor();

            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            if (controller != null) controller.enabled = true;

            if (deathPresentation == null) deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
            if (deathPresentation != null) deathPresentation.ResetPresentation();

            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                r.enabled = true;

            foreach (KurokageHitZone zone in GetComponentsInChildren<KurokageHitZone>(true))
            {
                Collider hitCollider = zone.GetComponent<Collider>();
                if (hitCollider != null) hitCollider.enabled = true;
            }

            RenkaiFPSController fps = GetComponent<RenkaiFPSController>();
            if (fps != null) fps.enabled = true;

            RenkaiWeaponController weapon = GetComponent<RenkaiWeaponController>();
            if (weapon != null)
            {
                weapon.enabled = true;
                weapon.ResetAmmo();
            }

            KairiAbilityController kairi = GetComponent<KairiAbilityController>();
            if (kairi != null)
                kairi.ResetAbilityState(true);

            KurokageEclipseProtocolPresenter eclipse = GetComponent<KurokageEclipseProtocolPresenter>();
            if (eclipse != null) eclipse.ResetPresentation();

            RenkaiBotAI bot = GetComponent<RenkaiBotAI>();
            if (bot != null) bot.enabled = true;

            RenkaiTacticalBotAI tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            if (tacticalBot != null) tacticalBot.enabled = true;

            RenkaiWorldHealthBar bar = GetComponentInChildren<RenkaiWorldHealthBar>(true);
            if (bar != null) bar.RefreshNow();
        }

        public void TakeDamage(float amount, RenkaiRoundPlayer attacker = null)
        {
            KurokageDamageInfo info = new KurokageDamageInfo(
                amount,
                attacker,
                transform.position,
                Vector3.up,
                KurokageDamageType.Ballistic,
                KurokageHitZoneType.Torso,
                "LEGACY_DIRECT_DAMAGE"
            );
            ApplyDamage(info);
        }

        public float ApplyDamage(KurokageDamageInfo info)
        {
            if (!isAlive) return 0f;

            if (armor == null) armor = GetComponent<KurokageArmor>();

            float armorBefore = armor != null ? armor.CurrentArmor : 0f;
            float healthDamage = armor != null ? armor.AbsorbDamage(info.Amount) : info.Amount;
            float armorAfter = armor != null ? armor.CurrentArmor : 0f;

            health = Mathf.Max(0f, health - healthDamage);
            Debug.Log(agentName + " took " + info.Amount + " " + info.DamageType + " damage from " + info.SourceId + ". Health damage: " + healthDamage + ". HP: " + health);

            KurokageGameEvents.RaiseDamageApplied(this, info, healthDamage);
            if (armorBefore > 0f && armorAfter <= 0f)
                KurokageGameEvents.RaiseArmorBroken(this, info);

            RenkaiHUDController hud = Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null && isHumanPlayer)
                hud.SetPlayerHP(Mathf.CeilToInt(health), Mathf.CeilToInt(maxHealth));

            if (health <= 0f)
                Die(info.Attacker);

            return healthDamage;
        }

        public void Die(RenkaiRoundPlayer killer = null)
        {
            if (!isAlive) return;

            isAlive = false;
            health = 0f;

            string killerName = killer != null ? killer.agentName : "Unknown";
            Debug.Log(agentName + " eliminated by " + killerName + ". No respawn until round end.");
            KurokageGameEvents.RaiseKillFeed(killerName, agentName);

            RenkaiHUDController hud = Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null)
            {
                hud.AddKillFeed(killerName + " eliminated " + agentName);
                if (isHumanPlayer)
                    hud.SetStatus("YOU ARE DOWN - WAIT FOR ROUND END");
            }

            if (deathPresentation == null) deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
            Vector3 hitDirection = killer != null ? transform.position - killer.transform.position : -transform.forward;
            if (deathPresentation != null) deathPresentation.PlayDeath(hitDirection);

            if (isHumanPlayer)
            {
                RenkaiFPSController fps = GetComponent<RenkaiFPSController>();
                if (fps != null) fps.enabled = false;

                RenkaiWeaponController weapon = GetComponent<RenkaiWeaponController>();
                if (weapon != null) weapon.enabled = false;
            }
            else
            {
                RenkaiBotAI bot = GetComponent<RenkaiBotAI>();
                if (bot != null) bot.enabled = false;

                RenkaiTacticalBotAI tacticalBot = GetComponent<RenkaiTacticalBotAI>();
                if (tacticalBot != null) tacticalBot.enabled = false;

                if (deathPresentation == null)
                {
                    foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                        r.enabled = false;
                }

                CharacterController controller = GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                foreach (KurokageHitZone zone in GetComponentsInChildren<KurokageHitZone>(true))
                {
                    Collider hitCollider = zone.GetComponent<Collider>();
                    if (hitCollider != null) hitCollider.enabled = false;
                }
            }

            RenkaiRoundManager manager = Object.FindObjectOfType<RenkaiRoundManager>();
            if (manager != null) manager.CheckWinConditions();
        }

        public float Health01()
        {
            if (maxHealth <= 0f) return 0f;
            return Mathf.Clamp01(health / maxHealth);
        }
    }
}
