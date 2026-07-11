using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageGameplayUpgradeInstaller
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    public static void Upgrade()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        bool ok = UpgradeSilent();
        EditorUtility.DisplayDialog(
            "Renkai",
            ok ? "Gun feel upgrade bağlandı. Ctrl+S ile kaydet ve Play'e bas." : "Gameplay upgrade başarısız. Player weapon/camera referanslarını kontrol et.",
            ok ? "OK" : "REVIEW"
        );
    }

    public static bool UpgradeSilent()
    {
        RenkaiWeaponController weapon = Object.FindObjectOfType<RenkaiWeaponController>();
        if (weapon == null) return false;

        Camera cam = weapon.playerCamera != null ? weapon.playerCamera : weapon.GetComponentInChildren<Camera>();
        if (cam == null) return false;

        Transform viewRoot = cam.transform.Find("KUROKAGE_VIEWMODEL");
        if (viewRoot == null)
        {
            GameObject root = new GameObject("KUROKAGE_VIEWMODEL");
            root.transform.SetParent(cam.transform, false);
            root.transform.localPosition = new Vector3(0.32f, -0.28f, 0.72f);
            root.transform.localRotation = Quaternion.Euler(2f, -4f, 0f);
            viewRoot = root.transform;
        }

        if (viewRoot.GetComponent<KurokageWeaponSway>() == null)
            viewRoot.gameObject.AddComponent<KurokageWeaponSway>();

        if (viewRoot.GetComponent<KurokageViewmodelAnimator>() == null)
            viewRoot.gameObject.AddComponent<KurokageViewmodelAnimator>();

        weapon.rifleView = BuildRifleView(viewRoot);
        weapon.pistolView = BuildPistolView(viewRoot);
        weapon.swordView = BuildSwordView(viewRoot);

        GameObject hudGo = GameObject.Find("KUROKAGE_COMPETITIVE_HUD");
        if (hudGo == null)
        {
            hudGo = new GameObject("KUROKAGE_COMPETITIVE_HUD");
            hudGo.AddComponent<KurokageCompetitiveHUD>();
        }

        weapon.rifleView.SetActive(true);
        weapon.pistolView.SetActive(false);
        weapon.swordView.SetActive(false);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = viewRoot.gameObject;
        return true;
    }

    private static GameObject BuildRifleView(Transform root)
    {
        Transform old = root.Find("KX9_KURO_VIEW");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject holder = new GameObject("KX9_KURO_VIEW");
        holder.transform.SetParent(root, false);
        holder.transform.localPosition = new Vector3(0.02f, -0.01f, 0.02f);
        holder.transform.localRotation = Quaternion.Euler(0f, -1.5f, 0f);

        Material dark = SharedOrFallback("M_DarkCeramic", new Color(0.035f, 0.045f, 0.065f), 0.72f, 0f);
        Material navy = SharedOrFallback("M_NavyMetal", new Color(0.055f, 0.08f, 0.12f), 0.62f, 0f);
        Material accent = SharedOrFallback("M_Accent_Blue", new Color(0.06f, 0.42f, 0.95f), 0.48f, 1.4f);
        Material light = SharedOrFallback("M_LightComposite", new Color(0.66f, 0.70f, 0.74f), 0.52f, 0f);

        Part("Receiver_Core", holder.transform, new Vector3(0f, 0f, 0.27f), new Vector3(0.20f, 0.17f, 0.54f), dark);
        Part("Receiver_LeftShell", holder.transform, new Vector3(-0.115f, 0.018f, 0.30f), new Vector3(0.055f, 0.135f, 0.41f), light, new Vector3(0f, -7f, 0f));
        Part("Receiver_RightTech", holder.transform, new Vector3(0.112f, 0.025f, 0.34f), new Vector3(0.048f, 0.12f, 0.30f), navy, new Vector3(0f, 8f, 0f));
        Part("Receiver_AftSlope", holder.transform, new Vector3(0f, 0.045f, -0.01f), new Vector3(0.15f, 0.10f, 0.18f), navy, new Vector3(-8f, 0f, 0f));
        Part("UpperReceiver", holder.transform, new Vector3(0f, 0.093f, 0.34f), new Vector3(0.14f, 0.055f, 0.35f), light);
        Part("BoltHousing", holder.transform, new Vector3(0.075f, 0.055f, 0.22f), new Vector3(0.055f, 0.045f, 0.22f), navy);
        Part("ChargingRail", holder.transform, new Vector3(0.098f, 0.086f, 0.20f), new Vector3(0.025f, 0.025f, 0.18f), accent);

        Part("Handguard_Core", holder.transform, new Vector3(0f, -0.004f, 0.66f), new Vector3(0.145f, 0.115f, 0.34f), light);
        Part("Handguard_Lower", holder.transform, new Vector3(0f, -0.085f, 0.64f), new Vector3(0.10f, 0.045f, 0.31f), navy);
        Part("Handguard_LeftRail", holder.transform, new Vector3(-0.092f, 0.015f, 0.66f), new Vector3(0.025f, 0.055f, 0.28f), dark);
        Part("Handguard_RightRail", holder.transform, new Vector3(0.092f, 0.015f, 0.66f), new Vector3(0.025f, 0.055f, 0.28f), dark);

        for (int i = 0; i < 4; i++)
        {
            float z = 0.51f + i * 0.085f;
            Part("Vent_Left_" + i, holder.transform, new Vector3(-0.108f, 0.035f, z), new Vector3(0.010f, 0.045f, 0.045f), navy, new Vector3(0f, -12f, 0f));
            Part("Vent_Right_" + i, holder.transform, new Vector3(0.108f, 0.035f, z), new Vector3(0.010f, 0.045f, 0.045f), navy, new Vector3(0f, 12f, 0f));
        }

        PrimitivePart("BarrelCore", holder.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 0.91f), new Vector3(0.036f, 0.18f, 0.036f), dark, new Vector3(90f, 0f, 0f));
        PrimitivePart("MuzzleCrown", holder.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 1.105f), new Vector3(0.085f, 0.065f, 0.085f), navy, new Vector3(90f, 0f, 0f));
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            Vector3 radial = Quaternion.Euler(0f, 0f, angle) * new Vector3(0.055f, 0f, 0f);
            Part("MuzzleProng_" + i, holder.transform, new Vector3(radial.x, radial.y + 0.012f, 1.19f), new Vector3(0.022f, 0.022f, 0.15f), i % 2 == 0 ? dark : navy);
        }

        Part("Grip", holder.transform, new Vector3(0.02f, -0.18f, 0.19f), new Vector3(0.095f, 0.26f, 0.12f), dark, new Vector3(13f, 0f, 0f));
        Part("GripAccent", holder.transform, new Vector3(0.071f, -0.17f, 0.19f), new Vector3(0.012f, 0.12f, 0.07f), accent, new Vector3(13f, 0f, 0f));
        Part("Magazine", holder.transform, new Vector3(0f, -0.16f, 0.32f), new Vector3(0.085f, 0.24f, 0.13f), light, new Vector3(11f, 0f, 0f));
        Part("MagazineDarkCap", holder.transform, new Vector3(0f, -0.285f, 0.345f), new Vector3(0.09f, 0.035f, 0.13f), dark, new Vector3(11f, 0f, 0f));
        Part("MagazineEnergyWindow", holder.transform, new Vector3(0.044f, -0.16f, 0.32f), new Vector3(0.012f, 0.13f, 0.078f), accent, new Vector3(11f, 0f, 0f));

        Part("StockBridge", holder.transform, new Vector3(0f, 0.018f, -0.06f), new Vector3(0.12f, 0.075f, 0.24f), navy);
        Part("StockUpper", holder.transform, new Vector3(0f, 0.055f, -0.24f), new Vector3(0.14f, 0.07f, 0.22f), light, new Vector3(-5f, 0f, 0f));
        Part("StockShoulder", holder.transform, new Vector3(0f, -0.02f, -0.31f), new Vector3(0.17f, 0.16f, 0.18f), dark, new Vector3(-7f, 0f, 0f));
        Part("StockPad", holder.transform, new Vector3(0f, -0.02f, -0.415f), new Vector3(0.18f, 0.17f, 0.035f), navy, new Vector3(-7f, 0f, 0f));

        Part("BlueRail", holder.transform, new Vector3(0f, 0.138f, 0.39f), new Vector3(0.034f, 0.024f, 0.52f), accent);
        Part("SightBase", holder.transform, new Vector3(0f, 0.158f, 0.23f), new Vector3(0.12f, 0.05f, 0.14f), navy);
        PrimitivePart("SightRing", holder.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.215f, 0.23f), new Vector3(0.075f, 0.018f, 0.075f), dark, new Vector3(90f, 0f, 0f));
        Part("SightGlass", holder.transform, new Vector3(0f, 0.218f, 0.242f), new Vector3(0.055f, 0.052f, 0.012f), accent);
        BuildRifleHands(holder.transform, dark, navy, accent, light);

        return holder;
    }

    private static void BuildRifleHands(Transform root, Material glove, Material armor, Material accent, Material palm)
    {
        Transform rightForearm = new GameObject("RIGHT_FOREARM").transform;
        rightForearm.SetParent(root, false);
        rightForearm.localPosition = new Vector3(0.24f, -0.22f, 0.02f);
        rightForearm.localRotation = Quaternion.Euler(18f, -8f, -22f);
        PrimitivePart("RIGHT_FOREARM_SLEEVE", rightForearm, PrimitiveType.Capsule, new Vector3(0f, 0f, 0.10f), new Vector3(0.105f, 0.24f, 0.105f), glove, new Vector3(90f, 0f, 0f));
        PrimitivePart("RIGHT_WRIST_ARMOR", rightForearm, PrimitiveType.Cube, new Vector3(-0.01f, 0.015f, 0.22f), new Vector3(0.16f, 0.12f, 0.12f), armor, Vector3.zero);
        PrimitivePart("RIGHT_GLOVE", rightForearm, PrimitiveType.Sphere, new Vector3(-0.04f, 0.01f, 0.33f), new Vector3(0.15f, 0.10f, 0.17f), palm, new Vector3(0f, 0f, -12f));
        PrimitivePart("RIGHT_GLOVE_PANEL", rightForearm, PrimitiveType.Cube, new Vector3(-0.04f, 0.06f, 0.35f), new Vector3(0.11f, 0.025f, 0.12f), accent, new Vector3(0f, 0f, -12f));
        for (int i = 0; i < 3; i++)
        {
            PrimitivePart("RIGHT_FINGER_" + i, rightForearm, PrimitiveType.Capsule, new Vector3(-0.055f + i * 0.04f, -0.035f, 0.43f), new Vector3(0.024f, 0.07f, 0.024f), glove, new Vector3(86f, 0f, 0f));
        }

        Transform leftForearm = new GameObject("LEFT_FOREARM").transform;
        leftForearm.SetParent(root, false);
        leftForearm.localPosition = new Vector3(-0.26f, -0.24f, 0.40f);
        leftForearm.localRotation = Quaternion.Euler(16f, 8f, 24f);
        PrimitivePart("LEFT_FOREARM_SLEEVE", leftForearm, PrimitiveType.Capsule, new Vector3(0f, 0f, 0.11f), new Vector3(0.11f, 0.27f, 0.11f), glove, new Vector3(90f, 0f, 0f));
        PrimitivePart("LEFT_WRIST_ARMOR", leftForearm, PrimitiveType.Cube, new Vector3(0.01f, 0.015f, 0.24f), new Vector3(0.17f, 0.12f, 0.13f), armor, Vector3.zero);
        PrimitivePart("LEFT_GLOVE", leftForearm, PrimitiveType.Sphere, new Vector3(0.05f, 0.01f, 0.37f), new Vector3(0.16f, 0.10f, 0.18f), palm, new Vector3(0f, 0f, 12f));
        PrimitivePart("LEFT_GLOVE_PANEL", leftForearm, PrimitiveType.Cube, new Vector3(0.05f, 0.06f, 0.39f), new Vector3(0.12f, 0.025f, 0.13f), accent, new Vector3(0f, 0f, 12f));
        for (int i = 0; i < 3; i++)
        {
            PrimitivePart("LEFT_FINGER_" + i, leftForearm, PrimitiveType.Capsule, new Vector3(0.060f - i * 0.04f, -0.035f, 0.47f), new Vector3(0.024f, 0.07f, 0.024f), glove, new Vector3(86f, 0f, 0f));
        }
    }

    private static GameObject BuildPistolView(Transform root)
    {
        Transform old = root.Find("SHIRO_SIDEARM_VIEW");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject holder = new GameObject("SHIRO_SIDEARM_VIEW");
        holder.transform.SetParent(root, false);
        holder.transform.localPosition = new Vector3(0.08f, -0.035f, 0.10f);
        holder.transform.localRotation = Quaternion.Euler(0f, -2f, 0f);

        Material light = SharedOrFallback("M_LightComposite", new Color(0.66f, 0.70f, 0.74f), 0.52f, 0f);
        Material dark = SharedOrFallback("M_DarkCeramic", new Color(0.035f, 0.045f, 0.065f), 0.68f, 0f);
        Material accent = SharedOrFallback("M_Accent_Violet", new Color(0.34f, 0.16f, 0.70f), 0.48f, 1.1f);
        Material blue = SharedOrFallback("M_Accent_Blue", new Color(0.06f, 0.42f, 0.95f), 0.48f, 1.0f);

        Part("Frame", holder.transform, new Vector3(0f, -0.02f, 0.19f), new Vector3(0.135f, 0.09f, 0.39f), dark);
        Part("Slide", holder.transform, new Vector3(0f, 0.042f, 0.24f), new Vector3(0.15f, 0.105f, 0.42f), light);
        Part("SlideTopSpine", holder.transform, new Vector3(0f, 0.102f, 0.23f), new Vector3(0.08f, 0.026f, 0.34f), dark);
        Part("SlideCutLeft", holder.transform, new Vector3(-0.081f, 0.05f, 0.30f), new Vector3(0.014f, 0.055f, 0.17f), accent, new Vector3(0f, -5f, 0f));
        Part("SlideCutRight", holder.transform, new Vector3(0.081f, 0.05f, 0.30f), new Vector3(0.014f, 0.055f, 0.17f), blue, new Vector3(0f, 5f, 0f));
        PrimitivePart("Barrel", holder.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0.50f), new Vector3(0.038f, 0.12f, 0.038f), dark, new Vector3(90f, 0f, 0f));
        PrimitivePart("MuzzleRing", holder.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0.635f), new Vector3(0.075f, 0.04f, 0.075f), dark, new Vector3(90f, 0f, 0f));
        Part("Grip", holder.transform, new Vector3(0f, -0.19f, 0.05f), new Vector3(0.11f, 0.28f, 0.14f), dark, new Vector3(12f, 0f, 0f));
        Part("GripInsert", holder.transform, new Vector3(0.061f, -0.18f, 0.05f), new Vector3(0.012f, 0.14f, 0.08f), blue, new Vector3(12f, 0f, 0f));
        Part("Magazine", holder.transform, new Vector3(0f, -0.15f, 0.30f), new Vector3(0.058f, 0.17f, 0.075f), dark, new Vector3(7f, 0f, 0f));
        Part("EnergyChannel", holder.transform, new Vector3(0f, -0.006f, 0.28f), new Vector3(0.048f, 0.026f, 0.26f), blue);
        Part("TriggerGuard", holder.transform, new Vector3(0f, -0.095f, 0.22f), new Vector3(0.10f, 0.025f, 0.13f), light, new Vector3(8f, 0f, 0f));
        Part("RearSight", holder.transform, new Vector3(0f, 0.118f, 0.08f), new Vector3(0.105f, 0.035f, 0.05f), dark);
        Part("FrontSight", holder.transform, new Vector3(0f, 0.118f, 0.45f), new Vector3(0.05f, 0.035f, 0.04f), accent);

        return holder;
    }

    private static GameObject BuildSwordView(Transform root)
    {
        Transform old = root.Find("ECLIPSE_BLADE_VIEW");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject holder = new GameObject("ECLIPSE_BLADE_VIEW");
        holder.transform.SetParent(root, false);
        holder.transform.localPosition = new Vector3(0.10f, -0.03f, 0.05f);
        holder.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);

        Material dark = SharedOrFallback("M_DarkCeramic", new Color(0.035f, 0.045f, 0.065f), 0.72f, 0f);
        Material light = SharedOrFallback("M_LightComposite", new Color(0.66f, 0.70f, 0.74f), 0.52f, 0f);
        Material blue = SharedOrFallback("M_Accent_Blue", new Color(0.06f, 0.42f, 0.95f), 0.48f, 1.7f);
        Material violet = SharedOrFallback("M_Accent_Violet", new Color(0.34f, 0.16f, 0.70f), 0.48f, 1.2f);

        Part("BladeSpine", holder.transform, new Vector3(0f, 0.01f, 0.62f), new Vector3(0.075f, 0.04f, 1.08f), dark);
        Part("BladeCore", holder.transform, new Vector3(-0.020f, 0.030f, 0.66f), new Vector3(0.028f, 0.026f, 0.96f), blue);
        Part("BladeEdge", holder.transform, new Vector3(0.046f, 0.026f, 0.68f), new Vector3(0.016f, 0.018f, 0.91f), violet, new Vector3(0f, 0f, -2f));
        Part("BladeWhiteHotCenter", holder.transform, new Vector3(0.016f, 0.036f, 0.71f), new Vector3(0.012f, 0.010f, 0.78f), light);
        Part("BladeTip", holder.transform, new Vector3(0f, 0.02f, 1.23f), new Vector3(0.050f, 0.030f, 0.16f), blue, new Vector3(0f, 0f, 18f));
        Part("GuardCore", holder.transform, new Vector3(0f, -0.01f, 0.08f), new Vector3(0.30f, 0.065f, 0.13f), light, new Vector3(0f, 0f, 8f));
        Part("GuardDark", holder.transform, new Vector3(0f, -0.02f, 0.03f), new Vector3(0.22f, 0.085f, 0.11f), dark);
        Part("GuardAccentLeft", holder.transform, new Vector3(-0.16f, 0.002f, 0.08f), new Vector3(0.11f, 0.020f, 0.05f), blue, new Vector3(0f, 0f, -16f));
        Part("GuardAccentRight", holder.transform, new Vector3(0.16f, 0.002f, 0.08f), new Vector3(0.11f, 0.020f, 0.05f), violet, new Vector3(0f, 0f, 16f));
        PrimitivePart("Grip", holder.transform, PrimitiveType.Cylinder, new Vector3(0f, -0.03f, -0.18f), new Vector3(0.065f, 0.18f, 0.065f), dark, new Vector3(90f, 0f, 0f));
        PrimitivePart("PommelCore", holder.transform, PrimitiveType.Sphere, new Vector3(0f, -0.03f, -0.39f), new Vector3(0.10f, 0.10f, 0.10f), violet);

        return holder;
    }

    private static GameObject Part(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Vector3? localEuler = null)
    {
        return PrimitivePart(name, parent, PrimitiveType.Cube, localPos, localScale, mat, localEuler);
    }

    private static GameObject PrimitivePart(string name, Transform parent, PrimitiveType primitiveType, Vector3 localPos, Vector3 localScale, Material mat, Vector3? localEuler = null)
    {
        GameObject go = GameObject.CreatePrimitive(primitiveType);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.transform.localEulerAngles = localEuler ?? Vector3.zero;

        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = mat;
        return go;
    }

    private static Material SharedOrFallback(string assetName, Color color, float smoothness, float emission)
    {
        Material shared = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + assetName + ".mat");
        return shared != null ? shared : MakeMaterial(color, smoothness, emission);
    }

    private static Material MakeMaterial(Color color, float smoothness, float emission)
    {
        Shader shader = ResolveViewmodelShader();
        if (shader == null) return null;

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (emission > 0f && mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emission);
        }
        return mat;
    }

    private static Shader ResolveViewmodelShader()
    {
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        string typeName = pipeline != null ? pipeline.GetType().FullName : string.Empty;
        if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Universal", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp != null && urp.isSupported) return urp;
        }
        if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("HDRenderPipeline", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader hdrp = Shader.Find("HDRP/Lit");
            if (hdrp != null && hdrp.isSupported) return hdrp;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null && standard.isSupported) return standard;
        Shader diffuse = Shader.Find("Legacy Shaders/Diffuse");
        if (diffuse != null && diffuse.isSupported) return diffuse;
        return Shader.Find("Unlit/Color");
    }
}
