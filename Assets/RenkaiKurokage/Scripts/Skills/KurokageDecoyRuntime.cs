using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageDecoyRuntime : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<KurokageDecoyRuntime> Active = new System.Collections.Generic.List<KurokageDecoyRuntime>();

        public Vector3 Velocity { get; private set; }
        public bool IsActive { get; private set; }

        private Vector3 direction;
        private float speed;
        private float expireTime;
        private float baseScale;

        public void Initialize(Vector3 moveDirection, float moveSpeed, float lifetime)
        {
            direction = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
            direction.y = 0f;
            speed = moveSpeed;
            expireTime = Time.time + lifetime;
            baseScale = transform.localScale.x;
            IsActive = true;
            Active.Add(this);
        }

        private void Update()
        {
            if (!IsActive) return;

            if (Time.time >= expireTime)
            {
                DissolveAndDestroy();
                return;
            }

            Velocity = direction * speed;
            transform.position += Velocity * Time.deltaTime;

            float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.035f;
            transform.localScale = Vector3.one * Mathf.Max(0.05f, baseScale * pulse);
        }

        public void DissolveAndDestroy()
        {
            if (!IsActive) return;
            IsActive = false;
            Active.Remove(this);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Active.Remove(this);
        }
    }
}
