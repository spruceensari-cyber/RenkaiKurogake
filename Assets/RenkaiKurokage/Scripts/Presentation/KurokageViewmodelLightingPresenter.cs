using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageViewmodelLightingPresenter : MonoBehaviour
    {
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float baseIntensity = 0.34f;
        [SerializeField] private float shotBoost = 0.46f;
        [SerializeField] private float boostDecay = 14f;

        private Light keyLight;
        private float currentBoost;

        private void Awake()
        {
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            BuildLight();
        }

        private void OnEnable()
        {
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (weapon != null) weapon.ShotFired += OnShotFired;
        }

        private void OnDisable()
        {
            if (weapon != null) weapon.ShotFired -= OnShotFired;
        }

        private void Update()
        {
            if (keyLight == null) BuildLight();
            if (keyLight == null) return;

            currentBoost = Mathf.Lerp(currentBoost, 0f, boostDecay * Time.deltaTime);
            keyLight.intensity = baseIntensity + currentBoost;

            if (weapon != null)
            {
                if (weapon.slot == RenkaiWeaponSlot.Rifle)
                    keyLight.color = new Color(0.72f, 0.84f, 1f, 1f);
                else if (weapon.slot == RenkaiWeaponSlot.Pistol)
                    keyLight.color = new Color(0.82f, 0.88f, 1f, 1f);
                else
                    keyLight.color = new Color(0.68f, 0.74f, 1f, 1f);
            }
        }

        public void ResetPresentation()
        {
            currentBoost = 0f;
            if (keyLight != null) keyLight.intensity = baseIntensity;
        }

        private void OnShotFired()
        {
            currentBoost = Mathf.Max(currentBoost, shotBoost);
        }

        private void BuildLight()
        {
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) return;

            Transform existing = playerCamera.transform.Find("KUROKAGE_VIEWMODEL_KEY_LIGHT");
            if (existing != null)
            {
                keyLight = existing.GetComponent<Light>();
                if (keyLight != null) return;
            }

            GameObject lightGo = new GameObject("KUROKAGE_VIEWMODEL_KEY_LIGHT");
            lightGo.transform.SetParent(playerCamera.transform, false);
            lightGo.transform.localPosition = new Vector3(0.28f, 0.12f, 0.18f);
            lightGo.transform.localRotation = Quaternion.Euler(22f, -18f, 0f);

            keyLight = lightGo.AddComponent<Light>();
            keyLight.type = LightType.Point;
            keyLight.color = new Color(0.72f, 0.84f, 1f, 1f);
            keyLight.intensity = baseIntensity;
            keyLight.range = 2.4f;
            keyLight.shadows = LightShadows.None;
            keyLight.renderMode = LightRenderMode.Auto;
        }
    }
}
