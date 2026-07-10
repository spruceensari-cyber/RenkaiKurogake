using UnityEngine;

namespace Renkai.Kurokage
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public sealed class KurokageCompetitivePostFX : MonoBehaviour
    {
        [Header("Shader")]
        [SerializeField] private Shader gradeShader;

        [Header("Competitive Grade")]
        [SerializeField, Range(-1f, 1f)] private float exposure = 0.18f;
        [SerializeField, Range(0.8f, 1.4f)] private float contrast = 1.08f;
        [SerializeField, Range(0.7f, 1.4f)] private float saturation = 1.03f;

        [Header("Highlight Energy")]
        [SerializeField, Range(0f, 1.5f)] private float bloomStrength = 0.32f;
        [SerializeField, Range(0f, 2f)] private float bloomThreshold = 0.76f;
        [SerializeField, Range(0f, 1f)] private float sharpness = 0.20f;

        [Header("Framing")]
        [SerializeField, Range(0f, 0.5f)] private float vignette = 0.12f;
        [SerializeField, Range(0f, 2f)] private float coolShadowStrength = 0.70f;

        private const string ShaderName = "Hidden/Renkai/CompetitiveGrade";
        private Material material;

        private void OnEnable()
        {
            EnsureMaterial();
        }

        private void OnDisable()
        {
            ReleaseMaterial();
        }

        private void OnDestroy()
        {
            ReleaseMaterial();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!EnsureMaterial())
            {
                Graphics.Blit(source, destination);
                return;
            }

            material.SetFloat("_Exposure", exposure);
            material.SetFloat("_Contrast", contrast);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_BloomStrength", bloomStrength);
            material.SetFloat("_BloomThreshold", bloomThreshold);
            material.SetFloat("_Sharpness", sharpness);
            material.SetFloat("_Vignette", vignette);
            material.SetFloat("_CoolShadowStrength", coolShadowStrength);

            Graphics.Blit(source, destination, material);
        }

        public void ConfigureCompetitivePreset(Shader shaderReference = null)
        {
            if (shaderReference != null && shaderReference != gradeShader)
            {
                gradeShader = shaderReference;
                ReleaseMaterial();
            }

            exposure = 0.18f;
            contrast = 1.08f;
            saturation = 1.03f;
            bloomStrength = 0.32f;
            bloomThreshold = 0.76f;
            sharpness = 0.20f;
            vignette = 0.12f;
            coolShadowStrength = 0.70f;
        }

        private bool EnsureMaterial()
        {
            if (material != null) return true;

            if (gradeShader == null)
                gradeShader = Shader.Find(ShaderName);

            if (gradeShader == null || !gradeShader.isSupported)
                return false;

            material = new Material(gradeShader)
            {
                name = "RENKAI_COMPETITIVE_POST_FX_RUNTIME",
                hideFlags = HideFlags.HideAndDontSave
            };
            return true;
        }

        private void ReleaseMaterial()
        {
            if (material == null) return;

            if (Application.isPlaying)
                Destroy(material);
            else
                DestroyImmediate(material);

            material = null;
        }
    }
}
