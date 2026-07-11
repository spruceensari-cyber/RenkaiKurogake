using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(KurokageAgentIdentity))]
    [RequireComponent(typeof(KurokageAgentAbilityController))]
    public sealed class KurokageBotAgentAbilityBrain : MonoBehaviour
    {
        [SerializeField] private float minimumDecisionInterval = 2.8f;
        [SerializeField] private float maximumDecisionInterval = 6.2f;
        [SerializeField, Range(0f, 1f)] private float abilityUseDiscipline = 0.68f;

        private KurokageAgentIdentity identity;
        private KurokageAgentAbilityController abilities;
        private RenkaiTacticalBotAI tacticalBot;
        private KurokageBotPerception perception;
        private RenkaiRoundPlayer owner;
        private float nextDecisionTime;

        private void Awake()
        {
            identity = GetComponent<KurokageAgentIdentity>();
            abilities = GetComponent<KurokageAgentAbilityController>();
            tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            perception = GetComponent<KurokageBotPerception>();
            owner = GetComponent<RenkaiRoundPlayer>();
            ScheduleNextDecision();
        }

        private void OnEnable()
        {
            ScheduleNextDecision();
        }

        private void Update()
        {
            if (owner == null || !owner.isAlive || owner.isHumanPlayer) return;
            if (identity == null || identity.Archetype == KurokageAgentArchetype.Kairi) return;
            if (abilities == null || tacticalBot == null || perception == null) return;
            if (Time.time < nextDecisionTime) return;

            ScheduleNextDecision();
            if (Random.value > abilityUseDiscipline) return;

            RenkaiRoundPlayer target = perception.FindBestVisibleEnemy(
                Object.FindObjectsOfType<RenkaiRoundPlayer>(true),
                owner.team
            );

            int slot = ChooseSlot(target);
            if (slot < 0 || !abilities.IsReady(slot))
            {
                slot = FindReadyFallback();
                if (slot < 0) return;
            }

            Vector3 direction = target != null
                ? target.transform.position - transform.position
                : transform.forward;
            direction.y = 0f;
            abilities.TryActivateSlot(slot, direction);
        }

        public void ResetBrain()
        {
            ScheduleNextDecision();
        }

        private int ChooseSlot(RenkaiRoundPlayer target)
        {
            float health01 = owner.maxHealth > 0f ? owner.health / owner.maxHealth : 1f;
            KurokageAbilityDefinition[] kit = identity.Definition.Abilities;

            if (health01 < 0.42f)
            {
                for (int i = 0; i < kit.Length; i++)
                {
                    if (IsDefensive(kit[i].Action) && abilities.IsReady(i)) return i;
                }
            }

            if (target != null && tacticalBot.state == RenkaiBotState.Engage)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < 8f && abilities.IsReady(2)) return 2;
                if (distance > 12f && abilities.IsReady(0)) return 0;
                if (abilities.IsReady(3) && Random.value < 0.22f) return 3;
                if (abilities.IsReady(1)) return 1;
            }

            if (tacticalBot.state == RenkaiBotState.GuardLink ||
                tacticalBot.state == RenkaiBotState.RotateToCore ||
                tacticalBot.state == RenkaiBotState.Sever)
            {
                if (abilities.IsReady(1)) return 1;
                if (abilities.IsReady(3) && Random.value < 0.35f) return 3;
            }

            return Random.Range(0, 4);
        }

        private int FindReadyFallback()
        {
            int start = Random.Range(0, 4);
            for (int offset = 0; offset < 4; offset++)
            {
                int slot = (start + offset) % 4;
                if (abilities.IsReady(slot)) return slot;
            }
            return -1;
        }

        private static bool IsDefensive(KurokageAbilityAction action)
        {
            switch (action)
            {
                case KurokageAbilityAction.HealingPulse:
                case KurokageAbilityAction.OrbitHeal:
                case KurokageAbilityAction.NetworkRestore:
                case KurokageAbilityAction.KineticShield:
                case KurokageAbilityAction.ResonanceGuard:
                case KurokageAbilityAction.HeatShield:
                case KurokageAbilityAction.Fortify:
                case KurokageAbilityAction.FortressProtocol:
                case KurokageAbilityAction.ColossusProtocol:
                case KurokageAbilityAction.Sanctuary:
                case KurokageAbilityAction.VeilCloak:
                    return true;
                default:
                    return false;
            }
        }

        private void ScheduleNextDecision()
        {
            nextDecisionTime = Time.time + Random.Range(minimumDecisionInterval, maximumDecisionInterval);
        }
    }
}
