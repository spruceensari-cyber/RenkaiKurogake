using UnityEngine;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RenkaiFPSController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5.6f;
        public float sprintSpeed = 7.4f;
        public float crouchSpeed = 3.1f;
        public float acceleration = 26f;
        public float deceleration = 34f;
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
        public float normalFov = 90f;
        public float sprintFov = 96f;
        public float aimFov = 72f;
        public float fovResponse = 14f;

        [Header("Recoil")]
        public float recoilReturnSpeed = 12f;
        public float recoilSnappiness = 20f;

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
        public bool allowManualRespawn;
        public KeyCode manualRespawnKey = KeyCode.F9;

        public bool IsCrouching => isCrouching;
        public bool IsSprinting { get; private set; }
        public float PlanarSpeed => controller == null ? 0f : new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        private CharacterController controller;
<<<<<<< Updated upstream
        private KairiAbilityController abilityController;
=======
        private Vector3 planarVelocity;
>>>>>>> Stashed changes
        private float verticalVelocity;
        private float pitch;
        private float cameraBaseY;
        private bool isCrouching;
        private bool isAiming;
        private Vector2 recoilCurrent;
        private Vector2 recoilTarget;
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

        public bool IsGrounded => controller != null && controller.enabled && controller.isGrounded;
        public bool IsCrouching => isCrouching;
        public bool IsAiming => isAiming;
        public bool IsSprinting { get; private set; }
        public float PlanarSpeed => new Vector3(planarVelocity.x, 0f, planarVelocity.z).magnitude;
        public Vector3 PlanarVelocity => planarVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
<<<<<<< Updated upstream
            abilityController = GetComponent<KairiAbilityController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
=======
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();
>>>>>>> Stashed changes

            standingHeight = controller.height;
            cameraBaseY = playerCamera != null ? playerCamera.transform.localPosition.y : standingCameraY;
            standingCameraY = cameraBaseY;
            fallbackSpawnPosition = transform.position;
            fallbackSpawnRotation = transform.rotation;

<<<<<<< Updated upstream
            if (playerCamera != null)
            {
                baseFov = playerCamera.fieldOfView > 1f ? playerCamera.fieldOfView : baseFov;
                playerCamera.fieldOfView = baseFov;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
=======
            LockCursor();
>>>>>>> Stashed changes
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;

<<<<<<< Updated upstream
            UpdateAbilityImpulses();
            Look();
            HandleCrouch();
            UpdateSprintState();

            if (abilityController == null)
                abilityController = GetComponent<KairiAbilityController>();

            if (abilityController == null || !abilityController.MovementAbilityActive)
                Move();

=======
            HandleCursor();
            HandleCrouch();
            UpdateMovement();
            UpdateLook();
>>>>>>> Stashed changes
            UpdateFov();
            SafetyRespawnCheck();
        }

        public void SetAiming(bool value)
        {
            isAiming = value && !isCrouching;
        }

        public void AddRecoil(float verticalKick, float horizontalKick)
        {
            recoilTarget.x += Mathf.Max(0f, verticalKick);
            recoilTarget.y += Random.Range(-Mathf.Abs(horizontalKick), Mathf.Abs(horizontalKick));
        }

        public void Respawn()
        {
            if (controller == null)
                return;

            controller.enabled = false;

            if (respawnPoint != null)
            {
                transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
            }
            else
            {
                transform.SetPositionAndRotation(fallbackSpawnPosition, fallbackSpawnRotation);
            }

            planarVelocity = Vector3.zero;
            verticalVelocity = 0f;
            recoilCurrent = Vector2.zero;
            recoilTarget = Vector2.zero;
            controller.enabled = true;
        }

        private void HandleCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
<<<<<<< Updated upstream
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
=======
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
>>>>>>> Stashed changes
        }

        private void HandleCrouch()
        {
            bool crouchRequested = Input.GetKey(crouchKey) ||
                                  (allowLeftControlCrouch && Input.GetKey(KeyCode.LeftControl));

            if (!crouchRequested && isCrouching && !CanStand())
                crouchRequested = true;

            isCrouching = crouchRequested;
            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            float targetCenterY = targetHeight * 0.5f;

            controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSmooth * Time.deltaTime);
            controller.center = Vector3.Lerp(
                controller.center,
                new Vector3(0f, targetCenterY, 0f),
                crouchSmooth * Time.deltaTime);

            if (playerCamera != null)
            {
                float targetCameraY = isCrouching ? crouchCameraY : standingCameraY;
                Vector3 localPosition = playerCamera.transform.localPosition;
                localPosition.y = Mathf.Lerp(localPosition.y, targetCameraY, crouchSmooth * Time.deltaTime);
                playerCamera.transform.localPosition = localPosition;
            }
        }

<<<<<<< Updated upstream
        private void UpdateSprintState()
        {
            float forward = Input.GetAxisRaw("Vertical");
            bool abilityLocked = abilityController != null && abilityController.MovementAbilityActive;
            IsSprinting = !abilityLocked && !isCrouching && controller.isGrounded && Input.GetKey(KeyCode.LeftShift) && forward > 0.1f;
        }

        private void Move()
=======
        private bool CanStand()
>>>>>>> Stashed changes
        {
            float radius = Mathf.Max(0.05f, controller.radius * 0.92f);
            Vector3 bottom = transform.position + Vector3.up * radius;
            Vector3 top = transform.position + Vector3.up * (standingHeight - radius);
            return !Physics.CheckCapsule(bottom, top, radius, ~0, QueryTriggerInteraction.Ignore);
        }

        private void UpdateMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = (transform.right * horizontal + transform.forward * vertical).normalized;

<<<<<<< Updated upstream
            float speed = walkSpeed;
            if (isCrouching) speed = crouchSpeed;
            else if (IsSprinting) speed = sprintSpeed;

            Vector3 velocity = direction * speed;
=======
            IsSprinting = !isCrouching && Input.GetKey(KeyCode.LeftShift) && vertical > 0.1f;
            float targetSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
            Vector3 targetVelocity = input * targetSpeed;
            float rate = targetVelocity.sqrMagnitude > planarVelocity.sqrMagnitude ? acceleration : deceleration;
            planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, rate * Time.deltaTime);
>>>>>>> Stashed changes

            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = -2f;

                if (Input.GetButtonDown("Jump") && !isCrouching)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void UpdateLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
            transform.Rotate(Vector3.up * mouseX, Space.World);

            recoilTarget = Vector2.Lerp(recoilTarget, Vector2.zero, recoilReturnSpeed * Time.deltaTime);
            recoilCurrent = Vector2.Lerp(recoilCurrent, recoilTarget, recoilSnappiness * Time.deltaTime);

            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Euler(pitch - recoilCurrent.x, recoilCurrent.y, 0f);
        }

        private void UpdateFov()
        {
            if (playerCamera == null)
                return;

            float targetFov = isAiming ? aimFov : IsSprinting ? sprintFov : normalFov;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovResponse * Time.deltaTime);
        }

        private void SafetyRespawnCheck()
        {
            bool fellOutOfMap = transform.position.y < killY;
            bool manualRequested = allowManualRespawn && Input.GetKeyDown(manualRespawnKey);
            if (fellOutOfMap || manualRequested)
                Respawn();
        }
<<<<<<< Updated upstream

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
            IsSprinting = false;
            controller.enabled = true;

            Debug.Log("Renkai player respawned.");
        }
=======
>>>>>>> Stashed changes
    }
}
