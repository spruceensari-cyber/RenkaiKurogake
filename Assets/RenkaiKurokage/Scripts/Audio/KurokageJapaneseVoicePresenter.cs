using System;
using UnityEngine;

namespace Renkai.Kurokage
{
    public enum KurokageVoiceCue
    {
        Select,
        Spawn,
        AbilityQ,
        AbilityE,
        AbilityC,
        AbilityX,
        Reload,
        EnemySpotted,
        Kill,
        RoundWin
    }

    [RequireComponent(typeof(AudioSource))]
    public sealed class KurokageJapaneseVoicePresenter : MonoBehaviour
    {
        [Header("Optional recorded Japanese voice clips")]
        [SerializeField] private AudioClip selectClip;
        [SerializeField] private AudioClip spawnClip;
        [SerializeField] private AudioClip abilityQClip;
        [SerializeField] private AudioClip abilityEClip;
        [SerializeField] private AudioClip abilityCClip;
        [SerializeField] private AudioClip abilityXClip;
        [SerializeField] private AudioClip reloadClip;
        [SerializeField] private AudioClip enemySpottedClip;
        [SerializeField] private AudioClip killClip;
        [SerializeField] private AudioClip roundWinClip;

        public static event Action<string, string, float> SubtitleRequested;

        private AudioSource source;
        private KurokageAgentIdentity identity;
        private AudioClip proceduralSelect;
        private AudioClip proceduralAbility;
        private AudioClip proceduralReload;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = GetComponent<Renkai.Kurogake.RenkaiFPSController>() != null ? 0f : 0.8f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2f;
            source.maxDistance = 24f;
            identity = GetComponent<KurokageAgentIdentity>();
            proceduralSelect = GenerateTone("RENKAI_SELECT", 420f, 620f, 0.22f, 0.18f);
            proceduralAbility = GenerateTone("RENKAI_ABILITY", 520f, 880f, 0.14f, 0.14f);
            proceduralReload = GenerateTone("RENKAI_RELOAD", 240f, 310f, 0.10f, 0.10f);
        }

        public void PlaySelect()
        {
            KurokageAgentDefinition definition = CurrentDefinition();
            Play(KurokageVoiceCue.Select, selectClip, proceduralSelect, definition.SelectVoiceJapanese, definition.SelectVoiceRomanized, 2.8f);
        }

        public void PlayAbility(int slot)
        {
            KurokageAgentDefinition definition = CurrentDefinition();
            slot = Mathf.Clamp(slot, 0, 3);
            KurokageAbilityDefinition ability = definition.Abilities[slot];
            KurokageVoiceCue cue = slot == 0 ? KurokageVoiceCue.AbilityQ :
                slot == 1 ? KurokageVoiceCue.AbilityE :
                slot == 2 ? KurokageVoiceCue.AbilityC : KurokageVoiceCue.AbilityX;
            AudioClip recorded = slot == 0 ? abilityQClip : slot == 1 ? abilityEClip : slot == 2 ? abilityCClip : abilityXClip;
            Play(cue, recorded, proceduralAbility, ability.JapaneseName + "、起動。", ability.DisplayName + " online.", 1.8f);
        }

        public void PlayReload()
        {
            Play(KurokageVoiceCue.Reload, reloadClip, proceduralReload, "装填する。", "Souten suru.", 1.2f);
        }

        public void PlaySpawn()
        {
            KurokageAgentDefinition definition = CurrentDefinition();
            Play(KurokageVoiceCue.Spawn, spawnClip, proceduralSelect, definition.JapaneseTitle + "、接続完了。", definition.Callsign + " link established.", 2f);
        }

        public void PlayEnemySpotted()
        {
            Play(KurokageVoiceCue.EnemySpotted, enemySpottedClip, proceduralAbility, "敵を確認。", "Teki o kakunin.", 1.2f);
        }

        public void PlayKill()
        {
            Play(KurokageVoiceCue.Kill, killClip, proceduralAbility, "一つ、沈黙。", "Hitotsu, chinmoku.", 1.2f);
        }

        public void PlayRoundWin()
        {
            Play(KurokageVoiceCue.RoundWin, roundWinClip, proceduralSelect, "共鳴は私たちのもの。", "Kyoumei wa watashitachi no mono.", 2.2f);
        }

        private void Play(KurokageVoiceCue cue, AudioClip recorded, AudioClip fallback, string japanese, string romanized, float subtitleDuration)
        {
            AudioClip clip = recorded != null ? recorded : fallback;
            if (clip != null && source != null) source.PlayOneShot(clip);
            SubtitleRequested?.Invoke(japanese, romanized, subtitleDuration);
            Debug.Log("[RENKAI VO " + cue + "] " + japanese + " / " + romanized);
        }

        private KurokageAgentDefinition CurrentDefinition()
        {
            if (identity == null) identity = GetComponent<KurokageAgentIdentity>();
            return identity != null ? identity.Definition : KurokageAgentCatalog.Get(KurokageAgentArchetype.Kairi);
        }

        private static AudioClip GenerateTone(string name, float startFrequency, float endFrequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                float envelope = Mathf.Sin(Mathf.PI * t);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * (i / (float)sampleRate)) * envelope * volume;
            }
            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
