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

        KurokageAgentArchetype[] alliedAgents =
        {
            KurokageAgentArchetype.Noa,
            KurokageAgentArchetype.Reiha,
            KurokageAgentArchetype.Mio,
            KurokageAgentArchetype.Sora
        };

        KurokageAgentArchetype[] enemyAgents =
        {
            KurokageAgentArchetype.Aiko,
            KurokageAgentArchetype.Ren,
            KurokageAgentArchetype.Hana,
            KurokageAgentArchetype.Toma,
            KurokageAgentArchetype.Yori
        };

        for (int i = 0; i < alliedSpawns.Length; i++)
            CreateBot(alliedAgents[i], RenkaiTeam.Attackers, alliedSpawns[i], attackers, i);

        for (int i = 0; i < enemySpawns.Length; i++)
            CreateBot(enemyAgents[i], RenkaiTeam.Defenders, enemySpawns[i], defenders, i + alliedSpawns.Length);

        EnsureSingleRoundManager(root.transform);
        EnsureExactRosterOrFail(humanRound);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        return true;
    }

    private static void ConfigureHuman(RenkaiRoundPlayer human)
    {
        human.team = RenkaiTeam.Attackers;
        human.isHumanPlayer = true;
        KurokageAgentIdentity identity = human.GetComponent<KurokageAgentIdentity>();
        if (identity == null) identity = human.gameObject.AddComponent<KurokageAgentIdentity>();
        identity.Configure(KurokageAgentArchetype.Kairi, true);
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

    private static void CreateBot(KurokageAgentArchetype archetype, RenkaiTeam team, Vector3 position, Transform parent, int index)
    {
        KurokageAgentDefinition definition = KurokageAgentCatalog.Get(archetype);
        GameObject bot = new GameObject("AGENT_" + definition.DisplayName + "_" + team);
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
        roundPlayer.team = team;
        roundPlayer.isHumanPlayer = false;

        KurokageAgentIdentity identity = bot.AddComponent<KurokageAgentIdentity>();
        identity.Configure(archetype, false);
        roundPlayer.RememberSpawn();

        RenkaiHealth health = bot.AddComponent<RenkaiHealth>();
        health.maxHealth = 100f;

        RenkaiTacticalBotAI ai = bot.AddComponent<RenkaiTacticalBotAI>();
        ai.team = team;
        ai.callSign = definition.FullIdentity;
        ai.role = (RenkaiAgentRole)(index % 5);
        ai.viewDistance = 48f;
        ai.fireDistance = 36f;
        ai.reactionDelay = Random.Range(0.18f, 0.34f);
        ai.accuracy = Random.Range(0.64f, 0.82f);
        ai.moveSpeed = Random.Range(3.4f, 4.0f);

        bot.AddComponent<KurokageBotPerception>();
        bot.AddComponent<KurokageBotLocalAvoidance>();
        bot.AddComponent<KurokageCharacterCollisionGuard>();
        bot.AddComponent<KurokageBotAutonomyMotor>();
        bot.AddComponent<KurokageBotWeaponState>();
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
                "RENKAI roster installation failed. Expected 1 human + 4 allied agents + 5 enemy agents, got " +
                humans + " human, " + alliedBots + " allied agents, " + enemyBots + " enemy agents."
            );
    }
}
