using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class KurokageBladeCombatController : MonoBehaviour
    {
        public enum BladeAttackState
        {
            Idle,
            Windup,
            Active,
            Recovery,
            ComboWindow,
            DashCutTravel,
            AerialDescent,
            UltimateChain
        }

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
        public bool IsAttacking => attackState != BladeAttackState.Idle;
        public bool CanChain => attackState == BladeAttackState.ComboWindow;
        public BladeAttackState CurrentAttackState => attackState;

        private CharacterController controller;
        private RenkaiRoundPlayer self;
        private float nextGhostEdge;
        private float nextAirBreak;
        private float nextKuroShift;
        private float nextPhantomBurst;
        private BladeAttackState attackState = BladeAttackState.Idle;
        private Coroutine activeRoutine;
        private readonly HashSet<RenkaiRoundPlayer> hitCache = new HashSet<RenkaiRoundPlayer>();

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            self = GetComponent<RenkaiRoundPlayer>();
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (abilities == null) abilities = GetComponent<KairiAbilityController>();
        }

        private void Update()
        {
            if (weapon == null || weapon.slot != RenkaiWeaponSlot.Sword || IsAttacking)
                return;

            if (Input.GetMouseButtonDown(1) && Time.time >= nextGhostEdge)
            {
                nextGhostEdge = Time.time + ghostEdgeCooldown;
                activeRoutine = StartCoroutine(GhostEdgeRoutine());
                return;
            }

            if (Input.GetKeyDown(KeyCode.V) && Time.time >= nextKuroShift)
            {
                nextKuroShift = Time.time + kuroShiftCooldown;
                activeRoutine = StartCoroutine(KuroShiftRoutine());
                return;
            }

            bool airborne = controller != null && !controller.isGrounded;
            if (airborne && Input.GetMouseButtonDown(0) && Time.time >= nextAirBreak)
            {
                nextAirBreak = Time.time + airBreakCooldown;
                activeRoutine = StartCoroutine(AirBreakRoutine());
                return;
            }

            if (abilities != null && abilities.UltimateActive && Input.GetMouseButtonDown(0) && Time.time >= nextPhantomBurst)
            {
                nextPhantomBurst = Time.time + phantomBurstCooldown;
                activeRoutine = StartCoroutine(PhantomBurstRoutine());
            }
        }

        public void ResetForRound()
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = null;
            attackState = BladeAttackState.Idle;
            hitCache.Clear();
            nextGhostEdge = 0f;
            nextAirBreak = 0f;
            nextKuroShift = 0f;
            nextPhantomBurst = 0f;
            LastComboName = "READY";
        }

        private IEnumerator GhostEdgeRoutine()
        {
            attackState = BladeAttackState.Windup;
            AnnounceCombo("GHOST EDGE");
            SpawnLayeredSlash(new Color(0.20f, 0.58f, 1f, 1f), new Color(0.46f, 0.24f, 1f, 1f), 2.0f, 0.16f, 28f);
            yield return new WaitForSeconds(0.05f);

            attackState = BladeAttackState.DashCutTravel;
            Vector3 dir = FlatForward();
            float elapsed = 0f;
            while (elapsed < 0.12f)
            {
                CollisionFlags flags = controller.Move(dir * (ghostEdgeLunge / 0.12f) * Time.deltaTime);
                if ((flags & CollisionFlags.Sides) != 0) break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            attackState = BladeAttackState.Active;
            DamageCone(ghostEdgeRange, 1.15f, ghostEdgeDamage, "GHOST EDGE");
            yield return Recovery(0.22f);
        }

        private IEnumerator AirBreakRoutine()
        {
            attackState = BladeAttackState.AerialDescent;
            AnnounceCombo("AIR BREAK");
            SpawnLayeredSlash(new Color(0.30f, 0.76f, 1f, 1f), new Color(0.48f, 0.30f, 1f, 1f), 2.4f, 0.19f, -32f);

            float elapsed = 0f;
            while (elapsed < 0.22f)
            {
                controller.Move((FlatForward() * 4.5f + Vector3.down * 9f) * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            attackState = BladeAttackState.Active;
            hitCache.Clear();
            DamageSphere(transform.position + transform.forward * 1.2f, airBreakRadius, airBreakDamage, "AIR BREAK", hitCache);
            SpawnLandingRing(transform.position, new Color(0.26f, 0.62f, 1f, 1f), 0.34f);
            yield return Recovery(0.32f);
        }

        private IEnumerator KuroShiftRoutine()
        {
            attackState = BladeAttackState.DashCutTravel;
            AnnounceCombo("KURO SHIFT");
            Vector3 direction = FlatForward();
            SpawnTravelStreak(new Color(0.38f, 0.22f, 1f, 1f), 0.24f);

            hitCache.Clear();
            float elapsed = 0f;
            float speed = kuroShiftDistance / Mathf.Max(0.01f, kuroShiftDuration);
            while (elapsed < kuroShiftDuration)
            {
                CollisionFlags flags = controller.Move(direction * speed * Time.deltaTime);
                DamageSphere(transform.position + Vector3.up * 1f, 1.35f, kuroShiftDamage, "KURO SHIFT", hitCache);
                if ((flags & CollisionFlags.Sides) != 0) break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            SpawnLayeredSlash(new Color(0.52f, 0.32f, 1f, 1f), new Color(0.20f, 0.64f, 1f, 1f), 2.2f, 0.12f, 75f);
            yield return Recovery(0.34f);
        }

        private IEnumerator PhantomBurstRoutine()
        {
            attackState = BladeAttackState.UltimateChain;
            AnnounceCombo("PHANTOM BURST");

            for (int i = 0; i < phantomBurstHits; i++)
            {
                float angle = i % 2 == 0 ? 48f : -48f;
                SpawnLayeredSlash(new Color(0.16f, 0.62f, 1f, 1f), new Color(0.48f, 0.22f, 1f, 1f), 2.5f, 0.11f, angle);
                hitCache.Clear();
                DamageCone(4.6f, 1.35f, phantomBurstDamage, "PHANTOM BURST", hitCache);
                controller.Move(FlatForward() * 0.75f);
                yield return new WaitForSeconds(phantomBurstInterval);
            }

            SpawnLandingRing(transform.position + transform.forward * 2.2f, new Color(0.18f, 0.50f, 1f, 1f), 0.4f);
            yield return Recovery(0.38f);
        }

        private IEnumerator Recovery(float duration)
        {
            attackState = BladeAttackState.Recovery;
            yield return new WaitForSeconds(duration * 0.72f);
            attackState = BladeAttackState.ComboWindow;
            yield return new WaitForSeconds(duration * 0.28f);
            attackState = BladeAttackState.Idle;
            activeRoutine = null;
        }

        private void DamageCone(float range, float radius, float damage, string sourceId, HashSet<RenkaiRoundPlayer> externalCache = null)
        {
            Vector3 origin = playerCamera != null ? playerCamera.transform.position : transform.position + Vector3.up * 1.4f;
            Vector3 direction = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, direction, range, ~0, QueryTriggerInteraction.Collide);
            HashSet<RenkaiRoundPlayer> damaged = externalCache ?? new HashSet<RenkaiRoundPlayer>();

            foreach (RaycastHit hit in hits)
            {
                RenkaiRoundPlayer victim = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (victim == null || damaged.Contains(victim)) continue;
                if (self != null && victim.team == self.team) continue;
                damaged.Add(victim);
                ApplyBladeDamage(victim, damage, hit.point, hit.normal, sourceId);
                SpawnImpactPulse(hit.point, new Color(0.22f, 0.66f, 1f, 1f), 0.16f);
            }
        }

        private void DamageSphere(Vector3 center, float radius, float damage, string sourceId, HashSet<RenkaiRoundPlayer> cache)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Collide);
            foreach (Collider hit in hits)
            {
                RenkaiRoundPlayer victim = hit.GetComponentInParent<RenkaiRoundPlayer>();
                if (victim == null) continue;
                if (self != null && victim.team == self.team) continue;
                if (cache != null && cache.Contains(victim)) continue;
                if (cache != null) cache.Add(victim);
                ApplyBladeDamage(victim, damage, hit.ClosestPoint(center), (center - victim.transform.position).normalized, sourceId);
            }
        }

        private void ApplyBladeDamage(RenkaiRoundPlayer victim, float damage, Vector3 point, Vector3 normal, string sourceId)
        {
            KurokageDamageInfo info = new KurokageDamageInfo(
                damage,
                self,
                point,
                normal,
                KurokageDamageType.Blade,
                KurokageHitZoneType.Torso,
                sourceId
            );
            victim.ApplyDamage(info);
        }

        private void SpawnLayeredSlash(Color core, Color edge, float width, float life, float zAngle)
        {
            if (playerCamera == null) return;
            Vector3 point = playerCamera.transform.TransformPoint(new Vector3(0f, -0.03f, 1.45f));
            Quaternion rotation = playerCamera.transform.rotation * Quaternion.Euler(0f, 0f, zAngle);

            KurokageVfxPool.Instance.Spawn(KurokageVfxShape.Cube, "BLADE_SLASH_CORE", point, rotation, new Vector3(width, 0.028f, 0.10f), core, 4.8f, life);
            KurokageVfxPool.Instance.Spawn(KurokageVfxShape.Cube, "BLADE_SLASH_EDGE", point, rotation, new Vector3(width * 1.05f, 0.012f, 0.16f), edge, 3.4f, life * 0.9f);
        }

        private void SpawnTravelStreak(Color color, float life)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cube,
                "KURO_SHIFT_STREAK",
                transform.position + Vector3.up * 1f,
                transform.rotation,
                new Vector3(0.12f, 1.1f, 2.2f),
                color,
                3.8f,
                life
            );
        }

        private void SpawnLandingRing(Vector3 position, Color color, float life)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "BLADE_LANDING_RING",
                position + Vector3.up * 0.03f,
                Quaternion.identity,
                new Vector3(1.5f, 0.025f, 1.5f),
                color,
                4f,
                life
            );
        }

        private void SpawnImpactPulse(Vector3 position, Color color, float life)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "ECLIPSE_IMPACT_PULSE",
                position,
                Quaternion.identity,
                Vector3.one * 0.32f,
                color,
                3.5f,
                life
            );
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
