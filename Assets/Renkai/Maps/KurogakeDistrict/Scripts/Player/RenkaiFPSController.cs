using UnityEngine;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    [RequireComponent(typeof(CharacterController))]
    public class RenkaiFPSController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5.6f;
        public float sprintSpeed = 7.4f;
        public float crouchSpeed = 3.1f;
        public float acceleration = 28f;
        public float deceleration = 38f;
        public float jumpHeight = 1.1f;
        public float gravity = -24f;

        [Header("Movement Response")]
        [SerializeField] private float airAcceleration = 8.5f;
        [SerializeField] private float airDeceleration = 2.4f;
        [SerializeField] private float airSpeedMultiplier = 0.96f;
        [SerializeField] private float coyoteTime = 0.105f;
        [SerializeField] private float jumpBufferTime = 0.13f;
        [SerializeField] private float groundedStickVelocity = -2.8f;

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

        public bool IsGrounded => controller != null && controller.enabled && controller.isGrounded;
        public bool IsCrouching => isCrouching;
        public bool IsSprinting { get; private set; }
        public bool InputLocked { get; private set; }
        public float PlanarSpeed => new Vector3(planarVelocity.x, 0f, planarVelocity.z).magnitude;
        public Vector3 PlanarVelocity => planarVelocity;

        private readonly Collider[] standCheckHits = new Collider[16];

        private CharacterController controller;
        private KairiAbilityController abilityController;
        private KurokageCharacterCollisionGuard collisionGuard;
        private Vector3 planarVelocity;
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
        private float lastGroundedTime = -999f;
        private float lastJumpPressedTime = -999f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            abilityController = GetComponent<KairiAbilityController>();
            collisionGuard = GetComponent<KurokageCharacterCollisionGuard>();
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            standingHeight = controller.height;
            if (playerCamera != null)
            {
                standingCameraY = playerCamera.transform.localPosition.y;
                baseFov = playerCamera.fieldOfView > 1f ? playerCamera.fieldOfView : baseFov;
                playerCamera.fieldOfView = baseFov;
            }

            fallbackSpawnPosition = transform.position;
            fallbackSpawnRotation = transform.rotation;
            LockCursor();
        }

        private void Update()
        {
            if (controller == null || !controller.enabled || !gameObject.activeInHierarchy)
                return;

            if (InputLocked)
            {
                IsSprinting = false;
                requestedSprintFovBonus = 0f;
                planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero, deceleration * Time.deltaTime);
                UpdateAbilityImpulses();
                UpdateFov();
                return;
            }

            HandleCursor();
            CaptureJumpInput();
            UpdateGroundMemory();
            UpdateAbilityImpulses();
            UpdateLook();
            HandleCrouch();
            UpdateSprintState();

            if (abilityController == null)
                abilityController = GetComponent<KairiAbilityController>();

            if (abilityController == null || !abilityController.MovementAbilityActive)
                UpdateMovement();
            else
                DampPlanarVelocityDuringAbility();

            UpdateFov();
            SafetyRespawnCheck();
        }

        public void SetInputLocked(bool locked)
        {
            InputLocked = locked;
            IsSprinting = false;
            requestedSprintFovBonus = 0f;
            lastJumpPressedTime = -999f;

            if (locked)
            {
                planarVelocity = Vector3.zero;
                recoilTarget = Vector2.zero;
            }
            else
            {
                LockCursor();
            }
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

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void CaptureJumpInput()
        {
            if (Input.GetButtonDown("Jump"))
                lastJumpPressedTime = Time.time;
        }

        private void UpdateGroundMemory()
        {
            if (controller.isGrounded)
                lastGroundedTime = Time.time;
        }

        private void UpdateLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX, Space.World);
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

            float target = requestedAdsFov > 1f
                ? requestedAdsFov
                : baseFov + requestedSprintFovBonus;

            target += abilityFovImpulse;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, target, fovSmooth * Time.deltaTime);
        }

        private void UpdateAbilityImpulses()
        {
            abilityRotationImpulse = Vector2.Lerp(abilityRotationImpulse, Vector2.zero, abilityImpulseReturnSpeed * Time.deltaTime);
            abilityRollImpulse = Mathf.Lerp(abilityRollImpulse, 0f, abilityImpulseReturnSpeed * Time.deltaTime);
            abilityFovImpulse = Mathf.Lerp(abilityFovImpulse, 0f, abilityFovReturnSpeed * Time.deltaTime);
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
                crouchSmooth * Time.deltaTime
            );

            if (playerCamera != null)
            {
                float targetCameraY = isCrouching ? crouchCameraY : standingCameraY;
                Vector3 localPosition = playerCamera.transform.localPosition;
                localPosition.y = Mathf.Lerp(localPosition.y, targetCameraY, crouchSmooth * Time.deltaTime);
                playerCamera.transform.localPosition = localPosition;
            }
        }

        private bool CanStand()
        {
            float radius = Mathf.Max(0.05f, controller.radius * 0.90f);
            Vector3 center = transform.position + transform.rotation * new Vector3(0f, standingHeight * 0.5f, 0f);
            float halfSegment = Mathf.Max(0f, standingHeight * 0.5f - radius);
            Vector3 top = center + transform.up * halfSegment;
            Vector3 bottom = center - transform.up * halfSegment;

            int count = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius,
                standCheckHits,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < count; i++)
            {
                Collider hit = standCheckHits[i];
                if (hit == null || hit == controller) continue;
                if (hit.transform.IsChildOf(transform) || transform.IsChildOf(hit.transform)) continue;
                return false;
            }
            return true;
        }

        private void UpdateSprintState()
        {
            float forward = Input.GetAxisRaw("Vertical");
            bool abilityLocked = abilityController != null && abilityController.MovementAbilityActive;
            IsSprinting = !abilityLocked &&
                          !isCrouching &&
                          controller.isGrounded &&
                          Input.GetKey(KeyCode.LeftShift) &&
                          forward > 0.1f;
        }

        private void UpdateMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = (transform.right * horizontal + transform.forward * vertical).normalized;

            bool grounded = controller.isGrounded;
            float targetSpeed = isCrouching
                ? crouchSpeed
                : IsSprinting
                    ? sprintSpeed
                    : walkSpeed;

            if (!grounded)
                targetSpeed *= airSpeedMultiplier;

            Vector3 targetVelocity = input * targetSpeed;
            float rate;
            if (grounded)
            {
                rate = targetVelocity.sqrMagnitude > planarVelocity.sqrMagnitude
                    ? acceleration
                    : deceleration;
            }
            else
            {
                rate = input.sqrMagnitude > 0.01f ? airAcceleration : airDeceleration;
            }

            planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, rate * Time.deltaTime);

            bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
            bool canUseCoyote = Time.time - lastGroundedTime <= coyoteTime;

            if (grounded && verticalVelocity < 0f)
                verticalVelocity = groundedStickVelocity;

            if (jumpBuffered && canUseCoyote && !isCrouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                lastJumpPressedTime = -999f;
                lastGroundedTime = -999f;
            }
            else if (!grounded)
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void DampPlanarVelocityDuringAbility()
        {
            planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero, deceleration * Time.deltaTime);
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = groundedStickVelocity;
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

            planarVelocity = Vector3.zero;
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
            InputLocked = false;
            lastGroundedTime = Time.time;
            lastJumpPressedTime = -999f;

            controller.enabled = true;
            if (collisionGuard == null) collisionGuard = GetComponent<KurokageCharacterCollisionGuard>();
            if (collisionGuard != null) collisionGuard.ResetGuard();
            Debug.Log("Renkai player respawned.");
        }
    }
}
