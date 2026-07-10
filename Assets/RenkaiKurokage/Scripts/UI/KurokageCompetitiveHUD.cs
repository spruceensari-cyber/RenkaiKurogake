using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageCompetitiveHUD : MonoBehaviour
    {
        [SerializeField] private RenkaiWeaponController weapon;
        [SerializeField] private RenkaiHealth health;
        [SerializeField] private CharacterController controller;

        private Text healthText;
        private Text ammoText;
        private Text weaponText;
        private RectTransform crosshairRoot;
        private Image hitMarker;
        private float hitMarkerUntil;

        private void Awake()
        {
            if (weapon == null) weapon = FindObjectOfType<RenkaiWeaponController>();
            if (health == null && weapon != null) health = weapon.GetComponent<RenkaiHealth>();
            if (controller == null && weapon != null) controller = weapon.GetComponent<CharacterController>();
            BuildHud();
        }

        private void Update()
        {
            if (healthText != null && health != null)
                healthText.text = Mathf.CeilToInt(health.CurrentHealth).ToString();

            if (weapon != null)
            {
                if (weapon.slot == RenkaiWeaponSlot.Rifle)
                {
                    weaponText.text = "KX-9 KURO";
                    ammoText.text = weapon.rifleAmmo + "  /  " + weapon.rifleReserve;
                }
                else if (weapon.slot == RenkaiWeaponSlot.Pistol)
                {
                    weaponText.text = "SHIRO SIDEARM";
                    ammoText.text = weapon.pistolAmmo + "  /  " + weapon.pistolReserve;
                }
                else
                {
                    weaponText.text = "ECLIPSE BLADE";
                    ammoText.text = "BLADE";
                }
            }

            if (crosshairRoot != null)
            {
                float speed = controller == null ? 0f : new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
                float target = 10f + Mathf.Clamp(speed * 1.8f, 0f, 18f);
                crosshairRoot.sizeDelta = Vector2.Lerp(crosshairRoot.sizeDelta, Vector2.one * target, 12f * Time.deltaTime);
            }

            if (hitMarker != null)
                hitMarker.enabled = Time.time < hitMarkerUntil;
        }

        public void PulseHitMarker(bool headshot)
        {
            hitMarkerUntil = Time.time + (headshot ? 0.18f : 0.10f);
            if (hitMarker != null)
                hitMarker.color = headshot ? new Color(1f, 0.3f, 0.3f, 1f) : Color.white;
        }

        private void BuildHud()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            healthText = CreateText("Health", new Vector2(70f, 45f), TextAnchor.LowerLeft, 30);
            ammoText = CreateText("Ammo", new Vector2(-70f, 45f), TextAnchor.LowerRight, 28);
            weaponText = CreateText("Weapon", new Vector2(-70f, 78f), TextAnchor.LowerRight, 16);

            crosshairRoot = new GameObject("Crosshair").AddComponent<RectTransform>();
            crosshairRoot.SetParent(transform, false);
            crosshairRoot.anchorMin = crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRoot.anchoredPosition = Vector2.zero;
            crosshairRoot.sizeDelta = Vector2.one * 10f;

            CreateCrosshairLine(new Vector2(0f, 7f), new Vector2(2f, 6f));
            CreateCrosshairLine(new Vector2(0f, -7f), new Vector2(2f, 6f));
            CreateCrosshairLine(new Vector2(7f, 0f), new Vector2(6f, 2f));
            CreateCrosshairLine(new Vector2(-7f, 0f), new Vector2(6f, 2f));

            hitMarker = new GameObject("HitMarker").AddComponent<Image>();
            RectTransform rt = hitMarker.rectTransform;
            rt.SetParent(transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(16f, 16f);
            hitMarker.color = Color.white;
            hitMarker.enabled = false;
        }

        private Text CreateText(string name, Vector2 position, TextAnchor alignment, int fontSize)
        {
            Text text = new GameObject(name).AddComponent<Text>();
            text.transform.SetParent(transform, false);
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            RectTransform rt = text.rectTransform;
            bool right = position.x < 0f;
            rt.anchorMin = rt.anchorMax = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rt.pivot = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(320f, 50f);
            return text;
        }

        private void CreateCrosshairLine(Vector2 pos, Vector2 size)
        {
            Image image = new GameObject("Line").AddComponent<Image>();
            image.transform.SetParent(crosshairRoot, false);
            image.color = Color.white;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
    }
}
