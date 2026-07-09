
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

public static class RenkaiV26Installer
{
    [MenuItem("Renkai/V2.6 Install Serious Game Mode")]
    public static void Install()
    {
        FixMaterials();
        CreateArenaBounds();
        GameObject player = PreparePlayer();
        CreateBombAndRound();
        CreateTeams();
        CreateHUD(player);

        EditorUtility.DisplayDialog(
            "Renkai V2.6",
            "Serious Game Mode kuruldu.\n\nBotlar iki tarafta da ateş eder.\nHP barlar ve ciddi HUD eklendi.",
            "OK"
        );
    }

    private static void FixMaterials()
    {
        foreach (Renderer r in Object.FindObjectsOfType<Renderer>(true))
        {
            if (r == null) continue;

            string n = r.gameObject.name.ToLower();

            if (n.Contains("floor") || n.Contains("site") || n.Contains("wall") ||
                n.Contains("bound") || n.Contains("mid") || n.Contains("cover") ||
                n.Contains("neon") || n.Contains("ring"))
            {
                r.sharedMaterial = MakeMapMaterial(r.gameObject.name);
            }
        }
    }

    private static Material MakeMapMaterial(string name)
    {
        string n = name.ToLower();
        Color c = new Color(0.04f, 0.05f, 0.09f);
        float e = 0f;

        if (n.Contains("a_site") || n.Contains("site_a"))
        {
            c = new Color(0.22f, 0.08f, 0.35f);
            e = 0.6f;
        }
        else if (n.Contains("b_site") || n.Contains("site_b"))
        {
            c = new Color(0.08f, 0.15f, 0.35f);
            e = 0.6f;
        }
        else if (n.Contains("neon") || n.Contains("ring") || n.Contains("portal"))
        {
            c = new Color(0.67f, 0.12f, 1f);
            e = 2.6f;
        }
        else if (n.Contains("wall") || n.Contains("bound"))
        {
            c = new Color(0.025f, 0.03f, 0.055f);
        }
        else if (n.Contains("floor"))
        {
            c = new Color(0.045f, 0.052f, 0.085f);
        }

        return SimpleMaterial(c, c * e);
    }

    private static void CreateArenaBounds()
    {
        CreateCube("V2_6_Safety_Floor", new Vector3(0, -0.25f, 0), new Vector3(170, 0.35f, 170), new Color(0.03f, 0.035f, 0.06f), 0f);
        CreateCube("V2_6_Bound_North", new Vector3(0, 4, 82), new Vector3(170, 8, 4), new Color(0.025f, 0.03f, 0.055f), 0f);
        CreateCube("V2_6_Bound_South", new Vector3(0, 4, -82), new Vector3(170, 8, 4), new Color(0.025f, 0.03f, 0.055f), 0f);
        CreateCube("V2_6_Bound_West", new Vector3(-82, 4, 0), new Vector3(4, 8, 170), new Color(0.025f, 0.03f, 0.055f), 0f);
        CreateCube("V2_6_Bound_East", new Vector3(82, 4, 0), new Vector3(4, 8, 170), new Color(0.025f, 0.03f, 0.055f), 0f);
    }

    private static GameObject PreparePlayer()
    {
        GameObject player = GameObject.Find("Demo_Player_Renkai_Controller");
        if (player == null) player = GameObject.Find("Demo_Player_FPS_Controller");

        if (player == null)
        {
            EditorUtility.DisplayDialog("Renkai V2.6", "Player bulunamadı. Önce çalışan Renkai sahnesini generate et.", "OK");
            return null;
        }

        player.tag = "Player";

        RenkaiRoundPlayer rp = player.GetComponent<RenkaiRoundPlayer>();
        if (rp == null) rp = player.AddComponent<RenkaiRoundPlayer>();

        rp.agentName = "YOU";
        rp.team = RenkaiTeam.Attackers;
        rp.isHumanPlayer = true;
        rp.RememberSpawn();

        if (player.GetComponent<RenkaiHealth>() == null) player.AddComponent<RenkaiHealth>();
        if (player.GetComponent<BombPlanter>() == null) player.AddComponent<BombPlanter>();

        RenkaiWeaponController weapon = player.GetComponent<RenkaiWeaponController>();
        if (weapon == null) weapon = player.AddComponent<RenkaiWeaponController>();

        weapon.playerCamera = player.GetComponentInChildren<Camera>();
        SetupViewModels(player, weapon);

        return player;
    }

    private static void SetupViewModels(GameObject player, RenkaiWeaponController weapon)
    {
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam == null) return;

        weapon.rifleView = CreateView("V26_Rifle_View", cam.transform, new Vector3(0.38f, -0.28f, 0.65f), new Vector3(0.22f, 0.16f, 0.9f), new Color(0.05f, 0.05f, 0.08f), new Color(0.6f, 0.1f, 1f));
        weapon.pistolView = CreateView("V26_Pistol_View", cam.transform, new Vector3(0.30f, -0.24f, 0.52f), new Vector3(0.14f, 0.12f, 0.42f), new Color(0.05f, 0.05f, 0.08f), new Color(0.2f, 0.4f, 1f));
        weapon.swordView = CreateView("V26_Sword_View", cam.transform, new Vector3(-0.34f, -0.25f, 0.70f), new Vector3(0.08f, 0.08f, 1.05f), new Color(0.1f, 0.35f, 1f), new Color(0.2f, 0.55f, 1f));
    }

    private static GameObject CreateView(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color, Color emission)
    {
        Transform old = parent.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(color, emission);
        return go;
    }

    private static void CreateBombAndRound()
    {
        GameObject oldBomb = GameObject.Find("V2_6_Bomb_Core");
        if (oldBomb != null) Object.DestroyImmediate(oldBomb);

        GameObject bombObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bombObject.name = "V2_6_Bomb_Core";
        bombObject.transform.position = new Vector3(-42, 0.25f, 15);
        bombObject.transform.localScale = new Vector3(0.5f, 0.18f, 0.5f);
        bombObject.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(new Color(0.67f, 0.12f, 1f), new Color(0.67f, 0.12f, 1f) * 2f);

        RenkaiBombCore bomb = bombObject.AddComponent<RenkaiBombCore>();
        bombObject.SetActive(false);

        GameObject oldManager = GameObject.Find("V2_6_Round_Manager");
        if (oldManager != null) Object.DestroyImmediate(oldManager);

        GameObject managerObject = new GameObject("V2_6_Round_Manager");
        RenkaiRoundManager manager = managerObject.AddComponent<RenkaiRoundManager>();
        manager.bomb = bomb;

        EnsureSiteTrigger("A_Site_Trigger", new Vector3(-42, 1.5f, 15), new Vector3(24, 3, 22), "A");
        EnsureSiteTrigger("B_Site_Trigger", new Vector3(42, 1.5f, 15), new Vector3(22, 3, 20), "B");
    }

    private static void EnsureSiteTrigger(string name, Vector3 pos, Vector3 size, string site)
    {
        GameObject go = GameObject.Find(name);

        if (go == null)
        {
            go = new GameObject(name);
            go.transform.position = pos;
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            BombSiteZone zone = go.AddComponent<BombSiteZone>();
            zone.siteName = site;
        }
    }

    private static void CreateTeams()
    {
        GameObject old = GameObject.Find("V2_6_Serious_Teams");
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject("V2_6_Serious_Teams");

        Vector3[] allies =
        {
            new Vector3(-7f, 0.05f, -62f),
            new Vector3(-3.5f, 0.05f, -64f),
            new Vector3(3.5f, 0.05f, -64f),
            new Vector3(7f, 0.05f, -62f)
        };

        for (int i = 0; i < allies.Length; i++)
            SpawnAgent("ALLY_" + (i + 1), RenkaiTeam.Attackers, allies[i], root.transform, new Color(0.18f, 0.55f, 1f), true);

        Vector3[] enemies =
        {
            new Vector3(-44f, 0.05f, 16f),
            new Vector3(-18f, 0.05f, 4f),
            new Vector3(0f, 0.05f, 8f),
            new Vector3(18f, 0.05f, 4f),
            new Vector3(44f, 0.05f, 16f)
        };

        for (int i = 0; i < enemies.Length; i++)
            SpawnAgent("ENEMY_" + (i + 1), RenkaiTeam.Defenders, enemies[i], root.transform, new Color(1f, 0.22f, 0.45f), false);
    }

    private static void SpawnAgent(string name, RenkaiTeam team, Vector3 position, Transform parent, Color teamColor, bool ally)
    {
        GameObject agent = new GameObject(name);
        agent.transform.SetParent(parent);
        agent.transform.position = position;
        agent.transform.rotation = Quaternion.Euler(0, team == RenkaiTeam.Attackers ? 0 : 180, 0);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(agent.transform);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        body.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(teamColor * 0.75f, teamColor * 0.4f);

        RenkaiHealth health = body.AddComponent<RenkaiHealth>();
        health.maxHealth = 100f;

        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Team_Ring";
        ring.transform.SetParent(agent.transform);
        ring.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        ring.transform.localScale = new Vector3(0.9f, 0.03f, 0.9f);
        ring.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(teamColor, teamColor * 1.4f);

        GameObject muzzle = new GameObject("Bot_Muzzle");
        muzzle.transform.SetParent(agent.transform);
        muzzle.transform.localPosition = new Vector3(0f, 1.35f, 0.65f);

        RenkaiRoundPlayer player = agent.AddComponent<RenkaiRoundPlayer>();
        player.agentName = name;
        player.team = team;
        player.isHumanPlayer = false;
        player.RememberSpawn();

        RenkaiBotAI bot = agent.AddComponent<RenkaiBotAI>();
        bot.team = team;
        bot.muzzle = muzzle.transform;
        bot.tracerColor = ally ? new Color(0.25f, 0.6f, 1f) : new Color(1f, 0.2f, 0.45f);
        bot.damage = ally ? 8f : 10f;
        bot.fireRate = ally ? 2.1f : 2.7f;
        bot.fireDistance = 34f;
        bot.viewDistance = 46f;

        CreateWorldHealthBar(agent.transform, player, teamColor);
    }

    private static void CreateWorldHealthBar(Transform parent, RenkaiRoundPlayer target, Color teamColor)
    {
        GameObject root = new GameObject("World_HP_Bar");
        root.transform.SetParent(parent);
        root.transform.localPosition = Vector3.up * 2.35f;

        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.name = "HP_Back";
        back.transform.SetParent(root.transform);
        back.transform.localPosition = Vector3.zero;
        back.transform.localScale = new Vector3(1.1f, 0.08f, 0.04f);
        back.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(Color.black, Color.black);

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "HP_Fill";
        fill.transform.SetParent(root.transform);
        fill.transform.localPosition = new Vector3(0f, 0.01f, -0.01f);
        fill.transform.localScale = new Vector3(1f, 0.065f, 0.045f);
        fill.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(teamColor, teamColor * 1.1f);

        GameObject textObj = new GameObject("Name_Text");
        textObj.transform.SetParent(root.transform);
        textObj.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = target.agentName;
        text.characterSize = 0.16f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = teamColor;

        RenkaiWorldHealthBar bar = root.AddComponent<RenkaiWorldHealthBar>();
        bar.target = target;
        bar.fill = fill.transform;
        bar.nameText = text;
    }

    private static void CreateHUD(GameObject player)
    {
        GameObject old = GameObject.Find("V2_6_HUD");
        if (old != null) Object.DestroyImmediate(old);

        GameObject canvasObject = new GameObject("V2_6_HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Text status = MakeText(canvasObject.transform, "Status", "BUY PHASE", 25, new Vector2(0.5f, 1f), new Vector2(0, -38), new Vector2(850, 44), TextAnchor.MiddleCenter, font);
        Text score = MakeText(canvasObject.transform, "Score", "ROUND 1    ATK 0 - 0 DEF", 21, new Vector2(0.5f, 1f), new Vector2(0, -78), new Vector2(850, 40), TextAnchor.MiddleCenter, font);
        Text timer = MakeText(canvasObject.transform, "Timer", "ROUND 95", 21, new Vector2(0.5f, 1f), new Vector2(0, -114), new Vector2(850, 40), TextAnchor.MiddleCenter, font);
        Text hp = MakeText(canvasObject.transform, "Player_HP", "HP 100 / 100", 22, new Vector2(0f, 0f), new Vector2(130, 42), new Vector2(260, 50), TextAnchor.MiddleLeft, font);
        Text ammo = MakeText(canvasObject.transform, "Ammo", "30 / 120", 24, new Vector2(1f, 0f), new Vector2(-120, 42), new Vector2(220, 60), TextAnchor.MiddleRight, font);
        Text weapon = MakeText(canvasObject.transform, "Weapon", "Rifle", 18, new Vector2(1f, 0f), new Vector2(-120, 82), new Vector2(220, 40), TextAnchor.MiddleRight, font);
        Text hit = MakeText(canvasObject.transform, "Hit", "HIT", 24, new Vector2(0.5f, 0.5f), new Vector2(0, 42), new Vector2(160, 50), TextAnchor.MiddleCenter, font);
        hit.enabled = false;
        Text team = MakeText(canvasObject.transform, "Team_List", "ATTACKERS 5/5\nDEFENDERS 5/5", 17, new Vector2(0f, 1f), new Vector2(130, -86), new Vector2(300, 90), TextAnchor.UpperLeft, font);
        Text kill = MakeText(canvasObject.transform, "Kill_Feed", "", 16, new Vector2(1f, 1f), new Vector2(-220, -110), new Vector2(420, 160), TextAnchor.UpperRight, font);
        MakeText(canvasObject.transform, "Controls", "1 Rifle | 2 Pistol | 3 Sword | Mouse1 Fire/Slash | R Reload | F Plant/Defuse | C/Ctrl Crouch", 15, new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(1150, 36), TextAnchor.MiddleCenter, font);

        foreach (Text t in canvasObject.GetComponentsInChildren<Text>(true))
            t.color = new Color(0.88f, 0.78f, 1f, 1f);

        RenkaiHUDController hud = canvasObject.AddComponent<RenkaiHUDController>();
        hud.statusText = status;
        hud.scoreText = score;
        hud.timerText = timer;
        hud.playerHPText = hp;
        hud.ammoText = ammo;
        hud.weaponText = weapon;
        hud.hitText = hit;
        hud.teamListText = team;
        hud.killFeedText = kill;

        if (player != null)
        {
            RenkaiWeaponController weaponController = player.GetComponent<RenkaiWeaponController>();
            if (weaponController != null)
            {
                weaponController.ammoText = ammo;
                weaponController.weaponText = weapon;
                weaponController.hitText = hit;
            }
        }

        RenkaiRoundManager manager = Object.FindObjectOfType<RenkaiRoundManager>();
        if (manager != null)
        {
            manager.statusText = status;
            manager.scoreText = score;
            manager.timerText = timer;
        }
    }

    private static Text MakeText(Transform parent, string name, string value, int size, Vector2 anchor, Vector2 pos, Vector2 dimensions, TextAnchor align, Font font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        Text text = obj.AddComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.alignment = align;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = dimensions;

        return text;
    }

    private static GameObject CreateCube(string name, Vector3 pos, Vector3 scale, Color color, float emission)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Object.DestroyImmediate(old);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(color, color * emission);
        return go;
    }

    private static Material SimpleMaterial(Color color, Color emission)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        mat.EnableKeyword("_EMISSION");

        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);

        return mat;
    }
}
