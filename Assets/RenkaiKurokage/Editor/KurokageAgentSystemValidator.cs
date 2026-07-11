using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageAgentSystemValidator
{
    public static bool ValidateSilent(out string reportText)
    {
        int errors = 0;
        int humans = 0;
        int autonomous = 0;
        HashSet<KurokageAgentArchetype> uniqueAgents = new HashSet<KurokageAgentArchetype>();
        StringBuilder report = new StringBuilder();
        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);

        report.AppendLine("RENKAI TEN-AGENT SYSTEM VALIDATION");
        report.AppendLine("Catalog definitions: " + KurokageAgentCatalog.All.Count + " / expected 10");
        if (KurokageAgentCatalog.All.Count != 10) errors++;

        foreach (RenkaiRoundPlayer player in players)
        {
            if (player == null) continue;
            KurokageAgentIdentity identity = player.GetComponent<KurokageAgentIdentity>();
            if (identity == null)
            {
                errors++;
                report.AppendLine("ERROR Missing identity: " + player.name);
                continue;
            }

            uniqueAgents.Add(identity.Archetype);
            KurokageAgentDefinition definition = identity.Definition;
            if (definition == null || definition.Abilities == null || definition.Abilities.Length != 4)
            {
                errors++;
                report.AppendLine("ERROR Invalid four-slot kit: " + player.name);
            }

            if (player.isHumanPlayer)
            {
                humans++;
                Require<KurokageAgentSelectionScreen>(player, ref errors, report, "selection screen");
                Require<KurokageAgentAbilityController>(player, ref errors, report, "shared ability runtime");
                Require<KairiAbilityController>(player, ref errors, report, "Kairi premium kit");
                Require<KurokageSprayController>(player, ref errors, report, "wall spray controller");
                Require<KurokageVoiceSubtitleOverlay>(player, ref errors, report, "Japanese subtitle overlay");
                Require<KurokageJapaneseVoicePresenter>(player, ref errors, report, "Japanese voice presenter");
            }
            else
            {
                autonomous++;
                Require<RenkaiTacticalBotAI>(player, ref errors, report, "tactical AI");
                Require<KurokageBotPerception>(player, ref errors, report, "strict perception");
                Require<KurokageBotAutonomyMotor>(player, ref errors, report, "autonomous jump motor");
                Require<KurokageBotWeaponState>(player, ref errors, report, "magazine and reload state");
                Require<KurokageBotWeaponPose>(player, ref errors, report, "weapon pose driver");
                Require<KurokageAgentAbilityController>(player, ref errors, report, "shared ability runtime");
                Require<KurokageBotAgentAbilityBrain>(player, ref errors, report, "tactical ability brain");
                Require<KurokageJapaneseVoicePresenter>(player, ref errors, report, "Japanese voice presenter");
            }
        }

        report.AppendLine("Participants: " + players.Length + " / expected 10");
        report.AppendLine("Human selectors: " + humans + " / expected 1");
        report.AppendLine("Autonomous agents: " + autonomous + " / expected 9");
        report.AppendLine("Unique initial archetypes: " + uniqueAgents.Count + " / expected 10");

        if (players.Length != 10) errors++;
        if (humans != 1) errors++;
        if (autonomous != 9) errors++;
        if (uniqueAgents.Count != 10) errors++;
        if (GameObject.Find("KUROKAGE_KUROGATE_DISTRICT") == null)
        {
            errors++;
            report.AppendLine("ERROR Kurogate district presentation root missing.");
        }

        report.AppendLine(errors == 0 ? "RESULT: TEN-AGENT SYSTEM PASSED" : "RESULT: TEN-AGENT SYSTEM FAILED");
        reportText = report.ToString();
        return errors == 0;
    }

    private static void Require<T>(RenkaiRoundPlayer player, ref int errors, StringBuilder report, string label) where T : Component
    {
        if (player.GetComponent<T>() != null) return;
        errors++;
        report.AppendLine("ERROR Missing " + label + ": " + player.agentName);
    }
}
