using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageHitReactionPresenter : MonoBehaviour
    {
        [SerializeField] private float positionalKick = 0.075f;
        [SerializeField] private float rotationalKick = 7.5f;
        [SerializeField] private float returnSpeed = 14f;
        [SerializeField] private float headshotMultiplier = 1.45f;

        private RenkaiRoundPlayer owner;
        private Transform modelRoot;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 positionOffset;
        private Vector3 rotationOffset;

        private void Awake()
        {
            owner = GetComponent<RenkaiRoundPlayer>();
            CacheModelRoot();
        }

        private void OnEnable()
        {
            KurokageGameEvents.DamageApplied += OnDamageApplied;
        }

        private void OnDisable()
        {
            KurokageGameEvents.DamageApplied -= OnDamageApplied;
        }

        private void LateUpdate()
        {
            if (modelRoot == null)
            {
                CacheModelRoot();
                return;
            }

            if (owner != null && !owner.isAlive)
            {
                positionOffset = Vector3.zero;
                rotationOffset = Vector3.zero;
                modelRoot.localPosition = baseLocalPosition;
                modelRoot.localRotation = baseLocalRotation;
                return;
            }

            positionOffset = Vector3.Lerp(positionOffset, Vector3.zero, returnSpeed * Time.deltaTime);
            rotationOffset = Vector3.Lerp(rotationOffset, Vector3.zero, returnSpeed * Time.deltaTime);
            modelRoot.localPosition = baseLocalPosition + positionOffset;
            modelRoot.localRotation = baseLocalRotation * Quaternion.Euler(rotationOffset);
        }

        public void ResetPresentation()
        {
            positionOffset = Vector3.zero;
            rotationOffset = Vector3.zero;
            CacheModelRoot();
            if (modelRoot == null) return;
            modelRoot.localPosition = baseLocalPosition;
            modelRoot.localRotation = baseLocalRotation;
        }

        private void OnDamageApplied(RenkaiRoundPlayer victim, KurokageDamageInfo info, float healthDamage)
        {
            if (victim == null || victim != owner || modelRoot == null) return;

            Vector3 sourceDirection = info.Attacker != null
                ? (transform.position - info.Attacker.transform.position).normalized
                : -transform.forward;

            float multiplier = info.HitZone == KurokageHitZoneType.Head ? headshotMultiplier : 1f;
            Vector3 localDirection = transform.InverseTransformDirection(sourceDirection);

            positionOffset += new Vector3(
                Mathf.Clamp(localDirection.x, -1f, 1f) * positionalKick,
                info.HitZone == KurokageHitZoneType.Head ? -positionalKick * 0.45f : 0f,
                Mathf.Clamp(localDirection.z, -1f, 1f) * positionalKick
            ) * multiplier;

            rotationOffset += new Vector3(
                -Mathf.Clamp(localDirection.z, -1f, 1f) * rotationalKick,
                Mathf.Clamp(localDirection.x, -1f, 1f) * rotationalKick * 0.35f,
                -Mathf.Clamp(localDirection.x, -1f, 1f) * rotationalKick
            ) * multiplier;
        }

        private void CacheModelRoot()
        {
            Transform visual = transform.Find("AGENT_VISUAL");
            if (visual == null || visual.childCount == 0)
            {
                modelRoot = null;
                return;
            }

            modelRoot = visual.GetChild(0);
            baseLocalPosition = modelRoot.localPosition;
            baseLocalRotation = modelRoot.localRotation;
        }
    }
}
