using UnityEngine;

namespace Renkai.Kurokage
{
    public abstract class KurokageSkill : MonoBehaviour
    {
        [SerializeField] private float cooldown = 8f;
        [SerializeField] private int maxCharges = 1;

        private int charges;
        private float nextRecharge;

        protected virtual void Awake()
        {
            charges = Mathf.Max(1, maxCharges);
        }

        protected virtual void Update()
        {
            if (charges < maxCharges && Time.time >= nextRecharge)
            {
                charges++;
                if (charges < maxCharges)
                    nextRecharge = Time.time + cooldown;
            }
        }

        public bool TryActivate()
        {
            if (charges <= 0 || !CanActivate()) return false;
            charges--;
            if (charges < maxCharges)
                nextRecharge = Time.time + cooldown;
            Activate();
            return true;
        }

        protected virtual bool CanActivate() => true;
        protected abstract void Activate();
    }

    public sealed class KurokageSkillRunner : MonoBehaviour
    {
        [SerializeField] private KurokageSkill qSkill;
        [SerializeField] private KurokageSkill eSkill;
        [SerializeField] private KurokageSkill cSkill;
        [SerializeField] private KurokageSkill ultimateSkill;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) && qSkill != null) qSkill.TryActivate();
            if (Input.GetKeyDown(KeyCode.E) && eSkill != null) eSkill.TryActivate();
            if (Input.GetKeyDown(KeyCode.C) && cSkill != null) cSkill.TryActivate();
            if (Input.GetKeyDown(KeyCode.X) && ultimateSkill != null) ultimateSkill.TryActivate();
        }
    }
}
