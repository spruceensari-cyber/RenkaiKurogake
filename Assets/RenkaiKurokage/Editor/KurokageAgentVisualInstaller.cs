using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

/// <summary>
/// Builds the production roster from original in-project geometry. No imported character model is used.
/// </summary>
public static class KurokageAgentVisualInstaller
{
    [MenuItem("Renkai/Install Code-Built Agent Visuals")]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Renkai", "Play modundan çıkıp tekrar çalıştır.", "OK");
            return;
        }

        bool ok = InstallSilent();
        EditorUtility.DisplayDialog(
            "Renkai",
            ok
                ? "Kod tabanlı agent roster, PBR yüzey detayları, reflection capture, bot avoidance ve collision integrity katmanı kuruldu."
                : "Kurulum tamamlanamadı. Console ve production validation raporunu kontrol et.",
            ok ? "OK" : "REVIEW"
        );
    }

    private const string MaterialFolder = "Assets/RenkaiKurokage/Art/OriginalAgentMaterials";
    private const string PreviousMaterialFolder = "Assets/RenkaiKurokage/Art/AgentMaterials";

    private static readonly AgentDesign[] Roster =
    {
        new AgentDesign(KurokageAgentArchetype.Kairi, "KAIRI", "RIFT-07", "Duelist", new Color(0.07f, 0.12f, 0.19f), new Color(0.035f, 0.055f, 0.09f), new Color(0.17f, 0.67f, 1f), 1.00f, 1.00f),
        new AgentDesign(KurokageAgentArchetype.Noa, "NOA", "PULSE-D5", "Recon", new Color(0.13f, 0.12f, 0.15f), new Color(0.06f, 0.055f, 0.075f), new Color(0.96f, 0.43f, 0.26f), 0.96f, 0.92f),
        new AgentDesign(KurokageAgentArchetype.Reiha, "REIHA", "VEIL-A3", "Controller", new Color(0.12f, 0.07f, 0.19f), new Color(0.055f, 0.035f, 0.09f), new Color(0.59f, 0.34f, 0.92f), 1.05f, 0.98f),
        new AgentDesign(KurokageAgentArchetype.Mio, "MIO", "GLINT-C2", "Intel", new Color(0.05f, 0.16f, 0.17f), new Color(0.025f, 0.07f, 0.08f), new Color(0.18f, 0.86f, 0.77f), 0.93f, 0.90f),
        new AgentDesign(KurokageAgentArchetype.Sora, "SORA", "BASTION-R4", "Vanguard", new Color(0.16f, 0.13f, 0.08f), new Color(0.07f, 0.055f, 0.03f), new Color(1f, 0.68f, 0.18f), 1.08f, 1.18f),
        new AgentDesign(KurokageAgentArchetype.Aiko, "AIKO", "LANCER-M9", "Skirmisher", new Color(0.08f, 0.16f, 0.14f), new Color(0.035f, 0.07f, 0.06f), new Color(0.29f, 0.88f, 0.57f), 0.99f, 0.95f),
        new AgentDesign(KurokageAgentArchetype.Ren, "REN", "FORGE-K6", "Breacher", new Color(0.18f, 0.09f, 0.055f), new Color(0.075f, 0.035f, 0.025f), new Color(1f, 0.34f, 0.12f), 1.04f, 1.12f),
        new AgentDesign(KurokageAgentArchetype.Hana, "HANA", "ORBIT-S1", "Support", new Color(0.15f, 0.08f, 0.13f), new Color(0.07f, 0.035f, 0.06f), new Color(0.98f, 0.47f, 0.71f), 0.97f, 0.93f),
        new AgentDesign(KurokageAgentArchetype.Toma, "TOMA", "ANCHOR-V8", "Sentinel", new Color(0.07f, 0.15f, 0.10f), new Color(0.025f, 0.07f, 0.045f), new Color(0.46f, 0.90f, 0.28f), 1.12f, 1.20f),
        new AgentDesign(KurokageAgentArchetype.Yori, "YORI", "WRAITH-N2", "Infiltrator", new Color(0.075f, 0.08f, 0.17f), new Color(0.03f, 0.035f, 0.08f), new Color(0.40f, 0.55f, 1f), 1.02f, 0.94f)
    };

    public static bool InstallSilent()
    {
        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        if (players == null || players.Length == 0) return false;

        System.Array.Sort(players, ComparePlayers);

        bool result = true;

        if (players.Length == Roster.Length)
        {
            EnsureMaterialFolder();
            bool success = true;
            for (int i = 0; i < players.Length; i++)
            {
                RenkaiRoundPlayer player = players[i];
                AgentDesign design = Roster[i];
                player.agentName = design.DisplayName + " // " + design.Callsign;
                if (!BuildOriginalVisual(player, design)) success = false;
            }

            RemovePreviousCharacterMaterials();
            result = success;
        }
        else
        {
            bool surfaceOk = KurokageSurfaceDetailPass.ApplySilent();
            bool qualityOk = KurokageHighFidelityQualityPass.ApplySilent();
            bool allAgentsOk = true;
            int attackerIndex = 0;
            int defenderIndex = 0;

            foreach (RenkaiRoundPlayer player in players)
            {
                KurokageAgentArchetype archetype;
                if (player.isHumanPlayer)
                {
                    archetype = KurokageAgentArchetype.Kairi;
                }
                else if (player.team == RenkaiTeam.Attackers)
                {
                    archetype = ResolveArchetype(attackerIndex++);
                }
                else
                {
                    archetype = ResolveArchetype(defenderIndex++ + 1);
                }

                player.agentName = BuildAgentName(player, archetype);

                bool built = KurokageProceduralAgentFactory.Build(player, archetype);
                if (built)
                    built = KurokageProceduralAgentDetailPass.Apply(player, archetype);

                if (!player.isHumanPlayer && player.GetComponent<KurokageBotLocalAvoidance>() == null)
                    player.gameObject.AddComponent<KurokageBotLocalAvoidance>();

                if (!built)
                {
                    allAgentsOk = false;
                    Debug.LogError("Renkai code-built agent visual failed for " + player.agentName + " archetype=" + archetype);
                }
            }

            bool collisionOk = KurokageCollisionIntegrityPass.ApplySilent();
            result = allAgentsOk && surfaceOk && qualityOk && collisionOk;
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = players[0].gameObject;
        return allAgentsOk && surfaceOk && qualityOk && collisionOk;
    }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = players[0].gameObject;
        return result;
    }

    private static KurokageAgentArchetype ResolveArchetype(int index)
    {
        switch (Mathf.Abs(index) % 4)
        {
            case 0: return KurokageAgentArchetype.Noa;
            case 1: return KurokageAgentArchetype.Reiha;
            case 2: return KurokageAgentArchetype.Mio;
            default: return KurokageAgentArchetype.Kairi;
        }
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
        if (string.IsNullOrWhiteSpace(value)) return "UNIT";
        int delimiter = value.LastIndexOf("//", System.StringComparison.Ordinal);
        return delimiter >= 0 ? value.Substring(delimiter + 2).Trim() : value.Trim();
    }

    private static bool BuildOriginalVisual(RenkaiRoundPlayer player, AgentDesign design)
    {
        Transform oldVisual = player.transform.Find("AGENT_VISUAL");
        if (oldVisual != null)
            Object.DestroyImmediate(oldVisual.gameObject);

        GameObject visualRoot = new GameObject("AGENT_VISUAL");
        visualRoot.transform.SetParent(player.transform, false);
        visualRoot.transform.localPosition = Vector3.zero;
        visualRoot.transform.localRotation = Quaternion.identity;
        visualRoot.transform.localScale = Vector3.one;

        GameObject model = new GameObject("ORIGINAL_AGENT_" + design.DisplayName);
        model.transform.SetParent(visualRoot.transform, false);

        AgentMaterials materials = CreateMaterials(design, player.team);
        if (!materials.IsValid)
        {
            Object.DestroyImmediate(visualRoot);
            return false;
        }

        AgentRig rig = BuildBaseRig(model.transform, design, materials);
        BuildSignature(model.transform, rig, design, materials);
        BuildTeamReadabilityMarker(model.transform, design, player.team, materials.Team);

        KurokageOriginalAgentPose originalPose = visualRoot.AddComponent<KurokageOriginalAgentPose>();
        originalPose.Configure(rig.Head, rig.Torso, rig.LeftArm, rig.RightArm, rig.LeftLeg, rig.RightLeg);
        visualRoot.AddComponent<KurokageAgentAnimationDriver>();

        Color teamAccent = player.team == RenkaiTeam.Attackers
            ? new Color(0.12f, 0.58f, 1f, 1f)
            : new Color(0.72f, 0.28f, 0.85f, 1f);
        KurokageAgentReadabilityPresenter readability = visualRoot.AddComponent<KurokageAgentReadabilityPresenter>();
        readability.Configure(teamAccent, 0.12f);

        if (player.GetComponent<KurokageHitReactionPresenter>() == null)
            player.gameObject.AddComponent<KurokageHitReactionPresenter>();

        KurokageHitZoneBinder binder = player.GetComponent<KurokageHitZoneBinder>();
        if (binder == null) binder = player.gameObject.AddComponent<KurokageHitZoneBinder>();
        binder.Bind();
        return binder.BoundZoneCount >= 6;
    }

    private static AgentRig BuildBaseRig(Transform root, AgentDesign design, AgentMaterials materials)
    {
        float h = design.Height;
        float w = design.Width;

        Transform pelvis = CreatePart("PELVIS", PrimitiveType.Cube, root, new Vector3(0f, 0.84f * h, 0f), new Vector3(0.38f * w, 0.18f * h, 0.27f * w), materials.Body, Vector3.zero);
        Transform torso = CreatePivot("TORSO", root, new Vector3(0f, 1.18f * h, 0f));
        CreatePart("CHEST", PrimitiveType.Cylinder, torso, Vector3.zero, new Vector3(0.34f * w, 0.28f * h, 0.27f * w), materials.Body, Vector3.zero);
        CreatePart("CHEST_PLATE", PrimitiveType.Cube, torso, new Vector3(0f, 0.04f * h, 0.26f * w), new Vector3(0.29f * w, 0.20f * h, 0.025f), materials.Fabric, Vector3.zero);
        CreatePart("CHEST_CORE", PrimitiveType.Cube, torso, new Vector3(0f, 0.05f * h, 0.295f * w), new Vector3(0.075f * w, 0.075f * h, 0.018f), materials.Accent, Vector3.zero);
        CreatePart("COLLAR", PrimitiveType.Cylinder, torso, new Vector3(0f, 0.28f * h, 0f), new Vector3(0.17f * w, 0.07f * h, 0.17f * w), materials.Metal, Vector3.zero);

        Transform head = CreatePart("HEAD", PrimitiveType.Sphere, torso, new Vector3(0f, 0.52f * h, 0f), new Vector3(0.23f * w, 0.25f * h, 0.23f * w), materials.Fabric, Vector3.zero);
        CreatePart("VISOR", PrimitiveType.Cube, head, new Vector3(0f, 0.02f, 0.92f), new Vector3(0.68f, 0.22f, 0.16f), materials.Visor, Vector3.zero);
        CreatePart("HELMET_CREST", PrimitiveType.Cube, head, new Vector3(0f, 0.82f, -0.04f), new Vector3(0.20f, 0.22f, 0.40f), materials.Body, new Vector3(0f, 0f, 7f));

        Transform leftArm = CreatePivot("LEFT_UPPER_ARM", torso, new Vector3(-0.38f * w, 0.20f * h, 0f));
        BuildArm(leftArm, "LEFT", h, w, materials);
        Transform rightArm = CreatePivot("RIGHT_UPPER_ARM", torso, new Vector3(0.38f * w, 0.20f * h, 0f));
        BuildArm(rightArm, "RIGHT", h, w, materials);

        Transform leftLeg = CreatePivot("LEFT_THIGH", root, new Vector3(-0.16f * w, 0.73f * h, 0f));
        BuildLeg(leftLeg, "LEFT", h, w, materials);
        Transform rightLeg = CreatePivot("RIGHT_THIGH", root, new Vector3(0.16f * w, 0.73f * h, 0f));
        BuildLeg(rightLeg, "RIGHT", h, w, materials);

        CreatePart("SHOULDER_L", PrimitiveType.Cube, torso, new Vector3(-0.38f * w, 0.22f * h, 0f), new Vector3(0.17f * w, 0.09f * h, 0.23f * w), materials.Metal, new Vector3(0f, 0f, -10f));
        CreatePart("SHOULDER_R", PrimitiveType.Cube, torso, new Vector3(0.38f * w, 0.22f * h, 0f), new Vector3(0.17f * w, 0.09f * h, 0.23f * w), materials.Metal, new Vector3(0f, 0f, 10f));
        CreatePart("BELT", PrimitiveType.Cube, root, new Vector3(0f, 0.86f * h, 0.02f), new Vector3(0.42f * w, 0.055f * h, 0.29f * w), materials.Metal, Vector3.zero);

        return new AgentRig(root, pelvis, head, torso, leftArm, rightArm, leftLeg, rightLeg, h, w);
    }

    private static void BuildArm(Transform pivot, string side, float h, float w, AgentMaterials materials)
    {
        CreatePart(side + "_UPPER_ARM_ARMOR", PrimitiveType.Capsule, pivot, new Vector3(0f, -0.18f * h, 0f), new Vector3(0.115f * w, 0.18f * h, 0.115f * w), materials.Fabric, Vector3.zero);
        CreatePart(side + "_FOREARM", PrimitiveType.Cube, pivot, new Vector3(0f, -0.43f * h, 0.035f), new Vector3(0.13f * w, 0.16f * h, 0.14f * w), materials.Body, new Vector3(4f, 0f, 0f));
        CreatePart(side + "_HAND", PrimitiveType.Sphere, pivot, new Vector3(0f, -0.63f * h, 0.045f), new Vector3(0.11f * w, 0.11f * h, 0.11f * w), materials.Metal, Vector3.zero);
        CreatePart(side + "_ARM_LINE", PrimitiveType.Cube, pivot, new Vector3(0f, -0.40f * h, 0.16f * w), new Vector3(0.025f, 0.22f * h, 0.015f), materials.Accent, Vector3.zero);
    }

    private static void BuildLeg(Transform pivot, string side, float h, float w, AgentMaterials materials)
    {
        CreatePart(side + "_THIGH_ARMOR", PrimitiveType.Capsule, pivot, new Vector3(0f, -0.19f * h, 0f), new Vector3(0.14f * w, 0.20f * h, 0.14f * w), materials.Fabric, Vector3.zero);
        CreatePart(side + "_SHIN", PrimitiveType.Cube, pivot, new Vector3(0f, -0.49f * h, 0.02f), new Vector3(0.15f * w, 0.19f * h, 0.16f * w), materials.Body, new Vector3(-3f, 0f, 0f));
        CreatePart(side + "_BOOT", PrimitiveType.Cube, pivot, new Vector3(0f, -0.72f * h, 0.08f * w), new Vector3(0.18f * w, 0.10f * h, 0.26f * w), materials.Metal, Vector3.zero);
        CreatePart(side + "_KNEE_LIGHT", PrimitiveType.Sphere, pivot, new Vector3(0f, -0.37f * h, 0.16f * w), new Vector3(0.055f * w, 0.055f * h, 0.035f), materials.Accent, Vector3.zero);
    }

    private static void BuildSignature(Transform root, AgentRig rig, AgentDesign design, AgentMaterials materials)
    {
        float h = rig.Height;
        float w = rig.Width;
        switch (design.Archetype)
        {
            case KurokageAgentArchetype.Kairi:
                CreatePart("KAIRI_SHEATH", PrimitiveType.Cube, root, new Vector3(-0.31f * w, 0.93f * h, -0.23f * w), new Vector3(0.065f * w, 0.38f * h, 0.08f * w), materials.Metal, new Vector3(0f, 0f, -24f));
                CreatePart("KAIRI_BLADE_LINE", PrimitiveType.Cube, root, new Vector3(-0.34f * w, 0.98f * h, -0.29f * w), new Vector3(0.016f, 0.31f * h, 0.014f), materials.Accent, new Vector3(0f, 0f, -24f));
                CreatePart("KAIRI_COAT_TAIL_L", PrimitiveType.Cube, root, new Vector3(-0.15f * w, 0.63f * h, -0.16f * w), new Vector3(0.12f * w, 0.31f * h, 0.035f), materials.Fabric, new Vector3(0f, 0f, 13f));
                CreatePart("KAIRI_COAT_TAIL_R", PrimitiveType.Cube, root, new Vector3(0.15f * w, 0.63f * h, -0.16f * w), new Vector3(0.12f * w, 0.31f * h, 0.035f), materials.Fabric, new Vector3(0f, 0f, -13f));
                break;
            case KurokageAgentArchetype.Noa:
                CreatePart("NOA_SENSOR_ARRAY", PrimitiveType.Cylinder, root, new Vector3(0.32f * w, 1.63f * h, 0.02f), new Vector3(0.09f * w, 0.022f * h, 0.09f * w), materials.Accent, new Vector3(90f, 0f, 0f));
                CreatePart("NOA_DRONE_CORE", PrimitiveType.Sphere, root, new Vector3(0f, 1.27f * h, -0.30f * w), new Vector3(0.13f * w, 0.13f * h, 0.13f * w), materials.Accent, Vector3.zero);
                CreatePart("NOA_DRONE_FRAME", PrimitiveType.Cube, root, new Vector3(0f, 1.27f * h, -0.33f * w), new Vector3(0.36f * w, 0.035f * h, 0.08f * w), materials.Metal, Vector3.zero);
                break;
            case KurokageAgentArchetype.Reiha:
                CreatePart("REIHA_CROWN_LEFT", PrimitiveType.Cube, root, new Vector3(-0.18f * w, 1.90f * h, 0f), new Vector3(0.045f * w, 0.20f * h, 0.045f * w), materials.Accent, new Vector3(0f, 0f, -18f));
                CreatePart("REIHA_CROWN_CENTER", PrimitiveType.Cube, root, new Vector3(0f, 1.96f * h, 0f), new Vector3(0.04f * w, 0.24f * h, 0.04f * w), materials.Accent, Vector3.zero);
                CreatePart("REIHA_CROWN_RIGHT", PrimitiveType.Cube, root, new Vector3(0.18f * w, 1.90f * h, 0f), new Vector3(0.045f * w, 0.20f * h, 0.045f * w), materials.Accent, new Vector3(0f, 0f, 18f));
                CreatePart("REIHA_MANTLE", PrimitiveType.Cube, root, new Vector3(0f, 0.80f * h, -0.22f * w), new Vector3(0.42f * w, 0.48f * h, 0.045f), materials.Fabric, Vector3.zero);
                break;
            case KurokageAgentArchetype.Mio:
                CreatePart("MIO_HOOD", PrimitiveType.Cylinder, root, new Vector3(0f, 1.66f * h, -0.02f), new Vector3(0.29f * w, 0.14f * h, 0.29f * w), materials.Metal, Vector3.zero);
                for (int i = 0; i < 3; i++)
                {
                    float angle = i * Mathf.PI * 2f / 3f;
                    CreatePart("MIO_ORBIT_" + i, PrimitiveType.Sphere, root, new Vector3(Mathf.Cos(angle) * 0.33f * w, 1.43f * h + Mathf.Sin(angle) * 0.11f * h, -0.18f * w), new Vector3(0.065f * w, 0.065f * h, 0.065f * w), materials.Accent, Vector3.zero);
                }
                break;
            case KurokageAgentArchetype.Sora:
                CreatePart("SORA_BULWARK_SHIELD", PrimitiveType.Cube, root, new Vector3(-0.47f * w, 1.20f * h, 0.18f * w), new Vector3(0.12f * w, 0.37f * h, 0.07f * w), materials.Metal, new Vector3(0f, 0f, -10f));
                CreatePart("SORA_BACKPACK", PrimitiveType.Cube, root, new Vector3(0f, 1.20f * h, -0.27f * w), new Vector3(0.34f * w, 0.30f * h, 0.13f * w), materials.Fabric, Vector3.zero);
                CreatePart("SORA_CORE", PrimitiveType.Cylinder, root, new Vector3(0f, 1.20f * h, -0.42f * w), new Vector3(0.09f * w, 0.03f * h, 0.09f * w), materials.Accent, new Vector3(90f, 0f, 0f));
                break;
            case KurokageAgentArchetype.Aiko:
                CreatePart("AIKO_SCARF", PrimitiveType.Cylinder, root, new Vector3(0f, 1.49f * h, 0f), new Vector3(0.24f * w, 0.07f * h, 0.24f * w), materials.Accent, Vector3.zero);
                CreatePart("AIKO_SCARF_TAIL_A", PrimitiveType.Cube, root, new Vector3(-0.24f * w, 1.03f * h, -0.18f * w), new Vector3(0.055f * w, 0.42f * h, 0.035f), materials.Accent, new Vector3(0f, 0f, 12f));
                CreatePart("AIKO_SCARF_TAIL_B", PrimitiveType.Cube, root, new Vector3(0.20f * w, 0.94f * h, -0.18f * w), new Vector3(0.045f * w, 0.34f * h, 0.035f), materials.Fabric, new Vector3(0f, 0f, -17f));
                break;
            case KurokageAgentArchetype.Ren:
                CreatePart("REN_BREAKER_GAUNTLET", PrimitiveType.Cube, rig.RightArm, new Vector3(0f, -0.39f * h, 0.10f * w), new Vector3(0.23f * w, 0.21f * h, 0.24f * w), materials.Metal, Vector3.zero);
                CreatePart("REN_GAUNTLET_CORE", PrimitiveType.Sphere, rig.RightArm, new Vector3(0f, -0.39f * h, 0.24f * w), new Vector3(0.07f * w, 0.07f * h, 0.04f), materials.Accent, Vector3.zero);
                CreatePart("REN_BACK_VENT", PrimitiveType.Cube, root, new Vector3(0f, 1.30f * h, -0.28f * w), new Vector3(0.30f * w, 0.25f * h, 0.08f * w), materials.Metal, Vector3.zero);
                break;
            case KurokageAgentArchetype.Hana:
                BuildHalo(root, h, w, materials.Accent);
                CreatePart("HANA_WING_L", PrimitiveType.Cube, root, new Vector3(-0.38f * w, 1.25f * h, -0.20f * w), new Vector3(0.30f * w, 0.06f * h, 0.035f), materials.Fabric, new Vector3(0f, 0f, 25f));
                CreatePart("HANA_WING_R", PrimitiveType.Cube, root, new Vector3(0.38f * w, 1.25f * h, -0.20f * w), new Vector3(0.30f * w, 0.06f * h, 0.035f), materials.Fabric, new Vector3(0f, 0f, -25f));
                break;
            case KurokageAgentArchetype.Toma:
                CreatePart("TOMA_EXO_PACK", PrimitiveType.Cube, root, new Vector3(0f, 1.20f * h, -0.31f * w), new Vector3(0.42f * w, 0.34f * h, 0.16f * w), materials.Metal, Vector3.zero);
                CreatePart("TOMA_CORE_LEFT", PrimitiveType.Sphere, root, new Vector3(-0.17f * w, 1.25f * h, -0.48f * w), new Vector3(0.09f * w, 0.09f * h, 0.06f), materials.Accent, Vector3.zero);
                CreatePart("TOMA_CORE_RIGHT", PrimitiveType.Sphere, root, new Vector3(0.17f * w, 1.25f * h, -0.48f * w), new Vector3(0.09f * w, 0.09f * h, 0.06f), materials.Accent, Vector3.zero);
                CreatePart("TOMA_ANTENNA", PrimitiveType.Cube, root, new Vector3(0.18f * w, 1.73f * h, -0.13f * w), new Vector3(0.02f, 0.25f * h, 0.02f), materials.Accent, new Vector3(0f, 0f, -18f));
                break;
            case KurokageAgentArchetype.Yori:
                CreatePart("YORI_MASK", PrimitiveType.Cube, root, new Vector3(0f, 1.64f * h, 0.22f * w), new Vector3(0.24f * w, 0.18f * h, 0.035f), materials.Metal, Vector3.zero);
                CreatePart("YORI_CAPE", PrimitiveType.Cube, root, new Vector3(0f, 0.85f * h, -0.23f * w), new Vector3(0.48f * w, 0.58f * h, 0.04f), materials.Fabric, Vector3.zero);
                CreatePart("YORI_CAPE_LINE", PrimitiveType.Cube, root, new Vector3(0f, 0.85f * h, -0.285f * w), new Vector3(0.035f, 0.54f * h, 0.012f), materials.Accent, Vector3.zero);
                break;
        }
    }

    private static void BuildHalo(Transform root, float h, float w, Material material)
    {
        const int segments = 8;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 0.33f * w, 1.77f * h + Mathf.Sin(angle) * 0.24f * h, 0.02f);
            CreatePart("HANA_HALO_" + i, PrimitiveType.Cube, root, position, new Vector3(0.065f * w, 0.028f * h, 0.024f), material, new Vector3(0f, 0f, i * 45f));
        }
    }

    private static void BuildTeamReadabilityMarker(Transform root, AgentDesign design, RenkaiTeam team, Material teamMaterial)
    {
        float side = team == RenkaiTeam.Attackers ? -1f : 1f;
        CreatePart("TEAM_SIGNAL", PrimitiveType.Cube, root, new Vector3(side * 0.39f * design.Width, 1.42f * design.Height, 0.20f * design.Width), new Vector3(0.055f * design.Width, 0.18f * design.Height, 0.028f), teamMaterial, new Vector3(0f, 0f, side * 12f));
        CreatePart("TEAM_BEACON", PrimitiveType.Sphere, root, new Vector3(side * 0.40f * design.Width, 1.52f * design.Height, 0.24f * design.Width), new Vector3(0.055f * design.Width, 0.055f * design.Height, 0.035f), teamMaterial, Vector3.zero);
    }

    private static AgentMaterials CreateMaterials(AgentDesign design, RenkaiTeam team)
    {
        Color teamColor = team == RenkaiTeam.Attackers
            ? new Color(0.10f, 0.58f, 1f)
            : new Color(0.68f, 0.30f, 0.84f);
        string prefix = "M_Original_" + design.DisplayName + "_";
        return new AgentMaterials(
            CreateMaterial(prefix + "Body", design.Body, 0.45f, 0.45f, 0f),
            CreateMaterial(prefix + "Fabric", design.Fabric, 0.05f, 0.25f, 0f),
            CreateMaterial(prefix + "Accent", design.Accent, 0.35f, 0.55f, 1.45f),
            CreateMaterial(prefix + "Visor", Color.Lerp(design.Accent, Color.white, 0.22f), 0.55f, 0.75f, 2.0f),
            CreateMaterial("M_Original_" + (team == RenkaiTeam.Attackers ? "Alpha" : "Omega") + "_Signal", teamColor, 0.25f, 0.55f, 1.65f),
            CreateMaterial("M_Original_DarkMetal", new Color(0.035f, 0.045f, 0.065f), 0.78f, 0.35f, 0f));
    }

    private static Material CreateMaterial(string name, Color color, float metallic, float smoothness, float emission)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = ResolveCharacterShader();
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
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission > 0f ? color * emission : Color.black);
            if (emission > 0f) material.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader ResolveCharacterShader()
    {
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        string pipelineName = pipeline != null ? pipeline.GetType().FullName : string.Empty;
        if (!string.IsNullOrEmpty(pipelineName) && pipelineName.IndexOf("Universal", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp != null && urp.isSupported) return urp;
        }
        if (!string.IsNullOrEmpty(pipelineName) && pipelineName.IndexOf("HDRenderPipeline", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Shader hdrp = Shader.Find("HDRP/Lit");
            if (hdrp != null && hdrp.isSupported) return hdrp;
        }

        Shader standard = Shader.Find("Standard");
        if (standard != null && standard.isSupported) return standard;
        Shader diffuse = Shader.Find("Legacy Shaders/Diffuse");
        if (diffuse != null && diffuse.isSupported) return diffuse;
        return Shader.Find("Unlit/Color");
    }

    private static Transform CreatePivot(string name, Transform parent, Vector3 localPosition)
    {
        GameObject pivot = new GameObject(name);
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = localPosition;
        pivot.transform.localRotation = Quaternion.identity;
        pivot.transform.localScale = Vector3.one;
        return pivot.transform;
    }

    private static Transform CreatePart(string name, PrimitiveType primitive, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3 localEuler)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localEuler);
        part.transform.localScale = localScale;

        foreach (Collider collider in part.GetComponents<Collider>())
            Object.DestroyImmediate(collider);

        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
        return part.transform;
    }

    private static void EnsureMaterialFolder()
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/RenkaiKurokage/Art", "OriginalAgentMaterials");
    }

    private static void RemovePreviousCharacterMaterials()
    {
        if (AssetDatabase.IsValidFolder(PreviousMaterialFolder))
            AssetDatabase.DeleteAsset(PreviousMaterialFolder);
    }

    private static int ComparePlayers(RenkaiRoundPlayer a, RenkaiRoundPlayer b)
    {
    private static int ComparePlayers(RenkaiRoundPlayer a, RenkaiRoundPlayer b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        if (a.isHumanPlayer != b.isHumanPlayer)
            return a.isHumanPlayer ? -1 : 1;

        int team = a.team.CompareTo(b.team);
        return team != 0 ? team : string.CompareOrdinal(a.agentName, b.agentName);
    }

    private sealed class AgentDesign
    {
        public readonly KurokageAgentArchetype Archetype;
        public readonly string DisplayName;
        public readonly string Callsign;
        public readonly string Role;
        public readonly Color Body;
        public readonly Color Fabric;
        public readonly Color Accent;
        public readonly float Height;
        public readonly float Width;

        public AgentDesign(KurokageAgentArchetype archetype, string displayName, string callsign, string role, Color body, Color fabric, Color accent, float height, float width)
        {
            Archetype = archetype;
            DisplayName = displayName;
            Callsign = callsign;
            Role = role;
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
        public readonly Material Visor;
        public readonly Material Team;
        public readonly Material Metal;

        public bool IsValid => Body != null && Fabric != null && Accent != null && Visor != null && Team != null && Metal != null;

        public AgentMaterials(Material body, Material fabric, Material accent, Material visor, Material team, Material metal)
        {
            Body = body;
            Fabric = fabric;
            Accent = accent;
            Visor = visor;
            Team = team;
            Metal = metal;
        }
    }

    private struct AgentRig
    {
        public readonly Transform Root;
        public readonly Transform Pelvis;
        public readonly Transform Head;
        public readonly Transform Torso;
        public readonly Transform LeftArm;
        public readonly Transform RightArm;
        public readonly Transform LeftLeg;
        public readonly Transform RightLeg;
        public readonly float Height;
        public readonly float Width;

        public AgentRig(Transform root, Transform pelvis, Transform head, Transform torso, Transform leftArm, Transform rightArm, Transform leftLeg, Transform rightLeg, float height, float width)
        {
            Root = root;
            Pelvis = pelvis;
            Head = head;
            Torso = torso;
            LeftArm = leftArm;
            RightArm = rightArm;
            LeftLeg = leftLeg;
            RightLeg = rightLeg;
            Height = height;
            Width = width;
        }
    }
}

    }
}
