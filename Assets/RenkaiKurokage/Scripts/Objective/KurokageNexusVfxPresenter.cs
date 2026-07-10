using UnityEngine;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(ZodiacNexusSite))]
    public sealed class KurokageNexusVfxPresenter : MonoBehaviour
    {
        [SerializeField] private ZodiacNexusSite site;
        [SerializeField] private KurokageZodiacObjectiveController objective;
        [SerializeField] private float idleRotationSpeed = 7f;
        [SerializeField] private float activeRotationSpeed = 38f;

        private Transform innerRing;
        private Transform outerRing;
        private Transform crown;
        private Renderer[] energyRenderers;
        private MaterialPropertyBlock block;
        private ZodiacCoreRuntime core;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            if (site == null) site = GetComponent<ZodiacNexusSite>();
            if (objective == null) objective = FindObjectOfType<KurokageZodiacObjectiveController>();
            core = objective != null ? objective.Core : FindObjectOfType<ZodiacCoreRuntime>();
            block = new MaterialPropertyBlock();
            BindArt();
        }

        private void Update()
        {
            if (site == null) return;
            if (objective == null) objective = FindObjectOfType<KurokageZodiacObjectiveController>();
            if (core == null) core = objective != null ? objective.Core : FindObjectOfType<ZodiacCoreRuntime>();
            if (innerRing == null || outerRing == null) BindArt();

            bool selectedSite = objective != null && objective.ActiveSiteId == site.SiteId;
            float progress = core != null ? core.Progress01 : 0f;
            ZodiacLinkState state = core != null ? core.State : ZodiacLinkState.Idle;
            float activity = selectedSite ? Activity(state, progress) : 0.08f;
            float spin = Mathf.Lerp(idleRotationSpeed, activeRotationSpeed, activity);

            if (innerRing != null)
            {
                innerRing.Rotate(Vector3.up, spin * Time.deltaTime, Space.Self);
                float pulse = 1f + Mathf.Sin(Time.time * Mathf.Lerp(1.8f, 8f, activity)) * Mathf.Lerp(0.01f, 0.075f, activity);
                innerRing.localScale = Vector3.one * pulse;
            }

            if (outerRing != null)
            {
                outerRing.Rotate(Vector3.up, -spin * 0.65f * Time.deltaTime, Space.Self);
                float pulse = 1f + Mathf.Sin(Time.time * Mathf.Lerp(1.4f, 6.5f, activity) + 1.3f) * Mathf.Lerp(0.008f, 0.052f, activity);
                outerRing.localScale = Vector3.one * pulse;
            }

            if (crown != null)
                crown.Rotate(Vector3.up, spin * 0.18f * Time.deltaTime, Space.Self);

            Color color = ResolveColor(state, selectedSite, progress);
            float emission = Mathf.Lerp(0.55f, 4.2f, activity);
            UpdateEnergy(color, emission);
        }

        private void BindArt()
        {
            Transform art = transform.Find("ZODIAC_NEXUS_ART");
            if (art == null) return;
            innerRing = art.Find("NEXUS_RING_INNER");
            outerRing = art.Find("NEXUS_RING_OUTER");
            crown = art.Find("NEXUS_CROWN");
            Transform energyRoot = art.Find("NEXUS_ENERGY");
            energyRenderers = energyRoot != null ? energyRoot.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
        }

        private void UpdateEnergy(Color color, float emission)
        {
            if (energyRenderers == null) return;
            foreach (Renderer renderer in energyRenderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, color * emission);
                block.SetColor(BaseColorId, color);
                block.SetColor(ColorId, color);
                renderer.SetPropertyBlock(block);
            }
        }

        private static float Activity(ZodiacLinkState state, float progress)
        {
            if (state == ZodiacLinkState.Linking) return Mathf.Lerp(0.32f, 0.58f, progress);
            if (state == ZodiacLinkState.Synchronized) return Mathf.Lerp(0.52f, 1f, progress);
            if (state == ZodiacLinkState.Severing) return Mathf.Lerp(0.92f, 0.38f, progress);
            if (state == ZodiacLinkState.Completed) return 1f;
            if (state == ZodiacLinkState.Severed) return 0.18f;
            return 0.12f;
        }

        private static Color ResolveColor(ZodiacLinkState state, bool selectedSite, float progress)
        {
            if (!selectedSite) return new Color(0.10f, 0.26f, 0.42f, 1f);
            if (state == ZodiacLinkState.Severing || state == ZodiacLinkState.Severed)
                return Color.Lerp(new Color(0.46f, 0.20f, 1f, 1f), new Color(0.96f, 0.92f, 1f, 1f), progress);
            if (state == ZodiacLinkState.Synchronized)
                return Color.Lerp(new Color(0.14f, 0.58f, 1f, 1f), new Color(0.42f, 0.24f, 1f, 1f), progress);
            if (state == ZodiacLinkState.Completed)
                return new Color(0.86f, 0.98f, 1f, 1f);
            return new Color(0.16f, 0.62f, 1f, 1f);
        }
    }
}
