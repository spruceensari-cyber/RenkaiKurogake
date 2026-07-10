using System;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurokage;

namespace Renkai.Kurogake
{
    public enum RenkaiWeaponSlot
    {
        Rifle,
        Pistol,
        Sword
    }

    public class RenkaiWeaponController : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform muzzlePoint;
        public Text ammoText;
        public Text hitText;
        public Text weaponText;
        public GameObject rifleView;
        public GameObject pistolView;
        public GameObject swordView;

        [Header("Weapon State")]
        public RenkaiWeaponSlot slot = RenkaiWeaponSlot.Rifle;
        public int rifleAmmo = 30;
        public int rifleReserve = 90;
        public int pistolAmmo = 12;
        public int pistolReserve = 36;

        [Header("Rifle Tuning")]
        public float rifleBodyDamage = 30f;
        public float rifleHeadMultiplier = 2.5f;
        public float rifleFireRate = 9.75f;
        public float rifleReloadTime = 2.05f;
        public float rifleStandingSpread = 0.18f;
        public float rifleMovingSpread = 1.35f;
        public float rifleAirSpread = 3.2f;
        public float rifleVerticalRecoil = 0.72f;
        public float rifleHorizontalRecoil = 0.28f;

        [Header("Pistol Tuning")]
        public float pistolBodyDamage = 34f;
        public float pistolHeadMultiplier = 2.4f;
        public float pistolFireRate = 4.2f;
        public float pistolReloadTime = 1.55f;
        public float pistolStandingSpread = 0.12f;
        public float pistolMovingSpread = 0.8f;
        public float pistolAirSpread = 2.5f;
        public float pistolVerticalRecoil = 1.0f;
        public float pistolHorizontalRecoil = 0.18f;

        [Header("ADS")]
        public float hipFov = 90f;
        public float adsFov = 72f;
        public float adsLerpSpeed = 11f;
        public float adsSpreadMultiplier = 0.72f;
        public float adsRecoilMultiplier = 0.82f;

        [Header("Feel")]
        public float range = 120f;
        public float tracerLife = 0.045f;
        public Color rifleTracerColor = new Color(0.18f, 0.55f, 1f);
        public Color pistolTracerColor = new Color(0.75f, 0.35f, 1f);

        [Header("Recoil Pattern")]
        [SerializeField] private float recoilPatternResetDelay = 0.34f;
        [SerializeField] private Vector2[] riflePattern =
        {
            new Vector2(-0.08f, 1.00f), new Vector2(0.05f, 1.08f), new Vector2(0.12f, 1.15f),
            new Vector2(-0.10f, 1.18f), new Vector2(-0.16f, 1.20f), new Vector2(0.14f, 1.16f),
            new Vector2(0.20f, 1.12f), new Vector2(-0.18f, 1.08f), new Vector2(0.10f, 1.02f)
        };

        public bool IsReloading => reloading;
        public bool IsAiming { get; private set; }
        public float ReloadNormalized
        {
            get
            {
                if (!reloading || reloadDuration <= 0f) return 0f;
                return Mathf.Clamp01((Time.time - reloadStartTime) / reloadDuration);
            }
        }
        public int CurrentAmmo => slot == RenkaiWeaponSlot.Rifle ? rifleAmmo : slot == RenkaiWeaponSlot.Pistol ? pistolAmmo : -1;
        public int CurrentReserve => slot == RenkaiWeaponSlot.Rifle ? rifleReserve : slot == RenkaiWeaponSlot.Pistol ? pistolReserve : -1;
        public string ActiveWeaponName => slot == RenkaiWeaponSlot.Rifle ? "KX-9 KURO" : slot == RenkaiWeaponSlot.Pistol ? "SHIRO SIDEARM" : "ECLIPSE BLADE";

        public event Action ShotFired;
        public event Action EmptyTriggered;
        public event Action ReloadStarted;
        public event Action ReloadFinished;
        public event Action<bool> HitConfirmed;

        private float nextFireTime;
        private float reloadEndTime;
        private float reloadStartTime;
        private float reloadDuration;
        private float hitTextUntil;
        private bool reloading;
        private CharacterController characterController;
        private RenkaiFPSController fpsController;
        private KurokageCombatVfxPresenter combatVfx;
        private KurokageBladeCombatController bladeCombat;
        private int recoilPatternIndex;
        private float lastShotTime = -999f;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            characterController = GetComponent<CharacterController>();
            fpsController = GetComponent<RenkaiFPSController>();
            combatVfx = GetComponent<KurokageCombatVfxPresenter>();
            bladeCombat = GetComponent<KurokageBladeCombatController>();

            if (muzzlePoint == null && playerCamera != null)
            {
                GameObject muzzle = new GameObject("MuzzlePoint");
                muzzle.transform.SetParent(playerCamera.transform);
                muzzle.transform.localPosition = new Vector3(0.42f, -0.22f, 0.85f);
                muzzlePoint = muzzle.transform;
            }

            if (fpsController != null)
                fpsController.baseFov = hipFov;

            SelectWeapon(0);
        }

        private void OnDisable()
        {
            if (fpsController != null)
                fpsController.SetAdsFovRequest(false, adsFov);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(2);

            IsAiming = !reloading && slot != RenkaiWeaponSlot.Sword && Input.GetMouseButton(1);
            UpdateAdsRequest();

            if (reloading && Time.time >= reloadEndTime)
                FinishReload();

            bool wantsFire = slot == RenkaiWeaponSlot.Pistol
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

            if (!reloading && wantsFire && Time.time >= nextFireTime)
            {
                if (slot == RenkaiWeaponSlot.Sword)
                {
                    if (bladeCombat == null)
                        bladeCombat = GetComponent<KurokageBladeCombatController>();

                    if (bladeCombat == null)
                        LegacySlash();
                }
                else
                {
                    FireGun();
                }
            }

            if (Input.GetKeyDown(KeyCode.R)) StartReload();

            if (Time.time - lastShotTime > recoilPatternResetDelay)
                recoilPatternIndex = 0;

            if (hitText != null && hitText.enabled && Time.time > hitTextUntil)
                hitText.enabled = false;

            UpdateUI();
        }

        public void ResetAmmo()
        {
            rifleAmmo = 30;
            rifleReserve = 90;
            pistolAmmo = 12;
            pistolReserve = 36;
            reloading = false;
            recoilPatternIndex = 0;
            SelectWeapon(0);
        }

        public void SelectSlot(RenkaiWeaponSlot targetSlot)
        {
            SelectWeapon((int)targetSlot);
        }

        private void UpdateAdsRequest()
        {
            if (fpsController != null)
                fpsController.SetAdsFovRequest(IsAiming, adsFov);
        }

        private void SelectWeapon(int index)
        {
            if (reloading) CancelReload();

            slot = index == 0
                ? RenkaiWeaponSlot.Rifle
                : index == 1
                    ? RenkaiWeaponSlot.Pistol
                    : RenkaiWeaponSlot.Sword;

            if (rifleView != null) rifleView.SetActive(slot == RenkaiWeaponSlot.Rifle);
            if (pistolView != null) pistolView.SetActive(slot == RenkaiWeaponSlot.Pistol);
            if (swordView != null) swordView.SetActive(slot == RenkaiWeaponSlot.Sword);

            IsAiming = false;
            recoilPatternIndex = 0;
            UpdateAdsRequest();
        }

        private void FireGun()
        {
            int ammo = slot == RenkaiWeaponSlot.Rifle ? rifleAmmo : pistolAmmo;
            if (ammo <= 0)
            {
                nextFireTime = Time.time + 0.16f;
                EmptyTriggered?.Invoke();
                return;
            }

            if (slot == RenkaiWeaponSlot.Rifle) rifleAmmo--;
            else pistolAmmo--;

            float fireRate = slot == RenkaiWeaponSlot.Rifle ? rifleFireRate : pistolFireRate;
            nextFireTime = Time.time + 1f / Mathf.Max(0.1f, fireRate);
            lastShotTime = Time.time;

            float spreadDegrees = GetCurrentSpread();
            if (IsAiming) spreadDegrees *= adsSpreadMultiplier;

            Vector2 random = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);
            Vector3 direction = (
                playerCamera.transform.forward +
                playerCamera.transform.right * random.x +
                playerCamera.transform.up * random.y
            ).normalized;

            Ray ray = new Ray(playerCamera.transform.position, direction);
            Vector3 end = ray.origin + ray.direction * range;

            if (Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Collide))
            {
                end = hit.point;
                ProcessHit(hit);
            }

            Color tracerColor = slot == RenkaiWeaponSlot.Rifle ? rifleTracerColor : pistolTracerColor;
            SpawnTracer(muzzlePoint != null ? muzzlePoint.position : ray.origin, end, tracerColor, tracerLife);
            SpawnMuzzleFlash();
            ApplyRecoil();
            ShotFired?.Invoke();
        }

        private void ProcessHit(RaycastHit hit)
        {
            RenkaiRoundPlayer roundPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
            RenkaiHealth legacyHealth = hit.collider.GetComponentInParent<RenkaiHealth>();
            KurokageHitZone hitZone = hit.collider.GetComponent<KurokageHitZone>();

            KurokageHitZoneType zoneType = hitZone != null ? hitZone.ZoneType : KurokageHitZoneType.Torso;
            bool fallbackHeadshot = hit.collider.name.ToLowerInvariant().Contains("head");
            bool headshot = zoneType == KurokageHitZoneType.Head || fallbackHeadshot;

            float bodyDamage = slot == RenkaiWeaponSlot.Rifle ? rifleBodyDamage : pistolBodyDamage;
            float headMultiplier = slot == RenkaiWeaponSlot.Rifle ? rifleHeadMultiplier : pistolHeadMultiplier;
            float zoneMultiplier = hitZone != null ? hitZone.DamageMultiplier : (headshot ? headMultiplier : 1f);
            float damage = bodyDamage * zoneMultiplier;

            if (roundPlayer != null)
            {
                KurokageArmor targetArmor = roundPlayer.GetComponent<KurokageArmor>();
                bool hadArmor = targetArmor != null && targetArmor.CurrentArmor > 0f;
                RenkaiRoundPlayer self = GetComponent<RenkaiRoundPlayer>();

                if (self == null || roundPlayer.team != self.team)
                {
                    KurokageDamageInfo info = new KurokageDamageInfo(
                        damage,
                        self,
                        hit.point,
                        hit.normal,
                        KurokageDamageType.Ballistic,
                        zoneType,
                        ActiveWeaponName
                    );
                    roundPlayer.ApplyDamage(info);
                    ShowHit(headshot);
                    if (combatVfx != null)
                    {
                        if (headshot) combatVfx.SpawnHeadshotImpact(hit.point, hit.normal);
                        else if (hadArmor) combatVfx.SpawnArmorImpact(hit.point, hit.normal);
                        else combatVfx.SpawnBodyImpact(hit.point, hit.normal);
                    }
                    return;
                }
            }

            if (legacyHealth != null && roundPlayer == null)
            {
                legacyHealth.TakeDamage(damage);
                ShowHit(headshot);
                if (combatVfx != null)
                {
                    if (headshot) combatVfx.SpawnHeadshotImpact(hit.point, hit.normal);
                    else combatVfx.SpawnBodyImpact(hit.point, hit.normal);
                }
                return;
            }

            if (combatVfx != null)
                combatVfx.SpawnWorldImpact(hit.point, hit.normal);
        }

        private float GetCurrentSpread()
        {
            bool grounded = characterController == null || characterController.isGrounded;
            float speed = characterController == null ? 0f : new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;

            if (slot == RenkaiWeaponSlot.Rifle)
            {
                if (!grounded) return rifleAirSpread;
                if (speed > 0.2f) return rifleMovingSpread;
                return rifleStandingSpread;
            }

            if (!grounded) return pistolAirSpread;
            if (speed > 0.2f) return pistolMovingSpread;
            return pistolStandingSpread;
        }

        private void ApplyRecoil()
        {
            float vertical = slot == RenkaiWeaponSlot.Rifle ? rifleVerticalRecoil : pistolVerticalRecoil;
            float horizontal = slot == RenkaiWeaponSlot.Rifle ? rifleHorizontalRecoil : pistolHorizontalRecoil;
            float yawKick;

            if (slot == RenkaiWeaponSlot.Rifle && riflePattern != null && riflePattern.Length > 0)
            {
                Vector2 pattern = riflePattern[recoilPatternIndex % riflePattern.Length];
                vertical *= pattern.y;
                yawKick = horizontal * pattern.x + UnityEngine.Random.Range(-horizontal * 0.18f, horizontal * 0.18f);
                recoilPatternIndex++;
            }
            else
            {
                yawKick = UnityEngine.Random.Range(-horizontal, horizontal);
            }

            if (IsAiming)
            {
                vertical *= adsRecoilMultiplier;
                yawKick *= adsRecoilMultiplier;
            }

            if (fpsController != null)
                fpsController.AddRecoil(vertical, yawKick);
        }

        private void StartReload()
        {
            if (slot == RenkaiWeaponSlot.Sword || reloading) return;
            if (slot == RenkaiWeaponSlot.Rifle && (rifleAmmo >= 30 || rifleReserve <= 0)) return;
            if (slot == RenkaiWeaponSlot.Pistol && (pistolAmmo >= 12 || pistolReserve <= 0)) return;

            reloading = true;
            reloadDuration = slot == RenkaiWeaponSlot.Rifle ? rifleReloadTime : pistolReloadTime;
            reloadStartTime = Time.time;
            reloadEndTime = Time.time + reloadDuration;
            IsAiming = false;
            UpdateAdsRequest();
            ReloadStarted?.Invoke();
        }

        private void CancelReload()
        {
            reloading = false;
            reloadDuration = 0f;
        }

        private void FinishReload()
        {
            if (slot == RenkaiWeaponSlot.Rifle)
            {
                int take = Mathf.Min(30 - rifleAmmo, rifleReserve);
                rifleAmmo += take;
                rifleReserve -= take;
            }
            else if (slot == RenkaiWeaponSlot.Pistol)
            {
                int take = Mathf.Min(12 - pistolAmmo, pistolReserve);
                pistolAmmo += take;
                pistolReserve -= take;
            }

            reloading = false;
            reloadDuration = 0f;
            ReloadFinished?.Invoke();
        }

        private void LegacySlash()
        {
            nextFireTime = Time.time + 0.55f;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.SphereCast(ray, 0.85f, out RaycastHit hit, 3.2f, ~0, QueryTriggerInteraction.Collide))
            {
                RenkaiRoundPlayer victim = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                RenkaiRoundPlayer self = GetComponent<RenkaiRoundPlayer>();
                KurokageHitZone zone = hit.collider.GetComponent<KurokageHitZone>();
                KurokageHitZoneType zoneType = zone != null ? zone.ZoneType : KurokageHitZoneType.Torso;

                if (victim != null && (self == null || victim.team != self.team))
                {
                    KurokageDamageInfo info = new KurokageDamageInfo(
                        55f,
                        self,
                        hit.point,
                        hit.normal,
                        KurokageDamageType.Blade,
                        zoneType,
                        "ECLIPSE BLADE LEGACY"
                    );
                    victim.ApplyDamage(info);
                    ShowHit(false);
                    if (combatVfx != null) combatVfx.SpawnBodyImpact(hit.point, hit.normal);
                }
                else if (victim == null)
                {
                    RenkaiHealth health = hit.collider.GetComponentInParent<RenkaiHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(55f);
                        ShowHit(false);
                    }
                }
            }

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cube,
                "Renkai_Sword_Slash_Legacy",
                playerCamera.transform.TransformPoint(new Vector3(0f, -0.05f, 1.35f)),
                playerCamera.transform.rotation * Quaternion.Euler(0f, 0f, 35f),
                new Vector3(1.6f, 0.045f, 0.18f),
                new Color(0.2f, 0.55f, 1f),
                3.5f,
                0.12f
            );
            ShotFired?.Invoke();
        }

        public void SpawnTracer(Vector3 from, Vector3 to, Color color, float life)
        {
            if (Vector3.Distance(from, to) < 0.1f) return;

            Vector3 mid = (from + to) * 0.5f;
            float length = Vector3.Distance(from, to);
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cube,
                "Renkai_Tracer",
                mid,
                Quaternion.LookRotation(to - from),
                new Vector3(0.018f, 0.018f, length),
                color,
                2.2f,
                life
            );
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzlePoint == null) return;

            Color flashColor = slot == RenkaiWeaponSlot.Rifle
                ? new Color(1f, 0.62f, 0.18f)
                : new Color(0.82f, 0.56f, 1f);

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "MUZZLE_FLASH_CORE",
                muzzlePoint.position,
                muzzlePoint.rotation,
                Vector3.one * (slot == RenkaiWeaponSlot.Rifle ? 0.11f : 0.085f),
                flashColor,
                3.5f,
                0.04f
            );

            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cube,
                "MUZZLE_FLASH_CONE",
                muzzlePoint.position + muzzlePoint.forward * 0.10f,
                muzzlePoint.rotation * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 180f)),
                new Vector3(0.055f, 0.055f, slot == RenkaiWeaponSlot.Rifle ? 0.28f : 0.18f),
                flashColor,
                4.2f,
                0.035f
            );
        }

        private void ShowHit(bool headshot)
        {
            if (hitText != null)
            {
                hitText.text = headshot ? "HEADSHOT" : "HIT";
                hitText.enabled = true;
                hitTextUntil = Time.time + (headshot ? 0.22f : 0.13f);
            }

            HitConfirmed?.Invoke(headshot);
        }

        private void UpdateUI()
        {
            if (weaponText != null)
                weaponText.text = ActiveWeaponName;

            if (ammoText == null) return;

            if (slot == RenkaiWeaponSlot.Sword)
                ammoText.text = "BLADE";
            else if (reloading)
                ammoText.text = "RELOADING " + Mathf.RoundToInt(ReloadNormalized * 100f) + "%";
            else
                ammoText.text = CurrentAmmo + " / " + CurrentReserve;
        }
    }
}
