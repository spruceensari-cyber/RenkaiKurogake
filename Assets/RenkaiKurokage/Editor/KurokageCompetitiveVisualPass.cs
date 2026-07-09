#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Renkai.Kurokage.Editor
{
    public static class KurokageCompetitiveVisualPass
    {
        private const string MaterialRoot = "Assets/RenkaiKurokage/Art/GeneratedMaterials";

        [MenuItem("Renkai/Visual/Apply Competitive Visual Pass")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Kurokage Visual Pass", "Exit Play Mode first.", "OK");
                return;
            }

            EnsureFolders();

            Material floor = GetOrCreateMaterial("M_Floor_Competitive", new Color(0.18f, 0.20f, 0.24f), 0.05f, 0.58f);
            Material wallDark = GetOrCreateMaterial("M_Wall_DarkCeramic", new Color(0.08f, 0.10f, 0.14f), 0.1f, 0.72f);
            Material wallLight = GetOrCreateMaterial("M_Wall_LightComposite", new Color(0.58f, 0.63f, 0.70f), 0.02f, 0.48f);
            Material cover = GetOrCreateMaterial("M_Cover_Readable", new Color(0.22f, 0.26f, 0.32f), 0.18f, 0.55f);
            Material accentBlue = GetOrCreateMaterial("M_Accent_Blue", new Color(0.06f, 0.42f, 0.90f), 0.0f, 0.35f);
            Material accentViolet = GetOrCreateMaterial("M_Accent_Violet", new Color(0.34f, 0.12f, 0.62f), 0.0f, 0.38f);

            int changed = 0;
            foreach (Renderer renderer in Object.FindObjectsOfType<Renderer>(true))
            {
                string n = renderer.gameObject.name.ToLowerInvariant();
                Material target = null;

                if (ContainsAny(n, "floor", "ground", "road", "street")) target = floor;
                else if (ContainsAny(n, "cover", "crate", "box", "barrier", "block")) target = cover;
                else if (ContainsAny(n, "wall", "building", "tower", "structure"))
                    target = renderer.bounds.size.y > 5f ? wallDark : wallLight;
                else if (ContainsAny(n, "neon", "holo", "energy", "accent", "sign"))
                    target = (changed % 3 == 0) ? accentViolet : accentBlue;

                if (target == null) continue;
                Undo.RecordObject(renderer, "Apply Kurokage Competitive Material");
                renderer.sharedMaterial = target;
                changed++;
            }

            SetupLighting();
            SetupCameras();
            SetupFog();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            SceneView.RepaintAll();
            Debug.Log($"Kurokage competitive visual pass applied. Renderers changed: {changed}");
        }

        private static void SetupLighting()
        {
            Light sun = null;
            foreach (Light light in Object.FindObjectsOfType<Light>(true))
            {
                if (light.type == LightType.Directional)
                {
                    sun = light;
                    break;
                }
            }

            if (sun == null)
            {
                GameObject go = new GameObject("Kurogake_Sun");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
                go.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
                Undo.RegisterCreatedObjectUndo(go, "Create Kurogake Sun");
            }

            Undo.RecordObject(sun, "Tune Kurogake Sun");
            sun.color = new Color(0.86f, 0.91f, 1f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.82f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.24f, 0.31f, 0.43f);
            RenderSettings.ambientEquatorColor = new Color(0.13f, 0.16f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.06f, 0.075f);
            RenderSettings.ambientIntensity = 0.9f;
        }

        private static void SetupFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.10f, 0.13f, 0.19f);
            RenderSettings.fogDensity = 0.0045f;
        }

        private static void SetupCameras()
        {
            foreach (Camera cam in Object.FindObjectsOfType<Camera>(true))
            {
                Undo.RecordObject(cam, "Tune Competitive Camera");
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.allowHDR = true;
                cam.allowMSAA = true;
                if (cam.fieldOfView < 70f || cam.fieldOfView > 110f)
                    cam.fieldOfView = 90f;
            }
        }

        private static Material GetOrCreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            foreach (string token in tokens)
                if (value.Contains(token)) return true;
            return false;
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets/RenkaiKurokage", "Art");
            CreateFolderIfMissing("Assets/RenkaiKurokage/Art", "GeneratedMaterials");
        }

        private static void CreateFolderIfMissing(string parent, string child)
        {
            string full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
