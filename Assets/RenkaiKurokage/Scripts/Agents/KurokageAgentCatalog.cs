using System;
using System.Collections.Generic;
using UnityEngine;

namespace Renkai.Kurokage
{
    public enum KurokageAbilityAction
    {
        DirectionalDash,
        HolographicDecoy,
        MomentumLeap,
        EclipseBlade,
        PulseScan,
        NullScreen,
        PhaseStep,
        Overclock,
        BladeLunge,
        ResonanceGuard,
        AirCut,
        HuntProtocol,
        HealingPulse,
        SupportDrone,
        BarrierField,
        NetworkRestore,
        KineticShield,
        GravityAnchor,
        BulwarkStep,
        FortressProtocol,
        SpearDash,
        WindScreen,
        VaultStrike,
        TempestChain,
        BreakerCharge,
        HeatShield,
        GroundSlam,
        ForgeDrive,
        OrbitHeal,
        BlossomDecoy,
        PetalStep,
        Sanctuary,
        AnchorPull,
        Fortify,
        HeavyLeap,
        ColossusProtocol,
        ShadowStep,
        VeilCloak,
        EchoMine,
        WraithProtocol
    }

    [Serializable]
    public sealed class KurokageAbilityDefinition
    {
        public string Slot;
        public string DisplayName;
        public string JapaneseName;
        public KurokageAbilityAction Action;
        public float Cooldown;
        public float Strength;
        public float Duration;

        public KurokageAbilityDefinition(
            string slot,
            string displayName,
            string japaneseName,
            KurokageAbilityAction action,
            float cooldown,
            float strength,
            float duration)
        {
            Slot = slot;
            DisplayName = displayName;
            JapaneseName = japaneseName;
            Action = action;
            Cooldown = cooldown;
            Strength = strength;
            Duration = duration;
        }
    }

    [Serializable]
    public sealed class KurokageAgentDefinition
    {
        public KurokageAgentArchetype Archetype;
        public string DisplayName;
        public string Callsign;
        public string Role;
        public string JapaneseTitle;
        public string SelectVoiceJapanese;
        public string SelectVoiceRomanized;
        public Color Accent;
        public KurokageAbilityDefinition[] Abilities;

        public string FullIdentity => DisplayName + " // " + Callsign;

        public KurokageAgentDefinition(
            KurokageAgentArchetype archetype,
            string displayName,
            string callsign,
            string role,
            string japaneseTitle,
            string selectVoiceJapanese,
            string selectVoiceRomanized,
            Color accent,
            params KurokageAbilityDefinition[] abilities)
        {
            Archetype = archetype;
            DisplayName = displayName;
            Callsign = callsign;
            Role = role;
            JapaneseTitle = japaneseTitle;
            SelectVoiceJapanese = selectVoiceJapanese;
            SelectVoiceRomanized = selectVoiceRomanized;
            Accent = accent;
            Abilities = abilities;
        }
    }

    public static class KurokageAgentCatalog
    {
        private static readonly KurokageAgentDefinition[] Definitions =
        {
            new KurokageAgentDefinition(
                KurokageAgentArchetype.Kairi, "KAIRI", "RIFT-07", "KINETIC INFILTRATOR", "裂界の刃",
                "静かに裂く。私が道を開く。", "Shizuka ni saku. Watashi ga michi o hiraku.",
                new Color(0.12f, 0.62f, 1f),
                Ability("Q", "Directional Dash", "方向加速", KurokageAbilityAction.DirectionalDash, 6f, 7.5f, 0.16f),
                Ability("E", "Holographic Decoy", "幻影投射", KurokageAbilityAction.HolographicDecoy, 14f, 6f, 6f),
                Ability("C", "Momentum Leap", "運動跳躍", KurokageAbilityAction.MomentumLeap, 11f, 9.5f, 0.55f),
                Ability("X", "Eclipse Blade Protocol", "蝕刃機構", KurokageAbilityAction.EclipseBlade, 45f, 1.18f, 12f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Noa, "NOA", "PULSE-D5", "SIGNAL CONTROLLER", "静脈の観測者",
                "全部見える。焦らず、線を支配する。", "Zenbu mieru. Aserazu, sen o shihai suru.",
                new Color(1f, 0.42f, 0.24f),
                Ability("Q", "Pulse Scan", "脈動索敵", KurokageAbilityAction.PulseScan, 12f, 22f, 2.5f),
                Ability("E", "Null Screen", "無音障壁", KurokageAbilityAction.NullScreen, 15f, 8f, 5f),
                Ability("C", "Phase Step", "位相歩法", KurokageAbilityAction.PhaseStep, 9f, 5.5f, 0.12f),
                Ability("X", "Overclock Lattice", "超過格子", KurokageAbilityAction.Overclock, 48f, 1.22f, 10f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Reiha, "REIHA", "VEIL-A3", "RESONANCE DUELIST", "紫電の狩人",
                "一閃で十分。残響だけを置いていく。", "Issen de juubun. Zankyou dake o oite iku.",
                new Color(0.65f, 0.30f, 1f),
                Ability("Q", "Blade Lunge", "刃走", KurokageAbilityAction.BladeLunge, 7f, 6.2f, 0.2f),
                Ability("E", "Resonance Guard", "共鳴防御", KurokageAbilityAction.ResonanceGuard, 14f, 28f, 4f),
                Ability("C", "Air Cut", "空断", KurokageAbilityAction.AirCut, 10f, 34f, 0.4f),
                Ability("X", "Hunt Protocol", "狩猟機構", KurokageAbilityAction.HuntProtocol, 46f, 1.2f, 11f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Mio, "MIO", "GLINT-C2", "NETWORK SUPPORT", "星網の医師",
                "繋がって。誰もここで失わせない。", "Tsunagatte. Dare mo koko de ushinawasenai.",
                new Color(0.18f, 0.88f, 0.76f),
                Ability("Q", "Healing Pulse", "治癒波", KurokageAbilityAction.HealingPulse, 12f, 24f, 0.1f),
                Ability("E", "Support Drone", "補助機", KurokageAbilityAction.SupportDrone, 16f, 7f, 8f),
                Ability("C", "Barrier Field", "防護領域", KurokageAbilityAction.BarrierField, 18f, 30f, 6f),
                Ability("X", "Network Restore", "星網再生", KurokageAbilityAction.NetworkRestore, 52f, 42f, 0.5f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Sora, "SORA", "BASTION-R4", "GRAVITY SENTINEL", "天蓋の守護者",
                "ここは通さない。重力は私の味方だ。", "Koko wa toosanai. Juuryoku wa watashi no mikata da.",
                new Color(1f, 0.68f, 0.18f),
                Ability("Q", "Kinetic Shield", "運動盾", KurokageAbilityAction.KineticShield, 12f, 30f, 4f),
                Ability("E", "Gravity Anchor", "重力錨", KurokageAbilityAction.GravityAnchor, 16f, 7f, 5f),
                Ability("C", "Bulwark Step", "城壁歩", KurokageAbilityAction.BulwarkStep, 9f, 4.5f, 0.18f),
                Ability("X", "Fortress Protocol", "要塞機構", KurokageAbilityAction.FortressProtocol, 50f, 50f, 10f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Aiko, "AIKO", "LANCER-M9", "AERIAL INITIATOR", "風槍の先導者",
                "風を読んで。先に高所を取る。", "Kaze o yonde. Saki ni kousho o toru.",
                new Color(0.30f, 0.90f, 0.58f),
                Ability("Q", "Spear Dash", "風槍突進", KurokageAbilityAction.SpearDash, 8f, 6.8f, 0.18f),
                Ability("E", "Wind Screen", "風幕", KurokageAbilityAction.WindScreen, 15f, 8f, 5f),
                Ability("C", "Vault Strike", "跳撃", KurokageAbilityAction.VaultStrike, 11f, 8.5f, 0.5f),
                Ability("X", "Tempest Chain", "嵐鎖", KurokageAbilityAction.TempestChain, 47f, 40f, 8f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Ren, "REN", "FORGE-K6", "BREACHER", "炎鉄の破城者",
                "扉がないなら、作ればいい。", "Tobira ga nai nara, tsukureba ii.",
                new Color(1f, 0.34f, 0.12f),
                Ability("Q", "Breaker Charge", "破城突撃", KurokageAbilityAction.BreakerCharge, 9f, 6f, 0.24f),
                Ability("E", "Heat Shield", "熱装甲", KurokageAbilityAction.HeatShield, 15f, 34f, 5f),
                Ability("C", "Ground Slam", "地砕", KurokageAbilityAction.GroundSlam, 13f, 38f, 0.4f),
                Ability("X", "Forge Drive", "鍛造駆動", KurokageAbilityAction.ForgeDrive, 48f, 1.25f, 10f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Hana, "HANA", "ORBIT-S1", "ORBITAL MEDIC", "花環の導師",
                "花は散っても、命は繋ぐ。", "Hana wa chitte mo, inochi wa tsunagu.",
                new Color(1f, 0.46f, 0.72f),
                Ability("Q", "Orbit Heal", "軌道治癒", KurokageAbilityAction.OrbitHeal, 11f, 20f, 0.2f),
                Ability("E", "Blossom Decoy", "花影", KurokageAbilityAction.BlossomDecoy, 15f, 6f, 6f),
                Ability("C", "Petal Step", "花弁歩", KurokageAbilityAction.PetalStep, 8f, 5.2f, 0.16f),
                Ability("X", "Sanctuary Bloom", "聖花域", KurokageAbilityAction.Sanctuary, 50f, 36f, 8f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Toma, "TOMA", "ANCHOR-V8", "HEAVY WARDEN", "不動の番人",
                "退かない。ここが最後の線だ。", "Hikanai. Koko ga saigo no sen da.",
                new Color(0.45f, 0.92f, 0.28f),
                Ability("Q", "Anchor Pull", "錨引", KurokageAbilityAction.AnchorPull, 13f, 7f, 0.3f),
                Ability("E", "Fortify", "不動装甲", KurokageAbilityAction.Fortify, 15f, 40f, 6f),
                Ability("C", "Heavy Leap", "重跳", KurokageAbilityAction.HeavyLeap, 12f, 7.2f, 0.6f),
                Ability("X", "Colossus Protocol", "巨神機構", KurokageAbilityAction.ColossusProtocol, 54f, 55f, 11f)),

            new KurokageAgentDefinition(
                KurokageAgentArchetype.Yori, "YORI", "WRAITH-N2", "VEIL ASSASSIN", "無影の潜行者",
                "見えないなら、止められない。", "Mienai nara, tomerarenai.",
                new Color(0.40f, 0.56f, 1f),
                Ability("Q", "Shadow Step", "影歩", KurokageAbilityAction.ShadowStep, 8f, 6.4f, 0.14f),
                Ability("E", "Veil Cloak", "隠形幕", KurokageAbilityAction.VeilCloak, 17f, 0.55f, 5f),
                Ability("C", "Echo Mine", "残響雷", KurokageAbilityAction.EchoMine, 13f, 32f, 7f),
                Ability("X", "Wraith Protocol", "無影機構", KurokageAbilityAction.WraithProtocol, 50f, 1.24f, 10f))
        };

        private static readonly Dictionary<KurokageAgentArchetype, KurokageAgentDefinition> Lookup = BuildLookup();

        public static IReadOnlyList<KurokageAgentDefinition> All => Definitions;

        public static KurokageAgentDefinition Get(KurokageAgentArchetype archetype)
        {
            return Lookup.TryGetValue(archetype, out KurokageAgentDefinition definition)
                ? definition
                : Definitions[0];
        }

        private static Dictionary<KurokageAgentArchetype, KurokageAgentDefinition> BuildLookup()
        {
            Dictionary<KurokageAgentArchetype, KurokageAgentDefinition> lookup = new Dictionary<KurokageAgentArchetype, KurokageAgentDefinition>();
            foreach (KurokageAgentDefinition definition in Definitions)
                lookup[definition.Archetype] = definition;
            return lookup;
        }

        private static KurokageAbilityDefinition Ability(
            string slot,
            string displayName,
            string japaneseName,
            KurokageAbilityAction action,
            float cooldown,
            float strength,
            float duration)
        {
            return new KurokageAbilityDefinition(slot, displayName, japaneseName, action, cooldown, strength, duration);
        }
    }
}
