using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

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
                ? "Meshy karakterleri kaldırıldı. Kairi, Noa, Reiha ve Mio kod tabanlı agent visual sistemiyle yeniden kuruldu."
                : "Kod tabanlı agent visual kurulumu tamamlanamadı. 5v5 player stack ve hit zone binding kontrol edilmeli.",
            ok ? "OK" : "REVIEW"
        );
    }

    public static bool InstallSilent()
    {
        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        if (players == null || players.Length == 0) return false;

        System.Array.Sort(players, ComparePlayers);

        int attackerIndex = 0;
        int defenderIndex = 0;
        bool allSucceeded = true;

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

            bool applied = KurokageProceduralAgentFactory.Build(player, archetype);
            if (!applied)
            {
                allSucceeded = false;
                Debug.LogError("Renkai code-built agent visual failed for " + player.agentName + " archetype=" + archetype);
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = players[0].gameObject;
        return allSucceeded;
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

    private static int ComparePlayers(RenkaiRoundPlayer a, RenkaiRoundPlayer b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        if (a.isHumanPlayer && !b.isHumanPlayer) return -1;
        if (!a.isHumanPlayer && b.isHumanPlayer) return 1;

        int teamCompare = a.team.CompareTo(b.team);
        if (teamCompare != 0) return teamCompare;
        return string.CompareOrdinal(a.agentName, b.agentName);
    }
}
