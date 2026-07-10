using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

/// <summary>
/// Production roster visual pass. It replaces generated proxy bodies with the project's imported FBX characters.
/// </summary>
public static class KurokageAgentVisualInstaller
{
    private const string CrimsonModelPath = "Assets/Renkai/Characters/CrimsonBlossomWarrior/CrimsonBlossomWarrior_Alert.fbx";
    private const string NocturneModelPath = "Assets/Renkai/Characters/NocturneEmpress/NocturneEmpress_Alert.fbx";
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/AgentMaterials";

    public static bool InstallSilent()
    {
        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        if (players == null || players.Length != 10)
            return false;

        GameObject crimson = AssetDatabase.LoadAssetAtPath<GameObject>(CrimsonModelPath);
        GameObject nocturne = AssetDatabase.LoadAssetAtPath<GameObject>(NocturneModelPath);
        if (crimson == null || nocturne == null)
        {
            Debug.LogError("Renkai roster visual pass could not locate the imported FBX character assets.");
            return false;
        }

        Material alphaBody = GetOrCreateMaterial("M_Renkai_Alpha_Body", new Color(0.055f, 0.085f, 0.13f), 0.25f, 0f);
        Material omegaBody = GetOrCreateMaterial("M_Renkai_Omega_Body", new Color(0.11f, 0.055f, 0.13f), 0.25f, 0f);
        Material alphaAccent = GetOrCreateMaterial("M_Renkai_Alpha_Accent", new Color(0.12f, 0.58f, 1f), 0.55f, 1.5f);
        Material omegaAccent = GetOrCreateMaterial("M_Renkai_Omega_Accent", new Color(0.78f, 0.28f, 0.82f), 0.55f, 1.2f);
        Material darkMetal = GetOrCreateMaterial("M_Renkai_Agent_DarkMetal", new Color(0.035f, 0.045f, 0.065f), 0.7f, 0f);

        System.Array.Sort(players, ComparePlayers);
        bool success = true;
        int attackerIndex = 0;
        int defenderIndex = 0;

        foreach (RenkaiRoundPlayer player in players)
        {
            KurokageAgentArchetype archetype = ResolveArchetype(player, ref attackerIndex, ref defenderIndex);
            player.agentName = BuildAgentName(player, archetype);

            GameObject source = UsesNocturneBody(archetype) ? nocturne : crimson;
            if (!BuildVisual(player, source, archetype, alphaBody, omegaBody, alphaAccent, omegaAccent, darkMetal))
                success = false;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        return success;
    }

    private static bool BuildVisual(
        RenkaiRoundPlayer player,
        GameObject source,
        KurokageAgentArchetype archetype,
        Material alphaBody,
        Material omegaBody,
        Material alphaAccent,
        Material omegaAccent,
        Material darkMetal)
    {
        Transform oldVisual = player.transform.Find("AGENT_VISUAL");
        if (oldVisual != null)
            Object.DestroyImmediate(oldVisual.gameObject);

        GameObject visualRoot = new GameObject("AGENT_VISUAL");
        visualRoot.transform.SetParent(player.transform, false);
        visualRoot.transform.localPosition = Vector3.zero;
        visualRoot.transform.localRotation = Quaternion.identity;
        visualRoot.transform.localScale = Vector3.one;

        GameObject model = PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (model == null)
        {
            Object.DestroyImmediate(visualRoot);
            return false;
        }

        model.name = ArchetypeName(archetype) + "_BODY";
        model.transform.SetParent(visualRoot.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        foreach (Animator animator in model.GetComponentsInChildren<Animator>(true))
            animator.applyRootMotion = false;

        foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(collider);

        bool attackers = player.team == RenkaiTeam.Attackers;
        ApplyMaterials(model, attackers ? alphaBody : omegaBody, darkMetal);
        BuildIdentityGear(visualRoot.transform, archetype, attackers ? alphaAccent : omegaAccent, darkMetal);

        Color teamAccent = attackers ? new Color(0.12f, 0.58f, 1f, 1f) : new Color(0.78f, 0.28f, 0.82f, 1f);
        KurokageAgentReadabilityPresenter readability = visualRoot.AddComponent<KurokageAgentReadabilityPresenter>();
        readability.Configure(teamAccent, 0.18f);

        visualRoot.AddComponent<KurokageAgentAnimationDriver>();
        if (player.GetComponent<KurokageHitReactionPresenter>() == null)
            player.gameObject.AddComponent<KurokageHitReactionPresenter>();

        KurokageHitZoneBinder binder = player.GetComponent<KurokageHitZoneBinder>();
        if (binder == null)
            binder = player.gameObject.AddComponent<KurokageHitZoneBinder>();
        binder.Bind();

        return binder.BoundZoneCount >= 3;
    }

    private static void ApplyMaterials(GameObject model, Material primary, Material darkMetal)
    {
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                renderer.sharedMaterial = primary;
                continue;
            }

            for (int i = 0; i < materials.Length; i++)
                materials[i] = i == 0 ? primary : darkMetal;
            renderer.sharedMaterials = materials;
        }
    }

    private static void BuildIdentityGear(Transform root, KurokageAgentArchetype archetype, Material accent, Material darkMetal)
    {
        GameObject shoulder = Primitive("TEAM_SIGNAL", PrimitiveType.Cube, root, new Vector3(0f, 1.47f, -0.16f), new Vector3(0.48f, 0.035f, 0.028f), accent);
        shoulder.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        switch (archetype)
        {
            case KurokageAgentArchetype.Kairi:
                Primitive("KAIRI_RIFT_SHEATH", PrimitiveType.Cube, root, new Vector3(-0.31f, 0.96f, -0.18f), new Vector3(0.06f, 0.64f, 0.08f), darkMetal).transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
                Primitive("KAIRI_RIFT_LINE", PrimitiveType.Cube, root, new Vector3(-0.34f, 0.98f, -0.24f), new Vector3(0.014f, 0.52f, 0.016f), accent).transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
                break;
            case KurokageAgentArchetype.Noa:
                Primitive("NOA_SENSOR", PrimitiveType.Cylinder, root, new Vector3(0.28f, 1.54f, 0.03f), new Vector3(0.07f, 0.02f, 0.07f), accent).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
            case KurokageAgentArchetype.Reiha:
                Primitive("REIHA_EDGE", PrimitiveType.Cube, root, new Vector3(0.32f, 0.98f, -0.16f), new Vector3(0.014f, 0.54f, 0.018f), accent).transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
                break;
            case KurokageAgentArchetype.Mio:
                Primitive("MIO_ORBIT", PrimitiveType.Sphere, root, new Vector3(0.34f, 1.31f, -0.11f), Vector3.one * 0.09f, accent);
                break;
        }
    }

    private static GameObject Primitive(string name, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(primitiveType);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);
        Renderer renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        return go;
    }

    private static KurokageAgentArchetype ResolveArchetype(RenkaiRoundPlayer player, ref int attackerIndex, ref int defenderIndex)
    {
        if (player.isHumanPlayer)
            return KurokageAgentArchetype.Kairi;

        int index = player.team == RenkaiTeam.Attackers ? attackerIndex++ : defenderIndex++;
        switch (index % 4)
        {
            case 0: return KurokageAgentArchetype.Noa;
            case 1: return KurokageAgentArchetype.Reiha;
            case 2: return KurokageAgentArchetype.Mio;
            default: return KurokageAgentArchetype.Kairi;
        }
    }

    private static bool UsesNocturneBody(KurokageAgentArchetype archetype)
    {
        return archetype == KurokageAgentArchetype.Kairi || archetype == KurokageAgentArchetype.Reiha;
    }

    private static string BuildAgentName(RenkaiRoundPlayer player, KurokageAgentArchetype archetype)
    {
        string callsign = ExtractCallSign(player.agentName);
        return ArchetypeName(archetype) + " // " + callsign;
    }

    private static string ArchetypeName(KurokageAgentArchetype archetype)
    {
        switch (archetype)
        {
            case KurokageAgentArchetype.Noa: return "NOA";
            case KurokageAgentArchetype.Reiha: return "REIHA";
            case KurokageAgentArchetype.Mio: return "MIO";
            default: return "KAIRI";
        }
    }

    private static string ExtractCallSign(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "UNIT";

        int delimiter = value.LastIndexOf("//", System.StringComparison.Ordinal);
        return delimiter >= 0 ? value.Substring(delimiter + 2).Trim() : value.Trim();
    }

    private static int ComparePlayers(RenkaiRoundPlayer a, RenkaiRoundPlayer b)
    {
        if (a.isHumanPlayer != b.isHumanPlayer)
            return a.isHumanPlayer ? -1 : 1;
        int team = a.team.CompareTo(b.team);
        return team != 0 ? team : string.CompareOrdinal(a.agentName, b.agentName);
    }

    private static Material GetOrCreateMaterial(string name, Color color, float smoothness, float emission)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage/Art", "AgentMaterials");

        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.25f);
        if (material.HasProperty("_EmissionColor"))
        {
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }
        EditorUtility.SetDirty(material);
        return material;
    }
}
