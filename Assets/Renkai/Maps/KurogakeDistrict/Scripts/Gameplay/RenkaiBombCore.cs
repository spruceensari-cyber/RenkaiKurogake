using UnityEngine;

namespace Renkai.Kurogake
{
    public class RenkaiBombCore : MonoBehaviour
    {
        public bool planted;
        public string site = "";
        public float plantTime = 4f;
        public float defuseTime = 5f;
        public float explodeTime = 35f;

        private float plantProgress;
        private float defuseProgress;
        private float plantedAt;

        public void ResetBomb()
        {
            planted = false;
            site = "";
            plantProgress = 0f;
            defuseProgress = 0f;
            plantedAt = 0f;
            gameObject.SetActive(false);
        }

        public void TryPlant(string siteName)
        {
            if (planted) return;

            plantProgress += Time.deltaTime;

            RenkaiHUDController hud = Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null) hud.SetStatus("PLANTING " + siteName + " " + plantProgress.ToString("0.0") + "/" + plantTime.ToString("0.0"));

            if (plantProgress >= plantTime)
            {
                planted = true;
                site = siteName;
                plantedAt = Time.time;
                gameObject.SetActive(true);

                if (hud != null) hud.SetStatus("BOMB PLANTED AT " + site);

                Debug.Log("BOMB PLANTED: " + site);
            }
        }

        public void TryDefuse()
        {
            if (!planted) return;

            defuseProgress += Time.deltaTime;

            RenkaiHUDController hud = Object.FindObjectOfType<RenkaiHUDController>();
            if (hud != null) hud.SetStatus("DEFUSING " + defuseProgress.ToString("0.0") + "/" + defuseTime.ToString("0.0"));

            if (defuseProgress >= defuseTime)
            {
                RenkaiRoundManager manager = Object.FindObjectOfType<RenkaiRoundManager>();
                if (manager != null) manager.EndRound(RenkaiTeam.Defenders, "Bomb defused");
            }
        }

        public void CancelActions()
        {
            if (!planted) plantProgress = 0f;
            defuseProgress = 0f;
        }

        private void Update()
        {
            if (planted && Time.time - plantedAt >= explodeTime)
            {
                RenkaiRoundManager manager = Object.FindObjectOfType<RenkaiRoundManager>();
                if (manager != null) manager.EndRound(RenkaiTeam.Attackers, "Bomb exploded");
            }
        }

        public float TimeLeft()
        {
            if (!planted) return 0f;
            return Mathf.Max(0f, explodeTime - (Time.time - plantedAt));
        }
    }
}
