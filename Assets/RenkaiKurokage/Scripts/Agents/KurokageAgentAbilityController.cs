using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(KurokageAgentIdentity))]
    public sealed class KurokageAgentAbilityController : MonoBehaviour
    {
        private readonly float[] nextUse = new float[4];

        private KurokageAgentIdentity identity;
        private RenkaiRoundPlayer roundPlayer;
        private RenkaiFPSController fps;
        private RenkaiTacticalBotAI tacticalBot;
        private CharacterController controller;
        private KurokageArmor armor;
        private KurokageJapaneseVoicePresenter voice;
        private Coroutine movementRoutine;
        private Coroutine buffRoutine;
        private float baseWalk;
        private float baseSprint;
        private float baseCrouch;
        private float baseBotMove;
        private Vector3 requestedDirection;
        private bool hasRequestedDirection;

        public float Cooldown01(int slot)
        {
            if (identity == null || slot < 0 || slot >= 4) return 0f;
            KurokageAbilityDefinition ability = identity.Definition.Abilities[slot];
            if (ability.Cooldown <= 0f) return 0f;
            return Mathf.Clamp01((nextUse[slot] - Time.time) / ability.Cooldown);
        }

        public bool IsReady(int slot)
        {
            return slot >= 0 && slot < 4 && Time.time >= nextUse[slot];
        }

        private void Awake()
        {
            identity = GetComponent<KurokageAgentIdentity>();
            roundPlayer = GetComponent<RenkaiRoundPlayer>();
            fps = GetComponent<RenkaiFPSController>();
            tacticalBot = GetComponent<RenkaiTacticalBotAI>();
            controller = GetComponent<CharacterController>();
            armor = GetComponent<KurokageArmor>();
            voice = GetComponent<KurokageJapaneseVoicePresenter>();

            if (fps != null)
            {
                baseWalk = fps.walkSpeed;
                baseSprint = fps.sprintSpeed;
                baseCrouch = fps.crouchSpeed;
            }
            if (tacticalBot != null) baseBotMove = tacticalBot.moveSpeed;
        }

        private void OnDisable()
        {
            ResetAbilityState(false);
        }

        private void Update()
        {
            if (roundPlayer != null && (!roundPlayer.isAlive || !roundPlayer.isHumanPlayer)) return;
            if (identity == null || identity.Archetype == KurokageAgentArchetype.Kairi) return;

            if (Input.GetKeyDown(KeyCode.Q)) TryActivateSlot(0);
            if (Input.GetKeyDown(KeyCode.E)) TryActivateSlot(1);
            if (Input.GetKeyDown(KeyCode.C)) TryActivateSlot(2);
            if (Input.GetKeyDown(KeyCode.X)) TryActivateSlot(3);
        }

        public bool TryActivateSlot(int slot)
        {
            hasRequestedDirection = false;
            return TryActivate(slot);
        }

        public bool TryActivateSlot(int slot, Vector3 desiredDirection)
        {
            requestedDirection = desiredDirection;
            requestedDirection.y = 0f;
            hasRequestedDirection = requestedDirection.sqrMagnitude > 0.01f;
            return TryActivate(slot);
        }

        public void ResetAbilityState(bool resetCooldowns = true)
        {
            if (movementRoutine != null) StopCoroutine(movementRoutine);
            if (buffRoutine != null) StopCoroutine(buffRoutine);
            movementRoutine = null;
            buffRoutine = null;
            hasRequestedDirection = false;

            if (fps != null)
            {
                fps.walkSpeed = baseWalk;
                fps.sprintSpeed = baseSprint;
                fps.crouchSpeed = baseCrouch;
            }
            if (tacticalBot != null && baseBotMove > 0f)
                tacticalBot.moveSpeed = baseBotMove;

            if (resetCooldowns)
            {
                for (int i = 0; i < nextUse.Length; i++) nextUse[i] = 0f;
            }
        }

        private bool TryActivate(int slot)
        {
            if (identity == null || slot < 0 || slot >= 4 || Time.time < nextUse[slot]) return false;
            KurokageAbilityDefinition ability = identity.Definition.Abilities[slot];
            nextUse[slot] = Time.time + ability.Cooldown;
            if (voice == null) voice = GetComponent<KurokageJapaneseVoicePresenter>();
            if (voice != null) voice.PlayAbility(slot);
            Execute(ability);
            return true;
        }

        private void Execute(KurokageAbilityDefinition ability)
        {
            Color color = identity.Definition.Accent;
            SpawnBurst(transform.position + Vector3.up * 0.8f, color, 0.35f, 0.65f);
            if (fps != null) fps.AddAbilityCameraImpulse(-0.45f, 0f, 0f, 2.5f);

            switch (ability.Action)
            {
                case KurokageAbilityAction.DirectionalDash:
                case KurokageAbilityAction.PhaseStep:
                case KurokageAbilityAction.BladeLunge:
                case KurokageAbilityAction.BulwarkStep:
                case KurokageAbilityAction.SpearDash:
                case KurokageAbilityAction.BreakerCharge:
                case KurokageAbilityAction.PetalStep:
                case KurokageAbilityAction.ShadowStep:
                    StartMovement(StepRoutine(ability.Strength, Mathf.Max(0.10f, ability.Duration), color));
                    break;

                case KurokageAbilityAction.MomentumLeap:
                case KurokageAbilityAction.VaultStrike:
                case KurokageAbilityAction.HeavyLeap:
                    StartMovement(LeapRoutine(ability.Strength, ability.Strength * 0.82f, Mathf.Max(0.45f, ability.Duration), color));
                    break;

                case KurokageAbilityAction.HealingPulse:
                case KurokageAbilityAction.OrbitHeal:
                case KurokageAbilityAction.NetworkRestore:
                    HealAllies(ability.Strength, ability.Action == KurokageAbilityAction.NetworkRestore ? 12f : 7f, color);
                    break;

                case KurokageAbilityAction.KineticShield:
                case KurokageAbilityAction.ResonanceGuard:
                case KurokageAbilityAction.HeatShield:
                case KurokageAbilityAction.Fortify:
                case KurokageAbilityAction.FortressProtocol:
                case KurokageAbilityAction.ColossusProtocol:
                    AddArmor(ability.Strength, color);
                    if (ability.Duration > 0f) StartBuff(TemporarySpeedBuff(1.06f, ability.Duration, color));
                    break;

                case KurokageAbilityAction.HolographicDecoy:
                case KurokageAbilityAction.SupportDrone:
                case KurokageAbilityAction.BlossomDecoy:
                    SpawnDecoy(ability.Duration, ability.Action == KurokageAbilityAction.SupportDrone ? 2.2f : 4.2f, color);
                    break;

                case KurokageAbilityAction.PulseScan:
                    PulseEnemies(ability.Strength, color);
                    break;

                case KurokageAbilityAction.NullScreen:
                case KurokageAbilityAction.WindScreen:
                case KurokageAbilityAction.BarrierField:
                case KurokageAbilityAction.GravityAnchor:
                    SpawnField(ability.Strength, ability.Duration, 3.2f, 0f, color);
                    break;

                case KurokageAbilityAction.AirCut:
                case KurokageAbilityAction.GroundSlam:
                case KurokageAbilityAction.TempestChain:
                case KurokageAbilityAction.AnchorPull:
                case KurokageAbilityAction.EchoMine:
                    DamageEnemies(ability.Strength, ability.Action == KurokageAbilityAction.TempestChain ? 8f : 5.5f, color);
                    break;

                case KurokageAbilityAction.Overclock:
                case KurokageAbilityAction.HuntProtocol:
                case KurokageAbilityAction.ForgeDrive:
                case KurokageAbilityAction.WraithProtocol:
                    StartBuff(TemporarySpeedBuff(ability.Strength, ability.Duration, color));
                    break;

                case KurokageAbilityAction.Sanctuary:
                    SpawnField(7f, ability.Duration, 0f, 6f, color);
                    AddArmor(18f, color);
                    break;

                case KurokageAbilityAction.VeilCloak:
                    StartBuff(CloakRoutine(ability.Strength, ability.Duration, color));
                    break;

                default:
                    SpawnField(5f, Mathf.Max(2f, ability.Duration), 2f, 0f, color);
                    break;
            }
        }

        private void StartMovement(IEnumerator routine)
        {
            if (movementRoutine != null) StopCoroutine(movementRoutine);
            movementRoutine = StartCoroutine(routine);
        }

        private void StartBuff(IEnumerator routine)
        {
            if (buffRoutine != null) StopCoroutine(buffRoutine);
            buffRoutine = StartCoroutine(routine);
        }

        private IEnumerator StepRoutine(float distance, float duration, Color color)
        {
            if (controller == null || !controller.enabled) yield break;
            Vector3 direction = ResolveMovementDirection();
            float elapsed = 0f;
            float speed = distance / Mathf.Max(0.01f, duration);
            while (elapsed < duration)
            {
                CollisionFlags flags = controller.Move(direction * speed * Time.deltaTime);
                SpawnTrail(transform.position + Vector3.up * 0.7f, color);
                if ((flags & CollisionFlags.Sides) != 0) break;
                elapsed += Time.deltaTime;
                yield return null;
            }
            hasRequestedDirection = false;
            movementRoutine = null;
        }

        private IEnumerator LeapRoutine(float forwardSpeed, float upSpeed, float duration, Color color)
        {
            if (controller == null || !controller.enabled) yield break;
            float elapsed = 0f;
            Vector3 forward = ResolveMovementDirection();
            float vertical = upSpeed;
            while (elapsed < duration)
            {
                vertical += Physics.gravity.y * Time.deltaTime;
                CollisionFlags flags = controller.Move((forward * forwardSpeed + Vector3.up * vertical) * Time.deltaTime);
                SpawnTrail(transform.position + Vector3.up * 0.5f, color);
                if ((flags & CollisionFlags.Above) != 0) vertical = Mathf.Min(vertical, 0f);
                if ((flags & CollisionFlags.Sides) != 0) forward = Vector3.zero;
                elapsed += Time.deltaTime;
                yield return null;
            }
            hasRequestedDirection = false;
            movementRoutine = null;
        }

        private Vector3 ResolveMovementDirection()
        {
            Vector3 input = hasRequestedDirection
                ? requestedDirection
                : transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
            if (input.sqrMagnitude < 0.01f) input = transform.forward;
            input.y = 0f;
            return input.normalized;
        }

        private IEnumerator TemporarySpeedBuff(float multiplier, float duration, Color color)
        {
            multiplier = Mathf.Max(1f, multiplier);
            if (fps != null)
            {
                fps.walkSpeed = baseWalk * multiplier;
                fps.sprintSpeed = baseSprint * multiplier;
                fps.crouchSpeed = baseCrouch * Mathf.Lerp(1f, multiplier, 0.55f);
            }
            if (tacticalBot != null)
                tacticalBot.moveSpeed = Mathf.Max(0.1f, baseBotMove) * multiplier;

            float end = Time.time + Mathf.Max(0.2f, duration);
            while (Time.time < end)
            {
                SpawnTrail(transform.position + Vector3.up * 0.65f, color);
                yield return new WaitForSeconds(0.12f);
            }

            if (fps != null)
            {
                fps.walkSpeed = baseWalk;
                fps.sprintSpeed = baseSprint;
                fps.crouchSpeed = baseCrouch;
            }
            if (tacticalBot != null && baseBotMove > 0f)
                tacticalBot.moveSpeed = baseBotMove;
            buffRoutine = null;
        }

        private IEnumerator CloakRoutine(float opacity, float duration, Color color)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            foreach (Renderer renderer in renderers)
            {
                renderer.GetPropertyBlock(block);
                Color cloak = new Color(color.r, color.g, color.b, Mathf.Clamp(opacity, 0.15f, 0.8f));
                block.SetColor("_Color", cloak);
                block.SetColor("_BaseColor", cloak);
                block.SetColor("_EmissionColor", color * 0.8f);
                renderer.SetPropertyBlock(block);
            }

            yield return TemporarySpeedBuff(1.12f, duration, color);

            foreach (Renderer renderer in renderers)
                renderer.SetPropertyBlock(null);
            buffRoutine = null;
        }

        private void HealAllies(float amount, float radius, Color color)
        {
            foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (player == null || !player.isAlive || roundPlayer == null || player.team != roundPlayer.team) continue;
                if (Vector3.Distance(transform.position, player.transform.position) > radius) continue;
                player.health = Mathf.Min(player.maxHealth, player.health + amount);
                SpawnBurst(player.transform.position + Vector3.up * 0.8f, color, 0.22f, 0.45f);
            }
        }

        private void AddArmor(float amount, Color color)
        {
            if (armor == null) armor = GetComponent<KurokageArmor>();
            if (armor != null) armor.SetArmor(armor.CurrentArmor + amount);
            SpawnBurst(transform.position + Vector3.up * 0.9f, color, 0.42f, 0.6f);
        }

        private void DamageEnemies(float amount, float radius, Color color)
        {
            if (roundPlayer == null) return;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Collide);
            HashSet<RenkaiRoundPlayer> processed = new HashSet<RenkaiRoundPlayer>();
            foreach (Collider hit in hits)
            {
                RenkaiRoundPlayer enemy = hit.GetComponentInParent<RenkaiRoundPlayer>();
                if (enemy == null || !enemy.isAlive || enemy.team == roundPlayer.team || !processed.Add(enemy)) continue;
                KurokageDamageInfo info = new KurokageDamageInfo(
                    amount,
                    roundPlayer,
                    enemy.transform.position,
                    Vector3.up,
                    KurokageDamageType.Ability,
                    KurokageHitZoneType.Torso,
                    identity.Definition.Callsign + "_ABILITY"
                );
                enemy.ApplyDamage(info);
                SpawnBurst(enemy.transform.position + Vector3.up * 0.9f, color, 0.28f, 0.45f);
            }
        }

        private void PulseEnemies(float radius, Color color)
        {
            if (roundPlayer == null) return;
            foreach (RenkaiRoundPlayer enemy in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (enemy == null || !enemy.isAlive || enemy.team == roundPlayer.team) continue;
                if (Vector3.Distance(transform.position, enemy.transform.position) > radius) continue;
                Vector3 origin = transform.position + Vector3.up * 1.4f;
                Vector3 target = enemy.transform.position + Vector3.up * 1.1f;
                if (Physics.Linecast(origin, target, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore) &&
                    hit.collider.GetComponentInParent<RenkaiRoundPlayer>() != enemy)
                    continue;
                SpawnBurst(enemy.transform.position + Vector3.up * 1.1f, color, 0.18f, 0.65f);
            }
        }

        private void SpawnField(float radius, float duration, float damage, float healing, Color color)
        {
            GameObject field = new GameObject(identity.Definition.Callsign + "_FIELD");
            field.transform.position = transform.position;
            KurokageAbilityField runtime = field.AddComponent<KurokageAbilityField>();
            runtime.Configure(roundPlayer, radius, duration, damage, healing, color);
        }

        private void SpawnDecoy(float duration, float speed, Color color)
        {
            Transform visual = transform.Find("AGENT_VISUAL");
            if (visual == null) return;
            GameObject clone = Instantiate(visual.gameObject, transform.position + transform.forward * 0.8f, transform.rotation);
            clone.name = identity.Definition.Callsign + "_HOLOGRAM";

            foreach (MonoBehaviour behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;
            foreach (Collider collider in clone.GetComponentsInChildren<Collider>(true))
                Destroy(collider);

            KurokageVisualDecoy decoy = clone.AddComponent<KurokageVisualDecoy>();
            decoy.Configure(ResolveMovementDirection(), speed, duration, color);
        }

        private void SpawnBurst(Vector3 position, Color color, float size, float lifetime)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "RENKAI_AGENT_BURST",
                position,
                Quaternion.identity,
                Vector3.one * size,
                color,
                2.2f,
                lifetime
            );
        }

        private void SpawnTrail(Vector3 position, Color color)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cube,
                "RENKAI_AGENT_TRAIL",
                position,
                transform.rotation,
                new Vector3(0.12f, 0.42f, 0.08f),
                color,
                1.8f,
                0.12f
            );
        }
    }
}
