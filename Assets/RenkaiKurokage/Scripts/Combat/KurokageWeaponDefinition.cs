using UnityEngine;

namespace Renkai.Kurokage
{
    [CreateAssetMenu(menuName = "Renkai Kurokage/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class KurokageWeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "KX-9 KURO";

        [Header("Damage")]
        public float bodyDamage = 30f;
        public float headshotMultiplier = 2.5f;
        public float effectiveRange = 150f;

        [Header("Fire")]
        public float roundsPerMinute = 650f;
        public bool automatic = true;
        public int magazineSize = 30;
        public int reserveAmmo = 90;
        public float reloadTime = 2.1f;

        [Header("Accuracy")]
        public float standingSpread = 0.15f;
        public float movingSpread = 1.2f;
        public float airborneSpread = 3.5f;

        [Header("Recoil")]
        public Vector2 verticalKick = new Vector2(0.8f, 1.25f);
        public Vector2 horizontalKick = new Vector2(-0.45f, 0.45f);
    }
}
