using System.Collections.Generic;
using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageArchitecturalResonancePresenter : MonoBehaviour
    {
        [SerializeField] private float idleEmission = 0.72f;
        [SerializeField] private float activeEmission = 1.55f;
        [SerializeField] private float idleFrequency = 0.45f;
        [SerializeField] private float activeFrequency = 2.6f;

        private readonly List<Renderer> animatedRenderers = new List<Renderer>();
        private MaterialPropertyBlock block;
        private ZodiacCoreRuntime core;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            core = FindObjectOfType<ZodiacCoreRuntime>();
            CacheRenderers();
        }

        private void Update()
        {
            if (core == null) core = FindObjectOfType<ZodiacCoreRuntime>();

            float activity = ResolveActivity();
            float frequency = Mathf.Lerp(idleFrequency, activeFrequency, activity);
            float pulse = 0.82f + Mathf.Sin(Time.time * frequency * Mathf.PI * 2f) * 0.18f;
            float intensity = Mathf.Lerp(idleEmission, activeEmission, activity) * pulse;

            foreach (Renderer renderer in animatedRenderers)
            {
                if (renderer == null || renderer.sharedMaterial == null) continue;
                if (!renderer.sharedMaterial.HasProperty("_EmissionColor")) continue;

                Color source = renderer.sharedMaterial.GetColor("_EmissionColor");
                if (source.maxColorComponent <= 0.001f)
                    source = new Color(0.16f, 0.58f, 1f, 1f);

                float maxComponent = Mathf.Max(0.001f, source.maxColorComponent);
                Color normalizedSource = source / maxComponent;
                normalizedSource.a = 1f;

                renderer.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, normalizedSource * intensity);
                renderer.SetPropertyBlock(block);
            }
        }

        private void CacheRenderers()
        {
            animatedRenderers.Clear();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                string n = renderer.gameObject.name;
                if (n.Contains("GUIDE") || n.Contains("APPROACH") || n.Contains("WAYFINDING") || n.Contains("CROSS_MARK"))
                    animatedRenderers.Add(renderer);
            }
        }

        private float ResolveActivity()
        {
            if (core == null) return 0f;
            if (core.State == ZodiacLinkState.Linking) return 0.35f + core.Progress01 * 0.25f;
            if (core.State == ZodiacLinkState.Synchronized) return 0.55f + core.Progress01 * 0.45f;
            if (core.State == ZodiacLinkState.Severing) return 0.75f;
            if (core.State == ZodiacLinkState.Completed) return 1f;
            return 0f;
        }
    }
}
