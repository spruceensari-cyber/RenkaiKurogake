using System.Collections;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KairiAbilityController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private RenkaiRoundPlayer roundPlayer;

        [Header("Q — Directional Dash")]
        [SerializeField] private float dashDistance = 7.5f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float dashCooldown = 6f;

        [Header("E — Holographic Decoy")]
        [SerializeField] private float decoyLifetime = 6f;
        [SerializeField] private float decoySpeed = 3.6f;
        [SerializeField] private float decoyCooldown = 14f;

        [Header("C — Momentum Leap")]
        [SerializeField] private float leapForwardSpeed = 9.5f;
        [SerializeField] private float leapUpSpeed = 8.2f;
        [SerializeField] private float leapDuration = 0.55f;
        [SerializeField] private float leapCooldown = 11f;

        [Header("X — Eclipse Blade Protocol")]
        [SerializeField] private float ultimateDuration = 12f;
        [SerializeField] private float ultimateCooldown = 45f;
        [SerializeField] private float ultimateSwordDamageMultiplier = 1.65f;
        [SerializeField] private float ultimateMoveBoost = 1.18f;

        public float QCooldown01 => Cooldown01(nextQ, dashCooldown);
        public float ECooldown01 => Cooldown01(nextE, decoyCooldown);
        public float CCooldown01 => Cooldown01(nextC, leapCooldown);
        public float XCooldown01 => Cooldown01(nextX, ultimateCooldown);
        public bool UltimateActive => ultimateActive;

        private CharacterController controller;
        private RenkaiFPSController fps;
        private float nextQ;
        private float nextE;
        private float nextC;
        private float nextX;
        private bool movementAbilityActive;
        private bool ultimateActive;
        private float originalWalk;
        private float originalSprint;
        private float originalCrouch;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            fps = GetComponent<RenkaiFPSController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (roundPlayer == null) roundPlayer = GetComponent<RenkaiRoundPlayer>();

            if (fps != null)
            {
                originalWalk = fps.walkSpeed;
                originalSprint = fps.sprintSpeed;
                originalCrouch = fps.crouchSpeed;
            }
        }

        private void Update()
        {
            if (roundPlayer != null && !roundPlayer.isAlive) return;

            if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextQ && !movementAbilityActive)
            {
                nextQ = Time.time + dashCooldown;
                StartCoroutine(DashRoutine());
            }

            if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextE)
            {
                nextE = Time.time + decoyCooldown;
                SpawnDecoy();
            }

            if (Input.GetKeyDown(KeyCode.C) && Time.time >= nextC && !movementAbilityActive)
            {
                nextC = Time.time + leapCooldown;
                StartCoroutine(MomentumLeapRoutine());
            }

            if (Input.GetKeyDown(KeyCode.X) && Time.time >= nextX && !ultimateActive)
            {
                nextX = Time.time + ultimateCooldown;
                StartCoroutine(UltimateRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            movementAbilityActive = true;
            Vector3 input = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
            if (input.sqrMagnitude < 0.01f) input = transform.forward;
            input.y = 0f;
            input.Normalize();

            SpawnBurstTrail(new Color(0.12f, 0.55f, 1f, 1f), 0.22f);

            float elapsed = 0f;
            float speed = dashDistance / Mathf.Max(0.01f, dashDuration);
            while (elapsed < dashDuration)
            {
                if (controller != null && controller.enabled)
                    controller.Move(input * speed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            movementAbilityActive = false;
        }

        private IEnumerator MomentumLeapRoutine()
        {
            movementAbilityActive = true;
            Vector3 forward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            forward.y = 0f;
            forward.Normalize();

            SpawnBurstTrail(new Color(0.36f, 0.30f, 1f, 1f), 0.35f);

            float elapsed = 0f;
            while (elapsed < leapDuration)
            {
                float t = elapsed / leapDuration;
                float vertical = Mathf.Lerp(leapUpSpeed, -3.2f, t);
                Vector3 motion = forward * leapForwardSpeed + Vector3.up * vertical;
                if (controller != null && controller.enabled)
                    controller.Move(motion * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            movementAbilityActive = false;
        }

        private void SpawnDecoy()
        {
            GameObject decoy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            decoy.name = "KAIRI_HOLOGRAPHIC_DECOY";
            decoy.transform.position = transform.position + transform.forward * 1.2f;
            decoy.transform.rotation = transform.rotation;
            Collider c = decoy.GetComponent<Collider>();
            if (c != null) c.isTrigger = true;

            Renderer r = decoy.GetComponent<Renderer>();
            if (r != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material m = new Material(shader);
                Color holo = new Color(0.10f, 0.55f, 1f, 0.55f);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", holo);
                if (m.HasProperty("_Color")) m.SetColor("_Color", holo);
                if (m.HasProperty("_EmissionColor"))
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", holo * 2.2f);
                }
                r.sharedMaterial = m;
            }

            KurokageDecoyRuntime runtime = decoy.AddComponent<KurokageDecoyRuntime>();
            runtime.Initialize(transform.forward, decoySpeed, decoyLifetime);
            SpawnBurstTrail(new Color(0.10f, 0.70f, 1f, 1f), 0.28f, decoy.transform.position);
        }

        private IEnumerator UltimateRoutine()
        {
            ultimateActive = true;
            if (weapon != null) weapon.SendMessage("SelectWeapon", 2, SendMessageOptions.DontRequireReceiver);

            if (fps != null)
            {
                fps.walkSpeed = originalWalk * ultimateMoveBoost;
                fps.sprintSpeed = originalSprint * ultimateMoveBoost;
                fps.crouchSpeed = originalCrouch * ultimateMoveBoost;
            }

            SpawnBurstTrail(new Color(0.20f, 0.42f, 1f, 1f), 0.6f);
            yield return new WaitForSeconds(ultimateDuration);

            if (fps != null)
            {
                fps.walkSpeed = originalWalk;
                fps.sprintSpeed = originalSprint;
                fps.crouchSpeed = originalCrouch;
            }

            ultimateActive = false;
        }

        private void SpawnBurstTrail(Color color, float life, Vector3? overridePosition = null)
        {
            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burst.name = "KAIRI_ABILITY_BURST";
            Object.Destroy(burst.GetComponent<Collider>());
            burst.transform.position = overridePosition ?? transform.position + Vector3.up * 1f;
            burst.transform.localScale = Vector3.one * 0.45f;

            Renderer r = burst.GetComponent<Renderer>();
            if (r != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material m = new Material(shader);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
                if (m.HasProperty("_Color")) m.SetColor("_Color", color);
                if (m.HasProperty("_EmissionColor"))
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", color * 3f);
                }
                r.sharedMaterial = m;
            }

            Object.Destroy(burst, life);
        }

        private static float Cooldown01(float nextReadyTime, float duration)
        {
            if (Time.time >= nextReadyTime) return 0f;
            return Mathf.Clamp01((nextReadyTime - Time.time) / Mathf.Max(0.01f, duration));
        }
    }
}
