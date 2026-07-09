using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurogake
{
    public class RenkaiHUDController : MonoBehaviour
    {
        public Text statusText;
        public Text scoreText;
        public Text timerText;
        public Text playerHPText;
        public Text ammoText;
        public Text weaponText;
        public Text hitText;
        public Text killFeedText;
        public Text teamListText;

        private readonly Queue<string> killFeed = new Queue<string>();

        private void Update()
        {
            UpdateTeamList();
        }

        public void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }

        public void SetScore(int atk, int def, int round)
        {
            if (scoreText != null) scoreText.text = "ROUND " + round + "    ATK " + atk + " - " + def + " DEF";
        }

        public void SetTimer(string value)
        {
            if (timerText != null) timerText.text = value;
        }

        public void SetPlayerHP(int hp, int max)
        {
            if (playerHPText != null) playerHPText.text = "HP " + Mathf.Max(0, hp) + " / " + max;
        }

        public void AddKillFeed(string value)
        {
            killFeed.Enqueue(value);
            while (killFeed.Count > 5) killFeed.Dequeue();

            if (killFeedText != null)
                killFeedText.text = string.Join("\n", killFeed.ToArray());
        }

        private void UpdateTeamList()
        {
            if (teamListText == null) return;

            int atkAlive = 0;
            int defAlive = 0;
            int atkTotal = 0;
            int defTotal = 0;

            foreach (RenkaiRoundPlayer p in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (p.team == RenkaiTeam.Attackers)
                {
                    atkTotal++;
                    if (p.isAlive) atkAlive++;
                }
                else
                {
                    defTotal++;
                    if (p.isAlive) defAlive++;
                }
            }

            teamListText.text = "ATTACKERS " + atkAlive + "/" + atkTotal + "\nDEFENDERS " + defAlive + "/" + defTotal;
        }
    }
}
