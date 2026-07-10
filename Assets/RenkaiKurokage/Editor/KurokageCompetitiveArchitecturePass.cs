using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurokage;

public static class KurokageCompetitiveArchitecturePass
{
    private const string RootName = "KUROKAGE_COMPETITIVE_ARCHITECTURE";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    private static Material lightComposite;
    private static Material darkCeramic;
    private static Material navyMetal;
    private static Material coverNeutral;
    private static Material blueAccent;
    private static Material violetAccent;
    private static Material hologram;

    public static bool ApplySilent()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        if (!LoadMaterials()) return false;

        GameObject root = new GameObject(RootName);
        Transform cladding = Group(root.transform, "ARCHITECTURE_CLADDING");
        Transform trims = Group(root.transform, "TACTICAL_LIGHT_TRIMS");
        Transform overhead = Group(root.transform, "OVERHEAD_DEPTH");
        Transform signage = Group(root.transform, "WAYFINDING_FRAMES");

        ApplyUnifiedSurfaceLanguage();
        BuildLaneCladding(cladding);
        BuildTacticalTrims(trims);
        BuildOverheadDepth(overhead);
        BuildWayfinding(signage);
        root.AddComponent<KurokageArchitecturalResonancePresenter>();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static bool LoadMaterials()
    {
        lightComposite = Load("M_LightComposite");
        darkCeramic = Load("M_DarkCeramic");
        navyMetal = Load("M_NavyMetal");
        coverNeutral = Load("M_CoverNeutral");
        blueAccent = Load("M_Accent_Blue");
        violetAccent = Load("M_Accent_Violet");
        hologram = Load("M_Hologram");
        return lightComposite != null && darkCeramic != null && navyMetal != null &&
               coverNeutral != null && blueAccent != null && violetAccent != null && hologram != null;
    }

    private static void ApplyUnifiedSurfaceLanguage()
    {
        GameObject map = GameObject.Find("MAP");
        if (map == null) return;

        foreach (Renderer renderer in map.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null) continue;
            string n = renderer.gameObject.name.ToLowerInvariant();

            if (n.Contains("floor")) renderer.sharedMaterial = coverNeutral;
            else if (n.Contains("a_main_left") || n.Contains("b_main_right") || n.Contains("mid_left") || n.Contains("mid_right")) renderer.sharedMaterial = lightComposite;
            else if (n.Contains("wall") || n.Contains("site_back")) renderer.sharedMaterial = darkCeramic;
            else if (n.Contains("cover") || n.Contains("box")) renderer.sharedMaterial = coverNeutral;
            else if (n.Contains("a_route")) renderer.sharedMaterial = blueAccent;
            else if (n.Contains("b_route")) renderer.sharedMaterial = violetAccent;
        }
    }

    private static void BuildLaneCladding(Transform parent)
    {
        Panel("A_CLADDING_01", parent, new Vector3(-43.55f, 3.6f, -28f), new Vector3(0.12f, 4.8f, 11f), lightComposite);
        Panel("A_CLADDING_02", parent, new Vector3(-24.45f, 3.2f, -31f), new Vector3(0.12f, 3.8f, 8.5f), darkCeramic);
        Panel("A_CLADDING_BLUE", parent, new Vector3(-43.42f, 3.7f, -28f), new Vector3(0.055f, 2.9f, 6.5f), blueAccent);

        Panel("B_CLADDING_01", parent, new Vector3(24.45f, 3.2f, -31f), new Vector3(0.12f, 3.8f, 8.5f), navyMetal);
        Panel("B_CLADDING_02", parent, new Vector3(43.55f, 3.6f, -28f), new Vector3(0.12f, 4.8f, 11f), lightComposite);
        Panel("B_CLADDING_VIOLET", parent, new Vector3(24.58f, 3.4f, -31f), new Vector3(0.055f, 2.4f, 5.2f), violetAccent);

        Panel("MID_LIGHT_FRAME_L", parent, new Vector3(-10.55f, 3.45f, -18f), new Vector3(0.10f, 4.4f, 8.0f), lightComposite);
        Panel("MID_LIGHT_FRAME_R", parent, new Vector3(10.55f, 3.45f, -18f), new Vector3(0.10f, 4.4f, 8.0f), lightComposite);
        Panel("MID_DARK_SPINE_L", parent, new Vector3(-10.40f, 4.0f, -18f), new Vector3(0.055f, 2.2f, 5.4f), navyMetal);
        Panel("MID_DARK_SPINE_R", parent, new Vector3(10.40f, 4.0f, -18f), new Vector3(0.055f, 2.2f, 5.4f), navyMetal);
    }

    private static void BuildTacticalTrims(Transform parent)
    {
        Strip("MID_GUIDE_L", parent, new Vector3(-6.6f, 0.035f, -8f), new Vector3(0.10f, 0.025f, 36f), blueAccent);
        Strip("MID_GUIDE_R", parent, new Vector3(6.6f, 0.035f, -8f), new Vector3(0.10f, 0.025f, 36f), blueAccent);

        Strip("A_SITE_APPROACH", parent, new Vector3(-34f, 0.04f, 4f), new Vector3(0.12f, 0.025f, 22f), blueAccent);
        Strip("B_SITE_APPROACH", parent, new Vector3(34f, 0.04f, 4f), new Vector3(0.12f, 0.025f, 22f), violetAccent);

        for (int i = -2; i <= 2; i++)
        {
            float z = -48f + i * 12f;
            Strip("MID_CROSS_MARK_" + i, parent, new Vector3(0f, 0.04f, z), new Vector3(6.2f, 0.02f, 0.08f), i == 0 ? blueAccent : lightComposite);
        }
    }

    private static void BuildOverheadDepth(Transform parent)
    {
        Beam("MID_OVERHEAD_01", parent, new Vector3(0f, 8.2f, -28f), new Vector3(24f, 0.42f, 0.68f), navyMetal);
        Beam("MID_OVERHEAD_02", parent, new Vector3(0f, 10.4f, -2f), new Vector3(22f, 0.34f, 0.58f), lightComposite);
        Beam("A_OVERHEAD_ARCH", parent, new Vector3(-34f, 8.8f, -8f), new Vector3(17f, 0.50f, 0.74f), lightComposite);
        Beam("B_OVERHEAD_ARCH", parent, new Vector3(34f, 8.8f, -8f), new Vector3(17f, 0.50f, 0.74f), navyMetal);

        for (int i = 0; i < 5; i++)
        {
            float z = -44f + i * 18f;
            Beam("MID_VERTICAL_DEPTH_L_" + i, parent, new Vector3(-13.2f, 6.3f, z), new Vector3(0.45f, 6.4f, 0.45f), i % 2 == 0 ? lightComposite : darkCeramic);
            Beam("MID_VERTICAL_DEPTH_R_" + i, parent, new Vector3(13.2f, 6.3f, z), new Vector3(0.45f, 6.4f, 0.45f), i % 2 == 0 ? lightComposite : darkCeramic);
        }
    }

    private static void BuildWayfinding(Transform parent)
    {
        WayfindingFrame("WAYFINDING_A", parent, new Vector3(-30f, 5.8f, -8f), new Vector3(5.2f, 1.5f, 0.10f), blueAccent);
        WayfindingFrame("WAYFINDING_B", parent, new Vector3(30f, 5.8f, -8f), new Vector3(5.2f, 1.5f, 0.10f), violetAccent);
        WayfindingFrame("WAYFINDING_MID", parent, new Vector3(0f, 7.1f, 5f), new Vector3(7.2f, 1.2f, 0.10f), hologram);
    }

    private static void WayfindingFrame(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject frame = Panel(name, parent, position, scale, material);
        frame.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
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

    private static GameObject Panel(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        return Primitive(name, parent, PrimitiveType.Cube, position, scale, material);
    }

    private static GameObject Strip(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        return Primitive(name, parent, PrimitiveType.Cube, position, scale, material);
    }

    private static GameObject Beam(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        return Primitive(name, parent, PrimitiveType.Cube, position, scale, material);
    }

    private static GameObject Primitive(string name, Transform parent, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }
}
