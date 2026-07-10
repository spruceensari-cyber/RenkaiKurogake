using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageBladeCombatController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private KairiAbilityController abilities;

        [Header("Ghost Edge — RMB")]
        [SerializeField] private float ghostEdgeDamage = 72f;
        [SerializeField] private float ghostEdgeRange = 4.2f;
        [SerializeField] private float ghostEdgeCooldown = 1.15f;
        [SerializeField] private float ghostEdgeLunge = 2.4f;

        [Header("Air Break — Jump + LMB")]
        [SerializeField] private float airBreakDamage = 64f;
        [SerializeField] private float airBreakRadius = 2.2f;
        [SerializeField] private float airBreakCooldown = 2.2f;

        [Header("Kuro Shift — V")]
        [SerializeField] private float kuroShiftDamage = 58f;
        [SerializeField] private float kuroShiftDistance = 6.5f;
        [SerializeField] private float kuroShiftDuration = 0.16f;
        [SerializeField] private float kuroShiftCooldown = 4.8f;

        [Header("Phantom Burst — Ultimate LMB chain")]
        [SerializeField] private float phantomBurstDamage = 48f;
        [SerializeField] private int phantomBurstHits = 3;
        [SerializeField] private float phantomBurstInterval = 0.11f;
        [SerializeField] private float phantomBurstCooldown = 1.6f;

        public string LastComboName { get; private set; } = "READY";
        public float ComboFlashUntil { get; private set; }

        private CharacterController controller;
        private float nextGhostEdge;
        private float nextAirBreak;
        private float nextKuroShift;
        private float nextPhantomBurst;
        private bool comboRoutineActive;

        private readonly HashSet<RenkaiRoundPlayer> hitCache = new HashSet<RenkaiRoundPlayer>();

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (abilities == null) abilities = GetComponent<KairiAbilityController>();
        }

        private void Update()
        {
            if (weapon == null || weapon.slot != RenkaiWeaponSlot.Sword || comboRoutineActive)
                return;

            if (Input.GetMouseButtonDown(1) && Time.time >= nextGhostEdge)
            {
                nextGhostEdge = Time.time + ghostEdgeCooldown;
                StartCoroutine(GhostEdgeRoutine());
                return;
            }

            if (Input.GetKeyDown(KeyCode.V) && Time.time >= nextKuroShift)
            {
                nextKuroShift = Time.time + kuroShiftCooldown;
                StartCoroutine(KuroShiftRoutine());
                return;
            }

            bool airborne = controller != null && !controller.isGrounded;
            if (airborne && Input.GetMouseButtonDown(0) && Time.time >= nextAirBreak)
            {
                nextAirBreak = Time.time + airBreakCooldown;
                StartCoroutine(AirBreakRoutine());
                return;
            }

            if (abilities != null && abilities.UltimateActive && Input.GetMouseButtonDown(0) && Time.time >= nextPhantomBurst)
            {
                nextPhantomBurst = Time.time + phantomBurstCooldown;
                StartCoroutine(PhantomBurstRoutine());
            }
        }

        private IEnumerator GhostEdgeRoutine()
        {
            comboRoutineActive = true;
            AnnounceCombo("GHOST EDGE");
            SpawnSlashArc(new Color(0.20f, 0.58f, 1f, 1f), 2.0f, 0.16f, 28f);

            Vector3 dir = FlatForward();
            float elapsed = 0f;
            while (elapsed < 0.12f)
            {
                controller.Move(dir * (ghostEdgeLunge / 0.12f) * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            DamageCone(ghostEdgeRange, 1.15f, ghostEdgeDamage);
            comboRoutineActive = false;
        }

        private IEnumerator AirBreakRoutine()
        {
            comboRoutineActive = true;
            AnnounceCombo("AIR BREAK");
            SpawnSlashArc(new Color(0.30f, 0.76f, 1f, 1f), 2.4f, 0.19f, -32f);

            float elapsed = 0f;
            while (elapsed < 0.22f)
            {
                controller.Move((FlatForward() * 4.5f + Vector3.down * 9f) * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            DamageSphere(transform.position + transform.forward * 1.2f, airBreakRadius, airBreakDamage);
            SpawnImpactPulse(transform.position, new Color(0.26f, 0.62f, 1f, 1f), 0.34f);
            comboRoutineActive = false;
        }

        private IEnumerator KuroShiftRoutine()
        {
            comboRoutineActive = true;
            AnnounceCombo("KURO SHIFT");
            Vector3 direction = FlatForward();
            SpawnTrailBurst(new Color(0.38f, 0.22f, 1f, 1f), 0.24f);

            hitCache.Clear();
            float elapsed = 0f;
            float speed = kuroShiftDistance / Mathf.Max(0.01f, kuroShiftDuration);
            while (elapsed < kuroShiftDuration)
            {
                controller.Move(direction * speed * Time.deltaTime);
                DamageSphere(transform.position + Vector3.up * 1f, 1.35f, kuroShiftDamage, hitCache);
                elapsed += Time.deltaTime;
                yield return null;
            }

            SpawnSlashArc(new Color(0.52f, 0.32f, 1f, 1f), 2.2f, 0.12f, 75f);
            comboRoutineActive = false;
        }

        private IEnumerator PhantomBurstRoutine()
        {
            comboRoutineActive = true;
            AnnounceCombo("PHANTOM BURST");

            for (int i = 0; i < phantomBurstHits; i++)
            {
                float angle = i % 2 == 0 ? 48f : -48f;
                SpawnSlashArc(new Color(0.16f, 0.62f, 1f, 1f), 2.5f, 0.11f, angle);
                DamageCone(4.6f, 1.35f, phantomBurstDamage);
                controller.Move(FlatForward() * 0.75f);
                yield return new WaitForSeconds(phantomBurstInterval);
            }

            SpawnImpactPulse(transform.position + transform.forward * 2.2f, new Color(0.18f, 0.50f, 1f, 1f), 0.4f);
            comboRoutineActive = false;
        }

        private void DamageCone(float range, float radius, float damage)
        {
            Vector3 origin = playerCamera != null ? playerCamera.transform.position : transform.position + Vector3.up * 1.4f;
            Vector3 direction = playerCamera != null ? playerCamera.transform.forward : transform.forward;

            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, direction, range, ~0, QueryTriggerInteraction.Ignore);
            HashSet<RenkaiRoundPlayer> damaged = new HashSet<RenkaiRoundPlayer>();
            foreach (RaycastHit hit in hits)
            {
                RenkaiRoundPlayer victim = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (victim == null || damaged.Contains(victim)) continue;
                RenkaiRoundPlayer self = GetComponent<RenkaiRoundPlayer>();
                if (self != null && victim.team == self.team) continue;
                damaged.Add(victim);
                victim.TakeDamage(damage, self);
                SpawnImpactPulse(hit.point, new Color(0.22f, 0.66f, 1f, 1f), 0.16f);
            }
        }

        private void DamageSphere(Vector3 center, float radius, float damage, HashSet<RenkaiRoundPlayer> cache = null)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Ignore);
            RenkaiRoundPlayer self = GetComponent<RenkaiRoundPlayer>();
            foreach (Collider hit in hits)
            {
                RenkaiRoundPlayer victim = hit.GetComponentInParent<RenkaiRoundPlayer>();
                if (victim == null) continue;
                if (self != null && victim.team == self.team) continue;
                if (cache != null && cache.Contains(victim)) continue;
                if (cache != null) cache.Add(victim);
                victim.TakeDamage(damage, self);
            }
        }

        private void SpawnSlashArc(Color color, float width, float life, float zAngle)
        {
            if (playerCamera == null) return;
            GameObject arc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arc.name = "ECLIPSE_SLASH_ARC";
            Object.Destroy(arc.GetComponent<Collider>());
            arc.transform.SetParent(playerCamera.transform, false);
            arc.transform.localPosition = new Vector3(0f, -0.03f, 1.45f);
            arc.transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
            arc.transform.localScale = new Vector3(width, 0.035f, 0.16f);
            ApplyEmission(arc.GetComponent<Renderer>(), color, 4.0f);
            Object.Destroy(arc, life);
        }

        private void SpawnTrailBurst(Color color, float life)
        {
            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            burst.name = "KURO_SHIFT_TRAIL";
            Object.Destroy(burst.GetComponent<Collider>());
            burst.transform.position = transform.position + Vector3.up * 1f;
            burst.transform.rotation = transform.rotation;
            burst.transform.localScale = new Vector3(0.55f, 1.0f, 0.55f);
            ApplyEmission(burst.GetComponent<Renderer>(), color, 3.2f);
            Object.Destroy(burst, life);
        }

        private void SpawnImpactPulse(Vector3 position, Color color, float life)
        {
            GameObject pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = "ECLIPSE_IMPACT_PULSE";
            Object.Destroy(pulse.GetComponent<Collider>());
            pulse.transform.position = position;
            pulse.transform.localScale = Vector3.one * 0.32f;
            ApplyEmission(pulse.GetComponent<Renderer>(), color, 3.5f);
            Object.Destroy(pulse, life);
        }

        private static void ApplyEmission(Renderer renderer, Color color, float emission)
        {
            if (renderer == null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            renderer.sharedMaterial = material;
        }

        private Vector3 FlatForward()
        {
            Vector3 forward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.01f ? forward.normalized : transform.forward;
        }

        private void AnnounceCombo(string comboName)
        {
            LastComboName = comboName;
            ComboFlashUntil = Time.time + 0.8f;
        }
    }
}
