using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageCombatVfxPresenter : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float impactLife = 0.16f;
        [SerializeField] private float sparkLife = 0.10f;
        [SerializeField] private float headshotLife = 0.24f;

        [Header("Scale")]
        [SerializeField] private float worldImpactScale = 0.14f;
        [SerializeField] private float armorImpactScale = 0.22f;
        [SerializeField] private float headshotScale = 0.32f;

        private void OnEnable()
        {
            KurokageGameEvents.ArmorBroken += OnArmorBroken;
        }

        private void OnDisable()
        {
            KurokageGameEvents.ArmorBroken -= OnArmorBroken;
        }

        public void SpawnWorldImpact(Vector3 point, Vector3 normal)
        {
            SpawnDisc("WORLD_IMPACT", point + normal * 0.015f, normal, new Color(0.72f, 0.78f, 0.88f, 1f), worldImpactScale, impactLife, 1.6f);
        }

        public void SpawnBodyImpact(Vector3 point, Vector3 normal)
        {
            SpawnDisc("BODY_IMPACT", point + normal * 0.02f, normal, new Color(0.18f, 0.55f, 1f, 1f), armorImpactScale * 0.82f, sparkLife, 2.8f);
        }

        public void SpawnArmorImpact(Vector3 point, Vector3 normal)
        {
            SpawnDisc("ARMOR_SPARK", point + normal * 0.02f, normal, new Color(0.28f, 0.78f, 1f, 1f), armorImpactScale, sparkLife, 4.2f);
            SpawnSparkCross(point, normal, new Color(0.52f, 0.90f, 1f, 1f), armorImpactScale * 1.6f, sparkLife);
        }

        public void SpawnHeadshotImpact(Vector3 point, Vector3 normal)
        {
            SpawnDisc("HEADSHOT_BURST", point + normal * 0.025f, normal, new Color(1f, 0.18f, 0.42f, 1f), headshotScale, headshotLife, 5.0f);
            SpawnSparkCross(point, normal, new Color(1f, 0.58f, 0.82f, 1f), headshotScale * 2.0f, headshotLife);
        }

        public void SpawnArmorBreak(Vector3 point, Vector3 normal)
        {
            SpawnDisc("ARMOR_BREAK", point + normal * 0.03f, normal, new Color(0.40f, 0.90f, 1f, 1f), armorImpactScale * 1.45f, headshotLife, 5.2f);
            SpawnSparkCross(point, normal, new Color(0.72f, 0.96f, 1f, 1f), armorImpactScale * 2.1f, headshotLife);
        }

        private void OnArmorBroken(RenkaiRoundPlayer victim, KurokageDamageInfo info)
        {
            Vector3 point = info.Point;
            Vector3 normal = info.Normal == Vector3.zero ? Vector3.up : info.Normal;
            if (point == Vector3.zero && victim != null)
                point = victim.transform.position + Vector3.up * 1.1f;
            SpawnArmorBreak(point, normal);
        }

        private static void SpawnDisc(string name, Vector3 point, Vector3 normal, Color color, float scale, float life, float emission)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                name,
                point,
                Quaternion.FromToRotation(Vector3.up, normal == Vector3.zero ? Vector3.up : normal),
                new Vector3(scale, 0.01f, scale),
                color,
                emission,
                life
            );
        }

        private static void SpawnSparkCross(Vector3 point, Vector3 normal, Color color, float size, float life)
        {
            Quaternion basis = Quaternion.LookRotation(normal == Vector3.zero ? Vector3.forward : normal);
            for (int i = 0; i < 2; i++)
            {
                KurokageVfxPool.Instance.Spawn(
                    KurokageVfxShape.Cube,
                    "IMPACT_SPARK",
                    point,
                    basis * Quaternion.Euler(0f, 0f, 45f + i * 90f),
                    new Vector3(size, 0.015f, 0.015f),
                    color,
                    4.5f,
                    life
                );
            }
        }
    }
}
