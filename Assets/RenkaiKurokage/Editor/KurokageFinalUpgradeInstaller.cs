using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageFinalUpgradeInstaller
{
    private const string ProductionMarkerName = "RENKAI_KUROKAGE_PRODUCTION_BUILD";
    private const string ProductionBuildId = "KUROKAGE_COMPETITIVE";
    private const string MainCompetitiveScenePath = "Assets/RenkaiKurokage/Scenes/Renkai_Kurogake_Competitive.unity";

    [MenuItem("Renkai/Apply Kurogake Production")]
    public static void RunAll()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        string validationReport;
        bool passed = BuildProductionVersionWithValidation(out validationReport);

        EditorUtility.DisplayDialog(
            "Renkai: Kurokage",
            "Tek üretim sürümü hazırlandı.\n\n" +
            "Ana akış: main branch + Renkai_Kurogake_Competitive scene + Apply Kurogake Production.\n\n" +
            "Validation: " + (passed ? "PASSED" : "REVIEW REQUIRED") + "\n\n" +
            validationReport + "\n\n" +
            "Build ID: " + ProductionBuildId,
            passed ? "OK" : "REVIEW"
        );
    }

    public static bool BuildProductionVersionWithValidation(out string validationReport)
    {
        bool sceneOk = EnsureActiveCompetitiveScene();
        bool environmentOk = KurokageEnvironmentArtPass.ApplySilent();
        bool brightVisualOk = KurokageBrightCompetitiveVisualPass.ApplySilent();
        bool architectureOk = KurokageCompetitiveArchitecturePass.ApplySilent();
        bool districtIdentityOk = KurokageDistrictIdentityPass.ApplySilent();
        bool siteLightingOk = KurokageSiteReadabilityLightingPass.ApplySilent();
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
        EnsureCompetitivePostFx();
        EnsureMatchStats();
        EnsureZodiacObjective();
        EnsureKairiAbilityKit();
        EnsureProductionMarker();
        EnsureUnifiedPresentation();

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        bool structurePassed = KurokageProductionValidator.ValidateSilent(out validationReport);
        AppendStepFailure(ref validationReport, sceneOk, "Unified scene silent build");
        AppendStepFailure(ref validationReport, environmentOk, "Environment art pass");
        AppendStepFailure(ref validationReport, brightVisualOk, "Bright competitive visual pass");
        AppendStepFailure(ref validationReport, architectureOk, "Competitive architecture pass");
        AppendStepFailure(ref validationReport, districtIdentityOk, "District identity pass");
        AppendStepFailure(ref validationReport, siteLightingOk, "Site readability lighting pass");
        AppendStepFailure(ref validationReport, ringRefineOk, "Environment ring refinement");
        AppendStepFailure(ref validationReport, zodiacArtOk, "Zodiac Core art pass");
        AppendStepFailure(ref validationReport, nexusArtOk, "Zodiac Nexus art pass");
        AppendStepFailure(ref validationReport, gameplayOk, "Gameplay upgrade silent step");
        AppendStepFailure(ref validationReport, matchOk, "5v5 install silent step");
        AppendStepFailure(ref validationReport, visualsOk, "Code-built agent visual step");

        bool passed = structurePassed && sceneOk && environmentOk && brightVisualOk && architectureOk &&
                      districtIdentityOk && siteLightingOk && ringRefineOk && zodiacArtOk && nexusArtOk &&
                      gameplayOk && matchOk && visualsOk;
        Debug.Log(validationReport);
        return passed;
    }

    public static void BuildProductionVersion()
    {
        string ignored;
        BuildProductionVersionWithValidation(out ignored);
    }

    private static void AppendStepFailure(ref string report, bool ok, string label)
    {
        if (!ok) report += "\nERROR " + label + " failed.";
    }

    private static bool EnsureActiveCompetitiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != MainCompetitiveScenePath)
            scene = EditorSceneManager.OpenScene(MainCompetitiveScenePath, OpenSceneMode.Single);

        return scene.IsValid() && scene.isLoaded && GameObject.Find("RENKAI_KUROKAGE_UNIFIED") != null;
    }

    private static void EnsureVfxPool()
    {
        KurokageVfxPool existing = Object.FindObjectOfType<KurokageVfxPool>(true);
        if (existing != null) return;

        GameObject poolRoot = GameObject.Find("KUROKAGE_VFX_POOL");
        if (poolRoot == null) poolRoot = new GameObject("KUROKAGE_VFX_POOL");
        if (poolRoot.GetComponent<KurokageVfxPool>() == null)
            poolRoot.AddComponent<KurokageVfxPool>();
    }

    private static void EnsureArmorAudioAndCombatVfx()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player.GetComponent<KurokageArmor>() == null)
                player.gameObject.AddComponent<KurokageArmor>();

            if (!player.isHumanPlayer) continue;
            if (player.GetComponent<KurokageAudioHooks>() == null)
                player.gameObject.AddComponent<KurokageAudioHooks>();
            if (player.GetComponent<KurokageCombatVfxPresenter>() == null)
                player.gameObject.AddComponent<KurokageCombatVfxPresenter>();
        }
    }

    private static void EnsureWorldHealthBarsHidden()
    {
        foreach (RenkaiWorldHealthBar bar in Object.FindObjectsOfType<RenkaiWorldHealthBar>(true))
            if (bar != null) bar.SetWorldVisible(false);
    }

    private static void EnsureDeathPresentation()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            if (player.GetComponent<KurokageAgentDeathPresentation>() == null)
                player.gameObject.AddComponent<KurokageAgentDeathPresentation>();
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

    private static void EnsureCompetitivePostFx()
    {
        Camera camera = Camera.main;
        if (camera == null) camera = Object.FindObjectOfType<Camera>(true);
        if (camera != null && camera.GetComponent<KurokageCompetitivePostFX>() == null)
            camera.gameObject.AddComponent<KurokageCompetitivePostFX>();
    }

    private static void EnsureEliteHud()
    {
        GameObject root = GameObject.Find("KUROKAGE_ELITE_HUD");
        if (root == null)
        {
            root = new GameObject("KUROKAGE_ELITE_HUD");
            root.AddComponent<KurokageEliteHUD>();
        }

        KurokageCompetitiveHUD oldHud = Object.FindObjectOfType<KurokageCompetitiveHUD>(true);
        if (oldHud != null && oldHud.gameObject != root)
            oldHud.gameObject.SetActive(false);
    }

    private static void EnsureTacticalRadarHud()
    {
        GameObject root = GameObject.Find("KUROKAGE_TACTICAL_RADAR_HUD");
        if (root == null)
        {
            root = new GameObject("KUROKAGE_TACTICAL_RADAR_HUD");
            root.AddComponent<KurokageTacticalRadarHUD>();
        }
    }

    private static void EnsureCombatFeedbackHud()
    {
        GameObject root = GameObject.Find("KUROKAGE_COMBAT_FEEDBACK_HUD");
        if (root == null)
        {
            root = new GameObject("KUROKAGE_COMBAT_FEEDBACK_HUD");
            root.AddComponent<KurokageCombatFeedbackHUD>();
        }
    }

    private static void EnsureDamageDirectionHud()
    {
        GameObject root = GameObject.Find("KUROKAGE_DAMAGE_DIRECTION_HUD");
        if (root == null)
        {
            root = new GameObject("KUROKAGE_DAMAGE_DIRECTION_HUD");
            root.AddComponent<KurokageDamageDirectionHUD>();
        }
    }

    private static void EnsureMatchPresentationHud()
    {
        GameObject root = GameObject.Find("KUROKAGE_MATCH_PRESENTATION_HUD");
        if (root == null)
        {
            root = new GameObject("KUROKAGE_MATCH_PRESENTATION_HUD");
            root.AddComponent<KurokageMatchPresentationHUD>();
        }
    }

    private static void EnsureMatchStats()
    {
        GameObject statsRoot = GameObject.Find("KUROKAGE_MATCH_STATS");
        if (statsRoot == null) statsRoot = new GameObject("KUROKAGE_MATCH_STATS");
        if (statsRoot.GetComponent<KurokageMatchStatsTracker>() == null)
            statsRoot.AddComponent<KurokageMatchStatsTracker>();

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

        GameObject objectiveRoot = GameObject.Find("KUROKAGE_ZODIAC_OBJECTIVE");
        if (objectiveRoot == null)
        {
            objectiveRoot = new GameObject("KUROKAGE_ZODIAC_OBJECTIVE");
            objectiveRoot.AddComponent<KurokageZodiacObjectiveController>();
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

    }

    private static void EnsureProductionMarker()
    {
        GameObject marker = GameObject.Find(ProductionMarkerName);
        if (marker == null) marker = new GameObject(ProductionMarkerName);

        KurokageProductionBuildMarker buildMarker = marker.GetComponent<KurokageProductionBuildMarker>();
        if (buildMarker == null) buildMarker = marker.AddComponent<KurokageProductionBuildMarker>();
        buildMarker.SetBuildId(ProductionBuildId);
    }

    private static void EnsureUnifiedPresentation()
    {
        GameObject marker = GameObject.Find(ProductionMarkerName);
        if (marker == null) marker = new GameObject(ProductionMarkerName);
        if (marker.GetComponent<KurokageUnifiedPresentationHUD>() == null)
            marker.AddComponent<KurokageUnifiedPresentationHUD>();

        RemoveLegacyHud<KurokageCompetitiveHUD>();
        RemoveLegacyHud<KurokageEliteHUD>();
        RemoveLegacyHud<KurokageTacticalRadarHUD>();
        RemoveLegacyHud<KurokageCombatFeedbackHUD>();
        RemoveLegacyHud<KurokageDamageDirectionHUD>();
        RemoveLegacyHud<KurokageMatchPresentationHUD>();
        RemoveLegacyHud<KurokageScoreboardHUD>();
        RemoveLegacyHud<KurokageZodiacHUD>();
        RemoveLegacyHud<KurokageAbilityHUD>();
        RemoveLegacyHud<RenkaiHUD>();
        RemoveLegacyHud<RenkaiHUDController>();
        RemoveLegacyHud<RenkaiHitFeedback>();
    }

    private static void RemoveLegacyHud<T>() where T : Behaviour
    {
        foreach (T legacy in Object.FindObjectsOfType<T>(true))
        {
            if (legacy != null)
                Object.DestroyImmediate(legacy.gameObject);
        }
    }
}
