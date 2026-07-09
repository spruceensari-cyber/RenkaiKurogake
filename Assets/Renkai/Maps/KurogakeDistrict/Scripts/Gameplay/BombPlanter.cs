using UnityEngine;

namespace Renkai.Kurogake
{
    public class BombPlanter : MonoBehaviour
    {
        public KeyCode interactKey = KeyCode.F;

        private string currentSite = "";
        private RenkaiBombCore bomb;
        private RenkaiRoundPlayer player;

        private void Start()
        {
            bomb = Object.FindObjectOfType<RenkaiBombCore>();
            player = GetComponent<RenkaiRoundPlayer>();
        }

        private void Update()
        {
            if (bomb == null) return;
            if (player != null && !player.isAlive) return;

            if (Input.GetKey(interactKey))
            {
                if (player == null || player.team == RenkaiTeam.Attackers)
                {
                    if (!string.IsNullOrEmpty(currentSite))
                        bomb.TryPlant(currentSite);
                }
                else
                {
                    bomb.TryDefuse();
                }
            }
            else
            {
                bomb.CancelActions();
            }
        }

        public void EnterSite(string siteName)
        {
            currentSite = siteName;
        }

        public void ExitSite(string siteName)
        {
            if (currentSite == siteName)
                currentSite = "";
        }
    }
}
