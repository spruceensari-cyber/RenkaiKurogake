using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageAgentVisualInstaller
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/OriginalAgentMaterials";

    private static readonly AgentProfile[] Roster =
    {
        new AgentProfile(KurokageAgentArchetype.Kairi, "KAIRI", "RIFT-07", new Color(0.055f, 0.09f, 0.15f), new Color(0.03f, 0.045f, 0.08f), new Color(0.14f, 0.62f, 1f), 1.00f, 1.00f),
        new AgentProfile(KurokageAgentArchetype.Noa, "NOA", "PULSE-D5", new Color(0.13f, 0.12f, 0.15f), new Color(0.055f, 0.05f, 0.07f), new Color(0.96f, 0.43f, 0.26f), 1.02f, 0.96f),
        new AgentProfile(KurokageAgentArchetype.Reiha, "REIHA", "VEIL-A3", new Color(0.11f, 0.06f, 0.18f), new Color(0.05f, 0.03f, 0.085f), new Color(0.62f, 0.32f, 0.95f), 1.01f, 1.04f),
        new AgentProfile(KurokageAgentArchetype.Mio, "MIO", "GLINT-C2", new Color(0.05f, 0.15f, 0.17f), new Color(0.025f, 0.065f, 0.075f), new Color(0.18f, 0.86f, 0.77f), 0.99f, 0.95f),
        new AgentProfile(KurokageAgentArchetype.Sora, "SORA", "BASTION-R4", new Color(0.16f, 0.13f, 0.08f), new Color(0.07f, 0.055f, 0.03f), new Color(1f, 0.68f, 0.18f), 1.04f, 1.10f),
        new AgentProfile(KurokageAgentArchetype.Aiko, "AIKO", "LANCER-M9", new Color(0.08f, 0.16f, 0.14f), new Color(0.035f, 0.07f, 0.06f), new Color(0.29f, 0.88f, 0.57f), 1.00f, 0.98f),
        new AgentProfile(KurokageAgentArchetype.Ren, "REN", "FORGE-K6", new Color(0.18f, 0.09f, 0.055f), new Color(0.075f, 0.035f, 0.025f), new Color(1f, 0.34f, 0.12f), 1.03f, 1.08f),
        new AgentProfile(KurokageAgentArchetype.Hana, "HANA", "ORBIT-S1", new Color(0.15f, 0.08f, 0.13f), new Color(0.07f, 0.035f, 0.06f), new Color(0.98f, 0.47f, 0.71f), 1.00f, 0.97f),
        new AgentProfile(KurokageAgentArchetype.Toma, "TOMA", "ANCHOR-V8", new Color(0.07f, 0.15f, 0.10f), new Color(0.025f, 0.07f, 0.045f), new Color(0.46f, 0.90f, 0.28f), 1.05f, 1.12f),
        new AgentProfile(KurokageAgentArchetype.Yori, "YORI", "WRAITH-N2", new Color(0.075f, 0.08f, 0.17f), new Color(0.03f, 0.035f, 0.08f), new Color(0.40f, 0.55f, 1f), 1.01f, 1.00f)
    };

    private static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

    public static bool InstallSilent()
    {
        RenkaiRoundPlayer[] players = UnityEngine.Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        if (players == null || players.Length == 0) return false;

        Array.Sort(players, ComparePlayers);
        EnsureMaterialFolder();
        MaterialCache.Clear();

        bool visualsOk = true;
        for (int i = 0; i < players.Length; i++)
        {
            RenkaiRoundPlayer player = players[i];
            KurokageAgentIdentity identity = player.GetComponent<KurokageAgentIdentity>();
            KurokageAgentArchetype fallback = player.isHumanPlayer
                ? KurokageAgentArchetype.Kairi
                : Roster[Mathf.Clamp(i, 1, Roster.Length - 1)].Archetype;

            if (identity == null)
            {
                identity = player.gameObject.AddComponent<KurokageAgentIdentity>();
                identity.Configure(fallback, player.isHumanPlayer);
            }

            AgentProfile profile = FindProfile(identity.Archetype);
            KurokageAgentDefinition definition = KurokageAgentCatalog.Get(profile.Archetype);
            player.agentName = definition.FullIdentity;

            if (!BuildVisual(player, profile)) visualsOk = false;

            if (!player.isHumanPlayer && player.GetComponent<KurokageBotLocalAvoidance>() == null)
                player.gameObject.AddComponent<KurokageBotLocalAvoidance>();
        }

        bool surfaceOk = KurokageSurfaceDetailPass.ApplySilent();
        bool qualityOk = KurokageHighFidelityQualityPass.ApplySilent();
        bool collisionOk = KurokageCollisionIntegrityPass.ApplySilent();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = players[0].gameObject;
        return visualsOk && surfaceOk && qualityOk && collisionOk;
    }

    private static AgentProfile FindProfile(KurokageAgentArchetype archetype)
    {
        foreach (AgentProfile profile in Roster)
            if (profile.Archetype == archetype) return profile;
        return Roster[0];
    }

    private static bool BuildVisual(RenkaiRoundPlayer player, AgentProfile profile)
    {
        Transform oldVisual = player.transform.Find("AGENT_VISUAL");
        if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);

        GameObject visualRoot = new GameObject("AGENT_VISUAL");
        visualRoot.transform.SetParent(player.transform, false);

        GameObject modelRootObject = new GameObject("PROCEDURAL_AGENT_ROOT");
        Transform modelRoot = modelRootObject.transform;
        modelRoot.SetParent(visualRoot.transform, false);
        modelRoot.localScale = new Vector3(profile.Width, profile.Height, profile.Width);

        AgentMaterials materials = CreateMaterials(profile, player.team);
        if (!materials.IsValid)
        {
            UnityEngine.Object.DestroyImmediate(visualRoot);
            return false;
        }

        Transform pelvis = Pivot("pelvis", modelRoot, new Vector3(0f, 0.86f, 0f));
        Part("PELVIS_SHELL", PrimitiveType.Capsule, pelvis, Vector3.zero, new Vector3(0.31f, 0.18f, 0.24f), materials.Body);
        Part("PELVIS_ARMOR", PrimitiveType.Cube, pelvis, new Vector3(0f, 0.02f, 0.20f), new Vector3(0.30f, 0.12f, 0.04f), materials.Armor);

        Transform spine = Pivot("spine1", pelvis, new Vector3(0f, 0.24f, 0f));
        Part("ABDOMEN", PrimitiveType.Capsule, spine, new Vector3(0f, 0.15f, 0f), new Vector3(0.25f, 0.22f, 0.20f), materials.Fabric);

        Transform chest = Pivot("chest", spine, new Vector3(0f, 0.38f, 0f));
        Part("CHEST_BODY", PrimitiveType.Capsule, chest, Vector3.zero, new Vector3(0.36f, 0.28f, 0.24f), materials.Body);
        Part("CHEST_PLATE", PrimitiveType.Cube, chest, new Vector3(0f, 0.03f, 0.225f), new Vector3(0.31f, 0.19f, 0.035f), materials.Armor);
        Part("CHEST_CORE", PrimitiveType.Cube, chest, new Vector3(0f, 0.05f, 0.265f), new Vector3(0.055f, 0.075f, 0.012f), materials.Accent);

        Transform neck = Pivot("neck", chest, new Vector3(0f, 0.32f, 0f));
        Part("NECK", PrimitiveType.Cylinder, neck, Vector3.zero, new Vector3(0.105f, 0.08f, 0.105f), materials.Skin);

        Transform head = Pivot("head", neck, new Vector3(0f, 0.25f, 0f));
        Part("HEAD_MESH", PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.22f, 0.245f, 0.215f), materials.Skin);
        Part("HAIR_CAP", PrimitiveType.Sphere, head, new Vector3(0f, 0.085f, -0.012f), new Vector3(0.225f, 0.17f, 0.22f), materials.Hair);
        Part("VISOR_EYE_L", PrimitiveType.Cube, head, new Vector3(-0.075f, 0.02f, 0.205f), new Vector3(0.045f, 0.012f, 0.008f), materials.Accent);
        Part("VISOR_EYE_R", PrimitiveType.Cube, head, new Vector3(0.075f, 0.02f, 0.205f), new Vector3(0.045f, 0.012f, 0.008f), materials.Accent);

        Transform leftUpperArm = Pivot("leftupperarm", chest, new Vector3(-0.39f, 0.19f, 0f));
        Transform leftLowerArm = BuildArm(leftUpperArm, "LEFT", materials, -1f);
        Transform rightUpperArm = Pivot("rightupperarm", chest, new Vector3(0.39f, 0.19f, 0f));
        Transform rightLowerArm = BuildArm(rightUpperArm, "RIGHT", materials, 1f);

        Transform leftUpperLeg = Pivot("leftupleg", pelvis, new Vector3(-0.17f, -0.10f, 0f));
        BuildLeg(leftUpperLeg, "LEFT", materials);
        Transform rightUpperLeg = Pivot("rightupleg", pelvis, new Vector3(0.17f, -0.10f, 0f));
        BuildLeg(rightUpperLeg, "RIGHT", materials);

        BuildSignature(profile.Archetype, modelRoot, pelvis, chest, head, leftLowerArm, rightLowerArm, materials);

        KurokageProceduralAgentRig rig = visualRoot.AddComponent<KurokageProceduralAgentRig>();
        rig.Configure(profile.Archetype);
        visualRoot.AddComponent<KurokageAgentAnimationDriver>();

        Color teamAccent = player.team == RenkaiTeam.Attackers
            ? new Color(0.12f, 0.58f, 1f, 1f)
            : new Color(0.72f, 0.28f, 0.85f, 1f);
        KurokageAgentReadabilityPresenter readability = visualRoot.AddComponent<KurokageAgentReadabilityPresenter>();
        readability.Configure(teamAccent, 0.13f);

        if (player.GetComponent<KurokageHitReactionPresenter>() == null)
            player.gameObject.AddComponent<KurokageHitReactionPresenter>();

        KurokageHitZoneBinder binder = player.GetComponent<KurokageHitZoneBinder>();
        if (binder == null) binder = player.gameObject.AddComponent<KurokageHitZoneBinder>();
        binder.Bind();

        return binder.BoundZoneCount >= 3;
    }

    private static Transform BuildArm(Transform upper, string side, AgentMaterials materials, float sideSign)
    {
        Part(side + "_UPPER_ARM", PrimitiveType.Capsule, upper, new Vector3(0f, -0.20f, 0f), new Vector3(0.115f, 0.22f, 0.115f), materials.Fabric);
        Part(side + "_SHOULDER", PrimitiveType.Cube, upper, new Vector3(sideSign * 0.015f, 0f, 0f), new Vector3(0.16f, 0.10f, 0.20f), materials.Armor, new Vector3(0f, 0f, sideSign * 10f));
        Transform lower = Pivot(side == "LEFT" ? "leftlowerarm" : "rightlowerarm", upper, new Vector3(0f, -0.39f, 0f));
        Part(side + "_FOREARM", PrimitiveType.Capsule, lower, new Vector3(0f, -0.18f, 0.025f), new Vector3(0.105f, 0.19f, 0.11f), materials.Body);
        Part(side + "_FOREARM_PLATE", PrimitiveType.Cube, lower, new Vector3(0f, -0.16f, 0.12f), new Vector3(0.11f, 0.16f, 0.025f), materials.Armor);
        Transform hand = Pivot(side == "LEFT" ? "lefthand" : "righthand", lower, new Vector3(0f, -0.39f, 0.03f));
        Part(side + "_HAND", PrimitiveType.Sphere, hand, Vector3.zero, new Vector3(0.105f, 0.12f, 0.105f), materials.Skin);
        return lower;
    }

    private static void BuildLeg(Transform upper, string side, AgentMaterials materials)
    {
        Part(side + "_THIGH", PrimitiveType.Capsule, upper, new Vector3(0f, -0.24f, 0f), new Vector3(0.145f, 0.25f, 0.15f), materials.Fabric);
        Part(side + "_THIGH_PLATE", PrimitiveType.Cube, upper, new Vector3(0f, -0.20f, 0.14f), new Vector3(0.13f, 0.18f, 0.025f), materials.Armor);
        Transform lower = Pivot(side == "LEFT" ? "leftleg" : "rightleg", upper, new Vector3(0f, -0.48f, 0f));
        Part(side + "_SHIN", PrimitiveType.Capsule, lower, new Vector3(0f, -0.23f, 0.02f), new Vector3(0.13f, 0.24f, 0.135f), materials.Body);
        Part(side + "_SHIN_PLATE", PrimitiveType.Cube, lower, new Vector3(0f, -0.20f, 0.14f), new Vector3(0.13f, 0.18f, 0.025f), materials.Armor);
        Transform foot = Pivot(side == "LEFT" ? "leftfoot" : "rightfoot", lower, new Vector3(0f, -0.47f, 0.07f));
        Part(side + "_BOOT", PrimitiveType.Cube, foot, Vector3.zero, new Vector3(0.17f, 0.10f, 0.27f), materials.DarkMetal);
    }

    private static void BuildSignature(KurokageAgentArchetype archetype, Transform root, Transform pelvis, Transform chest, Transform head, Transform leftLowerArm, Transform rightLowerArm, AgentMaterials materials)
    {
        switch (archetype)
        {
            case KurokageAgentArchetype.Kairi:
                Transform tail = Pivot("PONYTAIL_ROOT", head, new Vector3(0f, 0.13f, -0.20f));
                Part("PONYTAIL_SEGMENT_A", PrimitiveType.Capsule, tail, new Vector3(0f, -0.20f, -0.05f), new Vector3(0.07f, 0.22f, 0.07f), materials.Hair, new Vector3(15f, 0f, 0f));
                Part("KAIRI_BLADE_SHEATH", PrimitiveType.Cube, root, new Vector3(-0.31f, 0.94f, -0.20f), new Vector3(0.055f, 0.50f, 0.075f), materials.DarkMetal, new Vector3(0f, 0f, -24f));
                CoatPanels(root, materials.Fabric);
                break;
            case KurokageAgentArchetype.Noa:
                Part("NOA_SENSOR", PrimitiveType.Cylinder, head, new Vector3(0.20f, 0.05f, 0f), new Vector3(0.055f, 0.018f, 0.055f), materials.Accent, new Vector3(0f, 0f, 90f));
                Part("NOA_BACK_RIFLE", PrimitiveType.Cube, root, new Vector3(0f, 1.18f, -0.25f), new Vector3(0.10f, 0.47f, 0.08f), materials.DarkMetal, new Vector3(0f, 0f, 18f));
                CoatPanels(root, materials.Fabric);
                break;
            case KurokageAgentArchetype.Reiha:
                Part("REIHA_CHEST_NODE", PrimitiveType.Sphere, chest, new Vector3(-0.20f, 0.12f, 0.23f), Vector3.one * 0.065f, materials.Accent);
                Part("REIHA_HIP_BLADE", PrimitiveType.Cube, pelvis, new Vector3(0.34f, -0.03f, -0.05f), new Vector3(0.045f, 0.43f, 0.06f), materials.DarkMetal, new Vector3(0f, 0f, 22f));
                break;
            case KurokageAgentArchetype.Mio:
                Transform dronePivot = Pivot("DRONE_PIVOT", root, new Vector3(0f, 1.34f, 0f));
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * Mathf.PI * 0.5f;
                    Part("MIO_DRONE_" + i, PrimitiveType.Sphere, dronePivot, new Vector3(Mathf.Cos(angle) * 0.48f, Mathf.Sin(angle * 2f) * 0.05f, Mathf.Sin(angle) * 0.48f), Vector3.one * 0.075f, materials.Accent);
                }
                CoatPanels(root, materials.Fabric);
                break;
            case KurokageAgentArchetype.Sora:
                Part("SORA_BACK_SHIELD", PrimitiveType.Cube, root, new Vector3(0f, 1.16f, -0.28f), new Vector3(0.38f, 0.36f, 0.10f), materials.DarkMetal);
                break;
            case KurokageAgentArchetype.Aiko:
                Part("AIKO_SCARF", PrimitiveType.Cylinder, chest, new Vector3(0f, 0.29f, 0f), new Vector3(0.24f, 0.055f, 0.24f), materials.Accent);
                CoatPanels(root, materials.Fabric);
                break;
            case KurokageAgentArchetype.Ren:
                Part("REN_BREAKER_GAUNTLET", PrimitiveType.Cube, rightLowerArm, new Vector3(0f, -0.17f, 0.09f), new Vector3(0.17f, 0.20f, 0.17f), materials.DarkMetal);
                break;
            case KurokageAgentArchetype.Hana:
                Part("HANA_HALO", PrimitiveType.Cylinder, head, new Vector3(0f, 0.36f, 0f), new Vector3(0.30f, 0.018f, 0.30f), materials.Accent);
                CoatPanels(root, materials.Fabric);
                break;
            case KurokageAgentArchetype.Toma:
                Part("TOMA_EXO_PACK", PrimitiveType.Cube, root, new Vector3(0f, 1.17f, -0.30f), new Vector3(0.42f, 0.34f, 0.15f), materials.DarkMetal);
                break;
            case KurokageAgentArchetype.Yori:
                Part("YORI_MASK", PrimitiveType.Cube, head, new Vector3(0f, -0.01f, 0.205f), new Vector3(0.16f, 0.09f, 0.018f), materials.DarkMetal);
                CoatPanels(root, materials.Fabric);
                break;
        }
    }

    private static void CoatPanels(Transform root, Material material)
    {
        Transform left = Pivot("COAT_PANEL_L", root, new Vector3(-0.14f, 0.82f, -0.16f));
        Part("COAT_MESH_L", PrimitiveType.Cube, left, new Vector3(0f, -0.20f, 0f), new Vector3(0.12f, 0.30f, 0.035f), material, new Vector3(0f, 0f, 8f));
        Transform right = Pivot("COAT_PANEL_R", root, new Vector3(0.14f, 0.82f, -0.16f));
        Part("COAT_MESH_R", PrimitiveType.Cube, right, new Vector3(0f, -0.20f, 0f), new Vector3(0.12f, 0.30f, 0.035f), material, new Vector3(0f, 0f, -8f));
    }

    private static AgentMaterials CreateMaterials(AgentProfile profile, RenkaiTeam team)
    {
        Material body = MaterialAsset(profile.DisplayName + "_Body", profile.Body, 0.48f, 0.18f, Color.black);
        Material fabric = MaterialAsset(profile.DisplayName + "_Fabric", profile.Fabric, 0.26f, 0.04f, Color.black);
        Material accent = MaterialAsset(profile.DisplayName + "_Accent", profile.Accent, 0.62f, 0.10f, profile.Accent * 1.6f);
        Material armor = MaterialAsset("Shared_WhiteComposite", new Color(0.64f, 0.69f, 0.76f), 0.56f, 0.12f, Color.black);
        Material darkMetal = MaterialAsset("Shared_DarkMetal", new Color(0.028f, 0.038f, 0.058f), 0.70f, 0.42f, Color.black);
        Material skin = MaterialAsset("Shared_Skin", new Color(0.56f, 0.41f, 0.35f), 0.32f, 0.00f, Color.black);
        Material hair = MaterialAsset(profile.DisplayName + "_Hair", Color.Lerp(profile.Fabric, Color.black, 0.28f), 0.46f, 0.06f, Color.black);
        return new AgentMaterials(body, fabric, accent, armor, darkMetal, skin, hair);
    }

    private static Material MaterialAsset(string name, Color color, float smoothness, float metallic, Color emission)
    {
        if (MaterialCache.TryGetValue(name, out Material cached) && cached != null) return cached;

        string path = MaterialFolder + "/M_" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = ResolveShader();
        if (shader == null) return null;

        if (material == null)
        {
            material = new Material(shader) { name = "M_" + name };
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0.001f) material.EnableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(material);
        MaterialCache[name] = material;
        return material;
    }

    private static Shader ResolveShader()
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Diffuse");
        return shader;
    }

    private static Transform Pivot(string name, Transform parent, Vector3 localPosition)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        return go.transform;
    }

    private static GameObject Part(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3? localEuler = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler ?? Vector3.zero);
        go.transform.localScale = localScale;

        Collider collider = go.GetComponent<Collider>();
        if (collider != null) UnityEngine.Object.DestroyImmediate(collider);

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
        return go;
    }

    private static void EnsureMaterialFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/RenkaiKurokage/Art"))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage", "Art");
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage/Art", "OriginalAgentMaterials");
    }

    private static int ComparePlayers(RenkaiRoundPlayer a, RenkaiRoundPlayer b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        if (a.isHumanPlayer != b.isHumanPlayer) return a.isHumanPlayer ? -1 : 1;
        int team = a.team.CompareTo(b.team);
        return team != 0 ? team : string.CompareOrdinal(a.agentName, b.agentName);
    }

    private sealed class AgentProfile
    {
        public readonly KurokageAgentArchetype Archetype;
        public readonly string DisplayName;
        public readonly string Callsign;
        public readonly Color Body;
        public readonly Color Fabric;
        public readonly Color Accent;
        public readonly float Height;
        public readonly float Width;

        public AgentProfile(KurokageAgentArchetype archetype, string displayName, string callsign, Color body, Color fabric, Color accent, float height, float width)
        {
            Archetype = archetype;
            DisplayName = displayName;
            Callsign = callsign;
            Body = body;
            Fabric = fabric;
            Accent = accent;
            Height = height;
            Width = width;
        }
    }

    private struct AgentMaterials
    {
        public readonly Material Body;
        public readonly Material Fabric;
        public readonly Material Accent;
        public readonly Material Armor;
        public readonly Material DarkMetal;
        public readonly Material Skin;
        public readonly Material Hair;
        public bool IsValid => Body != null && Fabric != null && Accent != null && Armor != null && DarkMetal != null && Skin != null && Hair != null;

        public AgentMaterials(Material body, Material fabric, Material accent, Material armor, Material darkMetal, Material skin, Material hair)
        {
            Body = body;
            Fabric = fabric;
            Accent = accent;
            Armor = armor;
            DarkMetal = darkMetal;
            Skin = skin;
            Hair = hair;
        }
    }
}
