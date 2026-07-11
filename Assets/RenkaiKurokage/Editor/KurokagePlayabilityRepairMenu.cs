using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class KurokagePlayabilityRepairMenu
{
    [MenuItem("Renkai/Repair Playability and Collision")]
    public static void Run()
    {
        bool surfaceOk = KurokageSurfaceDetailPass.ApplySilent();
        bool qualityOk = KurokageHighFidelityQualityPass.ApplySilent();
        bool collisionOk = KurokageCollisionIntegrityPass.ApplySilent();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        bool passed = surfaceOk && qualityOk && collisionOk;
        EditorUtility.DisplayDialog(
            "Renkai Playability Repair",
            passed
                ? "Collision integrity, moving-solid physics, controller protection, PBR surface detail and local reflection capture applied."
                : "Repair completed with review items. Check Console for the failing pass.",
            passed ? "OK" : "REVIEW"
        );
    }
}
