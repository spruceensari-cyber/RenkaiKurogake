using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageViewmodelAnimator : MonoBehaviour
    {
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private float smooth = 13f;
        [SerializeField] private float adsPositionX = -0.11f;
        [SerializeField] private float adsPositionY = 0.045f;
        [SerializeField] private float adsPositionZ = 0.08f;
        [SerializeField] private float shotKickDistance = 0.055f;
        [SerializeField] private float shotKickRotation = 2.2f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float reloadStart;
        private float reloadDuration;
        private bool visualReloading;
        private float shotKick;
        private float shotRot;
        private int lastRifleAmmo;
        private int lastPistolAmmo;

        private void Awake()
        {
            if (weapon == null) weapon = GetComponentInParent<RenkaiWeaponController>();
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;

            if (weapon != null)
            {
                lastRifleAmmo = weapon.rifleAmmo;
                lastPistolAmmo = weapon.pistolAmmo;
            }
        }

        private void Update()
        {
            if (weapon == null) return;

            DetectShotKick();
            DetectReloadVisual();
            AnimateViewmodel();
        }

        private void DetectShotKick()
        {
            bool rifleShot = weapon.rifleAmmo < lastRifleAmmo;
            bool pistolShot = weapon.pistolAmmo < lastPistolAmmo;

            if (rifleShot || pistolShot)
            {
                shotKick = shotKickDistance;
                shotRot = shotKickRotation;
            }

            lastRifleAmmo = weapon.rifleAmmo;
            lastPistolAmmo = weapon.pistolAmmo;
        }

        private void DetectReloadVisual()
        {
            if (Input.GetKeyDown(KeyCode.R) && weapon.slot != RenkaiWeaponSlot.Sword)
            {
                bool canReload = weapon.slot == RenkaiWeaponSlot.Rifle
                    ? weapon.rifleAmmo < 30 && weapon.rifleReserve > 0
                    : weapon.pistolAmmo < 12 && weapon.pistolReserve > 0;

                if (canReload)
                {
                    visualReloading = true;
                    reloadStart = Time.time;
                    reloadDuration = weapon.slot == RenkaiWeaponSlot.Rifle ? weapon.rifleReloadTime : weapon.pistolReloadTime;
                }
            }

            if (visualReloading && Time.time - reloadStart >= reloadDuration)
                visualReloading = false;
        }

        private void AnimateViewmodel()
        {
            Vector3 targetPos = baseLocalPosition;
            Quaternion targetRot = baseLocalRotation;

            bool aiming = weapon.slot != RenkaiWeaponSlot.Sword && Input.GetMouseButton(1) && !visualReloading;
            if (aiming)
            {
                targetPos += new Vector3(adsPositionX, adsPositionY, adsPositionZ);
                targetRot *= Quaternion.Euler(-1.5f, -4f, 0f);
            }

            if (visualReloading)
            {
                float t = Mathf.Clamp01((Time.time - reloadStart) / Mathf.Max(0.01f, reloadDuration));
                float arc = Mathf.Sin(t * Mathf.PI);
                targetPos += new Vector3(0.075f * arc, -0.13f * arc, -0.07f * arc);
                targetRot *= Quaternion.Euler(25f * arc, 7f * arc, -18f * arc);
                AnimateMagazine(t);
            }

            shotKick = Mathf.Lerp(shotKick, 0f, 12f * Time.deltaTime);
            shotRot = Mathf.Lerp(shotRot, 0f, 13f * Time.deltaTime);
            targetPos += new Vector3(0f, 0f, -shotKick);
            targetRot *= Quaternion.Euler(-shotRot, 0f, 0f);

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, smooth * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, smooth * Time.deltaTime);
        }

        private void AnimateMagazine(float t)
        {
            Transform mag = null;
            if (weapon.slot == RenkaiWeaponSlot.Rifle && weapon.rifleView != null)
                mag = FindDeepChild(weapon.rifleView.transform, "Magazine");
            else if (weapon.slot == RenkaiWeaponSlot.Pistol && weapon.pistolView != null)
                mag = FindDeepChild(weapon.pistolView.transform, "Magazine");

            if (mag == null) return;

            float drop;
            if (t < 0.35f) drop = Mathf.SmoothStep(0f, 1f, t / 0.35f);
            else if (t < 0.72f) drop = 1f;
            else drop = Mathf.SmoothStep(1f, 0f, (t - 0.72f) / 0.28f);

            Vector3 p = mag.localPosition;
            p.y = -0.14f - 0.32f * drop;
            p.z = 0.30f - 0.05f * drop;
            mag.localPosition = p;
            mag.localRotation = Quaternion.Euler(8f + 18f * drop, 0f, 0f);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform result = FindDeepChild(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
