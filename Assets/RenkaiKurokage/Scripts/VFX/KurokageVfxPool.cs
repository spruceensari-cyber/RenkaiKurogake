using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renkai.Kurokage
{
    public enum KurokageVfxShape
    {
        Cube,
        Sphere,
        Cylinder
    }

    public sealed class KurokageVfxPool : MonoBehaviour
    {
        private sealed class PooledEntry
        {
            public GameObject GameObject;
            public Renderer Renderer;
            public float ReleaseTime;
            public bool Active;
        }

        private static KurokageVfxPool instance;
        private readonly Dictionary<KurokageVfxShape, Queue<PooledEntry>> available = new Dictionary<KurokageVfxShape, Queue<PooledEntry>>();
        private readonly List<PooledEntry> active = new List<PooledEntry>();
        private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

        public static KurokageVfxPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<KurokageVfxPool>();
                    if (instance == null)
                    {
                        GameObject root = new GameObject("KUROKAGE_VFX_POOL");
                        instance = root.AddComponent<KurokageVfxPool>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            EnsureQueue(KurokageVfxShape.Cube);
            EnsureQueue(KurokageVfxShape.Sphere);
            EnsureQueue(KurokageVfxShape.Cylinder);
        }

        private void Update()
        {
            float now = Time.time;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                PooledEntry entry = active[i];
                if (!entry.Active || now < entry.ReleaseTime) continue;
                Release(entry);
                active.RemoveAt(i);
            }
        }

        public GameObject Spawn(
            KurokageVfxShape shape,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Color color,
            float emission,
            float lifetime)
        {
            Queue<PooledEntry> queue = EnsureQueue(shape);
            PooledEntry entry = queue.Count > 0 ? queue.Dequeue() : CreateEntry(shape);
            entry.Active = true;
            entry.ReleaseTime = Time.time + Mathf.Max(0.01f, lifetime);
            entry.GameObject.name = name;
            entry.GameObject.transform.position = position;
            entry.GameObject.transform.rotation = rotation;
            entry.GameObject.transform.localScale = scale;

            Material material = GetMaterial(color, emission);
            if (entry.Renderer != null && material != null)
                entry.Renderer.sharedMaterial = material;

            entry.GameObject.SetActive(true);
            active.Add(entry);
            return entry.GameObject;
        }

        public void ClearAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
                Release(active[i]);
            active.Clear();
        }

        private Queue<PooledEntry> EnsureQueue(KurokageVfxShape shape)
        {
            Queue<PooledEntry> queue;
            if (!available.TryGetValue(shape, out queue))
            {
                queue = new Queue<PooledEntry>();
                available.Add(shape, queue);
            }
            return queue;
        }

        private PooledEntry CreateEntry(KurokageVfxShape shape)
        {
            PrimitiveType primitive = PrimitiveType.Cube;
            if (shape == KurokageVfxShape.Sphere) primitive = PrimitiveType.Sphere;
            else if (shape == KurokageVfxShape.Cylinder) primitive = PrimitiveType.Cylinder;

            GameObject go = GameObject.CreatePrimitive(primitive);
            go.transform.SetParent(transform, false);

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            PooledEntry entry = new PooledEntry
            {
                GameObject = go,
                Renderer = go.GetComponent<Renderer>(),
                Active = false
            };
            go.SetActive(false);
            go.AddComponent<KurokagePooledVfxTag>().Shape = shape;
            return entry;
        }

        private void Release(PooledEntry entry)
        {
            if (entry == null || entry.GameObject == null) return;
            entry.Active = false;
            entry.GameObject.SetActive(false);
            KurokagePooledVfxTag tag = entry.GameObject.GetComponent<KurokagePooledVfxTag>();
            KurokageVfxShape shape = tag != null ? tag.Shape : KurokageVfxShape.Cube;
            EnsureQueue(shape).Enqueue(entry);
        }

        private Material GetMaterial(Color color, float emission)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color) + "_" + emission.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            Material material;
            if (materialCache.TryGetValue(key, out material)) return material;

            bool srp = GraphicsSettings.currentRenderPipeline != null;
            Shader shader = srp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Diffuse");
            if (shader == null)
            {
                Debug.LogError("KurokageVfxPool could not resolve a compatible shader.");
                return null;
            }

            material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            materialCache.Add(key, material);
            return material;
        }
    }

    public sealed class KurokagePooledVfxTag : MonoBehaviour
    {
        public KurokageVfxShape Shape;
    }
}
