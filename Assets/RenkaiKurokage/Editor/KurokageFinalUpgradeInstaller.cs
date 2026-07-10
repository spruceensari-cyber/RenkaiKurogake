using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Renkai: Kurokage",
            "Final competitive upgrade tamamlandı.\n\n- unified scene\n- gameplay feel\n- 5v5 test match\n- FBX agent visuals\n- elite HUD\n\nPlay'e basıp test et.",
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
}
