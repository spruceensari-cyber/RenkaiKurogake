using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class RenkaiUnifiedCompetitiveBuilder
{
    private const string ScenePath = "Assets/RenkaiKurokage/Scenes/Renkai_Kurogake_Competitive.unity";

    private static Material floorMat;
    private static Material wallMat;
    private static Material lightWallMat;
    private static Material trimMat;
    private static Material coverMat;
    private static Material blueMat;
    private static Material violetMat;
    private static Material siteAMat;
    private static Material siteBMat;

    [MenuItem("Renkai/Build Unified Competitive Kurokage")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateMaterials();

        GameObject root = new GameObject("RENKAI_KUROKAGE_UNIFIED");
        Transform map = Group(root.transform, "MAP");
        Transform gameplay = Group(root.transform, "GAMEPLAY");
        Transform lighting = Group(root.transform, "LIGHTING");

        BuildArena(map);
        BuildObjectiveSites(gameplay);
        BuildPlayer(gameplay);
        BuildLighting(lighting);

        Directory.CreateDirectory("Assets/RenkaiKurokage/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog(
            "Renkai: Kurokage",
            "Tek sürüm rekabetçi sahne oluşturuldu.\n\n" + ScenePath +
            "\n\nEski V2.x kurulum menüleri artık kullanılmamalı.",
            "OK"
        );
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private static void CreateMaterials()
    {
        floorMat = Mat("Unified_Floor", new Color(0.18f, 0.20f, 0.23f), 0.18f, 0f);
        wallMat = Mat("Unified_DarkWall", new Color(0.12f, 0.14f, 0.18f), 0.26f, 0f);
        lightWallMat = Mat("Unified_LightWall", new Color(0.66f, 0.69f, 0.72f), 0.34f, 0f);
        trimMat = Mat("Unified_Trim", new Color(0.045f, 0.055f, 0.075f), 0.42f, 0f);
        coverMat = Mat("Unified_Cover", new Color(0.25f, 0.28f, 0.32f), 0.30f, 0f);
        blueMat = Mat("Unified_BlueAccent", new Color(0.08f, 0.42f, 0.92f), 0.55f, 1.1f);
        violetMat = Mat("Unified_VioletAccent", new Color(0.42f, 0.20f, 0.86f), 0.55f, 0.7f);
        siteAMat = Mat("Unified_SiteA", new Color(0.15f, 0.43f, 0.80f), 0.40f, 0.25f);
        siteBMat = Mat("Unified_SiteB", new Color(0.53f, 0.30f, 0.72f), 0.40f, 0.22f);
    }

    private static Material Mat(string name, Color color, float smoothness, float emission)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader) { name = name };
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.08f);
        if (emission > 0f && mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emission);
        }
        return mat;
    }

    private static void BuildArena(Transform parent)
    {
        Box("Floor", new Vector3(0, -0.5f, 0), new Vector3(110, 1, 150), floorMat, parent);
        Box("NorthWall", new Vector3(0, 4, 74), new Vector3(110, 8, 2), wallMat, parent);
        Box("SouthWall", new Vector3(0, 4, -74), new Vector3(110, 8, 2), wallMat, parent);
        Box("WestWall", new Vector3(-54, 4, 0), new Vector3(2, 8, 150), wallMat, parent);
        Box("EastWall", new Vector3(54, 4, 0), new Vector3(2, 8, 150), wallMat, parent);

        // Three readable routes: A / Mid / B.
        Box("A_Main_Left", new Vector3(-42, 3, -24), new Vector3(3, 6, 46), lightWallMat, parent);
        Box("A_Main_Right", new Vector3(-24, 3, -28), new Vector3(3, 6, 38), wallMat, parent);
        Box("B_Main_Left", new Vector3(24, 3, -28), new Vector3(3, 6, 38), wallMat, parent);
        Box("B_Main_Right", new Vector3(42, 3, -24), new Vector3(3, 6, 46), lightWallMat, parent);

        Box("Mid_Left", new Vector3(-9, 3, -18), new Vector3(3, 6, 42), lightWallMat, parent);
        Box("Mid_Right", new Vector3(9, 3, -18), new Vector3(3, 6, 42), lightWallMat, parent);

        Box("A_Site_Back", new Vector3(-34, 3, 30), new Vector3(32, 6, 3), wallMat, parent);
        Box("B_Site_Back", new Vector3(34, 3, 30), new Vector3(32, 6, 3), wallMat, parent);

        Cover("A_Default", new Vector3(-34, 1.1f, 16), new Vector3(5.5f, 2.2f, 3.5f), parent, blueMat);
        Cover("A_Box_1", new Vector3(-45, 0.9f, 8), new Vector3(4, 1.8f, 5), parent, coverMat);
        Cover("A_Box_2", new Vector3(-25, 1.4f, 21), new Vector3(3, 2.8f, 6), parent, coverMat);

        Cover("B_Default", new Vector3(34, 1.1f, 16), new Vector3(5.5f, 2.2f, 3.5f), parent, violetMat);
        Cover("B_Box_1", new Vector3(45, 0.9f, 8), new Vector3(4, 1.8f, 5), parent, coverMat);
        Cover("B_Box_2", new Vector3(25, 1.4f, 21), new Vector3(3, 2.8f, 6), parent, coverMat);

        Cover("Mid_Cover_1", new Vector3(0, 1.1f, 2), new Vector3(5, 2.2f, 2.6f), parent, coverMat);
        Cover("Mid_Cover_2", new Vector3(-4, 0.75f, 16), new Vector3(3, 1.5f, 4), parent, coverMat);
        Cover("Mid_Cover_3", new Vector3(4, 0.75f, 16), new Vector3(3, 1.5f, 4), parent, coverMat);

        // Restrained accents, not full-scene purple wash.
        Strip("A_Route_Accent", new Vector3(-33, 0.03f, -42), new Vector3(0.35f, 0.06f, 40), blueMat, parent);
        Strip("B_Route_Accent", new Vector3(33, 0.03f, -42), new Vector3(0.35f, 0.06f, 40), violetMat, parent);
    }

    private static void BuildObjectiveSites(Transform parent)
    {
        GameObject a = Cylinder("A_NEXUS", new Vector3(-34, 0.3f, 17), new Vector3(4.8f, 0.3f, 4.8f), siteAMat, parent);
        a.AddComponent<ZodiacNexusSite>();

        GameObject b = Cylinder("B_NEXUS", new Vector3(34, 0.3f, 17), new Vector3(4.8f, 0.3f, 4.8f), siteBMat, parent);
        b.AddComponent<ZodiacNexusSite>();

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "ZODIAC_CORE";
        core.transform.SetParent(parent);
        core.transform.position = new Vector3(0, 1.2f, -60);
        core.transform.localScale = Vector3.one * 0.8f;
        core.GetComponent<Renderer>().sharedMaterial = blueMat;
    }

    private static void BuildPlayer(Transform parent)
    {
        GameObject player = new GameObject("PLAYER_KUROKAGE");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(0, 1f, -62);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0, 0.9f, 0);

        GameObject cameraGo = new GameObject("PlayerCamera");
        cameraGo.transform.SetParent(player.transform);
        cameraGo.transform.localPosition = new Vector3(0, 1.62f, 0);
        Camera cam = cameraGo.AddComponent<Camera>();
        cam.fieldOfView = 90f;
        cameraGo.AddComponent<AudioListener>();

        RenkaiFPSController fps = player.AddComponent<RenkaiFPSController>();
        fps.playerCamera = cam;
        fps.walkSpeed = 5.6f;
        fps.sprintSpeed = 7.4f;
        fps.crouchSpeed = 3.1f;

        RenkaiWeaponController weapon = player.AddComponent<RenkaiWeaponController>();
        weapon.playerCamera = cam;
        player.AddComponent<RenkaiHealth>();
    }

    private static void BuildLighting(Transform parent)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.52f, 0.58f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.50f, 0.57f, 0.64f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 180f;

        GameObject sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(parent);
        sunGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        Light sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(0.93f, 0.96f, 1f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;

        PointLight("A_Site_Light", new Vector3(-34, 7, 17), new Color(0.18f, 0.55f, 1f), 8f, 22f, parent);
        PointLight("B_Site_Light", new Vector3(34, 7, 17), new Color(0.55f, 0.32f, 0.90f), 7f, 22f, parent);
        PointLight("Mid_Light", new Vector3(0, 8, 8), new Color(0.82f, 0.90f, 1f), 6f, 28f, parent);
    }

    private static void PointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static GameObject Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static GameObject Cover(string name, Vector3 pos, Vector3 scale, Transform parent, Material mat)
    {
        return Box(name, pos, scale, mat, parent);
    }

    private static void Strip(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        Box(name, pos, scale, mat, parent);
    }

    private static GameObject Cylinder(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }
}
