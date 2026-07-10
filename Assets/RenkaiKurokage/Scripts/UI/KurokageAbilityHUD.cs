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

            SetAbilityText(qText, "Q", "DASH", abilities.QCooldown01);
            SetAbilityText(eText, "E", "DECOY", abilities.ECooldown01);
            SetAbilityText(cText, "C", "LEAP", abilities.CCooldown01);
            SetAbilityText(xText, "X", abilities.UltimateActive ? "ECLIPSE ACTIVE" : "ECLIPSE", abilities.XCooldown01);
        }

        private static void SetAbilityText(Text text, string key, string label, float cooldown01)
        {
            if (text == null) return;
            bool ready = cooldown01 <= 0.001f;
            text.text = ready
                ? key + "\n" + label
                : key + "\n" + Mathf.CeilToInt(cooldown01 * 100f) + "%";
            text.color = ready
                ? new Color(0.92f, 0.97f, 1f, 1f)
                : new Color(0.45f, 0.52f, 0.64f, 1f);
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform root = new GameObject("AbilityBar").AddComponent<RectTransform>();
            root.SetParent(transform, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0f);
            root.anchoredPosition = new Vector2(0f, 56f);
            root.sizeDelta = new Vector2(540f, 72f);

            qText = CreateSlot(root, -195f);
            eText = CreateSlot(root, -65f);
            cText = CreateSlot(root, 65f);
            xText = CreateSlot(root, 195f);
        }

        private static Text CreateSlot(RectTransform parent, float x)
        {
            GameObject panelGo = new GameObject("AbilitySlot", typeof(Image));
            panelGo.transform.SetParent(parent, false);
            RectTransform panel = panelGo.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(x, 0f);
            panel.sizeDelta = new Vector2(112f, 62f);
            panelGo.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.09f, 0.82f);

            Text text = new GameObject("Label").AddComponent<Text>();
            text.transform.SetParent(panelGo.transform, false);
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 15;
            text.color = Color.white;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return text;
        }
    }
}
