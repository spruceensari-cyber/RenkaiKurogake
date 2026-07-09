using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

public static class RenkaiV25Installer
{
    [MenuItem("Renkai/V2.5 Install Bot AI + Pink Fix + Better Spawns")]
    public static void Install()
    {
        FixSceneMaterials();
        CreateSafetyArena();
        GameObject player = PreparePlayer();
        CreateBombAndRound();
        CreateTeamSetups();
        CreateHUD(player);
        EditorUtility.DisplayDialog("Renkai V2.5", "Bot AI + Pink Fix + Better Spawns kuruldu.\n\nPlay'e bas.\nEnemy botlar sana ateş edecek.", "OK");
    }

    static void FixSceneMaterials()
    {
        foreach (Renderer r in Object.FindObjectsOfType<Renderer>(true))
        {
            if (r == null) continue;
            if (r.sharedMaterial == null || r.sharedMaterial.shader == null || (r.sharedMaterial.shader.name != null && r.sharedMaterial.shader.name.Contains("Hidden/InternalErrorShader")))
            {
                r.sharedMaterial = MakeMaterialForName(r.gameObject.name);
                continue;
            }
            // Force named map objects away from magenta-looking imports.
            if (NeedsSceneStyle(r.gameObject.name))
                r.sharedMaterial = MakeMaterialForName(r.gameObject.name);
        }
    }

    static bool NeedsSceneStyle(string n)
    {
        n = n.ToLower();
        return n.Contains("site") || n.Contains("mid") || n.Contains("wall") || n.Contains("floor") || n.Contains("bound") || n.Contains("cover") || n.Contains("shrine") || n.Contains("ring") || n.Contains("neon") || n.Contains("reactor") || n.Contains("dummy");
    }

    static Material MakeMaterialForName(string name)
    {
        string n = name.ToLower();
        Color c = new Color(.04f,.05f,.09f);
        float emission = 0f;
        if (n.Contains("a_site") || n.Contains("site_a") || (n.Contains("a") && n.Contains("site"))) { c = new Color(.25f,.09f,.35f); emission = .8f; }
        else if (n.Contains("b_site") || n.Contains("site_b") || (n.Contains("b") && n.Contains("site"))) { c = new Color(.08f,.16f,.35f); emission = .8f; }
        else if (n.Contains("mid")) { c = new Color(.08f,.08f,.18f); emission = .25f; }
        else if (n.Contains("ring") || n.Contains("neon") || n.Contains("portal")) { c = new Color(.67f,.12f,1f); emission = 2.5f; }
        else if (n.Contains("wall") || n.Contains("bound")) c = new Color(.03f,.035f,.06f);
        else if (n.Contains("floor")) c = new Color(.05f,.06f,.10f);
        else if (n.Contains("cover")) c = new Color(.06f,.06f,.075f);
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * emission);
        return m;
    }

    static void CreateSafetyArena()
    {
        CreateCube("V2_5_Safety_Floor", new Vector3(0,-.25f,0), new Vector3(170,.35f,170), new Color(.03f,.035f,.06f));
        CreateCube("V2_5_Bound_North", new Vector3(0,4,82), new Vector3(170,8,4), new Color(.03f,.035f,.06f));
        CreateCube("V2_5_Bound_South", new Vector3(0,4,-82), new Vector3(170,8,4), new Color(.03f,.035f,.06f));
        CreateCube("V2_5_Bound_West", new Vector3(-82,4,0), new Vector3(4,8,170), new Color(.03f,.035f,.06f));
        CreateCube("V2_5_Bound_East", new Vector3(82,4,0), new Vector3(4,8,170), new Color(.03f,.035f,.06f));
    }

    static GameObject PreparePlayer()
    {
        GameObject p = GameObject.Find("Demo_Player_Renkai_Controller");
        if (p == null) p = GameObject.Find("Demo_Player_FPS_Controller");
        if (p == null)
        {
            EditorUtility.DisplayDialog("Renkai V2.5", "Önce çalışan Renkai sahnesini generate et.", "OK");
            return null;
        }
        p.tag = "Player";
        RenkaiRoundPlayer rp = p.GetComponent<RenkaiRoundPlayer>(); if (rp == null) rp = p.AddComponent<RenkaiRoundPlayer>();
        rp.agentName = "You / Empress of Violet"; rp.team = RenkaiTeam.Attackers; rp.isHumanPlayer = true; rp.RememberSpawn();
        if (p.GetComponent<RenkaiHealth>() == null) p.AddComponent<RenkaiHealth>();
        if (p.GetComponent<BombPlanter>() == null) p.AddComponent<BombPlanter>();
        RenkaiWeaponController wc = p.GetComponent<RenkaiWeaponController>(); if (wc == null) wc = p.AddComponent<RenkaiWeaponController>();
        wc.playerCamera = p.GetComponentInChildren<Camera>();
        SetupViewModels(p, wc);
        return p;
    }

    static void SetupViewModels(GameObject p, RenkaiWeaponController wc)
    {
        Camera cam = p.GetComponentInChildren<Camera>(); if (cam == null) return;
        wc.rifleView = CreateView("V25_Rifle_View", cam.transform, new Vector3(.38f,-.28f,.65f), new Vector3(.22f,.16f,.9f), new Color(.07f,.07f,.11f));
        wc.pistolView = CreateView("V25_Pistol_View", cam.transform, new Vector3(.30f,-.24f,.52f), new Vector3(.14f,.12f,.42f), new Color(.07f,.07f,.11f));
        wc.swordView = CreateView("V25_Sword_View", cam.transform, new Vector3(-.34f,-.25f,.70f), new Vector3(.08f,.08f,1.05f), new Color(.20f,.55f,1f));
    }

    static GameObject CreateView(string name, Transform parent, Vector3 pos, Vector3 scale, Color c)
    {
        Transform old = parent.Find(name); if (old != null) Object.DestroyImmediate(old.gameObject);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent); go.transform.localPosition = pos; go.transform.localRotation = Quaternion.identity; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = SimpleMat(c, c * 0.6f);
        return go;
    }

    static void CreateBombAndRound()
    {
        GameObject oldBomb = GameObject.Find("V2_5_Bomb_Core"); if (oldBomb != null) Object.DestroyImmediate(oldBomb);
        GameObject bombObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bombObj.name = "V2_5_Bomb_Core"; bombObj.transform.position = new Vector3(-42,.25f,15); bombObj.transform.localScale = new Vector3(.5f,.18f,.5f);
        bombObj.GetComponent<Renderer>().sharedMaterial = SimpleMat(new Color(.67f,.12f,1f), new Color(.67f,.12f,1f) * 2f);
        RenkaiBombCore bomb = bombObj.AddComponent<RenkaiBombCore>();
        bombObj.SetActive(false);

        GameObject oldMgr = GameObject.Find("V2_5_Round_Manager"); if (oldMgr != null) Object.DestroyImmediate(oldMgr);
        GameObject mgrObj = new GameObject("V2_5_Round_Manager");
        RenkaiRoundManager mgr = mgrObj.AddComponent<RenkaiRoundManager>();
        mgr.bomb = bomb;

        EnsureSiteTrigger("A_Site_Trigger", new Vector3(-42,1.5f,15), new Vector3(24,3,22), "A");
        EnsureSiteTrigger("B_Site_Trigger", new Vector3(42,1.5f,15), new Vector3(22,3,20), "B");
    }

    static void EnsureSiteTrigger(string name, Vector3 pos, Vector3 size, string site)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.position = pos;
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = true; col.size = size;
            BombSiteZone zone = go.AddComponent<BombSiteZone>();
            zone.siteName = site;
        }
    }

    static void CreateTeamSetups()
    {
        GameObject old = GameObject.Find("V2_5_Teams"); if (old != null) Object.DestroyImmediate(old);
        GameObject root = new GameObject("V2_5_Teams");
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Renkai/Characters/EmpressOfViolet/EmpressOfViolet_Player.glb");

        Vector3[] allies = new Vector3[] {
            new Vector3(-7f, 0.05f, -62f),
            new Vector3(-3.5f, 0.05f, -64f),
            new Vector3(3.5f, 0.05f, -64f),
            new Vector3(7f, 0.05f, -62f)
        };
        for (int i = 0; i < allies.Length; i++) SpawnBot("ALLY_" + (i+1), RenkaiTeam.Attackers, allies[i], root.transform, model, new Color(.18f,.55f,1f), true);

        Vector3[] enemies = new Vector3[] {
            new Vector3(-44f, 0.05f, 16f), // A site
            new Vector3(-16f, 0.05f, 3f),  // A connector
            new Vector3(0f, 0.05f, 6f),    // mid
            new Vector3(16f, 0.05f, 3f),   // B connector
            new Vector3(44f, 0.05f, 16f)   // B site
        };
        for (int i = 0; i < enemies.Length; i++) SpawnBot("ENEMY_" + (i+1), RenkaiTeam.Defenders, enemies[i], root.transform, model, new Color(1f,.22f,.45f), false);
    }

    static void SpawnBot(string name, RenkaiTeam team, Vector3 pos, Transform parent, GameObject model, Color teamColor, bool ally)
    {
        GameObject agent = new GameObject(name);
        agent.transform.SetParent(parent);
        agent.transform.position = pos;
        agent.transform.rotation = Quaternion.Euler(0, team == RenkaiTeam.Attackers ? 0 : 180, 0);

        // visual base ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "TeamRing"; ring.transform.SetParent(agent.transform); ring.transform.localPosition = new Vector3(0,.03f,0); ring.transform.localScale = new Vector3(.8f,.03f,.8f);
        ring.GetComponent<Renderer>().sharedMaterial = SimpleMat(teamColor, teamColor * 1.5f);

        GameObject hitbox = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hitbox.name = "GameplayHitbox"; hitbox.transform.SetParent(agent.transform); hitbox.transform.localPosition = new Vector3(0,1f,0); hitbox.transform.localScale = new Vector3(.8f,1f,.8f);
        hitbox.GetComponent<Renderer>().sharedMaterial = SimpleMat(new Color(teamColor.r,teamColor.g,teamColor.b,.15f), teamColor * .15f);
        RenkaiHealth health = hitbox.AddComponent<RenkaiHealth>();
        health.maxHealth = 100f;

        if (model != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "VisualModel";
            visual.transform.SetParent(agent.transform);
            visual.transform.localPosition = new Vector3(0, .05f, 0);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * .92f;
            ForceModelMaterials(visual, ally ? new Color(.30f,.40f,.70f) : new Color(.70f,.25f,.45f));
        }

        GameObject muzzle = new GameObject("BotMuzzle");
        muzzle.transform.SetParent(agent.transform); muzzle.transform.localPosition = new Vector3(0,1.4f,.6f);

        TextMesh tm = agent.AddComponent<TextMesh>();
        tm.text = name; tm.characterSize = .18f; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = teamColor;
        agent.transform.position = pos;
        tm.transform.localPosition = new Vector3(0,2.4f,0);

        RenkaiRoundPlayer rp = agent.AddComponent<RenkaiRoundPlayer>();
        rp.agentName = name; rp.team = team; rp.isHumanPlayer = false; rp.RememberSpawn();

        RenkaiBotAI bot = agent.AddComponent<RenkaiBotAI>();
        bot.team = team; bot.muzzle = muzzle.transform; bot.damage = ally ? 8f : 10f; bot.fireRate = ally ? 2.1f : 2.6f; bot.fireDistance = 30f; bot.viewDistance = 42f; bot.moveSpeed = ally ? 2.2f : 2.8f;
    }

    static void ForceModelMaterials(GameObject root, Color tint)
    {
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            r.sharedMaterial = SimpleMat(tint, tint * 0.3f);
        }
    }

    static void CreateHUD(GameObject player)
    {
        GameObject old = GameObject.Find("V2_5_Round_HUD"); if (old != null) Object.DestroyImmediate(old);
        GameObject canvasObj = new GameObject("V2_5_Round_HUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>(); canvasObj.AddComponent<GraphicRaycaster>();
        Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        Text status = MakeText(canvasObj.transform, "Status", "BUY PHASE", 24, new Vector2(.5f,1f), new Vector2(0,-42), new Vector2(800,46), TextAnchor.MiddleCenter, f);
        Text score = MakeText(canvasObj.transform, "Score", "ATK 0 - 0 DEF", 22, new Vector2(.5f,1f), new Vector2(0,-80), new Vector2(800,40), TextAnchor.MiddleCenter, f);
        Text timer = MakeText(canvasObj.transform, "Timer", "ROUND 95", 22, new Vector2(.5f,1f), new Vector2(0,-116), new Vector2(800,40), TextAnchor.MiddleCenter, f);
        Text ammo = MakeText(canvasObj.transform, "Ammo", "30 / 120", 24, new Vector2(1f,0f), new Vector2(-120,40), new Vector2(220,56), TextAnchor.MiddleRight, f);
        Text weapon = MakeText(canvasObj.transform, "Weapon", "Rifle", 18, new Vector2(1f,0f), new Vector2(-120,78), new Vector2(220,40), TextAnchor.MiddleRight, f);
        Text hit = MakeText(canvasObj.transform, "Hit", "HIT", 24, new Vector2(.5f,.5f), new Vector2(0,42), new Vector2(160,50), TextAnchor.MiddleCenter, f); hit.enabled = false;
        Text controls = MakeText(canvasObj.transform, "Controls", "1 Rifle | 2 Pistol | 3 Sword | Mouse1 Fire | R Reload | V Slash | F Plant/Defuse", 16, new Vector2(.5f,0f), new Vector2(0,32), new Vector2(1100,40), TextAnchor.MiddleCenter, f);
        foreach (Text t in canvasObj.GetComponentsInChildren<Text>(true)) t.color = new Color(.88f,.78f,1f,1f);

        if (player != null)
        {
            RenkaiWeaponController wc = player.GetComponent<RenkaiWeaponController>();
            if (wc != null) { wc.ammoText = ammo; wc.hitText = hit; wc.weaponText = weapon; }
        }
        RenkaiRoundManager mgr = Object.FindObjectOfType<RenkaiRoundManager>();
        if (mgr != null) { mgr.statusText = status; mgr.scoreText = score; mgr.timerText = timer; }
    }

    static Text MakeText(Transform parent, string name, string value, int size, Vector2 anchor, Vector2 pos, Vector2 dims, TextAnchor align, Font font)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent);
        Text t = go.AddComponent<Text>(); t.text = value; t.font = font; t.fontSize = size; t.alignment = align;
        RectTransform rt = go.GetComponent<RectTransform>(); rt.anchorMin = anchor; rt.anchorMax = anchor; rt.anchoredPosition = pos; rt.sizeDelta = dims;
        return t;
    }

    static GameObject CreateCube(string name, Vector3 pos, Vector3 scale, Color c)
    {
        GameObject old = GameObject.Find(name); if (old != null) Object.DestroyImmediate(old);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = SimpleMat(c, Color.black);
        return go;
    }

    static Material SimpleMat(Color c, Color emission)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
        return m;
    }
}
