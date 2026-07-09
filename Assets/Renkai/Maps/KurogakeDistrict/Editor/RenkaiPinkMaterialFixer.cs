
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class RenkaiPinkMaterialFixer
{
    [MenuItem("Renkai/Fix Pink Materials In Current Scene")]
    public static void FixPinkMaterialsInCurrentScene()
    {
        Shader compatible = GetCompatibleShader();
        if (compatible == null)
        {
            EditorUtility.DisplayDialog("Renkai", "Uygun shader bulunamadı. Unity render pipeline ayarlarını kontrol et.", "OK");
            return;
        }

        int fixedCount = 0;
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                Color color;
                float emission;
                GetMaterialLook(mat.name, out color, out emission);

                mat.shader = compatible;
                Apply(mat, color, emission);
                EditorUtility.SetDirty(mat);

                fixedCount++;
                changed = true;
            }

            if (changed)
            {
                renderer.sharedMaterials = mats;
                EditorUtility.SetDirty(renderer);
            }
        }

        EditorUtility.DisplayDialog("Renkai", "Pink material fix tamamlandı.\nDüzeltilen material sayısı: " + fixedCount + "\n\nPlay'e tekrar bas.", "OK");
    }

    private static Shader GetCompatibleShader()
    {
        bool srpActive = GraphicsSettings.currentRenderPipeline != null;

        if (srpActive)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null) return urpLit;

            Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (urpUnlit != null) return urpUnlit;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null) return standard;

        Shader unlit = Shader.Find("Unlit/Color");
        if (unlit != null) return unlit;

        return Shader.Find("Diffuse");
    }

    private static void Apply(Material mat, Color color, float emission)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.12f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.42f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.42f);

        if (emission > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * emission);
        }
    }

    private static void GetMaterialLook(string name, out Color color, out float emission)
    {
        string n = name.ToLowerInvariant();

        emission = 0f;
        color = new Color(0.045f, 0.050f, 0.070f);

        if (n.Contains("floor") || n.Contains("wet") || n.Contains("stone"))
            color = new Color(0.035f, 0.043f, 0.060f);

        if (n.Contains("wall") || n.Contains("metal"))
            color = new Color(0.045f, 0.050f, 0.070f);

        if (n.Contains("trim") || n.Contains("black"))
            color = new Color(0.015f, 0.015f, 0.022f);

        if (n.Contains("wood"))
            color = new Color(0.10f, 0.055f, 0.035f);

        if (n.Contains("roof") || n.Contains("indigo"))
            color = new Color(0.040f, 0.050f, 0.12f);

        if (n.Contains("glass"))
        {
            color = new Color(0.045f, 0.10f, 0.19f);
            emission = 0.25f;
        }

        if (n.Contains("cover") || n.Contains("crate"))
            color = new Color(0.055f, 0.058f, 0.070f);

        if (n.Contains("site_a") || n.Contains("a_site"))
        {
            color = new Color(0.20f, 0.08f, 0.35f);
            emission = 0.7f;
        }

        if (n.Contains("site_b") || n.Contains("b_site"))
        {
            color = new Color(0.08f, 0.15f, 0.35f);
            emission = 0.7f;
        }

        if (n.Contains("purple") || n.Contains("neon"))
        {
            color = new Color(0.65f, 0.10f, 1.00f);
            emission = 2.2f;
        }

        if (n.Contains("blue"))
        {
            color = new Color(0.10f, 0.35f, 1.00f);
            emission = 2.0f;
        }

        if (n.Contains("pink") || n.Contains("sakura"))
        {
            color = new Color(0.95f, 0.35f, 0.70f);
            emission = 0.8f;
        }

        if (n.Contains("portal") || n.Contains("kurogate") || n.Contains("energy"))
        {
            color = new Color(0.55f, 0.06f, 1.00f);
            emission = 3.2f;
        }
    }
}
