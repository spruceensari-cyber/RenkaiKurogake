using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageMovementPresentation : MonoBehaviour
    {
        [SerializeField] private RenkaiFPSController fps;
        [SerializeField] private float sprintFovBonus = 4.5f;
        [SerializeField] private float landingKick = 2.2f;
        [SerializeField] private float landingRecovery = 9f;
        [SerializeField] private float strafeLean = 1.7f;
        [SerializeField] private float leanSmooth = 10f;

        private CharacterController controller;
        private RenkaiWeaponController weapon;
        private bool wasGrounded;
        private float cameraKick;
        private float roll;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (fps == null) fps = GetComponent<RenkaiFPSController>();
            weapon = GetComponent<RenkaiWeaponController>();
            wasGrounded = controller != null && controller.isGrounded;
        }

        private void LateUpdate()
        {
            if (controller == null || fps == null) return;

            bool grounded = controller.isGrounded;
            if (!wasGrounded && grounded && controller.velocity.y < -1.5f)
                cameraKick = landingKick;
            wasGrounded = grounded;

            bool aiming = weapon != null && weapon.IsAiming;
            bool sprinting = !aiming && Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0.1f && controller.velocity.magnitude > 0.5f;

            cameraKick = Mathf.Lerp(cameraKick, 0f, landingRecovery * Time.deltaTime);
            float horizontal = Input.GetAxisRaw("Horizontal");
            roll = Mathf.Lerp(roll, -horizontal * strafeLean, leanSmooth * Time.deltaTime);

            fps.SetPresentationAdditives(cameraKick, roll);
            fps.SetSprintFovBonus(sprinting ? sprintFovBonus : 0f);
        }

        private void OnDisable()
        {
            if (fps == null) return;
            fps.SetPresentationAdditives(0f, 0f);
            fps.SetSprintFovBonus(0f);
        }
    }
}
