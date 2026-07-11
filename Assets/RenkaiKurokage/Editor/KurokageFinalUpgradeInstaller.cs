using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageFinalUpgradeInstaller
{
    public const string MainCompetitiveScenePath = "Assets/RenkaiKurokage/Scenes/Renkai_Kurogake_Competitive.unity";
    private const string ProductionMarkerName = "RENKAI_KUROKAGE_PRODUCTION_BUILD";
    private const string ProductionBuildId = "KUROKAGE_COMPETITIVE_UNIFIED_01";

    [MenuItem("Renkai/Build Production Version", priority = 0)]
    public static void RunAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Renkai", "Production build hazırlığı için Play Mode'dan çık.", "OK");
            return;
        }

        bool passed = PrepareProduction(true, out string report);
        EditorUtility.DisplayDialog(
            "RENKAI: KUROKAGE",
            (passed ? "PRODUCTION PASSED" : "PRODUCTION REVIEW REQUIRED") +
            "\n\n" + report +
            "\n\nBuild ID: " + ProductionBuildId,
            passed ? "OK" : "REVIEW"
        );
    }

    public static bool PrepareProduction(bool saveChanges, out string validationReport)
    {
        validationReport = string.Empty;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            validationReport = "ERROR Production preparation cannot run in Play Mode.";
            return false;
        }

        bool sceneOk = EnsureActiveCompetitiveScene();
        if (!sceneOk)
        {
            validationReport = "ERROR Canonical competitive scene could not be opened.";
            return false;
        }

        // Gameplay topology first. Presentation must never create an alternate gameplay scene.
        bool hierarchyBeforeOk = KurokageUnifiedHierarchyPass.ApplySilent();
        bool collisionBeforeOk = KurokageCollisionIntegrityPass.ApplySilent();
        bool gameplayOk = KurokageGameplayUpgradeInstaller.UpgradeSilent();
        bool matchOk = KurokageFiveVFiveInstaller.InstallSilent();
        bool visualsOk = KurokageAgentVisualInstaller.InstallSilent();
        bool collisionAfterPlayersOk = KurokageCollisionIntegrityPass.ApplySilent();

        // Environment and presentation decorate the canonical gameplay geometry.
        bool environmentOk = KurokageEnvironmentArtPass.ApplySilent();
        bool brightVisualOk = KurokageBrightCompetitiveVisualPass.ApplySilent();
        bool cinematicDistrictOk = KurokageCinematicDistrictPass.ApplySilent();
        bool architectureOk = KurokageCompetitiveArchitecturePass.ApplySilent();
        bool districtIdentityOk = KurokageDistrictIdentityPass.ApplySilent();
        bool siteLightingOk = KurokageSiteReadabilityLightingPass.ApplySilent();
        bool ringRefineOk = KurokageEnvironmentRingRefiner.ApplySilent();
        bool zodiacArtOk = KurokageZodiacCoreArtInstaller.ApplySilent();
        bool nexusArtOk = KurokageZodiacNexusArtInstaller.ApplySilent();

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

        // Sanitizer must run after every generator, otherwise a later pass can recreate capsules,
        // duplicate roots, unsupported materials, cameras, or AudioListeners.
        bool hierarchyAfterOk = KurokageUnifiedHierarchyPass.ApplySilent();
        bool sanitizerOk = KurokageProductionSanitizerPass.ApplySilent();
        bool finalCollisionOk = KurokageCollisionIntegrityPass.ApplySilent();

        if (saveChanges)
        {
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        bool structurePassed = KurokageProductionValidator.ValidateSilent(out validationReport);
        AppendStepFailure(ref validationReport, sceneOk, "Canonical scene");
        AppendStepFailure(ref validationReport, hierarchyBeforeOk && hierarchyAfterOk, "Unified hierarchy");
        AppendStepFailure(ref validationReport, collisionBeforeOk && collisionAfterPlayersOk && finalCollisionOk, "Collision integrity");
        AppendStepFailure(ref validationReport, gameplayOk, "Gameplay installation");
        AppendStepFailure(ref validationReport, matchOk, "5v5 installation");
        AppendStepFailure(ref validationReport, visualsOk, "Single code-built agent visual installation");
        AppendStepFailure(ref validationReport, environmentOk, "Environment art");
        AppendStepFailure(ref validationReport, brightVisualOk, "Bright competitive visual pass");
        AppendStepFailure(ref validationReport, cinematicDistrictOk, "Cinematic district pass");
        AppendStepFailure(ref validationReport, architectureOk, "Competitive architecture");
        AppendStepFailure(ref validationReport, districtIdentityOk, "District identity");
        AppendStepFailure(ref validationReport, siteLightingOk, "Site readability lighting");
        AppendStepFailure(ref validationReport, ringRefineOk, "Environment ring refinement");
        AppendStepFailure(ref validationReport, zodiacArtOk, "Zodiac Core art");
        AppendStepFailure(ref validationReport, nexusArtOk, "Zodiac Nexus art");
        AppendStepFailure(ref validationReport, sanitizerOk, "Final production sanitization");

        bool passed = structurePassed && sceneOk && hierarchyBeforeOk && hierarchyAfterOk &&
                      collisionBeforeOk && collisionAfterPlayersOk && finalCollisionOk &&
                      gameplayOk && matchOk && visualsOk && environmentOk && brightVisualOk &&
                      cinematicDistrictOk && architectureOk && districtIdentityOk && siteLightingOk &&
                      ringRefineOk && zodiacArtOk && nexusArtOk && sanitizerOk;

        Debug.Log(validationReport);
        return passed;
    }

    public static bool BuildProductionVersionWithValidation(out string validationReport)
    {
        return PrepareProduction(true, out validationReport);
    }

    public static void BuildProductionVersion()
    {
        PrepareProduction(true, out _);
    }

    private static bool EnsureActiveCompetitiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != MainCompetitiveScenePath)
            scene = EditorSceneManager.OpenScene(MainCompetitiveScenePath, OpenSceneMode.Single);
        return scene.IsValid() && scene.isLoaded && scene.path == MainCompetitiveScenePath;
    }

    private static void AppendStepFailure(ref string report, bool ok, string label)
    {
        if (!ok) report += "\nERROR " + label + " failed.";
    }

    private static void EnsureVfxPool()
    {
        KurokageVfxPool existing = Object.FindObjectOfType<KurokageVfxPool>(true);
        if (existing != null) return;
        GameObject root = GameObject.Find("KUROKAGE_VFX_POOL") ?? new GameObject("KUROKAGE_VFX_POOL");
        root.AddComponent<KurokageVfxPool>();
    }

    private static void EnsureArmorAudioAndCombatVfx()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player.GetComponent<KurokageArmor>() == null) player.gameObject.AddComponent<KurokageArmor>();
            if (!player.isHumanPlayer) continue;
            if (player.GetComponent<KurokageAudioHooks>() == null) player.gameObject.AddComponent<KurokageAudioHooks>();
            if (player.GetComponent<KurokageCombatVfxPresenter>() == null) player.gameObject.AddComponent<KurokageCombatVfxPresenter>();
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
            if (player.GetComponent<KurokageMovementPresentation>() == null) player.gameObject.AddComponent<KurokageMovementPresentation>();
            if (player.GetComponent<KurokageViewmodelLightingPresenter>() == null) player.gameObject.AddComponent<KurokageViewmodelLightingPresenter>();
            if (player.GetComponent<KurokageSprintWeaponGate>() == null) player.gameObject.AddComponent<KurokageSprintWeaponGate>();
        }
    }

    private static void EnsureCompetitivePostFx()
    {
        Camera camera = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>(true);
        if (camera != null && camera.GetComponent<KurokageCompetitivePostFX>() == null)
            camera.gameObject.AddComponent<KurokageCompetitivePostFX>();
    }

    private static void EnsureMatchStats()
    {
        GameObject root = GameObject.Find("KUROKAGE_MATCH_STATS") ?? new GameObject("KUROKAGE_MATCH_STATS");
        if (root.GetComponent<KurokageMatchStatsTracker>() == null) root.AddComponent<KurokageMatchStatsTracker>();
    }

    private static void EnsureZodiacObjective()
    {
        GameObject core = GameObject.Find("ZODIAC_CORE");
        if (core != null)
        {
            if (core.GetComponent<ZodiacCoreRuntime>() == null) core.AddComponent<ZodiacCoreRuntime>();
            if (core.GetComponent<KurokageZodiacVfxPresenter>() == null) core.AddComponent<KurokageZodiacVfxPresenter>();
        }

        GameObject root = GameObject.Find("KUROKAGE_ZODIAC_OBJECTIVE") ?? new GameObject("KUROKAGE_ZODIAC_OBJECTIVE");
        if (root.GetComponent<KurokageZodiacObjectiveController>() == null) root.AddComponent<KurokageZodiacObjectiveController>();
    }

    private static void EnsureKairiAbilityKit()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (!player.isHumanPlayer) continue;
            if (player.GetComponent<KurokageAfterimagePresenter>() == null) player.gameObject.AddComponent<KurokageAfterimagePresenter>();
            if (player.GetComponent<KairiAbilityController>() == null) player.gameObject.AddComponent<KairiAbilityController>();
            if (player.GetComponent<KurokageBladeCombatController>() == null) player.gameObject.AddComponent<KurokageBladeCombatController>();
            if (player.GetComponent<KurokageEclipseProtocolPresenter>() == null) player.gameObject.AddComponent<KurokageEclipseProtocolPresenter>();
            break;
        }
    }

    private static void EnsureProductionMarker()
    {
        GameObject marker = GameObject.Find(ProductionMarkerName) ?? new GameObject(ProductionMarkerName);
        KurokageProductionBuildMarker component = marker.GetComponent<KurokageProductionBuildMarker>();
        if (component == null) component = marker.AddComponent<KurokageProductionBuildMarker>();
        component.SetBuildId(ProductionBuildId);
    }

    private static void EnsureUnifiedPresentation()
    {
        GameObject root = GameObject.Find("KUROKAGE_FINAL_HUD") ?? new GameObject("KUROKAGE_FINAL_HUD");
        if (root.GetComponent<KurokageUnifiedPresentationHUD>() == null)
            root.AddComponent<KurokageUnifiedPresentationHUD>();

        RemoveLegacyHud<KurokageCompetitiveHUD>(root);
        RemoveLegacyHud<KurokageEliteHUD>(root);
        RemoveLegacyHud<KurokageTacticalRadarHUD>(root);
        RemoveLegacyHud<KurokageCombatFeedbackHUD>(root);
        RemoveLegacyHud<KurokageDamageDirectionHUD>(root);
        RemoveLegacyHud<KurokageMatchPresentationHUD>(root);
        RemoveLegacyHud<KurokageScoreboardHUD>(root);
        RemoveLegacyHud<KurokageZodiacHUD>(root);
        RemoveLegacyHud<KurokageAbilityHUD>(root);
        RemoveLegacyHud<RenkaiHUD>(root);
        RemoveLegacyHud<RenkaiHUDController>(root);
        RemoveLegacyHud<RenkaiHitFeedback>(root);
    }

    private static void RemoveLegacyHud<T>(GameObject finalRoot) where T : Behaviour
    {
        foreach (T legacy in Object.FindObjectsOfType<T>(true))
            if (legacy != null && legacy.gameObject != finalRoot)
                Object.DestroyImmediate(legacy.gameObject);
    }
}
