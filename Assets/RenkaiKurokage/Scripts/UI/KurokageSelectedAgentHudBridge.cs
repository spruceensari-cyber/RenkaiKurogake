using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(KurokageAgentIdentity))]
    public sealed class KurokageSelectedAgentHudBridge : MonoBehaviour
    {
        private KurokageAgentIdentity identity;
        private KurokageAgentAbilityController genericAbilities;
        private KairiAbilityController kairiAbilities;
        private readonly List<Text> abilityNames = new List<Text>();
        private readonly List<Image> abilityFills = new List<Image>();
        private Text emblemText;
        private float nextResolveTime;

        private void Awake()
        {
            identity = GetComponent<KurokageAgentIdentity>();
            genericAbilities = GetComponent<KurokageAgentAbilityController>();
            kairiAbilities = GetComponent<KairiAbilityController>();
        }

        private void OnEnable()
        {
            if (identity != null) identity.AgentChanged += OnAgentChanged;
            ResolveHud(true);
        }

        private void OnDisable()
        {
            if (identity != null) identity.AgentChanged -= OnAgentChanged;
        }

        private void LateUpdate()
        {
            ResolveHud(false);
            if (identity == null || abilityFills.Count != 4) return;

            for (int i = 0; i < 4; i++)
            {
                float cooldown = identity.Archetype == KurokageAgentArchetype.Kairi
                    ? KairiCooldown(i)
                    : genericAbilities != null ? genericAbilities.Cooldown01(i) : 0f;
                abilityFills[i].fillAmount = 1f - cooldown;
            }
        }

        private void OnAgentChanged(KurokageAgentDefinition definition)
        {
            ApplyDefinition(definition);
            KurokageJapaneseVoicePresenter voice = GetComponent<KurokageJapaneseVoicePresenter>();
            if (voice != null) voice.PlaySelect();
        }

        private void ResolveHud(bool force)
        {
            if (!force && abilityNames.Count == 4 && abilityFills.Count == 4) return;
            if (!force && Time.unscaledTime < nextResolveTime) return;
            nextResolveTime = Time.unscaledTime + 0.5f;

            GameObject root = GameObject.Find("KUROKAGE_FINAL_HUD");
            if (root == null) return;

            abilityNames.Clear();
            abilityFills.Clear();
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                if (text.gameObject.name == "AbilityName") abilityNames.Add(text);
                else if (text.gameObject.name == "K") emblemText = text;
            }
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.gameObject.name == "AbilityCharge") abilityFills.Add(image);
            }

            if (identity != null) ApplyDefinition(identity.Definition);
        }

        private void ApplyDefinition(KurokageAgentDefinition definition)
        {
            if (definition == null) return;
            for (int i = 0; i < abilityNames.Count && i < definition.Abilities.Length; i++)
            {
                string name = definition.Abilities[i].DisplayName;
                abilityNames[i].text = name.Length > 14 ? name.Substring(0, 14).ToUpperInvariant() : name.ToUpperInvariant();
                abilityNames[i].color = i == 3 ? Color.Lerp(definition.Accent, Color.white, 0.18f) : definition.Accent;
            }

            if (emblemText != null)
            {
                emblemText.text = definition.DisplayName.Substring(0, 1);
                emblemText.color = definition.Accent;
            }
        }

        private float KairiCooldown(int slot)
        {
            if (kairiAbilities == null) return 0f;
            switch (slot)
            {
                case 0: return kairiAbilities.QCooldown01;
                case 1: return kairiAbilities.ECooldown01;
                case 2: return kairiAbilities.CCooldown01;
                case 3: return kairiAbilities.XCooldown01;
                default: return 0f;
            }
        }
    }
}
