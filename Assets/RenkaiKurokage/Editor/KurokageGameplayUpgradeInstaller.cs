using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageGameplayUpgradeInstaller
{
    [MenuItem("Renkai/Upgrade Current Scene Gameplay Feel")]
    public static void Upgrade()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        RenkaiWeaponController weapon = Object.FindObjectOfType<RenkaiWeaponController>();
        if (weapon == null)
        {
            EditorUtility.DisplayDialog("Renkai", "Aktif sahnede RenkaiWeaponController bulunamadı.", "OK");
            return;
        }

        Camera cam = weapon.playerCamera != null ? weapon.playerCamera : weapon.GetComponentInChildren<Camera>();
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Renkai", "Player Camera bulunamadı.", "OK");
            return;
        }

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

        EditorUtility.DisplayDialog(
            "Renkai",
            "Gun feel upgrade bağlandı:\n- richer viewmodels\n- weapon sway\n- procedural reload\n- visible magazine movement\n- ADS presentation\n- dynamic crosshair\n- health/ammo HUD\n\nCtrl+S ile kaydet ve Play'e bas.",
            "OK"
        );
    }

    private static GameObject BuildRifleView(Transform root)
    {
        Transform old = root.Find("KX9_KURO_VIEW");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject holder = new GameObject("KX9_KURO_VIEW");
        holder.transform.SetParent(root, false);

        Material dark = MakeMaterial(new Color(0.055f, 0.065f, 0.085f), 0.55f, 0f);
        Material accent = MakeMaterial(new Color(0.06f, 0.42f, 0.95f), 0.45f, 0.5f);
        Material light = MakeMaterial(new Color(0.58f, 0.62f, 0.70f), 0.42f, 0f);

        Part("Receiver", holder.transform, new Vector3(0f, 0f, 0.28f), new Vector3(0.18f, 0.16f, 0.58f), dark);
        Part("UpperReceiver", holder.transform, new Vector3(0f, 0.075f, 0.34f), new Vector3(0.13f, 0.06f, 0.34f), light);
        Part("Barrel", holder.transform, new Vector3(0f, 0.015f, 0.78f), new Vector3(0.055f, 0.055f, 0.55f), dark);
        Part("Handguard", holder.transform, new Vector3(0f, 0f, 0.60f), new Vector3(0.12f, 0.11f, 0.26f), light);
        Part("Grip", holder.transform, new Vector3(0.02f, -0.16f, 0.22f), new Vector3(0.09f, 0.25f, 0.11f), dark, new Vector3(12f, 0f, 0f));
        Part("Magazine", holder.transform, new Vector3(0f, -0.14f, 0.30f), new Vector3(0.075f, 0.22f, 0.12f), light, new Vector3(8f, 0f, 0f));
        Part("Stock", holder.transform, new Vector3(0f, -0.015f, -0.14f), new Vector3(0.14f, 0.12f, 0.30f), dark);
        Part("BlueRail", holder.transform, new Vector3(0f, 0.115f, 0.35f), new Vector3(0.035f, 0.025f, 0.52f), accent);
        return holder;
    }

    private static GameObject BuildPistolView(Transform root)
    {
        Transform old = root.Find("SHIRO_SIDEARM_VIEW");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject holder = new GameObject("SHIRO_SIDEARM_VIEW");
        holder.transform.SetParent(root, false);
        holder.transform.localPosition = new Vector3(0.06f, -0.03f, 0.08f);

        Material light = MakeMaterial(new Color(0.48f, 0.52f, 0.58f), 0.5f, 0f);
        Material dark = MakeMaterial(new Color(0.05f, 0.055f, 0.07f), 0.4f, 0f);
        Material accent = MakeMaterial(new Color(0.70f, 0.34f, 0.96f), 0.55f, 0.40f);

        Part("Slide", holder.transform, new Vector3(0f, 0f, 0.20f), new Vector3(0.14f, 0.12f, 0.42f), light);
        Part("Barrel", holder.transform, new Vector3(0f, -0.01f, 0.42f), new Vector3(0.05f, 0.04f, 0.18f), dark);
        Part("Grip", holder.transform, new Vector3(0f, -0.17f, 0.05f), new Vector3(0.10f, 0.26f, 0.13f), dark, new Vector3(10f, 0f, 0f));
        Part("Magazine", holder.transform, new Vector3(0f, -0.14f, 0.30f), new Vector3(0.055f, 0.16f, 0.07f), dark, new Vector3(7f, 0f, 0f));
        Part("Accent", holder.transform, new Vector3(0f, 0.05f, 0.16f), new Vector3(0.08f, 0.025f, 0.15f), accent);
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

        Material blade = MakeMaterial(new Color(0.12f, 0.46f, 1f), 0.7f, 0.8f);
        Material grip = MakeMaterial(new Color(0.035f, 0.04f, 0.055f), 0.3f, 0f);

        Part("Blade", holder.transform, new Vector3(0f, 0.02f, 0.58f), new Vector3(0.055f, 0.025f, 1.0f), blade);
        Part("EnergyCore", holder.transform, new Vector3(0f, 0.025f, 0.42f), new Vector3(0.018f, 0.035f, 0.48f), blade);
        Part("Grip", holder.transform, new Vector3(0f, -0.03f, -0.05f), new Vector3(0.09f, 0.09f, 0.28f), grip);
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
