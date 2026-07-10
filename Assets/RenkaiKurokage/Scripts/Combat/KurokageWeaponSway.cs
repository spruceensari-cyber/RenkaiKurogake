using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageWeaponSway : MonoBehaviour
    {
        [SerializeField] private float positionAmount = 0.018f;
        [SerializeField] private float rotationAmount = 1.8f;
        [SerializeField] private float smooth = 10f;
        [SerializeField] private float bobAmount = 0.015f;
        [SerializeField] private float bobSpeed = 9f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private CharacterController controller;
        private float bobTime;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            controller = GetComponentInParent<CharacterController>();
        }

        private void LateUpdate()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 targetPosition = baseLocalPosition + new Vector3(-mouseX, -mouseY, 0f) * positionAmount;
            Quaternion targetRotation = baseLocalRotation * Quaternion.Euler(mouseY * rotationAmount, -mouseX * rotationAmount, mouseX * rotationAmount * 0.35f);

            bool moving = controller != null && controller.isGrounded && controller.velocity.sqrMagnitude > 0.3f;
            if (moving)
            {
                bobTime += Time.deltaTime * bobSpeed;
                targetPosition += new Vector3(Mathf.Cos(bobTime * 0.5f), Mathf.Abs(Mathf.Sin(bobTime)), 0f) * bobAmount;
            }
            else
            {
                bobTime = 0f;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smooth * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
        }
    }
}
