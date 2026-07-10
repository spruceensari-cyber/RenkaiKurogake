using System.Text;
using UnityEditor;
using UnityEngine;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageProductionValidator
{
    [MenuItem("Renkai/Validate Production Build")]
    public static void Validate()
    {
        string report;
        bool passed = ValidateSilent(out report);
        Debug.Log(report);
        EditorUtility.DisplayDialog(
            "Renkai Production Validation",
            report,
            passed ? "OK" : "REVIEW"
        );
    }

    public static bool ValidateSilent(out string reportText)
    {
        int errors = 0;
        int warnings = 0;
        StringBuilder report = new StringBuilder();
        report.AppendLine("RENKAI: KUROKAGE PRODUCTION VALIDATION");
        report.AppendLine("====================================");

        ValidateCount<RenkaiFPSController>("Human FPS controller", 1, ref errors, report);
        ValidateCount<RenkaiRoundManager>("Round manager", 1, ref errors, report);
        ValidateCount<ZodiacCoreRuntime>("Zodiac Core runtime", 1, ref errors, report);
        ValidateCount<KurokageZodiacObjectiveController>("Zodiac objective controller", 1, ref errors, report);
        ValidateCount<KurokageZodiacVfxPresenter>("Zodiac VFX presenter", 1, ref errors, report);
        ValidateCount<KurokageNexusVfxPresenter>("Zodiac Nexus VFX presenters", 2, ref errors, report);
        ValidateCount<KairiAbilityController>("Kairi ability controller", 1, ref errors, report);
        ValidateCount<KurokageBladeCombatController>("Blade combat controller", 1, ref errors, report);
        ValidateCount<KurokageAfterimagePresenter>("Kairi afterimage presenter", 1, ref errors, report);
        ValidateCount<KurokageEclipseProtocolPresenter>("Eclipse Protocol presenter", 1, ref errors, report);
        ValidateCount<KurokageAgentDeathPresentation>("Agent death presenters", 10, ref errors, report);
        ValidateCount<KurokageMatchStatsTracker>("Match stats tracker", 1, ref errors, report);
        ValidateCount<KurokageScoreboardHUD>("Scoreboard HUD", 1, ref errors, report);
        ValidateCount<KurokageEliteHUD>("Elite HUD", 1, ref errors, report);
        ValidateCount<KurokageCombatFeedbackHUD>("Combat feedback HUD", 1, ref errors, report);
        ValidateCount<KurokageDamageDirectionHUD>("Damage direction HUD", 1, ref errors, report);
        ValidateCount<KurokageMatchPresentationHUD>("Match presentation HUD", 1, ref errors, report);
        ValidateCount<KurokageVfxPool>("Shared VFX pool", 1, ref errors, report);

        Camera[] cameras = Object.FindObjectsOfType<Camera>(true);
        int activeCameras = 0;
        foreach (Camera camera in cameras)
            if (camera != null && camera.enabled && camera.gameObject.activeInHierarchy) activeCameras++;
        ValidateExact("Active camera", activeCameras, 1, ref errors, report);

        AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>(true);
        int activeListeners = 0;
        foreach (AudioListener listener in listeners)
            if (listener != null && listener.enabled && listener.gameObject.activeInHierarchy) activeListeners++;
        ValidateExact("Active AudioListener", activeListeners, 1, ref errors, report);

        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        int humans = 0;
        int attackers = 0;
        int defenders = 0;
        int visibleRootRenderers = 0;
        int tacticalBots = 0;

        foreach (RenkaiRoundPlayer player in players)
        {
            if (player == null) continue;
            if (player.isHumanPlayer) humans++;
            else if (player.GetComponent<RenkaiTacticalBotAI>() != null) tacticalBots++;

            if (player.team == RenkaiTeam.Attackers) attackers++;
            else defenders++;

            Renderer rootRenderer = player.GetComponent<Renderer>();
            if (rootRenderer != null && rootRenderer.enabled)
                visibleRootRenderers++;

            KurokageHitZoneBinder binder = player.GetComponent<KurokageHitZoneBinder>();
            int zones = player.GetComponentsInChildren<KurokageHitZone>(true).Length;
            if (binder == null || zones < 3)
            {
                errors++;
                report.AppendLine("ERROR Hit zone binding incomplete for " + player.agentName + ": zones=" + zones);
            }
            else
            {
                report.AppendLine("PASS  Hit zones " + player.agentName + ": " + zones);
            }
        }

        ValidateExact("Human player", humans, 1, ref errors, report);
        ValidateExact("Attackers", attackers, 5, ref errors, report);
        ValidateExact("Defenders", defenders, 5, ref errors, report);
        ValidateExact("Tactical bots", tacticalBots, 9, ref errors, report);

        if (visibleRootRenderers > 0)
        {
            warnings++;
            report.AppendLine("WARN  Visible root player renderers: " + visibleRootRenderers + " (capsule placeholders may still be visible)");
        }
        else
        {
            report.AppendLine("PASS  No visible root player renderers");
        }

        KurokageDecoyRuntime[] sceneDecoys = Object.FindObjectsOfType<KurokageDecoyRuntime>(true);
        if (sceneDecoys.Length > 0)
        {
            errors++;
            report.AppendLine("ERROR Initial scene contains active decoy objects: " + sceneDecoys.Length);
        }
        else
        {
            report.AppendLine("PASS  No decoys in initial scene state");
        }

        ValidateEnvironmentIdentity(ref errors, ref warnings, report);

        ZodiacNexusSite[] sites = Object.FindObjectsOfType<ZodiacNexusSite>(true);
        ValidateExact("Zodiac Nexus sites", sites.Length, 2, ref errors, report);
        foreach (ZodiacNexusSite site in sites)
            ValidateNexusArt(site, ref errors, report);

        GameObject core = GameObject.Find("ZODIAC_CORE");
        if (core == null)
        {
            errors++;
            report.AppendLine("ERROR ZODIAC_CORE object missing");
        }
        else
        {
            report.AppendLine("PASS  ZODIAC_CORE object present");
            ValidateZodiacCoreArt(core.transform, ref errors, report);
        }

        string[] requiredRoots =
        {
            "KUROKAGE_ENVIRONMENT_ART",
            "KUROKAGE_ELITE_HUD",
            "KUROKAGE_COMBAT_FEEDBACK_HUD",
            "KUROKAGE_DAMAGE_DIRECTION_HUD",
            "KUROKAGE_MATCH_PRESENTATION_HUD",
            "KUROKAGE_MATCH_STATS",
            "KUROKAGE_SCOREBOARD_HUD",
            "KUROKAGE_ABILITY_HUD",
            "KUROKAGE_ZODIAC_HUD",
            "KUROKAGE_VFX_POOL",
            "RENKAI_KUROKAGE_PRODUCTION_BUILD"
        };

        foreach (string rootName in requiredRoots)
        {
            if (GameObject.Find(rootName) == null)
            {
                errors++;
                report.AppendLine("ERROR Missing required root: " + rootName);
            }
            else
            {
                report.AppendLine("PASS  Root present: " + rootName);
            }
        }

        report.AppendLine("------------------------------------");
        report.AppendLine("Errors: " + errors);
        report.AppendLine("Warnings: " + warnings);
        report.AppendLine(errors == 0 ? "RESULT: STRUCTURE VALIDATION PASSED" : "RESULT: STRUCTURE VALIDATION FAILED");
        report.AppendLine("Note: this does not replace Unity compile, Play Mode, animation, scale, collision, navigation, audio asset, or visual validation.");

        reportText = report.ToString();
        return errors == 0;
    }

    private static void ValidateNexusArt(ZodiacNexusSite site, ref int errors, StringBuilder report)
    {
        if (site == null) return;
        string[] paths =
        {
            "ZODIAC_NEXUS_ART/NEXUS_FOUNDATION",
            "ZODIAC_NEXUS_ART/NEXUS_RING_INNER",
            "ZODIAC_NEXUS_ART/NEXUS_RING_OUTER",
            "ZODIAC_NEXUS_ART/NEXUS_CROWN",
            "ZODIAC_NEXUS_ART/NEXUS_ENERGY"
        };

        foreach (string path in paths)
        {
            if (site.transform.Find(path) == null)
            {
                errors++;
                report.AppendLine("ERROR Missing Nexus " + site.SiteId + " art layer: " + path);
            }
            else
            {
                report.AppendLine("PASS  Nexus " + site.SiteId + " art layer: " + path);
            }
        }
    }

    private static void ValidateZodiacCoreArt(Transform core, ref int errors, StringBuilder report)
    {
        string[] paths =
        {
            "ZODIAC_CORE_ART/CORE_INNER_ENERGY",
            "ZODIAC_CORE_ART/CORE_SHELL_STRUCTURE",
            "ZODIAC_CORE_ART/CORE_RING_A",
            "ZODIAC_CORE_ART/CORE_RING_B",
            "ZODIAC_CORE_ART/CORE_HALO_OUTER",
            "ZODIAC_CORE_ART/CORE_CELESTIAL_SPINES"
        };

        foreach (string path in paths)
        {
            if (core.Find(path) == null)
            {
                errors++;
                report.AppendLine("ERROR Missing Zodiac Core art layer: " + path);
            }
            else
            {
                report.AppendLine("PASS  Zodiac Core art layer: " + path);
            }
        }
    }

    private static void ValidateEnvironmentIdentity(ref int errors, ref int warnings, StringBuilder report)
    {
        string[] landmarks =
        {
            "THE_ZERO_GATE",
            "THE_MEMORY_CHOIR",
            "THE_RESONANCE_CHAMBER",
            "Ghost_Platform_09",
            "CELESTIAL_NETWORK_ORBITAL_RING"
        };

        foreach (string landmark in landmarks)
        {
            if (GameObject.Find(landmark) == null)
            {
                errors++;
                report.AppendLine("ERROR Missing environment landmark: " + landmark);
            }
            else
            {
                report.AppendLine("PASS  Landmark present: " + landmark);
            }
        }

        string[] materials =
        {
            "M_DarkCeramic.mat",
            "M_LightComposite.mat",
            "M_NavyMetal.mat",
            "M_Accent_Blue.mat",
            "M_Accent_Violet.mat",
            "M_Hologram.mat",
            "M_Energy_Core.mat"
        };

        foreach (string materialName in materials)
        {
            string path = "Assets/RenkaiKurokage/Art/GeneratedMaterials/" + materialName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                warnings++;
                report.AppendLine("WARN  Missing shared material asset: " + materialName);
            }
            else
            {
                report.AppendLine("PASS  Shared material: " + materialName);
            }
        }
    }

    private static void ValidateCount<T>(string label, int expected, ref int errors, StringBuilder report) where T : Object
    {
        T[] objects = Object.FindObjectsOfType<T>(true);
        ValidateExact(label, objects.Length, expected, ref errors, report);
    }

    private static void ValidateExact(string label, int actual, int expected, ref int errors, StringBuilder report)
    {
        if (actual == expected)
        {
            report.AppendLine("PASS  " + label + ": " + actual);
        }
        else
        {
            errors++;
            report.AppendLine("ERROR " + label + ": expected " + expected + ", found " + actual);
        }
    }
}
