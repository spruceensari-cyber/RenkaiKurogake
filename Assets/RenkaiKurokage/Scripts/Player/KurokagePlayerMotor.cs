using UnityEngine;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokagePlayerMotor : MonoBehaviour
    {
        [SerializeField] private Transform viewTransform;
        [SerializeField] private float walkSpeed = 5.2f;
        [SerializeField] private float sprintSpeed = 7.3f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -24f;

        private CharacterController controller;
        private Vector3 planarVelocity;
        private float verticalVelocity;

        public bool IsGrounded => controller != null && controller.enabled && controller.isGrounded;
        public float PlanarSpeed => new Vector3(planarVelocity.x, 0f, planarVelocity.z).magnitude;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (viewTransform == null && Camera.main != null)
                viewTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 forward = viewTransform != null ? Vector3.ProjectOnPlane(viewTransform.forward, Vector3.up).normalized : transform.forward;
            Vector3 right = viewTransform != null ? Vector3.ProjectOnPlane(viewTransform.right, Vector3.up).normalized : transform.right;
            Vector3 input = (forward * v + right * h).normalized;

            bool sprinting = Input.GetKey(KeyCode.LeftShift) && v > 0.1f;
            float speed = sprinting ? sprintSpeed : walkSpeed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, input * speed, acceleration * Time.deltaTime);

            if (IsGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump") && IsGrounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }
}
