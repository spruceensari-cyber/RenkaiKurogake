using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageAmmoState : MonoBehaviour
    {
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveCapacity = 90;

        public int Magazine { get; private set; }
        public int Reserve { get; private set; }

        private void Awake()
        {
            ResetAmmo();
        }

        public bool TryConsumeRound()
        {
            if (Magazine <= 0) return false;
            Magazine--;
            return true;
        }

        public int RefillMagazine()
        {
            int needed = magazineSize - Magazine;
            int moved = Mathf.Min(needed, Reserve);
            Magazine += moved;
            Reserve -= moved;
            return moved;
        }

        public void ResetAmmo()
        {
            Magazine = magazineSize;
            Reserve = reserveCapacity;
        }
    }
}
