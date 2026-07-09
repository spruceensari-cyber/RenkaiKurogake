
using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurogake
{
    public class RenkaiHUD : MonoBehaviour
    {
        public Text timerText;
        public Text infoText;
        public float roundTime = 100f;

        private float timeLeft;

        private void Awake()
        {
            timeLeft = roundTime;
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0f) timeLeft = 0f;

            if (timerText != null)
                timerText.text = "ROUND " + Mathf.CeilToInt(timeLeft).ToString();

            if (infoText != null)
                infoText.text = "WASD Move | Mouse Aim | LMB Fire | F Plant | Shift Sprint | Kurogate Teleport";
        }
    }
}
