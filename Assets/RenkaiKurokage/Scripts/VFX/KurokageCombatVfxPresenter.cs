using UnityEngine;

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

        private static void SpawnDisc(string name, Vector3 point, Vector3 normal, Color color, float scale, float life, float emission)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = point;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            go.transform.localScale = new Vector3(scale, 0.01f, scale);
            ApplyEmission(go.GetComponent<Renderer>(), color, emission);
            Object.Destroy(go, life);
        }

        private static void SpawnSparkCross(Vector3 point, Vector3 normal, Color color, float size, float life)
        {
            Quaternion basis = Quaternion.LookRotation(normal == Vector3.zero ? Vector3.forward : normal);
            for (int i = 0; i < 2; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spark.name = "IMPACT_SPARK";
                Object.Destroy(spark.GetComponent<Collider>());
                spark.transform.position = point;
                spark.transform.rotation = basis * Quaternion.Euler(0f, 0f, 45f + i * 90f);
                spark.transform.localScale = new Vector3(size, 0.015f, 0.015f);
                ApplyEmission(spark.GetComponent<Renderer>(), color, 4.5f);
                Object.Destroy(spark, life);
            }
        }

        private static void ApplyEmission(Renderer renderer, Color color, float emission)
        {
            if (renderer == null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            renderer.sharedMaterial = material;
        }
    }
}
