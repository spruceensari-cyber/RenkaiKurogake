
using UnityEngine;

namespace Renkai.Kurogake
{
    public class MinimapFollow : MonoBehaviour
    {
        public Transform target;
        public float height = 95f;

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = new Vector3(target.position.x, height, target.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
