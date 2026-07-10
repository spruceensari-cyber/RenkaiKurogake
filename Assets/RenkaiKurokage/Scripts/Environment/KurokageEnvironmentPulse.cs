using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageEnvironmentPulse : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobFrequency = 0.55f;
        [SerializeField] private bool enableBob = true;

        private Vector3 baseLocalPosition;
        private float phase;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            phase = Mathf.Abs(transform.GetInstanceID() % 1000) * 0.001f * Mathf.PI * 2f;
        }

        private void Update()
        {
            if (rotationAxis.sqrMagnitude > 0.001f && Mathf.Abs(rotationSpeed) > 0.001f)
                transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);

            if (enableBob)
            {
                float y = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f + phase) * bobAmplitude;
                transform.localPosition = baseLocalPosition + Vector3.up * y;
            }
        }
    }
}
