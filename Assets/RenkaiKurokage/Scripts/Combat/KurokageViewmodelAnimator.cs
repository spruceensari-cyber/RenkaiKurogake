using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageViewmodelAnimator : MonoBehaviour
    {
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private KurokageWeaponSway sway;
        [SerializeField] private float smooth = 13f;
        [SerializeField] private float adsPositionX = -0.11f;
        [SerializeField] private float adsPositionY = 0.045f;
        [SerializeField] private float adsPositionZ = 0.08f;
        [SerializeField] private float shotKickDistance = 0.055f;
        [SerializeField] private float shotKickRotation = 2.2f;
        [SerializeField] private float switchSettleSpeed = 7.5f;
        [SerializeField] private float switchDipDistance = 0.11f;
        [SerializeField] private float switchRoll = 5.5f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float shotKick;
        private float shotRot;
        private float switchBlend;
        private RenkaiWeaponSlot lastSlot;

        private Transform rifleMagazine;
        private Vector3 rifleMagazineBasePosition;
        private Quaternion rifleMagazineBaseRotation;
        private Transform pistolMagazine;
        private Vector3 pistolMagazineBasePosition;
        private Quaternion pistolMagazineBaseRotation;
        private bool magazinePosesCached;

        private void Awake()
        {
            if (weapon == null) weapon = GetComponentInParent<RenkaiWeaponController>();
            if (sway == null) sway = GetComponent<KurokageWeaponSway>();
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            if (weapon != null) lastSlot = weapon.slot;
            CacheMagazinePoses();
        }

        private void OnEnable()
        {
            if (weapon == null) weapon = GetComponentInParent<RenkaiWeaponController>();
            if (weapon != null) weapon.ShotFired += OnShotFired;
        }

        private void OnDisable()
        {
            if (weapon != null) weapon.ShotFired -= OnShotFired;
        }

        private void LateUpdate()
        {
            if (weapon == null) return;
            if (!magazinePosesCached) CacheMagazinePoses();
            DetectWeaponSwitch();
            AnimateViewmodel();
        }

        private void OnShotFired()
        {
            shotKick = shotKickDistance;
            shotRot = shotKickRotation;
        }

        private void DetectWeaponSwitch()
        {
            if (weapon.slot == lastSlot) return;
            RestoreMagazine(rifleMagazine, rifleMagazineBasePosition, rifleMagazineBaseRotation);
            RestoreMagazine(pistolMagazine, pistolMagazineBasePosition, pistolMagazineBaseRotation);
            lastSlot = weapon.slot;
            switchBlend = 1f;
        }

        private void AnimateViewmodel()
        {
            Vector3 targetPos = baseLocalPosition;
            Quaternion targetRot = baseLocalRotation;

            if (sway != null)
            {
                targetPos += sway.PositionOffset;
                targetRot *= sway.RotationOffset;
            }

            if (weapon.IsAiming && !weapon.IsReloading)
            {
                targetPos += new Vector3(adsPositionX, adsPositionY, adsPositionZ);
                targetRot *= Quaternion.Euler(-1.5f, -4f, 0f);
            }

            if (weapon.IsReloading)
            {
                float t = weapon.ReloadNormalized;
                float arc = Mathf.Sin(t * Mathf.PI);
                targetPos += new Vector3(0.075f * arc, -0.13f * arc, -0.07f * arc);
                targetRot *= Quaternion.Euler(25f * arc, 7f * arc, -18f * arc);
                AnimateMagazine(t);
            }
            else
            {
                ResetMagazinePose();
            }

            switchBlend = Mathf.MoveTowards(switchBlend, 0f, switchSettleSpeed * Time.deltaTime);
            if (switchBlend > 0.001f)
            {
                float shaped = Mathf.Sin(switchBlend * Mathf.PI * 0.5f);
                targetPos += new Vector3(0.025f * shaped, -switchDipDistance * shaped, -0.035f * shaped);
                float signedRoll = weapon.slot == RenkaiWeaponSlot.Sword ? -switchRoll : switchRoll;
                targetRot *= Quaternion.Euler(5f * shaped, 0f, signedRoll * shaped);
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
            Transform mag = GetActiveMagazine();
            if (mag == null) return;

            Vector3 basePosition;
            Quaternion baseRotation;
            GetActiveMagazineBasePose(out basePosition, out baseRotation);

            float drop;
            if (t < 0.35f) drop = Mathf.SmoothStep(0f, 1f, t / 0.35f);
            else if (t < 0.72f) drop = 1f;
            else drop = Mathf.SmoothStep(1f, 0f, (t - 0.72f) / 0.28f);

            Vector3 offset = new Vector3(0f, -0.32f * drop, -0.05f * drop);
            mag.localPosition = basePosition + offset;
            mag.localRotation = baseRotation * Quaternion.Euler(18f * drop, 0f, 0f);
        }

        private void ResetMagazinePose()
        {
            Transform mag = GetActiveMagazine();
            if (mag == null) return;

            Vector3 basePosition;
            Quaternion baseRotation;
            GetActiveMagazineBasePose(out basePosition, out baseRotation);
            RestoreMagazine(mag, basePosition, baseRotation);
        }

        private void CacheMagazinePoses()
        {
            if (weapon == null) return;

            rifleMagazine = weapon.rifleView != null ? FindDeepChild(weapon.rifleView.transform, "Magazine") : null;
            pistolMagazine = weapon.pistolView != null ? FindDeepChild(weapon.pistolView.transform, "Magazine") : null;

            if (rifleMagazine != null)
            {
                rifleMagazineBasePosition = rifleMagazine.localPosition;
                rifleMagazineBaseRotation = rifleMagazine.localRotation;
            }

            if (pistolMagazine != null)
            {
                pistolMagazineBasePosition = pistolMagazine.localPosition;
                pistolMagazineBaseRotation = pistolMagazine.localRotation;
            }

            magazinePosesCached = rifleMagazine != null || pistolMagazine != null;
        }

        private void GetActiveMagazineBasePose(out Vector3 position, out Quaternion rotation)
        {
            if (weapon.slot == RenkaiWeaponSlot.Pistol)
            {
                position = pistolMagazineBasePosition;
                rotation = pistolMagazineBaseRotation;
                return;
            }

            position = rifleMagazineBasePosition;
            rotation = rifleMagazineBaseRotation;
        }

        private Transform GetActiveMagazine()
        {
            if (weapon.slot == RenkaiWeaponSlot.Rifle) return rifleMagazine;
            if (weapon.slot == RenkaiWeaponSlot.Pistol) return pistolMagazine;
            return null;
        }

        private static void RestoreMagazine(Transform magazine, Vector3 position, Quaternion rotation)
        {
            if (magazine == null) return;
            magazine.localPosition = position;
            magazine.localRotation = rotation;
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
