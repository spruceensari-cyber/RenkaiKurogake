
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.IO;
using Renkai.Kurogake;

public static class RenkaiKurogakeDistrictV221Generator
{
    private const string ScenePath = "Assets/Renkai/Maps/KurogakeDistrict/Scenes/Renkai_KurogakeDistrict_V2_2_1_CompileFix.unity";

    private static Material matFloor, matWall, matTrim, matNeonPurple, matNeonBlue, matNeonPink, matGlass, matWood, matRoof, matCover, matSiteA, matSiteB, matPortal, matSakura;

    [MenuItem("Renkai/Create Kurogake District V2.2.1 Compile Fix Scene")]
    public static void CreateScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateMaterials();

        GameObject root = new GameObject("KurogakeDistrict_Root");
        Transform gameplay = Group(root.transform, "00_Gameplay");
        Transform ground = Group(root.transform, "01_Ground");
        Transform walls = Group(root.transform, "02_Walls");
        Transform covers = Group(root.transform, "03_Covers");
        Transform arch = Group(root.transform, "04_Architecture");
        Transform neon = Group(root.transform, "05_Neon");
        Transform props = Group(root.transform, "06_Props");
        Transform lighting = Group(root.transform, "07_Lighting");
        Transform vfx = Group(root.transform, "08_VFX");
        Transform audio = Group(root.transform, "09_Audio");
        Transform cameras = Group(root.transform, "10_Cameras");

        CreateGameplayLayout(ground, walls, covers, arch, neon, props);
        CreateBombSites(gameplay);
        CreateKurogates(gameplay, arch, neon, vfx);
        GameObject player = CreatePlayer(gameplay);
        CreateLighting(lighting);
        CreateMinimap(cameras, player.transform);
        CreateCombatDemo(gameplay);
        CreateHUD(root.transform);
        CreateLabels(props);
        CreateProductionNotes(root.transform);

        Directory.CreateDirectory("Assets/Renkai/Maps/KurogakeDistrict/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorUtility.DisplayDialog("Renkai", "Kurogake District V2.2.1 Compile Fix sahnesi oluşturuldu:\n" + ScenePath + "\n\nPlay'e basıp gezebilirsin.", "OK");
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private static void CreateMaterials()
    {
        matFloor = MakeMat("RKD_Wet_Dark_Stone", new Color(0.035f, 0.043f, 0.060f), 0.45f, 0f);
        matWall = MakeMat("RKD_Dark_Metal_Wall", new Color(0.045f, 0.050f, 0.070f), 0.25f, 0f);
        matTrim = MakeMat("RKD_Black_Trim", new Color(0.015f, 0.015f, 0.022f), 0.20f, 0f);
        matNeonPurple = MakeMat("RKD_Neon_Purple_Emission", new Color(0.65f, 0.10f, 1.00f), 0.85f, 3.5f);
        matNeonBlue = MakeMat("RKD_Neon_Blue_Emission", new Color(0.10f, 0.35f, 1.00f), 0.85f, 3.0f);
        matNeonPink = MakeMat("RKD_Neon_Pink_Emission", new Color(1.00f, 0.20f, 0.70f), 0.85f, 2.6f);
        matGlass = MakeMat("RKD_Dark_Blue_Glass", new Color(0.045f, 0.10f, 0.19f), 0.9f, 0.4f);
        matWood = MakeMat("RKD_Dark_Temple_Wood", new Color(0.10f, 0.055f, 0.035f), 0.15f, 0f);
        matRoof = MakeMat("RKD_Indigo_Shrine_Roof", new Color(0.040f, 0.050f, 0.12f), 0.35f, 0f);
        matCover = MakeMat("RKD_Cover_Black_Crate", new Color(0.055f, 0.058f, 0.070f), 0.25f, 0f);
        matSiteA = MakeMat("RKD_A_Site_Neon_Floor", new Color(0.20f, 0.08f, 0.35f), 0.7f, 0.9f);
        matSiteB = MakeMat("RKD_B_Site_Neon_Floor", new Color(0.08f, 0.15f, 0.35f), 0.7f, 0.8f);
        matPortal = MakeMat("RKD_Kurogate_Energy", new Color(0.55f, 0.06f, 1.00f), 0.9f, 4.5f);
        matSakura = MakeMat("RKD_Sakura_Petal_Pink", new Color(0.95f, 0.35f, 0.70f), 0.25f, 0.2f);
    }

    private static Material MakeMat(string name, Color color, float smoothness, float emission)
    {
        Shader shader = GetCompatibleLitShader();
        Material mat = new Material(shader);
        mat.name = name;
        ApplyColorSettings(mat, color, smoothness, emission);
        return mat;
    }

    private static Shader GetCompatibleLitShader()
    {
        bool srpActive = GraphicsSettings.currentRenderPipeline != null;

        if (srpActive)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null) return urpLit;

            Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (urpUnlit != null) return urpUnlit;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null) return standard;

        Shader unlit = Shader.Find("Unlit/Color");
        if (unlit != null) return unlit;

        return Shader.Find("Diffuse");
    }

    private static void ApplyColorSettings(Material mat, Color color, float smoothness, float emission)
    {
        if (mat == null) return;

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.15f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

        if (emission > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * emission);
        }
    }

    private static void CreateGameplayLayout(Transform ground, Transform walls, Transform covers, Transform arch, Transform neon, Transform props)
    {
        // Ground route slabs
        Slab("Attacker_Spawn_Plaza", new Vector3(0, -0.05f, -62), new Vector3(36, 0.12f, 20), matFloor, ground);
        Slab("Defender_Spawn_Plaza", new Vector3(0, -0.05f, 62), new Vector3(36, 0.12f, 20), matFloor, ground);
        Slab("A_Site_Floor", new Vector3(-42, -0.04f, 12), new Vector3(30, 0.12f, 28), matSiteA, ground);
        Slab("B_Site_Floor", new Vector3(42, -0.04f, 12), new Vector3(28, 0.12f, 26), matSiteB, ground);
        Slab("Mid_Shrine_Market_Floor", new Vector3(0, -0.04f, 0), new Vector3(38, 0.12f, 30), matFloor, ground);

        Slab("A_Lobby_Floor", new Vector3(-38, -0.04f, -45), new Vector3(22, 0.10f, 20), matFloor, ground);
        Slab("B_Lobby_Floor", new Vector3(38, -0.04f, -45), new Vector3(22, 0.10f, 20), matFloor, ground);
        Slab("A_Main_Floor", new Vector3(-45, -0.04f, -25), new Vector3(13, 0.10f, 32), matFloor, ground);
        Slab("B_Main_Floor", new Vector3(45, -0.04f, -25), new Vector3(13, 0.10f, 32), matFloor, ground);
        Slab("Mid_Approach_Floor", new Vector3(0, -0.04f, -33), new Vector3(17, 0.10f, 36), matFloor, ground);
        Slab("A_Connector_Floor", new Vector3(-22, -0.04f, 2), new Vector3(22, 0.10f, 8), matFloor, ground);
        Slab("B_Connector_Floor", new Vector3(22, -0.04f, 2), new Vector3(22, 0.10f, 7), matFloor, ground);
        Slab("A_Defender_Route", new Vector3(-28, -0.04f, 42), new Vector3(14, 0.10f, 34), matFloor, ground);
        Slab("B_Defender_Route", new Vector3(28, -0.04f, 42), new Vector3(14, 0.10f, 34), matFloor, ground);

        // Outer borders
        Wall("North_Back_Wall", new Vector3(0, 3, 74), new Vector3(130, 6, 3), matWall, walls);
        Wall("South_Back_Wall", new Vector3(0, 3, -74), new Vector3(130, 6, 3), matWall, walls);
        Wall("West_Back_Wall", new Vector3(-68, 3, 0), new Vector3(3, 6, 150), matWall, walls);
        Wall("East_Back_Wall", new Vector3(68, 3, 0), new Vector3(3, 6, 150), matWall, walls);
        Wall("Invisible_Safety_Floor", new Vector3(0, -14, 0), new Vector3(180, 1, 180), matTrim, walls);

        // Route walls / clean competitive boundaries
        Wall("A_Main_Left_Wall", new Vector3(-53, 3, -25), new Vector3(3, 6, 34), matWall, walls);
        Wall("A_Main_Right_Wall_Broken_1", new Vector3(-37, 3, -35), new Vector3(3, 6, 14), matWall, walls);
        Wall("A_Main_Right_Wall_Broken_2", new Vector3(-37, 3, -15), new Vector3(3, 6, 12), matWall, walls);
        Wall("B_Main_Right_Wall", new Vector3(53, 3, -25), new Vector3(3, 6, 34), matWall, walls);
        Wall("B_Main_Left_Wall_Broken_1", new Vector3(37, 3, -35), new Vector3(3, 6, 14), matWall, walls);
        Wall("B_Main_Left_Wall_Broken_2", new Vector3(37, 3, -15), new Vector3(3, 6, 12), matWall, walls);

        Wall("Mid_Approach_Left", new Vector3(-10, 3, -33), new Vector3(3, 6, 35), matWall, walls);
        Wall("Mid_Approach_Right", new Vector3(10, 3, -33), new Vector3(3, 6, 35), matWall, walls);
        Wall("A_Connector_Back", new Vector3(-22, 3, -5), new Vector3(20, 6, 3), matWall, walls);
        Wall("B_Connector_Back", new Vector3(22, 3, -5), new Vector3(20, 6, 3), matWall, walls);

        Wall("A_Site_Back_Shrine_Wall", new Vector3(-42, 3, 27), new Vector3(30, 6, 3), matWall, walls);
        Wall("B_Site_Back_Wall", new Vector3(42, 3, 27), new Vector3(28, 6, 3), matWall, walls);
        Wall("A_Site_Left_Screen", new Vector3(-58, 3, 12), new Vector3(3, 6, 28), matWall, walls);
        Wall("B_Site_Right_Screen", new Vector3(58, 3, 12), new Vector3(3, 6, 28), matWall, walls);

        // Covers
        Cover("A_Default_Neon_Core", new Vector3(-43, 1, 10), new Vector3(5.5f, 2.0f, 3.2f), matCover, covers, matNeonPurple);
        Cover("A_Pillar_Left", new Vector3(-50, 1.8f, 17), new Vector3(2.2f, 3.6f, 2.2f), matCover, covers, matNeonPurple);
        Cover("A_Low_Stone_Box", new Vector3(-36, 0.7f, 3), new Vector3(4.5f, 1.4f, 2.2f), matCover, covers, matNeonBlue);
        Cover("A_Dark_Box", new Vector3(-52, 0.9f, 2), new Vector3(3.5f, 1.8f, 3.5f), matCover, covers, matNeonPink);

        Cover("B_Reactor_Core", new Vector3(42, 1.2f, 12), new Vector3(5.0f, 2.4f, 5.0f), matCover, covers, matNeonBlue);
        Cover("B_Long_Box", new Vector3(35, 0.8f, 7), new Vector3(6.0f, 1.6f, 2.0f), matCover, covers, matNeonPurple);
        Cover("B_Dark_Box", new Vector3(52, 0.95f, 4), new Vector3(3.0f, 1.9f, 3.0f), matCover, covers, matNeonPink);
        Cover("B_Back_Cover", new Vector3(44, 0.9f, 22), new Vector3(7.0f, 1.8f, 2.2f), matCover, covers, matNeonBlue);

        Cover("Mid_Market_Center_Cover", new Vector3(0, 0.8f, -1), new Vector3(7.0f, 1.6f, 2.6f), matCover, covers, matNeonPurple);
        Cover("Mid_Left_Stall", new Vector3(-11, 0.9f, 5), new Vector3(5.0f, 1.8f, 3.0f), matCover, covers, matNeonPink);
        Cover("Mid_Right_Stall", new Vector3(11, 0.9f, 5), new Vector3(5.0f, 1.8f, 3.0f), matCover, covers, matNeonBlue);
        Cover("A_Main_Anti_Sniper_Box", new Vector3(-44, 1.0f, -25), new Vector3(3.2f, 2.0f, 4.5f), matCover, covers, matNeonPurple);
        Cover("B_Main_Close_Box", new Vector3(44, 1.0f, -25), new Vector3(3.2f, 2.0f, 4.5f), matCover, covers, matNeonBlue);

        // Heaven platforms and stairs
        Platform("A_Heaven_Platform", new Vector3(-31, 3.0f, 20), new Vector3(11, 0.5f, 7), arch, matFloor, matNeonPurple);
        Stairs("A_Heaven_Stairs", new Vector3(-29, 1.4f, 12), new Vector3(6, 0.35f, 2), 6, false, arch);
        Platform("B_Mini_Heaven", new Vector3(32, 2.5f, 20), new Vector3(10, 0.5f, 6), arch, matFloor, matNeonBlue);
        Stairs("B_Mini_Heaven_Stairs", new Vector3(31, 1.2f, 13), new Vector3(5, 0.32f, 1.8f), 5, false, arch);
        Platform("Mid_Balcony", new Vector3(0, 3.3f, 12), new Vector3(16, 0.5f, 5), arch, matFloor, matNeonPurple);
        Stairs("Mid_Balcony_Stairs_Left", new Vector3(-8, 1.3f, 9), new Vector3(4, 0.30f, 1.7f), 6, true, arch);
        Stairs("Mid_Balcony_Stairs_Right", new Vector3(8, 1.3f, 9), new Vector3(4, 0.30f, 1.7f), 6, true, arch);

        // Architecture: temple/cyberpunk buildings
        ShrineBuilding("A_Back_Shrine", new Vector3(-42, 3.2f, 33), new Vector3(18, 6, 8), arch, matWood, matRoof, matGlass, matNeonPurple);
        ShrineBuilding("B_Back_Neon_Building", new Vector3(42, 3.2f, 33), new Vector3(18, 6, 8), arch, matWall, matRoof, matGlass, matNeonBlue);
        ShrineBuilding("Mid_Central_Shrine", new Vector3(0, 3.2f, 18), new Vector3(20, 6, 8), arch, matWood, matRoof, matGlass, matNeonPurple);
        ShrineBuilding("Attacker_Gatehouse", new Vector3(0, 3.2f, -72), new Vector3(26, 6, 7), arch, matWall, matRoof, matGlass, matNeonPink);
        ShrineBuilding("Defender_Gatehouse", new Vector3(0, 3.2f, 72), new Vector3(26, 6, 7), arch, matWall, matRoof, matGlass, matNeonBlue);

        for (int i = 0; i < 5; i++)
        {
            float z = -45 + i * 12;
            ShrineBuilding("West_Cyber_Shop_" + i, new Vector3(-63, 2.8f, z), new Vector3(8, 5.2f, 7), arch, matWall, matRoof, matGlass, i % 2 == 0 ? matNeonPurple : matNeonPink);
            ShrineBuilding("East_Cyber_Shop_" + i, new Vector3(63, 2.8f, z), new Vector3(8, 5.2f, 7), arch, matWall, matRoof, matGlass, i % 2 == 0 ? matNeonBlue : matNeonPurple);
        }

        // Neon strips on floors/routes
        NeonStrip("A_Main_Neon_Line_L", new Vector3(-49.5f, 0.08f, -25), new Vector3(0.18f, 0.08f, 30), matNeonPurple, neon);
        NeonStrip("A_Main_Neon_Line_R", new Vector3(-40.5f, 0.08f, -25), new Vector3(0.18f, 0.08f, 30), matNeonPurple, neon);
        NeonStrip("B_Main_Neon_Line_L", new Vector3(40.5f, 0.08f, -25), new Vector3(0.18f, 0.08f, 30), matNeonBlue, neon);
        NeonStrip("B_Main_Neon_Line_R", new Vector3(49.5f, 0.08f, -25), new Vector3(0.18f, 0.08f, 30), matNeonBlue, neon);
        NeonStrip("Mid_Center_Line", new Vector3(0, 0.09f, 0), new Vector3(26, 0.08f, 0.18f), matNeonPurple, neon);
        NeonRing("A_Site_Neon_Ring", new Vector3(-42, 0.12f, 12), 8f, matNeonPurple, neon);
        NeonRing("B_Site_Neon_Ring", new Vector3(42, 0.12f, 12), 7.2f, matNeonBlue, neon);

        // Props
        SakuraTree("A_Sakura_Tree", new Vector3(-55, 0, 24), props);
        SakuraTree("B_Sakura_Tree", new Vector3(55, 0, 24), props);
        SakuraTree("Mid_Sakura_Tree_Left", new Vector3(-16, 0, -10), props);
        SakuraTree("Mid_Sakura_Tree_Right", new Vector3(16, 0, -10), props);

        for (int i = 0; i < 16; i++)
        {
            float x = (i % 2 == 0 ? -1 : 1) * (58 + (i % 3));
            float z = -55 + i * 7;
            NeonSign("Hologram_Sign_" + i, new Vector3(x, 4.0f + (i % 3), z), new Vector3(0.12f, 1.2f, 4.0f), (x < 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0)), i % 2 == 0 ? matNeonPurple : matNeonBlue, props);
        }
    }

    private static void CreateBombSites(Transform gameplay)
    {
        CreateTrigger("Site_A_Trigger", new Vector3(-42, 1.5f, 12), new Vector3(24, 3, 22), "A", gameplay);
        CreateTrigger("Site_B_Trigger", new Vector3(42, 1.5f, 12), new Vector3(22, 3, 20), "B", gameplay);
        Marker("Spawn_Attackers", new Vector3(0, 1, -62), gameplay);
        Marker("Spawn_Defenders", new Vector3(0, 1, 62), gameplay);
        Marker("Mid_Center", new Vector3(0, 0, 0), gameplay);
    }

    private static void CreateTrigger(string name, Vector3 pos, Vector3 size, string site, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;
        BombSiteZone zone = go.AddComponent<BombSiteZone>();
        zone.siteName = site;
    }

    private static void Marker(string name, Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
    }

    private static void CreateKurogates(Transform gameplay, Transform arch, Transform neon, Transform vfx)
    {
        // visual gates
        ToriiGate("Kurogate_A_Visual", new Vector3(-36, 0, -10), Quaternion.Euler(0, 0, 0), arch, matWall, matNeonPurple, matPortal);
        ToriiGate("Kurogate_B_Visual", new Vector3(36, 0, -10), Quaternion.Euler(0, 180, 0), arch, matWall, matNeonBlue, matPortal);
        ToriiGate("Mid_Symbol_Gate", new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0), arch, matWall, matNeonPurple, matPortal);

        GameObject exitA = new GameObject("Kurogate_A_Exit_Mid_Back");
        exitA.transform.SetParent(gameplay);
        exitA.transform.position = new Vector3(-4, 1.1f, 8);
        exitA.transform.rotation = Quaternion.Euler(0, 180, 0);

        GameObject exitB = new GameObject("Kurogate_B_Exit_A_Connector");
        exitB.transform.SetParent(gameplay);
        exitB.transform.position = new Vector3(-18, 1.1f, 2);
        exitB.transform.rotation = Quaternion.Euler(0, -90, 0);

        GateTrigger("Kurogate_A_Trigger_To_Mid", new Vector3(-36, 2.3f, -10), new Vector3(5, 4.6f, 2.2f), exitA.transform, gameplay);
        GateTrigger("Kurogate_B_Trigger_To_AConnector", new Vector3(36, 2.3f, -10), new Vector3(5, 4.6f, 2.2f), exitB.transform, gameplay);
    }

    private static void GateTrigger(string name, Vector3 pos, Vector3 size, Transform target, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;
        KurogateTeleporter tele = go.AddComponent<KurogateTeleporter>();
        tele.targetPoint = target;
        tele.teleportDelay = 0.7f;
        tele.cooldown = 4f;
    }

    private static GameObject CreatePlayer(Transform parent)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Demo_Player_FPS_Controller";
        player.tag = "Player";
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(0, 2, -62);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0, 0.9f, 0);

        player.AddComponent<BombPlanter>();
        RenkaiFPSController ctrl = player.AddComponent<RenkaiFPSController>();

        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 1.65f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 78f;
        cam.nearClipPlane = 0.03f;
        ctrl.playerCamera = cam;

        GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weapon.name = "Placeholder_Renkai_Rifle_Viewmodel";
        weapon.transform.SetParent(camObj.transform);
        weapon.transform.localPosition = new Vector3(0.38f, -0.28f, 0.65f);
        weapon.transform.localRotation = Quaternion.Euler(-6, 6, 0);
        weapon.transform.localScale = new Vector3(0.22f, 0.16f, 0.9f);
        weapon.GetComponent<Renderer>().sharedMaterial = matWall;

        GameObject weaponNeon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weaponNeon.name = "Rifle_Neon_Line";
        weaponNeon.transform.SetParent(weapon.transform);
        weaponNeon.transform.localPosition = new Vector3(0, 0.55f, 0);
        weaponNeon.transform.localScale = new Vector3(0.25f, 0.08f, 1.05f);
        weaponNeon.GetComponent<Renderer>().sharedMaterial = matNeonPurple;

        RenkaiWeaponController rifle = player.AddComponent<RenkaiWeaponController>();
        rifle.playerCamera = cam;
        ctrl.respawnPoint = CreateRespawnPoint(parent);

        return player;
    }

    private static void CreateLighting(Transform parent)
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.012f, 0.015f, 0.030f);
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.ambientLight = new Color(0.030f, 0.035f, 0.065f);

        GameObject moon = new GameObject("Moon_Directional_Light");
        moon.transform.SetParent(parent);
        moon.transform.rotation = Quaternion.Euler(52, -25, 0);
        Light ml = moon.AddComponent<Light>();
        ml.type = LightType.Directional;
        ml.color = new Color(0.50f, 0.58f, 0.90f);
        ml.intensity = 0.55f;

        PointLight("Neon_Purple_Mid_Light", new Vector3(0, 7, 0), new Color(0.65f, 0.12f, 1f), 6.5f, 42f, parent);
        PointLight("Neon_A_Site_Light", new Vector3(-42, 7, 12), new Color(0.80f, 0.18f, 1f), 5.5f, 34f, parent);
        PointLight("Neon_B_Site_Light", new Vector3(42, 7, 12), new Color(0.12f, 0.38f, 1f), 5.2f, 34f, parent);
        PointLight("Outer_Rain_Backlight", new Vector3(0, 12, -65), new Color(0.25f, 0.20f, 0.85f), 3f, 55f, parent);
        PointLight("Defender_Blue_Backlight", new Vector3(0, 12, 65), new Color(0.10f, 0.42f, 1f), 3f, 55f, parent);

        for (int i = 0; i < 8; i++)
        {
            float x = (i % 2 == 0 ? -52 : 52);
            float z = -44 + i * 14;
            PointLight("Side_Neon_Light_" + i, new Vector3(x, 5.0f, z), i % 2 == 0 ? new Color(0.65f, 0.10f, 1f) : new Color(0.10f, 0.35f, 1f), 2.0f, 18f, parent);
        }
    }

    private static void PointLight(string name, Vector3 pos, Color color, float intensity, float range, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
    }

    private static void CreateMinimap(Transform parent, Transform player)
    {
        GameObject camObj = new GameObject("Minimap_Overhead_Camera");
        camObj.transform.SetParent(parent);
        camObj.transform.position = new Vector3(0, 95, 0);
        camObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 78;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.005f, 0.006f, 0.012f);
        cam.depth = -5;
        MinimapFollow follow = camObj.AddComponent<MinimapFollow>();
        follow.target = player;
        follow.height = 95;
    }

    private static void CreateLabels(Transform parent)
    {
        Label("A SITE", new Vector3(-42, 0.18f, 12), 4.5f, parent, 90, matNeonPurple);
        Label("B SITE", new Vector3(42, 0.18f, 12), 4.5f, parent, 90, matNeonBlue);
        Label("MID", new Vector3(0, 0.18f, 0), 3.5f, parent, 90, matNeonPurple);
        Label("KUROGATE", new Vector3(-36, 4.6f, -11.3f), 1.2f, parent, 0, matNeonPurple);
        Label("KUROGATE", new Vector3(36, 4.6f, -8.7f), 1.2f, parent, 0, matNeonBlue);
    }

    private static void Label(string text, Vector3 pos, float size, Transform parent, float xRot, Material mat)
    {
        GameObject go = new GameObject("Label_" + text.Replace(" ", "_"));
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(xRot, 0, 0);
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    private static void CreateProductionNotes(Transform root)
    {
        GameObject notes = new GameObject("README_SCENE_NOTES");
        notes.transform.SetParent(root);
        notes.transform.position = Vector3.zero;
        TextMesh tm = notes.AddComponent<TextMesh>();
        tm.text = "RENKAI Kurogate District V2\\nPlayable scene generated by Editor script.\\nNext: replace primitives with modular PBR assets, bake lighting, add VFX/audio, playtest timings.";
        tm.characterSize = 1.2f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(0.8f, 0.7f, 1f);
    }


    private static void CreateCombatDemo(Transform parent)
    {
        TargetDummy("Dummy_A_Default", new Vector3(-39, 1, 7), parent, matNeonPurple);
        TargetDummy("Dummy_A_Heaven", new Vector3(-31, 4.0f, 20), parent, matNeonPurple);
        TargetDummy("Dummy_Mid_Left", new Vector3(-8, 1, 4), parent, matNeonPink);
        TargetDummy("Dummy_Mid_Right", new Vector3(8, 1, 4), parent, matNeonBlue);
        TargetDummy("Dummy_B_Reactor", new Vector3(42, 1, 8), parent, matNeonBlue);
        TargetDummy("Dummy_B_Back", new Vector3(48, 1, 21), parent, matNeonBlue);
    }

    private static void TargetDummy(string name, Vector3 pos, Transform parent, Material neonMat)
    {
        GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        dummy.name = name;
        dummy.transform.SetParent(parent);
        dummy.transform.position = pos;
        dummy.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);

        Renderer r = dummy.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = neonMat;

        RenkaiHealth health = dummy.AddComponent<RenkaiHealth>();
        health.maxHealth = 100f;
        health.isTargetDummy = true;
    }

    private static void CreateHUD(Transform parent)
    {
        GameObject canvasObj = new GameObject("HUD_Canvas");
        canvasObj.transform.SetParent(parent);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform);
        Text crosshair = crosshairObj.AddComponent<Text>();
        crosshair.text = "+";
        crosshair.font = font;
        crosshair.fontSize = 32;
        crosshair.alignment = TextAnchor.MiddleCenter;
        crosshair.color = new Color(0.85f, 0.65f, 1f, 0.95f);
        RectTransform crossRect = crosshairObj.GetComponent<RectTransform>();
        crossRect.anchorMin = new Vector2(0.5f, 0.5f);
        crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        crossRect.anchoredPosition = Vector2.zero;
        crossRect.sizeDelta = new Vector2(80, 80);

        GameObject timerObj = new GameObject("Round_Timer");
        timerObj.transform.SetParent(canvasObj.transform);
        Text timer = timerObj.AddComponent<Text>();
        timer.text = "ROUND 100";
        timer.font = font;
        timer.fontSize = 28;
        timer.alignment = TextAnchor.MiddleCenter;
        timer.color = new Color(0.90f, 0.78f, 1f, 1f);
        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0, -42);
        timerRect.sizeDelta = new Vector2(360, 60);

        GameObject infoObj = new GameObject("Control_Info");
        infoObj.transform.SetParent(canvasObj.transform);
        Text info = infoObj.AddComponent<Text>();
        info.text = "WASD Move | Mouse Aim | LMB Fire | F Plant | Shift Sprint | Kurogate";
        info.font = font;
        info.fontSize = 16;
        info.alignment = TextAnchor.MiddleCenter;
        info.color = new Color(0.75f, 0.70f, 1f, 0.90f);
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.5f, 0f);
        infoRect.anchorMax = new Vector2(0.5f, 0f);
        infoRect.anchoredPosition = new Vector2(0, 34);
        infoRect.sizeDelta = new Vector2(900, 40);


        GameObject hitObj = new GameObject("HitMarker_Text");
        hitObj.transform.SetParent(canvasObj.transform);
        Text hit = hitObj.AddComponent<Text>();
        hit.text = "HIT";
        hit.font = font;
        hit.fontSize = 24;
        hit.alignment = TextAnchor.MiddleCenter;
        hit.color = new Color(1f, 0.35f, 0.95f, 1f);
        hit.enabled = false;
        RectTransform hitRect = hitObj.GetComponent<RectTransform>();
        hitRect.anchorMin = new Vector2(0.5f, 0.5f);
        hitRect.anchorMax = new Vector2(0.5f, 0.5f);
        hitRect.anchoredPosition = new Vector2(0, 42);
        hitRect.sizeDelta = new Vector2(160, 50);

        RenkaiHitFeedback feedback = Object.FindObjectOfType<RenkaiHitFeedback>();
        if (feedback == null)
        {
            GameObject playerObj = GameObject.Find("Demo_Player_FPS_Controller");
            if (playerObj != null) feedback = playerObj.AddComponent<RenkaiHitFeedback>();
        }
        if (feedback != null) feedback.hitText = hit;


        RenkaiHUD hud = canvasObj.AddComponent<RenkaiHUD>();
        hud.timerText = timer;
        hud.infoText = info;
        hud.roundTime = 100f;
    }



    private static Transform CreateRespawnPoint(Transform parent)
    {
        GameObject spawn = new GameObject("Player_Respawn_Point");
        spawn.transform.SetParent(parent);
        spawn.transform.position = new Vector3(0f, 2f, -62f);
        spawn.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        return spawn.transform;
    }


    // Primitive helpers
    private static GameObject Slab(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static GameObject Wall(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = Slab(name, pos, scale, mat, parent);
        return go;
    }

    private static void Cover(string name, Vector3 pos, Vector3 scale, Material baseMat, Transform parent, Material neonMat)
    {
        GameObject go = Slab(name, pos, scale, baseMat, parent);
        NeonStrip(name + "_Neon_Top", pos + new Vector3(0, scale.y/2f + 0.04f, 0), new Vector3(scale.x * 0.82f, 0.08f, 0.12f), neonMat, parent);
        NeonStrip(name + "_Neon_Side", pos + new Vector3(0, 0.05f, scale.z/2f + 0.06f), new Vector3(scale.x * 0.72f, 0.08f, 0.10f), neonMat, parent);
    }

    private static void Platform(string name, Vector3 pos, Vector3 scale, Transform parent, Material baseMat, Material neonMat)
    {
        Slab(name, pos, scale, baseMat, parent);
        NeonStrip(name + "_Edge_Neon_Front", pos + new Vector3(0, 0.31f, scale.z/2f), new Vector3(scale.x, 0.10f, 0.12f), neonMat, parent);
    }

    private static void Stairs(string name, Vector3 startPos, Vector3 stepScale, int steps, bool mirror, Transform parent)
    {
        for (int i = 0; i < steps; i++)
        {
            float dir = mirror ? -1f : 1f;
            Vector3 pos = startPos + new Vector3(0, i * stepScale.y, dir * i * stepScale.z);
            Slab(name + "_Step_" + i, pos, stepScale, matFloor, parent);
        }
    }

    private static void ShrineBuilding(string name, Vector3 pos, Vector3 scale, Transform parent, Material bodyMat, Material roofMat, Material glassMat, Material neonMat)
    {
        Slab(name + "_Body", pos, scale, bodyMat, parent);
        Slab(name + "_Glass_Front", pos + new Vector3(0, 0.2f, -scale.z/2f - 0.05f), new Vector3(scale.x * 0.55f, scale.y * 0.45f, 0.12f), glassMat, parent);
        Slab(name + "_Roof_Main", pos + new Vector3(0, scale.y/2f + 0.75f, 0), new Vector3(scale.x + 3.5f, 0.8f, scale.z + 3.0f), roofMat, parent);
        Slab(name + "_Roof_Ridge", pos + new Vector3(0, scale.y/2f + 1.4f, 0), new Vector3(scale.x * 0.55f, 0.5f, scale.z + 4.0f), roofMat, parent);
        NeonStrip(name + "_Neon_Left", pos + new Vector3(-scale.x/2f - 0.08f, 0, -scale.z/2f), new Vector3(0.12f, scale.y * 0.75f, 0.12f), neonMat, parent);
        NeonStrip(name + "_Neon_Right", pos + new Vector3(scale.x/2f + 0.08f, 0, -scale.z/2f), new Vector3(0.12f, scale.y * 0.75f, 0.12f), neonMat, parent);
        NeonSign(name + "_Holo_Sign", pos + new Vector3(0, 1.3f, -scale.z/2f - 0.25f), new Vector3(scale.x * 0.42f, 1.0f, 0.12f), Quaternion.identity, neonMat, parent);
    }

    private static void ToriiGate(string name, Vector3 pos, Quaternion rot, Transform parent, Material frameMat, Material neonMat, Material portalMat)
    {
        GameObject holder = new GameObject(name);
        holder.transform.SetParent(parent);
        holder.transform.position = pos;
        holder.transform.rotation = rot;

        Slab("Post_L", new Vector3(-2.2f, 2.2f, 0), new Vector3(0.5f, 4.4f, 0.6f), frameMat, holder.transform);
        Slab("Post_R", new Vector3(2.2f, 2.2f, 0), new Vector3(0.5f, 4.4f, 0.6f), frameMat, holder.transform);
        Slab("Top_Beam", new Vector3(0, 4.6f, 0), new Vector3(6.0f, 0.55f, 0.7f), frameMat, holder.transform);
        Slab("Middle_Beam", new Vector3(0, 3.55f, 0), new Vector3(4.9f, 0.35f, 0.55f), frameMat, holder.transform);
        Slab("Neon_Top_Line", new Vector3(0, 4.95f, -0.05f), new Vector3(6.2f, 0.12f, 0.12f), neonMat, holder.transform);

        // portal disc represented by flattened cylinder
        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Energy_Disc";
        disc.transform.SetParent(holder.transform);
        disc.transform.localPosition = new Vector3(0, 2.15f, 0);
        disc.transform.localRotation = Quaternion.Euler(90, 0, 0);
        disc.transform.localScale = new Vector3(1.6f, 0.06f, 1.6f);
        disc.GetComponent<Renderer>().sharedMaterial = portalMat;

        PointLight(name + "_Portal_Point_Light", pos + Vector3.up * 2.4f, neonMat == matNeonBlue ? new Color(0.12f,0.38f,1f) : new Color(0.65f,0.10f,1f), 2.2f, 12f, parent);
    }

    private static void NeonStrip(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        Slab(name, pos, scale, mat, parent);
    }

    private static void NeonSign(string name, Vector3 pos, Vector3 scale, Quaternion rot, Material mat, Transform parent)
    {
        GameObject sign = Slab(name, pos, scale, mat, parent);
        sign.transform.rotation = rot;
    }

    private static void NeonRing(string name, Vector3 center, float radius, Material mat, Transform parent)
    {
        int segments = 24;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            float next = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 p1 = center + new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(next) * radius, 0, Mathf.Sin(next) * radius);
            Vector3 mid = (p1 + p2) * 0.5f;
            float len = Vector3.Distance(p1, p2);
            GameObject seg = Slab(name + "_Seg_" + i, mid, new Vector3(len, 0.08f, 0.12f), mat, parent);
            seg.transform.rotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg, 0);
        }
    }

    private static void SakuraTree(string name, Vector3 pos, Transform parent)
    {
        GameObject holder = new GameObject(name);
        holder.transform.SetParent(parent);
        holder.transform.position = pos;
        Slab(name + "_Trunk", pos + new Vector3(0, 1.6f, 0), new Vector3(0.45f, 3.2f, 0.45f), matWood, holder.transform);
        for (int i = 0; i < 7; i++)
        {
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = name + "_Petal_Cloud_" + i;
            crown.transform.SetParent(holder.transform);
            float a = i * 51.4f * Mathf.Deg2Rad;
            crown.transform.localPosition = new Vector3(Mathf.Cos(a) * 1.25f, 3.7f + (i % 3) * 0.35f, Mathf.Sin(a) * 1.05f);
            crown.transform.localScale = new Vector3(2.2f, 1.1f, 1.8f);
            crown.GetComponent<Renderer>().sharedMaterial = matSakura;
        }
    }
}
