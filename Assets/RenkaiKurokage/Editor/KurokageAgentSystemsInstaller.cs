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
            if (player.GetComponent<AudioSource>() == null)
                player.gameObject.AddComponent<AudioSource>();
            if (player.GetComponent<KurokageJapaneseVoicePresenter>() == null)
                player.gameObject.AddComponent<KurokageJapaneseVoicePresenter>();

            if (player.isHumanPlayer)
            {
                humans++;
                if (player.GetComponent<KurokageAgentAbilityController>() == null)
                    player.gameObject.AddComponent<KurokageAgentAbilityController>();
                if (player.GetComponent<KurokageAgentSelectionScreen>() == null)
                    player.gameObject.AddComponent<KurokageAgentSelectionScreen>();
                if (player.GetComponent<KairiAbilityController>() == null)
                    player.gameObject.AddComponent<KairiAbilityController>();
                if (player.GetComponent<KurokageSprayController>() == null)
                    player.gameObject.AddComponent<KurokageSprayController>();
                if (player.GetComponent<KurokageVoiceSubtitleOverlay>() == null)
                    player.gameObject.AddComponent<KurokageVoiceSubtitleOverlay>();
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

        bool kurogateIdentityOk = KurokageKurogateDistrictPass.ApplySilent();
        return humans == 1 && autonomousAgents == 9 && kurogateIdentityOk;
    }
}
