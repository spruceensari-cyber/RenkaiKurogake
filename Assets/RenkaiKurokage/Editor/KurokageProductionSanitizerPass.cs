using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageProductionSanitizerPass
{
    private static readonly string[] UniqueRoots =
    {
        "RENKAI_KUROKAGE_UNIFIED",
        "RENKAI_KUROKAGE_PRODUCTION_BUILD",
        "KUROKAGE_ENVIRONMENT_ART",
        "KUROKAGE_BRIGHT_VISUAL_PASS",
        "KUROKAGE_CINEMATIC_PRESENTATION",
        "KUROKAGE_COMPETITIVE_ARCHITECTURE",
        "KUROKAGE_DISTRICT_IDENTITY",
        "KUROKAGE_SITE_READABILITY_LIGHTING",
        "KUROKAGE_VFX_POOL",
        "KUROKAGE_MATCH_STATS",
        "KUROKAGE_ZODIAC_OBJECTIVE"
    };

    private static readonly string[] PlaceholderNames =
    {
        "AgentBlip",
        "AGENT_BLIP",
        "BotCapsule",
        "BOT_CAPSULE",
        "CharacterCapsule",
        "CHARACTER_CAPSULE",
        "DebugCapsule",
        "DEBUG_CAPSULE",
        "PlaceholderCapsule",
        "PLACEHOLDER_CAPSULE"
    };

    // Kept as a callable compatibility entry point for the unified production pipeline.
    // It intentionally has no MenuItem attribute; the only normal production command is
    // Renkai/Build Production Version.
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
        {
            Debug.LogWarning("RENKAI production sanitizer skipped because Unity is in Play Mode.");
            return;
        }

        bool ok = ApplySilent();
        EditorUtility.DisplayDialog(
            "Renkai Visual Repair",
            ok
                ? "Tek production görünümü temizlendi: duplicate rootlar kaldırıldı, placeholder kapsüller gizlendi ve materyaller aktif render pipeline'a uyarlandı."
                : "Visual repair tamamlanamadı. Console'u kontrol et.",
            ok ? "OK" : "REVIEW"
        );
    }

    public static bool ApplySilent()
    {
        // This class performs editor-only destructive scene and asset operations.
        // Returning successfully during play prevents delayed editor callbacks from
        // calling MarkSceneDirty/DestroyImmediate after the game has already started.
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            return true;

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogError("Kurokage production sanitizer could not resolve a loaded active scene.");
            return false;
        }

        try
        {
            RemoveDuplicateRoots();
            RemovePlaceholderObjects();
            HidePlayerPlaceholderRenderers();
            RepairSceneMaterials();
            RepairGeneratedMaterialAssets();
            EnsureSingleCameraAndAudioListener();

            EditorSceneManager.MarkSceneDirty(activeScene);
            AssetDatabase.SaveAssets();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Kurokage production sanitizer failed: " + exception);
            return false;
        }
    }

    private static void RemoveDuplicateRoots()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return;

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (string uniqueName in UniqueRoots)
        {
            GameObject keeper = null;
            foreach (GameObject root in roots)
            {
                if (root == null || root.name != uniqueName) continue;
                if (keeper == null)
                {
                    keeper = root;
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    private static void RemovePlaceholderObjects()
    {
        Transform[] allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
        List<GameObject> remove = new List<GameObject>();

        foreach (Transform transform in allTransforms)
        {
            if (transform == null) continue;
            string objectName = transform.name;
            if (!IsPlaceholderName(objectName)) continue;
            if (transform.GetComponentInParent<KurokageProceduralAgentRig>() != null) continue;
            remove.Add(transform.gameObject);
        }

        foreach (GameObject go in remove)
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
    }

    private static bool IsPlaceholderName(string objectName)
    {
        foreach (string candidate in PlaceholderNames)
        {
            if (string.Equals(objectName, candidate, StringComparison.OrdinalIgnoreCase)) return true;
            if (objectName.StartsWith(candidate + "_", StringComparison.OrdinalIgnoreCase)) return true;
        }
        return objectName.IndexOf("AgentBlip", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void HidePlayerPlaceholderRenderers()
    {
        foreach (RenkaiRoundPlayer player in UnityEngine.Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null) continue;
            Transform visualRoot = player.transform.Find("AGENT_VISUAL");

            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                if (visualRoot != null && renderer.transform.IsChildOf(visualRoot)) continue;
                if (player.isHumanPlayer && IsFirstPersonRenderer(renderer.transform)) continue;

                string name = renderer.gameObject.name;
                bool rootPrimitive = renderer.transform == player.transform;
                bool placeholder = rootPrimitive ||
                                   name.IndexOf("capsule", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   name.IndexOf("blip", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   name.IndexOf("placeholder", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   name.IndexOf("debug", StringComparison.OrdinalIgnoreCase) >= 0;

                if (placeholder) renderer.enabled = false;
            }
        }
    }

    private static bool IsFirstPersonRenderer(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            string name = current.name;
            if (name.IndexOf("viewmodel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("weapon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("rifle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("pistol", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("sword", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("blade", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("hand", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            current = current.parent;
        }
        return false;
    }

    private static void RepairSceneMaterials()
    {
        Shader targetShader = ResolveLitShader();
        if (targetShader == null) return;

        HashSet<Material> visited = new HashSet<Material>();
        foreach (Renderer renderer in UnityEngine.Object.FindObjectsOfType<Renderer>(true))
        {
            if (renderer == null) continue;
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    material = CreateFallbackMaterial(targetShader, renderer.gameObject.name);
                    materials[i] = material;
                    changed = true;
                }
                else if (visited.Add(material) && NeedsShaderRepair(material))
                {
                    PreserveAndSwitchShader(material, targetShader);
                    changed = true;
                }
            }

            if (changed) renderer.sharedMaterials = materials;
        }
    }

    private static void RepairGeneratedMaterialAssets()
    {
        Shader targetShader = ResolveLitShader();
        if (targetShader == null) return;

        string[] folders =
        {
            "Assets/RenkaiKurokage/Art/GeneratedMaterials",
            "Assets/RenkaiKurokage/Art/OriginalAgentMaterials"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder)) continue;
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) continue;

                if (NeedsShaderRepair(material)) PreserveAndSwitchShader(material, targetShader);
                ApplyKnownMaterialDefaults(material);
                EditorUtility.SetDirty(material);
            }
        }
    }

    private static bool NeedsShaderRepair(Material material)
    {
        if (material == null || material.shader == null) return true;
        string shaderName = material.shader.name;
        if (shaderName == "Hidden/InternalErrorShader") return true;

        bool usingPipeline = GraphicsSettings.currentRenderPipeline != null;
        if (usingPipeline)
            return shaderName == "Standard" || shaderName == "Diffuse";

        return shaderName.StartsWith("Universal Render Pipeline/", StringComparison.OrdinalIgnoreCase) ||
               shaderName.StartsWith("HDRP/", StringComparison.OrdinalIgnoreCase) ||
               shaderName.StartsWith("High Definition Render Pipeline/", StringComparison.OrdinalIgnoreCase);
    }

    private static void PreserveAndSwitchShader(Material material, Shader targetShader)
    {
        Color color = ReadColor(material);
        Color emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
        float metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0.08f;
        float smoothness = material.HasProperty("_Smoothness")
            ? material.GetFloat("_Smoothness")
            : material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.42f;

        material.shader = targetShader;
        WriteColor(material, color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0.001f) material.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(material);
    }

    private static void ApplyKnownMaterialDefaults(Material material)
    {
        string name = material.name.ToLowerInvariant();
        if (name.Contains("floor") || name.Contains("street"))
        {
            WriteColor(material, new Color(0.055f, 0.075f, 0.11f, 1f));
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.36f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.68f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.68f);
        }
        else if (name.Contains("lightcomposite"))
        {
            WriteColor(material, new Color(0.72f, 0.78f, 0.86f, 1f));
        }
        else if (name.Contains("darkceramic"))
        {
            WriteColor(material, new Color(0.035f, 0.05f, 0.08f, 1f));
        }
    }

    private static Color ReadColor(Material material)
    {
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color")) return material.GetColor("_Color");
        return Color.white;
    }

    private static void WriteColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static Material CreateFallbackMaterial(Shader shader, string ownerName)
    {
        string safeName = string.IsNullOrEmpty(ownerName) ? "Unknown" : ownerName.Replace(' ', '_');
        string folder = "Assets/RenkaiKurokage/Art/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder("Assets/RenkaiKurokage/Art"))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage", "Art");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage/Art", "GeneratedMaterials");

        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/M_Fallback_" + safeName + ".mat");
        Material material = new Material(shader) { name = "M_Fallback_" + safeName };
        WriteColor(material, new Color(0.18f, 0.22f, 0.29f, 1f));
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Shader ResolveLitShader()
    {
        if (GraphicsSettings.currentRenderPipeline != null)
        {
            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp != null) return urp;
            Shader hdrp = Shader.Find("HDRP/Lit");
            if (hdrp != null) return hdrp;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null) return standard;
        return Shader.Find("Diffuse");
    }

    private static void EnsureSingleCameraAndAudioListener()
    {
        Camera keeperCamera = Camera.main;
        if (keeperCamera == null)
        {
            foreach (Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>(true))
            {
                if (camera != null && camera.enabled)
                {
                    keeperCamera = camera;
                    break;
                }
            }
        }

        foreach (Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>(true))
        {
            if (camera != null && camera != keeperCamera) camera.enabled = false;
        }

        AudioListener keeperListener = keeperCamera != null ? keeperCamera.GetComponent<AudioListener>() : null;
        if (keeperCamera != null && keeperListener == null)
            keeperListener = keeperCamera.gameObject.AddComponent<AudioListener>();

        foreach (AudioListener listener in UnityEngine.Object.FindObjectsOfType<AudioListener>(true))
        {
            if (listener != null && listener != keeperListener) listener.enabled = false;
        }
    }
}
