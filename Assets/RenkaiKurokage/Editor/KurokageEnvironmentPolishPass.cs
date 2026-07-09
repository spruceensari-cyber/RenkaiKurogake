#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Renkai.Kurokage.Editor
{
    public static class KurokageEnvironmentPolishPass
    {
        [MenuItem("Renkai/Visual/Apply Full Kurokage Art Pass")]
        public static void ApplyFullPass()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Kurokage Art Pass", "Exit Play Mode first.", "OK");
                return;
            }

            KurokageCompetitiveVisualPass.Apply();
            RemoveOldGeneratedRoot();

            GameObject root = new GameObject("Kurokage_VisualPass_Generated");
            Undo.RegisterCreatedObjectUndo(root, "Create Kurokage Visual Pass Root");

            AddSiteLighting(root.transform, "A_Site_Trigger", new Color(0.10f, 0.55f, 1f), "A_CelestialArchive");
            AddSiteLighting(root.transform, "B_Site_Trigger", new Color(0.42f, 0.16f, 0.95f), "B_VoidReactor");
            AddMidLighting(root.transform);
            AddSkyline(root.transform);
            AddGuidanceLights(root.transform);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            SceneView.RepaintAll();
            Debug.Log("Kurokage full art pass applied: competitive palette, site identity, skyline silhouettes and route lighting.");
        }

        private static void RemoveOldGeneratedRoot()
        {
            GameObject old = GameObject.Find("Kurokage_VisualPass_Generated");
            if (old != null) Undo.DestroyObjectImmediate(old);
        }

        private static void AddSiteLighting(Transform root, string anchorName, Color color, string prefix)
        {
            GameObject anchor = GameObject.Find(anchorName);
            if (anchor == null) return;

            Vector3 center = anchor.transform.position;
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * 6f, 3.5f, Mathf.Sin(angle) * 6f);
                CreatePointLight(root, prefix + "_Light_" + i, center + offset, color, 4.5f, 12f);
            }

            CreateBeacon(root, prefix + "_Beacon", center + Vector3.up * 4f, color);
        }

        private static void AddMidLighting(Transform root)
        {
            Vector3 center = Vector3.zero;
            GameObject mid = FindFirst("V2.4_Mid", "Mid", "Center");
            if (mid != null) center = mid.transform.position;

            CreatePointLight(root, "Mid_ShiftBlue_0", center + new Vector3(-7f, 3f, 0f), new Color(0.08f, 0.45f, 1f), 3.2f, 11f);
            CreatePointLight(root, "Mid_ShiftBlue_1", center + new Vector3(7f, 3f, 0f), new Color(0.08f, 0.45f, 1f), 3.2f, 11f);
        }

        private static void AddSkyline(Transform root)
        {
            Vector3[] positions =
            {
                new Vector3(-48f, 18f, 42f),
                new Vector3(44f, 24f, 48f),
                new Vector3(-58f, 28f, -36f),
                new Vector3(56f, 20f, -44f)
            };

            Vector3[] scales =
            {
                new Vector3(12f, 42f, 10f),
                new Vector3(15f, 55f, 13f),
                new Vector3(10f, 62f, 14f),
                new Vector3(18f, 48f, 12f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tower.name = "Distant_Megastructure_" + i;
                tower.transform.SetParent(root);
                tower.transform.position = positions[i];
                tower.transform.localScale = scales[i];
                Collider collider = tower.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }
        }

        private static void AddGuidanceLights(Transform root)
        {
            Vector3[] points =
            {
                new Vector3(-14f, 0.35f, 0f),
                new Vector3(-7f, 0.35f, 0f),
                new Vector3(0f, 0.35f, 0f),
                new Vector3(7f, 0.35f, 0f),
                new Vector3(14f, 0.35f, 0f)
            };

            for (int i = 0; i < points.Length; i++)
                CreatePointLight(root, "RouteGuide_" + i, points[i], new Color(0.05f, 0.32f, 0.78f), 0.75f, 4f);
        }

        private static void CreateBeacon(Transform parent, string name, Vector3 position, Color color)
        {
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = name;
            beacon.transform.SetParent(parent);
            beacon.transform.position = position;
            beacon.transform.localScale = new Vector3(0.18f, 4f, 0.18f);
            Collider collider = beacon.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);

            Renderer renderer = beacon.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            Material material = new Material(shader);
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static GameObject FindFirst(params string[] names)
        {
            foreach (string name in names)
            {
                GameObject go = GameObject.Find(name);
                if (go != null) return go;
            }
            return null;
        }
    }
}
#endif
