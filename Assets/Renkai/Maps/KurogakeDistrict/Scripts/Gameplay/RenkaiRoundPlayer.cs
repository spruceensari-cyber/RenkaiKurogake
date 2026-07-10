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

        private void Awake()
        {
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

            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            if (controller != null) controller.enabled = true;

            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                r.enabled = true;

            foreach (Collider c in GetComponentsInChildren<Collider>(true))
                if (!c.isTrigger) c.enabled = true;

            RenkaiFPSController fps = GetComponent<RenkaiFPSController>();
            if (fps != null) fps.enabled = true;

            RenkaiWeaponController weapon = GetComponent<RenkaiWeaponController>();
            if (weapon != null)
            {
                weapon.enabled = true;
                weapon.ResetAmmo();
            }

            RenkaiBotAI bot = GetComponent<RenkaiBotAI>();
            if (bot != null) bot.enabled = true;

            RenkaiTacticalBotAI tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            if (tacticalBot != null) tacticalBot.enabled = true;

            RenkaiWorldHealthBar bar = GetComponentInChildren<RenkaiWorldHealthBar>(true);
            if (bar != null) bar.RefreshNow();
        }

        public void TakeDamage(float amount, RenkaiRoundPlayer attacker = null)
        {
            if (!isAlive) return;

            health = Mathf.Max(0f, health - amount);
            Debug.Log(agentName + " took " + amount + " damage. HP: " + health);

            RenkaiHUDController hud = Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null && isHumanPlayer)
                hud.SetPlayerHP(Mathf.CeilToInt(health), Mathf.CeilToInt(maxHealth));

            if (health <= 0f)
                Die(attacker);
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

                foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;

                foreach (Collider c in GetComponentsInChildren<Collider>(true))
                    if (!c.isTrigger) c.enabled = false;
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
