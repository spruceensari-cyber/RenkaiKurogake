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

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float shotKick;
        private float shotRot;

        private void Awake()
        {
            if (weapon == null) weapon = GetComponentInParent<RenkaiWeaponController>();
            if (sway == null) sway = GetComponent<KurokageWeaponSway>();
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
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
            AnimateViewmodel();
        }

        private void OnShotFired()
        {
            shotKick = shotKickDistance;
            shotRot = shotKickRotation;
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

        private void ResetMagazinePose()
        {
            Transform mag = GetActiveMagazine();
            if (mag == null) return;
            mag.localPosition = new Vector3(0f, -0.14f, 0.30f);
            mag.localRotation = Quaternion.Euler(8f, 0f, 0f);
        }

        private Transform GetActiveMagazine()
        {
            if (weapon.slot == RenkaiWeaponSlot.Rifle && weapon.rifleView != null)
                return FindDeepChild(weapon.rifleView.transform, "Magazine");
            if (weapon.slot == RenkaiWeaponSlot.Pistol && weapon.pistolView != null)
                return FindDeepChild(weapon.pistolView.transform, "Magazine");
            return null;
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
