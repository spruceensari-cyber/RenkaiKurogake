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

        BuildProductionVersion();

        EditorUtility.DisplayDialog(
            "Renkai: Kurokage",
            "Tek üretim sürümü hazırlandı.\n\n" +
            "Ana akış: main branch + Renkai_Kurogake_Competitive scene + Build Production Version.\n\n" +
            "Kurulan sistemler:\n" +
            "- unified competitive scene\n" +
            "- gameplay feel and viewmodels\n" +
            "- 5v5 test match\n" +
            "- FBX agent visuals\n" +
            "- health + armor combat pipeline\n" +
            "- elite HUD\n" +
            "- round banners and kill feed\n" +
            "- centralized gameplay audio hooks\n" +
            "- Zodiac Core objective + VFX\n" +
            "- Kairi Q/E/C/X ability kit\n" +
            "- Eclipse Blade combo combat\n" +
            "- ability cooldown HUD\n\n" +
            "Build ID: " + ProductionBuildId,
            "OK"
        );
    }

    public static void BuildProductionVersion()
    {
        RenkaiUnifiedCompetitiveBuilder.Build();
        KurokageGameplayUpgradeInstaller.Upgrade();
        KurokageFiveVFiveInstaller.Install();
        KurokageAgentVisualInstaller.Install();
        EnsureArmorAndAudio();
        EnsureEliteHud();
        EnsureMatchPresentationHud();
        EnsureZodiacObjective();
        EnsureKairiAbilityKit();
        EnsureProductionMarker();

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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

    private static void EnsureArmorAndAudio()
    {
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player.GetComponent<KurokageArmor>() == null)
                player.gameObject.AddComponent<KurokageArmor>();

            if (player.isHumanPlayer && player.GetComponent<KurokageAudioHooks>() == null)
                player.gameObject.AddComponent<KurokageAudioHooks>();
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
