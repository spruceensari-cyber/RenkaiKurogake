using System;
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

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
<<<<<<< Updated upstream
        private KurokageArmor armor;
        private KurokageAgentDeathPresentation deathPresentation;

        private void Awake()
        {
            armor = GetComponent<KurokageArmor>();
            deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
=======
        private CharacterController characterController;
        private RenkaiFPSController fpsController;
        private RenkaiWeaponController weaponController;

        public event Action<RenkaiRoundPlayer, RenkaiRoundPlayer> Eliminated;
        public static event Action<RenkaiRoundPlayer, RenkaiRoundPlayer> AnyPlayerEliminated;

        public float Health01 => maxHealth <= 0f ? 0f : Mathf.Clamp01(health / maxHealth);

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
>>>>>>> Stashed changes
            RememberSpawn();
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

<<<<<<< Updated upstream
            if (armor == null) armor = GetComponent<KurokageArmor>();
            if (armor != null) armor.ResetArmor();

            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
=======
            if (characterController != null)
                characterController.enabled = false;
>>>>>>> Stashed changes

            transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            if (characterController != null)
                characterController.enabled = true;

<<<<<<< Updated upstream
            if (deathPresentation == null) deathPresentation = GetComponent<KurokageAgentDeathPresentation>();
            if (deathPresentation != null) deathPresentation.ResetPresentation();

            KurokageHitReactionPresenter hitReaction = GetComponent<KurokageHitReactionPresenter>();
            if (hitReaction != null) hitReaction.ResetPresentation();

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

            RenkaiFPSController fps = GetComponent<RenkaiFPSController>();
            if (fps != null) fps.enabled = true;

            RenkaiWeaponController weapon = GetComponent<RenkaiWeaponController>();
            if (weapon != null)
            {
                weapon.enabled = true;
                weapon.ResetAmmo();
            }

            KurokageSprintWeaponGate sprintGate = GetComponent<KurokageSprintWeaponGate>();
            if (sprintGate != null) sprintGate.ResetGate();

            KurokageViewmodelLightingPresenter viewmodelLighting = GetComponent<KurokageViewmodelLightingPresenter>();
            if (viewmodelLighting != null) viewmodelLighting.ResetPresentation();

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
=======
            SetGameplayEnabled(true);
            SetCombatVisualsVisible(true);

            if (weaponController != null)
                weaponController.ResetAmmo();

            RenkaiWorldHealthBar healthBar = GetComponentInChildren<RenkaiWorldHealthBar>(true);
            if (healthBar != null)
                healthBar.RefreshNow();
>>>>>>> Stashed changes
        }

        public void TakeDamage(float amount, RenkaiRoundPlayer attacker = null)
        {
<<<<<<< Updated upstream
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
=======
            if (!isAlive || amount <= 0f)
                return;
>>>>>>> Stashed changes

            health = Mathf.Max(0f, health - amount);
            if (health <= 0f)
                Die(info.Attacker);

            return healthDamage;
        }

        public void Die(RenkaiRoundPlayer killer = null)
        {
            if (!isAlive)
                return;

            isAlive = false;
            health = 0f;

<<<<<<< Updated upstream
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
                    Transform visual = transform.Find("AGENT_VISUAL");
                    if (visual != null)
                    {
                        foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                            renderer.enabled = false;
                    }
                }

                CharacterController deadController = GetComponent<CharacterController>();
                if (deadController != null) deadController.enabled = false;

                foreach (KurokageHitZone zone in GetComponentsInChildren<KurokageHitZone>(true))
                {
                    Collider hitCollider = zone.GetComponent<Collider>();
                    if (hitCollider != null) hitCollider.enabled = false;
                }
            }

            RenkaiRoundManager manager = Object.FindObjectOfType<RenkaiRoundManager>();
            if (manager != null) manager.CheckWinConditions();
=======
            ZodiacCoreRuntime core = FindObjectOfType<ZodiacCoreRuntime>();
            if (core != null && core.Carrier == this)
                core.Drop(transform.position + transform.forward * 0.75f);

            SetGameplayEnabled(false);
            SetCombatVisualsVisible(false);

            Eliminated?.Invoke(this, killer);
            AnyPlayerEliminated?.Invoke(this, killer);

            if (RenkaiRoundManager.Instance != null)
                RenkaiRoundManager.Instance.CheckWinConditions();
>>>>>>> Stashed changes
        }

        private void CacheComponents()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (fpsController == null)
                fpsController = GetComponent<RenkaiFPSController>();
            if (weaponController == null)
                weaponController = GetComponent<RenkaiWeaponController>();
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (fpsController != null)
                fpsController.enabled = enabled && isHumanPlayer;
            if (weaponController != null)
                weaponController.enabled = enabled && isHumanPlayer;

            RenkaiTacticalBotAI tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            if (tacticalBot != null)
                tacticalBot.enabled = enabled && !isHumanPlayer;

            RenkaiBotAI legacyBot = GetComponent<RenkaiBotAI>();
            if (legacyBot != null)
                legacyBot.enabled = false;

            ZodiacObjectiveInteractor interactor = GetComponent<ZodiacObjectiveInteractor>();
            if (interactor != null)
                interactor.enabled = enabled && isHumanPlayer;
        }

        private void SetCombatVisualsVisible(bool visible)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponentInParent<RenkaiWeaponController>() != null && isHumanPlayer)
                    continue;

                renderer.enabled = visible;
            }

            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            {
                if (collider is CharacterController)
                    continue;
                collider.enabled = visible;
            }
        }
    }
}
