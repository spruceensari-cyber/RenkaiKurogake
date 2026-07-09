using UnityEngine;

namespace Renkai.Kurogake
{
    public class BombSiteZone : MonoBehaviour
    {
        public string siteName = "A";

        private void OnTriggerEnter(Collider other)
        {
            BombPlanter planter = other.GetComponent<BombPlanter>();
            if (planter != null) planter.EnterSite(siteName);
        }

        private void OnTriggerExit(Collider other)
        {
            BombPlanter planter = other.GetComponent<BombPlanter>();
            if (planter != null) planter.ExitSite(siteName);
        }
    }
}
