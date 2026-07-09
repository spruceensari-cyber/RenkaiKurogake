using System;
using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxArmor = 50f;
        [SerializeField] private float startingArmor = 0f;
        [SerializeField, Range(0f, 1f)] private float armorAbsorption = 0.5f;

        public float Health { get; private set; }
        public float Armor { get; private set; }
        public bool IsAlive => Health > 0f;

        public event Action<float, float> Damaged;
        public event Action Died;

        private void Awake()
        {
            ResetVitals();
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            float absorbed = Mathf.Min(Armor, amount * armorAbsorption);
            Armor -= absorbed;
            Health = Mathf.Max(0f, Health - (amount - absorbed));
            Damaged?.Invoke(Health, Armor);

            if (Health <= 0f) Died?.Invoke();
        }

        public void ResetVitals()
        {
            Health = maxHealth;
            Armor = Mathf.Clamp(startingArmor, 0f, maxArmor);
        }
    }
}
