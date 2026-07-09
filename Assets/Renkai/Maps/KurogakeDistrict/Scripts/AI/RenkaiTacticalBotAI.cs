using UnityEngine;

namespace Renkai.Kurogake
{
    public enum RenkaiBotState
    {
        Hold,
        Patrol,
        Investigate,
        Engage,
        RotateToBomb,
        Plant,
        Defuse,
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
        public float hearingRadius = 18f;
        public float memoryTime = 5.5f;
        public float reactionDelay = 0.22f;
        public LayerMask sightMask = ~0;

        [Header("Combat")]
        public float fireDistance = 34f;
        public float fireRate = 2.7f;
        public float damage = 9f;
        public float accuracy = 0.72f;
        public float burstLength = 0.55f;
        public float burstCooldown = 0.65f;
        public Color tracerColor = new Color(1f, 0.22f, 0.45f);

        [Header("Movement")]
        public float moveSpeed = 3.2f;
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
        private int routeIndex;
        private float seed;

        private void Start()
        {
            self = GetComponent<RenkaiRoundPlayer>();
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

            RenkaiBombCore bomb = Object.FindObjectOfType<RenkaiBombCore>();
            if (bomb != null && bomb.planted)
            {
                if (team == RenkaiTeam.Defenders)
                    EnterState(RenkaiBotState.RotateToBomb);
                else if (team == RenkaiTeam.Attackers && state != RenkaiBotState.Engage)
                    EnterState(RenkaiBotState.Hold);

                return;
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
                    if (ReachedGoal())
                        PickNextRouteGoal();
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

                case RenkaiBotState.RotateToBomb:
                    RotateToBomb();
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
            Vector3 hold = spawnAnchor + offset;
            transform.position = Vector3.Lerp(transform.position, new Vector3(hold.x, transform.position.y, hold.z), Time.deltaTime * 0.8f);
            LookTowardLikelyEnemy();
        }

        private void EngageTarget()
        {
            if (target == null || !target.isAlive)
            {
                if (Time.time - lastSeenEnemyTime < memoryTime)
                    EnterState(RenkaiBotState.Investigate);
                else
                    EnterState(team == RenkaiTeam.Attackers ? RenkaiBotState.Patrol : RenkaiBotState.Hold);
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

            if (Time.time >= nextFireTime)
                FireAtTarget(target);
        }

        private void RotateToBomb()
        {
            RenkaiBombCore bomb = Object.FindObjectOfType<RenkaiBombCore>();
            if (bomb == null || !bomb.planted)
            {
                EnterState(RenkaiBotState.Hold);
                return;
            }

            currentGoal = bomb.transform.position + new Vector3(Mathf.Sin(seed) * 2f, 0f, Mathf.Cos(seed) * 2f);
            MoveToGoal();

            if (Vector3.Distance(transform.position, bomb.transform.position) < 7f)
            {
                FacePosition(bomb.transform.position);
                if (target == null)
                    EnterState(RenkaiBotState.Hold);
            }
        }

        private void CombatStrafe()
        {
            Vector3 side = transform.right * Mathf.Sin(Time.time * 1.2f + seed) * strafeDistance;
            Vector3 desired = spawnAnchor + side;

            if (target != null)
            {
                Vector3 away = (transform.position - target.transform.position);
                away.y = 0f;
                if (away.magnitude < 5f)
                    desired += away.normalized * 2f;
            }

            transform.position = Vector3.Lerp(transform.position, new Vector3(desired.x, transform.position.y, desired.z), Time.deltaTime * moveSpeed);
        }

        private void MoveToGoal()
        {
            Vector3 dir = currentGoal - transform.position;
            dir.y = 0f;

            if (dir.magnitude <= stoppingDistance) return;

            FacePosition(currentGoal);

            Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
            transform.position += step;
        }

        private bool ReachedGoal()
        {
            Vector3 a = transform.position;
            Vector3 b = currentGoal;
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b) <= stoppingDistance;
        }

        private void PickInitialGoal()
        {
            if (team == RenkaiTeam.Attackers)
                PickNextRouteGoal();
            else
                currentGoal = spawnAnchor;
        }

        private void PickNextRouteGoal()
        {
            Vector3[] attackRoute;

            if (role == RenkaiAgentRole.Duelist || role == RenkaiAgentRole.Blade)
            {
                attackRoute = new Vector3[]
                {
                    new Vector3(0f, transform.position.y, -30f),
                    new Vector3(0f, transform.position.y, -4f),
                    new Vector3(-22f, transform.position.y, 3f),
                    new Vector3(-42f, transform.position.y, 15f)
                };
            }
            else if (role == RenkaiAgentRole.Controller)
            {
                attackRoute = new Vector3[]
                {
                    new Vector3(36f, transform.position.y, -42f),
                    new Vector3(44f, transform.position.y, -20f),
                    new Vector3(42f, transform.position.y, 15f)
                };
            }
            else
            {
                attackRoute = new Vector3[]
                {
                    new Vector3(-36f, transform.position.y, -42f),
                    new Vector3(-44f, transform.position.y, -20f),
                    new Vector3(-42f, transform.position.y, 15f)
                };
            }

            currentGoal = attackRoute[routeIndex % attackRoute.Length];
            routeIndex++;
        }

        private void LookTowardLikelyEnemy()
        {
            Vector3 lookTarget = team == RenkaiTeam.Attackers ? new Vector3(0f, transform.position.y, 15f) : new Vector3(0f, transform.position.y, -35f);
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
            float bestDistance = 9999f;

            foreach (RenkaiRoundPlayer candidate in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (candidate == null || !candidate.isAlive || candidate.team == team)
                    continue;

                float d = Vector3.Distance(transform.position, candidate.transform.position);
                if (d > viewDistance) continue;

                if (!InsideFov(candidate.transform.position)) continue;
                if (!CanSee(candidate)) continue;

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

            float angle = Vector3.Angle(transform.forward, dir.normalized);
            return angle <= fieldOfView * 0.5f;
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
            nextFireTime = Time.time + 1f / fireRate + Random.Range(0f, burstCooldown);

            if (victim == null || !victim.isAlive || muzzle == null) return;

            Vector3 start = muzzle.position;
            Vector3 aim = victim.transform.position + Vector3.up * 1.1f;

            // Human-like imperfect aim.
            float miss = (1f - accuracy) * 2.2f;
            aim += new Vector3(Random.Range(-miss, miss), Random.Range(-miss * 0.4f, miss * 0.4f), Random.Range(-miss, miss));

            if (Physics.Raycast(start, (aim - start).normalized, out RaycastHit hit, fireDistance, sightMask, QueryTriggerInteraction.Ignore))
            {
                RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (hitPlayer != null && hitPlayer.team != team && hitPlayer.isAlive)
                    hitPlayer.TakeDamage(damage, self);
            }

            SpawnTracer(start, aim);
        }

        private void SpawnTracer(Vector3 start, Vector3 end)
        {
            if (Vector3.Distance(start, end) < 0.1f) return;

            GameObject tracer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(tracer.GetComponent<Collider>());

            Vector3 mid = (start + end) * 0.5f;
            float len = Vector3.Distance(start, end);

            tracer.transform.position = mid;
            tracer.transform.rotation = Quaternion.LookRotation(end - start);
            tracer.transform.localScale = new Vector3(0.03f, 0.03f, len);

            Material mat = MakeEmission(tracerColor, tracerColor * 2.7f);
            tracer.GetComponent<Renderer>().sharedMaterial = mat;

            Object.Destroy(tracer, 0.065f);
        }

        private Material MakeEmission(Color color, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);

            return mat;
        }

        private void EnterState(RenkaiBotState newState)
        {
            if (state == newState) return;

            state = newState;
            stateEnteredTime = Time.time;
        }
    }
}
