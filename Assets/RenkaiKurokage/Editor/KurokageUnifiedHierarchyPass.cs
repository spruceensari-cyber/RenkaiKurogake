using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KurokageUnifiedHierarchyPass
{
    private const string UnifiedRootName = "RENKAI_KUROKAGE_UNIFIED";

    public static bool ApplySilent()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return false;

        GameObject unifiedRoot = GameObject.Find(UnifiedRootName);
        if (unifiedRoot == null) unifiedRoot = new GameObject(UnifiedRootName);
        unifiedRoot.transform.SetParent(null);

        GameObject systems = FindOrCreateChild(unifiedRoot.transform, "PRODUCTION_SYSTEMS");
        GameObject presentation = FindOrCreateChild(unifiedRoot.transform, "PRESENTATION");
        GameObject hud = FindOrCreateChild(unifiedRoot.transform, "HUD");
        GameObject objective = FindOrCreateChild(unifiedRoot.transform, "OBJECTIVE");

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == null || root == unifiedRoot) continue;
            string name = root.name;

            if (name.StartsWith("KUROKAGE_", StringComparison.OrdinalIgnoreCase) ||
                name == "RENKAI_KUROKAGE_PRODUCTION_BUILD")
            {
                Transform parent = ResolveParent(name, systems.transform, presentation.transform, hud.transform, objective.transform);
                root.transform.SetParent(parent, true);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        return true;
    }

    private static Transform ResolveParent(string name, Transform systems, Transform presentation, Transform hud, Transform objective)
    {
        if (name.IndexOf("HUD", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("SCOREBOARD", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("MATCH_STATS", StringComparison.OrdinalIgnoreCase) >= 0)
            return hud;

        if (name.IndexOf("ZODIAC", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("NEXUS", StringComparison.OrdinalIgnoreCase) >= 0)
            return objective;

        if (name.IndexOf("ENVIRONMENT", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("VISUAL", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("ARCHITECTURE", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("LIGHT", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("CINEMATIC", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("DISTRICT", StringComparison.OrdinalIgnoreCase) >= 0)
            return presentation;

        return systems;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }
}
