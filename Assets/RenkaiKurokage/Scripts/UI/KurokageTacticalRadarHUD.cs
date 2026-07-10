using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageTacticalRadarHUD : MonoBehaviour
    {
        [SerializeField] private Vector2 worldMin = new Vector2(-55f, -75f);
        [SerializeField] private Vector2 worldMax = new Vector2(55f, 75f);
        [SerializeField] private Vector2 radarSize = new Vector2(250f, 250f);
        [SerializeField] private float refreshInterval = 0.10f;

        private sealed class PlayerIcon
        {
            public RenkaiRoundPlayer Player;
            public RectTransform Rect;
            public Image Image;
        }

        private readonly List<PlayerIcon> playerIcons = new List<PlayerIcon>();
        private RectTransform radarRoot;
        private RectTransform coreIcon;
        private readonly List<RectTransform> nexusIcons = new List<RectTransform>();
        private RenkaiRoundPlayer localPlayer;
        private ZodiacCoreRuntime core;
        private ZodiacNexusSite[] sites;
        private float nextRefresh;

        private readonly Color panelColor = new Color(0.018f, 0.035f, 0.060f, 0.88f);
        private readonly Color gridColor = new Color(0.26f, 0.48f, 0.72f, 0.18f);
        private readonly Color teamColor = new Color(0.22f, 0.66f, 1f, 1f);
        private readonly Color localColor = new Color(0.94f, 0.98f, 1f, 1f);
        private readonly Color coreColor = new Color(0.28f, 0.76f, 1f, 1f);

        private void Awake()
        {
            Build();
            ResolveWorldReferences();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshInterval;

            if (localPlayer == null || core == null || sites == null || sites.Length != 2)
                ResolveWorldReferences();

            RefreshPlayers();
            RefreshObjectiveIcons();
        }

        private void ResolveWorldReferences()
        {
            playerIcons.Clear();
            foreach (RenkaiRoundPlayer player in FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (player == null) continue;
                if (player.isHumanPlayer) localPlayer = player;
            }

            core = FindObjectOfType<ZodiacCoreRuntime>();
            sites = FindObjectsOfType<ZodiacNexusSite>(true);
            RebuildPlayerIcons();
            RebuildObjectiveIcons();
        }

        private void RebuildPlayerIcons()
        {
            Transform iconsRoot = radarRoot.Find("PLAYER_ICONS");
            if (iconsRoot == null) return;

            for (int i = iconsRoot.childCount - 1; i >= 0; i--)
                Destroy(iconsRoot.GetChild(i).gameObject);

            playerIcons.Clear();
            if (localPlayer == null) return;

            foreach (RenkaiRoundPlayer player in FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (player == null || player.team != localPlayer.team) continue;

                GameObject iconGo = new GameObject("RadarAgent_" + player.agentName, typeof(Image));
                iconGo.transform.SetParent(iconsRoot, false);
                RectTransform rt = iconGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = player == localPlayer ? new Vector2(10f, 16f) : new Vector2(9f, 9f);
                rt.localRotation = player == localPlayer ? Quaternion.identity : Quaternion.Euler(0f, 0f, 45f);

                Image image = iconGo.GetComponent<Image>();
                image.color = player == localPlayer ? localColor : teamColor;
                image.raycastTarget = false;

                playerIcons.Add(new PlayerIcon { Player = player, Rect = rt, Image = image });
            }
        }

        private void RebuildObjectiveIcons()
        {
            Transform objectiveRoot = radarRoot.Find("OBJECTIVE_ICONS");
            if (objectiveRoot == null) return;

            for (int i = objectiveRoot.childCount - 1; i >= 0; i--)
                Destroy(objectiveRoot.GetChild(i).gameObject);

            nexusIcons.Clear();
            if (sites != null)
            {
                foreach (ZodiacNexusSite site in sites)
                {
                    RectTransform icon = CreateDiamond(objectiveRoot, "NEXUS_" + site.SiteId, new Vector2(13f, 13f), site.SiteId == "B"
                        ? new Color(0.62f, 0.36f, 0.92f, 0.95f)
                        : new Color(0.24f, 0.66f, 1f, 0.95f));
                    nexusIcons.Add(icon);
                }
            }

            coreIcon = CreateDiamond(objectiveRoot, "ZODIAC_CORE", new Vector2(11f, 11f), coreColor);
        }

        private void RefreshPlayers()
        {
            foreach (PlayerIcon icon in playerIcons)
            {
                if (icon == null || icon.Player == null || icon.Rect == null) continue;
                icon.Rect.anchoredPosition = WorldToRadar(icon.Player.transform.position);
                icon.Image.color = icon.Player.isAlive
                    ? (icon.Player == localPlayer ? localColor : teamColor)
                    : new Color(0.34f, 0.40f, 0.50f, 0.42f);

                if (icon.Player == localPlayer)
                {
                    float yaw = -icon.Player.transform.eulerAngles.y;
                    icon.Rect.localRotation = Quaternion.Euler(0f, 0f, yaw);
                }
            }
        }

        private void RefreshObjectiveIcons()
        {
            if (coreIcon != null && core != null)
            {
                coreIcon.anchoredPosition = WorldToRadar(core.transform.position);
                float pulse = 0.86f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f)) * 0.28f;
                coreIcon.localScale = Vector3.one * pulse;
            }

            if (sites == null) return;
            int count = Mathf.Min(sites.Length, nexusIcons.Count);
            for (int i = 0; i < count; i++)
            {
                if (sites[i] == null || nexusIcons[i] == null) continue;
                nexusIcons[i].anchoredPosition = WorldToRadar(sites[i].transform.position);
            }
        }

        private Vector2 WorldToRadar(Vector3 world)
        {
            float x01 = Mathf.InverseLerp(worldMin.x, worldMax.x, world.x);
            float y01 = Mathf.InverseLerp(worldMin.y, worldMax.y, world.z);
            float x = (x01 - 0.5f) * (radarSize.x - 18f);
            float y = (y01 - 0.5f) * (radarSize.y - 18f);
            return new Vector2(x, y);
        }

        private void Build()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 12;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("TACTICAL_RADAR_PANEL", typeof(Image));
            panel.transform.SetParent(transform, false);
            radarRoot = panel.GetComponent<RectTransform>();
            radarRoot.anchorMin = radarRoot.anchorMax = new Vector2(0f, 1f);
            radarRoot.pivot = new Vector2(0f, 1f);
            radarRoot.anchoredPosition = new Vector2(28f, -28f);
            radarRoot.sizeDelta = radarSize;
            panel.GetComponent<Image>().color = panelColor;
            panel.GetComponent<Image>().raycastTarget = false;

            BuildGrid(radarRoot);

            RectTransform objectiveRoot = new GameObject("OBJECTIVE_ICONS").AddComponent<RectTransform>();
            objectiveRoot.SetParent(radarRoot, false);
            Stretch(objectiveRoot);

            RectTransform playersRoot = new GameObject("PLAYER_ICONS").AddComponent<RectTransform>();
            playersRoot.SetParent(radarRoot, false);
            Stretch(playersRoot);

            Text title = CreateText("RadarTitle", radarRoot, 13, new Color(0.62f, 0.78f, 0.96f, 1f));
            title.text = "CELESTIAL NETWORK // LOCAL GRID";
            title.alignment = TextAnchor.MiddleLeft;
            RectTransform tr = title.rectTransform;
            tr.anchorMin = tr.anchorMax = new Vector2(0f, 1f);
            tr.pivot = new Vector2(0f, 1f);
            tr.anchoredPosition = new Vector2(10f, -8f);
            tr.sizeDelta = new Vector2(220f, 22f);
        }

        private void BuildGrid(RectTransform parent)
        {
            for (int i = 1; i < 4; i++)
            {
                float t = i / 4f;
                Image vertical = CreateLine(parent, "GridV_" + i, gridColor);
                RectTransform vrt = vertical.rectTransform;
                vrt.anchorMin = vrt.anchorMax = new Vector2(t, 0.5f);
                vrt.sizeDelta = new Vector2(1f, radarSize.y - 12f);

                Image horizontal = CreateLine(parent, "GridH_" + i, gridColor);
                RectTransform hrt = horizontal.rectTransform;
                hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, t);
                hrt.sizeDelta = new Vector2(radarSize.x - 12f, 1f);
            }

            Image axisV = CreateLine(parent, "AxisV", new Color(0.20f, 0.58f, 1f, 0.28f));
            axisV.rectTransform.anchorMin = axisV.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            axisV.rectTransform.sizeDelta = new Vector2(2f, radarSize.y - 8f);

            Image axisH = CreateLine(parent, "AxisH", new Color(0.20f, 0.58f, 1f, 0.28f));
            axisH.rectTransform.anchorMin = axisH.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            axisH.rectTransform.sizeDelta = new Vector2(radarSize.x - 8f, 2f);
        }

        private static RectTransform CreateDiamond(Transform parent, string name, Vector2 size, Color color)
        {
            Image image = new GameObject(name, typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.localRotation = Quaternion.Euler(0f, 0f, 45f);
            return rt;
        }

        private static Image CreateLine(Transform parent, string name, Color color)
        {
            Image image = new GameObject(name, typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, int size, Color color)
        {
            Text text = new GameObject(name).AddComponent<Text>();
            text.transform.SetParent(parent, false);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.raycastTarget = false;
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
