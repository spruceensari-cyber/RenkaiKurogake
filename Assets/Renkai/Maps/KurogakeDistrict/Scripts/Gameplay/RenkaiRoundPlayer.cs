using UnityEngine;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    public enum RenkaiTeam
    {
        Attackers,
        Defenders
    }

    public sealed class RenkaiRoundPlayer : MonoBehaviour
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

        public event System.Action<RenkaiRoundPlayer, RenkaiRoundPlayer> Eliminated;
        public static event System.Action<RenkaiRoundPlayer, RenkaiRoundPlayer> AnyPlayerEliminated;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private CharacterController characterController;
        private RenkaiFPSController fpsController;
        private RenkaiWeaponController weaponController;
        private KurokageArmor armor;
        private KurokageAgentDeathPresentation deathPresentation;

        private void Awake()
        {
            CacheComponents();
            RememberSpawn();
        }

        public void Configure(string identity, RenkaiTeam assignedTeam, bool human)
        {
            agentName = identity;
            team = assignedTeam;
            isHumanPlayer = human;
            CacheComponents();
        }

        public void RememberSpawn()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void ResetForRound()
        {
            CacheComponents();

            isAlive = true;
            health = maxHealth;
            gameObject.SetActive(true);

            if (armor != null) armor.ResetArmor();

            if (characterController != null) characterController.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            if (characterController != null) characterController.enabled = true;

            if (deathPresentation != null) deathPresentation.ResetPresentation();

            KurokageHitReactionPresenter hitReaction = GetComponent<KurokageHitReactionPresenter>();
            if (hitReaction != null) hitReaction.ResetPresentation();

            KurokageProceduralAgentRig proceduralRig = GetComponentInChildren<KurokageProceduralAgentRig>(true);
            if (proceduralRig != null) proceduralRig.ResetRigPose();

            Transform agentVisual = transform.Find("AGENT_VISUAL");
            if (agentVisual != null)
            {
                foreach (Renderer renderer in agentVisual.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = true;
            }

            foreach (KurokageHitZone zone in GetComponentsInChildren<KurokageHitZone>(true))
            {
                Collider hitCollider = zone.GetComponent<Collider>();
                if (hitCollider != null) hitCollider.enabled = true;
            }

            if (fpsController != null) fpsController.enabled = isHumanPlayer;

            if (weaponController != null)
            {
                weaponController.enabled = isHumanPlayer;
                weaponController.ResetAmmo();
            }

            KurokageSprintWeaponGate sprintGate = GetComponent<KurokageSprintWeaponGate>();
            if (sprintGate != null) sprintGate.ResetGate();

            KurokageViewmodelLightingPresenter viewmodelLighting = GetComponent<KurokageViewmodelLightingPresenter>();
            if (viewmodelLighting != null) viewmodelLighting.ResetPresentation();

            KairiAbilityController kairi = GetComponent<KairiAbilityController>();
            if (kairi != null) kairi.ResetAbilityState(true);

            KurokageEclipseProtocolPresenter eclipse = GetComponent<KurokageEclipseProtocolPresenter>();
            if (eclipse != null) eclipse.ResetPresentation();

            RenkaiTacticalBotAI tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            if (tacticalBot != null) tacticalBot.enabled = !isHumanPlayer;

            RenkaiBotAI legacyBot = GetComponent<RenkaiBotAI>();
            if (legacyBot != null) legacyBot.enabled = tacticalBot == null && !isHumanPlayer;

            RenkaiWorldHealthBar healthBar = GetComponentInChildren<RenkaiWorldHealthBar>(true);
            if (healthBar != null) healthBar.RefreshNow();
        }

        public void TakeDamage(float amount, RenkaiRoundPlayer attacker = null)
        {
            if (amount <= 0f) return;

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
            if (!isAlive || info.Amount <= 0f) return 0f;
            if (armor == null) armor = GetComponent<KurokageArmor>();

            float armorBefore = armor != null ? armor.CurrentArmor : 0f;
            float healthDamage = armor != null ? armor.AbsorbDamage(info.Amount) : info.Amount;
            float armorAfter = armor != null ? armor.CurrentArmor : 0f;

            health = Mathf.Max(0f, health - healthDamage);
            Debug.Log(agentName + " took " + info.Amount + " " + info.DamageType +
                      " damage from " + info.SourceId + ". Health damage: " + healthDamage + ". HP: " + health);

            KurokageGameEvents.RaiseDamageApplied(this, info, healthDamage);
            if (armorBefore > 0f && armorAfter <= 0f)
                KurokageGameEvents.RaiseArmorBroken(this, info);

            RenkaiHUDController hud = UnityEngine.Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null && isHumanPlayer)
                hud.SetPlayerHP(Mathf.CeilToInt(health), Mathf.CeilToInt(maxHealth));

            if (health <= 0f) Die(info.Attacker);
            return healthDamage;
        }

        public void Die(RenkaiRoundPlayer killer = null)
        {
            if (!isAlive) return;

            isAlive = false;
            health = 0f;

            ZodiacCoreRuntime core = UnityEngine.Object.FindObjectOfType<ZodiacCoreRuntime>();
            if (core != null && core.Carrier == transform)
                core.Drop(transform.position + transform.forward * 0.75f);

            string killerName = killer != null ? killer.agentName : "Unknown";
            Debug.Log(agentName + " eliminated by " + killerName + ". No respawn until round end.");
            KurokageGameEvents.RaiseKillFeed(killerName, agentName);

            RenkaiHUDController hud = UnityEngine.Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null)
            {
                hud.AddKillFeed(killerName + " eliminated " + agentName);
                if (isHumanPlayer) hud.SetStatus("YOU ARE DOWN - WAIT FOR ROUND END");
            }

            if (deathPresentation == null) deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
            Vector3 hitDirection = killer != null ? transform.position - killer.transform.position : -transform.forward;
            if (deathPresentation != null) deathPresentation.PlayDeath(hitDirection);

            if (fpsController != null) fpsController.enabled = false;
            if (weaponController != null) weaponController.enabled = false;

            RenkaiBotAI legacyBot = GetComponent<RenkaiBotAI>();
            if (legacyBot != null) legacyBot.enabled = false;

            RenkaiTacticalBotAI tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            if (tacticalBot != null) tacticalBot.enabled = false;

            if (!isHumanPlayer)
            {
                if (deathPresentation == null)
                {
                    Transform visual = transform.Find("AGENT_VISUAL");
                    if (visual != null)
                    {
                        foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                            renderer.enabled = false;
                    }
                }

                if (characterController != null) characterController.enabled = false;

                foreach (KurokageHitZone zone in GetComponentsInChildren<KurokageHitZone>(true))
                {
                    Collider hitCollider = zone.GetComponent<Collider>();
                    if (hitCollider != null) hitCollider.enabled = false;
                }
            }

            Eliminated?.Invoke(this, killer);
            AnyPlayerEliminated?.Invoke(this, killer);

            RenkaiRoundManager manager = UnityEngine.Object.FindObjectOfType<RenkaiRoundManager>();
            if (manager != null) manager.CheckWinConditions();
        }

        public float Health01()
        {
            return maxHealth <= 0f ? 0f : Mathf.Clamp01(health / maxHealth);
        }

        private void CacheComponents()
        {
            characterController = GetComponent<CharacterController>();
            fpsController = GetComponent<RenkaiFPSController>();
            weaponController = GetComponent<RenkaiWeaponController>();
            armor = GetComponent<KurokageArmor>();
            deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
        }
    }
}
