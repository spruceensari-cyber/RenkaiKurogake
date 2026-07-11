using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Renkai.Kurogake;

public static class KurokageBotWeaponVisualInstaller
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";

    public static bool InstallSilent()
    {
        Material dark = Load("M_DarkCeramic");
        Material light = Load("M_LightComposite");
        Material blue = Load("M_Accent_Blue");
        Material violet = Load("M_Accent_Violet");
        if (dark == null || light == null || blue == null || violet == null) return false;

        int installed = 0;
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null || player.isHumanPlayer) continue;
            RenkaiTacticalBotAI ai = player.GetComponent<RenkaiTacticalBotAI>();
            if (ai == null) continue;

            Transform visual = player.transform.Find("AGENT_VISUAL");
            if (visual == null) continue;

            Transform grip = FindDeep(visual, "RightHandGrip");
            if (grip == null)
            {
                Transform rightHand = FindDeep(visual, "righthand");
                if (rightHand == null) rightHand = visual;
                GameObject gripGo = new GameObject("RightHandGrip");
                gripGo.transform.SetParent(rightHand, false);
                gripGo.transform.localPosition = new Vector3(0.02f, -0.02f, 0.11f);
                gripGo.transform.localRotation = Quaternion.Euler(4f, 88f, -7f);
                grip = gripGo.transform;
            }

            Transform previous = grip.Find("BOT_WORLD_WEAPON");
            if (previous != null) Object.DestroyImmediate(previous.gameObject);

            GameObject weapon = new GameObject("BOT_WORLD_WEAPON");
            weapon.transform.SetParent(grip, false);
            weapon.transform.localPosition = new Vector3(0.02f, -0.02f, 0.08f);
            weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            weapon.transform.localScale = Vector3.one;

            BuildRifle(weapon.transform, dark, light, player.team == RenkaiTeam.Attackers ? blue : violet);

            GameObject muzzle = new GameObject("MuzzleSocket");
            muzzle.transform.SetParent(weapon.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.015f, 0.91f);
            muzzle.transform.localRotation = Quaternion.identity;
            ai.muzzle = muzzle.transform;

            KurokageBotWeaponPose pose = player.GetComponent<KurokageBotWeaponPose>();
            if (pose == null) pose = player.gameObject.AddComponent<KurokageBotWeaponPose>();
            pose.Configure(weapon.transform, muzzle.transform);
            installed++;
        }

        return installed == 9;
    }

    private static void BuildRifle(Transform root, Material dark, Material light, Material accent)
    {
        Part("RECEIVER", PrimitiveType.Cube, root, new Vector3(0f, 0f, 0.28f), new Vector3(0.13f, 0.11f, 0.36f), dark);
        Part("RECEIVER_TOP", PrimitiveType.Cube, root, new Vector3(0f, 0.105f, 0.28f), new Vector3(0.10f, 0.035f, 0.29f), light);
        Part("HANDGUARD", PrimitiveType.Cube, root, new Vector3(0f, 0f, 0.57f), new Vector3(0.105f, 0.09f, 0.25f), dark);
        Part("ENERGY_RAIL", PrimitiveType.Cube, root, new Vector3(0f, 0.095f, 0.54f), new Vector3(0.025f, 0.015f, 0.28f), accent);
        Part("BARREL", PrimitiveType.Cylinder, root, new Vector3(0f, 0f, 0.79f), new Vector3(0.035f, 0.18f, 0.035f), dark, new Vector3(90f, 0f, 0f));
        Part("MUZZLE", PrimitiveType.Cylinder, root, new Vector3(0f, 0f, 0.91f), new Vector3(0.055f, 0.055f, 0.055f), light, new Vector3(90f, 0f, 0f));
        Part("MAGAZINE", PrimitiveType.Cube, root, new Vector3(0f, -0.17f, 0.29f), new Vector3(0.085f, 0.17f, 0.11f), dark, new Vector3(-12f, 0f, 0f));
        Part("GRIP", PrimitiveType.Cube, root, new Vector3(0f, -0.15f, 0.10f), new Vector3(0.075f, 0.16f, 0.075f), dark, new Vector3(-8f, 0f, 0f));
        Part("STOCK", PrimitiveType.Cube, root, new Vector3(0f, 0.01f, -0.10f), new Vector3(0.12f, 0.10f, 0.22f), dark);
        Part("STOCK_PAD", PrimitiveType.Cube, root, new Vector3(0f, 0.015f, -0.29f), new Vector3(0.14f, 0.13f, 0.055f), light);
        Part("SIGHT_BASE", PrimitiveType.Cube, root, new Vector3(0f, 0.14f, 0.29f), new Vector3(0.08f, 0.025f, 0.12f), dark);
        Part("SIGHT_GLASS", PrimitiveType.Cube, root, new Vector3(0f, 0.19f, 0.30f), new Vector3(0.045f, 0.045f, 0.012f), accent);
    }

    private static GameObject Part(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3? localEuler = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler ?? Vector3.zero);
        go.transform.localScale = localScale;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
        return go;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static Material Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + name + ".mat");
    }
}
