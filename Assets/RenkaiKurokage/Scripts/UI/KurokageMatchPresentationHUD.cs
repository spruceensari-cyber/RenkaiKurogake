using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurokage
{
    public sealed class KurokageMatchPresentationHUD : MonoBehaviour
    {
        private Text bannerText;
        private CanvasGroup bannerGroup;
        private Text killFeedText;
        private readonly Queue<string> killFeed = new Queue<string>();
        private Coroutine bannerRoutine;

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            KurokageGameEvents.KillFeed += OnKillFeed;
            KurokageGameEvents.RoundBanner += OnRoundBanner;
        }

        private void OnDisable()
        {
            KurokageGameEvents.KillFeed -= OnKillFeed;
            KurokageGameEvents.RoundBanner -= OnRoundBanner;
        }

        private void OnKillFeed(string killer, string victim)
        {
            killFeed.Enqueue(killer + "  ▸  " + victim);
            while (killFeed.Count > 5) killFeed.Dequeue();
            killFeedText.text = string.Join("\n", killFeed.ToArray());
        }

        private void OnRoundBanner(string message)
        {
            if (bannerRoutine != null) StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(BannerRoutine(message));
        }

        private IEnumerator BannerRoutine(string message)
        {
            bannerText.text = message;
            bannerGroup.alpha = 0f;

            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.unscaledDeltaTime;
                bannerGroup.alpha = Mathf.Clamp01(t / 0.2f);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(1.1f);

            t = 0f;
            while (t < 0.45f)
            {
                t += Time.unscaledDeltaTime;
                bannerGroup.alpha = 1f - Mathf.Clamp01(t / 0.45f);
                yield return null;
            }
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();

            GameObject banner = new GameObject("ROUND_BANNER", typeof(Image), typeof(CanvasGroup));
            banner.transform.SetParent(transform, false);
            RectTransform brt = banner.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.72f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(780f, 92f);
            banner.GetComponent<Image>().color = new Color(0.02f, 0.035f, 0.07f, 0.88f);
            bannerGroup = banner.GetComponent<CanvasGroup>();
            bannerGroup.alpha = 0f;

            bannerText = CreateText("BannerText", banner.transform, 34, TextAnchor.MiddleCenter, new Color(0.94f, 0.98f, 1f, 1f));
            Stretch(bannerText.rectTransform);

            GameObject feedPanel = new GameObject("KILL_FEED", typeof(Image));
            feedPanel.transform.SetParent(transform, false);
            RectTransform frt = feedPanel.GetComponent<RectTransform>();
            frt.anchorMin = frt.anchorMax = new Vector2(1f, 1f);
            frt.pivot = new Vector2(1f, 1f);
            frt.anchoredPosition = new Vector2(-30f, -120f);
            frt.sizeDelta = new Vector2(420f, 180f);
            feedPanel.GetComponent<Image>().color = new Color(0.025f, 0.04f, 0.075f, 0.55f);

            killFeedText = CreateText("FeedText", feedPanel.transform, 18, TextAnchor.UpperRight, new Color(0.94f, 0.97f, 1f, 1f));
            Stretch(killFeedText.rectTransform);
            killFeedText.rectTransform.offsetMin = new Vector2(16f, 12f);
            killFeedText.rectTransform.offsetMax = new Vector2(-16f, -12f);
        }

        private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment, Color color)
        {
            Text text = new GameObject(name).AddComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
