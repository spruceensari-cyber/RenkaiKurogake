using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurokage
{
    public sealed class KurokageZodiacHUD : MonoBehaviour
    {
        private KurokageZodiacObjectiveController objective;
        private Text stateText;
        private Image progressFill;

        private void Awake()
        {
            objective = FindObjectOfType<KurokageZodiacObjectiveController>();
            Build();
        }

        private void Update()
        {
            if (objective == null)
            {
                objective = FindObjectOfType<KurokageZodiacObjectiveController>();
                return;
            }

            stateText.text = objective.StatusText;
            float progress = objective.Core != null ? objective.Core.Progress01 : 0f;
            progressFill.fillAmount = Mathf.Lerp(progressFill.fillAmount, progress, 10f * Time.deltaTime);
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("ZODIAC_PANEL", typeof(Image));
            panel.transform.SetParent(transform, false);
            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.025f, 0.04f, 0.075f, 0.84f);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 1f);
            prt.anchoredPosition = new Vector2(0f, -108f);
            prt.sizeDelta = new Vector2(620f, 52f);

            stateText = new GameObject("State").AddComponent<Text>();
            stateText.transform.SetParent(panel.transform, false);
            stateText.font = KurokageUiFont.Default;
            stateText.fontSize = 18;
            stateText.alignment = TextAnchor.MiddleCenter;
            stateText.color = new Color(0.92f, 0.97f, 1f, 1f);
            RectTransform srt = stateText.rectTransform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(18f, 10f);
            srt.offsetMax = new Vector2(-18f, -12f);

            GameObject progressBg = new GameObject("ProgressBG", typeof(Image));
            progressBg.transform.SetParent(panel.transform, false);
            progressBg.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.18f, 1f);
            RectTransform brt = progressBg.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0f);
            brt.anchorMax = new Vector2(0.5f, 0f);
            brt.anchoredPosition = new Vector2(0f, 5f);
            brt.sizeDelta = new Vector2(540f, 6f);

            progressFill = new GameObject("ProgressFill").AddComponent<Image>();
            progressFill.transform.SetParent(progressBg.transform, false);
            progressFill.color = new Color(0.18f, 0.52f, 1f, 1f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressFill.fillAmount = 0f;
            RectTransform frt = progressFill.rectTransform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
        }
    }
}
