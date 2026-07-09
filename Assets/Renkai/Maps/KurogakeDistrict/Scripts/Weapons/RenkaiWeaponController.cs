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
        public Camera playerCamera;
        public Transform muzzlePoint;

        public Text ammoText;
        public Text hitText;
        public Text weaponText;

        public GameObject rifleView;
        public GameObject pistolView;
        public GameObject swordView;

        public RenkaiWeaponSlot slot = RenkaiWeaponSlot.Rifle;

        public int rifleAmmo = 30;
        public int rifleReserve = 120;
        public int pistolAmmo = 12;
        public int pistolReserve = 48;

        private float nextFireTime;
        private float reloadEndTime;
        private float hitTextUntil;
        private bool reloading;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

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

            if (!reloading && Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                if (slot == RenkaiWeaponSlot.Sword) Slash();
                else FireGun();
            }

            if (Input.GetKeyDown(KeyCode.R)) StartReload();
            if (Input.GetKeyDown(KeyCode.V)) Slash();

            if (hitText != null && hitText.enabled && Time.time > hitTextUntil)
                hitText.enabled = false;

            UpdateUI();
        }

        public void ResetAmmo()
        {
            rifleAmmo = 30;
            rifleReserve = 120;
            pistolAmmo = 12;
            pistolReserve = 48;
            reloading = false;
            SelectWeapon(0);
        }

        private void SelectWeapon(int index)
        {
            slot = index == 0 ? RenkaiWeaponSlot.Rifle : index == 1 ? RenkaiWeaponSlot.Pistol : RenkaiWeaponSlot.Sword;

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

            float damage = slot == RenkaiWeaponSlot.Rifle ? 26f : 34f;
            float fireRate = slot == RenkaiWeaponSlot.Rifle ? 9f : 4f;
            nextFireTime = Time.time + 1f / fireRate;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            Vector3 end = ray.origin + ray.direction * 95f;

            if (Physics.Raycast(ray, out RaycastHit hit, 95f, ~0, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;

                RenkaiHealth health = hit.collider.GetComponentInParent<RenkaiHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    ShowHit();
                }
            }

            SpawnTracer(muzzlePoint.position, end, new Color(0.8f, 0.1f, 1f), 0.045f);
            SpawnMuzzleFlash();
        }

        private void StartReload()
        {
            if (slot == RenkaiWeaponSlot.Sword || reloading) return;

            if (slot == RenkaiWeaponSlot.Rifle && (rifleAmmo >= 30 || rifleReserve <= 0)) return;
            if (slot == RenkaiWeaponSlot.Pistol && (pistolAmmo >= 12 || pistolReserve <= 0)) return;

            reloading = true;
            reloadEndTime = Time.time + 1.45f;
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
                    ShowHit();
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
            tracer.transform.localScale = new Vector3(0.035f, 0.035f, length);

            Paint(tracer, color, 3f);
            Destroy(tracer, life);
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzlePoint == null) return;

            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(flash.GetComponent<Collider>());
            flash.transform.position = muzzlePoint.position;
            flash.transform.localScale = Vector3.one * 0.16f;
            Paint(flash, new Color(0.95f, 0.3f, 1f), 4f);
            Destroy(flash, 0.055f);
        }

        private void Paint(GameObject go, Color color, float emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * emission);

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
        }

        private void ShowHit()
        {
            if (hitText == null) return;
            hitText.enabled = true;
            hitTextUntil = Time.time + 0.13f;
        }

        private void UpdateUI()
        {
            if (weaponText != null)
                weaponText.text = slot.ToString();

            if (ammoText == null) return;

            if (slot == RenkaiWeaponSlot.Sword)
                ammoText.text = "SWORD";
            else if (reloading)
                ammoText.text = "RELOADING...";
            else
                ammoText.text = slot == RenkaiWeaponSlot.Rifle ? rifleAmmo + " / " + rifleReserve : pistolAmmo + " / " + pistolReserve;
        }
    }
}
