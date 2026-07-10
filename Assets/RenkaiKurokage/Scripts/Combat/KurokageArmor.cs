using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageArmor : MonoBehaviour
    {
        [SerializeField] private float maxArmor = 50f;
        [SerializeField] private float currentArmor = 50f;
        [SerializeField, Range(0f, 1f)] private float absorptionRatio = 0.65f;

        public float MaxArmor => maxArmor;
        public float CurrentArmor => currentArmor;
        public float Armor01 => maxArmor > 0f ? Mathf.Clamp01(currentArmor / maxArmor) : 0f;

        public float AbsorbDamage(float incomingDamage)
        {
            incomingDamage = Mathf.Max(0f, incomingDamage);
            if (incomingDamage <= 0f || currentArmor <= 0f)
                return incomingDamage;

            float absorbTarget = incomingDamage * absorptionRatio;
            float absorbed = Mathf.Min(currentArmor, absorbTarget);
            currentArmor -= absorbed;
            return Mathf.Max(0f, incomingDamage - absorbed);
        }

        public void ResetArmor()
        {
            currentArmor = maxArmor;
        }

        public void SetArmor(float value)
        {
            currentArmor = Mathf.Clamp(value, 0f, maxArmor);
        }
    }
}
