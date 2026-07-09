
using UnityEngine;

namespace Renkai.Kurogake
{
    [RequireComponent(typeof(CharacterController))]
    public class RenkaiFPSController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 6f;
        public float sprintSpeed = 9f;
        public float crouchSpeed = 3.2f;
        public float jumpHeight = 1.1f;
        public float gravity = -24f;

        [Header("Crouch")]
        public KeyCode crouchKey = KeyCode.C;
        public bool allowLeftControlCrouch = true;
        public float standingHeight = 1.8f;
        public float crouchingHeight = 1.05f;
        public float crouchCameraY = 0.95f;
        public float standingCameraY = 1.65f;
        public float crouchSmooth = 12f;

        [Header("Look")]
        public Camera playerCamera;
        public float mouseSensitivity = 2.2f;
        public float minPitch = -82f;
        public float maxPitch = 82f;

        [Header("Respawn / Safety")]
        public Transform respawnPoint;
        public float killY = -12f;
        public KeyCode manualRespawnKey = KeyCode.R;

        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;
        private bool isCrouching;
        private Vector3 fallbackSpawnPosition;
        private Quaternion fallbackSpawnRotation;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();

            standingHeight = controller.height;
            fallbackSpawnPosition = transform.position;
            fallbackSpawnRotation = transform.rotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            Look();
            HandleCrouch();
            Move();
            SafetyRespawnCheck();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (playerCamera != null)
                playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        private void HandleCrouch()
        {
            bool crouchHeld = Input.GetKey(crouchKey) || (allowLeftControlCrouch && Input.GetKey(KeyCode.LeftControl));
            isCrouching = crouchHeld;

            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            float targetCenterY = targetHeight * 0.5f;

            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSmooth);
            controller.center = Vector3.Lerp(controller.center, new Vector3(0f, targetCenterY, 0f), Time.deltaTime * crouchSmooth);

            if (playerCamera != null)
            {
                float targetCamY = isCrouching ? crouchCameraY : standingCameraY;
                Vector3 camPos = playerCamera.transform.localPosition;
                camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchSmooth);
                playerCamera.transform.localPosition = camPos;
            }
        }

        private void Move()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 direction = (transform.right * x + transform.forward * z).normalized;

            float speed = walkSpeed;
            if (isCrouching) speed = crouchSpeed;
            else if (Input.GetKey(KeyCode.LeftShift)) speed = sprintSpeed;

            Vector3 velocity = direction * speed;

            if (controller.isGrounded)
            {
                verticalVelocity = -1f;
                if (Input.GetButtonDown("Jump") && !isCrouching)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void SafetyRespawnCheck()
        {
            if (transform.position.y < killY || Input.GetKeyDown(manualRespawnKey))
            {
                Respawn();
            }
        }

        public void Respawn()
        {
            controller.enabled = false;

            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
                transform.rotation = respawnPoint.rotation;
            }
            else
            {
                transform.position = fallbackSpawnPosition;
                transform.rotation = fallbackSpawnRotation;
            }

            verticalVelocity = 0f;
            controller.enabled = true;

            Debug.Log("Renkai player respawned.");
        }
    }
}
