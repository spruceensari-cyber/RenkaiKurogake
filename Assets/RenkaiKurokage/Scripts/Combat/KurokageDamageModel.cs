using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public enum KurokageDamageType
    {
        Ballistic,
        Blade,
        Ability,
        Objective
    }

    public enum KurokageHitZoneType
    {
        Head,
        Torso,
        Limb
    }

    public struct KurokageDamageInfo
    {
        public float Amount;
        public RenkaiRoundPlayer Attacker;
        public Vector3 Point;
        public Vector3 Normal;
        public KurokageDamageType DamageType;
        public KurokageHitZoneType HitZone;
        public string SourceId;

        public KurokageDamageInfo(
            float amount,
            RenkaiRoundPlayer attacker,
            Vector3 point,
            Vector3 normal,
            KurokageDamageType damageType,
            KurokageHitZoneType hitZone,
            string sourceId)
        {
            Amount = amount;
            Attacker = attacker;
            Point = point;
            Normal = normal;
            DamageType = damageType;
            HitZone = hitZone;
            SourceId = sourceId;
        }
    }
}
