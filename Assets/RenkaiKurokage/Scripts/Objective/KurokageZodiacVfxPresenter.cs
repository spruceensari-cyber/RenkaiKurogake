using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageZodiacVfxPresenter : MonoBehaviour
    {
        [SerializeField] private ZodiacCoreRuntime core;
        [SerializeField] private float idlePulseSpeed = 2.2f;
        [SerializeField] private float activePulseSpeed = 7.5f;
        [SerializeField] private float rotationSpeed = 55f;

        private Transform ringA;
        private Transform ringB;
        private Light coreLight;
        private ZodiacLinkState lastState;

        private void Awake()
        {
            if (core == null) core = GetComponent<ZodiacCoreRuntime>();
            BuildVisuals();
            lastState = core != null ? core.State : ZodiacLinkState.Idle;
        }

        private void Update()
        {
            if (core == null) return;

            float speed = core.State == ZodiacLinkState.Idle || core.State == ZodiacLinkState.Carried
                ? idlePulseSpeed
                : activePulseSpeed;

            float pulse = 0.82f + (Mathf.Sin(Time.time * speed) + 1f) * 0.16f;
            if (ringA != null)
            {
                ringA.localScale = Vector3.one * pulse;
                ringA.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            }

            if (ringB != null)
            {
                ringB.localScale = Vector3.one * (1.15f - (pulse - 0.82f));
                ringB.Rotate(Vector3.up, -rotationSpeed * 0.72f * Time.deltaTime, Space.Self);
            }

            if (coreLight != null)
            {
                coreLight.intensity = 1.8f + pulse * 1.4f;
                coreLight.color = StateColor(core.State);
            }

            if (core.State != lastState)
            {
                SpawnStateBurst(StateColor(core.State));
                lastState = core.State;
            }
        }

        private void BuildVisuals()
        {
            Transform existingA = transform.Find("ZODIAC_RING_A");
            Transform existingB = transform.Find("ZODIAC_RING_B");

            ringA = existingA != null ? existingA : CreateRing("ZODIAC_RING_A", 1.15f, 0.035f);
            ringB = existingB != null ? existingB : CreateRing("ZODIAC_RING_B", 1.55f, 0.025f);

            coreLight = GetComponentInChildren<Light>();
            if (coreLight == null)
            {
                GameObject lightGo = new GameObject("ZODIAC_CORE_LIGHT");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = Vector3.up * 0.2f;
                coreLight = lightGo.AddComponent<Light>();
                coreLight.type = LightType.Point;
                coreLight.range = 8f;
                coreLight.intensity = 2.4f;
            }
        }

        private Transform CreateRing(string name, float radius, float thickness)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = name;
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(radius, thickness, radius);
            Collider c = ring.GetComponent<Collider>();
            if (c != null) Destroy(c);
            ApplyEmission(ring.GetComponent<Renderer>(), new Color(0.16f, 0.55f, 1f, 1f), 3.5f);
            return ring.transform;
        }

        private void SpawnStateBurst(Color color)
        {
            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burst.name = "ZODIAC_STATE_BURST";
            burst.transform.position = transform.position;
            burst.transform.localScale = Vector3.one * 0.45f;
            Collider c = burst.GetComponent<Collider>();
            if (c != null) Destroy(c);
            ApplyEmission(burst.GetComponent<Renderer>(), color, 4f);
            Destroy(burst, 0.22f);
        }

        private static Color StateColor(ZodiacLinkState state)
        {
            if (state == ZodiacLinkState.Severing || state == ZodiacLinkState.Severed)
                return new Color(0.9f, 0.18f, 0.52f, 1f);
            if (state == ZodiacLinkState.Completed)
                return new Color(0.18f, 1f, 0.72f, 1f);
            if (state == ZodiacLinkState.Synchronized || state == ZodiacLinkState.Linking)
                return new Color(0.24f, 0.48f, 1f, 1f);
            return new Color(0.2f, 0.72f, 1f, 1f);
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
