using UnityEngine;

namespace Renkai.Kurogake
{
    [RequireComponent(typeof(CharacterController))]
    public class RenkaiFPSController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5.6f;
        public float sprintSpeed = 7.4f;
        public float crouchSpeed = 3.1f;
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

        [Header("Camera FOV")]
        public float baseFov = 90f;
        public float fovSmooth = 10f;

        [Header("Recoil Layer")]
        public float recoilReturnSpeed = 11f;
        public float recoilFollowSpeed = 22f;

        [Header("Ability Camera Layer")]
        public float abilityImpulseReturnSpeed = 12f;
        public float abilityFovReturnSpeed = 9f;

        [Header("Respawn / Safety")]
        public Transform respawnPoint;
        public float killY = -12f;
        public bool allowManualRespawn = false;
        public KeyCode manualRespawnKey = KeyCode.F9;

        public bool IsCrouching => isCrouching;
        public float PlanarSpeed => controller == null ? 0f : new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;
        private bool isCrouching;
        private Vector3 fallbackSpawnPosition;
        private Quaternion fallbackSpawnRotation;
        private Vector2 recoilTarget;
        private Vector2 recoilCurrent;
        private float presentationPitch;
        private float presentationRoll;
        private float requestedAdsFov = -1f;
        private float requestedSprintFovBonus;
        private Vector2 abilityRotationImpulse;
        private float abilityRollImpulse;
        private float abilityFovImpulse;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();

            standingHeight = controller.height;
            fallbackSpawnPosition = transform.position;
            fallbackSpawnRotation = transform.rotation;

            if (playerCamera != null)
            {
                baseFov = playerCamera.fieldOfView > 1f ? playerCamera.fieldOfView : baseFov;
                playerCamera.fieldOfView = baseFov;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;

            UpdateAbilityImpulses();
            Look();
            HandleCrouch();
            Move();
            UpdateFov();
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
            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);

            recoilCurrent = Vector2.Lerp(recoilCurrent, recoilTarget, recoilFollowSpeed * Time.deltaTime);
            recoilTarget = Vector2.Lerp(recoilTarget, Vector2.zero, recoilReturnSpeed * Time.deltaTime);

            if (playerCamera != null)
            {
                playerCamera.transform.localEulerAngles = new Vector3(
                    Mathf.Clamp(pitch - recoilCurrent.y + presentationPitch + abilityRotationImpulse.x, minPitch - 8f, maxPitch + 8f),
                    recoilCurrent.x + abilityRotationImpulse.y,
                    presentationRoll + abilityRollImpulse
                );
            }
        }

        private void UpdateFov()
        {
            if (playerCamera == null) return;
            float target = requestedAdsFov > 1f ? requestedAdsFov : baseFov + requestedSprintFovBonus;
            target += abilityFovImpulse;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, target, fovSmooth * Time.deltaTime);
        }

        private void UpdateAbilityImpulses()
        {
            abilityRotationImpulse = Vector2.Lerp(abilityRotationImpulse, Vector2.zero, abilityImpulseReturnSpeed * Time.deltaTime);
            abilityRollImpulse = Mathf.Lerp(abilityRollImpulse, 0f, abilityImpulseReturnSpeed * Time.deltaTime);
            abilityFovImpulse = Mathf.Lerp(abilityFovImpulse, 0f, abilityFovReturnSpeed * Time.deltaTime);
        }

        public void AddRecoil(float pitchKick, float yawKick)
        {
            recoilTarget += new Vector2(yawKick, pitchKick);
        }

        public void AddAbilityCameraImpulse(float pitchKick, float yawKick, float rollKick, float fovKick)
        {
            abilityRotationImpulse += new Vector2(pitchKick, yawKick);
            abilityRollImpulse += rollKick;
            abilityFovImpulse = Mathf.Max(abilityFovImpulse, fovKick);
        }

        public void SetPresentationAdditives(float pitchOffset, float rollOffset)
        {
            presentationPitch = pitchOffset;
            presentationRoll = rollOffset;
        }

        public void SetAdsFovRequest(bool active, float adsFov)
        {
            requestedAdsFov = active ? adsFov : -1f;
        }

        public void SetSprintFovBonus(float bonus)
        {
            requestedSprintFovBonus = Mathf.Max(0f, bonus);
        }

        private void HandleCrouch()
        {
            bool crouchHeld = Input.GetKey(crouchKey) ||
                              (allowLeftControlCrouch && Input.GetKey(KeyCode.LeftControl));

            isCrouching = crouchHeld;

            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            float targetCenterY = targetHeight * 0.5f;

            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSmooth);
            controller.center = Vector3.Lerp(
                controller.center,
                new Vector3(0f, targetCenterY, 0f),
                Time.deltaTime * crouchSmooth
            );

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
            else if (Input.GetKey(KeyCode.LeftShift) && z > 0.1f) speed = sprintSpeed;

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
            bool fellOutOfMap = transform.position.y < killY;
            bool manualRequested = allowManualRespawn && Input.GetKeyDown(manualRespawnKey);

            if (fellOutOfMap || manualRequested)
                Respawn();
        }

        public void Respawn()
        {
            if (controller == null) return;

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
            recoilTarget = Vector2.zero;
            recoilCurrent = Vector2.zero;
            presentationPitch = 0f;
            presentationRoll = 0f;
            requestedAdsFov = -1f;
            requestedSprintFovBonus = 0f;
            abilityRotationImpulse = Vector2.zero;
            abilityRollImpulse = 0f;
            abilityFovImpulse = 0f;
            controller.enabled = true;

            Debug.Log("Renkai player respawned.");
        }
    }
}
