using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class KurokageBrightCompetitiveVisualPass
{
    private const string RootName = "KUROKAGE_BRIGHT_VISUAL_PASS";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    public static bool ApplySilent()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject(RootName);

        ApplyWorldTone();
        ApplyMaterialTone();
        BuildReadabilityLighting(root.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        return true;
    }

    private static void ApplyWorldTone()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.67f, 0.75f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.60f, 0.66f, 0.74f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 94f;
        RenderSettings.fogEndDistance = 245f;

        foreach (Light light in Object.FindObjectsOfType<Light>(true))
        {
            if (light == null || light.type != LightType.Directional) continue;
            light.color = new Color(0.96f, 0.975f, 1f);
            light.intensity = 1.32f;
            light.shadows = LightShadows.Soft;
        }
    }

    private static void ApplyMaterialTone()
    {
        SetMaterial("M_DarkCeramic", new Color(0.075f, 0.09f, 0.12f), 0.70f, 0.12f, Color.black);
        SetMaterial("M_LightComposite", new Color(0.80f, 0.84f, 0.88f), 0.56f, 0.05f, Color.black);
        SetMaterial("M_NavyMetal", new Color(0.105f, 0.145f, 0.205f), 0.64f, 0.36f, Color.black);
        SetMaterial("M_CoverNeutral", new Color(0.33f, 0.38f, 0.45f), 0.44f, 0.12f, Color.black);
        SetMaterial("M_Accent_Blue", new Color(0.08f, 0.46f, 1f), 0.50f, 0.08f, new Color(0.12f, 0.52f, 1f) * 1.55f);
        SetMaterial("M_Accent_Violet", new Color(0.40f, 0.22f, 0.76f), 0.50f, 0.08f, new Color(0.52f, 0.30f, 1f) * 0.95f);
        SetMaterial("M_Hologram", new Color(0.16f, 0.42f, 0.70f), 0.38f, 0.02f, new Color(0.18f, 0.62f, 1f) * 1.55f);
        SetMaterial("M_Energy_Core", new Color(0.18f, 0.54f, 1f), 0.60f, 0.12f, new Color(0.22f, 0.66f, 1f) * 2.25f);
        SetMaterial("M_Glass_Subtle", new Color(0.32f, 0.40f, 0.50f), 0.84f, 0.04f, Color.black);
    }

    private static void BuildReadabilityLighting(Transform parent)
    {
        CreateFillLight(parent, "SHIBUYA_ZERO_FILL", new Vector3(0f, 8f, 4f), new Color(0.78f, 0.88f, 1f), 0.56f, 44f);
        CreateFillLight(parent, "CELESTIAL_ARCHIVE_FILL", new Vector3(-34f, 8f, 17f), new Color(0.88f, 0.94f, 1f), 0.74f, 38f);
        CreateFillLight(parent, "VOID_REACTOR_FILL", new Vector3(34f, 8f, 17f), new Color(0.66f, 0.76f, 1f), 0.52f, 36f);
        CreateFillLight(parent, "GHOST_LINE_FILL", new Vector3(0f, -0.8f, 26f), new Color(0.72f, 0.84f, 1f), 0.46f, 30f);
    }

    private static void CreateFillLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        light.renderMode = LightRenderMode.Auto;
    }

    private static void SetMaterial(string name, Color color, float smoothness, float metallic, Color emission)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + name + ".mat");
        if (material == null) return;

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);

        if (material.HasProperty("_EmissionColor"))
        {
            if (emission.maxColorComponent > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }

        EditorUtility.SetDirty(material);
    }
}
