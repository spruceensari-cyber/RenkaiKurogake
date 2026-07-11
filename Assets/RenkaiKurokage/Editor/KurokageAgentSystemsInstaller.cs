using UnityEngine;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageAgentSystemsInstaller
{
    public static bool InstallSilent()
    {
        int humans = 0;
        int autonomousAgents = 0;

        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null) continue;

            KurokageAgentIdentity identity = player.GetComponent<KurokageAgentIdentity>();
            if (identity == null) identity = player.gameObject.AddComponent<KurokageAgentIdentity>();

            if (player.isHumanPlayer)
            {
                humans++;
                if (player.GetComponent<KurokageAgentAbilityController>() == null)
                    player.gameObject.AddComponent<KurokageAgentAbilityController>();
                if (player.GetComponent<KurokageAgentSelectionScreen>() == null)
                    player.gameObject.AddComponent<KurokageAgentSelectionScreen>();
                if (player.GetComponent<KairiAbilityController>() == null)
                    player.gameObject.AddComponent<KairiAbilityController>();
            }
            else
            {
                if (player.GetComponent<KurokageBotAutonomyMotor>() == null)
                    player.gameObject.AddComponent<KurokageBotAutonomyMotor>();
                if (player.GetComponent<KurokageBotWeaponState>() == null)
                    player.gameObject.AddComponent<KurokageBotWeaponState>();
                if (player.GetComponent<KurokageBotPerception>() == null)
                    player.gameObject.AddComponent<KurokageBotPerception>();
                autonomousAgents++;
            }
        }

        return humans == 1 && autonomousAgents == 9;
    }
}
