using System;
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
        private bool restoringGameplay;

        public bool IsOpen => open;

        private void Awake()
        {
            ResolveReferences();
        }

        private IEnumerator Start()
        {
            yield return null;
            ResolveReferences();
            if (roundPlayer != null && roundPlayer.isHumanPlayer && showOnStart)
                OpenSelection();
        }

        private void Update()
        {
            ResolveReferences();
            if (roundPlayer == null || !roundPlayer.isHumanPlayer) return;

            if (!open)
            {
                if (Input.GetKeyDown(reopenKey)) OpenSelection();
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                if (PressedAgentHotkey(i))
                {
                    Preview((KurokageAgentArchetype)i);
                    SelectUiButton(i);
                }
            }

            float scroll = Input.mouseScrollDelta.y;
            if (scroll > 0.01f) CyclePreview(-1);
            else if (scroll < -0.01f) CyclePreview(1);

            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmSelection();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSelection();
            }
        }

        private void OnDisable()
        {
            if (open) RestoreGameplayAfterSelection();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) return;
            if (open)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (roundPlayer != null && roundPlayer.isHumanPlayer && roundPlayer.isAlive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OpenSelection()
        {
            if (open) return;
            ResolveReferences();
            previewArchetype = identity != null ? identity.Archetype : KurokageAgentArchetype.Kairi;
            BuildCanvasIfNeeded();
            if (canvas == null) return;

            open = true;
            canvas.gameObject.SetActive(true);
            SetGameplayLocked(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Preview(previewArchetype);
            SelectUiButton((int)previewArchetype);
        }

        private void ConfirmSelection()
        {
            if (!open) return;

            try
            {
                if (identity != null)
                    identity.Select(previewArchetype);
            }
            catch (Exception exception)
            {
                Debug.LogError("RENKAI agent selection failed, gameplay input was restored: " + exception);
            }
            finally
            {
                RestoreGameplayAfterSelection();
            }
        }

        private void CancelSelection()
        {
            if (!open) return;
            if (identity != null) previewArchetype = identity.Archetype;
            RestoreGameplayAfterSelection();
        }

        private void RestoreGameplayAfterSelection()
        {
            if (restoringGameplay) return;
            restoringGameplay = true;

            open = false;
            if (canvas != null) canvas.gameObject.SetActive(false);
            SetGameplayLocked(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            restoringGameplay = false;
        }

        private void SetGameplayLocked(bool locked)
        {
            ResolveReferences();

            bool canControl = roundPlayer == null || roundPlayer.isAlive;

            if (fps != null)
            {
                fps.enabled = canControl;
                fps.SetInputLocked(locked || !canControl);
            }

            if (weapon != null)
            {
                weapon.enabled = canControl;
                weapon.SetExternalFireLock(locked || !canControl);
            }

            if (locked || !canControl)
            {
                if (kairi != null) kairi.enabled = false;
                if (genericAbilities != null) genericAbilities.enabled = false;
                return;
            }

            if (identity != null)
            {
                if (kairi != null) kairi.enabled = identity.Archetype == KurokageAgentArchetype.Kairi;
                if (genericAbilities != null) genericAbilities.enabled = identity.Archetype != KurokageAgentArchetype.Kairi;
            }
        }

        private void ResolveReferences()
        {
            if (identity == null) identity = GetComponent<KurokageAgentIdentity>();
            if (roundPlayer == null) roundPlayer = GetComponent<RenkaiRoundPlayer>();

            if (fps == null)
            {
                fps = GetComponent<RenkaiFPSController>();
                if (fps == null) fps = GetComponentInChildren<RenkaiFPSController>(true);
            }

            if (weapon == null)
            {
                weapon = GetComponent<RenkaiWeaponController>();
                if (weapon == null) weapon = GetComponentInChildren<RenkaiWeaponController>(true);
            }

            if (kairi == null)
            {
                kairi = GetComponent<KairiAbilityController>();
                if (kairi == null) kairi = GetComponentInChildren<KairiAbilityController>(true);
            }

            if (genericAbilities == null)
            {
                genericAbilities = GetComponent<KurokageAgentAbilityController>();
                if (genericAbilities == null) genericAbilities = GetComponentInChildren<KurokageAgentAbilityController>(true);
            }
        }

        private static bool PressedAgentHotkey(int index)
        {
            switch (index)
            {
                case 0: return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
                case 1: return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
                case 2: return Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
                case 3: return Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4);
                case 4: return Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5);
                case 5: return Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6);
                case 6: return Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7);
                case 7: return Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8);
                case 8: return Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9);
                case 9: return Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0);
                default: return false;
            }
        }

        private void CyclePreview(int direction)
        {
            int next = ((int)previewArchetype + direction + 10) % 10;
            Preview((KurokageAgentArchetype)next);
            SelectUiButton(next);
        }

        private void SelectUiButton(int index)
        {
            if (index < 0 || index >= agentButtons.Count) return;
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(agentButtons[index].gameObject);
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
                selectionHintText.text = "1–0 / NUMPAD PREVIEW   •   ENTER OR SPACE CONFIRM   •   ESC CANCEL   •   " + definition.DisplayName;

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

            RectTransform header = CreatePanel("HEADER", root.transform, new Vector2(0.055f, 0.835f), new Vector2(0.945f, 0.955f), new Color(0.025f, 0.045f, 0.085f, 0.97f));
            Text headerLabel = CreateText("HEADER_TEXT", header, font, 34, TextAnchor.MiddleLeft, Color.white);
            headerLabel.text = "SELECT RESONANT AGENT";
            headerLabel.rectTransform.offsetMin = new Vector2(30f, 0f);
            headerLabel.rectTransform.offsetMax = new Vector2(-500f, 0f);

            Text sub = CreateText("HEADER_SUB", header, font, 16, TextAnchor.MiddleRight, new Color(0.55f, 0.72f, 1f));
            sub.text = "RENKAI COMBAT LINK // エージェント選択";
            sub.rectTransform.offsetMin = new Vector2(500f, 0f);
            sub.rectTransform.offsetMax = new Vector2(-30f, 0f);

            RectTransform roster = CreatePanel("ROSTER", root.transform, new Vector2(0.055f, 0.135f), new Vector2(0.435f, 0.800f), new Color(0.030f, 0.050f, 0.090f, 0.97f));
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
                Button button = CreateButton("AGENT_" + agents[i].DisplayName, roster, font, hotkey + "   " + agents[i].DisplayName + "\n     " + agents[i].Role);
                button.onClick.AddListener(() => Preview((KurokageAgentArchetype)index));
                agentButtons.Add(button);
            }

            RectTransform detail = CreatePanel("DETAIL", root.transform, new Vector2(0.455f, 0.135f), new Vector2(0.945f, 0.800f), new Color(0.030f, 0.050f, 0.090f, 0.97f));
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

            confirmButton = CreateButton("CONFIRM", detail, font, "ESTABLISH LINK  [ENTER / SPACE]");
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
            EventSystem eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("RENKAI_EVENT_SYSTEM");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<BaseInputModule>() == null)
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }
}
