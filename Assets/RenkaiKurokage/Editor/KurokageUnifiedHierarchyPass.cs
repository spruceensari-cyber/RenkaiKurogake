using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;

public static class KurokageUnifiedHierarchyPass
{
    public const string UnifiedRootName = "RENKAI_KUROKAGE_UNIFIED";

    public static bool ApplySilent()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return false;

        GameObject unifiedRoot = FindOrCreateUniqueRoot(scene, UnifiedRootName);
        Transform map = FindOrCreateChild(unifiedRoot.transform, "MAP").transform;
        Transform gameplay = FindOrCreateChild(unifiedRoot.transform, "GAMEPLAY").transform;
        Transform players = FindOrCreateChild(unifiedRoot.transform, "PLAYERS").transform;
        Transform objective = FindOrCreateChild(unifiedRoot.transform, "OBJECTIVE").transform;
        Transform presentation = FindOrCreateChild(unifiedRoot.transform, "PRESENTATION").transform;
        Transform hud = FindOrCreateChild(unifiedRoot.transform, "HUD").transform;
        Transform vfx = FindOrCreateChild(unifiedRoot.transform, "VFX").transform;
        Transform audio = FindOrCreateChild(unifiedRoot.transform, "AUDIO").transform;
        Transform validation = FindOrCreateChild(unifiedRoot.transform, "VALIDATION").transform;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == null || root == unifiedRoot) continue;
            Transform target = ResolveParent(root, map, gameplay, players, objective, presentation, hud, vfx, audio, validation);
            if (target != null) root.transform.SetParent(target, true);
        }

        // Players can be nested under an old gameplay root. Move the authoritative player roots directly.
        foreach (RenkaiRoundPlayer player in UnityEngine.Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null) continue;
            player.transform.SetParent(players, true);
        }

        RemoveLegacyContainer(unifiedRoot.transform, "PRODUCTION_SYSTEMS");
        EditorSceneManager.MarkSceneDirty(scene);
        return true;
    }

    private static GameObject FindOrCreateUniqueRoot(Scene scene, string name)
    {
        GameObject keeper = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == null || root.name != name) continue;
            if (keeper == null) keeper = root;
            else UnityEngine.Object.DestroyImmediate(root);
        }

        if (keeper == null) keeper = new GameObject(name);
        keeper.transform.SetParent(null);
        return keeper;
    }

    private static Transform ResolveParent(
        GameObject root,
        Transform map,
        Transform gameplay,
        Transform players,
        Transform objective,
        Transform presentation,
        Transform hud,
        Transform vfx,
        Transform audio,
        Transform validation)
    {
        string name = root.name;

        if (root.GetComponent<RenkaiRoundPlayer>() != null ||
            name.StartsWith("PLAYER", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("BOT", StringComparison.OrdinalIgnoreCase))
            return players;

        if (name == "MAP" || name.IndexOf("MAP", StringComparison.OrdinalIgnoreCase) >= 0 &&
            name.IndexOf("MINIMAP", StringComparison.OrdinalIgnoreCase) < 0)
            return map;

        if (name.IndexOf("HUD", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("SCOREBOARD", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("MATCH_STATS", StringComparison.OrdinalIgnoreCase) >= 0)
            return hud;

        if (name.IndexOf("ZODIAC", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("NEXUS", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("CORE", StringComparison.OrdinalIgnoreCase) >= 0)
            return objective;

        if (name.IndexOf("VFX", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("PARTICLE", StringComparison.OrdinalIgnoreCase) >= 0)
            return vfx;

        if (name.IndexOf("AUDIO", StringComparison.OrdinalIgnoreCase) >= 0)
            return audio;

        if (name.IndexOf("VALIDATION", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name == "RENKAI_KUROKAGE_PRODUCTION_BUILD")
            return validation;

        if (name.IndexOf("ENVIRONMENT", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("VISUAL", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("ARCHITECTURE", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("LIGHT", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("CINEMATIC", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("DISTRICT", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("SKY", StringComparison.OrdinalIgnoreCase) >= 0)
            return presentation;

        if (name.StartsWith("KUROKAGE_", StringComparison.OrdinalIgnoreCase) ||
            name.IndexOf("ROUND", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("GAME", StringComparison.OrdinalIgnoreCase) >= 0)
            return gameplay;

        return null;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static void RemoveLegacyContainer(Transform root, string name)
    {
        Transform legacy = root.Find(name);
        if (legacy == null) return;
        while (legacy.childCount > 0)
            legacy.GetChild(0).SetParent(root, true);
        UnityEngine.Object.DestroyImmediate(legacy.gameObject);
    }
}
