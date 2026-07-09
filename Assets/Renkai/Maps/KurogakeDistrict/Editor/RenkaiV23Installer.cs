
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

public static class RenkaiV23Installer
{
    [MenuItem("Renkai/V2.3 Install Character Reload Arena Update")]
    public static void Install()
    {
        CreateSafetyArenaFloor();
        PatchPlayer();
        CreateCharacterShowcase();
        CreateSimpleHUD();
        EditorUtility.DisplayDialog("Renkai V2.3", "Character + Reload + Arena update kuruldu.\n\nPlay'e basıp test et:\nMouse1 fire, R reload, V slash, Ctrl/C crouch.", "OK");
    }

    private static void CreateSafetyArenaFloor()
    {
        GameObject old = GameObject.Find("V2_3_Safety_Arena_Floor");
        if (old != null) Object.DestroyImmediate(old);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "V2_3_Safety_Arena_Floor";
        floor.transform.position = new Vector3(0, -0.25f, 0);
        floor.transform.localScale = new Vector3(160, 0.35f, 160);

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.02f, 0.025f, 0.04f);
        floor.GetComponent<Renderer>().sharedMaterial = mat;

        CreateWall("V2_3_Bound_North", new Vector3(0, 4, 78), new Vector3(160, 8, 4));
        CreateWall("V2_3_Bound_South", new Vector3(0, 4, -78), new Vector3(160, 8, 4));
        CreateWall("V2_3_Bound_West", new Vector3(-78, 4, 0), new Vector3(4, 8, 160));
        CreateWall("V2_3_Bound_East", new Vector3(78, 4, 0), new Vector3(4, 8, 160));
    }

    private static void CreateWall(string name, Vector3 pos, Vector3 scale)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Object.DestroyImmediate(old);

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.025f, 0.03f, 0.055f);
        wall.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void PatchPlayer()
    {
        GameObject player = GameObject.Find("Demo_Player_FPS_Controller");
        if (player == null) player = GameObject.Find("Demo_Player_Renkai_Controller");

        if (player == null)
        {
            EditorUtility.DisplayDialog("Renkai V2.3", "Player bulunamadı. Önce Renkai map scene generate et.", "OK");
            return;
        }

        Camera cam = player.GetComponentInChildren<Camera>();

        RenkaiWeaponController weapon = player.GetComponent<RenkaiWeaponController>();
        if (weapon == null) weapon = player.AddComponent<RenkaiWeaponController>();
        weapon.playerCamera = cam;

        RenkaiArenaSafety safety = player.GetComponent<RenkaiArenaSafety>();
        if (safety == null) safety = player.AddComponent<RenkaiArenaSafety>();
        safety.respawnPosition = new Vector3(0, 2, -58);

        // Add simple rifle/sword viewmodel if missing
        if (cam != null && cam.transform.Find("V2_3_Rifle") == null)
        {
            GameObject rifle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rifle.name = "V2_3_Rifle";
            rifle.transform.SetParent(cam.transform);
            rifle.transform.localPosition = new Vector3(0.38f, -0.28f, 0.65f);
            rifle.transform.localRotation = Quaternion.Euler(-6, 6, 0);
            rifle.transform.localScale = new Vector3(0.22f, 0.16f, 0.9f);

            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "V2_3_Sword_Handle";
            sword.transform.SetParent(cam.transform);
            sword.transform.localPosition = new Vector3(-0.38f, -0.32f, 0.65f);
            sword.transform.localRotation = Quaternion.Euler(0, -15, 45);
            sword.transform.localScale = new Vector3(0.08f, 0.08f, 0.85f);
        }
    }

    private static void CreateCharacterShowcase()
    {
        string[] paths = new string[]
        {
            "Assets/Renkai/Characters/NocturneEmpress/NocturneEmpress_Agree.fbx",
            "Assets/Renkai/Characters/AzureStarweaver/AzureStarweaver_Agree.fbx",
            "Assets/Renkai/Characters/AzureNightEmpress/AzureNightEmpress_Agree.fbx",
            "Assets/Renkai/Characters/VeilweaverViolet/VeilweaverViolet_Agree.fbx",
            "Assets/Renkai/Characters/CrimsonBlossomWarrior/CrimsonBlossomWarrior_Agree.fbx"
        };

        GameObject root = GameObject.Find("V2_3_Character_Showcase");
        if (root != null) Object.DestroyImmediate(root);
        root = new GameObject("V2_3_Character_Showcase");

        for (int i = 0; i < paths.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Renkai_Showcase_Character_" + (i + 1);
            instance.transform.SetParent(root.transform);
            instance.transform.position = new Vector3(-16 + i * 8, 0.1f, -64);
            instance.transform.rotation = Quaternion.Euler(0, 180, 0);
            instance.transform.localScale = Vector3.one;
        }
    }

    private static void CreateSimpleHUD()
    {
        GameObject old = GameObject.Find("V2_3_HUD_Canvas");
        if (old != null) Object.DestroyImmediate(old);

        GameObject canvasObj = new GameObject("V2_3_HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Text controls = CreateText(canvasObj.transform, "Controls", "Mouse1 Fire | R Reload | V Slash | C/Ctrl Crouch | F Plant", 16, new Vector2(0.5f, 0f), new Vector2(0, 34), new Vector2(900, 40), TextAnchor.MiddleCenter, font);
        controls.color = new Color(0.8f, 0.72f, 1f, 1f);

        Text ammo = CreateText(canvasObj.transform, "Ammo", "30 / 120", 24, new Vector2(1f, 0f), new Vector2(-120, 42), new Vector2(220, 60), TextAnchor.MiddleRight, font);
        ammo.color = new Color(0.85f, 0.72f, 1f, 1f);

        Text hit = CreateText(canvasObj.transform, "Hit", "HIT", 24, new Vector2(0.5f, 0.5f), new Vector2(0, 42), new Vector2(160, 50), TextAnchor.MiddleCenter, font);
        hit.color = new Color(1f, 0.35f, 0.95f, 1f);
        hit.enabled = false;

        GameObject player = GameObject.Find("Demo_Player_FPS_Controller");
        if (player == null) player = GameObject.Find("Demo_Player_Renkai_Controller");
        if (player != null)
        {
            RenkaiWeaponController weapon = player.GetComponent<RenkaiWeaponController>();
            if (weapon != null)
            {
                weapon.ammoText = ammo;
                weapon.hitText = hit;
            }
        }
    }

    private static Text CreateText(Transform parent, string name, string text, int size, Vector2 anchor, Vector2 pos, Vector2 dims, TextAnchor align, Font font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = size;
        t.alignment = align;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = dims;
        return t;
    }
}
