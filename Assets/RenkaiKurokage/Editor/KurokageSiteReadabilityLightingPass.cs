using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KurokageSiteReadabilityLightingPass
{
    private const string RootName = "KUROKAGE_SITE_READABILITY_LIGHTING";

    public static bool ApplySilent()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject(RootName);
        BuildSiteA(root.transform);
        BuildSiteB(root.transform);
        BuildMidTransition(root.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static void BuildSiteA(Transform parent)
    {
        Vector3 center = new Vector3(-34f, 1.2f, 17f);
        CreateSpot(parent, "A_KEY_ARCHIVE", center + new Vector3(0f, 10f, -8f), center + Vector3.up * 1.2f,
            new Color(0.80f, 0.90f, 1f), 1.05f, 28f, 66f);
        CreateSpot(parent, "A_RIM_LEFT", center + new Vector3(-9f, 5.5f, 2f), center + Vector3.up * 1.1f,
            new Color(0.28f, 0.62f, 1f), 0.72f, 24f, 58f);
        CreateSpot(parent, "A_RIM_RIGHT", center + new Vector3(9f, 5.2f, 1f), center + Vector3.up * 1.0f,
            new Color(0.62f, 0.82f, 1f), 0.58f, 22f, 54f);
        CreatePoint(parent, "A_SOFT_BOUNCE", center + new Vector3(0f, 3.8f, 4f), new Color(0.60f, 0.78f, 1f), 0.42f, 18f);
    }

    private static void BuildSiteB(Transform parent)
    {
        Vector3 center = new Vector3(34f, 1.2f, 17f);
        CreateSpot(parent, "B_KEY_REACTOR", center + new Vector3(0f, 10f, -8f), center + Vector3.up * 1.2f,
            new Color(0.72f, 0.80f, 1f), 0.92f, 28f, 66f);
        CreateSpot(parent, "B_RIM_LEFT", center + new Vector3(-9f, 5.2f, 1f), center + Vector3.up * 1.0f,
            new Color(0.42f, 0.54f, 1f), 0.54f, 22f, 54f);
        CreateSpot(parent, "B_RIM_RIGHT", center + new Vector3(9f, 5.5f, 2f), center + Vector3.up * 1.1f,
            new Color(0.58f, 0.38f, 1f), 0.68f, 24f, 58f);
        CreatePoint(parent, "B_SOFT_BOUNCE", center + new Vector3(0f, 3.8f, 4f), new Color(0.46f, 0.58f, 1f), 0.36f, 18f);
    }

    private static void BuildMidTransition(Transform parent)
    {
        CreateSpot(parent, "MID_DUEL_KEY", new Vector3(0f, 12f, -5f), new Vector3(0f, 1.2f, 4f),
            new Color(0.82f, 0.90f, 1f), 0.76f, 34f, 72f);
        CreatePoint(parent, "MID_DUEL_FILL", new Vector3(0f, 5.5f, 12f), new Color(0.56f, 0.72f, 1f), 0.34f, 24f);
    }

    private static void CreateSpot(Transform parent, string name, Vector3 position, Vector3 target, Color color, float intensity, float range, float angle)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.rotation = Quaternion.LookRotation((target - position).normalized);

        Light light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = angle;
        light.shadows = LightShadows.None;
        light.renderMode = LightRenderMode.Auto;
    }

    private static void CreatePoint(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
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
}
