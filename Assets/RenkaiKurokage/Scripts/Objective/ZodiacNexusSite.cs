using UnityEngine;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(Collider))]
    public sealed class ZodiacNexusSite : MonoBehaviour
    {
        [SerializeField] private string siteId = "A";

        public string SiteId => siteId;

        public void Configure(string id)
        {
            siteId = string.IsNullOrWhiteSpace(id) ? "A" : id.Trim().ToUpperInvariant();
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }
}
