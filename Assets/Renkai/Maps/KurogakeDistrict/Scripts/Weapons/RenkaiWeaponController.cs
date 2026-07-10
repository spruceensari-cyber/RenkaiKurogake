using UnityEngine;
using UnityEngine.UI;

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

        [Header("Feel")]
        public float range = 120f;
        public float recoilReturnSpeed = 9f;
        public float tracerLife = 0.045f;
        public Color rifleTracerColor = new Color(0.18f, 0.55f, 1f);
        public Color pistolTracerColor = new Color(0.75f, 0.35f, 1f);

        private float nextFireTime;
        private float reloadEndTime;
        private float hitTextUntil;
        private bool reloading;
        private CharacterController characterController;
        private Vector2 recoilOffset;
        private Vector2 recoilVelocity;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            characterController = GetComponent<CharacterController>();

            if (muzzlePoint == null && playerCamera != null)
            {
                GameObject muzzle = new GameObject("MuzzlePoint");
                muzzle.transform.SetParent(playerCamera.transform);
                muzzle.transform.localPosition = new Vector3(0.42f, -0.22f, 0.85f);
                muzzlePoint = muzzle.transform;
            }

            SelectWeapon(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(2);

            if (reloading && Time.time >= reloadEndTime)
                FinishReload();

            bool wantsFire = slot == RenkaiWeaponSlot.Pistol
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

            if (!reloading && wantsFire && Time.time >= nextFireTime)
            {
                if (slot == RenkaiWeaponSlot.Sword) Slash();
                else FireGun();
            }

            if (Input.GetKeyDown(KeyCode.R)) StartReload();
            if (Input.GetKeyDown(KeyCode.V)) Slash();

            UpdateRecoilRecovery();

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
            SelectWeapon(0);
        }

        private void SelectWeapon(int index)
        {
            if (reloading) reloading = false;

            slot = index == 0
                ? RenkaiWeaponSlot.Rifle
                : index == 1
                    ? RenkaiWeaponSlot.Pistol
                    : RenkaiWeaponSlot.Sword;

            if (rifleView != null) rifleView.SetActive(slot == RenkaiWeaponSlot.Rifle);
            if (pistolView != null) pistolView.SetActive(slot == RenkaiWeaponSlot.Pistol);
            if (swordView != null) swordView.SetActive(slot == RenkaiWeaponSlot.Sword);
        }

        private void FireGun()
        {
            int ammo = slot == RenkaiWeaponSlot.Rifle ? rifleAmmo : pistolAmmo;
            if (ammo <= 0)
            {
                StartReload();
                return;
            }

            if (slot == RenkaiWeaponSlot.Rifle) rifleAmmo--;
            else pistolAmmo--;

            float fireRate = slot == RenkaiWeaponSlot.Rifle ? rifleFireRate : pistolFireRate;
            nextFireTime = Time.time + 1f / Mathf.Max(0.1f, fireRate);

            float spreadDegrees = GetCurrentSpread();
            Vector2 random = Random.insideUnitCircle * Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);
            Vector3 direction = (
                playerCamera.transform.forward +
                playerCamera.transform.right * random.x +
                playerCamera.transform.up * random.y
            ).normalized;

            Ray ray = new Ray(playerCamera.transform.position, direction);
            Vector3 end = ray.origin + ray.direction * range;

            if (Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;

                RenkaiHealth health = hit.collider.GetComponentInParent<RenkaiHealth>();
                if (health != null)
                {
                    float bodyDamage = slot == RenkaiWeaponSlot.Rifle ? rifleBodyDamage : pistolBodyDamage;
                    float headMultiplier = slot == RenkaiWeaponSlot.Rifle ? rifleHeadMultiplier : pistolHeadMultiplier;
                    bool headshot = hit.collider.name.ToLowerInvariant().Contains("head");
                    float damage = bodyDamage * (headshot ? headMultiplier : 1f);

                    health.TakeDamage(damage);
                    ShowHit(headshot);
                }
            }

            Color tracerColor = slot == RenkaiWeaponSlot.Rifle ? rifleTracerColor : pistolTracerColor;
            SpawnTracer(muzzlePoint.position, end, tracerColor, tracerLife);
            SpawnMuzzleFlash();
            ApplyRecoil();
        }

        private float GetCurrentSpread()
        {
            bool grounded = characterController == null || characterController.isGrounded;
            float speed = characterController == null ? 0f : characterController.velocity.magnitude;

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

            recoilVelocity += new Vector2(
                Random.Range(-horizontal, horizontal),
                vertical
            );
        }

        private void UpdateRecoilRecovery()
        {
            if (playerCamera == null) return;

            recoilVelocity = Vector2.Lerp(recoilVelocity, Vector2.zero, recoilReturnSpeed * Time.deltaTime);
            recoilOffset += recoilVelocity * Time.deltaTime * 8f;
            recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, recoilReturnSpeed * 0.7f * Time.deltaTime);

            Vector3 euler = playerCamera.transform.localEulerAngles;
            float x = euler.x > 180f ? euler.x - 360f : euler.x;
            playerCamera.transform.localEulerAngles = new Vector3(
                x - recoilVelocity.y * Time.deltaTime,
                recoilVelocity.x * Time.deltaTime,
                0f
            );
        }

        private void StartReload()
        {
            if (slot == RenkaiWeaponSlot.Sword || reloading) return;

            if (slot == RenkaiWeaponSlot.Rifle && (rifleAmmo >= 30 || rifleReserve <= 0)) return;
            if (slot == RenkaiWeaponSlot.Pistol && (pistolAmmo >= 12 || pistolReserve <= 0)) return;

            reloading = true;
            reloadEndTime = Time.time + (slot == RenkaiWeaponSlot.Rifle ? rifleReloadTime : pistolReloadTime);
            Debug.Log("RELOAD STARTED");
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
            Debug.Log("RELOAD FINISHED");
        }

        private void Slash()
        {
            nextFireTime = Time.time + 0.55f;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.SphereCast(ray, 0.85f, out RaycastHit hit, 3.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                RenkaiHealth health = hit.collider.GetComponentInParent<RenkaiHealth>();
                if (health != null)
                {
                    health.TakeDamage(55f);
                    ShowHit(false);
                }
            }

            GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(slash.GetComponent<Collider>());
            slash.name = "Renkai_Sword_Slash";
            slash.transform.SetParent(playerCamera.transform);
            slash.transform.localPosition = new Vector3(0f, -0.05f, 1.35f);
            slash.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
            slash.transform.localScale = new Vector3(1.6f, 0.045f, 0.18f);
            Paint(slash, new Color(0.2f, 0.55f, 1f), 3.5f);
            Destroy(slash, 0.12f);
        }

        public void SpawnTracer(Vector3 from, Vector3 to, Color color, float life)
        {
            if (Vector3.Distance(from, to) < 0.1f) return;

            GameObject tracer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(tracer.GetComponent<Collider>());
            tracer.name = "Renkai_Tracer";

            Vector3 mid = (from + to) * 0.5f;
            float length = Vector3.Distance(from, to);
            tracer.transform.position = mid;
            tracer.transform.rotation = Quaternion.LookRotation(to - from);
            tracer.transform.localScale = new Vector3(0.018f, 0.018f, length);

            Paint(tracer, color, 2.2f);
            Destroy(tracer, life);
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzlePoint == null) return;

            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(flash.GetComponent<Collider>());
            flash.transform.position = muzzlePoint.position;
            flash.transform.localScale = Vector3.one * 0.11f;
            Paint(flash, new Color(1f, 0.62f, 0.18f), 3.5f);
            Destroy(flash, 0.04f);
        }

        private void Paint(GameObject go, Color color, float emission)
        {
            bool srpActive = GraphicsSettings.currentRenderPipeline != null;
            Shader shader = srpActive ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * emission);

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
        }

        private void ShowHit(bool headshot)
        {
            if (hitText == null) return;
            hitText.text = headshot ? "HEADSHOT" : "HIT";
            hitText.enabled = true;
            hitTextUntil = Time.time + (headshot ? 0.22f : 0.13f);
        }

        private void UpdateUI()
        {
            if (weaponText != null)
                weaponText.text = slot == RenkaiWeaponSlot.Rifle
                    ? "KX-9 KURO"
                    : slot == RenkaiWeaponSlot.Pistol
                        ? "SHIRO SIDEARM"
                        : "ECLIPSE BLADE";

            if (ammoText == null) return;

            if (slot == RenkaiWeaponSlot.Sword)
                ammoText.text = "BLADE";
            else if (reloading)
                ammoText.text = "RELOADING...";
            else
                ammoText.text = slot == RenkaiWeaponSlot.Rifle
                    ? rifleAmmo + " / " + rifleReserve
                    : pistolAmmo + " / " + pistolReserve;
        }
    }
}
