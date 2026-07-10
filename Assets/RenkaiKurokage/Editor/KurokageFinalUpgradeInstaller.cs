using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageFinalUpgradeInstaller
{
    [MenuItem("Renkai/Run Final Competitive Upgrade")]
    public static void RunAll()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        RenkaiUnifiedCompetitiveBuilder.Build();
        KurokageGameplayUpgradeInstaller.Upgrade();
        KurokageFiveVFiveInstaller.Install();
        KurokageAgentVisualInstaller.Install();
        EnsureEliteHud();
        EnsureZodiacObjective();
        EnsureKairiAbilityKit();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Renkai: Kurokage",
            "Final competitive upgrade tamamlandı.\n\n- unified scene\n- gameplay feel\n- 5v5 test match\n- FBX agent visuals\n- elite HUD\n- Zodiac Core objective loop\n- Kairi Q/E/C/X ability kit\n- ability cooldown HUD\n\nPlay'e basıp test et.",
            "OK"
        );
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

    private static void EnsureZodiacObjective()
    {
        GameObject coreGo = GameObject.Find("ZODIAC_CORE");
        if (coreGo != null && coreGo.GetComponent<ZodiacCoreRuntime>() == null)
            coreGo.AddComponent<ZodiacCoreRuntime>();

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

        if (human != null && human.GetComponent<KairiAbilityController>() == null)
            human.gameObject.AddComponent<KairiAbilityController>();

        GameObject abilityHud = GameObject.Find("KUROKAGE_ABILITY_HUD");
        if (abilityHud == null)
        {
            abilityHud = new GameObject("KUROKAGE_ABILITY_HUD");
            abilityHud.AddComponent<KurokageAbilityHUD>();
        }
    }
}
