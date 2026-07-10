using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageDecoyHitReceiver : MonoBehaviour
    {
        [SerializeField] private KurokageDecoyRuntime runtime;

        private void Awake()
        {
            if (runtime == null) runtime = GetComponentInParent<KurokageDecoyRuntime>();
        }

        public void Hit(Vector3 point, Vector3 normal)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "DECOY_GLITCH_HIT",
                point,
                Quaternion.identity,
                Vector3.one * 0.28f,
                new Color(0.18f, 0.66f, 1f, 1f),
                4.4f,
                0.18f
            );

            if (runtime != null) runtime.DissolveAndDestroy();
        }
    }
}
