using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageEliteHUD : MonoBehaviour
    {
        private RenkaiWeaponController weapon;
        private RenkaiRoundPlayer player;
        private RenkaiRoundManager roundManager;
        private CharacterController controller;
        private KurokageArmor armor;

        private Text healthText;
        private Text armorText;
        private Text ammoText;
        private Text reserveText;
        private Text weaponText;
        private Text scoreText;
        private Text aliveText;
        private Text reloadText;
        private Image healthFill;
        private Image armorFill;
        private RectTransform crosshairRoot;

        private void Awake()
        {
            weapon = FindObjectOfType<RenkaiWeaponController>();
            if (weapon != null)
            {
                player = weapon.GetComponent<RenkaiRoundPlayer>();
                controller = weapon.GetComponent<CharacterController>();
                armor = weapon.GetComponent<KurokageArmor>();
            }
            roundManager = FindObjectOfType<RenkaiRoundManager>();
            Build();
        }

        private void Update()
        {
            float hp = player != null ? player.health : 100f;
            float maxHp = player != null ? player.maxHealth : 100f;
            healthText.text = Mathf.CeilToInt(hp).ToString();
            healthFill.fillAmount = maxHp > 0f ? Mathf.Clamp01(hp / maxHp) : 0f;

            if (armor != null)
            {
                armorText.text = Mathf.CeilToInt(armor.CurrentArmor).ToString();
                armorFill.fillAmount = armor.Armor01;
            }
            else
            {
                armorText.text = "0";
                armorFill.fillAmount = 0f;
            }

            if (weapon != null)
            {
                if (weapon.slot == RenkaiWeaponSlot.Rifle)
                {
                    weaponText.text = "KX-9 KURO";
                    ammoText.text = weapon.rifleAmmo.ToString("00");
                    reserveText.text = weapon.rifleReserve.ToString("000");
                }
                else if (weapon.slot == RenkaiWeaponSlot.Pistol)
                {
                    weaponText.text = "SHIRO SIDEARM";
                    ammoText.text = weapon.pistolAmmo.ToString("00");
                    reserveText.text = weapon.pistolReserve.ToString("000");
                }
                else
                {
                    weaponText.text = "ECLIPSE BLADE";
                    ammoText.text = "BLADE";
                    reserveText.text = string.Empty;
                }

                bool reloadHeld = Input.GetKey(KeyCode.R) && weapon.slot != RenkaiWeaponSlot.Sword;
                reloadText.gameObject.SetActive(reloadHeld);
                reloadText.text = reloadHeld ? "RELOAD" : string.Empty;
            }

            if (roundManager != null)
                scoreText.text = "ATK " + roundManager.attackersScore + "  -  " + roundManager.defendersScore + " DEF";

            int atkAlive = 0, atkTotal = 0, defAlive = 0, defTotal = 0;
            foreach (RenkaiRoundPlayer p in FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (p.team == RenkaiTeam.Attackers)
                {
                    atkTotal++;
                    if (p.isAlive) atkAlive++;
                }
                else
                {
                    defTotal++;
                    if (p.isAlive) defAlive++;
                }
            }
            aliveText.text = "A " + atkAlive + "/" + atkTotal + "   D " + defAlive + "/" + defTotal;

            float speed = controller == null ? 0f : new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
            float target = 10f + Mathf.Clamp(speed * 1.8f, 0f, 18f);
            crosshairRoot.sizeDelta = Vector2.Lerp(crosshairRoot.sizeDelta, Vector2.one * target, 14f * Time.deltaTime);
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();

            Color panel = new Color(0.025f, 0.04f, 0.07f, 0.82f);
            Color text = new Color(0.95f, 0.98f, 1f, 1f);
            Color accent = new Color(0.16f, 0.48f, 1f, 1f);
            Color armorColor = new Color(0.40f, 0.76f, 1f, 1f);

            GameObject top = Panel("TOP_CENTER", transform, new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(620f, 62f), panel);
            scoreText = Label("Score", top.transform, Vector2.zero, new Vector2(360f, 30f), 24, TextAnchor.MiddleCenter, text);
            aliveText = Label("Alive", top.transform, new Vector2(0f, -20f), new Vector2(360f, 22f), 16, TextAnchor.MiddleCenter, accent);

            GameObject left = Panel("BOTTOM_LEFT", transform, Vector2.zero, new Vector2(150f, 96f), new Vector2(390f, 112f), panel);
            Label("HealthTitle", left.transform, new Vector2(-130f, 28f), new Vector2(120f, 20f), 15, TextAnchor.MiddleLeft, accent).text = "HEALTH";
            healthText = Label("HealthValue", left.transform, new Vector2(-126f, -3f), new Vector2(100f, 42f), 38, TextAnchor.MiddleLeft, text);
            GameObject hpBg = Panel("HealthBG", left.transform, new Vector2(0.5f, 0.5f), new Vector2(55f, 6f), new Vector2(190f, 14f), new Color(0.08f,0.11f,0.17f,0.95f));
            healthFill = Fill("HealthFill", hpBg.transform, accent);

            Label("ArmorTitle", left.transform, new Vector2(-130f, -34f), new Vector2(120f, 20f), 14, TextAnchor.MiddleLeft, armorColor).text = "ARMOR";
            armorText = Label("ArmorValue", left.transform, new Vector2(-48f, -34f), new Vector2(70f, 20f), 18, TextAnchor.MiddleLeft, armorColor);
            GameObject armorBg = Panel("ArmorBG", left.transform, new Vector2(0.5f, 0.5f), new Vector2(55f, -34f), new Vector2(190f, 10f), new Color(0.08f,0.11f,0.17f,0.95f));
            armorFill = Fill("ArmorFill", armorBg.transform, armorColor);

            GameObject right = Panel("BOTTOM_RIGHT", transform, Vector2.one, new Vector2(-150f, -90f), new Vector2(420f, 110f), panel);
            weaponText = Label("Weapon", right.transform, new Vector2(-110f, 28f), new Vector2(260f, 24f), 18, TextAnchor.MiddleLeft, accent);
            ammoText = Label("Ammo", right.transform, new Vector2(-110f, -12f), new Vector2(120f, 48f), 42, TextAnchor.MiddleLeft, text);
            reserveText = Label("Reserve", right.transform, new Vector2(65f, -12f), new Vector2(90f, 30f), 22, TextAnchor.MiddleLeft, new Color(0.72f,0.78f,0.88f,1f));
            reloadText = Label("Reload", right.transform, new Vector2(100f, 28f), new Vector2(150f, 22f), 16, TextAnchor.MiddleLeft, new Color(1f,0.72f,0.28f,1f));

            crosshairRoot = new GameObject("Crosshair").AddComponent<RectTransform>();
            crosshairRoot.SetParent(transform, false);
            crosshairRoot.anchorMin = crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRoot.sizeDelta = Vector2.one * 10f;
            CrosshairLine(new Vector2(0f, 7f), new Vector2(2f, 6f));
            CrosshairLine(new Vector2(0f, -7f), new Vector2(2f, 6f));
            CrosshairLine(new Vector2(7f, 0f), new Vector2(6f, 2f));
            CrosshairLine(new Vector2(-7f, 0f), new Vector2(6f, 2f));
        }

        private static Image Fill(string name, Transform parent, Color color)
        {
            Image fill = new GameObject(name).AddComponent<Image>();
            fill.transform.SetParent(parent, false);
            fill.color = color;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            RectTransform rt = fill.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return fill;
        }

        private static GameObject Panel(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Text Label(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            Text t = new GameObject(name).AddComponent<Text>();
            t.transform.SetParent(parent, false);
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            RectTransform rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return t;
        }

        private void CrosshairLine(Vector2 pos, Vector2 size)
        {
            Image image = new GameObject("Line").AddComponent<Image>();
            image.transform.SetParent(crosshairRoot, false);
            image.color = Color.white;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
    }
}
