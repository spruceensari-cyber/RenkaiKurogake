using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageAgentReadabilityPresenter : MonoBehaviour
    {
        [SerializeField] private Color accent = new Color(0.14f, 0.48f, 1f, 1f);
        [SerializeField] private float emissionStrength = 0.18f;
        [SerializeField] private float nearDistance = 6f;
        [SerializeField] private float farDistance = 38f;
        [SerializeField] private float farReadabilityBoost = 1.55f;
        [SerializeField] private float hitPulseStrength = 0.42f;
        [SerializeField] private float hitPulseDuration = 0.16f;

        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private RenkaiRoundPlayer owner;
        private Camera viewCamera;
        private float hitPulseUntil;
        private float nextUpdate;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            owner = GetComponentInParent<RenkaiRoundPlayer>();
            viewCamera = Camera.main;
            CacheRenderers();
            Apply(emissionStrength);
        }

        private void OnEnable()
        {
            KurokageGameEvents.DamageApplied += OnDamageApplied;
            if (renderers == null || renderers.Length == 0) CacheRenderers();
            Apply(emissionStrength);
        }

        private void OnDisable()
        {
            KurokageGameEvents.DamageApplied -= OnDamageApplied;
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + 0.08f;

            if (viewCamera == null) viewCamera = Camera.main;
            float strength = emissionStrength;

            if (viewCamera != null)
            {
                float distance = Vector3.Distance(viewCamera.transform.position, transform.position);
                float t = Mathf.InverseLerp(nearDistance, farDistance, distance);
                strength *= Mathf.Lerp(1f, farReadabilityBoost, t);
            }

            if (Time.time < hitPulseUntil)
                strength = Mathf.Max(strength, hitPulseStrength);

            Apply(Mathf.Clamp01(strength));
        }

        public void Configure(Color teamAccent, float strength)
        {
            accent = teamAccent;
            emissionStrength = Mathf.Clamp(strength, 0f, 1f);
            if (block == null) block = new MaterialPropertyBlock();
            CacheRenderers();
            Apply(emissionStrength);
        }

        private void OnDamageApplied(RenkaiRoundPlayer victim, KurokageDamageInfo info, float healthDamage)
        {
            if (owner == null) owner = GetComponentInParent<RenkaiRoundPlayer>();
            if (victim == owner)
                hitPulseUntil = Time.time + hitPulseDuration;
        }

        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            if (block == null) block = new MaterialPropertyBlock();
        }

        private void Apply(float strength)
        {
            if (renderers == null || block == null) return;

            Color emission = accent * strength;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.sharedMaterial == null) continue;
                if (!renderer.sharedMaterial.HasProperty("_EmissionColor")) continue;

                renderer.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, emission);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
