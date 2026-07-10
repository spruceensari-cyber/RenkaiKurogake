using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;

public static class KurokageFiveVFiveInstaller
{
    [MenuItem("Renkai/Install Competitive 5v5 Test Match")]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        GameObject old = GameObject.Find("KUROKAGE_5V5_TEST_MATCH");
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject("KUROKAGE_5V5_TEST_MATCH");
        Transform attackers = Group(root.transform, "ATTACKERS");
        Transform defenders = Group(root.transform, "DEFENDERS");

        RenkaiFPSController humanFps = Object.FindObjectOfType<RenkaiFPSController>();
        if (humanFps == null)
        {
            EditorUtility.DisplayDialog("Renkai", "Önce birleşik sahneyi oluştur. Human player bulunamadı.", "OK");
            return;
        }

        GameObject human = humanFps.gameObject;
        RenkaiRoundPlayer humanRound = human.GetComponent<RenkaiRoundPlayer>();
        if (humanRound == null) humanRound = human.AddComponent<RenkaiRoundPlayer>();
        humanRound.agentName = "KAIRI // RIFT-07";
        humanRound.team = RenkaiTeam.Attackers;
        humanRound.isHumanPlayer = true;
        humanRound.RememberSpawn();

        Vector3[] atkSpawns =
        {
            new Vector3(-7f, 1f, -60f),
            new Vector3(-3.5f, 1f, -63f),
            new Vector3(3.5f, 1f, -63f),
            new Vector3(7f, 1f, -60f)
        };

        Vector3[] defSpawns =
        {
            new Vector3(-42f, 1f, 34f),
            new Vector3(-26f, 1f, 34f),
            new Vector3(0f, 1f, 38f),
            new Vector3(26f, 1f, 34f),
            new Vector3(42f, 1f, 34f)
        };

        string[] atkNames = { "RIFT-A2", "RIFT-A3", "RIFT-A4", "RIFT-A5" };
        string[] defNames = { "PULSE-D1", "PULSE-D2", "PULSE-D3", "PULSE-D4", "PULSE-D5" };

        for (int i = 0; i < atkSpawns.Length; i++)
            CreateBot(atkNames[i], RenkaiTeam.Attackers, atkSpawns[i], attackers, new Color(0.15f, 0.48f, 1f));

        for (int i = 0; i < defSpawns.Length; i++)
            CreateBot(defNames[i], RenkaiTeam.Defenders, defSpawns[i], defenders, new Color(0.72f, 0.24f, 0.82f));

        if (Object.FindObjectOfType<RenkaiRoundManager>() == null)
        {
            GameObject managerGo = new GameObject("ROUND_MANAGER");
            managerGo.transform.SetParent(root.transform);
            managerGo.AddComponent<RenkaiRoundManager>();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog(
            "Renkai",
            "5v5 test match kuruldu:\n1 human + 4 attacker bot\n5 defender bot\nround manager\nteam identities\n\nCtrl+S ile kaydet.",
            "OK"
        );
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private static void CreateBot(string callSign, RenkaiTeam team, Vector3 position, Transform parent, Color color)
    {
        GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bot.name = callSign;
        bot.transform.SetParent(parent);
        bot.transform.position = position;
        bot.transform.localScale = new Vector3(0.78f, 1f, 0.78f);

        CapsuleCollider primitiveCollider = bot.GetComponent<CapsuleCollider>();
        if (primitiveCollider != null) Object.DestroyImmediate(primitiveCollider);

        CharacterController cc = bot.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.38f;
        cc.center = new Vector3(0f, 1f, 0f);

        RenkaiRoundPlayer roundPlayer = bot.AddComponent<RenkaiRoundPlayer>();
        roundPlayer.agentName = callSign;
        roundPlayer.team = team;
        roundPlayer.isHumanPlayer = false;
        roundPlayer.RememberSpawn();

        RenkaiHealth health = bot.AddComponent<RenkaiHealth>();
        health.maxHealth = 100f;

        RenkaiTacticalBotAI ai = bot.AddComponent<RenkaiTacticalBotAI>();
        ai.team = team;
        ai.callSign = callSign;
        ai.viewDistance = 48f;
        ai.fireDistance = 36f;
        ai.reactionDelay = Random.Range(0.16f, 0.32f);
        ai.accuracy = Random.Range(0.62f, 0.82f);
        ai.moveSpeed = Random.Range(3.3f, 4.1f);

        Renderer renderer = bot.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(shader);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            renderer.sharedMaterial = mat;
        }
    }
}
