using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageFpsCamera : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float sensitivity = 2.2f;
        [SerializeField] private float normalFov = 90f;
        [SerializeField] private float sprintFov = 96f;
        private float pitch;

        private void Awake()
        {
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            float x = Input.GetAxis("Mouse X") * sensitivity;
            float y = Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch - y, -85f, 85f);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            if (playerBody != null) playerBody.Rotate(Vector3.up * x, Space.World);

            if (playerCamera != null)
            {
                bool sprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0.1f;
                float target = sprinting ? sprintFov : normalFov;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, target, 8f * Time.deltaTime);
            }
        }
    }
}
