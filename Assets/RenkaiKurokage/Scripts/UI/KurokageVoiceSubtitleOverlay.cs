using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurokage
{
    public sealed class KurokageVoiceSubtitleOverlay : MonoBehaviour
    {
        private Canvas canvas;
        private CanvasGroup group;
        private Text japaneseText;
        private Text romanizedText;
        private float hideAt;

        private void OnEnable()
        {
            KurokageJapaneseVoicePresenter.SubtitleRequested += ShowSubtitle;
        }

        private void OnDisable()
        {
            KurokageJapaneseVoicePresenter.SubtitleRequested -= ShowSubtitle;
        }

        private void Update()
        {
            if (group == null) return;
            float target = Time.time < hideAt ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 4.5f);
        }

        private void ShowSubtitle(string japanese, string romanized, float duration)
        {
            EnsureUi();
            japaneseText.text = japanese;
            romanizedText.text = romanized;
            hideAt = Time.time + Mathf.Max(0.5f, duration);
            group.alpha = 1f;
        }

        private void EnsureUi()
        {
            if (canvas != null) return;
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject root = new GameObject("KUROKAGE_VOICE_SUBTITLES");
            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 450;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.AddComponent<GraphicRaycaster>();
            group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            GameObject panelGo = new GameObject("SUBTITLE_PANEL");
            panelGo.transform.SetParent(root.transform, false);
            Image panel = panelGo.AddComponent<Image>();
            panel.color = new Color(0.015f, 0.025f, 0.055f, 0.82f);
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0.28f, 0.085f);
            panelRect.anchorMax = new Vector2(0.72f, 0.19f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            japaneseText = CreateText("JP", panelRect, font, 26, new Color(0.92f, 0.96f, 1f), new Vector2(0f, 0.46f), new Vector2(1f, 0.96f));
            romanizedText = CreateText("ROMAJI", panelRect, font, 16, new Color(0.46f, 0.72f, 1f), new Vector2(0f, 0.08f), new Vector2(1f, 0.48f));
        }

        private static Text CreateText(string name, Transform parent, Font font, int size, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(18f, 0f);
            rect.offsetMax = new Vector2(-18f, 0f);
            return text;
        }
    }
}
