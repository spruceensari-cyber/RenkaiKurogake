using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageDecoyRuntime : MonoBehaviour
    {
        public static readonly List<KurokageDecoyRuntime> Active = new List<KurokageDecoyRuntime>();

        public Vector3 Velocity { get; private set; }
        public bool IsActive { get; private set; }

        private Vector3 direction;
        private float speed;
        private float expireTime;
        private float baseScale;
        private bool dissolving;

        public void Initialize(Vector3 moveDirection, float moveSpeed, float lifetime)
        {
            direction = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
            direction.y = 0f;
            speed = moveSpeed;
            expireTime = Time.time + lifetime;
            baseScale = transform.localScale.x;
            IsActive = true;
            Active.Add(this);

            CapsuleCollider hitCollider = gameObject.AddComponent<CapsuleCollider>();
            hitCollider.height = 1.8f;
            hitCollider.radius = 0.34f;
            hitCollider.center = new Vector3(0f, 0.9f, 0f);
            hitCollider.isTrigger = true;

            KurokageDecoyHitReceiver receiver = gameObject.AddComponent<KurokageDecoyHitReceiver>();
        }

        private void Update()
        {
            if (!IsActive || dissolving) return;

            if (Time.time >= expireTime)
            {
                DissolveAndDestroy();
                return;
            }

            Velocity = direction * speed;
            float distance = speed * Time.deltaTime;
            Vector3 origin = transform.position + Vector3.up * 0.9f;
            if (Physics.Raycast(origin, direction, distance + 0.25f, ~0, QueryTriggerInteraction.Ignore))
            {
                Velocity = Vector3.zero;
                speed = 0f;
            }
            else
            {
                transform.position += Velocity * Time.deltaTime;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.035f;
            transform.localScale = Vector3.one * Mathf.Max(0.05f, baseScale * pulse);
        }

        public void DissolveAndDestroy()
        {
            if (!IsActive || dissolving) return;
            dissolving = true;
            StartCoroutine(DissolveRoutine());
        }

        private IEnumerator DissolveRoutine()
        {
            IsActive = false;
            Active.Remove(this);
            speed = 0f;

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            float duration = 0.22f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "DECOY_DISSOLVE_BURST",
                transform.position + Vector3.up * 1f,
                Quaternion.identity,
                Vector3.one * 0.42f,
                new Color(0.20f, 0.54f, 1f, 1f),
                4f,
                duration
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float flicker = Mathf.Sin(elapsed * 70f) > 0f ? 1f : 0.45f;
                foreach (Renderer renderer in renderers)
                    if (renderer != null) renderer.enabled = flicker > 0.5f;
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.08f, t);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Active.Remove(this);
        }
    }
}
