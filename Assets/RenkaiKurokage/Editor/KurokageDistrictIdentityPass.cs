using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KurokageDistrictIdentityPass
{
    private const string RootName = "KUROKAGE_DISTRICT_IDENTITY";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    private static Material light;
    private static Material dark;
    private static Material navy;
    private static Material cover;
    private static Material blue;
    private static Material violet;
    private static Material hologram;
    private static Material glass;

    public static bool ApplySilent()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        if (!LoadMaterials()) return false;

        GameObject root = new GameObject(RootName);
        BuildShibuyaIdentity(Group(root.transform, "SHIBUYA_ZERO_IDENTITY"));
        BuildArchiveIdentity(Group(root.transform, "CELESTIAL_ARCHIVE_IDENTITY"));
        BuildReactorIdentity(Group(root.transform, "VOID_REACTOR_IDENTITY"));
        BuildGhostIdentity(Group(root.transform, "GHOST_LINE_IDENTITY"));

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static bool LoadMaterials()
    {
        light = Load("M_LightComposite");
        dark = Load("M_DarkCeramic");
        navy = Load("M_NavyMetal");
        cover = Load("M_CoverNeutral");
        blue = Load("M_Accent_Blue");
        violet = Load("M_Accent_Violet");
        hologram = Load("M_Hologram");
        glass = Load("M_Glass_Subtle");
        return light != null && dark != null && navy != null && cover != null && blue != null && violet != null && hologram != null;
    }

    private static void BuildShibuyaIdentity(Transform parent)
    {
        // Clean crossing language: bright curb islands, dark framing, suspended transit elements.
        for (int i = -3; i <= 3; i++)
        {
            float z = -50f + i * 12f;
            Slab("ZERO_CROSSING_L_" + i, parent, new Vector3(-3.3f, 0.035f, z), new Vector3(4.5f, 0.025f, 0.32f), light);
            Slab("ZERO_CROSSING_R_" + i, parent, new Vector3(3.3f, 0.035f, z), new Vector3(4.5f, 0.025f, 0.32f), light);
        }

        for (int i = 0; i < 4; i++)
        {
            float z = -42f + i * 22f;
            Pillar("ZERO_TRANSIT_PYLON_L_" + i, parent, new Vector3(-15.6f, 5.2f, z), new Vector3(0.72f, 10.4f, 0.88f), i % 2 == 0 ? light : navy);
            Pillar("ZERO_TRANSIT_PYLON_R_" + i, parent, new Vector3(15.6f, 5.2f, z), new Vector3(0.72f, 10.4f, 0.88f), i % 2 == 0 ? light : navy);
            Beam("ZERO_TRANSIT_SPAN_" + i, parent, new Vector3(0f, 9.4f, z), new Vector3(30f, 0.42f, 0.76f), navy);
            Slab("ZERO_TRANSIT_SIGNAL_" + i, parent, new Vector3(0f, 8.95f, z - 0.42f), new Vector3(6.4f, 0.08f, 0.06f), blue);
        }

        BuildFramedSign("ZERO_GATE_NAV", parent, new Vector3(0f, 6.4f, 12f), new Vector3(8f, 1.3f, 0.08f), hologram, navy);
    }

    private static void BuildArchiveIdentity(Transform parent)
    {
        Vector3 center = new Vector3(-34f, 0f, 17f);

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            Vector3 radial = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 10.2f);
            Pillar("ARCHIVE_MEMORY_COLUMN_" + i, parent, center + radial + Vector3.up * 4.4f, new Vector3(0.92f, 8.8f, 0.92f), light);
            Slab("ARCHIVE_DATA_SPINE_" + i, parent, center + radial + Vector3.up * 4.7f, new Vector3(0.12f, 5.7f, 1.05f), blue, new Vector3(0f, angle, 0f));
        }

        for (int i = 0; i < 5; i++)
        {
            float y = 3.1f + i * 1.05f;
            float z = 27.8f + i * 0.11f;
            Slab("ARCHIVE_FLOATING_MEMORY_" + i, parent, new Vector3(-34f, y, z), new Vector3(8.5f - i * 0.72f, 0.08f, 0.34f), i % 2 == 0 ? hologram : blue, new Vector3(0f, i * 6f, 0f));
        }

        BuildFramedSign("ARCHIVE_NAV_A", parent, new Vector3(-34f, 6.2f, 29.2f), new Vector3(10f, 1.45f, 0.10f), hologram, light);
    }

    private static void BuildReactorIdentity(Transform parent)
    {
        Vector3 center = new Vector3(34f, 0f, 17f);

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            Vector3 radial = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 9.6f);
            Pillar("REACTOR_STABILIZER_" + i, parent, center + radial + Vector3.up * 3.8f, new Vector3(1.05f, 7.6f, 1.05f), i % 2 == 0 ? navy : dark);
            Slab("REACTOR_ENERGY_CHANNEL_" + i, parent, center + radial + Vector3.up * 4.0f, new Vector3(0.16f, 4.9f, 0.14f), i == 0 || i == 3 ? violet : blue);
        }

        Beam("REACTOR_SERVICE_BRIDGE", parent, center + new Vector3(0f, 7.6f, -7.8f), new Vector3(22f, 0.42f, 1.0f), navy);
        Beam("REACTOR_SERVICE_LIGHT", parent, center + new Vector3(0f, 7.2f, -7.8f), new Vector3(13f, 0.06f, 0.18f), violet);

        for (int i = -2; i <= 2; i++)
        {
            float x = center.x + i * 4.1f;
            Slab("REACTOR_BACKPLATE_" + i, parent, new Vector3(x, 4.8f, 30.1f), new Vector3(3.5f, 7.5f, 0.16f), i % 2 == 0 ? navy : cover);
        }

        BuildFramedSign("REACTOR_NAV_B", parent, new Vector3(34f, 6.3f, 29.7f), new Vector3(10f, 1.45f, 0.10f), violet, navy);
    }

    private static void BuildGhostIdentity(Transform parent)
    {
        Vector3 origin = new Vector3(0f, -4.9f, 26f);

        for (int i = -4; i <= 4; i++)
        {
            float z = origin.z + i * 5.2f;
            Beam("GHOST_SIGNAL_BEAM_" + i, parent, new Vector3(0f, -0.7f, z), new Vector3(18f, 0.22f, 0.28f), i % 2 == 0 ? light : navy);
            Slab("GHOST_ROUTE_MARK_" + i, parent, new Vector3(-6.9f, -4.35f, z), new Vector3(0.22f, 0.06f, 2.2f), blue);
        }

        for (int i = 0; i < 3; i++)
        {
            float x = -8f + i * 8f;
            Pillar("GHOST_PLATFORM_STRUCTURE_" + i, parent, new Vector3(x, -2.1f, 26f), new Vector3(0.65f, 5.6f, 0.65f), dark);
        }

        BuildFramedSign("GHOST_PLATFORM_09_NAV", parent, new Vector3(-7.2f, -1.4f, 26f), new Vector3(0.10f, 2.8f, 6f), hologram, navy, new Vector3(0f, 90f, 0f));
    }

    private static void BuildFramedSign(string name, Transform parent, Vector3 position, Vector3 scale, Material face, Material frame, Vector3? euler = null)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, true);
        root.transform.position = position;
        root.transform.eulerAngles = euler ?? Vector3.zero;

        Slab(name + "_FACE", root.transform, position, scale, face, euler);
        Vector3 border = scale + new Vector3(0.22f, 0.22f, 0.10f);
        Slab(name + "_FRAME", root.transform, position + Vector3.forward * 0.06f, border, frame, euler);
        root.transform.DetachChildren();
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
        {
            if (go.name == name + "_FACE" || go.name == name + "_FRAME")
                go.transform.SetParent(root.transform, true);
        }
    }

    private static Material Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + name + ".mat");
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject Pillar(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? euler = null)
    {
        return Primitive(name, parent, position, scale, material, euler);
    }

    private static GameObject Beam(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? euler = null)
    {
        return Primitive(name, parent, position, scale, material, euler);
    }

    private static GameObject Slab(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? euler = null)
    {
        return Primitive(name, parent, position, scale, material, euler);
    }

    private static GameObject Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? euler)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.transform.eulerAngles = euler ?? Vector3.zero;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }
}
