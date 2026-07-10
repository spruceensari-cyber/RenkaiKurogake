using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageFinalUpgradeInstaller
{
    private const string ProductionMarkerName = "RENKAI_KUROKAGE_PRODUCTION_BUILD";
    private const string ProductionBuildId = "PRODUCTION_ALPHA_09";

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
        bool environmentOk = KurokageEnvironmentArtPass.ApplySilent();
        bool brightVisualOk = KurokageBrightCompetitiveVisualPass.ApplySilent();
        bool architectureOk = KurokageCompetitiveArchitecturePass.ApplySilent();
        bool districtIdentityOk = KurokageDistrictIdentityPass.ApplySilent();
        bool ringRefineOk = KurokageEnvironmentRingRefiner.ApplySilent();
        bool zodiacArtOk = KurokageZodiacCoreArtInstaller.ApplySilent();
        bool nexusArtOk = KurokageZodiacNexusArtInstaller.ApplySilent();
        bool gameplayOk = KurokageGameplayUpgradeInstaller.UpgradeSilent();
        bool matchOk = KurokageFiveVFiveInstaller.InstallSilent();
        bool visualsOk = KurokageAgentVisualInstaller.InstallSilent();

        EnsureVfxPool();
        EnsureArmorAudioAndCombatVfx();
        EnsureWorldHealthBarsHidden();
        EnsureDeathPresentation();
        EnsureMovementPresentation();
        EnsureEliteHud();
        EnsureTacticalRadarHud();
        EnsureCombatFeedbackHud();
        EnsureDamageDirectionHud();
        EnsureMatchPresentationHud();
        EnsureMatchStatsAndScoreboard();
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
        if (!environmentOk) validationReport += "\nERROR Environment art pass failed.";
        if (!brightVisualOk) validationReport += "\nERROR Bright competitive visual pass failed.";
        if (!architectureOk) validationReport += "\nERROR Competitive architecture pass failed.";
        if (!districtIdentityOk) validationReport += "\nERROR District identity pass failed.";
        if (!ringRefineOk) validationReport += "\nERROR Environment ring refinement failed.";
        if (!zodiacArtOk) validationReport += "\nERROR Zodiac Core art pass failed.";
        if (!nexusArtOk) validationReport += "\nERROR Zodiac Nexus art pass failed.";
        if (!gameplayOk) validationReport += "\nERROR Gameplay upgrade silent step failed.";
        if (!matchOk) validationReport += "\nERROR 5v5 install silent step failed.";
        if (!visualsOk) validationReport += "\nERROR Agent visual silent step failed.";

        bool passed = structurePassed && sceneOk && environmentOk && brightVisualOk && architectureOk && districtIdentityOk && ringRefineOk && zodiacArtOk && nexusArtOk && gameplayOk && matchOk && visualsOk;
        Debug.Log(validationReport);
        return passed;
    }

    public static void BuildProductionVersion()
    {
        string ignored;
        BuildProductionVersion(out ignored);
    }

    private static void EnsureVfxPool()
    {
        KurokageVfxPool existing = Object.FindObjectOfType<KurokageVfxPool>(true);
        if (existing != null) return;

        GameObject poolRoot = GameObject.Find("KUROKAGE_VFX_POOL");
        if (poolRoot == null)
            poolRoot = new GameObject("KUROKAGE_VFX_POOL");

        if (poolRoot.GetComponent<KurokageVfxPool>() == null)
            poolRoot.AddComponent<KurokageVfxPool>();
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

    private static void EnsureWorldHealthBarsHidden()
    {
        foreach (RenkaiWorldHealthBar bar in Object.FindObjectsOfType<RenkaiWorldHealthBar>(true))
            bar.SetWorldVisible(false);
    }

    private static void EnsureDeathPresentation()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player.GetComponent<KurokageAgentDeathPresentation>() == null)
                player.gameObject.AddComponent<KurokageAgentDeathPresentation>();
        }
    }

    private static void EnsureMovementPresentation()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (!player.isHumanPlayer) continue;
            if (player.GetComponent<KurokageMovementPresentation>() == null)
                player.gameObject.AddComponent<KurokageMovementPresentation>();
            if (player.GetComponent<KurokageViewmodelLightingPresenter>() == null)
                player.gameObject.AddComponent<KurokageViewmodelLightingPresenter>();
            if (player.GetComponent<KurokageSprintWeaponGate>() == null)
                player.gameObject.AddComponent<KurokageSprintWeaponGate>();
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

    private static void EnsureTacticalRadarHud()
    {
        GameObject existing = GameObject.Find("KUROKAGE_TACTICAL_RADAR_HUD");
        if (existing == null)
        {
            existing = new GameObject("KUROKAGE_TACTICAL_RADAR_HUD");
            existing.AddComponent<KurokageTacticalRadarHUD>();
        }
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

    private static void EnsureDamageDirectionHud()
    {
        GameObject existing = GameObject.Find("KUROKAGE_DAMAGE_DIRECTION_HUD");
        if (existing == null)
        {
            existing = new GameObject("KUROKAGE_DAMAGE_DIRECTION_HUD");
            existing.AddComponent<KurokageDamageDirectionHUD>();
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

    private static void EnsureMatchStatsAndScoreboard()
    {
        GameObject statsRoot = GameObject.Find("KUROKAGE_MATCH_STATS");
        if (statsRoot == null)
            statsRoot = new GameObject("KUROKAGE_MATCH_STATS");
        if (statsRoot.GetComponent<KurokageMatchStatsTracker>() == null)
            statsRoot.AddComponent<KurokageMatchStatsTracker>();

        GameObject scoreboardRoot = GameObject.Find("KUROKAGE_SCOREBOARD_HUD");
        if (scoreboardRoot == null)
            scoreboardRoot = new GameObject("KUROKAGE_SCOREBOARD_HUD");
        if (scoreboardRoot.GetComponent<KurokageScoreboardHUD>() == null)
            scoreboardRoot.AddComponent<KurokageScoreboardHUD>();
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
            if (human.GetComponent<KurokageAfterimagePresenter>() == null)
                human.gameObject.AddComponent<KurokageAfterimagePresenter>();

            if (human.GetComponent<KairiAbilityController>() == null)
                human.gameObject.AddComponent<KairiAbilityController>();

            if (human.GetComponent<KurokageBladeCombatController>() == null)
                human.gameObject.AddComponent<KurokageBladeCombatController>();

            if (human.GetComponent<KurokageEclipseProtocolPresenter>() == null)
                human.gameObject.AddComponent<KurokageEclipseProtocolPresenter>();
        }

        GameObject abilityHud = GameObject.Find("KUROKAGE_ABILITY_HUD");
        if (abilityHud == null)
        {
            abilityHud = new GameObject("KUROKAGE_ABILITY_HUD");
            abilityHud.AddComponent<KurokageAbilityHUD>();
        }
    }
}
