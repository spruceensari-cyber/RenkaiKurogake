using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class KurokageKurogateDistrictPass
{
    private const string RootName = "KUROKAGE_KUROGATE_DISTRICT";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    private static Material dark;
    private static Material light;
    private static Material navy;
    private static Material floor;
    private static Material blue;
    private static Material violet;
    private static Material glass;

    public static bool ApplySilent()
    {
        if (!LoadMaterials()) return false;

        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject root = new GameObject(RootName);
        BuildMidGate(root.transform);
        BuildSiteGate(root.transform, "A_GATE", new Vector3(-42f, 0f, 15f), Quaternion.Euler(0f, 90f, 0f), blue, "A // CELESTIAL");
        BuildSiteGate(root.transform, "B_GATE", new Vector3(42f, 0f, 15f), Quaternion.Euler(0f, -90f, 0f), violet, "B // VOID");
        BuildRoofline(root.transform, new Vector3(-29f, 6.8f, 5f), new Vector3(18f, 1f, 8f), 1f);
        BuildRoofline(root.transform, new Vector3(29f, 6.8f, 5f), new Vector3(18f, 1f, 8f), -1f);
        BuildLanternRoute(root.transform, new Vector3(-10f, 0f, -37f), Vector3.forward, 7, 7.5f, blue);
        BuildLanternRoute(root.transform, new Vector3(10f, 0f, -37f), Vector3.forward, 7, 7.5f, violet);
        BuildArchiveFacade(root.transform);
        BuildVoidFacade(root.transform);
        BuildFloorLanguage(root.transform);
        BuildDistantShrineSilhouette(root.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        return true;
    }

    private static void BuildMidGate(Transform parent)
    {
        Transform group = Group(parent, "MID_KUROGATE");
        Vector3 center = new Vector3(0f, 0f, -5f);
        Box("MID_PILLAR_L", group, center + new Vector3(-5.2f, 3.4f, 0f), new Vector3(0.72f, 6.8f, 0.82f), dark);
        Box("MID_PILLAR_R", group, center + new Vector3(5.2f, 3.4f, 0f), new Vector3(0.72f, 6.8f, 0.82f), dark);
        Box("MID_BEAM_MAIN", group, center + new Vector3(0f, 6.35f, 0f), new Vector3(12.4f, 0.62f, 1.05f), navy);
        Box("MID_BEAM_CROWN", group, center + new Vector3(0f, 7.2f, 0f), new Vector3(14.2f, 0.28f, 1.28f), light);
        Box("MID_LIGHT_L", group, center + new Vector3(-4.55f, 3.65f, -0.45f), new Vector3(0.06f, 5.1f, 0.05f), blue);
        Box("MID_LIGHT_R", group, center + new Vector3(4.55f, 3.65f, -0.45f), new Vector3(0.06f, 5.1f, 0.05f), violet);
        Sign("MID_SIGN", group, center + new Vector3(0f, 5.15f, -0.62f), "KUROGATE // MID", 0.34f, Color.white);
        BuildGeometricCrest(group, center + new Vector3(0f, 3.55f, -0.64f), blue);
    }

    private static void BuildSiteGate(Transform parent, string name, Vector3 position, Quaternion rotation, Material accent, string label)
    {
        Transform group = Group(parent, name);
        group.position = position;
        group.rotation = rotation;

        Box(name + "_PILLAR_L", group, new Vector3(-4.2f, 3f, 0f), new Vector3(0.64f, 6f, 0.72f), dark, true);
        Box(name + "_PILLAR_R", group, new Vector3(4.2f, 3f, 0f), new Vector3(0.64f, 6f, 0.72f), dark, true);
        Box(name + "_BEAM", group, new Vector3(0f, 5.65f, 0f), new Vector3(10.2f, 0.56f, 0.92f), navy, true);
        Box(name + "_TOP", group, new Vector3(0f, 6.36f, 0f), new Vector3(11.8f, 0.24f, 1.16f), light, true);
        Box(name + "_ACCENT", group, new Vector3(0f, 5.18f, -0.5f), new Vector3(7.2f, 0.055f, 0.035f), accent, true);
        Sign(name + "_SIGN", group, new Vector3(0f, 4.55f, -0.56f), label, 0.28f, Color.white, true);
    }

    private static void BuildRoofline(Transform parent, Vector3 center, Vector3 scale, float direction)
    {
        Transform group = Group(parent, direction > 0f ? "WEST_ROOFLINE" : "EAST_ROOFLINE");
        Box("ROOF_BODY", group, center, scale, dark);
        Box("ROOF_EDGE_FRONT", group, center + new Vector3(0f, 0.75f, -scale.z * 0.48f), new Vector3(scale.x + 1.2f, 0.18f, 0.55f), navy, false, new Vector3(direction * 7f, 0f, 0f));
        Box("ROOF_EDGE_BACK", group, center + new Vector3(0f, 0.75f, scale.z * 0.48f), new Vector3(scale.x + 1.2f, 0.18f, 0.55f), navy, false, new Vector3(-direction * 7f, 0f, 0f));
        Box("ROOF_RIDGE", group, center + new Vector3(0f, 1.15f, 0f), new Vector3(scale.x * 0.84f, 0.18f, 0.34f), light);
        for (int i = -4; i <= 4; i++)
            Box("ROOF_RIB_" + i, group, center + new Vector3(i * 1.8f, 0.82f, 0f), new Vector3(0.08f, 0.10f, scale.z + 0.8f), i % 2 == 0 ? blue : violet);
    }

    private static void BuildLanternRoute(Transform parent, Vector3 start, Vector3 direction, int count, float spacing, Material accent)
    {
        Transform group = Group(parent, accent == blue ? "BLUE_LANTERN_ROUTE" : "VIOLET_LANTERN_ROUTE");
        for (int i = 0; i < count; i++)
        {
            Vector3 position = start + direction * spacing * i;
            Cylinder("LANTERN_POST_" + i, group, position + Vector3.up * 1.4f, new Vector3(0.07f, 1.4f, 0.07f), dark);
            Box("LANTERN_FRAME_" + i, group, position + Vector3.up * 2.55f, new Vector3(0.46f, 0.76f, 0.46f), navy);
            Box("LANTERN_LIGHT_" + i, group, position + Vector3.up * 2.55f, new Vector3(0.34f, 0.62f, 0.34f), accent);
            Box("LANTERN_CAP_" + i, group, position + Vector3.up * 3.0f, new Vector3(0.68f, 0.12f, 0.68f), light);
        }
    }

    private static void BuildArchiveFacade(Transform parent)
    {
        Transform group = Group(parent, "CELESTIAL_ARCHIVE_FACADE");
        Vector3 center = new Vector3(-46f, 5.5f, 18f);
        Box("ARCHIVE_WALL", group, center, new Vector3(1.2f, 11f, 19f), light);
        for (int i = -4; i <= 4; i++)
            Box("ARCHIVE_DATA_BAND_" + i, group, center + new Vector3(-0.66f, i * 1.05f, 0f), new Vector3(0.035f, 0.08f, 16f), blue);
        Sign("ARCHIVE_TITLE", group, center + new Vector3(-0.74f, 2.4f, 0f), "CELESTIAL ARCHIVE", 0.32f, new Color(0.20f, 0.52f, 0.92f), false, new Vector3(0f, -90f, 0f));
    }

    private static void BuildVoidFacade(Transform parent)
    {
        Transform group = Group(parent, "VOID_REACTOR_FACADE");
        Vector3 center = new Vector3(46f, 5.5f, 18f);
        Box("VOID_WALL", group, center, new Vector3(1.2f, 11f, 19f), dark);
        for (int i = -3; i <= 3; i++)
            Box("VOID_CONDUIT_" + i, group, center + new Vector3(0.66f, i * 1.25f, 0f), new Vector3(0.035f, 0.12f, 15.5f), i % 2 == 0 ? violet : blue);
        Sign("VOID_TITLE", group, center + new Vector3(0.74f, 2.4f, 0f), "VOID REACTOR", 0.32f, new Color(0.72f, 0.34f, 1f), false, new Vector3(0f, 90f, 0f));
    }

    private static void BuildFloorLanguage(Transform parent)
    {
        Transform group = Group(parent, "KUROGATE_FLOOR_LANGUAGE");
        Box("MID_SPINE", group, new Vector3(0f, 0.035f, -8f), new Vector3(0.08f, 0.012f, 72f), blue);
        Box("A_ROUTE", group, new Vector3(-22f, 0.035f, 5f), new Vector3(42f, 0.012f, 0.08f), blue);
        Box("B_ROUTE", group, new Vector3(22f, 0.035f, 5f), new Vector3(42f, 0.012f, 0.08f), violet);
        for (int i = -6; i <= 6; i++)
        {
            Box("FLOOR_SEAM_L_" + i, group, new Vector3(-8f, 0.028f, i * 7f - 7f), new Vector3(13f, 0.008f, 0.035f), navy);
            Box("FLOOR_SEAM_R_" + i, group, new Vector3(8f, 0.028f, i * 7f - 7f), new Vector3(13f, 0.008f, 0.035f), navy);
        }
    }

    private static void BuildDistantShrineSilhouette(Transform parent)
    {
        Transform group = Group(parent, "DISTANT_ORBITAL_SHRINE");
        Vector3 center = new Vector3(0f, 24f, 104f);
        Box("SHRINE_BODY", group, center, new Vector3(24f, 14f, 12f), dark);
        Box("SHRINE_ROOF", group, center + Vector3.up * 8f, new Vector3(34f, 0.9f, 18f), navy, false, new Vector3(4f, 0f, 0f));
        Box("SHRINE_RIDGE", group, center + Vector3.up * 9.1f, new Vector3(21f, 0.34f, 1f), light);
        BuildGeometricCrest(group, center + new Vector3(0f, 1f, -6.2f), violet, 3.2f);
    }

    private static void BuildGeometricCrest(Transform parent, Vector3 center, Material accent, float scale = 1f)
    {
        Box("CREST_VERTICAL", parent, center, new Vector3(0.16f, 2.4f, 0.08f) * scale, accent);
        Box("CREST_TOP", parent, center + Vector3.up * 0.72f * scale, new Vector3(1.7f, 0.14f, 0.08f) * scale, accent, false, new Vector3(0f, 0f, 18f));
        Box("CREST_BOTTOM", parent, center - Vector3.up * 0.72f * scale, new Vector3(1.7f, 0.14f, 0.08f) * scale, accent, false, new Vector3(0f, 0f, -18f));
    }

    private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool local = false, Vector3? euler = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        if (local) go.transform.localPosition = position; else go.transform.position = position;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
        ApplyRenderer(go, material);
        RemoveCollider(go);
        return go;
    }

    private static GameObject Cylinder(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        ApplyRenderer(go, material);
        RemoveCollider(go);
        return go;
    }

    private static void Sign(string name, Transform parent, Vector3 position, string text, float size, Color color, bool local = false, Vector3? euler = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        if (local) go.transform.localPosition = position; else go.transform.position = position;
        go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
        TextMesh mesh = go.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = 64;
        mesh.characterSize = size;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static void ApplyRenderer(GameObject go, Material material)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }

    private static void RemoveCollider(GameObject go)
    {
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static bool LoadMaterials()
    {
        dark = Load("M_DarkCeramic");
        light = Load("M_LightComposite");
        navy = Load("M_NavyMetal");
        floor = Load("M_Floor_Competitive");
        blue = Load("M_Accent_Blue");
        violet = Load("M_Accent_Violet");
        glass = Load("M_Glass_Subtle");
        return dark != null && light != null && navy != null && floor != null && blue != null && violet != null && glass != null;
    }

    private static Material Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + name + ".mat");
    }
}
