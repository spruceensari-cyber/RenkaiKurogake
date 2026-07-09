using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

public static class RenkaiV27Installer
{
    [MenuItem("Renkai/V2.7 Install Autonomous Bots + Agent Visuals")]
    public static void Install()
    {
        ClearOldTeams();
        FixMaterialMood();
        GameObject player = PreparePlayer();
        CreateAutonomousTeams();
        UpgradeHUDReferences(player);
        EditorUtility.DisplayDialog(
            "Renkai V2.7",
            "Autonomous Tactical Bots + Agent Visuals kuruldu.\n\nBotlar artık seni duvar arkasından bilmez; görürse veya hedefe yaklaşırsa tepki verir.",
            "OK"
        );
    }

    private static void ClearOldTeams()
    {
        string[] oldNames =
        {
            "V2_4_5v5_Teams",
            "V2_5_Teams",
            "V2_6_Serious_Teams",
            "V2_7_Autonomous_Teams"
        };

        foreach (string n in oldNames)
        {
            GameObject go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    private static void FixMaterialMood()
    {
        foreach (Renderer r in Object.FindObjectsOfType<Renderer>(true))
        {
            if (r == null) continue;

            string n = r.gameObject.name.ToLower();

            if (n.Contains("floor") || n.Contains("wall") || n.Contains("site") || n.Contains("mid") ||
                n.Contains("bound") || n.Contains("cover") || n.Contains("neon") || n.Contains("ring"))
            {
                r.sharedMaterial = MapMaterial(n);
            }
        }
    }

    private static Material MapMaterial(string name)
    {
        Color color = new Color(0.04f, 0.05f, 0.09f);
        Color emission = Color.black;

        if (name.Contains("a_site"))
        {
            color = new Color(0.18f, 0.07f, 0.28f);
            emission = color * 0.45f;
        }
        else if (name.Contains("b_site"))
        {
            color = new Color(0.07f, 0.13f, 0.30f);
            emission = color * 0.45f;
        }
        else if (name.Contains("neon") || name.Contains("ring") || name.Contains("portal"))
        {
            color = new Color(0.62f, 0.12f, 1f);
            emission = color * 2.4f;
        }
        else if (name.Contains("wall") || name.Contains("bound"))
        {
            color = new Color(0.025f, 0.03f, 0.055f);
        }

        return SimpleMaterial(color, emission);
    }

    private static GameObject PreparePlayer()
    {
        GameObject player = GameObject.Find("Demo_Player_Renkai_Controller");
        if (player == null) player = GameObject.Find("Demo_Player_FPS_Controller");

        if (player == null)
        {
            EditorUtility.DisplayDialog("Renkai V2.7", "Player bulunamadı. Önce çalışan Renkai sahnesini generate et.", "OK");
            return null;
        }

        RenkaiRoundPlayer rp = player.GetComponent<RenkaiRoundPlayer>();
        if (rp == null) rp = player.AddComponent<RenkaiRoundPlayer>();

        rp.agentName = "YOU";
        rp.team = RenkaiTeam.Attackers;
        rp.isHumanPlayer = true;
        rp.RememberSpawn();

        return player;
    }

    private static void CreateAutonomousTeams()
    {
        GameObject root = new GameObject("V2_7_Autonomous_Teams");

        SpawnAgent("ALLY_DUELIST", RenkaiTeam.Attackers, RenkaiAgentRole.Duelist, new Vector3(-8f, 0.05f, -62f), root.transform, new Color(0.22f, 0.58f, 1f), true);
        SpawnAgent("ALLY_CONTROLLER", RenkaiTeam.Attackers, RenkaiAgentRole.Controller, new Vector3(-4f, 0.05f, -64f), root.transform, new Color(0.15f, 0.45f, 1f), true);
        SpawnAgent("ALLY_INITIATOR", RenkaiTeam.Attackers, RenkaiAgentRole.Initiator, new Vector3(4f, 0.05f, -64f), root.transform, new Color(0.30f, 0.70f, 1f), true);
        SpawnAgent("ALLY_BLADE", RenkaiTeam.Attackers, RenkaiAgentRole.Blade, new Vector3(8f, 0.05f, -62f), root.transform, new Color(0.10f, 0.85f, 1f), true);

        SpawnAgent("ENEMY_A_HOLDER", RenkaiTeam.Defenders, RenkaiAgentRole.Sentinel, new Vector3(-44f, 0.05f, 16f), root.transform, new Color(1f, 0.20f, 0.42f), false);
        SpawnAgent("ENEMY_A_CON", RenkaiTeam.Defenders, RenkaiAgentRole.Controller, new Vector3(-18f, 0.05f, 4f), root.transform, new Color(1f, 0.28f, 0.55f), false);
        SpawnAgent("ENEMY_MID", RenkaiTeam.Defenders, RenkaiAgentRole.Initiator, new Vector3(0f, 0.05f, 8f), root.transform, new Color(1f, 0.22f, 0.48f), false);
        SpawnAgent("ENEMY_B_CON", RenkaiTeam.Defenders, RenkaiAgentRole.Duelist, new Vector3(18f, 0.05f, 4f), root.transform, new Color(1f, 0.30f, 0.60f), false);
        SpawnAgent("ENEMY_B_HOLDER", RenkaiTeam.Defenders, RenkaiAgentRole.Blade, new Vector3(44f, 0.05f, 16f), root.transform, new Color(1f, 0.15f, 0.40f), false);
    }

    private static void SpawnAgent(string name, RenkaiTeam team, RenkaiAgentRole role, Vector3 position, Transform parent, Color teamColor, bool ally)
    {
        GameObject agent = new GameObject(name);
        agent.transform.SetParent(parent);
        agent.transform.position = position;
        agent.transform.rotation = Quaternion.Euler(0, team == RenkaiTeam.Attackers ? 0 : 180, 0);

        CreateStylizedAgentBody(agent.transform, teamColor, role);

        RenkaiRoundPlayer roundPlayer = agent.AddComponent<RenkaiRoundPlayer>();
        roundPlayer.agentName = name;
        roundPlayer.team = team;
        roundPlayer.isHumanPlayer = false;
        roundPlayer.RememberSpawn();

        RenkaiTacticalBotAI bot = agent.AddComponent<RenkaiTacticalBotAI>();
        bot.team = team;
        bot.role = role;
        bot.callSign = name;
        bot.tracerColor = ally ? new Color(0.25f, 0.65f, 1f) : new Color(1f, 0.18f, 0.45f);
        bot.damage = ally ? 8.5f : 10f;
        bot.fireRate = ally ? 2.2f : 2.7f;
        bot.accuracy = ally ? 0.62f : 0.72f;
        bot.viewDistance = 44f;
        bot.fireDistance = 34f;
        bot.fieldOfView = team == RenkaiTeam.Defenders ? 115f : 100f;

        Transform muzzle = agent.transform.Find("Muzzle");
        if (muzzle != null) bot.muzzle = muzzle;

        CreateWorldHealthBar(agent.transform, roundPlayer, teamColor);
    }

    private static void CreateStylizedAgentBody(Transform parent, Color color, RenkaiAgentRole role)
    {
        GameObject root = new GameObject("Agent_Visual");
        root.transform.SetParent(parent);
        root.transform.localPosition = Vector3.zero;

        // Main body
        GameObject torso = Cube("Torso", root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.65f, 0.95f, 0.35f), color * 0.65f, color * 0.25f);
        GameObject chest = Cube("Chest_Neon_Core", root.transform, new Vector3(0f, 1.28f, -0.21f), new Vector3(0.38f, 0.12f, 0.05f), color, color * 1.8f);
        GameObject head = Sphere("Helmet", root.transform, new Vector3(0f, 1.9f, 0f), new Vector3(0.46f, 0.36f, 0.46f), color * 0.8f, color * 0.3f);
        GameObject visor = Cube("Visor", root.transform, new Vector3(0f, 1.92f, -0.25f), new Vector3(0.38f, 0.08f, 0.04f), Color.black, color * 2.2f);

        // Arms
        Cube("Left_Shoulder", root.transform, new Vector3(-0.48f, 1.48f, 0f), new Vector3(0.28f, 0.25f, 0.28f), color * 0.7f, color * 0.2f);
        Cube("Right_Shoulder", root.transform, new Vector3(0.48f, 1.48f, 0f), new Vector3(0.28f, 0.25f, 0.28f), color * 0.7f, color * 0.2f);
        Cube("Left_Arm", root.transform, new Vector3(-0.65f, 1.05f, -0.05f), new Vector3(0.18f, 0.55f, 0.18f), color * 0.55f, color * 0.15f);
        Cube("Right_Arm", root.transform, new Vector3(0.65f, 1.05f, -0.05f), new Vector3(0.18f, 0.55f, 0.18f), color * 0.55f, color * 0.15f);

        // Legs
        Cube("Left_Leg", root.transform, new Vector3(-0.22f, 0.45f, 0f), new Vector3(0.22f, 0.75f, 0.22f), color * 0.45f, color * 0.1f);
        Cube("Right_Leg", root.transform, new Vector3(0.22f, 0.45f, 0f), new Vector3(0.22f, 0.75f, 0.22f), color * 0.45f, color * 0.1f);

        // Weapon
        GameObject rifle = Cube("Rifle", root.transform, new Vector3(0.42f, 1.12f, -0.42f), new Vector3(0.13f, 0.10f, 0.85f), Color.black, color * 0.8f);
        rifle.transform.localRotation = Quaternion.Euler(0f, -8f, 0f);

        // Sword on back for anime/S4 League feel
        GameObject sword = Cube("Back_Sword", root.transform, new Vector3(-0.38f, 1.15f, 0.28f), new Vector3(0.07f, 0.07f, 1.15f), color * 0.9f, color * 1.5f);
        sword.transform.localRotation = Quaternion.Euler(35f, 0f, -35f);

        // Role icon
        if (role == RenkaiAgentRole.Controller)
            Cube("Role_Controller_Wings", root.transform, new Vector3(0f, 1.52f, 0.25f), new Vector3(0.95f, 0.10f, 0.08f), color, color * 1.2f);
        else if (role == RenkaiAgentRole.Blade)
            Cube("Role_Blade_Horn", root.transform, new Vector3(0f, 2.25f, 0f), new Vector3(0.08f, 0.45f, 0.08f), color, color * 1.6f);
        else if (role == RenkaiAgentRole.Sentinel)
            Cube("Role_Sentinel_Backpack", root.transform, new Vector3(0f, 1.18f, 0.35f), new Vector3(0.48f, 0.70f, 0.12f), color * 0.55f, color * 0.2f);
        else if (role == RenkaiAgentRole.Initiator)
            Cube("Role_Initiator_Headband", root.transform, new Vector3(0f, 2.08f, -0.08f), new Vector3(0.55f, 0.05f, 0.06f), color, color * 1.6f);

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(parent);
        muzzle.transform.localPosition = new Vector3(0.42f, 1.22f, -0.82f);

        CapsuleCollider collider = parent.gameObject.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 1f, 0f);
        collider.height = 2f;
        collider.radius = 0.42f;

        RenkaiHealth hp = parent.gameObject.AddComponent<RenkaiHealth>();
        hp.maxHealth = 100f;
    }

    private static void CreateWorldHealthBar(Transform parent, RenkaiRoundPlayer target, Color color)
    {
        GameObject root = new GameObject("World_HP_Bar");
        root.transform.SetParent(parent);
        root.transform.localPosition = Vector3.up * 2.45f;

        GameObject back = Cube("HP_Back", root.transform, Vector3.zero, new Vector3(1.15f, 0.08f, 0.04f), Color.black, Color.black);
        GameObject fill = Cube("HP_Fill", root.transform, new Vector3(0f, 0.01f, -0.01f), new Vector3(1f, 0.065f, 0.045f), color, color * 1.2f);

        GameObject textObj = new GameObject("Name_Text");
        textObj.transform.SetParent(root.transform);
        textObj.transform.localPosition = new Vector3(0f, 0.18f, 0f);

        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = target.agentName;
        text.characterSize = 0.16f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;

        RenkaiWorldHealthBar bar = root.AddComponent<RenkaiWorldHealthBar>();
        bar.target = target;
        bar.fill = fill.transform;
        bar.nameText = text;
    }

    private static void UpgradeHUDReferences(GameObject player)
    {
        RenkaiHUDController hud = Object.FindObjectOfType<RenkaiHUDController>();
        RenkaiRoundManager manager = Object.FindObjectOfType<RenkaiRoundManager>();

        if (hud != null && manager != null)
        {
            manager.statusText = hud.statusText;
            manager.scoreText = hud.scoreText;
            manager.timerText = hud.timerText;
        }

        if (player != null)
        {
            RenkaiWeaponController wc = player.GetComponent<RenkaiWeaponController>();
            if (wc != null && hud != null)
            {
                wc.ammoText = hud.ammoText;
                wc.weaponText = hud.weaponText;
                wc.hitText = hud.hitText;
            }
        }
    }

    private static GameObject Cube(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color, Color emission)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(color, emission);
        return go;
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color, Color emission)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = SimpleMaterial(color, emission);
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
