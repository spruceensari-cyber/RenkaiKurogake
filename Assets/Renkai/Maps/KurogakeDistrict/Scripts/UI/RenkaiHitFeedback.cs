
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurogake
{
    public class RenkaiHitFeedback : MonoBehaviour
    {
        public Text hitText;
        public float showTime = 0.12f;

        private Coroutine routine;

        public void ShowHit()
        {
            if (hitText == null) return;

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            hitText.enabled = true;
            yield return new WaitForSeconds(showTime);
            hitText.enabled = false;
        }
    }
}
