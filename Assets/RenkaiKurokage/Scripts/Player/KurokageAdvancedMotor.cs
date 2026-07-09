using UnityEngine;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageAdvancedMotor : MonoBehaviour
    {
        [SerializeField] private Transform viewTransform;
        [SerializeField] private float walkSpeed = 5.2f;
        [SerializeField] private float sprintSpeed = 7.3f;
        [SerializeField] private float silentSpeed = 2.8f;
        [SerializeField] private float crouchSpeed = 3.3f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float slideSpeed = 10f;
        [SerializeField] private float slideDuration = 0.65f;
        [SerializeField] private float slideCooldown = 0.8f;

        private CharacterController controller;
        private Vector3 planarVelocity;
        private float verticalVelocity;
        private float slideEnd;
        private float nextSlide;
        private Vector3 slideDirection;

        public bool IsGrounded => controller != null && controller.enabled && controller.isGrounded;
        public bool IsSliding => Time.time < slideEnd;
        public float PlanarSpeed => new Vector3(planarVelocity.x, 0f, planarVelocity.z).magnitude;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (viewTransform == null && Camera.main != null) viewTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 forward = viewTransform != null ? Vector3.ProjectOnPlane(viewTransform.forward, Vector3.up).normalized : transform.forward;
            Vector3 right = viewTransform != null ? Vector3.ProjectOnPlane(viewTransform.right, Vector3.up).normalized : transform.right;
            Vector3 input = (forward * v + right * h).normalized;

            bool crouch = Input.GetKey(KeyCode.LeftControl);
            bool silent = Input.GetKey(KeyCode.LeftAlt);
            bool sprint = Input.GetKey(KeyCode.LeftShift) && v > 0.1f && !crouch && !silent;

            if (Input.GetKeyDown(KeyCode.LeftControl) && sprint && IsGrounded && Time.time >= nextSlide)
            {
                slideDirection = input.sqrMagnitude > 0.01f ? input : forward;
                slideEnd = Time.time + slideDuration;
                nextSlide = Time.time + slideCooldown;
            }

            float speed = silent ? silentSpeed : crouch ? crouchSpeed : sprint ? sprintSpeed : walkSpeed;
            Vector3 target = IsSliding ? slideDirection * slideSpeed : input * speed;
            float rate = target.sqrMagnitude > planarVelocity.sqrMagnitude ? acceleration : deceleration;
            planarVelocity = Vector3.MoveTowards(planarVelocity, target, rate * Time.deltaTime);

            if (IsGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (Input.GetButtonDown("Jump") && IsGrounded && !IsSliding) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }
}
