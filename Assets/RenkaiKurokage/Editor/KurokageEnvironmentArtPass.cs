using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurokage;

public static class KurokageEnvironmentArtPass
{
    private const string RootName = "KUROKAGE_ENVIRONMENT_ART";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials";

    private static Material darkCeramic;
    private static Material lightComposite;
    private static Material navyMetal;
    private static Material neutralCover;
    private static Material blueAccent;
    private static Material violetAccent;
    private static Material hologram;
    private static Material energyCore;
    private static Material glassSubtle;

    public static bool ApplySilent()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        EnsureMaterials();

        GameObject root = new GameObject(RootName);
        Transform shibuya = Group(root.transform, "SHIBUYA_ZERO_ART");
        Transform archive = Group(root.transform, "CELESTIAL_ARCHIVE_ART");
        Transform reactor = Group(root.transform, "VOID_REACTOR_ART");
        Transform ghost = Group(root.transform, "GHOST_LINE_ART");
        Transform skyline = Group(root.transform, "ORBITAL_SKYLINE_ART");

        BuildShibuyaZero(shibuya);
        BuildCelestialArchive(archive);
        BuildVoidReactor(reactor);
        BuildGhostLine(ghost);
        BuildSkyline(skyline);
        ApplyLightingMood();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static void BuildShibuyaZero(Transform parent)
    {
        // ZERO GATE: original broken orbital transit arch framing Mid without changing collision lanes.
        Transform gate = Group(parent, "THE_ZERO_GATE");
        Beam("ZeroGate_Left", gate, new Vector3(-13f, 9f, 8f), new Vector3(1.4f, 18f, 1.8f), lightComposite, false, new Vector3(0f, 0f, -8f));
        Beam("ZeroGate_Right", gate, new Vector3(13f, 9f, 8f), new Vector3(1.4f, 18f, 1.8f), darkCeramic, false, new Vector3(0f, 0f, 8f));
        Beam("ZeroGate_Bridge", gate, new Vector3(0f, 16.5f, 8f), new Vector3(25f, 1.2f, 1.6f), navyMetal, false);
        Ring("ZeroGate_BrokenRing", gate, new Vector3(0f, 15.5f, 10f), new Vector3(11f, 11f, 0.55f), blueAccent, new Vector3(90f, 0f, 0f), false);

        for (int i = -2; i <= 2; i++)
        {
            float x = i * 6.2f;
            Beam("Mid_Wayfinding_" + i, parent, new Vector3(x, 5.4f, -4f), new Vector3(3.6f, 0.16f, 0.8f), i % 2 == 0 ? hologram : blueAccent, false);
        }

        // Suspended communication blades add depth above playable space only.
        for (int i = 0; i < 6; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * (15f + (i % 3) * 4.5f);
            float z = -18f + (i / 2) * 14f;
            Beam("Shibuya_CommBlade_" + i, parent, new Vector3(x, 7.5f, z), new Vector3(0.45f, 8f, 3.2f), i % 3 == 0 ? lightComposite : navyMetal, false, new Vector3(0f, side * 10f, 0f));
            Beam("Shibuya_BlueStrip_" + i, parent, new Vector3(x - side * 0.28f, 8f, z), new Vector3(0.08f, 5.4f, 2.3f), blueAccent, false, new Vector3(0f, side * 10f, 0f));
        }

        // Controlled identity accent: a sculptural blossom canopy, not gameplay collision.
        Transform blossom = Group(parent, "ORBITAL_BLOSSOM_BEACON");
        Beam("Blossom_Trunk", blossom, new Vector3(-18f, 3f, 28f), new Vector3(0.75f, 6f, 0.75f), darkCeramic, false, new Vector3(0f, 0f, 8f));
        for (int i = 0; i < 7; i++)
        {
            float angle = i * 360f / 7f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 3f + (i % 2) * 0.7f);
            Sphere("Blossom_PetalNode_" + i, blossom, new Vector3(-18f, 6.4f, 28f) + offset, Vector3.one * (0.55f + (i % 3) * 0.08f), violetAccent, false);
        }
    }

    private static void BuildCelestialArchive(Transform parent)
    {
        Transform choir = Group(parent, "THE_MEMORY_CHOIR");
        Vector3 center = new Vector3(-34f, 1.5f, 17f);

        Ring("Archive_BaseRing", choir, center + Vector3.up * 0.05f, new Vector3(5.8f, 5.8f, 0.35f), lightComposite, new Vector3(90f, 0f, 0f), false);
        Ring("Archive_EnergyRing", choir, center + Vector3.up * 0.18f, new Vector3(5.1f, 5.1f, 0.18f), blueAccent, new Vector3(90f, 0f, 0f), false);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 radial = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 7.2f);
            Beam("Archive_Column_" + i, choir, center + radial + Vector3.up * 4.2f, new Vector3(0.8f, 8.4f, 0.8f), i % 2 == 0 ? lightComposite : darkCeramic, false);
            Beam("Archive_DataBand_" + i, choir, center + radial + Vector3.up * 4.6f, new Vector3(0.12f, 5.2f, 0.95f), blueAccent, false, new Vector3(0f, angle, 0f));
        }

        for (int i = 0; i < 5; i++)
        {
            float y = 2.4f + i * 1.15f;
            GameObject plate = Beam("Floating_Archive_Plate_" + i, choir, center + new Vector3(0f, y, 0f), new Vector3(4.2f - i * 0.38f, 0.08f, 1.1f), i % 2 == 0 ? hologram : blueAccent, false, new Vector3(0f, i * 22f, 0f));
            plate.AddComponent<KurokageEnvironmentPulse>();
        }

        Beam("Archive_Backdrop_Light", parent, new Vector3(-34f, 7f, 31.2f), new Vector3(24f, 10f, 0.18f), lightComposite, false);
        for (int i = -3; i <= 3; i++)
            Beam("Archive_Backdrop_Data_" + i, parent, new Vector3(-34f + i * 3f, 7f, 31f), new Vector3(0.12f, 7f, 0.2f), blueAccent, false);
    }

    private static void BuildVoidReactor(Transform parent)
    {
        Transform chamber = Group(parent, "THE_RESONANCE_CHAMBER");
        Vector3 center = new Vector3(34f, 2f, 17f);

        Beam("Reactor_CoreHousing", chamber, center + Vector3.up * 3.3f, new Vector3(5.8f, 7.2f, 5.8f), darkCeramic, false);
        Beam("Reactor_WhiteSpine", chamber, center + new Vector3(0f, 3.5f, -3.05f), new Vector3(1.3f, 6.6f, 0.24f), lightComposite, false);
        Sphere("Reactor_EnergyCore", chamber, center + Vector3.up * 3.4f, Vector3.one * 2.2f, energyCore, false);

        for (int i = 0; i < 3; i++)
        {
            GameObject ring = Ring("Reactor_ResonanceRing_" + i, chamber, center + Vector3.up * (2.3f + i * 1.1f), new Vector3(4.6f + i * 0.6f, 4.6f + i * 0.6f, 0.22f), i == 1 ? violetAccent : navyMetal, new Vector3(90f, 0f, 0f), false);
            KurokageEnvironmentPulse pulse = ring.AddComponent<KurokageEnvironmentPulse>();
        }

        for (int i = -2; i <= 2; i++)
        {
            float x = 34f + i * 4.2f;
            Beam("Reactor_Conduit_" + i, parent, new Vector3(x, 5.2f, 30.4f), new Vector3(1.15f, 9f, 1.15f), navyMetal, false);
            Beam("Reactor_ConduitEnergy_" + i, parent, new Vector3(x, 5.2f, 29.75f), new Vector3(0.18f, 6.5f, 0.12f), i == 0 ? violetAccent : blueAccent, false);
        }

        Beam("Reactor_OverheadRail", parent, new Vector3(34f, 10.8f, 12f), new Vector3(28f, 0.7f, 1.1f), navyMetal, false);
        Beam("Reactor_OverheadSignal", parent, new Vector3(34f, 10.35f, 12f), new Vector3(18f, 0.08f, 1.15f), violetAccent, false);
    }

    private static void BuildGhostLine(Transform parent)
    {
        // The flank route is represented by a non-colliding underground visual spine below the current floor.
        Vector3 origin = new Vector3(0f, -5.2f, 26f);
        Beam("Ghost_Platform_09", parent, origin, new Vector3(34f, 0.8f, 15f), navyMetal, false);
        Beam("Ghost_Rail_Left", parent, origin + new Vector3(-5.4f, 0.6f, 0f), new Vector3(0.42f, 0.22f, 24f), darkCeramic, false);
        Beam("Ghost_Rail_Right", parent, origin + new Vector3(5.4f, 0.6f, 0f), new Vector3(0.42f, 0.22f, 24f), darkCeramic, false);
        Beam("Ghost_Route_Blue", parent, origin + new Vector3(-7.8f, 0.52f, 0f), new Vector3(0.18f, 0.08f, 20f), blueAccent, false);

        for (int i = -3; i <= 3; i++)
        {
            float z = origin.z + i * 4.2f;
            Ring("Ghost_TunnelFrame_" + i, parent, new Vector3(0f, -1.8f, z), new Vector3(8.8f, 8.8f, 0.35f), i % 2 == 0 ? darkCeramic : navyMetal, Vector3.zero, false);
        }

        Beam("Ghost_PlatformSign", parent, new Vector3(-7.5f, -1.8f, 26f), new Vector3(0.2f, 3.8f, 6.4f), hologram, false);
    }

    private static void BuildSkyline(Transform parent)
    {
        // Non-colliding distant masses establish scale without affecting gameplay.
        Vector3[] positions =
        {
            new Vector3(-90f, 28f, 120f), new Vector3(-58f, 42f, 135f), new Vector3(-20f, 34f, 145f),
            new Vector3(28f, 48f, 142f), new Vector3(66f, 31f, 126f), new Vector3(96f, 40f, 118f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 scale = new Vector3(10f + (i % 3) * 5f, 48f + (i % 2) * 22f, 10f + ((i + 1) % 3) * 4f);
            Beam("Orbital_Megastructure_" + i, parent, positions[i], scale, i % 2 == 0 ? navyMetal : darkCeramic, false, new Vector3(0f, i * 9f, 0f));
            Beam("Skyline_LightSpine_" + i, parent, positions[i] + new Vector3(0f, 3f, -scale.z * 0.52f), new Vector3(0.22f, scale.y * 0.62f, 0.16f), blueAccent, false, new Vector3(0f, i * 9f, 0f));
        }

        GameObject orbitalRing = Ring("CELESTIAL_NETWORK_ORBITAL_RING", parent, new Vector3(0f, 72f, 150f), new Vector3(54f, 54f, 1.2f), lightComposite, new Vector3(90f, 0f, 0f), false);
        orbitalRing.AddComponent<KurokageEnvironmentPulse>();
        Ring("CELESTIAL_NETWORK_ENERGY_RING", parent, new Vector3(0f, 72f, 149.4f), new Vector3(49f, 49f, 0.35f), blueAccent, new Vector3(90f, 0f, 0f), false);

        for (int i = 0; i < 4; i++)
        {
            float x = -48f + i * 32f;
            Beam("Floating_Comms_Array_" + i, parent, new Vector3(x, 48f + i * 3f, 105f + i * 4f), new Vector3(8f, 0.8f, 2f), hologram, false, new Vector3(0f, i * 17f, 0f));
        }
    }

    private static void ApplyLightingMood()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.47f, 0.51f, 0.58f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.50f, 0.57f, 0.64f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 78f;
        RenderSettings.fogEndDistance = 210f;

        foreach (Light light in Object.FindObjectsOfType<Light>(true))
        {
            if (light.type == LightType.Directional)
            {
                light.color = new Color(0.90f, 0.94f, 1f);
                light.intensity = 1.08f;
                light.shadows = LightShadows.Soft;
            }
        }
    }

    private static void EnsureMaterials()
    {
        Directory.CreateDirectory(MaterialFolder);
        darkCeramic = MaterialAsset("M_DarkCeramic", new Color(0.035f, 0.045f, 0.065f), 0.72f, 0.10f, Color.black);
        lightComposite = MaterialAsset("M_LightComposite", new Color(0.66f, 0.70f, 0.74f), 0.52f, 0.04f, Color.black);
        navyMetal = MaterialAsset("M_NavyMetal", new Color(0.055f, 0.08f, 0.12f), 0.62f, 0.34f, Color.black);
        neutralCover = MaterialAsset("M_CoverNeutral", new Color(0.24f, 0.28f, 0.34f), 0.40f, 0.10f, Color.black);
        blueAccent = MaterialAsset("M_Accent_Blue", new Color(0.06f, 0.40f, 0.95f), 0.48f, 0.08f, new Color(0.08f, 0.45f, 1f) * 1.8f);
        violetAccent = MaterialAsset("M_Accent_Violet", new Color(0.34f, 0.16f, 0.70f), 0.48f, 0.08f, new Color(0.48f, 0.22f, 1f) * 1.25f);
        hologram = MaterialAsset("M_Hologram", new Color(0.10f, 0.34f, 0.62f), 0.34f, 0.02f, new Color(0.14f, 0.56f, 1f) * 1.9f);
        energyCore = MaterialAsset("M_Energy_Core", new Color(0.12f, 0.45f, 0.96f), 0.58f, 0.12f, new Color(0.18f, 0.58f, 1f) * 2.7f);
        glassSubtle = MaterialAsset("M_Glass_Subtle", new Color(0.22f, 0.32f, 0.42f), 0.82f, 0.04f, Color.black);
        AssetDatabase.SaveAssets();
    }

    private static Material MaterialAsset(string fileName, Color color, float smoothness, float metallic, Color emission)
    {
        string path = MaterialFolder + "/" + fileName + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = ResolveShader();
        if (existing == null)
        {
            existing = new Material(shader) { name = fileName };
            AssetDatabase.CreateAsset(existing, path);
        }
        else if (existing.shader != shader && shader != null)
        {
            existing.shader = shader;
        }

        if (existing.HasProperty("_BaseColor")) existing.SetColor("_BaseColor", color);
        if (existing.HasProperty("_Color")) existing.SetColor("_Color", color);
        if (existing.HasProperty("_Smoothness")) existing.SetFloat("_Smoothness", smoothness);
        if (existing.HasProperty("_Glossiness")) existing.SetFloat("_Glossiness", smoothness);
        if (existing.HasProperty("_Metallic")) existing.SetFloat("_Metallic", metallic);
        if (emission.maxColorComponent > 0.001f && existing.HasProperty("_EmissionColor"))
        {
            existing.EnableKeyword("_EMISSION");
            existing.SetColor("_EmissionColor", emission);
        }
        EditorUtility.SetDirty(existing);
        return existing;
    }

    private static Shader ResolveShader()
    {
        bool srp = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = srp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Diffuse");
        return shader;
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject Beam(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider, Vector3? euler = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.transform.eulerAngles = euler ?? Vector3.zero;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider col = go.GetComponent<Collider>();
        if (!collider && col != null) Object.DestroyImmediate(col);
        return go;
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider col = go.GetComponent<Collider>();
        if (!collider && col != null) Object.DestroyImmediate(col);
        return go;
    }

    private static GameObject Ring(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 euler, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.eulerAngles = euler;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider col = go.GetComponent<Collider>();
        if (!collider && col != null) Object.DestroyImmediate(col);
        return go;
    }
}
