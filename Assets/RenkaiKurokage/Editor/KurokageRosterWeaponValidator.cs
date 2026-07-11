using System.Text;
using UnityEngine;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageRosterWeaponValidator
{
    public static bool ValidateSilent(out string reportText)
    {
        int errors = 0;
        int humans = 0;
        int alliedBots = 0;
        int enemyBots = 0;
        int armedBots = 0;
        int botWeaponPoseCount = 0;
        RenkaiTeam humanTeam = RenkaiTeam.Attackers;
        RenkaiRoundPlayer human = null;

        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        foreach (RenkaiRoundPlayer player in players)
        {
            if (player == null) continue;
            if (player.isHumanPlayer)
            {
                humans++;
                human = player;
                humanTeam = player.team;
            }
        }

        foreach (RenkaiRoundPlayer player in players)
        {
            if (player == null || player.isHumanPlayer) continue;
            if (player.team == humanTeam) alliedBots++;
            else enemyBots++;

            Transform visual = player.transform.Find("AGENT_VISUAL");
            Transform weapon = visual != null ? FindDeep(visual, "BOT_WORLD_WEAPON") : null;
            Transform muzzle = weapon != null ? weapon.Find("MuzzleSocket") : null;
            RenkaiTacticalBotAI ai = player.GetComponent<RenkaiTacticalBotAI>();
            KurokageBotWeaponPose pose = player.GetComponent<KurokageBotWeaponPose>();

            if (weapon != null && muzzle != null && ai != null && ai.muzzle == muzzle)
                armedBots++;
            else
                errors++;

            if (pose != null) botWeaponPoseCount++;
            else errors++;
        }

        StringBuilder report = new StringBuilder();
        report.AppendLine("RENKAI ROSTER AND HELD-WEAPON VALIDATION");
        report.AppendLine("Human players: " + humans + " / expected 1");
        report.AppendLine("Allied bots: " + alliedBots + " / expected 4");
        report.AppendLine("Enemy bots: " + enemyBots + " / expected 5");
        report.AppendLine("Armed tactical bots: " + armedBots + " / expected 9");
        report.AppendLine("Bot weapon pose drivers: " + botWeaponPoseCount + " / expected 9");

        if (humans != 1) errors++;
        if (alliedBots != 4) errors++;
        if (enemyBots != 5) errors++;
        if (players.Length != 10) errors++;
        if (armedBots != 9) errors++;
        if (botWeaponPoseCount != 9) errors++;

        if (human == null || human.GetComponent<RenkaiWeaponController>() == null)
        {
            errors++;
            report.AppendLine("ERROR Human player weapon controller missing.");
        }

        report.AppendLine(errors == 0 ? "RESULT: ROSTER/WEAPON PASSED" : "RESULT: ROSTER/WEAPON FAILED");
        reportText = report.ToString();
        return errors == 0;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            if (transform.name == name) return transform;
        return null;
    }
}
