using UnityEngine;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    public enum RenkaiBotState
    {
        Hold,
        Patrol,
        Investigate,
        Engage,
        RotateToCore,
        GuardLink,
        Sever,
        Retreat
    }

    public enum RenkaiAgentRole
    {
        Duelist,
        Controller,
        Sentinel,
        Initiator,
        Blade
    }

    public class RenkaiTacticalBotAI : MonoBehaviour
    {
        [Header("Identity")]
        public RenkaiTeam team;
        public RenkaiAgentRole role = RenkaiAgentRole.Duelist;
        public string callSign = "BOT";

        [Header("Perception")]
        public float viewDistance = 42f;
        public float fieldOfView = 105f;
        public float memoryTime = 5.5f;
        public float reactionDelay = 0.22f;
        public LayerMask sightMask = ~0;

        [Header("Combat")]
        public float fireDistance = 34f;
        public float fireRate = 4.4f;
        public float damage = 12f;
        [Range(0.1f, 1f)] public float accuracy = 0.72f;
        public float burstCooldown = 0.34f;
        public Color tracerColor = new Color(1f, 0.22f, 0.45f);

        [Header("Movement")]
        public float moveSpeed = 3.8f;
        public float rotateSpeed = 9f;
        public float stoppingDistance = 1.1f;
        public float strafeDistance = 2.4f;
        public bool canAdvance = true;

        [Header("Runtime")]
        public RenkaiBotState state = RenkaiBotState.Hold;
        public Transform muzzle;

        private Vector3 spawnAnchor;
        private Vector3 currentGoal;
        private Vector3 lastKnownEnemyPosition;
        private float lastSeenEnemyTime = -999f;
        private float nextFireTime;
        private float nextThinkTime;
        private float stateEnteredTime;
        private RenkaiRoundPlayer target;
        private RenkaiRoundPlayer self;
        private CharacterController controller;
        private ZodiacCoreRuntime core;
        private int routeIndex;
        private float seed;

        private void Start()
        {
            self = GetComponent<RenkaiRoundPlayer>();
            controller = GetComponent<CharacterController>();
            core = Object.FindObjectOfType<ZodiacCoreRuntime>();

            if (self != null)
            {
                team = self.team;
                callSign = self.agentName;
            }

            spawnAnchor = transform.position;
            currentGoal = spawnAnchor;
            seed = Random.Range(0f, 100f);

            if (muzzle == null)
            {
                GameObject m = new GameObject("TacticalBot_Muzzle");
                m.transform.SetParent(transform);
                m.transform.localPosition = new Vector3(0f, 1.35f, 0.65f);
                muzzle = m.transform;
            }

            EnterState(team == RenkaiTeam.Attackers ? RenkaiBotState.Patrol : RenkaiBotState.Hold);
            PickInitialGoal();
        }

        private void Update()
        {
            if (self == null) self = GetComponent<RenkaiRoundPlayer>();
            if (self == null || !self.isAlive) return;
            if (core == null) core = Object.FindObjectOfType<ZodiacCoreRuntime>();

            if (Time.time >= nextThinkTime)
            {
                nextThinkTime = Time.time + 0.18f;
                Think();
            }

            ExecuteState();
        }

        private void Think()
        {
            target = FindVisibleEnemy();

            if (target != null)
            {
                lastKnownEnemyPosition = target.transform.position;
                lastSeenEnemyTime = Time.time;
                if (Time.time - stateEnteredTime >= reactionDelay)
                    EnterState(RenkaiBotState.Engage);
                return;
            }

            if (core != null)
            {
                if (core.State == ZodiacLinkState.Synchronized || core.State == ZodiacLinkState.Severing)
                {
                    EnterState(team == RenkaiTeam.Defenders ? RenkaiBotState.RotateToCore : RenkaiBotState.GuardLink);
                    return;
                }

                if (core.State == ZodiacLinkState.Linking)
                {
                    EnterState(team == RenkaiTeam.Defenders ? RenkaiBotState.RotateToCore : RenkaiBotState.GuardLink);
                    return;
                }
            }

            if (Time.time - lastSeenEnemyTime < memoryTime && state != RenkaiBotState.Engage)
            {
                EnterState(RenkaiBotState.Investigate);
                return;
            }

            if (team == RenkaiTeam.Attackers && (state == RenkaiBotState.Hold || ReachedGoal()))
                EnterState(RenkaiBotState.Patrol);

            if (team == RenkaiTeam.Defenders && ReachedGoal())
                EnterState(RenkaiBotState.Hold);
        }

        private void ExecuteState()
        {
            switch (state)
            {
                case RenkaiBotState.Hold:
                    HoldArea();
                    break;
                case RenkaiBotState.Patrol:
                    MoveToGoal();
                    if (ReachedGoal()) PickNextRouteGoal();
                    break;
                case RenkaiBotState.Investigate:
                    currentGoal = lastKnownEnemyPosition;
                    MoveToGoal();
                    if (ReachedGoal() || Time.time - lastSeenEnemyTime > memoryTime)
                        EnterState(team == RenkaiTeam.Attackers ? RenkaiBotState.Patrol : RenkaiBotState.Hold);
                    break;
                case RenkaiBotState.Engage:
                    EngageTarget();
                    break;
                case RenkaiBotState.RotateToCore:
                    RotateToCore();
                    break;
                case RenkaiBotState.GuardLink:
                    GuardLinkedCore();
                    break;
                case RenkaiBotState.Sever:
                    HoldNearCore(2.2f);
                    break;
                case RenkaiBotState.Retreat:
                    currentGoal = spawnAnchor;
                    MoveToGoal();
                    break;
            }
        }

        private void HoldArea()
        {
            Vector3 offset = transform.right * Mathf.Sin(Time.time * 0.7f + seed) * 0.45f;
            MoveToward(spawnAnchor + offset, 0.8f);
            LookTowardLikelyEnemy();
        }

        private void EngageTarget()
        {
            if (target == null || !target.isAlive)
            {
                EnterState(Time.time - lastSeenEnemyTime < memoryTime ? RenkaiBotState.Investigate : RenkaiBotState.Hold);
                return;
            }

            FacePosition(target.transform.position + Vector3.up * 1.1f);
            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance > fireDistance || !CanSee(target))
            {
                lastKnownEnemyPosition = target.transform.position;
                EnterState(RenkaiBotState.Investigate);
                return;
            }

            CombatStrafe();
            if (Time.time >= nextFireTime) FireAtTarget(target);
        }

        private void RotateToCore()
        {
            if (core == null)
            {
                EnterState(RenkaiBotState.Hold);
                return;
            }

            currentGoal = core.transform.position + ObjectiveOffset(2.3f);
            MoveToGoal();

            if (Vector3.Distance(transform.position, core.transform.position) <= 4.2f)
            {
                FacePosition(core.transform.position);
                if (core.State == ZodiacLinkState.Synchronized)
                    EnterState(RenkaiBotState.Sever);
            }
        }

        private void GuardLinkedCore()
        {
            if (core == null)
            {
                EnterState(RenkaiBotState.Patrol);
                return;
            }

            HoldNearCore(5.5f);
            LookTowardLikelyEnemy();
        }

        private void HoldNearCore(float radius)
        {
            if (core == null) return;
            Vector3 desired = core.transform.position + ObjectiveOffset(radius);
            MoveToward(desired, moveSpeed);
            FacePosition(core.transform.position);
        }

        private Vector3 ObjectiveOffset(float radius)
        {
            return new Vector3(Mathf.Sin(seed * 0.73f) * radius, 0f, Mathf.Cos(seed * 0.73f) * radius);
        }

        private void CombatStrafe()
        {
            if (target == null) return;

            Vector3 side = transform.right * Mathf.Sin(Time.time * 1.2f + seed) * strafeDistance;
            Vector3 desired = transform.position + side * Time.deltaTime;

            Vector3 away = transform.position - target.transform.position;
            away.y = 0f;
            if (away.magnitude < 5f) desired += away.normalized * 2f * Time.deltaTime;

            MoveToward(desired, moveSpeed);
        }

        private void MoveToGoal()
        {
            Vector3 dir = currentGoal - transform.position;
            dir.y = 0f;
            if (dir.magnitude <= stoppingDistance) return;
            FacePosition(currentGoal);
            MoveToward(currentGoal, moveSpeed);
        }

        private void MoveToward(Vector3 goal, float speed)
        {
            Vector3 dir = goal - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            Vector3 motion = dir.normalized * speed * Time.deltaTime;
            if (controller != null && controller.enabled)
                controller.Move(motion + Vector3.down * 1.5f * Time.deltaTime);
            else
                transform.position += motion;
        }

        private bool ReachedGoal()
        {
            Vector3 a = transform.position;
            Vector3 b = currentGoal;
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b) <= stoppingDistance;
        }

        private void PickInitialGoal()
        {
            if (team == RenkaiTeam.Attackers) PickNextRouteGoal();
            else currentGoal = spawnAnchor;
        }

        private void PickNextRouteGoal()
        {
            Vector3[] route;

            if (role == RenkaiAgentRole.Duelist || role == RenkaiAgentRole.Blade)
            {
                route = new[]
                {
                    new Vector3(0f, transform.position.y, -30f),
                    new Vector3(0f, transform.position.y, -4f),
                    new Vector3(-22f, transform.position.y, 3f),
                    new Vector3(-42f, transform.position.y, 15f)
                };
            }
            else if (role == RenkaiAgentRole.Controller)
            {
                route = new[]
                {
                    new Vector3(36f, transform.position.y, -42f),
                    new Vector3(44f, transform.position.y, -20f),
                    new Vector3(42f, transform.position.y, 15f)
                };
            }
            else
            {
                route = new[]
                {
                    new Vector3(-36f, transform.position.y, -42f),
                    new Vector3(-44f, transform.position.y, -20f),
                    new Vector3(-42f, transform.position.y, 15f)
                };
            }

            currentGoal = route[routeIndex % route.Length];
            routeIndex++;
        }

        private void LookTowardLikelyEnemy()
        {
            Vector3 lookTarget = team == RenkaiTeam.Attackers
                ? new Vector3(0f, transform.position.y, 15f)
                : new Vector3(0f, transform.position.y, -35f);
            FacePosition(lookTarget);
        }

        private void FacePosition(Vector3 position)
        {
            Vector3 dir = position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotateSpeed);
        }

        private RenkaiRoundPlayer FindVisibleEnemy()
        {
            RenkaiRoundPlayer best = null;
            float bestDistance = float.MaxValue;

            foreach (RenkaiRoundPlayer candidate in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (candidate == null || !candidate.isAlive || candidate.team == team) continue;
                float d = Vector3.Distance(transform.position, candidate.transform.position);
                if (d > viewDistance || !InsideFov(candidate.transform.position) || !CanSee(candidate)) continue;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = candidate;
                }
            }

            return best;
        }

        private bool InsideFov(Vector3 point)
        {
            Vector3 dir = point - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude <= 0.01f) return true;
            return Vector3.Angle(transform.forward, dir.normalized) <= fieldOfView * 0.5f;
        }

        private bool CanSee(RenkaiRoundPlayer candidate)
        {
            if (muzzle == null || candidate == null) return false;
            Vector3 start = muzzle.position;
            Vector3 end = candidate.transform.position + Vector3.up * 1.1f;
            Vector3 dir = (end - start).normalized;

            if (Physics.Raycast(start, dir, out RaycastHit hit, viewDistance, sightMask, QueryTriggerInteraction.Ignore))
            {
                RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                return hitPlayer != null && hitPlayer == candidate;
            }

            return false;
        }

        private void FireAtTarget(RenkaiRoundPlayer victim)
        {
            nextFireTime = Time.time + 1f / Mathf.Max(0.1f, fireRate) + Random.Range(0f, burstCooldown);
            if (victim == null || !victim.isAlive || muzzle == null) return;

            Vector3 start = muzzle.position;
            Vector3 aim = victim.transform.position + Vector3.up * 1.1f;
            float miss = (1f - accuracy) * 2.2f;
            aim += new Vector3(Random.Range(-miss, miss), Random.Range(-miss * 0.4f, miss * 0.4f), Random.Range(-miss, miss));

            Vector3 end = aim;
            if (Physics.Raycast(start, (aim - start).normalized, out RaycastHit hit, fireDistance, sightMask, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (hitPlayer != null && hitPlayer.team != team && hitPlayer.isAlive)
                    hitPlayer.TakeDamage(damage, self);
            }

            SpawnTracer(start, end);
        }

        private void SpawnTracer(Vector3 start, Vector3 end)
        {
            if (Vector3.Distance(start, end) < 0.1f) return;

            GameObject tracer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(tracer.GetComponent<Collider>());
            tracer.name = "Kurokage_BotTracer";

            Vector3 mid = (start + end) * 0.5f;
            float len = Vector3.Distance(start, end);
            tracer.transform.position = mid;
            tracer.transform.rotation = Quaternion.LookRotation(end - start);
            tracer.transform.localScale = new Vector3(0.018f, 0.018f, len);

            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(shader);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tracerColor);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tracerColor);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", tracerColor * 2.2f);
            }
            tracer.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(tracer, 0.055f);
        }

        private void EnterState(RenkaiBotState newState)
        {
            if (state == newState) return;
            state = newState;
            stateEnteredTime = Time.time;
        }
    }
}
