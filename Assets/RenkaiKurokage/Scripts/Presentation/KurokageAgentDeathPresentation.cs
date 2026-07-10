using System.Collections;
using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageAgentDeathPresentation : MonoBehaviour
    {
        [SerializeField] private float collapseDuration = 0.42f;
        [SerializeField] private float dissolveFlickerDuration = 0.18f;
        [SerializeField] private float dropDistance = 0.62f;
        [SerializeField] private float leanAngle = 68f;

        private Transform visualRoot;
        private Renderer[] visualRenderers;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalScale;
        private Coroutine deathRoutine;
        private bool cached;

        private void Awake()
        {
            CacheVisualState();
        }

        public void PlayDeath(Vector3 hitDirection)
        {
            CacheVisualState();
            if (visualRoot == null) return;

            if (deathRoutine != null) StopCoroutine(deathRoutine);
            deathRoutine = StartCoroutine(DeathRoutine(hitDirection));
        }

        public void ResetPresentation()
        {
            if (deathRoutine != null) StopCoroutine(deathRoutine);
            deathRoutine = null;
            CacheVisualState();
            if (visualRoot == null) return;

            visualRoot.localPosition = baseLocalPosition;
            visualRoot.localRotation = baseLocalRotation;
            visualRoot.localScale = baseLocalScale;

            if (visualRenderers != null)
            {
                foreach (Renderer renderer in visualRenderers)
                    if (renderer != null) renderer.enabled = true;
            }
        }

        private IEnumerator DeathRoutine(Vector3 hitDirection)
        {
            Vector3 horizontal = hitDirection;
            horizontal.y = 0f;
            if (horizontal.sqrMagnitude < 0.01f) horizontal = -transform.forward;
            horizontal.Normalize();

            float side = Mathf.Sign(Vector3.Dot(horizontal, transform.right));
            if (Mathf.Approximately(side, 0f)) side = 1f;

            Vector3 startPosition = visualRoot.localPosition;
            Quaternion startRotation = visualRoot.localRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, leanAngle * side);
            Vector3 endPosition = startPosition + Vector3.down * dropDistance + Vector3.right * side * 0.16f;

            float elapsed = 0f;
            while (elapsed < collapseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / collapseDuration));
                visualRoot.localPosition = Vector3.Lerp(startPosition, endPosition, t);
                visualRoot.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "AGENT_NEURAL_COLLAPSE",
                transform.position + Vector3.up * 0.95f,
                Quaternion.identity,
                Vector3.one * 0.48f,
                new Color(0.20f, 0.54f, 1f, 1f),
                3.8f,
                0.22f
            );

            elapsed = 0f;
            while (elapsed < dissolveFlickerDuration)
            {
                elapsed += Time.deltaTime;
                bool visible = Mathf.Sin(elapsed * 72f) > -0.15f;
                if (visualRenderers != null)
                {
                    foreach (Renderer renderer in visualRenderers)
                        if (renderer != null) renderer.enabled = visible;
                }
                visualRoot.localScale = Vector3.Lerp(baseLocalScale, baseLocalScale * 0.92f, elapsed / dissolveFlickerDuration);
                yield return null;
            }

            if (visualRenderers != null)
            {
                foreach (Renderer renderer in visualRenderers)
                    if (renderer != null) renderer.enabled = false;
            }

            deathRoutine = null;
        }

        private void CacheVisualState()
        {
            Transform found = transform.Find("AGENT_VISUAL");
            if (found == null)
            {
                visualRoot = null;
                visualRenderers = null;
                return;
            }

            if (cached && visualRoot == found) return;

            visualRoot = found;
            visualRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            baseLocalPosition = visualRoot.localPosition;
            baseLocalRotation = visualRoot.localRotation;
            baseLocalScale = visualRoot.localScale;
            cached = true;
        }
    }
}
