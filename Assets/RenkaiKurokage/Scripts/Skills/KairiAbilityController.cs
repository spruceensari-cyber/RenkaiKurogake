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
        [SerializeField] private KurokageAfterimagePresenter afterimages;

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
        [SerializeField] private float ultimateMoveBoost = 1.18f;

        public float QCooldown01 => Cooldown01(nextQ, dashCooldown);
        public float ECooldown01 => Cooldown01(nextE, decoyCooldown);
        public float CCooldown01 => Cooldown01(nextC, leapCooldown);
        public float XCooldown01 => Cooldown01(nextX, ultimateCooldown);
        public bool UltimateActive => ultimateActive;
        public bool MovementAbilityActive => movementAbilityActive;
        public bool IsMomentumLeaping => isMomentumLeaping;

        private CharacterController controller;
        private RenkaiFPSController fps;
        private float nextQ;
        private float nextE;
        private float nextC;
        private float nextX;
        private bool movementAbilityActive;
        private bool ultimateActive;
        private bool isMomentumLeaping;
        private bool waitingForLeapLanding;
        private float originalWalk;
        private float originalSprint;
        private float originalCrouch;
        private Coroutine movementRoutine;
        private Coroutine ultimateRoutine;
        private Coroutine leapLandingRoutine;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            fps = GetComponent<RenkaiFPSController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (roundPlayer == null) roundPlayer = GetComponent<RenkaiRoundPlayer>();
            if (afterimages == null) afterimages = GetComponent<KurokageAfterimagePresenter>();

            if (fps != null)
            {
                originalWalk = fps.walkSpeed;
                originalSprint = fps.sprintSpeed;
                originalCrouch = fps.crouchSpeed;
            }
        }

        private void OnEnable()
        {
            ResetAbilityState(false);
        }

        private void Update()
        {
            if (roundPlayer != null && !roundPlayer.isAlive) return;

            if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextQ && !movementAbilityActive)
            {
                nextQ = Time.time + dashCooldown;
                movementRoutine = StartCoroutine(DashRoutine());
            }

            if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextE)
            {
                nextE = Time.time + decoyCooldown;
                SpawnDecoy();
            }

            if (Input.GetKeyDown(KeyCode.C) && Time.time >= nextC && !movementAbilityActive)
            {
                nextC = Time.time + leapCooldown;
                movementRoutine = StartCoroutine(MomentumLeapRoutine());
            }

            if (Input.GetKeyDown(KeyCode.X) && Time.time >= nextX && !ultimateActive)
            {
                nextX = Time.time + ultimateCooldown;
                ultimateRoutine = StartCoroutine(UltimateRoutine());
            }
        }

        public void ResetAbilityState(bool resetCooldowns = true)
        {
            if (movementRoutine != null) StopCoroutine(movementRoutine);
            if (ultimateRoutine != null) StopCoroutine(ultimateRoutine);
            if (leapLandingRoutine != null) StopCoroutine(leapLandingRoutine);
            movementRoutine = null;
            ultimateRoutine = null;
            leapLandingRoutine = null;
            movementAbilityActive = false;
            ultimateActive = false;
            isMomentumLeaping = false;
            waitingForLeapLanding = false;

            if (fps != null)
            {
                fps.walkSpeed = originalWalk;
                fps.sprintSpeed = originalSprint;
                fps.crouchSpeed = originalCrouch;
            }

            if (afterimages != null)
                afterimages.ClearAll();

            if (resetCooldowns)
            {
                nextQ = 0f;
                nextE = 0f;
                nextC = 0f;
                nextX = 0f;
            }

            for (int i = KurokageDecoyRuntime.Active.Count - 1; i >= 0; i--)
            {
                KurokageDecoyRuntime decoy = KurokageDecoyRuntime.Active[i];
                if (decoy != null) decoy.DissolveAndDestroy();
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
            if (afterimages != null) afterimages.SpawnDashSequence(dashDuration);
            if (fps != null)
            {
                float side = Vector3.Dot(input, transform.right);
                fps.AddAbilityCameraImpulse(-0.8f, side * 0.65f, -side * 2.4f, 5.2f);
            }

            float elapsed = 0f;
            float speed = dashDistance / Mathf.Max(0.01f, dashDuration);
            while (elapsed < dashDuration)
            {
                if (controller != null && controller.enabled)
                {
                    CollisionFlags flags = controller.Move(input * speed * Time.deltaTime);
                    if ((flags & CollisionFlags.Sides) != 0) break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            movementAbilityActive = false;
            movementRoutine = null;
        }

        private IEnumerator MomentumLeapRoutine()
        {
            movementAbilityActive = true;
            isMomentumLeaping = true;
            waitingForLeapLanding = false;

            Vector3 forward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            forward.y = 0f;
            forward.Normalize();

            SpawnBurstTrail(new Color(0.36f, 0.30f, 1f, 1f), 0.35f);
            if (fps != null)
                fps.AddAbilityCameraImpulse(2.4f, 0f, 0f, 3.6f);

            float elapsed = 0f;
            while (elapsed < leapDuration)
            {
                float t = elapsed / leapDuration;
                float forwardCurve = Mathf.Lerp(1f, 0.35f, t);
                float vertical = Mathf.Lerp(leapUpSpeed, -3.2f, t);
                Vector3 redirect = transform.right * Input.GetAxisRaw("Horizontal") * 2.2f;
                Vector3 motion = forward * leapForwardSpeed * forwardCurve + redirect + Vector3.up * vertical;
                if (controller != null && controller.enabled)
                    controller.Move(motion * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            movementAbilityActive = false;
            movementRoutine = null;
            waitingForLeapLanding = true;
            leapLandingRoutine = StartCoroutine(WaitForLeapLanding());
        }

        private IEnumerator WaitForLeapLanding()
        {
            float safetyTimeout = Time.time + 4f;
            while (waitingForLeapLanding && Time.time < safetyTimeout)
            {
                if (controller != null && controller.enabled && controller.isGrounded)
                {
                    waitingForLeapLanding = false;
                    isMomentumLeaping = false;
                    SpawnLeapLandingShockwave();
                    if (fps != null)
                        fps.AddAbilityCameraImpulse(3.4f, 0f, 0f, 1.8f);
                    leapLandingRoutine = null;
                    yield break;
                }
                yield return null;
            }

            waitingForLeapLanding = false;
            isMomentumLeaping = false;
            leapLandingRoutine = null;
        }

        private void SpawnLeapLandingShockwave()
        {
            Vector3 ground = transform.position + Vector3.up * 0.03f;
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "MOMENTUM_LEAP_LANDING_RING",
                ground,
                Quaternion.identity,
                new Vector3(1.8f, 0.025f, 1.8f),
                new Color(0.28f, 0.64f, 1f, 1f),
                4.2f,
                0.34f
            );

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "MOMENTUM_LEAP_LANDING_CORE",
                transform.position + Vector3.up * 0.25f,
                Quaternion.identity,
                Vector3.one * 0.42f,
                new Color(0.42f, 0.30f, 1f, 1f),
                3.8f,
                0.22f
            );
        }

        private void SpawnDecoy()
        {
            GameObject decoyRoot = new GameObject("KAIRI_HOLOGRAPHIC_DECOY");
            decoyRoot.transform.position = transform.position + transform.forward * 1.2f;
            decoyRoot.transform.rotation = transform.rotation;

            Transform sourceVisual = transform.Find("AGENT_VISUAL");
            if (sourceVisual != null)
            {
                GameObject visualClone = Object.Instantiate(sourceVisual.gameObject, decoyRoot.transform);
                visualClone.name = "HOLOGRAM_VISUAL";
                visualClone.transform.localPosition = Vector3.zero;
                visualClone.transform.localRotation = Quaternion.identity;

                foreach (MonoBehaviour behaviour in visualClone.GetComponentsInChildren<MonoBehaviour>(true))
                    behaviour.enabled = false;

                foreach (Collider c in visualClone.GetComponentsInChildren<Collider>(true))
                    c.enabled = false;

                foreach (KurokageHitZone zone in visualClone.GetComponentsInChildren<KurokageHitZone>(true))
                    Object.Destroy(zone);

                ApplyHologramMaterial(visualClone);
            }
            else
            {
                Debug.LogWarning("Kairi decoy spawned without AGENT_VISUAL. Decoy visual skipped.");
            }

            KurokageDecoyRuntime runtime = decoyRoot.AddComponent<KurokageDecoyRuntime>();
            runtime.Initialize(transform.forward, decoySpeed, decoyLifetime);
            SpawnBurstTrail(new Color(0.10f, 0.70f, 1f, 1f), 0.28f, decoyRoot.transform.position);
        }

        private static void ApplyHologramMaterial(GameObject root)
        {
            Color holo = new Color(0.10f, 0.55f, 1f, 0.68f);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material m = new Material(shader);
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", holo);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", holo);
                    if (m.HasProperty("_EmissionColor"))
                    {
                        m.EnableKeyword("_EMISSION");
                        m.SetColor("_EmissionColor", holo * 2.4f);
                    }
                    mats[i] = m;
                }
                r.materials = mats;
            }
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
                fps.AddAbilityCameraImpulse(-2.2f, 0f, 0.8f, 4.5f);
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
            ultimateRoutine = null;
        }

        private void SpawnBurstTrail(Color color, float life, Vector3? overridePosition = null)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "KAIRI_ABILITY_BURST",
                overridePosition ?? transform.position + Vector3.up * 1f,
                Quaternion.identity,
                Vector3.one * 0.45f,
                color,
                3f,
                life
            );
        }

        private static float Cooldown01(float nextReadyTime, float duration)
        {
            if (Time.time >= nextReadyTime) return 0f;
            return Mathf.Clamp01((nextReadyTime - Time.time) / Mathf.Max(0.01f, duration));
        }
    }
}
