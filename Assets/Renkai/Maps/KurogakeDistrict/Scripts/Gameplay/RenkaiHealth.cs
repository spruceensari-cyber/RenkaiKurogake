using UnityEngine;

namespace Renkai.Kurogake
{
    public class RenkaiHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        public bool isTargetDummy = false;

        private float currentHealth;
        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Awake()
        {
            currentHealth = maxHealth;
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        public void TakeDamage(float damage)
        {
            RenkaiRoundPlayer roundPlayer = GetComponentInParent<RenkaiRoundPlayer>();
            if (roundPlayer != null)
            {
                roundPlayer.TakeDamage(damage);
                return;
            }

            currentHealth -= damage;
            Debug.Log(name + " took " + damage + " damage. HP: " + currentHealth);

            if (currentHealth <= 0f)
            {
                if (isTargetDummy)
                {
                    currentHealth = maxHealth;
                    transform.position = startPosition;
                    transform.rotation = startRotation;
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
