using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageAgentReadabilityPresenter : MonoBehaviour
    {
        [SerializeField] private Color accent = new Color(0.14f, 0.48f, 1f, 1f);
        [SerializeField] private float emissionStrength = 0.18f;

        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            CacheRenderers();
            Apply();
        }

        private void OnEnable()
        {
            if (renderers == null || renderers.Length == 0) CacheRenderers();
            Apply();
        }

        public void Configure(Color teamAccent, float strength)
        {
            accent = teamAccent;
            emissionStrength = Mathf.Clamp(strength, 0f, 1f);
            if (block == null) block = new MaterialPropertyBlock();
            CacheRenderers();
            Apply();
        }

        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            if (block == null) block = new MaterialPropertyBlock();
        }

        private void Apply()
        {
            if (renderers == null || block == null) return;

            Color emission = accent * emissionStrength;
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
