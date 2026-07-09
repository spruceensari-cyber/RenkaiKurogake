using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageAdsController : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private Vector3 hipLocalPosition;
        [SerializeField] private Vector3 adsLocalPosition;
        [SerializeField] private float hipFov = 90f;
        [SerializeField] private float adsFov = 68f;
        [SerializeField] private float transitionSpeed = 14f;

        public bool IsAiming { get; private set; }

        private void Awake()
        {
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (weaponRoot != null) hipLocalPosition = weaponRoot.localPosition;
        }

        private void Update()
        {
            IsAiming = Input.GetMouseButton(1);

            if (playerCamera != null)
            {
                float targetFov = IsAiming ? adsFov : hipFov;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, transitionSpeed * Time.deltaTime);
            }

            if (weaponRoot != null)
            {
                Vector3 targetPosition = IsAiming ? adsLocalPosition : hipLocalPosition;
                weaponRoot.localPosition = Vector3.Lerp(weaponRoot.localPosition, targetPosition, transitionSpeed * Time.deltaTime);
            }
        }
    }
}
