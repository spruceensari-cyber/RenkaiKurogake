using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurokage;

[InitializeOnLoad]
public static class KurokageAutoProductionPass
{
    private const string SceneName = "Renkai_Kurogake_Competitive";
    private const string MarkerName = "KUROKAGE_PRODUCTION_PASS_V1";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/ProductionMaterials";

    static KurokageAutoProductionPass()
    {
        EditorApplication.delayCall += TryAutoApply;
    }

    private static void TryAutoApply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != SceneName) return;
        if (GameObject.Find(MarkerName) != null) return;
        ApplyProductionPass();
    }

    public static void ApplyProductionPass()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        EnsureFolder(MaterialFolder);

        Material floor = GetOrCreateMat("M_Prod_Floor_DarkCeramic", new Color(0.105f, 0.12f, 0.145f), 0.38f, 0.03f);
        Material wallDark = GetOrCreateMat("M_Prod_Wall_BlackCeramic", new Color(0.055f, 0.068f, 0.092f), 0.56f, 0.10f);
        Material wallLight = GetOrCreateMat("M_Prod_Wall_LightComposite", new Color(0.54f, 0.59f, 0.64f), 0.30f, 0.01f);
        Material cover = GetOrCreateMat("M_Prod_Cover_Military", new Color(0.16f, 0.19f, 0.23f), 0.46f, 0.05f);
        Material trim = GetOrCreateMat("M_Prod_Trim_Black", new Color(0.018f, 0.026f, 0.042f), 0.64f, 0.02f);
        Material blue = GetOrCreateMat("M_Prod_Accent_Blue", new Color(0.05f, 0.32f, 0.96f), 0.52f, 1.4f);
        Material violet = GetOrCreateMat("M_Prod_Accent_Violet", new Color(0.42f, 0.12f, 0.88f), 0.50f, 1.0f);
        Material hologram = GetOrCreateMat("M_Prod_Hologram", new Color(0.18f, 0.74f, 1f), 0.20f, 2.0f);
        Material skyline = GetOrCreateMat("M_Prod_Skyline", new Color(0.035f, 0.05f, 0.08f), 0.12f, 0.0f);

        RemapExistingMaterials(floor, wallDark, wallLight, cover, blue, violet);

        GameObject old = GameObject.Find(MarkerName);
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject(MarkerName);
        BuildArchitecture(root.transform, wallDark, wallLight, trim, cover);
        BuildRouteIdentity(root.transform, blue, violet, hologram);
        BuildSkyline(root.transform, skyline, blue, violet);
        BuildLighting(root.transform, blue, violet);
        BuildAtmosphere(root.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();

        Debug.Log("Renkai Kurokage Production Graphics Pass V1 automatically applied and saved.");
    }

    private static void RemapExistingMaterials(Material floor, Material dark, Material light, Material cover, Material blue, Material violet)
    {
        foreach (Renderer r in Object.FindObjectsOfType<Renderer>(true))
        {
            string n = r.gameObject.name.ToLowerInvariant();
            if (n == "floor") r.sharedMaterial = floor;
            else if (n.Contains("route_accent") || n.Contains("a_default")) r.sharedMaterial = n.StartsWith("a_") ? blue : violet;
            else if (n.Contains("cover") || n.Contains("box")) r.sharedMaterial = cover;
            else if (n.Contains("main_left") || n.Contains("mid_left") || n.Contains("mid_right")) r.sharedMaterial = light;
            else if (n.Contains("wall") || n.Contains("site_back")) r.sharedMaterial = dark;
            EditorUtility.SetDirty(r);
        }
    }

    private static void BuildArchitecture(Transform parent, Material dark, Material light, Material trim, Material cover)
    {
        Transform architecture = Group(parent, "ARCHITECTURE_POLISH");

        // Spawn hall and mid gateway.
        Box("SpawnArch_L", new Vector3(-12f, 3.2f, -53f), new Vector3(3f, 6.4f, 3f), light, architecture);
        Box("SpawnArch_R", new Vector3(12f, 3.2f, -53f), new Vector3(3f, 6.4f, 3f), light, architecture);
        Box("SpawnArch_Top", new Vector3(0f, 6.4f, -53f), new Vector3(27f, 1.2f, 3f), dark, architecture);
        Box("SpawnTrim", new Vector3(0f, 5.65f, -51.4f), new Vector3(22f, 0.20f, 0.25f), trim, architecture);

        Box("MidGate_L", new Vector3(-13.5f, 3f, 7f), new Vector3(2.4f, 6f, 5f), dark, architecture);
        Box("MidGate_R", new Vector3(13.5f, 3f, 7f), new Vector3(2.4f, 6f, 5f), dark, architecture);
        Box("MidGate_Top", new Vector3(0f, 6f, 7f), new Vector3(29f, 1.4f, 5f), light, architecture);
        Box("MidBridge", new Vector3(0f, 4.8f, 26f), new Vector3(20f, 0.7f, 3.6f), dark, architecture);

        // Site landmark frames.
        BuildSiteFrame("A", new Vector3(-34f, 0f, 18f), blueSite: true, architecture, dark, light, trim);
        BuildSiteFrame("B", new Vector3(34f, 0f, 18f), blueSite: false, architecture, dark, light, trim);

        // Readable cover silhouettes.
        Box("Mid_SentinelCover", new Vector3(0f, 1.4f, 23f), new Vector3(7f, 2.8f, 2.2f), cover, architecture);
        Box("A_OuterCover", new Vector3(-48f, 1.3f, 22f), new Vector3(3.8f, 2.6f, 7f), cover, architecture);
        Box("B_OuterCover", new Vector3(48f, 1.3f, 22f), new Vector3(3.8f, 2.6f, 7f), cover, architecture);

        // Ceiling ribs create depth without obstructing gameplay.
        for (int i = 0; i < 6; i++)
        {
            float z = -42f + i * 12f;
            Box("Mid_Rib_" + i, new Vector3(0f, 6.8f, z), new Vector3(22f, 0.28f, 0.45f), trim, architecture);
        }
    }

    private static void BuildSiteFrame(string prefix, Vector3 center, bool blueSite, Transform parent, Material dark, Material light, Material trim)
    {
        float x = center.x;
        float z = center.z + 9f;
        Box(prefix + "_Landmark_L", new Vector3(x - 11f, 4f, z), new Vector3(2.2f, 8f, 2.4f), light, parent);
        Box(prefix + "_Landmark_R", new Vector3(x + 11f, 4f, z), new Vector3(2.2f, 8f, 2.4f), light, parent);
        Box(prefix + "_Landmark_Top", new Vector3(x, 7.2f, z), new Vector3(24f, 1.2f, 2.4f), dark, parent);
        Box(prefix + "_Landmark_Trim", new Vector3(x, 6.45f, z - 1.25f), new Vector3(18f, 0.18f, 0.22f), trim, parent);
    }

    private static void BuildRouteIdentity(Transform parent, Material blue, Material violet, Material hologram)
    {
        Transform identity = Group(parent, "ROUTE_IDENTITY");

        // Route light lines.
        for (int i = 0; i < 8; i++)
        {
            float z = -52f + i * 10f;
            Box("A_Line_" + i, new Vector3(-33f, 0.05f, z), new Vector3(0.20f, 0.08f, 5.5f), blue, identity);
            Box("B_Line_" + i, new Vector3(33f, 0.05f, z), new Vector3(0.20f, 0.08f, 5.5f), violet, identity);
        }

        // Hologram landmarks.
        CreateHologram("A_CELESTIAL_ARCHIVE", new Vector3(-34f, 4.5f, 27f), blue, identity, 0.8f);
        CreateHologram("B_VOID_REACTOR", new Vector3(34f, 4.5f, 27f), violet, identity, -0.9f);
        CreateHologram("MID_KUROGATE_DISTRICT", new Vector3(0f, 5.8f, 26f), hologram, identity, 0.45f);

        // Zodiac rings around core area.
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ZODIAC_RING_" + i;
            ring.transform.SetParent(identity);
            ring.transform.position = new Vector3(0f, 1.0f + i * 0.35f, -60f);
            ring.transform.localScale = new Vector3(1.6f + i * 0.42f, 0.02f, 1.6f + i * 0.42f);
            Object.DestroyImmediate(ring.GetComponent<Collider>());
            ring.GetComponent<Renderer>().sharedMaterial = i % 2 == 0 ? blue : hologram;
            ring.AddComponent<KurokageHologramPulse>().rotationSpeed = 12f + i * 8f;
        }
    }

    private static void CreateHologram(string name, Vector3 pos, Material mat, Transform parent, float rotationSpeed)
    {
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        core.name = name;
        core.transform.SetParent(parent);
        core.transform.position = pos;
        core.transform.localScale = new Vector3(2.2f, 0.05f, 2.2f);
        Object.DestroyImmediate(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().sharedMaterial = mat;
        KurokageHologramPulse pulse = core.AddComponent<KurokageHologramPulse>();
        pulse.rotationSpeed = rotationSpeed * 25f;
        pulse.pulseAmount = 0.06f;
    }

    private static void BuildSkyline(Transform parent, Material skyline, Material blue, Material violet)
    {
        Transform city = Group(parent, "DISTANT_TOKYO_2500_SKYLINE");
        int[] heights = { 18, 26, 34, 22, 42, 28, 37, 24, 31, 46, 20, 35 };

        for (int i = 0; i < heights.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / heights.Length;
            float radius = 95f + (i % 3) * 12f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, heights[i] * 0.5f - 1f, Mathf.Sin(angle) * radius + 8f);
            float width = 8f + (i % 4) * 3f;
            Box("MegaStructure_" + i, pos, new Vector3(width, heights[i], width * 0.72f), skyline, city);

            if (i % 2 == 0)
            {
                Material accent = i % 4 == 0 ? blue : violet;
                Box("Skyline_Accent_" + i, pos + new Vector3(0f, heights[i] * 0.23f, -width * 0.38f), new Vector3(width * 0.5f, 0.25f, 0.18f), accent, city);
            }
        }

        // Floating transit rails.
        Box("OrbitalRail_East", new Vector3(76f, 18f, 8f), new Vector3(4f, 1.2f, 115f), skyline, city, new Vector3(0f, 0f, -5f));
        Box("OrbitalRail_West", new Vector3(-78f, 22f, 4f), new Vector3(4f, 1.2f, 125f), skyline, city, new Vector3(0f, 0f, 7f));
    }

    private static void BuildLighting(Transform parent, Material blue, Material violet)
    {
        Transform lights = Group(parent, "PRODUCTION_LIGHTING");

        foreach (Light l in Object.FindObjectsOfType<Light>(true))
        {
            if (l.type == LightType.Point) l.enabled = false;
        }

        Light sun = null;
        foreach (Light l in Object.FindObjectsOfType<Light>(true))
            if (l.type == LightType.Directional) { sun = l; break; }

        if (sun == null)
        {
            GameObject sunGo = new GameObject("ProductionSun");
            sunGo.transform.SetParent(lights);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        sun.transform.rotation = Quaternion.Euler(46f, -38f, 0f);
        sun.color = new Color(0.88f, 0.94f, 1f);
        sun.intensity = 1.08f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.72f;

        AddAreaPoint("A_Key", new Vector3(-34f, 8f, 17f), new Color(0.20f, 0.52f, 1f), 3.2f, 24f, lights);
        AddAreaPoint("B_Key", new Vector3(34f, 8f, 17f), new Color(0.55f, 0.25f, 0.94f), 3.0f, 24f, lights);
        AddAreaPoint("Mid_Key", new Vector3(0f, 9f, 8f), new Color(0.72f, 0.86f, 1f), 2.4f, 30f, lights);
        AddAreaPoint("Spawn_Key", new Vector3(0f, 7f, -53f), new Color(0.42f, 0.68f, 1f), 2.0f, 26f, lights);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.30f, 0.38f, 0.50f);
        RenderSettings.ambientEquatorColor = new Color(0.20f, 0.24f, 0.31f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.10f, 0.14f);
        RenderSettings.reflectionIntensity = 0.82f;

        ReflectionProbe probe = new GameObject("GlobalReflectionProbe").AddComponent<ReflectionProbe>();
        probe.transform.SetParent(lights);
        probe.transform.position = new Vector3(0f, 7f, 2f);
        probe.size = new Vector3(105f, 22f, 145f);
        probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
        probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
        probe.intensity = 0.75f;
    }

    private static void BuildAtmosphere(Transform parent)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.24f, 0.31f, 0.40f);
        RenderSettings.fogStartDistance = 85f;
        RenderSettings.fogEndDistance = 220f;

        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader != null)
        {
            string path = MaterialFolder + "/M_Prod_Skybox.mat";
            Material sky = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (sky == null)
            {
                sky = new Material(skyShader);
                sky.name = "M_Prod_Skybox";
                AssetDatabase.CreateAsset(sky, path);
            }
            sky.SetColor("_SkyTint", new Color(0.30f, 0.42f, 0.62f));
            sky.SetColor("_GroundColor", new Color(0.07f, 0.09f, 0.14f));
            sky.SetFloat("_AtmosphereThickness", 0.78f);
            sky.SetFloat("_Exposure", 1.05f);
            RenderSettings.skybox = sky;
            EditorUtility.SetDirty(sky);
        }
    }

    private static void AddAreaPoint(string name, Vector3 pos, Color color, float intensity, float range, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.None;
    }

    private static Material GetOrCreateMat(string name, Color color, float smoothness, float emission)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            bool srp = GraphicsSettings.currentRenderPipeline != null;
            Shader shader = srp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.14f);

        if (emission > 0f && mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emission);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent, Vector3? euler = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        if (euler.HasValue) go.transform.eulerAngles = euler.Value;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
