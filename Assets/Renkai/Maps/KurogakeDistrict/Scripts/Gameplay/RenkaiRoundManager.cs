using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    public class RenkaiRoundManager : MonoBehaviour
    {
        public int attackersScore;
        public int defendersScore;
        public int roundNumber = 1;

        public float buyTime = 5f;
        public float roundTime = 95f;

        public RenkaiBombCore bomb;

        public Text statusText;
        public Text scoreText;
        public Text timerText;

        private bool roundActive;
        private float roundStartTime;
        private RenkaiHUDController hud;

        private void Start()
        {
            hud = Object.FindObjectOfType<RenkaiHUDController>();
            StartCoroutine(NewRoundRoutine());
        }

        private IEnumerator NewRoundRoutine()
        {
            roundActive = false;
            hud = Object.FindObjectOfType<RenkaiHUDController>();

            SetStatus("BUY PHASE");
            KurokageGameEvents.RaiseRoundBanner("PREPARE // ROUND " + roundNumber);
            SetScoreUI();

            KurokageVfxPool pool = Object.FindObjectOfType<KurokageVfxPool>();
            if (pool != null) pool.ClearAll();

            ZodiacCoreRuntime core = Object.FindObjectOfType<ZodiacCoreRuntime>();
            if (core != null) core.ResetObjective();

            foreach (RenkaiRoundPlayer p in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                p.ResetForRound();
                KurokageBladeCombatController blade = p.GetComponent<KurokageBladeCombatController>();
                if (blade != null) blade.ResetForRound();
            }

            for (int i = KurokageDecoyRuntime.Active.Count - 1; i >= 0; i--)
            {
                KurokageDecoyRuntime decoy = KurokageDecoyRuntime.Active[i];
                if (decoy != null) decoy.DissolveAndDestroy();
            }

            if (bomb != null)
                bomb.ResetBomb();

            yield return new WaitForSeconds(buyTime);

            roundActive = true;
            roundStartTime = Time.time;
            SetStatus("ROUND " + roundNumber + " START");
            KurokageGameEvents.RaiseRoundBanner("ROUND " + roundNumber + " // ENGAGE");
        }

        private void Update()
        {
            if (!roundActive) return;

            float timeLeft = Mathf.Max(0f, roundTime - (Time.time - roundStartTime));

            if (bomb != null && bomb.planted)
                SetTimer("BOMB " + Mathf.CeilToInt(bomb.TimeLeft()));
            else
                SetTimer("ROUND " + Mathf.CeilToInt(timeLeft));

            SetScoreUI();

            if (timeLeft <= 0f && (bomb == null || !bomb.planted))
                EndRound(RenkaiTeam.Defenders, "Time expired");
        }

        public void CheckWinConditions()
        {
            if (!roundActive) return;

            int aliveAttackers = 0;
            int aliveDefenders = 0;

            foreach (RenkaiRoundPlayer p in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (!p.isAlive) continue;
                if (p.team == RenkaiTeam.Attackers) aliveAttackers++;
                else aliveDefenders++;
            }

            if (aliveAttackers <= 0)
                EndRound(RenkaiTeam.Defenders, "Attackers eliminated");
            else if (aliveDefenders <= 0)
                EndRound(RenkaiTeam.Attackers, "Defenders eliminated");
        }

        public void EndRound(RenkaiTeam winner, string reason)
        {
            if (!roundActive) return;

            roundActive = false;
            if (winner == RenkaiTeam.Attackers) attackersScore++;
            else defendersScore++;

            SetStatus(winner + " WIN - " + reason);
            SetScoreUI();
            KurokageGameEvents.RaiseRoundEnded(winner, reason);
            KurokageGameEvents.RaiseRoundBanner((winner == RenkaiTeam.Attackers ? "ATTACKERS" : "DEFENDERS") + " // VICTORY");

            StartCoroutine(NextRoundRoutine());
        }

        private IEnumerator NextRoundRoutine()
        {
            yield return new WaitForSeconds(4f);
            roundNumber++;
            StartCoroutine(NewRoundRoutine());
        }

        public void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
            if (hud != null) hud.SetStatus(value);
            Debug.Log(value);
        }

        private void SetTimer(string value)
        {
            if (timerText != null) timerText.text = value;
            if (hud != null) hud.SetTimer(value);
        }

        private void SetScoreUI()
        {
            string value = "ROUND " + roundNumber + "    ATK " + attackersScore + " - " + defendersScore + " DEF";
            if (scoreText != null) scoreText.text = value;
            if (hud != null) hud.SetScore(attackersScore, defendersScore, roundNumber);
        }
    }
}
