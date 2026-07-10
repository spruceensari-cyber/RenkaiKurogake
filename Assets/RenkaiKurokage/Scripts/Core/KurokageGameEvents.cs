using System;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public static class KurokageGameEvents
    {
        public static event Action<string, string> KillFeed;
        public static event Action<string> RoundBanner;
        public static event Action<RenkaiTeam, string> RoundEnded;

        public static void RaiseKillFeed(string killer, string victim)
        {
            KillFeed?.Invoke(killer, victim);
        }

        public static void RaiseRoundBanner(string message)
        {
            RoundBanner?.Invoke(message);
        }

        public static void RaiseRoundEnded(RenkaiTeam winner, string reason)
        {
            RoundEnded?.Invoke(winner, reason);
        }
    }
}
