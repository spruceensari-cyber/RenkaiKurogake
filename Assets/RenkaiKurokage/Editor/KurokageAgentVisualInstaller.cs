using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageAgentVisualInstaller
{
    private static readonly string[] CharacterPaths =
    {
        "Assets/Renkai/Characters/CrimsonBlossomWarrior/CrimsonBlossomWarrior_Alert.fbx",
        "Assets/Renkai/Characters/CrimsonBlossomWarrior/CrimsonBlossomWarrior_Agree.fbx",
        "Assets/Renkai/Characters/NocturneEmpress/NocturneEmpress_Alert.fbx",
        "Assets/Renkai/Characters/NocturneEmpress/NocturneEmpress_Agree.fbx"
    };

    [MenuItem("Renkai/Install High-End Agent Visuals")]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        bool ok = InstallSilent();
        EditorUtility.DisplayDialog(
            "Renkai",
            ok ? "Karakter görselleri ve competitive hit zones uygulandı." : "Karakter görselleri uygulanamadı. FBX asset yollarını ve 5v5 kurulumunu kontrol et.",
            ok ? "OK" : "REVIEW"
        );
    }

    public static bool InstallSilent()
    {
        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        if (players == null || players.Length == 0) return false;

        int index = 0;
        bool allSucceeded = true;
        foreach (RenkaiRoundPlayer rp in players)
        {
            bool applied = ApplyVisual(rp, CharacterPaths[index % CharacterPaths.Length]);
            if (!applied) allSucceeded = false;
            index++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = players[0].gameObject;
        return allSucceeded;
    }

    private static bool ApplyVisual(RenkaiRoundPlayer roundPlayer, string assetPath)
    {
        Transform old = roundPlayer.transform.Find("AGENT_VISUAL");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            Debug.LogError("Renkai agent visual missing: " + assetPath + " for " + roundPlayer.agentName);
            return false;
        }

        foreach (Renderer r in roundPlayer.GetComponents<Renderer>())
            r.enabled = false;

        GameObject visualRoot = new GameObject("AGENT_VISUAL");
        visualRoot.transform.SetParent(roundPlayer.transform, false);
        visualRoot.transform.localPosition = Vector3.zero;
        visualRoot.transform.localRotation = Quaternion.identity;
        visualRoot.transform.localScale = new Vector3(1.055f, 1f, 1.035f);

        Object spawned = PrefabUtility.InstantiatePrefab(asset);
        GameObject instance = spawned as GameObject;
        if (instance == null)
        {
            Debug.LogError("Renkai could not instantiate FBX visual: " + assetPath);
            Object.DestroyImmediate(visualRoot);
            return false;
        }

        instance.transform.SetParent(visualRoot.transform, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        instance.transform.localScale = Vector3.one;

        NormalizeVisual(instance.transform, 1.82f);

        Animator animator = instance.GetComponentInChildren<Animator>();
        if (animator != null)
            animator.applyRootMotion = false;

        foreach (Collider c in instance.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        Color accent = roundPlayer.team == RenkaiTeam.Attackers
            ? new Color(0.14f, 0.50f, 1f, 1f)
            : new Color(0.62f, 0.32f, 0.92f, 1f);

        KurokageAgentReadabilityPresenter readability = visualRoot.GetComponent<KurokageAgentReadabilityPresenter>();
        if (readability == null) readability = visualRoot.AddComponent<KurokageAgentReadabilityPresenter>();
        readability.Configure(accent, 0.18f);

        if (visualRoot.GetComponent<KurokageAgentAnimationDriver>() == null)
            visualRoot.AddComponent<KurokageAgentAnimationDriver>();

        KurokageHitReactionPresenter hitReaction = roundPlayer.GetComponent<KurokageHitReactionPresenter>();
        if (hitReaction == null)
            roundPlayer.gameObject.AddComponent<KurokageHitReactionPresenter>();

        KurokageHitZoneBinder binder = roundPlayer.GetComponent<KurokageHitZoneBinder>();
        if (binder == null) binder = roundPlayer.gameObject.AddComponent<KurokageHitZoneBinder>();
        binder.Bind();

        return binder.BoundZoneCount >= 3;
    }

    private static void NormalizeVisual(Transform visual, float targetHeight)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float height = Mathf.Max(0.01f, bounds.size.y);
        float scale = targetHeight / height;
        visual.localScale = Vector3.one * scale;

        renderers = visual.GetComponentsInChildren<Renderer>(true);
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float parentY = visual.parent.position.y;
        float offset = parentY - bounds.min.y;
        visual.position += Vector3.up * offset;
    }
}
