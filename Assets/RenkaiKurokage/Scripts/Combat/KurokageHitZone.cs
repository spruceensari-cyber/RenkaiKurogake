using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageHitZone : MonoBehaviour
    {
        [SerializeField] private KurokageHitZoneType zoneType = KurokageHitZoneType.Torso;
        [SerializeField] private float damageMultiplier = 1f;

        public KurokageHitZoneType ZoneType => zoneType;
        public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);

        public void Configure(KurokageHitZoneType type, float multiplier)
        {
            zoneType = type;
            damageMultiplier = Mathf.Max(0f, multiplier);
        }
    }
}
