using System.Collections.Generic;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageMatchStatsTracker : MonoBehaviour
    {
        public sealed class PlayerStats
        {
            public RenkaiRoundPlayer Player;
            public int Kills;
            public int Deaths;
            public int Assists;
            public int Damage;
            public int Headshots;
            public int BladeKills;
            public int CorePickups;
        }

        private struct LastHitInfo
        {
            public RenkaiRoundPlayer Attacker;
            public KurokageDamageType DamageType;
            public KurokageHitZoneType HitZone;
            public float Time;
        }

        private readonly Dictionary<RenkaiRoundPlayer, PlayerStats> stats = new Dictionary<RenkaiRoundPlayer, PlayerStats>();
        private readonly Dictionary<RenkaiRoundPlayer, LastHitInfo> lastHitByVictim = new Dictionary<RenkaiRoundPlayer, LastHitInfo>();
        private ZodiacCoreRuntime core;

        public IEnumerable<PlayerStats> AllStats => stats.Values;

        private void Awake()
        {
            BuildRoster();
            core = FindObjectOfType<ZodiacCoreRuntime>();
        }

        private void OnEnable()
        {
            KurokageGameEvents.DamageApplied += OnDamageApplied;
            KurokageGameEvents.KillFeed += OnKillFeed;
            BindCore();
        }

        private void OnDisable()
        {
            KurokageGameEvents.DamageApplied -= OnDamageApplied;
            KurokageGameEvents.KillFeed -= OnKillFeed;
            UnbindCore();
        }

        public PlayerStats GetStats(RenkaiRoundPlayer player)
        {
            if (player == null) return null;
            PlayerStats record;
            if (!stats.TryGetValue(player, out record))
            {
                record = new PlayerStats { Player = player };
                stats.Add(player, record);
            }
            return record;
        }

        private void BuildRoster()
        {
            stats.Clear();
            foreach (RenkaiRoundPlayer player in FindObjectsOfType<RenkaiRoundPlayer>(true))
                if (player != null) stats[player] = new PlayerStats { Player = player };
        }

        private void OnDamageApplied(RenkaiRoundPlayer victim, KurokageDamageInfo info, float healthDamage)
        {
            if (victim == null) return;
            if (info.Attacker != null && info.Attacker != victim)
            {
                PlayerStats attackerStats = GetStats(info.Attacker);
                if (attackerStats != null)
                {
                    attackerStats.Damage += Mathf.RoundToInt(Mathf.Max(0f, info.Amount));
                    if (info.HitZone == KurokageHitZoneType.Head) attackerStats.Headshots++;
                }
            }

            lastHitByVictim[victim] = new LastHitInfo
            {
                Attacker = info.Attacker,
                DamageType = info.DamageType,
                HitZone = info.HitZone,
                Time = Time.time
            };
        }

        private void OnKillFeed(string killerName, string victimName)
        {
            RenkaiRoundPlayer killer = FindByAgentName(killerName);
            RenkaiRoundPlayer victim = FindByAgentName(victimName);

            PlayerStats killerStats = GetStats(killer);
            PlayerStats victimStats = GetStats(victim);
            if (killerStats != null) killerStats.Kills++;
            if (victimStats != null) victimStats.Deaths++;

            LastHitInfo lastHit;
            if (killerStats != null && victim != null && lastHitByVictim.TryGetValue(victim, out lastHit))
            {
                if (lastHit.Attacker == killer && lastHit.DamageType == KurokageDamageType.Blade && Time.time - lastHit.Time < 3f)
                    killerStats.BladeKills++;
            }
        }

        private void BindCore()
        {
            if (core == null) core = FindObjectOfType<ZodiacCoreRuntime>();
            if (core != null) core.CorePickedUp += OnCorePickedUp;
        }

        private void UnbindCore()
        {
            if (core != null) core.CorePickedUp -= OnCorePickedUp;
        }

        private void OnCorePickedUp(Transform carrier)
        {
            if (carrier == null) return;
            RenkaiRoundPlayer player = carrier.GetComponentInParent<RenkaiRoundPlayer>();
            PlayerStats playerStats = GetStats(player);
            if (playerStats != null) playerStats.CorePickups++;
        }

        private RenkaiRoundPlayer FindByAgentName(string agentName)
        {
            foreach (KeyValuePair<RenkaiRoundPlayer, PlayerStats> pair in stats)
            {
                if (pair.Key != null && pair.Key.agentName == agentName)
                    return pair.Key;
            }
            return null;
        }
    }
}
