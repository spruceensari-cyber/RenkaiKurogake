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
        private Quaternion baseLocalRotation;
        private float phase;
        private Rigidbody body;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            phase = Mathf.Abs(transform.GetInstanceID() % 1000) * 0.001f * Mathf.PI * 2f;
            body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (body != null && body.isKinematic) return;

            if (rotationAxis.sqrMagnitude > 0.001f && Mathf.Abs(rotationSpeed) > 0.001f)
                transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);

            if (enableBob)
            {
                float y = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f + phase) * bobAmplitude;
                transform.localPosition = baseLocalPosition + Vector3.up * y;
            }
        }

        private void FixedUpdate()
        {
            if (body == null || !body.isKinematic) return;

            float time = Time.fixedTime;
            float y = enableBob
                ? Mathf.Sin(time * bobFrequency * Mathf.PI * 2f + phase) * bobAmplitude
                : 0f;

            Vector3 localTarget = baseLocalPosition + Vector3.up * y;
            Vector3 worldTarget = transform.parent != null
                ? transform.parent.TransformPoint(localTarget)
                : localTarget;

            Quaternion localSpin = rotationAxis.sqrMagnitude > 0.001f
                ? Quaternion.AngleAxis(time * rotationSpeed, rotationAxis.normalized)
                : Quaternion.identity;
            Quaternion localTargetRotation = baseLocalRotation * localSpin;
            Quaternion worldTargetRotation = transform.parent != null
                ? transform.parent.rotation * localTargetRotation
                : localTargetRotation;

            body.MovePosition(worldTarget);
            body.MoveRotation(worldTargetRotation);
        }
    }
}
