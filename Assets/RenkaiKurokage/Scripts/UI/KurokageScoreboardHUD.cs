using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageScoreboardHUD : MonoBehaviour
    {
        [SerializeField] private KurokageMatchStatsTracker tracker;
        [SerializeField] private KeyCode scoreboardKey = KeyCode.Tab;

        private GameObject panelRoot;
        private RectTransform rowsRoot;
        private readonly List<RowRefs> rows = new List<RowRefs>();
        private readonly List<KurokageMatchStatsTracker.PlayerStats> orderedStats = new List<KurokageMatchStatsTracker.PlayerStats>();
        private float nextRefresh;

        private sealed class RowRefs
        {
            public Text Team;
            public Text Agent;
            public Text Kills;
            public Text Deaths;
            public Text Damage;
            public Text Headshots;
            public Text BladeKills;
            public Text Objective;
            public Image Background;
        }

        private void Awake()
        {
            if (tracker == null) tracker = FindObjectOfType<KurokageMatchStatsTracker>();
            Build();
            panelRoot.SetActive(false);
        }

        private void Update()
        {
            bool visible = Input.GetKey(scoreboardKey);
            if (panelRoot.activeSelf != visible)
                panelRoot.SetActive(visible);

            if (!visible) return;
            if (tracker == null)
            {
                tracker = FindObjectOfType<KurokageMatchStatsTracker>();
                if (tracker == null) return;
            }

            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + 0.12f;
                RefreshRows();
            }
        }

        private void RefreshRows()
        {
            orderedStats.Clear();
            foreach (KurokageMatchStatsTracker.PlayerStats stat in tracker.AllStats)
                if (stat != null && stat.Player != null) orderedStats.Add(stat);

            orderedStats.Sort(CompareStats);
            EnsureRowCount(orderedStats.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                bool active = i < orderedStats.Count;
                rows[i].Background.gameObject.SetActive(active);
                if (!active) continue;

                KurokageMatchStatsTracker.PlayerStats stat = orderedStats[i];
                bool attacker = stat.Player.team == RenkaiTeam.Attackers;
                rows[i].Team.text = attacker ? "ATK" : "DEF";
                rows[i].Team.color = attacker
                    ? new Color(0.30f, 0.72f, 1f, 1f)
                    : new Color(0.72f, 0.42f, 1f, 1f);
                rows[i].Agent.text = stat.Player.agentName + (stat.Player.isAlive ? string.Empty : "   // DOWN");
                rows[i].Kills.text = stat.Kills.ToString();
                rows[i].Deaths.text = stat.Deaths.ToString();
                rows[i].Damage.text = stat.Damage.ToString();
                rows[i].Headshots.text = stat.Headshots.ToString();
                rows[i].BladeKills.text = stat.BladeKills.ToString();
                rows[i].Objective.text = stat.CorePickups.ToString();
                rows[i].Background.color = i % 2 == 0
                    ? new Color(0.025f, 0.045f, 0.075f, 0.94f)
                    : new Color(0.035f, 0.055f, 0.09f, 0.92f);
            }
        }

        private static int CompareStats(KurokageMatchStatsTracker.PlayerStats a, KurokageMatchStatsTracker.PlayerStats b)
        {
            int teamCompare = a.Player.team.CompareTo(b.Player.team);
            if (teamCompare != 0) return teamCompare;
            int killCompare = b.Kills.CompareTo(a.Kills);
            if (killCompare != 0) return killCompare;
            return b.Damage.CompareTo(a.Damage);
        }

        private void EnsureRowCount(int count)
        {
            while (rows.Count < count)
                rows.Add(CreateRow(rowsRoot, rows.Count));
        }

        private void Build()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 46;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("RENKAI_SCOREBOARD_PANEL", typeof(Image));
            panelRoot.transform.SetParent(transform, false);
            RectTransform panel = panelRoot.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(1180f, 690f);
            panelRoot.GetComponent<Image>().color = new Color(0.012f, 0.022f, 0.04f, 0.965f);

            Text title = CreateText("Title", panelRoot.transform, 28, TextAnchor.MiddleLeft, new Color(0.92f, 0.97f, 1f, 1f));
            title.text = "RENKAI // KUROKAGE   —   TACTICAL TELEMETRY";
            RectTransform titleRt = title.rectTransform;
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0f, 1f);
            titleRt.pivot = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(34f, -26f);
            titleRt.sizeDelta = new Vector2(850f, 40f);

            Text mode = CreateText("Mode", panelRoot.transform, 15, TextAnchor.MiddleRight, new Color(0.48f, 0.68f, 0.92f, 1f));
            mode.text = "OFFLINE 5V5 // ZODIAC NETWORK";
            RectTransform modeRt = mode.rectTransform;
            modeRt.anchorMin = modeRt.anchorMax = new Vector2(1f, 1f);
            modeRt.pivot = new Vector2(1f, 1f);
            modeRt.anchoredPosition = new Vector2(-34f, -32f);
            modeRt.sizeDelta = new Vector2(300f, 28f);

            RectTransform header = new GameObject("HEADER").AddComponent<RectTransform>();
            header.SetParent(panelRoot.transform, false);
            header.anchorMin = header.anchorMax = new Vector2(0.5f, 1f);
            header.anchoredPosition = new Vector2(0f, -98f);
            header.sizeDelta = new Vector2(1110f, 34f);

            CreateHeaderLabel(header, "TEAM", -500f, 80f);
            CreateHeaderLabel(header, "AGENT", -315f, 270f);
            CreateHeaderLabel(header, "K", -105f, 60f);
            CreateHeaderLabel(header, "D", -25f, 60f);
            CreateHeaderLabel(header, "DMG", 85f, 100f);
            CreateHeaderLabel(header, "HS", 200f, 80f);
            CreateHeaderLabel(header, "BLADE", 330f, 110f);
            CreateHeaderLabel(header, "CORE", 470f, 90f);

            rowsRoot = new GameObject("ROWS").AddComponent<RectTransform>();
            rowsRoot.SetParent(panelRoot.transform, false);
            rowsRoot.anchorMin = rowsRoot.anchorMax = new Vector2(0.5f, 1f);
            rowsRoot.anchoredPosition = new Vector2(0f, -142f);
            rowsRoot.sizeDelta = new Vector2(1110f, 510f);
        }

        private RowRefs CreateRow(RectTransform parent, int index)
        {
            GameObject rowGo = new GameObject("ScoreRow_" + index.ToString("00"), typeof(Image));
            rowGo.transform.SetParent(parent, false);
            RectTransform row = rowGo.GetComponent<RectTransform>();
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -index * 48f);
            row.sizeDelta = new Vector2(1110f, 43f);

            RowRefs refs = new RowRefs();
            refs.Background = rowGo.GetComponent<Image>();
            refs.Team = CreateCell(row, -500f, 80f, TextAnchor.MiddleCenter);
            refs.Agent = CreateCell(row, -315f, 270f, TextAnchor.MiddleLeft);
            refs.Kills = CreateCell(row, -105f, 60f, TextAnchor.MiddleCenter);
            refs.Deaths = CreateCell(row, -25f, 60f, TextAnchor.MiddleCenter);
            refs.Damage = CreateCell(row, 85f, 100f, TextAnchor.MiddleCenter);
            refs.Headshots = CreateCell(row, 200f, 80f, TextAnchor.MiddleCenter);
            refs.BladeKills = CreateCell(row, 330f, 110f, TextAnchor.MiddleCenter);
            refs.Objective = CreateCell(row, 470f, 90f, TextAnchor.MiddleCenter);
            return refs;
        }

        private static Text CreateCell(RectTransform parent, float x, float width, TextAnchor alignment)
        {
            Text text = CreateText("Cell", parent, 16, alignment, new Color(0.88f, 0.93f, 1f, 1f));
            RectTransform rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(width, 42f);
            return text;
        }

        private static void CreateHeaderLabel(RectTransform parent, string label, float x, float width)
        {
            Text text = CreateText("Header_" + label, parent, 13, TextAnchor.MiddleCenter, new Color(0.42f, 0.62f, 0.84f, 1f));
            text.text = label;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(width, 30f);
        }

        private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment, Color color)
        {
            Text text = new GameObject(name).AddComponent<Text>();
            text.transform.SetParent(parent, false);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }
    }
}
