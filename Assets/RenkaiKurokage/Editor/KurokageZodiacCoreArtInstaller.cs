using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KurokageZodiacCoreArtInstaller
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    public static bool ApplySilent()
    {
        GameObject core = GameObject.Find("ZODIAC_CORE");
        if (core == null) return false;

        Transform oldArt = core.transform.Find("ZODIAC_CORE_ART");
        if (oldArt != null) Object.DestroyImmediate(oldArt.gameObject);

        Material dark = Load("M_DarkCeramic");
        Material light = Load("M_LightComposite");
        Material blue = Load("M_Accent_Blue");
        Material violet = Load("M_Accent_Violet");
        Material energy = Load("M_Energy_Core");
        if (dark == null || light == null || blue == null || violet == null || energy == null) return false;

        Renderer rootRenderer = core.GetComponent<Renderer>();
        if (rootRenderer != null) rootRenderer.enabled = false;

        GameObject art = new GameObject("ZODIAC_CORE_ART");
        art.transform.SetParent(core.transform, false);
        art.transform.localPosition = Vector3.zero;
        art.transform.localRotation = Quaternion.identity;

        Sphere("CORE_INNER_ENERGY", art.transform, Vector3.zero, Vector3.one * 0.46f, energy);
        Sphere("CORE_INNER_CAGE", art.transform, Vector3.zero, Vector3.one * 0.58f, blue);

        Transform shell = Group(art.transform, "CORE_SHELL_STRUCTURE");
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 0.48f);
            GameObject fin = Cube("CORE_SHELL_FIN_" + i.ToString("00"), shell, offset, new Vector3(0.14f, 0.72f, 0.24f), i % 2 == 0 ? light : dark);
            fin.transform.localRotation = Quaternion.Euler(0f, angle, i % 2 == 0 ? 12f : -12f);
        }

        Transform ringA = Group(art.transform, "CORE_RING_A");
        BuildSegmentedRing(ringA, 0.88f, 12, 0.22f, 0.08f, light);
        ringA.localRotation = Quaternion.Euler(18f, 0f, 0f);

        Transform ringB = Group(art.transform, "CORE_RING_B");
        BuildSegmentedRing(ringB, 1.10f, 14, 0.18f, 0.065f, blue);
        ringB.localRotation = Quaternion.Euler(90f, 0f, 24f);

        Transform halo = Group(art.transform, "CORE_HALO_OUTER");
        BuildSegmentedRing(halo, 1.36f, 16, 0.16f, 0.045f, violet);
        halo.localRotation = Quaternion.Euler(62f, 30f, 14f);

        Transform spines = Group(art.transform, "CORE_CELESTIAL_SPINES");
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f + 45f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 0.86f);
            GameObject spine = Cube("CORE_SPINE_" + i, spines, offset, new Vector3(0.045f, 0.92f, 0.045f), i % 2 == 0 ? blue : violet);
            spine.transform.localRotation = Quaternion.Euler(0f, angle, 28f);
        }

        GameObject lightGo = new GameObject("ZODIAC_CORE_LIGHT");
        lightGo.transform.SetParent(art.transform, false);
        Light point = lightGo.AddComponent<Light>();
        point.type = LightType.Point;
        point.range = 6.5f;
        point.intensity = 1.8f;
        point.color = new Color(0.16f, 0.55f, 1f);
        point.shadows = LightShadows.None;

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
            GameObject segment = Cube("SEGMENT_" + i.ToString("00"), parent, pos, new Vector3(segmentLength, thickness, 0.08f), material);
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

    private static GameObject Sphere(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
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
