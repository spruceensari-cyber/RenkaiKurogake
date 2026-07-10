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

    [MenuItem("Renkai/Upgrade Current Scene Gameplay Feel")]
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

        Material dark = SharedOrFallback("M_DarkCeramic", new Color(0.035f, 0.045f, 0.065f), 0.72f, 0f);
        Material navy = SharedOrFallback("M_NavyMetal", new Color(0.055f, 0.08f, 0.12f), 0.62f, 0f);
        Material accent = SharedOrFallback("M_Accent_Blue", new Color(0.06f, 0.42f, 0.95f), 0.48f, 1.4f);
        Material light = SharedOrFallback("M_LightComposite", new Color(0.66f, 0.70f, 0.74f), 0.52f, 0f);

        Part("Receiver_Core", holder.transform, new Vector3(0f, 0f, 0.27f), new Vector3(0.19f, 0.17f, 0.54f), dark);
        Part("Receiver_LeftShell", holder.transform, new Vector3(-0.11f, 0.015f, 0.30f), new Vector3(0.055f, 0.13f, 0.40f), light, new Vector3(0f, -6f, 0f));
        Part("Receiver_RightTech", holder.transform, new Vector3(0.105f, 0.025f, 0.33f), new Vector3(0.045f, 0.12f, 0.28f), navy, new Vector3(0f, 7f, 0f));
        Part("UpperReceiver", holder.transform, new Vector3(0f, 0.083f, 0.33f), new Vector3(0.13f, 0.055f, 0.33f), light);
        Part("BarrelCore", holder.transform, new Vector3(0f, 0.012f, 0.82f), new Vector3(0.045f, 0.045f, 0.50f), dark);
        Part("MuzzleCrown", holder.transform, new Vector3(0f, 0.012f, 1.08f), new Vector3(0.10f, 0.10f, 0.12f), navy);
        Part("Handguard", holder.transform, new Vector3(0f, -0.005f, 0.62f), new Vector3(0.13f, 0.11f, 0.28f), light);
        Part("HandguardLower", holder.transform, new Vector3(0f, -0.083f, 0.60f), new Vector3(0.09f, 0.045f, 0.24f), navy);
        Part("Grip", holder.transform, new Vector3(0.02f, -0.17f, 0.21f), new Vector3(0.09f, 0.25f, 0.11f), dark, new Vector3(12f, 0f, 0f));
        Part("Magazine", holder.transform, new Vector3(0f, -0.15f, 0.31f), new Vector3(0.08f, 0.23f, 0.12f), light, new Vector3(10f, 0f, 0f));
        Part("MagazineEnergyWindow", holder.transform, new Vector3(0.041f, -0.15f, 0.31f), new Vector3(0.012f, 0.12f, 0.07f), accent, new Vector3(10f, 0f, 0f));
        Part("StockBridge", holder.transform, new Vector3(0f, 0.015f, -0.04f), new Vector3(0.12f, 0.08f, 0.22f), navy);
        Part("StockShoulder", holder.transform, new Vector3(0f, -0.015f, -0.22f), new Vector3(0.16f, 0.15f, 0.20f), dark, new Vector3(-4f, 0f, 0f));
        Part("BlueRail", holder.transform, new Vector3(0f, 0.125f, 0.36f), new Vector3(0.032f, 0.022f, 0.48f), accent);
        Part("SightBase", holder.transform, new Vector3(0f, 0.145f, 0.22f), new Vector3(0.11f, 0.05f, 0.13f), navy);
        Part("SightGlass", holder.transform, new Vector3(0f, 0.205f, 0.22f), new Vector3(0.07f, 0.065f, 0.015f), accent);
        return holder;
    }

    private static GameObject BuildPistolView(Transform root)
    {
        Transform old = root.Find("SHIRO_SIDEARM_VIEW");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject holder = new GameObject("SHIRO_SIDEARM_VIEW");
        holder.transform.SetParent(root, false);
        holder.transform.localPosition = new Vector3(0.06f, -0.03f, 0.08f);

        Material light = SharedOrFallback("M_LightComposite", new Color(0.66f, 0.70f, 0.74f), 0.52f, 0f);
        Material dark = SharedOrFallback("M_DarkCeramic", new Color(0.035f, 0.045f, 0.065f), 0.68f, 0f);
        Material accent = SharedOrFallback("M_Accent_Violet", new Color(0.34f, 0.16f, 0.70f), 0.48f, 1.1f);
        Material blue = SharedOrFallback("M_Accent_Blue", new Color(0.06f, 0.42f, 0.95f), 0.48f, 1.0f);

        Part("Frame", holder.transform, new Vector3(0f, -0.02f, 0.19f), new Vector3(0.13f, 0.09f, 0.38f), dark);
        Part("Slide", holder.transform, new Vector3(0f, 0.035f, 0.23f), new Vector3(0.145f, 0.10f, 0.40f), light);
        Part("SlideCutLeft", holder.transform, new Vector3(-0.078f, 0.05f, 0.30f), new Vector3(0.015f, 0.055f, 0.16f), accent, new Vector3(0f, -5f, 0f));
        Part("Barrel", holder.transform, new Vector3(0f, 0.01f, 0.48f), new Vector3(0.05f, 0.045f, 0.20f), dark);
        Part("MuzzleBlock", holder.transform, new Vector3(0f, 0.01f, 0.59f), new Vector3(0.10f, 0.08f, 0.08f), dark);
        Part("Grip", holder.transform, new Vector3(0f, -0.18f, 0.05f), new Vector3(0.105f, 0.27f, 0.13f), dark, new Vector3(11f, 0f, 0f));
        Part("Magazine", holder.transform, new Vector3(0f, -0.14f, 0.30f), new Vector3(0.055f, 0.16f, 0.07f), dark, new Vector3(7f, 0f, 0f));
        Part("EnergyChannel", holder.transform, new Vector3(0f, -0.005f, 0.27f), new Vector3(0.045f, 0.025f, 0.24f), blue);
        Part("RearSight", holder.transform, new Vector3(0f, 0.105f, 0.08f), new Vector3(0.10f, 0.035f, 0.05f), dark);
        Part("FrontSight", holder.transform, new Vector3(0f, 0.105f, 0.43f), new Vector3(0.05f, 0.035f, 0.04f), accent);
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

        Part("BladeSpine", holder.transform, new Vector3(0f, 0.01f, 0.62f), new Vector3(0.07f, 0.035f, 1.05f), dark);
        Part("BladeCore", holder.transform, new Vector3(-0.018f, 0.028f, 0.65f), new Vector3(0.026f, 0.025f, 0.92f), blue);
        Part("BladeEdge", holder.transform, new Vector3(0.044f, 0.025f, 0.67f), new Vector3(0.016f, 0.018f, 0.88f), violet, new Vector3(0f, 0f, -2f));
        Part("BladeTip", holder.transform, new Vector3(0f, 0.02f, 1.20f), new Vector3(0.045f, 0.028f, 0.14f), blue, new Vector3(0f, 0f, 18f));
        Part("GuardCore", holder.transform, new Vector3(0f, -0.01f, 0.08f), new Vector3(0.28f, 0.06f, 0.12f), light, new Vector3(0f, 0f, 8f));
        Part("GuardDark", holder.transform, new Vector3(0f, -0.02f, 0.03f), new Vector3(0.20f, 0.08f, 0.10f), dark);
        Part("Grip", holder.transform, new Vector3(0f, -0.03f, -0.16f), new Vector3(0.10f, 0.10f, 0.32f), dark);
        Part("PommelCore", holder.transform, new Vector3(0f, -0.03f, -0.35f), new Vector3(0.08f, 0.08f, 0.10f), violet);
        return holder;
    }

    private static GameObject Part(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Vector3? localEuler = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.transform.localEulerAngles = localEuler ?? Vector3.zero;
        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static Material SharedOrFallback(string assetName, Color color, float smoothness, float emission)
    {
        Material shared = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + assetName + ".mat");
        return shared != null ? shared : MakeMaterial(color, smoothness, emission);
    }

    private static Material MakeMaterial(Color color, float smoothness, float emission)
    {
        bool srpActive = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = srpActive ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Diffuse");

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
}
