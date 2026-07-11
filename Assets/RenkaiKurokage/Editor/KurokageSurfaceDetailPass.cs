using System.IO;
using UnityEditor;
using UnityEngine;

public static class KurokageSurfaceDetailPass
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/GeneratedMaterials/";
    private const string TextureFolder = "Assets/RenkaiKurokage/Art/GeneratedSurfaceTextures";
    private const int TextureSize = 256;

    public static bool ApplySilent()
    {
        EnsureFolder("Assets/RenkaiKurokage/Art");
        EnsureFolder(TextureFolder);

        int applied = 0;
        applied += ApplyMaterialSet("M_DarkCeramic", "DarkCeramic", 11, 5, 7, 0.72f, 0.22f, 5.5f) ? 1 : 0;
        applied += ApplyMaterialSet("M_LightComposite", "LightComposite", 23, 4, 6, 0.38f, 0.12f, 4.5f) ? 1 : 0;
        applied += ApplyMaterialSet("M_NavyMetal", "NavyMetal", 41, 7, 9, 0.82f, 0.72f, 6.5f) ? 1 : 0;
        applied += ApplyMaterialSet("M_CoverNeutral", "CoverNeutral", 67, 3, 5, 0.46f, 0.28f, 3.5f) ? 1 : 0;
        applied += ApplyMaterialSet("M_Floor_Competitive", "CompetitiveFloor", 89, 8, 10, 0.54f, 0.20f, 8f) ? 1 : 0;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Kurokage Surface Detail: applied PBR texture sets to " + applied + " shared materials.");
        return applied >= 4;
    }

    private static bool ApplyMaterialSet(
        string materialName,
        string textureStem,
        int seed,
        int panelsX,
        int panelsY,
        float smoothness,
        float metallic,
        float tiling)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + materialName + ".mat");
        if (material == null) return false;

        Texture2D detail = EnsureDetailTexture(textureStem, seed, panelsX, panelsY);
        Texture2D normal = EnsureNormalTexture(textureStem, seed, panelsX, panelsY);
        Texture2D mask = EnsureMaskTexture(textureStem, seed, panelsX, panelsY, metallic, smoothness);
        if (detail == null || normal == null || mask == null) return false;

        SetTexture(material, "_MainTex", detail, tiling);
        SetTexture(material, "_BaseMap", detail, tiling);

        if (material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normal);
            material.SetTextureScale("_BumpMap", Vector2.one * tiling);
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 0.56f);
            material.EnableKeyword("_NORMALMAP");
        }

        if (material.HasProperty("_MetallicGlossMap"))
        {
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTextureScale("_MetallicGlossMap", Vector2.one * tiling);
            material.EnableKeyword("_METALLICGLOSSMAP");
        }

        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);

        EditorUtility.SetDirty(material);
        return true;
    }

    private static Texture2D EnsureDetailTexture(string stem, int seed, int panelsX, int panelsY)
    {
        string path = TextureFolder + "/T_" + stem + "_Detail.png";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;

        Texture2D texture = NewTexture(false);
        Color[] pixels = new Color[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float h = HeightSample(x, y, seed, panelsX, panelsY);
                float value = Mathf.Clamp01(0.90f + (h - 0.5f) * 0.24f);
                pixels[y * TextureSize + x] = new Color(value, value, value, 1f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        WritePngAndImport(texture, path, false, true);
        Object.DestroyImmediate(texture);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Texture2D EnsureNormalTexture(string stem, int seed, int panelsX, int panelsY)
    {
        string path = TextureFolder + "/T_" + stem + "_Normal.png";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;

        Texture2D texture = NewTexture(true);
        Color[] pixels = new Color[TextureSize * TextureSize];
        const float strength = 5.2f;

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float left = HeightSample(Wrap(x - 1), y, seed, panelsX, panelsY);
                float right = HeightSample(Wrap(x + 1), y, seed, panelsX, panelsY);
                float down = HeightSample(x, Wrap(y - 1), seed, panelsX, panelsY);
                float up = HeightSample(x, Wrap(y + 1), seed, panelsX, panelsY);

                Vector3 normal = new Vector3((left - right) * strength, (down - up) * strength, 1f).normalized;
                pixels[y * TextureSize + x] = new Color(
                    normal.x * 0.5f + 0.5f,
                    normal.y * 0.5f + 0.5f,
                    normal.z * 0.5f + 0.5f,
                    1f
                );
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        WritePngAndImport(texture, path, true, false);
        Object.DestroyImmediate(texture);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Texture2D EnsureMaskTexture(
        string stem,
        int seed,
        int panelsX,
        int panelsY,
        float metallic,
        float smoothness)
    {
        string path = TextureFolder + "/T_" + stem + "_Mask.png";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;

        Texture2D texture = NewTexture(true);
        Color[] pixels = new Color[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float h = HeightSample(x, y, seed, panelsX, panelsY);
                float localSmoothness = Mathf.Clamp01(smoothness + (h - 0.5f) * 0.18f);
                pixels[y * TextureSize + x] = new Color(metallic, metallic, metallic, localSmoothness);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        WritePngAndImport(texture, path, false, false);
        Object.DestroyImmediate(texture);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static float HeightSample(int x, int y, int seed, int panelsX, int panelsY)
    {
        float u = x / (float)TextureSize;
        float v = y / (float)TextureSize;

        float wave = Mathf.Sin((u * panelsX + seed * 0.013f) * Mathf.PI * 2f) *
                     Mathf.Sin((v * panelsY + seed * 0.021f) * Mathf.PI * 2f);

        float cellU = Mathf.Repeat(u * panelsX, 1f);
        float cellV = Mathf.Repeat(v * panelsY, 1f);
        float edgeDistance = Mathf.Min(Mathf.Min(cellU, 1f - cellU), Mathf.Min(cellV, 1f - cellV));
        float seam = 1f - Mathf.SmoothStep(0.012f, 0.055f, edgeDistance);

        float micro = Hash01(x, y, seed) - 0.5f;
        return Mathf.Clamp01(0.53f + wave * 0.075f + micro * 0.055f - seam * 0.20f);
    }

    private static float Hash01(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0x00FFFFFF) / 16777215f;
        }
    }

    private static Texture2D NewTexture(bool linear)
    {
        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, linear);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Trilinear;
        return texture;
    }

    private static void WritePngAndImport(Texture2D texture, string path, bool normalMap, bool srgb)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Trilinear;
        importer.mipmapEnabled = true;
        importer.sRGBTexture = srgb;
        if (normalMap) importer.textureType = TextureImporterType.NormalMap;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
    }

    private static void SetTexture(Material material, string property, Texture texture, float tiling)
    {
        if (!material.HasProperty(property)) return;
        material.SetTexture(property, texture);
        material.SetTextureScale(property, Vector2.one * tiling);
    }

    private static int Wrap(int value)
    {
        if (value < 0) return TextureSize - 1;
        if (value >= TextureSize) return 0;
        return value;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        if (!string.IsNullOrEmpty(parent)) AssetDatabase.CreateFolder(parent, name);
    }
}
