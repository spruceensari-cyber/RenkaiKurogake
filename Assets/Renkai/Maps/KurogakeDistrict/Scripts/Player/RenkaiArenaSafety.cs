
using UnityEngine;

namespace Renkai.Kurogake
{
    public class RenkaiArenaSafety : MonoBehaviour
    {
        public float killY = -12f;
        public Vector3 respawnPosition = new Vector3(0f, 2f, -58f);

        private CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (transform.position.y < killY)
                Respawn();
        }

        public void Respawn()
        {
            if (controller != null) controller.enabled = false;
            transform.position = respawnPosition;
            transform.rotation = Quaternion.identity;
            if (controller != null) controller.enabled = true;
        }
    }
}
