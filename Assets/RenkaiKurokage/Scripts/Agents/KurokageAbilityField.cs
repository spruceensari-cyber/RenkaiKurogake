using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageAbilityField : MonoBehaviour
    {
        private RenkaiRoundPlayer owner;
        private float radius;
        private float duration;
        private float damagePerTick;
        private float healingPerTick;
        private Color color;
        private float expireTime;
        private float nextTick;

        public void Configure(
            RenkaiRoundPlayer fieldOwner,
            float fieldRadius,
            float fieldDuration,
            float damage,
            float healing,
            Color fieldColor)
        {
            owner = fieldOwner;
            radius = Mathf.Max(1f, fieldRadius);
            duration = Mathf.Max(0.2f, fieldDuration);
            damagePerTick = Mathf.Max(0f, damage);
            healingPerTick = Mathf.Max(0f, healing);
            color = fieldColor;
            expireTime = Time.time + duration;
            nextTick = Time.time;
        }

        private void Update()
        {
            if (Time.time >= expireTime)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.time < nextTick) return;
            nextTick = Time.time + 0.45f;

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "RENKAI_AGENT_FIELD",
                transform.position + Vector3.up * 0.025f,
                Quaternion.identity,
                new Vector3(radius * 2f, 0.025f, radius * 2f),
                color,
                1.6f,
                0.32f
            );

            Collider[] hits = Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Collide);
            foreach (Collider hit in hits)
            {
                RenkaiRoundPlayer player = hit.GetComponentInParent<RenkaiRoundPlayer>();
                if (player == null || !player.isAlive || owner == null) continue;

                if (player.team != owner.team && damagePerTick > 0f)
                {
                    KurokageDamageInfo info = new KurokageDamageInfo(
                        damagePerTick,
                        owner,
                        player.transform.position,
                        Vector3.up,
                        KurokageDamageType.Ability,
                        KurokageHitZoneType.Torso,
                        "AGENT_FIELD"
                    );
                    player.ApplyDamage(info);
                }
                else if (player.team == owner.team && healingPerTick > 0f)
                {
                    player.health = Mathf.Min(player.maxHealth, player.health + healingPerTick);
                }
            }
        }
    }
}
