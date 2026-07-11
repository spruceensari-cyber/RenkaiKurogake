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

    [RequireComponent(typeof(RenkaiRoundPlayer))]
    public class RenkaiTacticalBotAI : MonoBehaviour
    {
        [Header("Identity")]
        public RenkaiTeam team;
        public RenkaiAgentRole role = RenkaiAgentRole.Duelist;
        public string callSign = "BOT";

        [Header("Perception")]
        public float viewDistance = 42f;
        public float fieldOfView = 105f;
        public float memoryTime = 4.2f;
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

        public bool IsReloading => weaponState != null && weaponState.IsReloading;
        public int MagazineAmmo => weaponState != null ? weaponState.MagazineAmmo : 0;

        private Vector3 spawnAnchor;
        private Vector3 currentGoal;
        private float nextFireTime;
        private float nextThinkTime;
        private float nextRosterRefresh;
        private float stateEnteredTime;
        private RenkaiRoundPlayer target;
        private RenkaiRoundPlayer self;
        private RenkaiRoundPlayer[] cachedPlayers;
        private CharacterController controller;
        private ZodiacCoreRuntime core;
        private KurokageBotPerception perception;
        private KurokageBotAutonomyMotor autonomy;
        private KurokageBotWeaponState weaponState;
        private KurokageBotWeaponPose weaponPose;
        private int routeIndex;
        private float seed;

        private void Start()
        {
            self = GetComponent<RenkaiRoundPlayer>();
            controller = GetComponent<CharacterController>();
            core = Object.FindObjectOfType<ZodiacCoreRuntime>();
            perception = GetComponent<KurokageBotPerception>();
            if (perception == null) perception = gameObject.AddComponent<KurokageBotPerception>();
            autonomy = GetComponent<KurokageBotAutonomyMotor>();
            if (autonomy == null) autonomy = gameObject.AddComponent<KurokageBotAutonomyMotor>();
            weaponState = GetComponent<KurokageBotWeaponState>();
            if (weaponState == null) weaponState = gameObject.AddComponent<KurokageBotWeaponState>();
            weaponPose = GetComponent<KurokageBotWeaponPose>();

            if (self != null)
            {
                team = self.team;
                callSign = self.agentName;
            }

            spawnAnchor = transform.position;
            currentGoal = spawnAnchor;
            seed = Random.Range(0f, 100f);
            RefreshRoster();
            EnsureMuzzle();
            perception.Configure(muzzle, viewDistance, fieldOfView, memoryTime, sightMask);

            EnterState(team == RenkaiTeam.Attackers ? RenkaiBotState.Patrol : RenkaiBotState.Hold);
            PickInitialGoal();
        }

        private void Update()
        {
            if (self == null) self = GetComponent<RenkaiRoundPlayer>();
            if (self == null || !self.isAlive) return;
            if (core == null) core = Object.FindObjectOfType<ZodiacCoreRuntime>();
            if (perception == null)
            {
                perception = GetComponent<KurokageBotPerception>();
                if (perception == null) perception = gameObject.AddComponent<KurokageBotPerception>();
                perception.Configure(muzzle, viewDistance, fieldOfView, memoryTime, sightMask);
            }
            if (autonomy == null) autonomy = GetComponent<KurokageBotAutonomyMotor>();
            if (weaponState == null) weaponState = GetComponent<KurokageBotWeaponState>();
            if (weaponPose == null) weaponPose = GetComponent<KurokageBotWeaponPose>();

            if (Time.time >= nextRosterRefresh) RefreshRoster();
            if (Time.time >= nextThinkTime)
            {
                nextThinkTime = Time.time + 0.18f;
                Think();
            }

            ExecuteState();
        }

        public void ResetCombatState()
        {
            target = null;
            nextFireTime = 0f;
            nextThinkTime = 0f;
            routeIndex = 0;
            currentGoal = spawnAnchor;
            if (weaponState != null) weaponState.ResetState();
            if (autonomy != null) autonomy.ResetMotor();
            EnterState(team == RenkaiTeam.Attackers ? RenkaiBotState.Patrol : RenkaiBotState.Hold);
            PickInitialGoal();
        }

        private void EnsureMuzzle()
        {
            if (muzzle != null) return;
            Transform existing = transform.Find("TacticalBot_Muzzle");
            if (existing != null)
            {
                muzzle = existing;
                return;
            }

            GameObject muzzleObject = new GameObject("TacticalBot_Muzzle");
            muzzleObject.transform.SetParent(transform, false);
            muzzleObject.transform.localPosition = new Vector3(0f, 1.35f, 0.65f);
            muzzle = muzzleObject.transform;
        }

        private void RefreshRoster()
        {
            cachedPlayers = Object.FindObjectsOfType<RenkaiRoundPlayer>(true);
            nextRosterRefresh = Time.time + 1f;
        }

        private void Think()
        {
            target = perception.FindBestVisibleEnemy(cachedPlayers, team);

            if (target != null)
            {
                if (state != RenkaiBotState.Engage)
                    EnterState(RenkaiBotState.Engage);
                return;
            }

            if (state == RenkaiBotState.Engage)
            {
                target = null;
                EnterState(perception.HasMemory ? RenkaiBotState.Investigate : DefaultIdleState());
                return;
            }

            if (core != null && (core.State == ZodiacLinkState.Linking || core.State == ZodiacLinkState.Synchronized || core.State == ZodiacLinkState.Severing))
            {
                EnterState(team == RenkaiTeam.Defenders ? RenkaiBotState.RotateToCore : RenkaiBotState.GuardLink);
                return;
            }

            if (perception.HasMemory)
            {
                EnterState(RenkaiBotState.Investigate);
                return;
            }

            if (team == RenkaiTeam.Attackers && (state == RenkaiBotState.Hold || ReachedGoal()))
                EnterState(RenkaiBotState.Patrol);
            else if (team == RenkaiTeam.Defenders && ReachedGoal())
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
                    InvestigateMemory();
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

        private void InvestigateMemory()
        {
            if (!perception.HasMemory)
            {
                target = null;
                EnterState(DefaultIdleState());
                return;
            }

            currentGoal = perception.LastKnownPosition;
            MoveToGoal();
            if (ReachedGoal()) FacePosition(perception.LastKnownPosition);
        }

        private void EngageTarget()
        {
            if (target == null || !target.isAlive)
            {
                target = null;
                EnterState(perception.HasMemory ? RenkaiBotState.Investigate : DefaultIdleState());
                return;
            }

            if (!perception.IsCurrentlyVisible(target))
            {
                target = null;
                EnterState(perception.HasMemory ? RenkaiBotState.Investigate : DefaultIdleState());
                return;
            }

            FacePosition(target.transform.position + Vector3.up * 1.1f);
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > fireDistance)
            {
                if (canAdvance) MoveToward(target.transform.position, moveSpeed);
                return;
            }

            CombatStrafe();
            if (Time.time >= nextFireTime && Time.time - stateEnteredTime >= reactionDelay)
                FireAtTarget(target);
        }

        private void RotateToCore()
        {
            if (core == null)
            {
                EnterState(DefaultIdleState());
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
                EnterState(DefaultIdleState());
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
            Vector3 direction = currentGoal - transform.position;
            direction.y = 0f;
            if (direction.magnitude <= stoppingDistance) return;
            FacePosition(currentGoal);
            MoveToward(currentGoal, moveSpeed);
        }

        private void MoveToward(Vector3 goal, float speed)
        {
            Vector3 direction = goal - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;

            Vector3 planarDirection = direction.normalized;
            if (autonomy != null) planarDirection = autonomy.AdjustPlanarDirection(planarDirection);
            Vector3 motion = planarDirection * speed * Time.deltaTime;
            if (controller != null && controller.enabled)
                controller.Move(motion);
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
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            Quaternion rotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
        }

        private void FireAtTarget(RenkaiRoundPlayer victim)
        {
            if (victim == null || !victim.isAlive || muzzle == null) return;
            if (!perception.IsCurrentlyVisible(victim)) return;
            if (weaponState != null && !weaponState.TryConsumeRound())
            {
                nextFireTime = Time.time + 0.12f;
                return;
            }

            nextFireTime = Time.time + 1f / Mathf.Max(0.1f, fireRate) + Random.Range(0f, burstCooldown);
            Vector3 start = muzzle.position;
            Vector3 idealAim = victim.transform.position + Vector3.up * 1.1f;
            float miss = (1f - accuracy) * 2.2f;
            Vector3 finalAim = idealAim + new Vector3(
                Random.Range(-miss, miss),
                Random.Range(-miss * 0.4f, miss * 0.4f),
                Random.Range(-miss, miss)
            );

            if (!perception.TryGetClearShot(victim, fireDistance, finalAim, out RaycastHit hit))
                return;

            Vector3 end = hit.point;
            KurokageDecoyHitReceiver decoy = hit.collider.GetComponentInParent<KurokageDecoyHitReceiver>();
            if (decoy != null)
            {
                decoy.Hit(hit.point, hit.normal);
            }
            else
            {
                RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (hitPlayer != null && hitPlayer.team != team && hitPlayer.isAlive)
                {
                    KurokageDamageInfo info = new KurokageDamageInfo(
                        damage,
                        self,
                        hit.point,
                        hit.normal,
                        KurokageDamageType.Ballistic,
                        KurokageHitZoneType.Torso,
                        "BOT_RIFLE"
                    );
                    hitPlayer.ApplyDamage(info);
                }
            }

            SpawnTracer(start, end);
            SpawnMuzzleFlash(start);
            if (weaponPose != null) weaponPose.AddRecoil();
        }

        private void SpawnTracer(Vector3 start, Vector3 end)
        {
            if (Vector3.Distance(start, end) < 0.1f) return;
            Vector3 midpoint = (start + end) * 0.5f;
            float length = Vector3.Distance(start, end);
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cube,
                "KUROKAGE_BOT_TRACER",
                midpoint,
                Quaternion.LookRotation(end - start),
                new Vector3(0.012f, 0.012f, length),
                tracerColor,
                1.8f,
                0.05f
            );
        }

        private void SpawnMuzzleFlash(Vector3 position)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "KUROKAGE_BOT_MUZZLE",
                position,
                transform.rotation,
                Vector3.one * 0.07f,
                tracerColor,
                2.4f,
                0.035f
            );
        }

        private RenkaiBotState DefaultIdleState()
        {
            return team == RenkaiTeam.Attackers ? RenkaiBotState.Patrol : RenkaiBotState.Hold;
        }

        private void EnterState(RenkaiBotState newState)
        {
            if (state == newState) return;
            state = newState;
            stateEnteredTime = Time.time;
        }
    }
}
