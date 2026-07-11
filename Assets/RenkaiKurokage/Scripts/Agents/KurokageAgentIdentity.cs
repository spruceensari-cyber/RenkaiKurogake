using System;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageAgentIdentity : MonoBehaviour
    {
        private const string HumanAgentPreference = "RENKAI_SELECTED_AGENT";

        [SerializeField] private KurokageAgentArchetype archetype = KurokageAgentArchetype.Kairi;
        [SerializeField] private bool persistHumanSelection = true;

        public KurokageAgentArchetype Archetype => archetype;
        public KurokageAgentDefinition Definition => KurokageAgentCatalog.Get(archetype);

        public event Action<KurokageAgentDefinition> AgentChanged;

        private RenkaiRoundPlayer roundPlayer;

        private void Awake()
        {
            roundPlayer = GetComponent<RenkaiRoundPlayer>();
            if (roundPlayer != null && roundPlayer.isHumanPlayer && persistHumanSelection)
            {
                int saved = PlayerPrefs.GetInt(HumanAgentPreference, (int)archetype);
                if (Enum.IsDefined(typeof(KurokageAgentArchetype), saved))
                    archetype = (KurokageAgentArchetype)saved;
            }
            ApplyCurrentDefinition(false);
        }

        public void Configure(KurokageAgentArchetype selectedArchetype, bool persist)
        {
            archetype = selectedArchetype;
            persistHumanSelection = persist;
            ApplyCurrentDefinition(persist);
        }

        public void Select(KurokageAgentArchetype selectedArchetype)
        {
            archetype = selectedArchetype;
            ApplyCurrentDefinition(true);
        }

        private void ApplyCurrentDefinition(bool savePreference)
        {
            if (roundPlayer == null) roundPlayer = GetComponent<RenkaiRoundPlayer>();
            KurokageAgentDefinition definition = Definition;

            if (roundPlayer != null)
                roundPlayer.agentName = definition.FullIdentity;

            KairiAbilityController kairi = GetComponent<KairiAbilityController>();
            if (kairi != null)
                kairi.enabled = archetype == KurokageAgentArchetype.Kairi;

            KurokageAgentAbilityController generic = GetComponent<KurokageAgentAbilityController>();
            if (generic != null)
                generic.enabled = archetype != KurokageAgentArchetype.Kairi;

            KurokageProceduralAgentRig rig = GetComponentInChildren<KurokageProceduralAgentRig>(true);
            if (rig != null)
                rig.Configure(archetype);

            if (roundPlayer != null && roundPlayer.isHumanPlayer && persistHumanSelection && savePreference)
            {
                PlayerPrefs.SetInt(HumanAgentPreference, (int)archetype);
                PlayerPrefs.Save();
            }

            AgentChanged?.Invoke(definition);
        }
    }
}
