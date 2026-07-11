using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(KurokageAgentIdentity))]
    public sealed class KurokageAgentSelectionScreen : MonoBehaviour
    {
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode reopenKey = KeyCode.F2;

        private KurokageAgentIdentity identity;
        private RenkaiRoundPlayer roundPlayer;
        private RenkaiFPSController fps;
        private RenkaiWeaponController weapon;
        private KairiAbilityController kairi;
        private KurokageAgentAbilityController genericAbilities;
        private Canvas canvas;
        private Text titleText;
        private Text roleText;
        private Text voiceText;
        private Text abilityText;
        private Text selectionHintText;
        private Image accentLine;
        private Button confirmButton;
        private readonly List<Button> agentButtons = new List<Button>();
        private KurokageAgentArchetype previewArchetype;
        private bool open;

        private void Awake()
        {
            identity = GetComponent<KurokageAgentIdentity>();
            roundPlayer = GetComponent<RenkaiRoundPlayer>();
            fps = GetComponent<RenkaiFPSController>();
            weapon = GetComponent<RenkaiWeaponController>();
            kairi = GetComponent<KairiAbilityController>();
            genericAbilities = GetComponent<KurokageAgentAbilityController>();
        }

        private IEnumerator Start()
        {
            yield return null;
            if (roundPlayer != null && roundPlayer.isHumanPlayer && showOnStart)
                OpenSelection();
        }

        private void Update()
        {
            if (roundPlayer == null || !roundPlayer.isHumanPlayer) return;
            if (!open && Input.GetKeyDown(reopenKey)) OpenSelection();
            if (!open) return;

            for (int i = 0; i < 10; i++)
            {
                KeyCode key = i == 9 ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha1 + i);
                if (Input.GetKeyDown(key)) Preview((KurokageAgentArchetype)i);
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                ConfirmSelection();
        }

        public void OpenSelection()
        {
            if (open) return;
            previewArchetype = identity != null ? identity.Archetype : KurokageAgentArchetype.Kairi;
            BuildCanvasIfNeeded();
            if (canvas == null) return;

            open = true;
            canvas.gameObject.SetActive(true);
            SetGameplayEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Preview(previewArchetype);
        }

        private void ConfirmSelection()
        {
            if (!open || identity == null) return;
            identity.Select(previewArchetype);
            open = false;
            if (canvas != null) canvas.gameObject.SetActive(false);
            SetGameplayEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (fps != null) fps.enabled = enabled;
            if (weapon != null) weapon.enabled = enabled;

            if (enabled)
            {
                if (kairi != null) kairi.enabled = identity.Archetype == KurokageAgentArchetype.Kairi;
                if (genericAbilities != null) genericAbilities.enabled = identity.Archetype != KurokageAgentArchetype.Kairi;
            }
            else
            {
                if (kairi != null) kairi.enabled = false;
                if (genericAbilities != null) genericAbilities.enabled = false;
            }
        }

        private void Preview(KurokageAgentArchetype archetype)
        {
            if (titleText == null || roleText == null || voiceText == null || abilityText == null || accentLine == null)
                return;

            previewArchetype = archetype;
            KurokageAgentDefinition definition = KurokageAgentCatalog.Get(archetype);
            titleText.text = definition.DisplayName + "  //  " + definition.Callsign;
            roleText.text = definition.Role + "   •   " + definition.JapaneseTitle;
            voiceText.text = "『" + definition.SelectVoiceJapanese + "』\n" + definition.SelectVoiceRomanized;

            string abilities = string.Empty;
            foreach (KurokageAbilityDefinition ability in definition.Abilities)
                abilities += ability.Slot + "   " + ability.DisplayName + "   /   " + ability.JapaneseName + "\n";
            abilityText.text = abilities;
            accentLine.color = definition.Accent;

            if (selectionHintText != null)
                selectionHintText.text = "1–0 PREVIEW   •   ENTER ESTABLISH LINK   •   SELECTED: " + definition.DisplayName;

            for (int i = 0; i < agentButtons.Count; i++)
            {
                ColorBlock colors = agentButtons[i].colors;
                colors.normalColor = i == (int)archetype
                    ? Color.Lerp(definition.Accent, Color.white, 0.16f)
                    : new Color(0.10f, 0.14f, 0.22f, 0.96f);
                colors.selectedColor = colors.highlightedColor;
                agentButtons[i].colors = colors;
            }
        }

        private void BuildCanvasIfNeeded()
        {
            if (canvas != null) return;
            EnsureEventSystem();

            Font font = KurokageUiFont.Default;
            if (font == null)
            {
                Debug.LogError("RENKAI agent selection could not resolve a runtime font.");
                return;
            }

            GameObject root = new GameObject("KUROKAGE_AGENT_SELECTION");
            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            Image background = CreateImage("BACKDROP", root.transform, new Color(0.012f, 0.022f, 0.050f, 0.76f));
            Stretch(background.rectTransform);

            Image topGlow = CreateImage("TOP_GLOW", root.transform, new Color(0.08f, 0.34f, 0.72f, 0.14f));
            SetAnchors(topGlow.rectTransform, new Vector2(0f, 0.80f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            RectTransform header = CreatePanel(
                "HEADER",
                root.transform,
                new Vector2(0.055f, 0.835f),
                new Vector2(0.945f, 0.955f),
                new Color(0.025f, 0.045f, 0.085f, 0.97f));

            Text headerLabel = CreateText("HEADER_TEXT", header, font, 34, TextAnchor.MiddleLeft, Color.white);
            headerLabel.text = "SELECT RESONANT AGENT";
            headerLabel.rectTransform.offsetMin = new Vector2(30f, 0f);
            headerLabel.rectTransform.offsetMax = new Vector2(-500f, 0f);

            Text sub = CreateText("HEADER_SUB", header, font, 16, TextAnchor.MiddleRight, new Color(0.55f, 0.72f, 1f));
            sub.text = "RENKAI COMBAT LINK // エージェント選択";
            sub.rectTransform.offsetMin = new Vector2(500f, 0f);
            sub.rectTransform.offsetMax = new Vector2(-30f, 0f);

            RectTransform roster = CreatePanel(
                "ROSTER",
                root.transform,
                new Vector2(0.055f, 0.135f),
                new Vector2(0.435f, 0.800f),
                new Color(0.030f, 0.050f, 0.090f, 0.97f));

            GridLayoutGroup grid = roster.gameObject.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(24, 24, 24, 24);
            grid.cellSize = new Vector2(295f, 106f);
            grid.spacing = new Vector2(16f, 14f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;

            IReadOnlyList<KurokageAgentDefinition> agents = KurokageAgentCatalog.All;
            for (int i = 0; i < agents.Count; i++)
            {
                int index = i;
                string hotkey = i == 9 ? "0" : (i + 1).ToString();
                Button button = CreateButton(
                    "AGENT_" + agents[i].DisplayName,
                    roster,
                    font,
                    hotkey + "   " + agents[i].DisplayName + "\n     " + agents[i].Role);
                button.onClick.AddListener(() => Preview((KurokageAgentArchetype)index));
                agentButtons.Add(button);
            }

            RectTransform detail = CreatePanel(
                "DETAIL",
                root.transform,
                new Vector2(0.455f, 0.135f),
                new Vector2(0.945f, 0.800f),
                new Color(0.030f, 0.050f, 0.090f, 0.97f));

            titleText = CreateText("AGENT_TITLE", detail, font, 42, TextAnchor.UpperLeft, Color.white);
            SetAnchors(titleText.rectTransform, new Vector2(0f, 0.77f), new Vector2(1f, 0.97f), new Vector2(38f, 0f), new Vector2(-38f, 0f));

            roleText = CreateText("ROLE", detail, font, 18, TextAnchor.UpperLeft, new Color(0.58f, 0.76f, 1f));
            SetAnchors(roleText.rectTransform, new Vector2(0f, 0.67f), new Vector2(1f, 0.79f), new Vector2(38f, 0f), new Vector2(-38f, 0f));

            accentLine = CreateImage("ACCENT", detail, Color.cyan);
            SetAnchors(accentLine.rectTransform, new Vector2(0f, 0.635f), new Vector2(1f, 0.645f), new Vector2(38f, 0f), new Vector2(-38f, 0f));

            abilityText = CreateText("ABILITIES", detail, font, 22, TextAnchor.UpperLeft, Color.white);
            SetAnchors(abilityText.rectTransform, new Vector2(0f, 0.31f), new Vector2(1f, 0.61f), new Vector2(38f, 0f), new Vector2(-38f, 0f));
            abilityText.lineSpacing = 1.35f;

            voiceText = CreateText("VOICE", detail, font, 18, TextAnchor.UpperLeft, new Color(0.74f, 0.79f, 0.90f));
            SetAnchors(voiceText.rectTransform, new Vector2(0f, 0.13f), new Vector2(1f, 0.29f), new Vector2(38f, 0f), new Vector2(-38f, 0f));

            confirmButton = CreateButton("CONFIRM", detail, font, "ESTABLISH LINK  [ENTER]");
            RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
            SetAnchors(confirmRect, new Vector2(0.52f, 0.025f), new Vector2(0.96f, 0.115f), Vector2.zero, Vector2.zero);
            confirmButton.onClick.AddListener(ConfirmSelection);

            selectionHintText = CreateText("SELECTION_HINT", root.transform, font, 16, TextAnchor.MiddleCenter, new Color(0.62f, 0.78f, 1f));
            SetAnchors(selectionHintText.rectTransform, new Vector2(0.10f, 0.035f), new Vector2(0.90f, 0.095f), Vector2.zero, Vector2.zero);
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            Image image = CreateImage(name, parent, color);
            SetAnchors(image.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            return image.rectTransform;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, Font font, int size, TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            Stretch(text.rectTransform);
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label)
        {
            Image image = CreateImage(name, parent, new Color(0.10f, 0.14f, 0.22f, 0.96f));
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.18f, 0.30f, 0.48f, 1f);
            colors.pressedColor = new Color(0.30f, 0.46f, 0.68f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text text = CreateText("LABEL", image.transform, font, 20, TextAnchor.MiddleLeft, Color.white);
            text.rectTransform.offsetMin = new Vector2(22f, 8f);
            text.rectTransform.offsetMax = new Vector2(-12f, -8f);
            text.text = label;
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            GameObject eventSystem = new GameObject("RENKAI_EVENT_SYSTEM");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
