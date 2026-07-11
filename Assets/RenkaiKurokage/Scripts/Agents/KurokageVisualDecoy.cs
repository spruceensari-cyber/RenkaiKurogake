using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageVisualDecoy : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float expireTime;
        private Color tint;
        private Renderer[] renderers;
        private MaterialPropertyBlock block;

        public void Configure(Vector3 moveDirection, float moveSpeed, float lifetime, Color hologramTint)
        {
            direction = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
            speed = Mathf.Max(0f, moveSpeed);
            expireTime = Time.time + Mathf.Max(0.2f, lifetime);
            tint = hologramTint;
            renderers = GetComponentsInChildren<Renderer>(true);
            block = new MaterialPropertyBlock();
            ApplyTint(1f);
        }

        private void Update()
        {
            float remaining = expireTime - Time.time;
            if (remaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 0.8f;
            float distance = speed * Time.deltaTime;
            if (!Physics.SphereCast(origin, 0.22f, direction, out _, distance + 0.08f, ~0, QueryTriggerInteraction.Ignore))
                transform.position += direction * distance;
            else
                speed = 0f;

            float alpha = Mathf.Clamp01(remaining / 0.8f);
            ApplyTint(alpha);
        }

        private void ApplyTint(float alpha)
        {
            if (renderers == null) return;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(block);
                Color final = new Color(tint.r, tint.g, tint.b, alpha);
                block.SetColor("_Color", final);
                block.SetColor("_BaseColor", final);
                block.SetColor("_EmissionColor", tint * 1.7f);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
