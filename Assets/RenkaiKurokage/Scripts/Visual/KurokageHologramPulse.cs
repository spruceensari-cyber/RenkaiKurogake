using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageHologramPulse : MonoBehaviour
    {
        public float rotationSpeed = 18f;
        public float pulseSpeed = 2.2f;
        public float pulseAmount = 0.08f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = baseScale * pulse;
        }
    }
}
