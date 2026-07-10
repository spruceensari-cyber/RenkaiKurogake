using System;
<<<<<<< Updated upstream
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurokage;
=======
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
>>>>>>> Stashed changes

namespace Renkai.Kurogake
{
    public enum RenkaiWeaponSlot
    {
        Rifle,
        Pistol,
        Sword
    }

    public sealed class RenkaiWeaponController : MonoBehaviour
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

        [Header("Accuracy States")]
        [SerializeField] private float crouchStationarySpreadMultiplier = 0.74f;
        [SerializeField] private float crouchMovingSpreadMultiplier = 0.88f;
        [SerializeField] private float movingThreshold = 0.2f;

        [Header("ADS")]
        public float hipFov = 90f;
        public float adsFov = 72f;
        public float adsLerpSpeed = 11f;
        public float adsSpreadMultiplier = 0.72f;
        public float adsRecoilMultiplier = 0.82f;

        [Header("Feel")]
        public float range = 120f;
<<<<<<< Updated upstream
        public float tracerLife = 0.045f;
=======
        public float tracerLife = 0.055f;
>>>>>>> Stashed changes
        public Color rifleTracerColor = new Color(0.18f, 0.55f, 1f);
        public Color pistolTracerColor = new Color(0.75f, 0.35f, 1f);
        public float weaponResponse = 14f;

<<<<<<< Updated upstream
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
        public bool IsFireLocked => externalFireLocked;
        public bool CanFireNow => !externalFireLocked && !reloading && Time.time >= nextFireTime;
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
        private bool externalFireLocked;
        private CharacterController characterController;
        private RenkaiFPSController fpsController;
        private KurokageCombatVfxPresenter combatVfx;
        private KurokageBladeCombatController bladeCombat;
        private RenkaiRoundPlayer selfPlayer;
        private int recoilPatternIndex;
        private float lastShotTime = -999f;
=======
        private const int RifleMagazineSize = 30;
        private const int PistolMagazineSize = 12;
        private const int TracerPoolSize = 12;
        private const int ImpactPoolSize = 10;

        private readonly List<TransientLine> tracers = new List<TransientLine>();
        private readonly List<TransientObject> impacts = new List<TransientObject>();
        private RenkaiFPSController motor;
        private Transform viewRoot;
        private Transform activeView;
        private Vector3 hipPosition;
        private Vector3 adsPosition;
        private Vector3 readyPosition;
        private Quaternion readyRotation;
        private GameObject muzzleFlash;
        private float muzzleFlashUntil;
        private float nextFireTime;
        private float reloadStartedAt;
        private float reloadEndsAt;
        private float viewKick;
        private float bobTime;
        private bool reloading;
        private bool hitTextWasManaged;

        public bool IsReloading => reloading;
        public bool IsAiming { get; private set; }
        public float ReloadNormalized => !reloading ? 0f : Mathf.Clamp01((Time.time - reloadStartedAt) / Mathf.Max(0.01f, reloadEndsAt - reloadStartedAt));
        public int CurrentAmmo => slot == RenkaiWeaponSlot.Rifle ? rifleAmmo : slot == RenkaiWeaponSlot.Pistol ? pistolAmmo : 0;
        public int CurrentReserve => slot == RenkaiWeaponSlot.Rifle ? rifleReserve : slot == RenkaiWeaponSlot.Pistol ? pistolReserve : 0;
        public int CurrentMagazineSize => slot == RenkaiWeaponSlot.Rifle ? RifleMagazineSize : slot == RenkaiWeaponSlot.Pistol ? PistolMagazineSize : 0;
        public string ActiveWeaponName => slot == RenkaiWeaponSlot.Rifle ? "KX-9 KURO" : slot == RenkaiWeaponSlot.Pistol ? "SHIRO SIDEARM" : "ECLIPSE BLADE";

        public event Action<bool> HitConfirmed;

        private sealed class TransientLine
        {
            public LineRenderer Line;
            public float ExpiresAt;
        }

        private sealed class TransientObject
        {
            public GameObject Object;
            public float ExpiresAt;
        }
>>>>>>> Stashed changes

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

<<<<<<< Updated upstream
            characterController = GetComponent<CharacterController>();
            fpsController = GetComponent<RenkaiFPSController>();
            combatVfx = GetComponent<KurokageCombatVfxPresenter>();
            bladeCombat = GetComponent<KurokageBladeCombatController>();
            selfPlayer = GetComponent<RenkaiRoundPlayer>();

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
=======
            motor = GetComponent<RenkaiFPSController>();
            CreateViewModels();
            CreateEffectPool();
            SelectWeapon((int)slot, true);
        }

        private void OnDisable()
        {
            IsAiming = false;
            if (motor != null)
                motor.SetAiming(false);
>>>>>>> Stashed changes
        }

        private void OnDisable()
        {
            if (fpsController != null)
                fpsController.SetAdsFovRequest(false, adsFov);
        }

        private void Update()
        {
<<<<<<< Updated upstream
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(2);

            IsAiming = !externalFireLocked && !reloading && slot != RenkaiWeaponSlot.Sword && Input.GetMouseButton(1);
            UpdateAdsRequest();

            if (reloading && Time.time >= reloadEndTime)
                FinishReload();

            bool wantsFire = slot == RenkaiWeaponSlot.Pistol
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

            if (!externalFireLocked && !reloading && wantsFire && Time.time >= nextFireTime)
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
=======
            HandleWeaponSelection();
            UpdateReload();
            UpdateAim();
            HandleFireInput();
            UpdateViewModel();
            UpdateTransientEffects();
            UpdateLegacyTextFields();
>>>>>>> Stashed changes
        }

        public void SetExternalFireLock(bool locked)
        {
            externalFireLocked = locked;
            if (locked)
            {
                IsAiming = false;
                UpdateAdsRequest();
            }
        }

        public void ResetAmmo()
        {
            rifleAmmo = RifleMagazineSize;
            rifleReserve = 90;
            pistolAmmo = PistolMagazineSize;
            pistolReserve = 36;
<<<<<<< Updated upstream
            reloading = false;
            externalFireLocked = false;
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
=======
            CancelReload();
            SelectWeapon(0, true);
        }

        public void TryStartReload()
        {
            if (reloading || slot == RenkaiWeaponSlot.Sword || CurrentAmmo >= CurrentMagazineSize || CurrentReserve <= 0)
                return;
>>>>>>> Stashed changes

            reloading = true;
            reloadStartedAt = Time.time;
            reloadEndsAt = Time.time + (slot == RenkaiWeaponSlot.Rifle ? rifleReloadTime : pistolReloadTime);
            IsAiming = false;
            if (motor != null)
                motor.SetAiming(false);
        }

        private void HandleWeaponSelection()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0, false);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1, false);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(2, false);
            if (Input.GetKeyDown(KeyCode.R)) TryStartReload();
        }

        private void UpdateReload()
        {
            if (!reloading || Time.time < reloadEndsAt)
                return;

            if (slot == RenkaiWeaponSlot.Rifle)
            {
                int moved = Mathf.Min(RifleMagazineSize - rifleAmmo, rifleReserve);
                rifleAmmo += moved;
                rifleReserve -= moved;
            }
            else if (slot == RenkaiWeaponSlot.Pistol)
            {
                int moved = Mathf.Min(PistolMagazineSize - pistolAmmo, pistolReserve);
                pistolAmmo += moved;
                pistolReserve -= moved;
            }

            reloading = false;
        }

        private void UpdateAim()
        {
            IsAiming = !reloading && slot != RenkaiWeaponSlot.Sword && Input.GetMouseButton(1);
            if (motor != null)
                motor.SetAiming(IsAiming);
        }

        private void HandleFireInput()
        {
            if (reloading || Time.time < nextFireTime)
                return;

            bool wantsToFire = slot == RenkaiWeaponSlot.Pistol
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

            if (!wantsToFire)
                return;

            if (slot == RenkaiWeaponSlot.Sword)
            {
                Slash();
                return;
            }

            if (CurrentAmmo <= 0)
            {
                TryStartReload();
                return;
            }

            FireGun();
        }

        private void SelectWeapon(int index, bool immediate)
        {
            CancelReload();
            slot = index == 1 ? RenkaiWeaponSlot.Pistol : index == 2 ? RenkaiWeaponSlot.Sword : RenkaiWeaponSlot.Rifle;

            if (rifleView != null) rifleView.SetActive(slot == RenkaiWeaponSlot.Rifle);
            if (pistolView != null) pistolView.SetActive(slot == RenkaiWeaponSlot.Pistol);
            if (swordView != null) swordView.SetActive(slot == RenkaiWeaponSlot.Sword);

<<<<<<< Updated upstream
            IsAiming = false;
            recoilPatternIndex = 0;
            UpdateAdsRequest();
=======
            activeView = slot == RenkaiWeaponSlot.Rifle ? rifleView != null ? rifleView.transform : null :
                         slot == RenkaiWeaponSlot.Pistol ? pistolView != null ? pistolView.transform : null :
                         swordView != null ? swordView.transform : null;

            if (activeView != null && immediate)
            {
                activeView.localPosition = hipPosition;
                activeView.localRotation = readyRotation;
            }
        }

        private void CancelReload()
        {
            reloading = false;
>>>>>>> Stashed changes
        }

        private void FireGun()
        {
<<<<<<< Updated upstream
            if (playerCamera == null) return;

            int ammo = slot == RenkaiWeaponSlot.Rifle ? rifleAmmo : pistolAmmo;
            if (ammo <= 0)
            {
                nextFireTime = Time.time + 0.16f;
                EmptyTriggered?.Invoke();
                return;
            }

            if (slot == RenkaiWeaponSlot.Rifle) rifleAmmo--;
            else pistolAmmo--;
=======
            if (slot == RenkaiWeaponSlot.Rifle)
                rifleAmmo--;
            else
                pistolAmmo--;
>>>>>>> Stashed changes

            float fireRate = slot == RenkaiWeaponSlot.Rifle ? rifleFireRate : pistolFireRate;
            nextFireTime = Time.time + 1f / Mathf.Max(0.1f, fireRate);
            lastShotTime = Time.time;

<<<<<<< Updated upstream
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
=======
            float spread = GetCurrentSpread() * (IsAiming ? 0.62f : 1f);
            Vector2 randomSpread = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spread * Mathf.Deg2Rad);
            Transform cameraTransform = playerCamera.transform;
            Vector3 direction = (cameraTransform.forward + cameraTransform.right * randomSpread.x + cameraTransform.up * randomSpread.y).normalized;
            Ray ray = new Ray(cameraTransform.position, direction);
            Vector3 endpoint = ray.origin + ray.direction * range;
>>>>>>> Stashed changes

            if (KurokageCombatRayResolver.TryResolve(ray, range, transform, out RaycastHit hit))
            {
<<<<<<< Updated upstream
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
            KurokageDecoyHitReceiver decoy = hit.collider.GetComponentInParent<KurokageDecoyHitReceiver>();
            if (decoy != null)
            {
                decoy.Hit(hit.point, hit.normal);
                ShowHit(false);
                return;
            }

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

                if (selfPlayer == null || roundPlayer.team != selfPlayer.team)
                {
                    KurokageDamageInfo info = new KurokageDamageInfo(
                        damage,
                        selfPlayer,
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

                // Friendly bodies still block the shot, but do not receive hit confirmation or damage.
                if (combatVfx != null)
                    combatVfx.SpawnBodyImpact(hit.point, hit.normal);
                return;
            }

            if (legacyHealth != null)
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
=======
                endpoint = hit.point;
                ApplyHit(hit);
                SpawnImpact(hit.point, hit.normal);
            }

            Color tracerColor = slot == RenkaiWeaponSlot.Rifle ? rifleTracerColor : pistolTracerColor;
            SpawnTracer(muzzlePoint != null ? muzzlePoint.position : ray.origin, endpoint, tracerColor);
            SpawnMuzzleFlash();

            float vertical = slot == RenkaiWeaponSlot.Rifle ? rifleVerticalRecoil : pistolVerticalRecoil;
            float horizontal = slot == RenkaiWeaponSlot.Rifle ? rifleHorizontalRecoil : pistolHorizontalRecoil;
            if (motor != null)
                motor.AddRecoil(vertical * (IsAiming ? 0.82f : 1f), horizontal);
            viewKick = Mathf.Min(0.12f, viewKick + vertical * 0.035f);
        }

        private void ApplyHit(RaycastHit hit)
        {
            RenkaiRoundPlayer roundPlayer = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
            bool headshot = hit.collider.name.IndexOf("head", StringComparison.OrdinalIgnoreCase) >= 0;
            float bodyDamage = slot == RenkaiWeaponSlot.Rifle ? rifleBodyDamage : pistolBodyDamage;
            float multiplier = slot == RenkaiWeaponSlot.Rifle ? rifleHeadMultiplier : pistolHeadMultiplier;

            if (roundPlayer != null)
            {
                RenkaiRoundPlayer owner = GetComponent<RenkaiRoundPlayer>();
                if (owner != null && roundPlayer.team == owner.team)
                    return;

                roundPlayer.TakeDamage(bodyDamage * (headshot ? multiplier : 1f), owner);
                ConfirmHit(headshot);
                return;
            }

            RenkaiHealth health = hit.collider.GetComponentInParent<RenkaiHealth>();
            if (health != null)
            {
                health.TakeDamage(bodyDamage * (headshot ? multiplier : 1f));
                ConfirmHit(headshot);
            }
        }

        private void ConfirmHit(bool headshot)
        {
            HitConfirmed?.Invoke(headshot);

            if (hitText != null)
            {
                hitText.text = headshot ? "HEADSHOT" : "HIT";
                hitText.enabled = true;
                hitTextWasManaged = true;
            }
>>>>>>> Stashed changes
        }

        private float GetCurrentSpread()
        {
<<<<<<< Updated upstream
            bool grounded = characterController == null || characterController.isGrounded;
            float speed = characterController == null ? 0f : new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;
            bool moving = speed > movingThreshold;
=======
            CharacterController controller = GetComponent<CharacterController>();
            bool grounded = controller == null || controller.isGrounded;
            float speed = motor != null ? motor.PlanarSpeed : controller != null ? controller.velocity.magnitude : 0f;
>>>>>>> Stashed changes

            float spread;
            if (slot == RenkaiWeaponSlot.Rifle)
            {
<<<<<<< Updated upstream
                spread = !grounded ? rifleAirSpread : moving ? rifleMovingSpread : rifleStandingSpread;
            }
            else
            {
                spread = !grounded ? pistolAirSpread : moving ? pistolMovingSpread : pistolStandingSpread;
            }

            if (grounded && fpsController != null && fpsController.IsCrouching)
                spread *= moving ? crouchMovingSpreadMultiplier : crouchStationarySpreadMultiplier;

            return spread;
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
            if (externalFireLocked || slot == RenkaiWeaponSlot.Sword || reloading) return;
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
=======
                if (!grounded) return rifleAirSpread;
                return speed > 0.2f ? rifleMovingSpread : rifleStandingSpread;
            }

            if (!grounded) return pistolAirSpread;
            return speed > 0.2f ? pistolMovingSpread : pistolStandingSpread;
>>>>>>> Stashed changes
        }

        private void LegacySlash()
        {
            nextFireTime = Time.time + 0.55f;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
<<<<<<< Updated upstream

            if (Physics.SphereCast(ray, 0.85f, out RaycastHit hit, 3.2f, ~0, QueryTriggerInteraction.Collide))
            {
                RenkaiRoundPlayer victim = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                KurokageHitZone zone = hit.collider.GetComponent<KurokageHitZone>();
                KurokageHitZoneType zoneType = zone != null ? zone.ZoneType : KurokageHitZoneType.Torso;

                if (victim != null && (selfPlayer == null || victim.team != selfPlayer.team))
                {
                    KurokageDamageInfo info = new KurokageDamageInfo(
                        55f,
                        selfPlayer,
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
=======
            if (Physics.SphereCast(ray, 0.85f, out RaycastHit hit, 3.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                RenkaiRoundPlayer target = hit.collider.GetComponentInParent<RenkaiRoundPlayer>();
                RenkaiRoundPlayer owner = GetComponent<RenkaiRoundPlayer>();
                if (target != null && (owner == null || target.team != owner.team))
                {
                    target.TakeDamage(55f, owner);
                    ConfirmHit(false);
                }
                else
>>>>>>> Stashed changes
                {
                    RenkaiHealth health = hit.collider.GetComponentInParent<RenkaiHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(55f);
<<<<<<< Updated upstream
                        ShowHit(false);
=======
                        ConfirmHit(false);
>>>>>>> Stashed changes
                    }
                }
            }

<<<<<<< Updated upstream
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
=======
            SpawnSlashVisual();
            viewKick = 0.16f;
>>>>>>> Stashed changes
        }

        private void UpdateViewModel()
        {
            if (activeView == null)
                return;

<<<<<<< Updated upstream
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
=======
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float speed = motor != null ? motor.PlanarSpeed : 0f;
            bool moving = motor != null && motor.IsGrounded && speed > 0.2f;
            bobTime = moving ? bobTime + Time.deltaTime * Mathf.Lerp(6f, 10f, Mathf.InverseLerp(0f, 7f, speed)) : 0f;

            Vector3 bob = moving
                ? new Vector3(Mathf.Cos(bobTime * 0.5f), Mathf.Abs(Mathf.Sin(bobTime)), 0f) * 0.012f
                : Vector3.zero;
            Vector3 sway = new Vector3(-mouseX, -mouseY, 0f) * 0.012f;
            Vector3 reloadOffset = reloading
                ? new Vector3(0.08f, -0.13f, 0.12f) * Mathf.Sin(ReloadNormalized * Mathf.PI)
                : Vector3.zero;
            Quaternion reloadRotation = reloading
                ? Quaternion.Euler(38f * Mathf.Sin(ReloadNormalized * Mathf.PI), 0f, -26f * Mathf.Sin(ReloadNormalized * Mathf.PI))
                : Quaternion.identity;

            viewKick = Mathf.MoveTowards(viewKick, 0f, Time.deltaTime * 0.7f);
            Vector3 targetPosition = (IsAiming ? adsPosition : hipPosition) + bob + sway + reloadOffset + Vector3.back * viewKick;
            Quaternion targetRotation = readyRotation * Quaternion.Euler(mouseY * 1.6f, -mouseX * 1.6f, mouseX * 0.5f) * reloadRotation;
            activeView.localPosition = Vector3.Lerp(activeView.localPosition, targetPosition, weaponResponse * Time.deltaTime);
            activeView.localRotation = Quaternion.Slerp(activeView.localRotation, targetRotation, weaponResponse * Time.deltaTime);
        }

        private void CreateViewModels()
        {
            if (playerCamera == null)
                return;

            viewRoot = new GameObject("Kurokage_Viewmodels").transform;
            viewRoot.SetParent(playerCamera.transform, false);
            viewRoot.localPosition = Vector3.zero;
            viewRoot.localRotation = Quaternion.identity;
            hipPosition = new Vector3(0.34f, -0.31f, 0.58f);
            adsPosition = new Vector3(0.12f, -0.20f, 0.46f);
            readyPosition = hipPosition;
            readyRotation = Quaternion.Euler(0f, 0f, 0f);

            if (rifleView == null) rifleView = BuildRifleView();
            if (pistolView == null) pistolView = BuildPistolView();
            if (swordView == null) swordView = BuildSwordView();

            if (muzzlePoint == null)
            {
                muzzlePoint = new GameObject("MuzzlePoint").transform;
                muzzlePoint.SetParent(playerCamera.transform, false);
                muzzlePoint.localPosition = new Vector3(0.12f, -0.14f, 0.82f);
            }
        }

        private GameObject BuildRifleView()
        {
            GameObject root = new GameObject("KX-9_KURO_View");
            root.transform.SetParent(viewRoot, false);
            AddViewPart(root.transform, "Receiver", new Vector3(0f, -0.02f, 0.18f), new Vector3(0.16f, 0.14f, 0.48f), new Color(0.07f, 0.09f, 0.13f));
            AddViewPart(root.transform, "Handguard", new Vector3(0f, 0.00f, 0.50f), new Vector3(0.12f, 0.10f, 0.35f), new Color(0.16f, 0.19f, 0.22f));
            AddViewPart(root.transform, "Barrel", new Vector3(0f, 0.01f, 0.75f), new Vector3(0.045f, 0.045f, 0.24f), new Color(0.04f, 0.05f, 0.07f));
            AddViewPart(root.transform, "Stock", new Vector3(0f, -0.02f, -0.25f), new Vector3(0.13f, 0.12f, 0.30f), new Color(0.04f, 0.05f, 0.07f));
            AddViewPart(root.transform, "Grip", new Vector3(0f, -0.18f, 0.05f), new Vector3(0.10f, 0.22f, 0.12f), new Color(0.05f, 0.06f, 0.08f));
            AddViewPart(root.transform, "Magazine", new Vector3(0f, -0.22f, 0.25f), new Vector3(0.13f, 0.28f, 0.16f), new Color(0.10f, 0.12f, 0.15f));
            AddViewPart(root.transform, "EnergyRail", new Vector3(0f, 0.09f, 0.40f), new Vector3(0.035f, 0.025f, 0.50f), rifleTracerColor, true);
            return root;
        }

        private GameObject BuildPistolView()
        {
            GameObject root = new GameObject("SHIRO_SIDEARM_View");
            root.transform.SetParent(viewRoot, false);
            AddViewPart(root.transform, "Slide", new Vector3(0f, 0.02f, 0.27f), new Vector3(0.14f, 0.11f, 0.42f), new Color(0.10f, 0.12f, 0.16f));
            AddViewPart(root.transform, "Barrel", new Vector3(0f, -0.01f, 0.53f), new Vector3(0.055f, 0.055f, 0.16f), new Color(0.04f, 0.05f, 0.07f));
            AddViewPart(root.transform, "Grip", new Vector3(0f, -0.20f, 0.14f), new Vector3(0.12f, 0.26f, 0.14f), new Color(0.06f, 0.07f, 0.10f));
            AddViewPart(root.transform, "Accent", new Vector3(0f, 0.09f, 0.27f), new Vector3(0.035f, 0.02f, 0.25f), pistolTracerColor, true);
            return root;
        }

        private GameObject BuildSwordView()
        {
            GameObject root = new GameObject("ECLIPSE_BLADE_View");
            root.transform.SetParent(viewRoot, false);
            AddViewPart(root.transform, "Grip", new Vector3(0f, -0.15f, 0.12f), new Vector3(0.08f, 0.20f, 0.11f), new Color(0.04f, 0.05f, 0.07f));
            AddViewPart(root.transform, "Guard", new Vector3(0f, -0.03f, 0.22f), new Vector3(0.22f, 0.035f, 0.05f), new Color(0.12f, 0.15f, 0.18f));
            AddViewPart(root.transform, "BladeCore", new Vector3(0f, 0.02f, 0.72f), new Vector3(0.035f, 0.035f, 0.88f), rifleTracerColor, true);
            return root;
        }

        private static void AddViewPart(Transform parent, string partName, Vector3 position, Vector3 scale, Color color, bool emissive = false)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            UnityEngine.Object.Destroy(part.GetComponent<Collider>());
            part.layer = 2;

            Renderer renderer = part.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = CreateRuntimeMaterial(color, emissive ? 2.2f : 0f);
        }

        private void CreateEffectPool()
        {
            Transform root = new GameObject("Kurokage_CombatEffects").transform;
            for (int i = 0; i < TracerPoolSize; i++)
            {
                GameObject tracer = new GameObject("Tracer");
                tracer.transform.SetParent(root, false);
                LineRenderer line = tracer.AddComponent<LineRenderer>();
                line.enabled = false;
                line.positionCount = 2;
                line.startWidth = 0.018f;
                line.endWidth = 0.008f;
                line.material = CreateRuntimeMaterial(rifleTracerColor, 2.6f);
                line.shadowCastingMode = ShadowCastingMode.Off;
                tracers.Add(new TransientLine { Line = line });
            }

            for (int i = 0; i < ImpactPoolSize; i++)
            {
                GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                impact.name = "Impact";
                UnityEngine.Object.Destroy(impact.GetComponent<Collider>());
                impact.transform.SetParent(root, false);
                impact.transform.localScale = Vector3.one * 0.045f;
                impact.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                impact.GetComponent<Renderer>().sharedMaterial = CreateRuntimeMaterial(new Color(0.8f, 0.92f, 1f), 2.8f);
                impact.SetActive(false);
                impacts.Add(new TransientObject { Object = impact });
            }

            muzzleFlash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            muzzleFlash.name = "MuzzleFlash";
            UnityEngine.Object.Destroy(muzzleFlash.GetComponent<Collider>());
            muzzleFlash.transform.SetParent(root, false);
            muzzleFlash.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            muzzleFlash.GetComponent<Renderer>().sharedMaterial = CreateRuntimeMaterial(new Color(1f, 0.66f, 0.24f), 4.5f);
            muzzleFlash.SetActive(false);
        }

        private void SpawnTracer(Vector3 from, Vector3 to, Color color)
        {
            TransientLine available = tracers.Find(item => !item.Line.enabled || item.ExpiresAt <= Time.time);
            if (available == null)
                available = tracers[0];

            available.Line.startColor = color;
            available.Line.endColor = new Color(color.r, color.g, color.b, 0.15f);
            available.Line.SetPosition(0, from);
            available.Line.SetPosition(1, to);
            available.Line.enabled = true;
            available.ExpiresAt = Time.time + tracerLife;
        }

        private void SpawnImpact(Vector3 position, Vector3 normal)
        {
            TransientObject available = impacts.Find(item => !item.Object.activeSelf || item.ExpiresAt <= Time.time);
            if (available == null)
                available = impacts[0];

            available.Object.transform.position = position + normal * 0.01f;
            available.Object.transform.localScale = Vector3.one * 0.06f;
            available.Object.SetActive(true);
            available.ExpiresAt = Time.time + 0.08f;
>>>>>>> Stashed changes
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzleFlash == null || muzzlePoint == null)
                return;

<<<<<<< Updated upstream
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
            else if (externalFireLocked)
                ammoText.text = CurrentAmmo + " / " + CurrentReserve + "  // READYING";
            else
                ammoText.text = CurrentAmmo + " / " + CurrentReserve;
=======
            muzzleFlash.transform.position = muzzlePoint.position;
            muzzleFlash.transform.localScale = Vector3.one * 0.09f;
            muzzleFlash.SetActive(true);
            muzzleFlashUntil = Time.time + 0.035f;
        }

        private void SpawnSlashVisual()
        {
            if (muzzleFlash == null || playerCamera == null)
                return;

            muzzleFlash.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1.0f;
            muzzleFlash.transform.localScale = new Vector3(1.5f, 0.025f, 0.08f);
            muzzleFlash.GetComponent<Renderer>().sharedMaterial = CreateRuntimeMaterial(rifleTracerColor, 3.2f);
            muzzleFlash.SetActive(true);
            muzzleFlashUntil = Time.time + 0.10f;
        }

        private void UpdateTransientEffects()
        {
            foreach (TransientLine tracer in tracers)
            {
                if (tracer.Line.enabled && Time.time >= tracer.ExpiresAt)
                    tracer.Line.enabled = false;
            }

            foreach (TransientObject impact in impacts)
            {
                if (impact.Object.activeSelf && Time.time >= impact.ExpiresAt)
                    impact.Object.SetActive(false);
            }

            if (muzzleFlash != null && muzzleFlash.activeSelf && Time.time >= muzzleFlashUntil)
                muzzleFlash.SetActive(false);
        }

        private void UpdateLegacyTextFields()
        {
            if (weaponText != null)
                weaponText.text = ActiveWeaponName;

            if (ammoText != null)
            {
                ammoText.text = slot == RenkaiWeaponSlot.Sword
                    ? "BLADE"
                    : reloading
                        ? "RELOADING " + Mathf.RoundToInt(ReloadNormalized * 100f) + "%"
                        : CurrentAmmo + " / " + CurrentReserve;
            }

            if (hitTextWasManaged && hitText != null && hitText.enabled)
            {
                // A dedicated HUD consumes HitConfirmed; legacy text is only a compatibility fallback.
                hitTextWasManaged = false;
            }
        }

        private static Material CreateRuntimeMaterial(Color color, float emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (emission > 0f && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }

            return material;
>>>>>>> Stashed changes
        }
    }
}
