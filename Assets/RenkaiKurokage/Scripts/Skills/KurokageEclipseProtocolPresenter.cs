using System.Collections;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageEclipseProtocolPresenter : MonoBehaviour
    {
        [SerializeField] private KairiAbilityController abilities;
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private float auraPulseInterval = 0.48f;

        private bool wasActive;
        private float nextAuraPulse;
        private Vector3 swordBaseScale = Vector3.one;
        private Coroutine bladeExtensionRoutine;

        private void Awake()
        {
            if (abilities == null) abilities = GetComponent<KairiAbilityController>();
            if (weapon == null) weapon = GetComponent<RenkaiWeaponController>();
            CacheSwordScale();
        }

        private void Update()
        {
            if (abilities == null) return;
            bool active = abilities.UltimateActive;

            if (active && !wasActive)
                ActivatePresentation();
            else if (!active && wasActive)
                DeactivatePresentation();

            if (active && Time.time >= nextAuraPulse)
            {
                nextAuraPulse = Time.time + auraPulseInterval;
                SpawnAuraPulse();
            }

            wasActive = active;
        }

        public void ResetPresentation()
        {
            wasActive = false;
            nextAuraPulse = 0f;
            if (bladeExtensionRoutine != null) StopCoroutine(bladeExtensionRoutine);
            bladeExtensionRoutine = null;
            RestoreSwordScale();
        }

        private void ActivatePresentation()
        {
            CacheSwordScale();
            SpawnActivationBurst();
            nextAuraPulse = Time.time + 0.12f;

            if (weapon != null && weapon.swordView != null)
            {
                if (bladeExtensionRoutine != null) StopCoroutine(bladeExtensionRoutine);
                bladeExtensionRoutine = StartCoroutine(BladeExtensionRoutine());
            }
        }

        private void DeactivatePresentation()
        {
            RestoreSwordScale();
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "ECLIPSE_PROTOCOL_END_RING",
                transform.position + Vector3.up * 0.05f,
                Quaternion.identity,
                new Vector3(1.3f, 0.025f, 1.3f),
                new Color(0.28f, 0.46f, 1f, 1f),
                3.2f,
                0.30f
            );
        }

        private IEnumerator BladeExtensionRoutine()
        {
            Transform blade = weapon.swordView.transform;
            Vector3 start = new Vector3(swordBaseScale.x, swordBaseScale.y, swordBaseScale.z * 0.68f);
            blade.localScale = start;

            float elapsed = 0f;
            const float duration = 0.20f;
            while (elapsed < duration && blade != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                blade.localScale = Vector3.Lerp(start, swordBaseScale, t);
                yield return null;
            }

            if (blade != null) blade.localScale = swordBaseScale;
            bladeExtensionRoutine = null;
        }

        private void SpawnActivationBurst()
        {
            Vector3 center = transform.position + Vector3.up * 1f;
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "ECLIPSE_PROTOCOL_CORE",
                center,
                Quaternion.identity,
                Vector3.one * 0.58f,
                new Color(0.88f, 0.97f, 1f, 1f),
                5f,
                0.22f
            );

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "ECLIPSE_PROTOCOL_RING_BLUE",
                transform.position + Vector3.up * 0.04f,
                Quaternion.identity,
                new Vector3(2.0f, 0.025f, 2.0f),
                new Color(0.16f, 0.58f, 1f, 1f),
                4.4f,
                0.44f
            );

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "ECLIPSE_PROTOCOL_RING_VIOLET",
                transform.position + Vector3.up * 0.08f,
                Quaternion.Euler(0f, 28f, 0f),
                new Vector3(1.55f, 0.02f, 1.55f),
                new Color(0.44f, 0.22f, 1f, 1f),
                3.8f,
                0.36f
            );
        }

        private void SpawnAuraPulse()
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "ECLIPSE_PROTOCOL_AURA",
                transform.position + Vector3.up * 0.03f,
                Quaternion.identity,
                new Vector3(1.15f, 0.018f, 1.15f),
                new Color(0.22f, 0.48f, 1f, 1f),
                2.8f,
                0.28f
            );
        }

        private void CacheSwordScale()
        {
            if (weapon != null && weapon.swordView != null)
                swordBaseScale = weapon.swordView.transform.localScale;
        }

        private void RestoreSwordScale()
        {
            if (weapon != null && weapon.swordView != null)
                weapon.swordView.transform.localScale = swordBaseScale;
        }
    }
}
