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
        ValidateCount<KairiAbilityController>("Kairi ability controller", 1, ref errors, report);
        ValidateCount<KurokageBladeCombatController>("Blade combat controller", 1, ref errors, report);
        ValidateCount<KurokageEliteHUD>("Elite HUD", 1, ref errors, report);
        ValidateCount<KurokageCombatFeedbackHUD>("Combat feedback HUD", 1, ref errors, report);
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

        foreach (RenkaiRoundPlayer player in players)
        {
            if (player == null) continue;
            if (player.isHumanPlayer) humans++;
            if (player.team == RenkaiTeam.Attackers) attackers++;
            else defenders++;

            Renderer rootRenderer = player.GetComponent<Renderer>();
            if (rootRenderer != null && rootRenderer.enabled)
                visibleRootRenderers++;
        }

        ValidateExact("Human player", humans, 1, ref errors, report);
        ValidateExact("Attackers", attackers, 5, ref errors, report);
        ValidateExact("Defenders", defenders, 5, ref errors, report);

        if (visibleRootRenderers > 0)
        {
            warnings++;
            report.AppendLine("WARN  Visible root player renderers: " + visibleRootRenderers + " (capsule placeholders may still be visible)");
        }
        else
        {
            report.AppendLine("PASS  No visible root player renderers");
        }

        ZodiacNexusSite[] sites = Object.FindObjectsOfType<ZodiacNexusSite>(true);
        ValidateExact("Zodiac Nexus sites", sites.Length, 2, ref errors, report);

        GameObject core = GameObject.Find("ZODIAC_CORE");
        if (core == null)
        {
            errors++;
            report.AppendLine("ERROR ZODIAC_CORE object missing");
        }
        else
        {
            report.AppendLine("PASS  ZODIAC_CORE object present");
        }

        string[] requiredRoots =
        {
            "KUROKAGE_ELITE_HUD",
            "KUROKAGE_COMBAT_FEEDBACK_HUD",
            "KUROKAGE_MATCH_PRESENTATION_HUD",
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
        report.AppendLine("Note: this does not replace Unity compile, Play Mode, animation, scale, collision, or visual validation.");

        reportText = report.ToString();
        return errors == 0;
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
