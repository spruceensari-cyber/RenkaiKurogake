using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageMovementPresentation : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private RenkaiFPSController fps;
        [SerializeField] private float sprintFovBonus = 4.5f;
        [SerializeField] private float fovSmooth = 8f;
        [SerializeField] private float landingKick = 2.2f;
        [SerializeField] private float landingRecovery = 9f;
        [SerializeField] private float strafeLean = 1.7f;
        [SerializeField] private float leanSmooth = 10f;

        private CharacterController controller;
        private bool wasGrounded;
        private float cameraKick;
        private float roll;
        private float baseFov = 90f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (fps == null) fps = GetComponent<RenkaiFPSController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null) baseFov = playerCamera.fieldOfView;
            wasGrounded = controller != null && controller.isGrounded;
        }

        private void LateUpdate()
        {
            if (controller == null || playerCamera == null) return;

            bool grounded = controller.isGrounded;
            if (!wasGrounded && grounded && controller.velocity.y < -1.5f)
                cameraKick = landingKick;
            wasGrounded = grounded;

            bool sprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0.1f && controller.velocity.magnitude > 0.5f;
            RenkaiWeaponController weapon = GetComponent<RenkaiWeaponController>();
            bool aiming = weapon != null && weapon.IsAiming;

            float targetFov = aiming ? weapon.adsFov : baseFov + (sprinting ? sprintFovBonus : 0f);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovSmooth * Time.deltaTime);

            cameraKick = Mathf.Lerp(cameraKick, 0f, landingRecovery * Time.deltaTime);
            float horizontal = Input.GetAxisRaw("Horizontal");
            roll = Mathf.Lerp(roll, -horizontal * strafeLean, leanSmooth * Time.deltaTime);

            Vector3 euler = playerCamera.transform.localEulerAngles;
            float x = euler.x > 180f ? euler.x - 360f : euler.x;
            float y = euler.y > 180f ? euler.y - 360f : euler.y;
            playerCamera.transform.localEulerAngles = new Vector3(x + cameraKick, y, roll);
        }
    }
}
