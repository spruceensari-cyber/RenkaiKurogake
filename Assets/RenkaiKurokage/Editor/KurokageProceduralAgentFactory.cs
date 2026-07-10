using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageProceduralAgentFactory
{
    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/AgentMaterials";
    private const string MeshFolder = "Assets/RenkaiKurokage/Art/GeneratedAgentMeshes";

    private sealed class Palette
    {
        public Material Skin;
        public Material Hair;
        public Material Suit;
        public Material Armor;
        public Material Gunmetal;
        public Material Blue;
        public Material Violet;
        public Material Glass;
    }

    private static Palette palette;
    private static Mesh taperedTorsoMesh;
    private static Mesh coatPanelMesh;

    public static bool Build(RenkaiRoundPlayer player, KurokageAgentArchetype archetype)
    {
        if (player == null) return false;
        EnsureAssets();
        if (palette == null || taperedTorsoMesh == null || coatPanelMesh == null) return false;

        Transform old = player.transform.Find("AGENT_VISUAL");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        foreach (Renderer renderer in player.GetComponents<Renderer>())
            renderer.enabled = false;

        GameObject visualRoot = new GameObject("AGENT_VISUAL");
        visualRoot.transform.SetParent(player.transform, false);

        GameObject modelRoot = new GameObject("PROCEDURAL_AGENT_ROOT");
        modelRoot.transform.SetParent(visualRoot.transform, false);

        Transform pelvis = Bone(modelRoot.transform, "pelvis", new Vector3(0f, 1.02f, 0f));
        Transform spine = Bone(pelvis, "spine1", new Vector3(0f, 0.20f, 0f));
        Transform chest = Bone(spine, "chest", new Vector3(0f, 0.23f, 0f));
        Transform neck = Bone(chest, "neck", new Vector3(0f, 0.28f, 0f));
        Transform head = Bone(neck, "head", new Vector3(0f, 0.15f, 0f));

        Transform leftUpperArm = Bone(chest, "leftupperarm", new Vector3(-0.31f, 0.18f, 0f), new Vector3(0f, 0f, -5f));
        Transform rightUpperArm = Bone(chest, "rightupperarm", new Vector3(0.31f, 0.18f, 0f), new Vector3(0f, 0f, 5f));
        Transform leftLowerArm = Bone(leftUpperArm, "leftlowerarm", new Vector3(0f, -0.34f, 0f));
        Transform rightLowerArm = Bone(rightUpperArm, "rightlowerarm", new Vector3(0f, -0.34f, 0f));
        Transform leftHand = Bone(leftLowerArm, "lefthand", new Vector3(0f, -0.29f, 0f));
        Transform rightHand = Bone(rightLowerArm, "righthand", new Vector3(0f, -0.29f, 0f));

        Transform leftUpperLeg = Bone(pelvis, "leftupleg", new Vector3(-0.145f, -0.08f, 0f));
        Transform rightUpperLeg = Bone(pelvis, "rightupleg", new Vector3(0.145f, -0.08f, 0f));
        Transform leftLowerLeg = Bone(leftUpperLeg, "leftleg", new Vector3(0f, -0.47f, 0f));
        Transform rightLowerLeg = Bone(rightUpperLeg, "rightleg", new Vector3(0f, -0.47f, 0f));
        Transform leftFoot = Bone(leftLowerLeg, "leftfoot", new Vector3(0f, -0.43f, 0.055f));
        Transform rightFoot = Bone(rightLowerLeg, "rightfoot", new Vector3(0f, -0.43f, 0.055f));

        BuildBaseBody(pelvis, spine, chest, neck, head, leftUpperArm, rightUpperArm, leftLowerArm, rightLowerArm,
            leftHand, rightHand, leftUpperLeg, rightUpperLeg, leftLowerLeg, rightLowerLeg, leftFoot, rightFoot);
        BuildHair(head, archetype);
        BuildArmor(chest, pelvis, leftUpperArm, rightUpperArm, leftLowerArm, rightLowerArm, leftLowerLeg, rightLowerLeg, archetype);
        BuildProfileGear(modelRoot.transform, chest, pelvis, head, leftHand, rightHand, archetype);

        Color teamAccent = player.team == RenkaiTeam.Attackers
            ? new Color(0.14f, 0.50f, 1f, 1f)
            : new Color(0.62f, 0.32f, 0.92f, 1f);

        KurokageAgentReadabilityPresenter readability = visualRoot.AddComponent<KurokageAgentReadabilityPresenter>();
        readability.Configure(teamAccent, 0.14f);

        KurokageProceduralAgentRig rig = visualRoot.AddComponent<KurokageProceduralAgentRig>();
        rig.Configure(archetype);

        KurokageHitReactionPresenter hitReaction = player.GetComponent<KurokageHitReactionPresenter>();
        if (hitReaction == null) player.gameObject.AddComponent<KurokageHitReactionPresenter>();

        KurokageHitZoneBinder binder = player.GetComponent<KurokageHitZoneBinder>();
        if (binder == null) binder = player.gameObject.AddComponent<KurokageHitZoneBinder>();
        binder.Bind();

        return binder.BoundZoneCount >= 3;
    }

    private static void BuildBaseBody(
        Transform pelvis, Transform spine, Transform chest, Transform neck, Transform head,
        Transform leftUpperArm, Transform rightUpperArm, Transform leftLowerArm, Transform rightLowerArm,
        Transform leftHand, Transform rightHand, Transform leftUpperLeg, Transform rightUpperLeg,
        Transform leftLowerLeg, Transform rightLowerLeg, Transform leftFoot, Transform rightFoot)
    {
        MeshPart("BODY_PELVIS", pelvis, taperedTorsoMesh, new Vector3(0f, 0.02f, 0f), new Vector3(0.62f, 0.38f, 0.42f), palette.Suit);
        MeshPart("BODY_ABDOMEN", spine, taperedTorsoMesh, new Vector3(0f, 0.09f, 0f), new Vector3(0.56f, 0.42f, 0.36f), palette.Suit);
        MeshPart("BODY_CHEST", chest, taperedTorsoMesh, new Vector3(0f, 0.08f, 0f), new Vector3(0.76f, 0.52f, 0.42f), palette.Suit);
        Primitive("NECK_BODY", PrimitiveType.Cylinder, neck, new Vector3(0f, 0.02f, 0f), new Vector3(0.13f, 0.08f, 0.13f), palette.Skin);
        Primitive("HEAD_BODY", PrimitiveType.Sphere, head, new Vector3(0f, 0.08f, 0.012f), new Vector3(0.22f, 0.255f, 0.215f), palette.Skin);
        Primitive("FACE_VISOR_LINE", PrimitiveType.Cube, head, new Vector3(0f, 0.09f, 0.205f), new Vector3(0.13f, 0.012f, 0.008f), palette.Glass);

        Limb("L_UPPER_ARM_BODY", leftUpperArm, 0.34f, 0.12f, palette.Suit);
        Limb("R_UPPER_ARM_BODY", rightUpperArm, 0.34f, 0.12f, palette.Suit);
        Limb("L_LOWER_ARM_BODY", leftLowerArm, 0.29f, 0.105f, palette.Suit);
        Limb("R_LOWER_ARM_BODY", rightLowerArm, 0.29f, 0.105f, palette.Suit);
        Primitive("L_HAND_BODY", PrimitiveType.Capsule, leftHand, new Vector3(0f, -0.085f, 0f), new Vector3(0.095f, 0.09f, 0.08f), palette.Gunmetal);
        Primitive("R_HAND_BODY", PrimitiveType.Capsule, rightHand, new Vector3(0f, -0.085f, 0f), new Vector3(0.095f, 0.09f, 0.08f), palette.Gunmetal);

        Limb("L_THIGH_BODY", leftUpperLeg, 0.47f, 0.16f, palette.Suit);
        Limb("R_THIGH_BODY", rightUpperLeg, 0.47f, 0.16f, palette.Suit);
        Limb("L_CALF_BODY", leftLowerLeg, 0.43f, 0.135f, palette.Suit);
        Limb("R_CALF_BODY", rightLowerLeg, 0.43f, 0.135f, palette.Suit);
        Primitive("L_BOOT", PrimitiveType.Cube, leftFoot, new Vector3(0f, -0.09f, 0.09f), new Vector3(0.16f, 0.12f, 0.27f), palette.Gunmetal);
        Primitive("R_BOOT", PrimitiveType.Cube, rightFoot, new Vector3(0f, -0.09f, 0.09f), new Vector3(0.16f, 0.12f, 0.27f), palette.Gunmetal);
    }

    private static void BuildHair(Transform head, KurokageAgentArchetype archetype)
    {
        Primitive("HAIR_CAP", PrimitiveType.Sphere, head, new Vector3(0f, 0.135f, -0.035f), new Vector3(0.232f, 0.265f, 0.225f), palette.Hair);
        Primitive("BANG_L", PrimitiveType.Capsule, head, new Vector3(-0.085f, 0.04f, 0.19f), new Vector3(0.035f, 0.105f, 0.025f), palette.Hair, new Vector3(12f, 0f, 18f));
        Primitive("BANG_R", PrimitiveType.Capsule, head, new Vector3(0.075f, 0.03f, 0.19f), new Vector3(0.032f, 0.11f, 0.024f), palette.Hair, new Vector3(10f, 0f, -15f));

        if (archetype == KurokageAgentArchetype.Kairi || archetype == KurokageAgentArchetype.Reiha)
        {
            Transform tailRoot = Bone(head, "PONYTAIL_ROOT", new Vector3(0f, 0.21f, -0.18f), new Vector3(25f, 0f, 0f));
            Transform segment = tailRoot;
            for (int i = 0; i < 5; i++)
            {
                Primitive("PONYTAIL_SEG_" + i, PrimitiveType.Capsule, segment, new Vector3(0f, -0.10f, -0.06f), new Vector3(0.07f - i * 0.006f, 0.15f, 0.06f - i * 0.004f), palette.Hair, new Vector3(18f, 0f, 0f));
                if (i < 4) segment = Bone(segment, "PONYTAIL_BONE_" + i, new Vector3(0f, -0.20f, -0.10f), new Vector3(7f, 0f, i % 2 == 0 ? 2f : -2f));
            }
            Primitive("PONYTAIL_BIND", PrimitiveType.Cylinder, tailRoot, new Vector3(0f, 0f, 0f), new Vector3(0.075f, 0.028f, 0.075f), palette.Violet, new Vector3(90f, 0f, 0f));
        }
        else
        {
            for (int i = -1; i <= 1; i += 2)
                Primitive("SIDE_HAIR_" + i, PrimitiveType.Capsule, head, new Vector3(i * 0.18f, -0.04f, -0.02f), new Vector3(0.055f, 0.22f, 0.045f), palette.Hair, new Vector3(6f, 0f, i * 8f));
        }
    }

    private static void BuildArmor(
        Transform chest, Transform pelvis,
        Transform leftUpperArm, Transform rightUpperArm,
        Transform leftLowerArm, Transform rightLowerArm,
        Transform leftLowerLeg, Transform rightLowerLeg,
        KurokageAgentArchetype archetype)
    {
        MeshPart("CHEST_ARMOR", chest, taperedTorsoMesh, new Vector3(0f, 0.11f, 0.045f), new Vector3(0.78f, 0.30f, 0.22f), palette.Armor);
        Primitive("CHEST_CORE", PrimitiveType.Cube, chest, new Vector3(0f, 0.10f, 0.24f), new Vector3(0.12f, 0.055f, 0.025f), archetype == KurokageAgentArchetype.Reiha ? palette.Violet : palette.Blue);
        Primitive("BELT_CORE", PrimitiveType.Cylinder, pelvis, new Vector3(0f, 0.08f, 0f), new Vector3(0.33f, 0.035f, 0.33f), palette.Gunmetal);
        Primitive("BELT_NODE", PrimitiveType.Cube, pelvis, new Vector3(0f, 0.08f, 0.22f), new Vector3(0.08f, 0.06f, 0.04f), palette.Blue);

        ShoulderPlate("SHOULDER_L", leftUpperArm, -1f, archetype);
        ShoulderPlate("SHOULDER_R", rightUpperArm, 1f, archetype);
        Primitive("FOREARM_L", PrimitiveType.Cube, leftLowerArm, new Vector3(0f, -0.14f, 0.02f), new Vector3(0.15f, 0.22f, 0.15f), palette.Armor, new Vector3(0f, 0f, 2f));
        Primitive("FOREARM_R", PrimitiveType.Cube, rightLowerArm, new Vector3(0f, -0.14f, 0.02f), new Vector3(0.15f, 0.22f, 0.15f), palette.Armor, new Vector3(0f, 0f, -2f));
        Primitive("SHIN_L", PrimitiveType.Cube, leftLowerLeg, new Vector3(0f, -0.20f, 0.055f), new Vector3(0.18f, 0.28f, 0.13f), palette.Armor);
        Primitive("SHIN_R", PrimitiveType.Cube, rightLowerLeg, new Vector3(0f, -0.20f, 0.055f), new Vector3(0.18f, 0.28f, 0.13f), palette.Armor);

        Primitive("LEG_ACCENT_L", PrimitiveType.Cube, leftLowerLeg, new Vector3(-0.095f, -0.18f, 0.13f), new Vector3(0.018f, 0.20f, 0.018f), palette.Blue);
        Primitive("LEG_ACCENT_R", PrimitiveType.Cube, rightLowerLeg, new Vector3(0.095f, -0.18f, 0.13f), new Vector3(0.018f, 0.20f, 0.018f), archetype == KurokageAgentArchetype.Reiha ? palette.Violet : palette.Blue);
    }

    private static void BuildProfileGear(
        Transform root, Transform chest, Transform pelvis, Transform head,
        Transform leftHand, Transform rightHand, KurokageAgentArchetype archetype)
    {
        switch (archetype)
        {
            case KurokageAgentArchetype.Kairi:
                BuildKairiGear(root, chest, pelvis);
                break;
            case KurokageAgentArchetype.Noa:
                BuildNoaGear(root, chest, pelvis);
                break;
            case KurokageAgentArchetype.Reiha:
                BuildReihaGear(root, chest, pelvis, rightHand);
                break;
            case KurokageAgentArchetype.Mio:
                BuildMioGear(root, chest, pelvis, leftHand);
                break;
        }
    }

    private static void BuildKairiGear(Transform root, Transform chest, Transform pelvis)
    {
        Primitive("KAIRI_COLLAR_L", PrimitiveType.Cube, chest, new Vector3(-0.16f, 0.27f, 0.03f), new Vector3(0.12f, 0.18f, 0.12f), palette.Armor, new Vector3(0f, -10f, -12f));
        Primitive("KAIRI_COLLAR_R", PrimitiveType.Cube, chest, new Vector3(0.16f, 0.27f, 0.03f), new Vector3(0.12f, 0.18f, 0.12f), palette.Armor, new Vector3(0f, 10f, 12f));
        Primitive("KAIRI_BACK_CORE", PrimitiveType.Cylinder, chest, new Vector3(0f, 0.10f, -0.24f), new Vector3(0.13f, 0.045f, 0.13f), palette.Blue, new Vector3(90f, 0f, 0f));
        Primitive("KAIRI_BLADE_SHEATH", PrimitiveType.Cube, root, new Vector3(-0.30f, 1.02f, -0.22f), new Vector3(0.055f, 0.68f, 0.07f), palette.Gunmetal, new Vector3(0f, 0f, -28f));
        Primitive("KAIRI_BLADE_LINE", PrimitiveType.Cube, root, new Vector3(-0.33f, 1.02f, -0.29f), new Vector3(0.012f, 0.58f, 0.012f), palette.Violet, new Vector3(0f, 0f, -28f));
        BuildCoatPanels(pelvis, false);
    }

    private static void BuildNoaGear(Transform root, Transform chest, Transform pelvis)
    {
        Primitive("NOA_COAT_SHOULDER", PrimitiveType.Cube, chest, new Vector3(0f, 0.18f, -0.09f), new Vector3(0.62f, 0.13f, 0.24f), palette.Armor);
        Primitive("NOA_SCOPE_NODE", PrimitiveType.Cylinder, chest, new Vector3(0.28f, 0.24f, 0.08f), new Vector3(0.055f, 0.025f, 0.055f), palette.Blue, new Vector3(90f, 0f, 0f));
        Primitive("NOA_BACK_RIFLE_BODY", PrimitiveType.Cube, root, new Vector3(0.26f, 1.18f, -0.24f), new Vector3(0.09f, 0.55f, 0.10f), palette.Gunmetal, new Vector3(0f, 0f, 18f));
        Primitive("NOA_BACK_RIFLE_RAIL", PrimitiveType.Cube, root, new Vector3(0.30f, 1.25f, -0.30f), new Vector3(0.018f, 0.40f, 0.018f), palette.Blue, new Vector3(0f, 0f, 18f));
        BuildCoatPanels(pelvis, true);
    }

    private static void BuildReihaGear(Transform root, Transform chest, Transform pelvis, Transform rightHand)
    {
        Primitive("REIHA_ASYM_ARMOR", PrimitiveType.Cube, chest, new Vector3(-0.24f, 0.15f, 0.10f), new Vector3(0.25f, 0.22f, 0.18f), palette.Gunmetal, new Vector3(0f, 0f, -12f));
        Primitive("REIHA_RESONANCE_NODE", PrimitiveType.Sphere, chest, new Vector3(-0.20f, 0.16f, 0.23f), Vector3.one * 0.055f, palette.Violet);
        Primitive("REIHA_HIP_BLADE", PrimitiveType.Cube, pelvis, new Vector3(0.34f, -0.10f, -0.02f), new Vector3(0.055f, 0.52f, 0.075f), palette.Gunmetal, new Vector3(0f, 0f, 22f));
        Primitive("REIHA_EDGE_LINE", PrimitiveType.Cube, pelvis, new Vector3(0.38f, -0.10f, -0.08f), new Vector3(0.012f, 0.44f, 0.012f), palette.Violet, new Vector3(0f, 0f, 22f));
        Primitive("REIHA_WRIST_EDGE", PrimitiveType.Cube, rightHand, new Vector3(0.08f, -0.08f, 0.04f), new Vector3(0.018f, 0.22f, 0.025f), palette.Violet, new Vector3(0f, 0f, -18f));
        BuildCoatPanels(pelvis, false);
    }

    private static void BuildMioGear(Transform root, Transform chest, Transform pelvis, Transform leftHand)
    {
        Primitive("MIO_PROJECTOR_CHEST", PrimitiveType.Cylinder, chest, new Vector3(0f, 0.10f, 0.25f), new Vector3(0.10f, 0.025f, 0.10f), palette.Blue, new Vector3(90f, 0f, 0f));
        Primitive("MIO_WRIST_PROJECTOR", PrimitiveType.Cylinder, leftHand, new Vector3(0f, -0.04f, 0.08f), new Vector3(0.075f, 0.018f, 0.075f), palette.Blue, new Vector3(90f, 0f, 0f));
        BuildCoatPanels(pelvis, true);

        Transform dronePivot = Bone(root, "DRONE_PIVOT", new Vector3(0f, 1.34f, 0f));
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            Vector3 local = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0.05f, 0.62f);
            GameObject drone = new GameObject("MIO_DRONE_" + i);
            drone.transform.SetParent(dronePivot, false);
            drone.transform.localPosition = local;
            drone.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            Primitive("DRONE_BODY", PrimitiveType.Cylinder, drone.transform, Vector3.zero, new Vector3(0.12f, 0.025f, 0.12f), palette.Armor);
            Primitive("DRONE_RING", PrimitiveType.Cylinder, drone.transform, new Vector3(0f, -0.018f, 0f), new Vector3(0.16f, 0.008f, 0.16f), palette.Blue);
            Primitive("DRONE_EYE", PrimitiveType.Sphere, drone.transform, new Vector3(0f, -0.04f, 0f), Vector3.one * 0.04f, palette.Glass);
        }
    }

    private static void BuildCoatPanels(Transform pelvis, bool longCoat)
    {
        float yScale = longCoat ? 0.72f : 0.48f;
        GameObject left = MeshPart("COAT_PANEL_L", pelvis, coatPanelMesh, new Vector3(-0.18f, -0.08f, -0.09f), new Vector3(0.26f, yScale, 0.18f), palette.Armor, new Vector3(4f, 4f, 4f));
        GameObject right = MeshPart("COAT_PANEL_R", pelvis, coatPanelMesh, new Vector3(0.18f, -0.08f, -0.09f), new Vector3(0.26f, yScale, 0.18f), palette.Armor, new Vector3(4f, -4f, -4f));
        Primitive("COAT_ACCENT_L", PrimitiveType.Cube, left.transform, new Vector3(0f, -0.22f, -0.06f), new Vector3(0.018f, 0.18f, 0.012f), palette.Blue);
        Primitive("COAT_ACCENT_R", PrimitiveType.Cube, right.transform, new Vector3(0f, -0.22f, -0.06f), new Vector3(0.018f, 0.18f, 0.012f), palette.Blue);
    }

    private static void ShoulderPlate(string name, Transform parent, float side, KurokageAgentArchetype archetype)
    {
        Material material = archetype == KurokageAgentArchetype.Reiha && side < 0f ? palette.Gunmetal : palette.Armor;
        Primitive(name, PrimitiveType.Cube, parent, new Vector3(side * 0.035f, -0.04f, 0f), new Vector3(0.23f, 0.12f, 0.22f), material, new Vector3(0f, 0f, side * -10f));
        Primitive(name + "_ACCENT", PrimitiveType.Cube, parent, new Vector3(side * 0.04f, -0.02f, 0.13f), new Vector3(0.08f, 0.025f, 0.018f), archetype == KurokageAgentArchetype.Reiha ? palette.Violet : palette.Blue);
    }

    private static void Limb(string name, Transform bone, float length, float radius, Material material)
    {
        Primitive(name, PrimitiveType.Capsule, bone, new Vector3(0f, -length * 0.5f, 0f), new Vector3(radius, length * 0.5f, radius), material);
    }

    private static Transform Bone(Transform parent, string name, Vector3 localPosition, Vector3? localEuler = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler ?? Vector3.zero);
        return go.transform;
    }

    private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3? localEuler = null)
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

    private static GameObject MeshPart(string name, Transform parent, Mesh mesh, Vector3 localPosition, Vector3 localScale, Material material, Vector3? localEuler = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler ?? Vector3.zero);
        go.transform.localScale = localScale;
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
        return go;
    }

    private static void EnsureAssets()
    {
        EnsureFolder("Assets/RenkaiKurokage/Art");
        EnsureFolder(MaterialFolder);
        EnsureFolder(MeshFolder);

        palette = new Palette
        {
            Skin = GetOrCreateMaterial("M_Agent_Skin", new Color(0.76f, 0.61f, 0.54f), 0.38f, 0f, Color.black),
            Hair = GetOrCreateMaterial("M_Agent_Hair", new Color(0.018f, 0.026f, 0.045f), 0.56f, 0.08f, Color.black),
            Suit = GetOrCreateMaterial("M_Agent_BlackSuit", new Color(0.028f, 0.043f, 0.070f), 0.48f, 0.16f, Color.black),
            Armor = GetOrCreateMaterial("M_Agent_WhiteArmor", new Color(0.66f, 0.72f, 0.80f), 0.62f, 0.22f, Color.black),
            Gunmetal = GetOrCreateMaterial("M_Agent_Gunmetal", new Color(0.075f, 0.10f, 0.15f), 0.72f, 0.68f, Color.black),
            Blue = GetOrCreateMaterial("M_Agent_BlueEnergy", new Color(0.08f, 0.38f, 0.96f), 0.55f, 0.12f, new Color(0.08f, 0.52f, 1f) * 2.2f),
            Violet = GetOrCreateMaterial("M_Agent_VioletEnergy", new Color(0.38f, 0.16f, 0.80f), 0.55f, 0.12f, new Color(0.54f, 0.24f, 1f) * 1.65f),
            Glass = GetOrCreateMaterial("M_Agent_Glass", new Color(0.11f, 0.36f, 0.62f), 0.86f, 0.05f, new Color(0.08f, 0.46f, 1f) * 1.2f)
        };

        taperedTorsoMesh = GetOrCreateTaperedMesh();
        coatPanelMesh = GetOrCreateCoatPanelMesh();
        AssetDatabase.SaveAssets();
    }

    private static Material GetOrCreateMaterial(string name, Color color, float smoothness, float metallic, Color emission)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = ResolveLitShader();
        if (shader == null) return material;

        if (material == null)
        {
            material = new Material(shader) { name = name };
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
        return material;
    }

    private static Shader ResolveLitShader()
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
        return shader;
    }

    private static Mesh GetOrCreateTaperedMesh()
    {
        string path = MeshFolder + "/Agent_TaperedTorso.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;

        Vector3[] vertices =
        {
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.38f, 0.5f, -0.42f), new Vector3(0.38f, 0.5f, -0.42f),
            new Vector3(0.38f, 0.5f, 0.42f), new Vector3(-0.38f, 0.5f, 0.42f)
        };
        int[] triangles =
        {
            0,2,1, 0,3,2, 4,5,6, 4,6,7,
            0,1,5, 0,5,4, 1,2,6, 1,6,5,
            2,3,7, 2,7,6, 3,0,4, 3,4,7
        };
        mesh = new Mesh { name = "Agent_TaperedTorso" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Mesh GetOrCreateCoatPanelMesh()
    {
        string path = MeshFolder + "/Agent_CoatPanel.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;

        Vector3[] vertices =
        {
            new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f),
            new Vector3(0.34f, -0.5f, 0.12f), new Vector3(-0.24f, -0.5f, 0.12f),
            new Vector3(-0.5f, 0.5f, -0.08f), new Vector3(0.5f, 0.5f, -0.08f),
            new Vector3(0.34f, -0.5f, 0.04f), new Vector3(-0.24f, -0.5f, 0.04f)
        };
        int[] triangles =
        {
            0,1,2, 0,2,3, 5,4,7, 5,7,6,
            4,5,1, 4,1,0, 3,2,6, 3,6,7,
            1,5,6, 1,6,2, 4,0,3, 4,3,7
        };
        mesh = new Mesh { name = "Agent_CoatPanel" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
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
