using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageCombatFeedbackHUD : MonoBehaviour
    {
        [SerializeField] private RenkaiWeaponController weapon;

        private CanvasGroup hitGroup;
        private Image hitMarker;
        private Text headshotText;
        private GameObject reloadRoot;
        private Image reloadFill;
        private Text reloadLabel;
        private Coroutine hitRoutine;

        private void Awake()
        {
            if (weapon == null) weapon = FindObjectOfType<RenkaiWeaponController>();
            Build();
        }

        private void OnEnable()
        {
            if (weapon == null) weapon = FindObjectOfType<RenkaiWeaponController>();
            if (weapon != null) weapon.HitConfirmed += OnHitConfirmed;
        }

        private void OnDisable()
        {
            if (weapon != null) weapon.HitConfirmed -= OnHitConfirmed;
        }

        private void Update()
        {
            if (weapon == null)
            {
                weapon = FindObjectOfType<RenkaiWeaponController>();
                if (weapon != null) weapon.HitConfirmed += OnHitConfirmed;
                return;
            }

            bool reloading = weapon.IsReloading;
            reloadRoot.SetActive(reloading);
            if (reloading)
            {
                reloadFill.fillAmount = weapon.ReloadNormalized;
                reloadLabel.text = "RELOAD // " + Mathf.RoundToInt(weapon.ReloadNormalized * 100f) + "%";
            }
        }

        private void OnHitConfirmed(bool headshot)
        {
            if (hitRoutine != null) StopCoroutine(hitRoutine);
            hitRoutine = StartCoroutine(HitRoutine(headshot));
        }

        private IEnumerator HitRoutine(bool headshot)
        {
            hitGroup.alpha = 1f;
            hitMarker.color = headshot
                ? new Color(1f, 0.18f, 0.38f, 1f)
                : new Color(0.95f, 0.98f, 1f, 1f);
            headshotText.gameObject.SetActive(headshot);

            float hold = headshot ? 0.20f : 0.09f;
            yield return new WaitForSecondsRealtime(hold);

            float t = 0f;
            const float fade = 0.16f;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                hitGroup.alpha = 1f - Mathf.Clamp01(t / fade);
                yield return null;
            }

            headshotText.gameObject.SetActive(false);
            hitGroup.alpha = 0f;
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            GameObject hitRoot = new GameObject("HIT_FEEDBACK", typeof(CanvasGroup));
            hitRoot.transform.SetParent(transform, false);
            RectTransform hrt = hitRoot.GetComponent<RectTransform>();
            hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.5f);
            hrt.anchoredPosition = Vector2.zero;
            hrt.sizeDelta = new Vector2(180f, 120f);
            hitGroup = hitRoot.GetComponent<CanvasGroup>();
            hitGroup.alpha = 0f;

            hitMarker = new GameObject("HitMarker").AddComponent<Image>();
            hitMarker.transform.SetParent(hitRoot.transform, false);
            RectTransform mrt = hitMarker.rectTransform;
            mrt.anchorMin = mrt.anchorMax = new Vector2(0.5f, 0.5f);
            mrt.anchoredPosition = Vector2.zero;
            mrt.sizeDelta = new Vector2(24f, 24f);
            hitMarker.color = Color.white;

            headshotText = CreateText("HEADSHOT", hitRoot.transform, 18, TextAnchor.MiddleCenter, new Color(1f, 0.30f, 0.48f, 1f));
            RectTransform hst = headshotText.rectTransform;
            hst.anchorMin = hst.anchorMax = new Vector2(0.5f, 0.5f);
            hst.anchoredPosition = new Vector2(0f, -38f);
            hst.sizeDelta = new Vector2(180f, 24f);
            headshotText.gameObject.SetActive(false);

            reloadRoot = new GameObject("RELOAD_PROGRESS", typeof(Image));
            reloadRoot.transform.SetParent(transform, false);
            RectTransform rrt = reloadRoot.GetComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0f);
            rrt.anchoredPosition = new Vector2(0f, 150f);
            rrt.sizeDelta = new Vector2(320f, 34f);
            reloadRoot.GetComponent<Image>().color = new Color(0.025f, 0.04f, 0.075f, 0.88f);

            reloadFill = new GameObject("Fill").AddComponent<Image>();
            reloadFill.transform.SetParent(reloadRoot.transform, false);
            reloadFill.color = new Color(0.18f, 0.55f, 1f, 0.9f);
            reloadFill.type = Image.Type.Filled;
            reloadFill.fillMethod = Image.FillMethod.Horizontal;
            reloadFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            RectTransform fillRt = reloadFill.rectTransform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(3f, 3f);
            fillRt.offsetMax = new Vector2(-3f, -3f);

            reloadLabel = CreateText("Label", reloadRoot.transform, 14, TextAnchor.MiddleCenter, Color.white);
            RectTransform labelRt = reloadLabel.rectTransform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            reloadRoot.SetActive(false);
        }

        private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment, Color color)
        {
            Text text = new GameObject(name).AddComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            return text;
        }
    }
}
