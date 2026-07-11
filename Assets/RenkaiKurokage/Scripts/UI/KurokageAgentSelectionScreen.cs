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

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ConfirmSelection();
        }

        public void OpenSelection()
        {
            if (open) return;
            open = true;
            previewArchetype = identity != null ? identity.Archetype : KurokageAgentArchetype.Kairi;
            BuildCanvasIfNeeded();
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

            for (int i = 0; i < agentButtons.Count; i++)
            {
                ColorBlock colors = agentButtons[i].colors;
                colors.normalColor = i == (int)archetype
                    ? Color.Lerp(definition.Accent, Color.white, 0.15f)
                    : new Color(0.10f, 0.14f, 0.22f, 0.96f);
                agentButtons[i].colors = colors;
            }
        }

        private void BuildCanvasIfNeeded()
        {
            if (canvas != null) return;
            EnsureEventSystem();

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            GameObject root = new GameObject("KUROKAGE_AGENT_SELECTION");
            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.AddComponent<GraphicRaycaster>();

            Image background = CreateImage("BACKDROP", root.transform, new Color(0.015f, 0.025f, 0.055f, 0.96f));
            Stretch(background.rectTransform);

            RectTransform header = CreatePanel("HEADER", root.transform, new Vector2(120f, -55f), new Vector2(-120f, -170f));
            Text headerLabel = CreateText("HEADER_TEXT", header, font, 34, TextAnchor.MiddleLeft, Color.white);
            headerLabel.text = "SELECT RESONANT AGENT";
            headerLabel.rectTransform.offsetMin = new Vector2(30f, 0f);
            Text sub = CreateText("HEADER_SUB", header, font, 16, TextAnchor.MiddleRight, new Color(0.55f, 0.72f, 1f));
            sub.text = "RENKAI COMBAT LINK // エージェント選択";
            sub.rectTransform.offsetMax = new Vector2(-30f, 0f);

            RectTransform roster = CreatePanel("ROSTER", root.transform, new Vector2(120f, -200f), new Vector2(780f, -900f));
            GridLayoutGroup grid = roster.gameObject.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(24, 24, 24, 24);
            grid.cellSize = new Vector2(295f, 112f);
            grid.spacing = new Vector2(16f, 16f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            IReadOnlyList<KurokageAgentDefinition> agents = KurokageAgentCatalog.All;
            for (int i = 0; i < agents.Count; i++)
            {
                int index = i;
                Button button = CreateButton("AGENT_" + agents[i].DisplayName, roster, font, agents[i].DisplayName + "\n" + agents[i].Role);
                button.onClick.AddListener(() => Preview((KurokageAgentArchetype)index));
                agentButtons.Add(button);
            }

            RectTransform detail = CreatePanel("DETAIL", root.transform, new Vector2(815f, -200f), new Vector2(-120f, -900f));
            titleText = CreateText("AGENT_TITLE", detail, font, 42, TextAnchor.UpperLeft, Color.white);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0.78f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 0.98f);
            titleText.rectTransform.offsetMin = new Vector2(38f, 0f);
            titleText.rectTransform.offsetMax = new Vector2(-38f, 0f);

            roleText = CreateText("ROLE", detail, font, 18, TextAnchor.UpperLeft, new Color(0.58f, 0.76f, 1f));
            roleText.rectTransform.anchorMin = new Vector2(0f, 0.68f);
            roleText.rectTransform.anchorMax = new Vector2(1f, 0.79f);
            roleText.rectTransform.offsetMin = new Vector2(38f, 0f);
            roleText.rectTransform.offsetMax = new Vector2(-38f, 0f);

            accentLine = CreateImage("ACCENT", detail, Color.cyan);
            accentLine.rectTransform.anchorMin = new Vector2(0f, 0.645f);
            accentLine.rectTransform.anchorMax = new Vector2(1f, 0.655f);
            accentLine.rectTransform.offsetMin = new Vector2(38f, 0f);
            accentLine.rectTransform.offsetMax = new Vector2(-38f, 0f);

            abilityText = CreateText("ABILITIES", detail, font, 22, TextAnchor.UpperLeft, Color.white);
            abilityText.rectTransform.anchorMin = new Vector2(0f, 0.31f);
            abilityText.rectTransform.anchorMax = new Vector2(1f, 0.63f);
            abilityText.rectTransform.offsetMin = new Vector2(38f, 0f);
            abilityText.rectTransform.offsetMax = new Vector2(-38f, 0f);
            abilityText.lineSpacing = 1.35f;

            voiceText = CreateText("VOICE", detail, font, 18, TextAnchor.UpperLeft, new Color(0.74f, 0.79f, 0.90f));
            voiceText.rectTransform.anchorMin = new Vector2(0f, 0.13f);
            voiceText.rectTransform.anchorMax = new Vector2(1f, 0.30f);
            voiceText.rectTransform.offsetMin = new Vector2(38f, 0f);
            voiceText.rectTransform.offsetMax = new Vector2(-38f, 0f);

            confirmButton = CreateButton("CONFIRM", detail, font, "ESTABLISH LINK  [ENTER]");
            RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.52f, 0.025f);
            confirmRect.anchorMax = new Vector2(0.96f, 0.115f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;
            confirmButton.onClick.AddListener(ConfirmSelection);
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 min, Vector2 max)
        {
            Image image = CreateImage(name, parent, new Color(0.035f, 0.055f, 0.095f, 0.94f));
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = min;
            rect.offsetMax = max;
            return rect;
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

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            GameObject eventSystem = new GameObject("RENKAI_EVENT_SYSTEM");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
