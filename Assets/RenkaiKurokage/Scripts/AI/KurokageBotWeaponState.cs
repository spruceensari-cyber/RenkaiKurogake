using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageBotWeaponState : MonoBehaviour
    {
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveAmmo = 90;
        [SerializeField] private float reloadDuration = 2.05f;

        public int MagazineAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }
        public bool IsReloading { get; private set; }
        public float Reload01 => IsReloading && reloadDuration > 0f
            ? Mathf.Clamp01((Time.time - reloadStartedAt) / reloadDuration)
            : 0f;

        private float reloadStartedAt;

        private void Awake()
        {
            ResetState();
        }

        private void OnEnable()
        {
            if (MagazineAmmo <= 0 && ReserveAmmo <= 0) ResetState();
        }

        private void Update()
        {
            if (!IsReloading) return;
            if (Time.time - reloadStartedAt < reloadDuration) return;
            CompleteReload();
        }

        public bool TryConsumeRound()
        {
            if (IsReloading) return false;
            if (MagazineAmmo <= 0)
            {
                BeginReload();
                return false;
            }

            MagazineAmmo--;
            if (MagazineAmmo <= 0 && ReserveAmmo > 0)
                BeginReload();
            return true;
        }

        public bool BeginReload()
        {
            if (IsReloading || ReserveAmmo <= 0 || MagazineAmmo >= magazineSize) return false;
            IsReloading = true;
            reloadStartedAt = Time.time;
            return true;
        }

        public void ResetState()
        {
            MagazineAmmo = magazineSize;
            ReserveAmmo = reserveAmmo;
            IsReloading = false;
            reloadStartedAt = 0f;
        }

        private void CompleteReload()
        {
            int needed = Mathf.Max(0, magazineSize - MagazineAmmo);
            int loaded = Mathf.Min(needed, ReserveAmmo);
            MagazineAmmo += loaded;
            ReserveAmmo -= loaded;
            IsReloading = false;
        }
    }
}
