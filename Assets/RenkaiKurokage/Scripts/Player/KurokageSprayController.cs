using System.Collections.Generic;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageSprayController : MonoBehaviour
    {
        [SerializeField] private KeyCode sprayKey = KeyCode.T;
        [SerializeField] private KeyCode cycleKey = KeyCode.G;
        [SerializeField] private float sprayDistance = 5.2f;
        [SerializeField] private float sprayCooldown = 1.4f;
        [SerializeField] private float spraySize = 0.82f;
        [SerializeField] private int maxActiveSprays = 24;

        private readonly Queue<GameObject> activeSprays = new Queue<GameObject>();
        private Camera playerCamera;
        private RenkaiRoundPlayer owner;
        private Material[] sprayMaterials;
        private int selectedPattern;
        private float nextSprayTime;

        private void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
            owner = GetComponent<RenkaiRoundPlayer>();
            sprayMaterials = BuildSprayMaterials();
        }

        private void Update()
        {
            if (owner != null && !owner.isAlive) return;
            if (Input.GetKeyDown(cycleKey))
                selectedPattern = (selectedPattern + 1) % sprayMaterials.Length;
            if (Input.GetKeyDown(sprayKey) && Time.time >= nextSprayTime)
                TrySpray();
        }

        private void TrySpray()
        {
            if (playerCamera == null || sprayMaterials == null || sprayMaterials.Length == 0) return;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, sprayDistance, ~0, QueryTriggerInteraction.Ignore)) return;
            if (hit.collider.GetComponentInParent<RenkaiRoundPlayer>() != null) return;
            if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > 0.82f) return;

            nextSprayTime = Time.time + sprayCooldown;
            GameObject decal = GameObject.CreatePrimitive(PrimitiveType.Quad);
            decal.name = "RENKAI_WALL_SPRAY";
            decal.transform.position = hit.point + hit.normal * 0.012f;
            decal.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            decal.transform.localScale = Vector3.one * spraySize;

            Collider collider = decal.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = decal.GetComponent<Renderer>();
            renderer.sharedMaterial = sprayMaterials[selectedPattern];
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            KurokageSprayLifetime lifetime = decal.AddComponent<KurokageSprayLifetime>();
            lifetime.Configure(90f);
            activeSprays.Enqueue(decal);
            while (activeSprays.Count > maxActiveSprays)
            {
                GameObject oldest = activeSprays.Dequeue();
                if (oldest != null) Destroy(oldest);
            }
        }

        private static Material[] BuildSprayMaterials()
        {
            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) return new Material[0];

            Color[] colors =
            {
                new Color(0.22f, 0.72f, 1f, 0.95f),
                new Color(0.72f, 0.28f, 1f, 0.95f),
                new Color(1f, 0.36f, 0.64f, 0.95f),
                new Color(0.86f, 0.92f, 1f, 0.95f)
            };

            Material[] materials = new Material[4];
            for (int i = 0; i < materials.Length; i++)
            {
                Texture2D texture = BuildPattern(i, colors[i]);
                Material material = new Material(shader) { name = "RENKAI_SPRAY_" + i };
                material.mainTexture = texture;
                material.color = Color.white;
                materials[i] = material;
            }
            return materials;
        }

        private static Texture2D BuildPattern(int pattern, Color color)
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "RENKAI_SPRAY_PATTERN_" + pattern,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
            texture.SetPixels(pixels);

            if (pattern == 0)
            {
                DrawLine(texture, 22, 90, 106, 90, color, 5);
                DrawLine(texture, 32, 88, 32, 28, color, 6);
                DrawLine(texture, 96, 88, 96, 28, color, 6);
                DrawLine(texture, 20, 104, 108, 104, color, 4);
                DrawLine(texture, 45, 68, 83, 68, color, 3);
            }
            else if (pattern == 1)
            {
                DrawLine(texture, 64, 108, 24, 36, color, 6);
                DrawLine(texture, 64, 108, 104, 36, color, 6);
                DrawLine(texture, 24, 36, 64, 54, color, 5);
                DrawLine(texture, 104, 36, 64, 54, color, 5);
                DrawCircle(texture, 64, 64, 16, color, 4);
            }
            else if (pattern == 2)
            {
                DrawCircle(texture, 64, 64, 42, color, 5);
                DrawLine(texture, 38, 92, 92, 38, color, 7);
                DrawLine(texture, 38, 38, 92, 92, color, 3);
                DrawCircle(texture, 64, 64, 10, color, 5);
            }
            else
            {
                DrawLine(texture, 30, 98, 98, 98, color, 5);
                DrawLine(texture, 64, 108, 64, 22, color, 7);
                DrawLine(texture, 30, 74, 98, 74, color, 4);
                DrawLine(texture, 44, 52, 84, 52, color, 4);
                DrawLine(texture, 38, 28, 90, 28, color, 5);
            }

            texture.Apply(false, false);
            return texture;
        }

        private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int thickness)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                Paint(texture, x0, y0, color, thickness);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color, int thickness)
        {
            for (int angle = 0; angle < 360; angle += 2)
            {
                float radians = angle * Mathf.Deg2Rad;
                Paint(texture, cx + Mathf.RoundToInt(Mathf.Cos(radians) * radius), cy + Mathf.RoundToInt(Mathf.Sin(radians) * radius), color, thickness);
            }
        }

        private static void Paint(Texture2D texture, int x, int y, Color color, int thickness)
        {
            int radius = Mathf.Max(1, thickness / 2);
            for (int ix = -radius; ix <= radius; ix++)
            for (int iy = -radius; iy <= radius; iy++)
            {
                int px = x + ix;
                int py = y + iy;
                if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    texture.SetPixel(px, py, color);
            }
        }
    }

    public sealed class KurokageSprayLifetime : MonoBehaviour
    {
        private float expireTime;

        public void Configure(float lifetime)
        {
            expireTime = Time.time + Mathf.Max(1f, lifetime);
        }

        private void Update()
        {
            if (Time.time >= expireTime) Destroy(gameObject);
        }
    }
}
