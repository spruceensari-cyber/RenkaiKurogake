using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KurokageProductionSanitizerAutoHook
{
    private const string CompetitiveScenePath = "Assets/RenkaiKurokage/Scenes/Renkai_Kurogake_Competitive.unity";
    private static bool scheduled;

    static KurokageProductionSanitizerAutoHook()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        ScheduleIfProductionScene();
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        ScheduleIfProductionScene();
    }

    private static void ScheduleIfProductionScene()
    {
        if (scheduled || EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != CompetitiveScenePath) return;

        scheduled = true;
        EditorApplication.delayCall += ApplyOnce;
    }

    private static void ApplyOnce()
    {
        scheduled = false;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != CompetitiveScenePath) return;
        if (GameObject.Find("RENKAI_KUROKAGE_UNIFIED") == null &&
            GameObject.Find("RENKAI_KUROKAGE_PRODUCTION_BUILD") == null)
            return;

        bool hierarchyOk = KurokageUnifiedHierarchyPass.ApplySilent();
        bool sanitizerOk = KurokageProductionSanitizerPass.ApplySilent();
        if (hierarchyOk && sanitizerOk)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Renkai single production hierarchy consolidated and visual sanitizer applied automatically.");
        }
    }
}
