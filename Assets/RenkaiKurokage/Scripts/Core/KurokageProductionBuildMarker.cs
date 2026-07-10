using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageProductionBuildMarker : MonoBehaviour
    {
        [SerializeField] private string buildId = "UNSET";

        public string BuildId => buildId;

        public void SetBuildId(string value)
        {
            buildId = string.IsNullOrWhiteSpace(value) ? "UNSET" : value;
        }
    }
}
