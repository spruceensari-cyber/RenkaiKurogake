using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds a non-colliding cinematic city layer over the competitive arena.
/// </summary>
public static class KurokageCinematicDistrictPass
{
    private const string RootName = "KUROKAGE_CINEMATIC_PRESENTATION";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials";

    private static Material wetStreet;
    private static Material wetHighlight;
    private static Material facadeDark;
    private static Material facadeLight;
    private static Material windowCool;
    private static Material windowWarm;
    private static Material neonCyan;
    private static Material neonViolet;
    private static Material signalWhite;
    private static Material rainMaterial;

    public static bool ApplySilent()
    {
        GameObject previous = GameObject.Find(RootName);
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);

        if (!EnsureMaterials()) return false;

        GameObject root = new GameObject(RootName);
        Transform street = Group(root.transform, "WET_STREET");
        Transform canyon = Group(root.transform, "ARRIVAL_CANYON");
        Transform transit = Group(root.transform, "ELEVATED_TRANSIT");
        Transform skyline = Group(root.transform, "DISTANT_CITY");
        Transform signals = Group(root.transform, "NEON_SIGNAL_GRID");
        Transform weather = Group(root.transform, "WEATHER");

        BuildWetStreet(street);
        BuildArrivalCanyon(canyon);
        BuildElevatedTransit(transit);
        BuildDistantCity(skyline);
        BuildSignalGrid(signals);
        BuildWeather(weather);
        ApplyCinematicLighting(root.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        return true;
    }

    private static void BuildWetStreet(Transform parent)
    {
        Box("RAIN_POLISHED_STREET", parent, new Vector3(0f, 0.012f, -1f), new Vector3(106f, 0.024f, 146f), wetStreet, Vector3.zero);

        Vector3[] puddles =
        {
            new Vector3(-3.8f, 0.031f, -53f), new Vector3(3.6f, 0.031f, -46f), new Vector3(-2.3f, 0.031f, -28f),
            new Vector3(4.8f, 0.031f, -8f), new Vector3(-25f, 0.031f, 10f), new Vector3(31f, 0.031f, 13f)
        };
        Vector3[] scales =
        {
            new Vector3(5.8f, 0.009f, 2.1f), new Vector3(4.2f, 0.009f, 1.5f), new Vector3(3.2f, 0.009f, 2.6f),
            new Vector3(4.8f, 0.009f, 1.7f), new Vector3(6.2f, 0.009f, 2.0f), new Vector3(5.4f, 0.009f, 1.8f)
        };
        for (int i = 0; i < puddles.Length; i++)
            Box("PUDDLE_" + i, parent, puddles[i], scales[i], wetHighlight, new Vector3(0f, i * 17f, 0f));

        for (int i = -5; i <= 5; i++)
        {
            float z = -57f + i * 5.8f;
            Box("STREET_REFLECTION_CYAN_" + i, parent, new Vector3(-7.25f, 0.034f, z), new Vector3(0.075f, 0.008f, 2.25f), neonCyan, Vector3.zero);
            Box("STREET_REFLECTION_VIOLET_" + i, parent, new Vector3(7.25f, 0.034f, z + 1.2f), new Vector3(0.075f, 0.008f, 1.6f), neonViolet, Vector3.zero);
        }
    }

    private static void BuildArrivalCanyon(Transform parent)
    {
        BuildFacade("SOUTHWEST_FACADE", parent, new Vector3(-29f, 13f, -47f), new Vector3(24f, 26f, 5f), facadeDark, windowCool, 7, 7);
        BuildFacade("SOUTHEAST_FACADE", parent, new Vector3(29f, 15f, -47f), new Vector3(24f, 30f, 5f), facadeDark, windowWarm, 8, 7);
        BuildFacade("MIDWEST_FACADE", parent, new Vector3(-23f, 10f, -24f), new Vector3(19f, 20f, 4f), facadeLight, windowCool, 6, 5);
        BuildFacade("MIDEAST_FACADE", parent, new Vector3(23f, 11f, -21f), new Vector3(19f, 22f, 4f), facadeDark, windowWarm, 6, 5);

        Box("ARRIVAL_GATE_LEFT", parent, new Vector3(-10.3f, 6.5f, -43.5f), new Vector3(1.3f, 13f, 1.7f), facadeLight, new Vector3(0f, 0f, -8f));
        Box("ARRIVAL_GATE_RIGHT", parent, new Vector3(10.3f, 6.5f, -43.5f), new Vector3(1.3f, 13f, 1.7f), facadeDark, new Vector3(0f, 0f, 8f));
        Box("ARRIVAL_GATE_TOP", parent, new Vector3(0f, 12.4f, -43.5f), new Vector3(22f, 1.15f, 1.7f), facadeDark, Vector3.zero);
        Box("ARRIVAL_GATE_LIGHT", parent, new Vector3(0f, 11.84f, -42.62f), new Vector3(17f, 0.08f, 0.07f), signalWhite, Vector3.zero);
        Box("ARRIVAL_GATE_CORE", parent, new Vector3(0f, 8.1f, -42.58f), new Vector3(4.8f, 1.5f, 0.06f), neonCyan, Vector3.zero);

        for (int i = 0; i < 5; i++)
        {
            float x = -8.4f + i * 4.2f;
            Box("GATE_SIGIL_" + i, parent, new Vector3(x, 8.1f, -42.48f), new Vector3(0.85f, 0.20f, 0.025f), i % 2 == 0 ? neonCyan : neonViolet, new Vector3(0f, 0f, i % 2 == 0 ? 0f : 22f));
        }

        BuildLuminousCanopy(parent, new Vector3(-42f, 3.2f, 9f), neonViolet, "WEST_BIOLUMEN_CANOPY");
        BuildLuminousCanopy(parent, new Vector3(42f, 3.2f, 12f), neonCyan, "EAST_BIOLUMEN_CANOPY");
    }

    private static void BuildElevatedTransit(Transform parent)
    {
        Box("SKYRAIL_MAIN", parent, new Vector3(0f, 13.4f, -12f), new Vector3(52f, 0.72f, 3.6f), facadeDark, Vector3.zero);
        Box("SKYRAIL_UNDERLIGHT", parent, new Vector3(0f, 12.98f, -12f), new Vector3(47f, 0.06f, 0.22f), neonCyan, Vector3.zero);
        Box("SKYRAIL_PLATFORM", parent, new Vector3(0f, 14.0f, -12f), new Vector3(43f, 0.18f, 1.5f), facadeLight, Vector3.zero);

        for (int i = -2; i <= 2; i++)
        {
            float x = i * 11.5f;
            Box("SKYRAIL_PILLAR_" + i, parent, new Vector3(x, 6.5f, -12f), new Vector3(0.8f, 13f, 0.9f), i % 2 == 0 ? facadeLight : facadeDark, Vector3.zero);
            Box("SKYRAIL_PILLAR_SIGNAL_" + i, parent, new Vector3(x, 7.5f, -11.48f), new Vector3(0.10f, 7.4f, 0.04f), i % 2 == 0 ? neonCyan : neonViolet, Vector3.zero);
        }

        for (int i = 0; i < 4; i++)
        {
            float x = -17f + i * 11.3f;
            Box("SKYRAIL_HANGING_SCREEN_" + i, parent, new Vector3(x, 10.7f, -10.0f), new Vector3(4.8f, 2.2f, 0.08f), i % 2 == 0 ? neonCyan : neonViolet, Vector3.zero);
            Box("SKYRAIL_SCREEN_FRAME_" + i, parent, new Vector3(x, 10.7f, -10.12f), new Vector3(5.1f, 2.5f, 0.12f), facadeDark, Vector3.zero);
        }
    }

    private static void BuildDistantCity(Transform parent)
    {
        Vector3[] positions =
        {
            new Vector3(-66f, 35f, 88f), new Vector3(-38f, 46f, 104f), new Vector3(-11f, 34f, 112f),
            new Vector3(19f, 52f, 108f), new Vector3(48f, 39f, 99f), new Vector3(78f, 46f, 86f)
        };
        Vector3[] dimensions =
        {
            new Vector3(19f, 70f, 16f), new Vector3(18f, 92f, 18f), new Vector3(15f, 68f, 14f),
            new Vector3(21f, 104f, 19f), new Vector3(16f, 78f, 16f), new Vector3(19f, 92f, 17f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Material windows = i % 3 == 0 ? windowWarm : windowCool;
            BuildFacade("SKYLINE_TOWER_" + i, parent, positions[i], dimensions[i], i % 2 == 0 ? facadeDark : facadeLight, windows, 11 + i % 3, 5 + i % 2);
            Box("SKYLINE_SPIRE_" + i, parent, positions[i] + new Vector3(0f, dimensions[i].y * 0.56f, 0f), new Vector3(0.9f, 18f + i * 2f, 0.9f), i % 2 == 0 ? neonCyan : neonViolet, Vector3.zero);
        }

        BuildOrbitalLattice(parent);
    }

    private static void BuildOrbitalLattice(Transform parent)
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(36f, 0f, 0f);
            Box("ORBITAL_LATTICE_" + i, parent, new Vector3(offset.x, 63f + Mathf.Sin(angle * Mathf.Deg2Rad) * 7f, 132f + offset.z * 0.25f), new Vector3(7f, 0.22f, 1.2f), i % 3 == 0 ? signalWhite : facadeLight, new Vector3(0f, angle, 0f));
        }
        Box("ORBITAL_LATTICE_CORE", parent, new Vector3(0f, 63f, 132f), new Vector3(4.5f, 4.5f, 0.7f), neonCyan, Vector3.zero);
    }

    private static void BuildSignalGrid(Transform parent)
    {
        for (int i = 0; i < 8; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * (14f + (i % 4) * 7.5f);
            float z = -51f + (i / 2) * 19f;
            Box("DISTRICT_SIGNAL_SPINE_" + i, parent, new Vector3(x, 7.5f, z), new Vector3(0.14f, 8f, 0.12f), i % 3 == 0 ? neonViolet : neonCyan, new Vector3(0f, side * 8f, 0f));
            Box("DISTRICT_SIGNAL_PANEL_" + i, parent, new Vector3(x - side * 0.24f, 8.4f, z), new Vector3(2.2f, 1.3f, 0.08f), i % 2 == 0 ? windowCool : windowWarm, new Vector3(0f, side * 10f, 0f));
        }
    }

    private static void BuildWeather(Transform parent)
    {
        GameObject rain = new GameObject("NEON_RAIN_VOLUME");
        rain.transform.SetParent(parent, false);
        rain.transform.position = new Vector3(0f, 19f, -5f);

        ParticleSystem particleSystem = rain.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = 1.85f;
        main.startSpeed = 0f;
        main.startSize = 0.024f;
        main.maxParticles = 1050;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.54f, 0.74f, 1f, 0.42f);

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 410f;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(105f, 26f, 148f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.35f);
        velocity.y = new ParticleSystem.MinMaxCurve(-16f);
        velocity.z = new ParticleSystem.MinMaxCurve(0.7f);

        ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.StretchedBillboard;
        renderer.lengthScale = 5.2f;
        renderer.velocityScale = 0.16f;
        renderer.sharedMaterial = rainMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static void BuildLuminousCanopy(Transform parent, Vector3 position, Material glow, string name)
    {
        Transform canopy = Group(parent, name);
        Box("TRUNK", canopy, position, new Vector3(0.6f, 6.4f, 0.6f), facadeDark, new Vector3(0f, 0f, 9f));
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            Vector3 branch = Quaternion.Euler(0f, angle, 0f) * new Vector3(2.4f + (i % 3) * 0.35f, 0f, 0f);
            Box("BRANCH_" + i, canopy, position + new Vector3(branch.x * 0.55f, 4.9f + (i % 2) * 0.55f, branch.z * 0.55f), new Vector3(0.18f, 0.18f, 3.4f), facadeDark, new Vector3(0f, angle, 26f));
            Sphere("BLOOM_" + i, canopy, position + new Vector3(branch.x, 6.0f + (i % 3) * 0.25f, branch.z), Vector3.one * (0.62f + (i % 2) * 0.12f), glow);
        }
    }

    private static void BuildFacade(string name, Transform parent, Vector3 position, Vector3 scale, Material body, Material window, int rows, int columns)
    {
        Box(name + "_MASS", parent, position, scale, body, Vector3.zero);
        float cellWidth = scale.x * 0.78f / columns;
        float cellHeight = scale.y * 0.78f / rows;
        float frontZ = position.z - scale.z * 0.505f;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if ((row + column) % 5 == 0) continue;
                float x = position.x + ((column + 0.5f) / columns - 0.5f) * scale.x * 0.82f;
                float y = position.y + ((row + 0.5f) / rows - 0.5f) * scale.y * 0.80f;
                Material selected = (row + column) % 4 == 0 ? signalWhite : window;
                Box(name + "_WINDOW_" + row + "_" + column, parent, new Vector3(x, y, frontZ), new Vector3(cellWidth * 0.64f, cellHeight * 0.48f, 0.035f), selected, Vector3.zero);
            }
        }

        Box(name + "_EDGE_L", parent, position + new Vector3(-scale.x * 0.47f, 0f, -scale.z * 0.52f), new Vector3(0.16f, scale.y * 0.94f, 0.08f), neonCyan, Vector3.zero);
        Box(name + "_EDGE_R", parent, position + new Vector3(scale.x * 0.47f, 0f, -scale.z * 0.52f), new Vector3(0.16f, scale.y * 0.94f, 0.08f), neonViolet, Vector3.zero);
    }

    private static void ApplyCinematicLighting(Transform parent)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.19f, 0.27f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.12f, 0.18f, 0.28f);
        RenderSettings.fogStartDistance = 72f;
        RenderSettings.fogEndDistance = 235f;

        foreach (Light light in UnityEngine.Object.FindObjectsOfType<Light>(true))
        {
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(0.68f, 0.78f, 1f);
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.82f;
            }
        }

        CreatePointLight(parent, "ARRIVAL_KEY", new Vector3(0f, 8f, -46f), new Color(0.33f, 0.65f, 1f), 2.1f, 34f);
        CreatePointLight(parent, "WEST_NEON_BOUNCE", new Vector3(-20f, 7f, -33f), new Color(0.16f, 0.54f, 1f), 2.7f, 30f);
        CreatePointLight(parent, "EAST_NEON_BOUNCE", new Vector3(20f, 8f, -24f), new Color(0.66f, 0.22f, 0.90f), 2.4f, 30f);
        CreatePointLight(parent, "MID_CITY_KEY", new Vector3(0f, 11f, -12f), new Color(0.72f, 0.86f, 1f), 1.6f, 46f);
    }

    private static void CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    private static bool EnsureMaterials()
    {
        EnsureFolder();
        wetStreet = MaterialAsset("M_Kurogake_WetStreet", new Color(0.028f, 0.055f, 0.085f), 0.82f, 0.64f, new Color(0.015f, 0.055f, 0.12f));
        wetHighlight = MaterialAsset("M_Kurogake_WetHighlight", new Color(0.09f, 0.18f, 0.29f), 0.94f, 0.38f, new Color(0.04f, 0.16f, 0.34f));
        facadeDark = MaterialAsset("M_Kurogake_FacadeDark", new Color(0.028f, 0.04f, 0.065f), 0.46f, 0.68f, Color.black);
        facadeLight = MaterialAsset("M_Kurogake_FacadeLight", new Color(0.20f, 0.26f, 0.34f), 0.62f, 0.28f, Color.black);
        windowCool = MaterialAsset("M_Kurogake_WindowCool", new Color(0.07f, 0.33f, 0.68f), 0.38f, 0.12f, new Color(0.12f, 0.52f, 1f) * 2.1f);
        windowWarm = MaterialAsset("M_Kurogake_WindowWarm", new Color(0.70f, 0.32f, 0.20f), 0.35f, 0.10f, new Color(1f, 0.32f, 0.14f) * 1.55f);
        neonCyan = MaterialAsset("M_Kurogake_NeonCyan", new Color(0.12f, 0.66f, 1f), 0.46f, 0.12f, new Color(0.10f, 0.67f, 1f) * 2.8f);
        neonViolet = MaterialAsset("M_Kurogake_NeonViolet", new Color(0.55f, 0.27f, 0.95f), 0.45f, 0.12f, new Color(0.55f, 0.24f, 1f) * 2.05f);
        signalWhite = MaterialAsset("M_Kurogake_SignalWhite", new Color(0.78f, 0.90f, 1f), 0.48f, 0.10f, new Color(0.58f, 0.78f, 1f) * 2.2f);
        rainMaterial = ParticleMaterialAsset("M_Kurogake_RainParticle");

        return wetStreet != null && wetHighlight != null && facadeDark != null && facadeLight != null &&
               windowCool != null && windowWarm != null && neonCyan != null && neonViolet != null &&
               signalWhite != null && rainMaterial != null;
    }

    private static Material MaterialAsset(string name, Color color, float smoothness, float metallic, Color emission)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = ResolveLitShader();
        if (shader == null) return null;

        if (material == null)
        {
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0.001f) material.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material ParticleMaterialAsset(string name)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = ResolveParticleShader();
        if (shader == null) return null;

        if (material == null)
        {
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_Color")) material.SetColor("_Color", new Color(0.58f, 0.76f, 1f, 0.36f));
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(0.58f, 0.76f, 1f, 0.36f));
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader ResolveLitShader()
    {
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        string typeName = pipeline != null ? pipeline.GetType().FullName : string.Empty;
        if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null && shader.isSupported) return shader;
        }
        if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("HDRenderPipeline", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader != null && shader.isSupported) return shader;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null && standard.isSupported) return standard;
        Shader diffuse = Shader.Find("Legacy Shaders/Diffuse");
        if (diffuse != null && diffuse.isSupported) return diffuse;
        return Shader.Find("Unlit/Color");
    }

    private static Shader ResolveParticleShader()
    {
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        string typeName = pipeline != null ? pipeline.GetType().FullName : string.Empty;
        if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null && shader.isSupported) return shader;
        }

        Shader particle = Shader.Find("Particles/Standard Unlit");
        if (particle != null && particle.isSupported) return particle;
        return Shader.Find("Sprites/Default");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage/Art", "GeneratedMaterials");
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 rotation)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        gameObject.transform.eulerAngles = rotation;
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        return gameObject;
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        return gameObject;
    }
}
