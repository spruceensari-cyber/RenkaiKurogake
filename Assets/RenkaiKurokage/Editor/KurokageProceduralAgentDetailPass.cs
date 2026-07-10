using UnityEditor;
using UnityEngine;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageProceduralAgentDetailPass
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/AgentMaterials/";

    public static bool Apply(RenkaiRoundPlayer player, KurokageAgentArchetype archetype)
    {
        if (player == null) return false;
        Transform visual = player.transform.Find("AGENT_VISUAL");
        if (visual == null) return false;

        Transform model = FindDeep(visual, "PROCEDURAL_AGENT_ROOT");
        Transform head = FindDeep(visual, "head");
        Transform chest = FindDeep(visual, "chest");
        Transform pelvis = FindDeep(visual, "pelvis");
        Transform leftHand = FindDeep(visual, "lefthand");
        Transform rightHand = FindDeep(visual, "righthand");
        Transform leftFoot = FindDeep(visual, "leftfoot");
        Transform rightFoot = FindDeep(visual, "rightfoot");

        if (model == null || head == null || chest == null || pelvis == null || leftHand == null || rightHand == null)
            return false;

        ClearOldDetails(visual);
        ApplyProfileScale(model, archetype);

        Material gunmetal = Load("M_Agent_Gunmetal");
        Material blue = Load("M_Agent_BlueEnergy");
        Material violet = Load("M_Agent_VioletEnergy");
        Material glass = Load("M_Agent_Glass");
        Material armor = Load("M_Agent_WhiteArmor");
        if (gunmetal == null || blue == null || violet == null || glass == null || armor == null) return false;

        Material energy = archetype == KurokageAgentArchetype.Reiha ? violet : blue;

        CreatePart("PROC_DETAIL_EYE_L", PrimitiveType.Cube, head, new Vector3(-0.072f, 0.105f, 0.218f), new Vector3(0.036f, 0.010f, 0.008f), glass);
        CreatePart("PROC_DETAIL_EYE_R", PrimitiveType.Cube, head, new Vector3(0.072f, 0.105f, 0.218f), new Vector3(0.036f, 0.010f, 0.008f), glass);
        CreatePart("PROC_DETAIL_EAR_COMMS", PrimitiveType.Cylinder, head, new Vector3(0.205f, 0.07f, 0f), new Vector3(0.045f, 0.018f, 0.045f), energy, new Vector3(0f, 0f, 90f));

        CreatePart("PROC_DETAIL_CHEST_RAIL_L", PrimitiveType.Cube, chest, new Vector3(-0.18f, 0.10f, 0.245f), new Vector3(0.016f, 0.17f, 0.012f), energy, new Vector3(0f, 0f, -8f));
        CreatePart("PROC_DETAIL_CHEST_RAIL_R", PrimitiveType.Cube, chest, new Vector3(0.18f, 0.10f, 0.245f), new Vector3(0.016f, 0.17f, 0.012f), energy, new Vector3(0f, 0f, 8f));
        CreatePart("PROC_DETAIL_SPINE_NODE", PrimitiveType.Sphere, chest, new Vector3(0f, 0.12f, -0.23f), Vector3.one * 0.045f, energy);

        CreatePart("PROC_DETAIL_HIP_MODULE_L", PrimitiveType.Cube, pelvis, new Vector3(-0.32f, -0.04f, 0.02f), new Vector3(0.085f, 0.16f, 0.11f), gunmetal, new Vector3(0f, 0f, -8f));
        CreatePart("PROC_DETAIL_HIP_MODULE_R", PrimitiveType.Cube, pelvis, new Vector3(0.32f, -0.04f, 0.02f), new Vector3(0.085f, 0.16f, 0.11f), gunmetal, new Vector3(0f, 0f, 8f));

        if (leftFoot != null)
        {
            CreatePart("PROC_DETAIL_BOOT_SOLE_L", PrimitiveType.Cube, leftFoot, new Vector3(0f, -0.17f, 0.09f), new Vector3(0.175f, 0.035f, 0.285f), gunmetal);
            CreatePart("PROC_DETAIL_BOOT_LIGHT_L", PrimitiveType.Cube, leftFoot, new Vector3(-0.10f, -0.12f, 0.12f), new Vector3(0.012f, 0.035f, 0.10f), energy);
        }
        if (rightFoot != null)
        {
            CreatePart("PROC_DETAIL_BOOT_SOLE_R", PrimitiveType.Cube, rightFoot, new Vector3(0f, -0.17f, 0.09f), new Vector3(0.175f, 0.035f, 0.285f), gunmetal);
            CreatePart("PROC_DETAIL_BOOT_LIGHT_R", PrimitiveType.Cube, rightFoot, new Vector3(0.10f, -0.12f, 0.12f), new Vector3(0.012f, 0.035f, 0.10f), energy);
        }

        EnsureSocket(rightHand, "RightHandGrip", new Vector3(0f, -0.09f, 0.08f), Vector3.zero);
        EnsureSocket(leftHand, "LeftHandSupport", new Vector3(0f, -0.07f, 0.07f), Vector3.zero);
        EnsureSocket(pelvis, "BladeGrip", new Vector3(0.32f, -0.10f, -0.08f), new Vector3(0f, 0f, 18f));
        EnsureSocket(leftHand, "AbilityProjectionSocket", new Vector3(0f, -0.05f, 0.12f), new Vector3(-90f, 0f, 0f));

        switch (archetype)
        {
            case KurokageAgentArchetype.Kairi:
                CreatePart("PROC_DETAIL_KAIRI_KINETIC_L", PrimitiveType.Cylinder, pelvis, new Vector3(-0.24f, 0.08f, -0.18f), new Vector3(0.055f, 0.025f, 0.055f), blue, new Vector3(90f, 0f, 0f));
                CreatePart("PROC_DETAIL_KAIRI_KINETIC_R", PrimitiveType.Cylinder, pelvis, new Vector3(0.24f, 0.08f, -0.18f), new Vector3(0.055f, 0.025f, 0.055f), blue, new Vector3(90f, 0f, 0f));
                break;
            case KurokageAgentArchetype.Noa:
                CreatePart("PROC_DETAIL_NOA_SENSOR", PrimitiveType.Cylinder, chest, new Vector3(0.24f, 0.24f, 0.17f), new Vector3(0.05f, 0.018f, 0.05f), blue, new Vector3(90f, 0f, 0f));
                CreatePart("PROC_DETAIL_NOA_COLLAR", PrimitiveType.Cube, chest, new Vector3(0f, 0.28f, 0f), new Vector3(0.30f, 0.10f, 0.14f), armor);
                break;
            case KurokageAgentArchetype.Reiha:
                CreatePart("PROC_DETAIL_REIHA_NODE", PrimitiveType.Sphere, chest, new Vector3(-0.21f, 0.18f, 0.24f), Vector3.one * 0.06f, violet);
                CreatePart("PROC_DETAIL_REIHA_GUARD", PrimitiveType.Cube, pelvis, new Vector3(0.34f, 0.02f, 0.12f), new Vector3(0.16f, 0.04f, 0.05f), violet, new Vector3(0f, 0f, 22f));
                break;
            case KurokageAgentArchetype.Mio:
                CreatePart("PROC_DETAIL_MIO_PROJECTOR", PrimitiveType.Cylinder, leftHand, new Vector3(0f, -0.05f, 0.11f), new Vector3(0.085f, 0.015f, 0.085f), blue, new Vector3(90f, 0f, 0f));
                CreatePart("PROC_DETAIL_MIO_DATA_NODE", PrimitiveType.Sphere, chest, new Vector3(0f, 0.10f, 0.27f), Vector3.one * 0.05f, blue);
                break;
        }

        return true;
    }

    private static void ApplyProfileScale(Transform model, KurokageAgentArchetype archetype)
    {
        switch (archetype)
        {
            case KurokageAgentArchetype.Noa:
                model.localScale = new Vector3(0.98f, 1.025f, 1.00f);
                break;
            case KurokageAgentArchetype.Reiha:
                model.localScale = new Vector3(1.035f, 1.00f, 1.035f);
                break;
            case KurokageAgentArchetype.Mio:
                model.localScale = new Vector3(0.965f, 0.99f, 0.965f);
                break;
            default:
                model.localScale = Vector3.one;
                break;
        }
    }

    private static void ClearOldDetails(Transform visual)
    {
        Transform[] all = visual.GetComponentsInChildren<Transform>(true);
        for (int i = all.Length - 1; i >= 0; i--)
        {
            if (all[i] != null && all[i] != visual && all[i].name.StartsWith("PROC_DETAIL_"))
                Object.DestroyImmediate(all[i].gameObject);
        }
    }

    private static void EnsureSocket(Transform parent, string name, Vector3 localPosition, Vector3 localEuler)
    {
        Transform socket = parent.Find(name);
        if (socket == null)
        {
            GameObject go = new GameObject(name);
            socket = go.transform;
            socket.SetParent(parent, false);
        }
        socket.localPosition = localPosition;
        socket.localRotation = Quaternion.Euler(localEuler);
    }

    private static GameObject CreatePart(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3? localEuler = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler ?? Vector3.zero);
        go.transform.localScale = localScale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return go;
    }

    private static Material Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + name + ".mat");
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}
