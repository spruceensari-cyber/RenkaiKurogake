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
                ? "Kod tabanlı agent roster, PBR yüzey detayları, reflection capture ve collision integrity katmanı kuruldu."
                : "Kurulum tamamlanamadı. Console ve production validation raporunu kontrol et.",
            ok ? "OK" : "REVIEW"
        );
    }

    public static bool InstallSilent()
    {
        RenkaiRoundPlayer[] players = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
        if (players == null || players.Length == 0) return false;

        System.Array.Sort(players, ComparePlayers);

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

            if (!built)
            {
                allAgentsOk = false;
                Debug.LogError("Renkai code-built agent visual failed for " + player.agentName + " archetype=" + archetype);
            }
        }

        bool collisionOk = KurokageCollisionIntegrityPass.ApplySilent();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = players[0].gameObject;
        return allAgentsOk && surfaceOk && qualityOk && collisionOk;
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

    private static int ComparePlayers(RenkaiRoundPlayer a, RenkaiRoundPlayer b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        if (a.isHumanPlayer != b.isHumanPlayer) return a.isHumanPlayer ? -1 : 1;
        int teamCompare = a.team.CompareTo(b.team);
        return teamCompare != 0 ? teamCompare : string.CompareOrdinal(a.agentName, b.agentName);
    }
}
