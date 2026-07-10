using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageProductionBuildMarker : MonoBehaviour
    {
        [SerializeField] private string buildId = "UNSET";

        public string BuildId => buildId;

        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (FindObjectOfType<KurokageUnifiedPresentationHUD>(true) == null)
                gameObject.AddComponent<KurokageUnifiedPresentationHUD>();
        }

        public void SetBuildId(string value)
        {
            buildId = string.IsNullOrWhiteSpace(value) ? "UNSET" : value;
        }
    }
}
