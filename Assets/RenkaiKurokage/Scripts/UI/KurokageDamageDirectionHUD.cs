using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageDamageDirectionHUD : MonoBehaviour
    {
        [SerializeField] private float visibleDuration = 0.46f;
        [SerializeField] private float ringRadius = 96f;

        private readonly Image[] segments = new Image[8];
        private readonly Coroutine[] fadeRoutines = new Coroutine[8];
        private RenkaiRoundPlayer localPlayer;

        private void Awake()
        {
            Build();
            ResolveLocalPlayer();
        }

        private void OnEnable()
        {
            KurokageGameEvents.DamageApplied += OnDamageApplied;
        }

        private void OnDisable()
        {
            KurokageGameEvents.DamageApplied -= OnDamageApplied;
        }

        private void ResolveLocalPlayer()
        {
            foreach (RenkaiRoundPlayer player in FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (player.isHumanPlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
        }

        private void OnDamageApplied(RenkaiRoundPlayer victim, KurokageDamageInfo info, float healthDamage)
        {
            if (localPlayer == null) ResolveLocalPlayer();
            if (victim == null || victim != localPlayer) return;
            if (info.Attacker == null || info.Attacker == victim) return;

            Vector3 toAttacker = info.Attacker.transform.position - victim.transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude < 0.01f) return;

            Vector3 forward = victim.transform.forward;
            forward.y = 0f;
            float signedAngle = Vector3.SignedAngle(forward.normalized, toAttacker.normalized, Vector3.up);
            int index = Mathf.RoundToInt(signedAngle / 45f);
            index = (index % 8 + 8) % 8;

            Pulse(index, healthDamage > 0f ? 1f : 0.72f);
        }

        private void Pulse(int index, float strength)
        {
            if (index < 0 || index >= segments.Length || segments[index] == null) return;
            if (fadeRoutines[index] != null) StopCoroutine(fadeRoutines[index]);
            fadeRoutines[index] = StartCoroutine(FadeSegment(index, strength));
        }

        private IEnumerator FadeSegment(int index, float strength)
        {
            Image segment = segments[index];
            float elapsed = 0f;
            Color active = new Color(0.88f, 0.24f, 0.38f, Mathf.Clamp01(strength));
            segment.color = active;

            while (elapsed < visibleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / visibleDuration);
                float alpha = 1f - t * t;
                segment.color = new Color(active.r, active.g, active.b, active.a * alpha);
                yield return null;
            }

            segment.color = new Color(active.r, active.g, active.b, 0f);
            fadeRoutines[index] = null;
        }

        private void Build()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 42;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            GameObject ring = new GameObject("RESONANCE_DIRECTION_RING", typeof(RectTransform));
            ring.transform.SetParent(transform, false);
            RectTransform ringRt = ring.GetComponent<RectTransform>();
            ringRt.anchorMin = ringRt.anchorMax = new Vector2(0.5f, 0.5f);
            ringRt.anchoredPosition = Vector2.zero;
            ringRt.sizeDelta = new Vector2(ringRadius * 2.6f, ringRadius * 2.6f);

            for (int i = 0; i < segments.Length; i++)
            {
                float angle = i * 45f;
                float rad = angle * Mathf.Deg2Rad;
                GameObject segmentGo = new GameObject("ResonanceSegment_" + i);
                segmentGo.transform.SetParent(ring.transform, false);
                Image image = segmentGo.AddComponent<Image>();
                image.color = new Color(0.88f, 0.24f, 0.38f, 0f);
                image.raycastTarget = false;

                RectTransform rt = image.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * ringRadius;
                rt.sizeDelta = new Vector2(7f, 28f);
                rt.localRotation = Quaternion.Euler(0f, 0f, -angle);
                segments[i] = image;
            }
        }
    }
}
