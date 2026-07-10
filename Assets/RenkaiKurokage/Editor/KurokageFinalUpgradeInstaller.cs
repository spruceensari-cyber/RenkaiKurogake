using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageFinalUpgradeInstaller
{
    private const string ProductionMarkerName = "RENKAI_KUROKAGE_PRODUCTION_BUILD";
    private const string ProductionBuildId = "PRODUCTION_ALPHA_01";

    [MenuItem("Renkai/Build Production Version")]
    public static void RunAll()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        string validationReport;
        bool passed = BuildProductionVersion(out validationReport);

        EditorUtility.DisplayDialog(
            "Renkai: Kurokage",
            "Tek üretim sürümü hazırlandı.\n\n" +
            "Ana akış: main branch + Renkai_Kurogake_Competitive scene + Build Production Version.\n\n" +
            "Validation: " + (passed ? "PASSED" : "REVIEW REQUIRED") + "\n\n" +
            validationReport + "\n\n" +
            "Build ID: " + ProductionBuildId,
            passed ? "OK" : "REVIEW"
        );
    }

    public static bool BuildProductionVersion(out string validationReport)
    {
        bool sceneOk = RenkaiUnifiedCompetitiveBuilder.BuildSilent();
        bool gameplayOk = KurokageGameplayUpgradeInstaller.UpgradeSilent();
        bool matchOk = KurokageFiveVFiveInstaller.InstallSilent();
        bool visualsOk = KurokageAgentVisualInstaller.InstallSilent();

        EnsureArmorAudioAndCombatVfx();
        EnsureMovementPresentation();
        EnsureEliteHud();
        EnsureCombatFeedbackHud();
        EnsureMatchPresentationHud();
        EnsureZodiacObjective();
        EnsureKairiAbilityKit();
        EnsureProductionMarker();

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        bool structurePassed = KurokageProductionValidator.ValidateSilent(out validationReport);
        if (!sceneOk) validationReport += "\nERROR Unified scene silent build failed.";
        if (!gameplayOk) validationReport += "\nERROR Gameplay upgrade silent step failed.";
        if (!matchOk) validationReport += "\nERROR 5v5 install silent step failed.";
        if (!visualsOk) validationReport += "\nERROR Agent visual silent step failed.";

        bool passed = structurePassed && sceneOk && gameplayOk && matchOk && visualsOk;
        Debug.Log(validationReport);
        return passed;
    }

    public static void BuildProductionVersion()
    {
        string ignored;
        BuildProductionVersion(out ignored);
    }

    private static void EnsureProductionMarker()
    {
        GameObject marker = GameObject.Find(ProductionMarkerName);
        if (marker == null)
            marker = new GameObject(ProductionMarkerName);

        KurokageProductionBuildMarker buildMarker = marker.GetComponent<KurokageProductionBuildMarker>();
        if (buildMarker == null)
            buildMarker = marker.AddComponent<KurokageProductionBuildMarker>();

        buildMarker.SetBuildId(ProductionBuildId);
    }

    private static void EnsureArmorAudioAndCombatVfx()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player.GetComponent<KurokageArmor>() == null)
                player.gameObject.AddComponent<KurokageArmor>();

            if (player.isHumanPlayer)
            {
                if (player.GetComponent<KurokageAudioHooks>() == null)
                    player.gameObject.AddComponent<KurokageAudioHooks>();

                if (player.GetComponent<KurokageCombatVfxPresenter>() == null)
                    player.gameObject.AddComponent<KurokageCombatVfxPresenter>();
            }
        }
    }

    private static void EnsureMovementPresentation()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (!player.isHumanPlayer) continue;
            if (player.GetComponent<KurokageMovementPresentation>() == null)
                player.gameObject.AddComponent<KurokageMovementPresentation>();
        }
    }

    private static void EnsureEliteHud()
    {
        GameObject existing = GameObject.Find("KUROKAGE_ELITE_HUD");
        if (existing == null)
        {
            existing = new GameObject("KUROKAGE_ELITE_HUD");
            existing.AddComponent<KurokageEliteHUD>();
        }

        KurokageCompetitiveHUD oldPremium = Object.FindObjectOfType<KurokageCompetitiveHUD>();
        if (oldPremium != null && oldPremium.gameObject != existing)
            oldPremium.gameObject.SetActive(false);
    }

    private static void EnsureCombatFeedbackHud()
    {
        GameObject existing = GameObject.Find("KUROKAGE_COMBAT_FEEDBACK_HUD");
        if (existing == null)
        {
            existing = new GameObject("KUROKAGE_COMBAT_FEEDBACK_HUD");
            existing.AddComponent<KurokageCombatFeedbackHUD>();
        }
    }

    private static void EnsureMatchPresentationHud()
    {
        GameObject existing = GameObject.Find("KUROKAGE_MATCH_PRESENTATION_HUD");
        if (existing == null)
        {
            existing = new GameObject("KUROKAGE_MATCH_PRESENTATION_HUD");
            existing.AddComponent<KurokageMatchPresentationHUD>();
        }
    }

    private static void EnsureZodiacObjective()
    {
        GameObject coreGo = GameObject.Find("ZODIAC_CORE");
        if (coreGo != null)
        {
            if (coreGo.GetComponent<ZodiacCoreRuntime>() == null)
                coreGo.AddComponent<ZodiacCoreRuntime>();

            if (coreGo.GetComponent<KurokageZodiacVfxPresenter>() == null)
                coreGo.AddComponent<KurokageZodiacVfxPresenter>();
        }

        GameObject objectiveGo = GameObject.Find("KUROKAGE_ZODIAC_OBJECTIVE");
        if (objectiveGo == null)
        {
            objectiveGo = new GameObject("KUROKAGE_ZODIAC_OBJECTIVE");
            objectiveGo.AddComponent<KurokageZodiacObjectiveController>();
        }

        GameObject hudGo = GameObject.Find("KUROKAGE_ZODIAC_HUD");
        if (hudGo == null)
        {
            hudGo = new GameObject("KUROKAGE_ZODIAC_HUD");
            hudGo.AddComponent<KurokageZodiacHUD>();
        }
    }

    private static void EnsureKairiAbilityKit()
    {
        RenkaiRoundPlayer human = null;
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player.isHumanPlayer)
            {
                human = player;
                break;
            }
        }

        if (human != null)
        {
            if (human.GetComponent<KairiAbilityController>() == null)
                human.gameObject.AddComponent<KairiAbilityController>();

            if (human.GetComponent<KurokageBladeCombatController>() == null)
                human.gameObject.AddComponent<KurokageBladeCombatController>();
        }

        GameObject abilityHud = GameObject.Find("KUROKAGE_ABILITY_HUD");
        if (abilityHud == null)
        {
            abilityHud = new GameObject("KUROKAGE_ABILITY_HUD");
            abilityHud.AddComponent<KurokageAbilityHUD>();
        }
    }
}
