using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class KurokageHighFidelityQualityPass
{
    private const string RootName = "KUROKAGE_HIGH_FIDELITY_QUALITY";

    public static bool ApplySilent()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        GameObject root = new GameObject(RootName);

        ApplyQualitySettings();
        CreateProbe(root.transform, "REFLECTION_SHIBUYA_ZERO", new Vector3(0f, 7f, -3f), new Vector3(46f, 18f, 72f), 128, 1.0f);
        CreateProbe(root.transform, "REFLECTION_CELESTIAL_ARCHIVE", new Vector3(-34f, 6f, 17f), new Vector3(38f, 16f, 34f), 128, 1.08f);
        CreateProbe(root.transform, "REFLECTION_VOID_REACTOR", new Vector3(34f, 6f, 17f), new Vector3(38f, 16f, 34f), 128, 1.04f);
        CreateProbe(root.transform, "REFLECTION_GHOST_LINE", new Vector3(0f, -1.5f, 26f), new Vector3(34f, 10f, 46f), 64, 0.86f);

        foreach (Camera camera in Object.FindObjectsOfType<Camera>(true))
        {
            if (camera == null) continue;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.useOcclusionCulling = true;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static void ApplyQualitySettings()
    {
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 4);
        QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 105f);
        QualitySettings.shadowCascades = 4;
        QualitySettings.lodBias = Mathf.Max(QualitySettings.lodBias, 1.7f);
        QualitySettings.maximumLODLevel = 0;
        QualitySettings.softParticles = true;
        QualitySettings.realtimeReflectionProbes = true;
        QualitySettings.billboardsFaceCameraPosition = true;
    }

    private static void CreateProbe(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        int resolution,
        float intensity)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        ReflectionProbe probe = go.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
        probe.boxProjection = true;
        probe.size = size;
        probe.resolution = resolution;
        probe.intensity = intensity;
        probe.blendDistance = 5f;
        probe.importance = 2;
        probe.hdr = true;
        probe.clearFlags = ReflectionProbeClearFlags.Skybox;
        probe.nearClipPlane = 0.3f;
        probe.farClipPlane = 110f;
    }
}
