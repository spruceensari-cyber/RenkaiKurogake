using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageRecoil : MonoBehaviour
    {
        [SerializeField] private Transform recoilPivot;
        [SerializeField] private float returnSpeed = 12f;
        [SerializeField] private float snappiness = 18f;
        private Vector3 currentRotation;
        private Vector3 targetRotation;

        private void Awake()
        {
            if (recoilPivot == null) recoilPivot = transform;
        }

        private void LateUpdate()
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
            recoilPivot.localRotation = Quaternion.Euler(currentRotation);
        }

        public void Kick(Vector2 vertical, Vector2 horizontal)
        {
            targetRotation += new Vector3(-Random.Range(vertical.x, vertical.y), Random.Range(horizontal.x, horizontal.y), 0f);
        }
    }
}
