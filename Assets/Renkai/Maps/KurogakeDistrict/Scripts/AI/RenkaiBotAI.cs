using UnityEngine;

namespace Renkai.Kurogake
{
    public class RenkaiBotAI : MonoBehaviour
    {
        [Header("Combat")]
        public RenkaiTeam team;
        public float viewDistance = 42f;
        public float fireDistance = 32f;
        public float fireRate = 2.4f;
        public float damage = 9f;
        public float aimHeight = 1.1f;

        [Header("Movement")]
        public float moveSpeed = 2.6f;
        public float strafeRadius = 2.4f;
        public float pushTowardObjective = 0.35f;

        [Header("Visual")]
        public Transform muzzle;
        public Color tracerColor = new Color(1f, 0.2f, 0.45f);

        private float nextFireTime;
        private Vector3 anchor;
        private float seed;
        private Transform target;

        private void Start()
        {
            anchor = transform.position;
            seed = Random.Range(0f, 100f);

            if (muzzle == null)
            {
                GameObject m = new GameObject("Bot_Muzzle");
                m.transform.SetParent(transform);
                m.transform.localPosition = new Vector3(0f, 1.35f, 0.65f);
                muzzle = m.transform;
            }
        }

        private void Update()
        {
            RenkaiRoundPlayer self = GetComponent<RenkaiRoundPlayer>();
            if (self == null || !self.isAlive) return;

            AcquireTarget();

            if (target == null)
            {
                PatrolObjective();
                return;
            }

            FaceTarget();
            CombatStrafe();

            if (Vector3.Distance(transform.position, target.position) <= fireDistance && HasLineOfSight())
            {
                if (Time.time >= nextFireTime)
                    FireAtTarget();
            }
        }

        private void AcquireTarget()
        {
            target = null;
            float best = 9999f;

            foreach (RenkaiRoundPlayer p in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (p == null || !p.isAlive || p.team == team) continue;

                float distance = Vector3.Distance(transform.position, p.transform.position);
                if (distance < best && distance <= viewDistance)
                {
                    best = distance;
                    target = p.transform;
                }
            }
        }

        private void FaceTarget()
        {
            if (target == null) return;

            Vector3 dir = target.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 7f);
            }
        }

        private void CombatStrafe()
        {
            float t = Time.time + seed;
            Vector3 side = transform.right * Mathf.Sin(t * 1.1f) * strafeRadius;
            Vector3 targetPos = anchor + side;
            transform.position = Vector3.Lerp(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), Time.deltaTime * moveSpeed);
        }

        private void PatrolObjective()
        {
            Vector3 objective = team == RenkaiTeam.Attackers ? new Vector3(0f, transform.position.y, 10f) : anchor;
            Vector3 desired = Vector3.Lerp(anchor, objective, pushTowardObjective);
            desired += transform.right * Mathf.Sin(Time.time + seed) * 1.5f;
            transform.position = Vector3.Lerp(transform.position, new Vector3(desired.x, transform.position.y, desired.z), Time.deltaTime * 0.4f);
        }

        private bool HasLineOfSight()
        {
            if (target == null || muzzle == null) return false;

            Vector3 start = muzzle.position;
            Vector3 end = target.position + Vector3.up * aimHeight;
            Vector3 dir = (end - start).normalized;

            if (Physics.Raycast(start, dir, out RaycastHit hit, fireDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                RenkaiRoundPlayer hitPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                return hitPlayer != null && hitPlayer.team != team && hitPlayer.isAlive;
            }

            return false;
        }

        private void FireAtTarget()
        {
            nextFireTime = Time.time + 1f / fireRate;

            Vector3 start = muzzle.position;
            Vector3 end = target.position + Vector3.up * aimHeight;

            if (Physics.Raycast(start, (end - start).normalized, out RaycastHit hit, fireDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                RenkaiRoundPlayer victim = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();

                if (victim != null && victim.team != team && victim.isAlive)
                    victim.TakeDamage(damage, GetComponent<RenkaiRoundPlayer>());
            }

            SpawnTracer(start, end);
        }

        private void SpawnTracer(Vector3 start, Vector3 end)
        {
            GameObject tracer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(tracer.GetComponent<Collider>());

            Vector3 mid = (start + end) * 0.5f;
            float len = Vector3.Distance(start, end);

            tracer.transform.position = mid;
            tracer.transform.rotation = Quaternion.LookRotation(end - start);
            tracer.transform.localScale = new Vector3(0.03f, 0.03f, len);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tracerColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tracerColor);

            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", tracerColor * 2.8f);

            tracer.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(tracer, 0.065f);
        }
    }
}
