
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renkai.Kurogake
{
    public class KurogateTeleporter : MonoBehaviour
    {
        public Transform targetPoint;
        public float teleportDelay = 0.7f;
        public float cooldown = 4f;
        public AudioSource gateAudio;
        public ParticleSystem gateParticles;

        private readonly Dictionary<GameObject, float> nextUseTime = new Dictionary<GameObject, float>();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (targetPoint == null) return;

            float allowed;
            nextUseTime.TryGetValue(other.gameObject, out allowed);
            if (Time.time < allowed) return;

            StartCoroutine(TeleportRoutine(other.gameObject));
        }

        private IEnumerator TeleportRoutine(GameObject player)
        {
            nextUseTime[player] = Time.time + cooldown;

            if (gateAudio != null) gateAudio.Play();
            if (gateParticles != null) gateParticles.Play();

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            yield return new WaitForSeconds(teleportDelay);

            player.transform.position = targetPoint.position;
            player.transform.rotation = targetPoint.rotation;

            if (controller != null) controller.enabled = true;
        }
    }
}
