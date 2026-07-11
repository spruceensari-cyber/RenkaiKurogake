using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class KurokageProductionBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new BuildFailedException("RENKAI production preparation cannot run while Unity is in Play Mode.");

        Scene previousScene = SceneManager.GetActiveScene();
        string previousPath = previousScene.IsValid() ? previousScene.path : string.Empty;

        Scene productionScene = EditorSceneManager.OpenScene(
            KurokageFinalUpgradeInstaller.MainCompetitiveScenePath,
            OpenSceneMode.Single
        );

        if (!productionScene.IsValid() || !productionScene.isLoaded)
            throw new BuildFailedException("RENKAI canonical competitive scene could not be opened.");

        bool passed = KurokageFinalUpgradeInstaller.PrepareProduction(true, out string validationReport);
        if (!passed)
        {
            Debug.LogError(validationReport);
            throw new BuildFailedException(
                "RENKAI production validation failed. Player build was stopped.\n\n" + validationReport
            );
        }

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        bool canonicalEnabled = false;
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == KurokageFinalUpgradeInstaller.MainCompetitiveScenePath && scene.enabled)
            {
                canonicalEnabled = true;
                break;
            }
        }

        if (!canonicalEnabled)
            throw new BuildFailedException(
                "Canonical RENKAI competitive scene is not enabled in Build Settings: " +
                KurokageFinalUpgradeInstaller.MainCompetitiveScenePath
            );

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();

        // Do not reopen another scene before the build; the validated canonical scene stays active.
        Debug.Log("RENKAI production pre-build validation passed.\n" + validationReport);
    }
}
