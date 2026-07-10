using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurokage;

public static class KurokageZodiacNexusArtInstaller
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    public static bool ApplySilent()
    {
        ZodiacNexusSite[] sites = Object.FindObjectsOfType<ZodiacNexusSite>(true);
        if (sites == null || sites.Length != 2) return false;

        Material dark = Load("M_DarkCeramic");
        Material light = Load("M_LightComposite");
        Material navy = Load("M_NavyMetal");
        Material blue = Load("M_Accent_Blue");
        Material violet = Load("M_Accent_Violet");
        Material energy = Load("M_Energy_Core");
        if (dark == null || light == null || navy == null || blue == null || violet == null || energy == null) return false;

        foreach (ZodiacNexusSite site in sites)
        {
            string resolvedId = site.gameObject.name.StartsWith("B_") || site.gameObject.name.Contains("B_NEXUS") ? "B" : "A";
            site.Configure(resolvedId);

            Transform old = site.transform.Find("ZODIAC_NEXUS_ART");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            GameObject art = new GameObject("ZODIAC_NEXUS_ART");
            art.transform.SetParent(site.transform, false);
            art.transform.localPosition = new Vector3(0f, 0.18f, 0f);

            Material siteAccent = site.SiteId == "B" ? violet : blue;

            Transform foundation = Group(art.transform, "NEXUS_FOUNDATION");
            BuildSegmentedRing(foundation, 3.55f, 20, 0.86f, 0.22f, dark);
            BuildSegmentedRing(foundation, 2.92f, 18, 0.68f, 0.16f, light);

            Transform innerRing = Group(art.transform, "NEXUS_RING_INNER");
            BuildSegmentedRing(innerRing, 2.28f, 16, 0.56f, 0.09f, siteAccent);
            innerRing.localPosition = new Vector3(0f, 0.18f, 0f);

            Transform outerRing = Group(art.transform, "NEXUS_RING_OUTER");
            BuildSegmentedRing(outerRing, 3.05f, 18, 0.68f, 0.07f, navy);
            outerRing.localPosition = new Vector3(0f, 0.34f, 0f);

            Transform crown = Group(art.transform, "NEXUS_CROWN");
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 2.72f);
                GameObject pylon = Cube("NEXUS_PYLON_" + i.ToString("00"), crown, offset + Vector3.up * 1.55f, new Vector3(0.28f, 3.1f, 0.42f), i % 2 == 0 ? light : dark);
                pylon.transform.localRotation = Quaternion.Euler(0f, angle, i % 2 == 0 ? 7f : -7f);
            }

            Transform energyRoot = Group(art.transform, "NEXUS_ENERGY");
            Cylinder("NEXUS_ENERGY_WELL", energyRoot, new Vector3(0f, 0.12f, 0f), new Vector3(1.25f, 0.045f, 1.25f), energy);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 1.58f);
                GameObject beam = Cube("NEXUS_RESONANCE_BEAM_" + i, energyRoot, offset + Vector3.up * 1.2f, new Vector3(0.06f, 2.35f, 0.06f), i % 2 == 0 ? siteAccent : energy);
                beam.transform.localRotation = Quaternion.Euler(0f, angle, 18f);
            }

            if (site.GetComponent<KurokageNexusVfxPresenter>() == null)
                site.gameObject.AddComponent<KurokageNexusVfxPresenter>();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static void BuildSegmentedRing(Transform parent, float radius, int count, float segmentLength, float thickness, Material material)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * 360f / count;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
            GameObject segment = Cube("SEGMENT_" + i.ToString("00"), parent, pos, new Vector3(segmentLength, thickness, 0.14f), material);
            segment.transform.localRotation = Quaternion.Euler(0f, -angle, 0f);
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

    private static GameObject Cube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }

    private static GameObject Cylinder(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }
}
