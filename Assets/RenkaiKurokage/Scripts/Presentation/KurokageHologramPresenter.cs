using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageHologramPresenter : MonoBehaviour
    {
        [SerializeField] private Color hologramColor = new Color(0.10f, 0.55f, 1f, 0.72f);
        [SerializeField] private float baseEmission = 1.8f;
        [SerializeField] private float pulseEmission = 3.2f;
        [SerializeField] private float pulseFrequency = 7.5f;
        [SerializeField] private float scanFrequency = 2.4f;
        [SerializeField] private float scanAmplitude = 0.035f;

        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Vector3 basePosition;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            block = new MaterialPropertyBlock();
            basePosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseFrequency);
            float emission = Mathf.Lerp(baseEmission, pulseEmission, pulse);
            float scan = Mathf.Sin(Time.time * scanFrequency * Mathf.PI * 2f) * scanAmplitude;

            Vector3 p = basePosition;
            p.y += scan;
            transform.localPosition = p;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.sharedMaterial == null) continue;
                renderer.GetPropertyBlock(block);

                if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                    block.SetColor(BaseColorId, hologramColor);
                if (renderer.sharedMaterial.HasProperty("_Color"))
                    block.SetColor(ColorId, hologramColor);
                if (renderer.sharedMaterial.HasProperty("_EmissionColor"))
                    block.SetColor(EmissionColorId, hologramColor * emission);

                renderer.SetPropertyBlock(block);
            }
        }
    }
}
