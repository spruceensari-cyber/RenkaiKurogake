using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageFiveVFiveInstaller
{
    private const string MatchRootName = "KUROKAGE_5V5_MATCH";

    public static bool InstallSilent()
    {
        RenkaiFPSController humanFps = Object.FindObjectOfType<RenkaiFPSController>(true);
        if (humanFps == null) return false;

        RenkaiRoundPlayer humanRound = humanFps.GetComponent<RenkaiRoundPlayer>();
        if (humanRound == null) humanRound = humanFps.gameObject.AddComponent<RenkaiRoundPlayer>();

        RemoveDuplicateMatchRoots();
        RemoveAllNonHumanParticipants(humanRound);
        RemoveExtraHumanParticipants(humanRound);

        GameObject root = new GameObject(MatchRootName);
        Transform attackers = Group(root.transform, "ATTACKERS");
        Transform defenders = Group(root.transform, "DEFENDERS");

        ConfigureHuman(humanRound);

        Vector3[] alliedSpawns =
        {
            new Vector3(-7f, 1f, -60f),
            new Vector3(-3.5f, 1f, -63f),
            new Vector3(3.5f, 1f, -63f),
            new Vector3(7f, 1f, -60f)
        };

        Vector3[] enemySpawns =
        {
            new Vector3(-42f, 1f, 34f),
            new Vector3(-26f, 1f, 34f),
            new Vector3(0f, 1f, 38f),
            new Vector3(26f, 1f, 34f),
            new Vector3(42f, 1f, 34f)
        };

        string[] alliedNames = { "NOA // PULSE-A2", "REIHA // VEIL-A3", "MIO // GLINT-A4", "SORA // BASTION-A5" };
        string[] enemyNames = { "AIKO // LANCER-D1", "REN // FORGE-D2", "HANA // ORBIT-D3", "TOMA // ANCHOR-D4", "YORI // WRAITH-D5" };

        for (int i = 0; i < alliedSpawns.Length; i++)
            CreateBot(alliedNames[i], RenkaiTeam.Attackers, alliedSpawns[i], attackers, i);

        for (int i = 0; i < enemySpawns.Length; i++)
            CreateBot(enemyNames[i], RenkaiTeam.Defenders, enemySpawns[i], defenders, i + alliedSpawns.Length);

        EnsureSingleRoundManager(root.transform);
        EnsureExactRosterOrFail(humanRound);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        return true;
    }

    private static void ConfigureHuman(RenkaiRoundPlayer human)
    {
        human.agentName = "KAIRI // RIFT-07";
        human.team = RenkaiTeam.Attackers;
        human.isHumanPlayer = true;
        human.RememberSpawn();
    }

    private static void RemoveDuplicateMatchRoots()
    {
        string[] legacyNames = { MatchRootName, "KUROKAGE_5V5_TEST_MATCH" };
        foreach (string rootName in legacyNames)
        {
            GameObject root;
            while ((root = GameObject.Find(rootName)) != null)
                Object.DestroyImmediate(root);
        }
    }

    private static void RemoveAllNonHumanParticipants(RenkaiRoundPlayer authoritativeHuman)
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null || player == authoritativeHuman) continue;
            if (player.isHumanPlayer && player.GetComponent<RenkaiFPSController>() != null) continue;
            Object.DestroyImmediate(player.gameObject);
        }
    }

    private static void RemoveExtraHumanParticipants(RenkaiRoundPlayer authoritativeHuman)
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null || player == authoritativeHuman) continue;
            if (player.isHumanPlayer) Object.DestroyImmediate(player.gameObject);
        }
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void CreateBot(string identity, RenkaiTeam team, Vector3 position, Transform parent, int index)
    {
        GameObject bot = new GameObject("BOT_" + team + "_" + (index + 1).ToString("00"));
        bot.transform.SetParent(parent, true);
        bot.transform.position = position;
        bot.transform.rotation = Quaternion.LookRotation(team == RenkaiTeam.Attackers ? Vector3.forward : Vector3.back);

        CharacterController cc = bot.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.skinWidth = 0.045f;
        cc.stepOffset = 0.34f;
        cc.slopeLimit = 52f;
        cc.minMoveDistance = 0f;

        RenkaiRoundPlayer roundPlayer = bot.AddComponent<RenkaiRoundPlayer>();
        roundPlayer.agentName = identity;
        roundPlayer.team = team;
        roundPlayer.isHumanPlayer = false;
        roundPlayer.RememberSpawn();

        RenkaiHealth health = bot.AddComponent<RenkaiHealth>();
        health.maxHealth = 100f;

        RenkaiTacticalBotAI ai = bot.AddComponent<RenkaiTacticalBotAI>();
        ai.team = team;
        ai.callSign = identity;
        ai.role = (RenkaiAgentRole)(index % 5);
        ai.viewDistance = 48f;
        ai.fireDistance = 36f;
        ai.reactionDelay = Random.Range(0.18f, 0.34f);
        ai.accuracy = Random.Range(0.64f, 0.82f);
        ai.moveSpeed = Random.Range(3.4f, 4.0f);

        bot.AddComponent<KurokageBotPerception>();
        bot.AddComponent<KurokageBotLocalAvoidance>();
        bot.AddComponent<KurokageCharacterCollisionGuard>();
    }

    private static void EnsureSingleRoundManager(Transform parent)
    {
        RenkaiRoundManager keeper = null;
        foreach (RenkaiRoundManager manager in Object.FindObjectsOfType<RenkaiRoundManager>(true))
        {
            if (manager == null) continue;
            if (keeper == null) keeper = manager;
            else Object.DestroyImmediate(manager.gameObject);
        }

        if (keeper == null)
        {
            GameObject managerGo = new GameObject("ROUND_MANAGER");
            managerGo.transform.SetParent(parent, false);
            keeper = managerGo.AddComponent<RenkaiRoundManager>();
        }
    }

    private static void EnsureExactRosterOrFail(RenkaiRoundPlayer human)
    {
        int humans = 0;
        int alliedBots = 0;
        int enemyBots = 0;

        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null) continue;
            if (player.isHumanPlayer) humans++;
            else if (player.team == human.team) alliedBots++;
            else enemyBots++;
        }

        if (humans != 1 || alliedBots != 4 || enemyBots != 5)
            throw new System.InvalidOperationException(
                "RENKAI roster installation failed. Expected 1 human + 4 allied bots + 5 enemy bots, got " +
                humans + " human, " + alliedBots + " allied bots, " + enemyBots + " enemy bots."
            );
    }
}
