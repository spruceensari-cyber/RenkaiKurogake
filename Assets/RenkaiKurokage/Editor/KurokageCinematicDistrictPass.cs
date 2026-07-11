using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds a non-colliding cinematic presentation layer for the competitive map.
/// Compatible with Unity 2022.3 built-in and URP projects.
/// </summary>
public static class KurokageCinematicDistrictPass
{
    private const string RootName = "KUROKAGE_CINEMATIC_PRESENTATION";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials";

    private static Material wetStreet;
    private static Material facadeDark;
    private static Material facadeLight;
    private static Material windowCool;
    private static Material neonCyan;
    private static Material neonViolet;
    private static Material signalWhite;
    private static Material rainMaterial;

    public static bool ApplySilent()
    {
        GameObject previous = GameObject.Find(RootName);
        if (previous != null) Object.DestroyImmediate(previous);
        if (!EnsureMaterials()) return false;

        GameObject root = new GameObject(RootName);
        BuildStreet(Group(root.transform, "WET_STREET"));
        BuildArrival(Group(root.transform, "ARRIVAL_CANYON"));
        BuildTransit(Group(root.transform, "ELEVATED_TRANSIT"));
        BuildSkyline(Group(root.transform, "DISTANT_CITY"));
        BuildWeather(Group(root.transform, "WEATHER"));
        BuildLighting(root.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        return true;
    }

    private static void BuildStreet(Transform parent)
    {
        Box("RAIN_POLISHED_STREET", parent, new Vector3(0f, 0.012f, -1f), new Vector3(106f, 0.024f, 146f), wetStreet);
        for (int i = -5; i <= 5; i++)
        {
            float z = -57f + i * 5.8f;
            Box("STREET_REFLECTION_CYAN_" + i, parent, new Vector3(-7.25f, 0.034f, z), new Vector3(0.075f, 0.008f, 2.25f), neonCyan);
            Box("STREET_REFLECTION_VIOLET_" + i, parent, new Vector3(7.25f, 0.034f, z + 1.2f), new Vector3(0.075f, 0.008f, 1.6f), neonViolet);
        }
    }

    private static void BuildArrival(Transform parent)
    {
        Facade("SOUTHWEST_FACADE", parent, new Vector3(-29f, 13f, -47f), new Vector3(24f, 26f, 5f), facadeDark, 7, 7);
        Facade("SOUTHEAST_FACADE", parent, new Vector3(29f, 15f, -47f), new Vector3(24f, 30f, 5f), facadeDark, 8, 7);
        Facade("MIDWEST_FACADE", parent, new Vector3(-23f, 10f, -24f), new Vector3(19f, 20f, 4f), facadeLight, 6, 5);
        Facade("MIDEAST_FACADE", parent, new Vector3(23f, 11f, -21f), new Vector3(19f, 22f, 4f), facadeDark, 6, 5);

        Box("ARRIVAL_GATE_LEFT", parent, new Vector3(-10.3f, 6.5f, -43.5f), new Vector3(1.3f, 13f, 1.7f), facadeLight);
        Box("ARRIVAL_GATE_RIGHT", parent, new Vector3(10.3f, 6.5f, -43.5f), new Vector3(1.3f, 13f, 1.7f), facadeDark);
        Box("ARRIVAL_GATE_TOP", parent, new Vector3(0f, 12.4f, -43.5f), new Vector3(22f, 1.15f, 1.7f), facadeDark);
        Box("ARRIVAL_GATE_CORE", parent, new Vector3(0f, 8.1f, -42.58f), new Vector3(4.8f, 1.5f, 0.06f), neonCyan);
    }

    private static void BuildTransit(Transform parent)
    {
        Box("SKYRAIL_MAIN", parent, new Vector3(0f, 13.4f, -12f), new Vector3(52f, 0.72f, 3.6f), facadeDark);
        Box("SKYRAIL_UNDERLIGHT", parent, new Vector3(0f, 12.98f, -12f), new Vector3(47f, 0.06f, 0.22f), neonCyan);
        Box("SKYRAIL_PLATFORM", parent, new Vector3(0f, 14f, -12f), new Vector3(43f, 0.18f, 1.5f), facadeLight);

        for (int i = -2; i <= 2; i++)
        {
            float x = i * 11.5f;
            Box("SKYRAIL_PILLAR_" + i, parent, new Vector3(x, 6.5f, -12f), new Vector3(0.8f, 13f, 0.9f), i % 2 == 0 ? facadeLight : facadeDark);
            Box("SKYRAIL_SIGNAL_" + i, parent, new Vector3(x, 7.5f, -11.48f), new Vector3(0.10f, 7.4f, 0.04f), i % 2 == 0 ? neonCyan : neonViolet);
        }
    }

    private static void BuildSkyline(Transform parent)
    {
        Vector3[] positions =
        {
            new Vector3(-66f, 35f, 88f), new Vector3(-38f, 46f, 104f), new Vector3(-11f, 34f, 112f),
            new Vector3(19f, 52f, 108f), new Vector3(48f, 39f, 99f), new Vector3(78f, 46f, 86f)
        };
        Vector3[] scales =
        {
            new Vector3(19f, 70f, 16f), new Vector3(18f, 92f, 18f), new Vector3(15f, 68f, 14f),
            new Vector3(21f, 104f, 19f), new Vector3(16f, 78f, 16f), new Vector3(19f, 92f, 17f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Facade("SKYLINE_TOWER_" + i, parent, positions[i], scales[i], i % 2 == 0 ? facadeDark : facadeLight, 10, 5);
            Box("SKYLINE_SPIRE_" + i, parent, positions[i] + Vector3.up * (scales[i].y * 0.56f), new Vector3(0.9f, 18f + i * 2f, 0.9f), i % 2 == 0 ? neonCyan : neonViolet);
        }

        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(36f, 0f, 0f);
            Box("ORBITAL_LATTICE_" + i, parent, new Vector3(offset.x, 63f, 132f + offset.z * 0.25f), new Vector3(7f, 0.22f, 1.2f), i % 3 == 0 ? signalWhite : facadeLight, new Vector3(0f, angle, 0f));
        }
    }

    private static void BuildWeather(Transform parent)
    {
        GameObject rain = new GameObject("NEON_RAIN_VOLUME");
        rain.transform.SetParent(parent, false);
        rain.transform.position = new Vector3(0f, 19f, -5f);

        ParticleSystem particles = rain.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = 1.85f;
        main.startSpeed = 0f;
        main.startSize = 0.024f;
        main.maxParticles = 1050;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.54f, 0.74f, 1f, 0.42f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 410f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(105f, 26f, 148f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.35f);
        velocity.y = new ParticleSystem.MinMaxCurve(-16f);
        velocity.z = new ParticleSystem.MinMaxCurve(0.7f);

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 5.2f;
        renderer.velocityScale = 0.16f;
        renderer.sharedMaterial = rainMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static void BuildLighting(Transform parent)
    {
        GameObject key = new GameObject("CINEMATIC_DISTRICT_KEY");
        key.transform.SetParent(parent, false);
        key.transform.position = new Vector3(0f, 22f, -24f);
        key.transform.rotation = Quaternion.Euler(52f, -24f, 0f);
        Light light = key.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.68f, 0.78f, 1f);
        light.intensity = 0.34f;
        light.shadows = LightShadows.Soft;
    }

    private static void Facade(string name, Transform parent, Vector3 position, Vector3 scale, Material material, int rows, int columns)
    {
        Box(name + "_MASS", parent, position, scale, material);
        float xStep = scale.x * 0.75f / Mathf.Max(1, columns - 1);
        float yStep = scale.y * 0.75f / Mathf.Max(1, rows - 1);
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 p = position + new Vector3(-scale.x * 0.375f + column * xStep, -scale.y * 0.375f + row * yStep, -scale.z * 0.505f);
                Box(name + "_WINDOW_" + row + "_" + column, parent, p, new Vector3(xStep * 0.48f, yStep * 0.28f, 0.025f), windowCool);
            }
        }
    }

    private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? euler = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.transform.eulerAngles = euler ?? Vector3.zero;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static bool EnsureMaterials()
    {
        wetStreet = Load("M_Floor_Competitive");
        facadeDark = Load("M_DarkCeramic");
        facadeLight = Load("M_LightComposite");
        windowCool = Load("M_Glass_Subtle");
        neonCyan = Load("M_Accent_Blue");
        neonViolet = Load("M_Accent_Violet");
        signalWhite = Load("M_Energy_Core");
        rainMaterial = Load("M_Hologram");
        return wetStreet != null && facadeDark != null && facadeLight != null && windowCool != null && neonCyan != null && neonViolet != null && signalWhite != null && rainMaterial != null;
    }

    private static Material Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + name + ".mat");
    }
}
