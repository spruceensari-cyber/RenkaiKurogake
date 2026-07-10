using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurokage
{
    public sealed class KurokageAbilityHUD : MonoBehaviour
    {
        [SerializeField] private KairiAbilityController abilities;

        private Text qText;
        private Text eText;
        private Text cText;
        private Text xText;
        private Image qPanel;
        private Image ePanel;
        private Image cPanel;
        private Image xPanel;
        private RectTransform xFrame;

        private readonly Color readyText = new Color(0.92f, 0.97f, 1f, 1f);
        private readonly Color cooldownText = new Color(0.45f, 0.52f, 0.64f, 1f);
        private readonly Color panelIdle = new Color(0.025f, 0.045f, 0.075f, 0.88f);
        private readonly Color panelReady = new Color(0.035f, 0.075f, 0.13f, 0.92f);
        private readonly Color panelUltimate = new Color(0.10f, 0.055f, 0.18f, 0.96f);

        private void Awake()
        {
            if (abilities == null) abilities = FindObjectOfType<KairiAbilityController>();
            Build();
        }

        private void Update()
        {
            if (abilities == null)
            {
                abilities = FindObjectOfType<KairiAbilityController>();
                return;
            }

            SetAbility(qText, qPanel, "Q", "DIRECTIONAL DASH", abilities.QCooldown01, false);
            SetAbility(eText, ePanel, "E", "HOLOGRAPHIC DECOY", abilities.ECooldown01, false);
            SetAbility(cText, cPanel, "C", "MOMENTUM LEAP", abilities.CCooldown01, false);
            SetAbility(xText, xPanel, "X", abilities.UltimateActive ? "ECLIPSE PROTOCOL // ACTIVE" : "ECLIPSE BLADE PROTOCOL", abilities.XCooldown01, abilities.UltimateActive);

            if (abilities.UltimateActive && xFrame != null)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.035f;
                xFrame.localScale = Vector3.one * pulse;
            }
            else if (xFrame != null)
            {
                xFrame.localScale = Vector3.one;
            }
        }

        private void SetAbility(Text text, Image panel, string key, string label, float cooldown01, bool ultimateActive)
        {
            if (text == null || panel == null) return;
            bool ready = cooldown01 <= 0.001f;

            if (ultimateActive)
            {
                text.text = key + "   " + label;
                text.color = new Color(0.92f, 0.86f, 1f, 1f);
                panel.color = panelUltimate;
                return;
            }

            text.text = ready
                ? key + "   " + label
                : key + "   " + label + "   //   " + Mathf.CeilToInt(cooldown01 * 100f) + "%";
            text.color = ready ? readyText : cooldownText;
            panel.color = ready ? panelReady : panelIdle;
        }

        private void Build()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            RectTransform root = new GameObject("KAIRI_PROTOCOL_BAR", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(transform, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0f);
            root.anchoredPosition = new Vector2(0f, 58f);
            root.sizeDelta = new Vector2(920f, 76f);

            qText = CreateSlot(root, -345f, 160f, out qPanel, out _);
            eText = CreateSlot(root, -120f, 250f, out ePanel, out _);
            cText = CreateSlot(root, 125f, 220f, out cPanel, out _);
            xText = CreateSlot(root, 375f, 260f, out xPanel, out xFrame);
        }

        private Text CreateSlot(RectTransform parent, float x, float width, out Image panelImage, out RectTransform frame)
        {
            GameObject panelGo = new GameObject("AbilitySlot", typeof(Image));
            panelGo.transform.SetParent(parent, false);
            RectTransform panel = panelGo.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(x, 0f);
            panel.sizeDelta = new Vector2(width, 58f);
            panelImage = panelGo.GetComponent<Image>();
            panelImage.color = panelIdle;
            panelImage.raycastTarget = false;

            GameObject accent = new GameObject("ResonanceAccent", typeof(Image));
            accent.transform.SetParent(panelGo.transform, false);
            RectTransform accentRt = accent.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(0f, 1f);
            accentRt.pivot = new Vector2(0f, 0.5f);
            accentRt.anchoredPosition = Vector2.zero;
            accentRt.sizeDelta = new Vector2(4f, 0f);
            accent.GetComponent<Image>().color = x > 300f
                ? new Color(0.46f, 0.24f, 1f, 0.95f)
                : new Color(0.16f, 0.56f, 1f, 0.95f);
            accent.GetComponent<Image>().raycastTarget = false;

            Text text = new GameObject("Label").AddComponent<Text>();
            text.transform.SetParent(panelGo.transform, false);
            text.font = KurokageUiFont.Default;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 14;
            text.color = readyText;
            text.raycastTarget = false;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8f, 0f);
            rt.offsetMax = new Vector2(-6f, 0f);

            frame = panel;
            return text;
        }
    }
}
