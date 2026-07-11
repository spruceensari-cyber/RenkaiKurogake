using System;
using System.Collections;
using System.Collections.Generic;
using Renkai.Kurogake;
using UnityEngine;
using UnityEngine.UI;

namespace Renkai.Kurokage
{
    /// <summary>
    /// The only gameplay HUD used by the competitive scene. It owns every visible HUD region.
    /// </summary>
    public sealed class KurokageUnifiedPresentationHUD : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private readonly List<PlayerBlip> playerBlips = new List<PlayerBlip>();
        private readonly Queue<string> killFeed = new Queue<string>();

        private RenkaiRoundPlayer player;
        private RenkaiWeaponController weapon;
        private RenkaiRoundManager roundManager;
        private KurokageArmor armor;
        private KairiAbilityController abilities;
        private KurokageZodiacObjectiveController objective;
        private ZodiacCoreRuntime core;
        private ZodiacNexusSite[] nexusSites;

        private Text locationText;
        private Text roundText;
        private Text timerText;
        private Text attackersScoreText;
        private Text defendersScoreText;
        private Text attackersAliveText;
        private Text defendersAliveText;
        private Text objectiveTitleText;
        private Text objectiveStateText;
        private Image objectiveProgress;
        private Text healthText;
        private Text armorText;
        private Image healthFill;
        private Image armorFill;
        private Text agentText;
        private Text weaponText;
        private Text ammoText;
        private Text reserveText;
        private Text reloadText;
        private Image reloadFill;
        private RectTransform crosshairRoot;
        private CanvasGroup hitGroup;
        private Image hitMarker;
        private Text hitText;
        private Text bannerText;
        private CanvasGroup bannerGroup;
        private Text killFeedText;
        private Text scoreboardText;
        private GameObject scoreboardRoot;
        private Image coreBlip;
        private readonly List<Image> nexusBlips = new List<Image>();
        private Image[] abilityFills;
        private Text[] abilityLabels;
        private float nextRosterUpdate;
        private Coroutine hitRoutine;
        private Coroutine bannerRoutine;

        private sealed class PlayerBlip
        {
            public RenkaiRoundPlayer Player;
            public Image Image;
        }

        private void Awake()
        {
            DisableLegacyHud();
            ResolveReferences();
            Build();
        }

        private void OnEnable()
        {
            KurokageGameEvents.KillFeed += OnKillFeed;
            KurokageGameEvents.RoundBanner += OnRoundBanner;
            if (weapon != null)
                weapon.HitConfirmed += OnHitConfirmed;
        }

        private void OnDisable()
        {
            KurokageGameEvents.KillFeed -= OnKillFeed;
            KurokageGameEvents.RoundBanner -= OnRoundBanner;
            if (weapon != null)
                weapon.HitConfirmed -= OnHitConfirmed;
        }

        private void Update()
        {
            ResolveReferences();
            UpdateTopBar();
            UpdateObjective();
            UpdateVitals();
            UpdateWeapon();
            UpdateAbilities();
            UpdateCrosshair();
            UpdateRadar();
            UpdateScoreboard();
        }

        private void ResolveReferences()
        {
            if (weapon == null)
            {
                weapon = FindObjectOfType<RenkaiWeaponController>();
                if (weapon != null)
                {
                    player = weapon.GetComponent<RenkaiRoundPlayer>();
                    armor = weapon.GetComponent<KurokageArmor>();
                    abilities = weapon.GetComponent<KairiAbilityController>();
                    weapon.HitConfirmed -= OnHitConfirmed;
                    weapon.HitConfirmed += OnHitConfirmed;
                }
            }

            if (player == null)
            {
                foreach (RenkaiRoundPlayer candidate in FindObjectsOfType<RenkaiRoundPlayer>(true))
                {
                    if (candidate.isHumanPlayer)
                    {
                        player = candidate;
                        armor = candidate.GetComponent<KurokageArmor>();
                        abilities = candidate.GetComponent<KairiAbilityController>();
                        break;
                    }
                }
            }

            if (roundManager == null)
                roundManager = FindObjectOfType<RenkaiRoundManager>();
            if (objective == null)
                objective = FindObjectOfType<KurokageZodiacObjectiveController>();
            if (core == null)
                core = FindObjectOfType<ZodiacCoreRuntime>();
            if (nexusSites == null || nexusSites.Length == 0)
                nexusSites = FindObjectsOfType<ZodiacNexusSite>(true);

            if (Time.unscaledTime >= nextRosterUpdate)
            {
                nextRosterUpdate = Time.unscaledTime + 0.7f;
                RefreshPlayerBlips();
            }
        }

        private void DisableLegacyHud()
        {
            DisableLegacy<KurokageCompetitiveHUD>();
            DisableLegacy<KurokageEliteHUD>();
            DisableLegacy<KurokageTacticalRadarHUD>();
            DisableLegacy<KurokageCombatFeedbackHUD>();
            DisableLegacy<KurokageDamageDirectionHUD>();
            DisableLegacy<KurokageMatchPresentationHUD>();
            DisableLegacy<KurokageScoreboardHUD>();
            DisableLegacy<KurokageZodiacHUD>();
            DisableLegacy<KurokageAbilityHUD>();
        }

        private void DisableLegacy<T>() where T : Behaviour
        {
            foreach (T legacy in FindObjectsOfType<T>(true))
            {
                if (legacy != null && legacy.gameObject != gameObject)
                    legacy.gameObject.SetActive(false);
            }
        }

        private void UpdateTopBar()
        {
            int attackersAlive = 0;
            int defendersAlive = 0;
            int attackersTotal = 0;
            int defendersTotal = 0;
            foreach (RenkaiRoundPlayer contestant in FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                if (contestant.team == RenkaiTeam.Attackers)
                {
                    attackersTotal++;
                    if (contestant.isAlive) attackersAlive++;
                }
                else
                {
                    defendersTotal++;
                    if (contestant.isAlive) defendersAlive++;
                }
            }

            attackersScoreText.text = roundManager != null ? roundManager.attackersScore.ToString() : "0";
            defendersScoreText.text = roundManager != null ? roundManager.defendersScore.ToString() : "0";
            attackersAliveText.text = "ALPHA  " + attackersAlive + "/" + attackersTotal;
            defendersAliveText.text = "OMEGA  " + defendersAlive + "/" + defendersTotal;
            roundText.text = roundManager != null ? "ROUND " + roundManager.roundNumber : "ROUND --";
            timerText.text = roundManager != null ? FormatTime(roundManager.TimeRemaining) : "--:--";
            locationText.text = "KUROGATE DISTRICT // LINK A";
        }

        private void UpdateObjective()
        {
            if (objective == null || core == null)
            {
                objectiveTitleText.text = "ZODIAC CORE";
                objectiveStateText.text = "NETWORK OFFLINE";
                objectiveProgress.fillAmount = 0f;
                return;
            }

            objectiveTitleText.text = "ZODIAC CORE";
            objectiveStateText.text = objective.StatusText;
            objectiveProgress.fillAmount = core.State == ZodiacLinkState.Linking ||
                                           core.State == ZodiacLinkState.Synchronized ||
                                           core.State == ZodiacLinkState.Severing
                ? core.Progress01
                : 0f;
        }

        private void UpdateVitals()
        {
            float health = player != null ? player.health : 0f;
            float maxHealth = player != null ? player.maxHealth : 100f;
            float health01 = maxHealth <= 0f ? 0f : Mathf.Clamp01(health / maxHealth);
            healthText.text = Mathf.CeilToInt(health).ToString();
            healthFill.fillAmount = health01;
            healthFill.color = health01 <= 0.28f
                ? new Color(1f, 0.26f, 0.42f, 1f)
                : new Color(0.15f, 0.58f, 1f, 1f);

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

            agentText.text = player != null ? AgentDisplayName(player.agentName) : "KAIRI";
        }

        private void UpdateWeapon()
        {
            if (weapon == null)
            {
                weaponText.text = "NO WEAPON LINK";
                ammoText.text = "--";
                reserveText.text = "---";
                reloadText.gameObject.SetActive(false);
                return;
            }

            weaponText.text = weapon.ActiveWeaponName;
            ammoText.text = weapon.slot == RenkaiWeaponSlot.Sword ? "--" : weapon.CurrentAmmo.ToString("00");
            reserveText.text = weapon.slot == RenkaiWeaponSlot.Sword ? "BLADE" : weapon.CurrentReserve.ToString("000");
            reloadText.gameObject.SetActive(weapon.IsReloading);
            if (weapon.IsReloading)
            {
                float normalized = weapon.ReloadNormalized;
                reloadText.text = "RELOADING " + Mathf.RoundToInt(normalized * 100f) + "%";
                reloadFill.fillAmount = normalized;
            }
        }

        private void UpdateAbilities()
        {
            if (abilityFills == null || abilityLabels == null)
                return;

            float[] cooldowns =
            {
                abilities != null ? abilities.QCooldown01 : 0f,
                abilities != null ? abilities.ECooldown01 : 0f,
                abilities != null ? abilities.CCooldown01 : 0f,
                abilities != null ? abilities.XCooldown01 : 0f
            };

            for (int i = 0; i < abilityFills.Length; i++)
            {
                abilityFills[i].fillAmount = 1f - cooldowns[i];
                abilityLabels[i].color = cooldowns[i] <= 0.01f
                    ? new Color(0.89f, 0.96f, 1f, 1f)
                    : new Color(0.43f, 0.53f, 0.68f, 1f);
            }
        }

        private void UpdateCrosshair()
        {
            if (crosshairRoot == null)
                return;

            CharacterController controller = player != null ? player.GetComponent<CharacterController>() : null;
            float speed = controller != null ? new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude : 0f;
            float target = 10f + Mathf.Clamp(speed * 1.75f, 0f, 18f);
            crosshairRoot.sizeDelta = Vector2.Lerp(crosshairRoot.sizeDelta, Vector2.one * target, Time.unscaledDeltaTime * 16f);
        }

        private void UpdateRadar()
        {
            foreach (PlayerBlip blip in playerBlips)
            {
                if (blip.Player == null || blip.Image == null)
                    continue;

                blip.Image.rectTransform.anchoredPosition = RadarPosition(blip.Player.transform.position);
                blip.Image.color = !blip.Player.isAlive
                    ? new Color(0.35f, 0.42f, 0.52f, 0.35f)
                    : blip.Player == player
                        ? new Color(0.94f, 0.98f, 1f, 1f)
                        : blip.Player.team == RenkaiTeam.Attackers
                            ? new Color(0.18f, 0.65f, 1f, 1f)
                            : new Color(0.90f, 0.38f, 0.78f, 1f);
            }

            if (coreBlip != null && core != null)
            {
                coreBlip.rectTransform.anchoredPosition = RadarPosition(core.transform.position);
                coreBlip.rectTransform.localScale = Vector3.one * (0.8f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f)) * 0.3f);
            }

            for (int i = 0; i < nexusBlips.Count && nexusSites != null && i < nexusSites.Length; i++)
            {
                if (nexusSites[i] != null)
                    nexusBlips[i].rectTransform.anchoredPosition = RadarPosition(nexusSites[i].transform.position);
            }
        }

        private void UpdateScoreboard()
        {
            if (scoreboardRoot == null)
                return;

            bool visible = Input.GetKey(KeyCode.Tab);
            if (scoreboardRoot.activeSelf != visible)
                scoreboardRoot.SetActive(visible);
            if (!visible)
                return;

            List<RenkaiRoundPlayer> roster = new List<RenkaiRoundPlayer>(FindObjectsOfType<RenkaiRoundPlayer>(true));
            roster.Sort((a, b) => a.team == b.team ? string.CompareOrdinal(a.agentName, b.agentName) : a.team.CompareTo(b.team));
            List<string> rows = new List<string>();
            foreach (RenkaiRoundPlayer contestant in roster)
            {
                string side = contestant.team == RenkaiTeam.Attackers ? "ALPHA" : "OMEGA";
                string state = contestant.isAlive ? "ONLINE" : "DOWN";
                rows.Add(side.PadRight(8) + contestant.agentName.PadRight(24) + " " + Mathf.CeilToInt(contestant.health).ToString("000") + "  " + state);
            }
            scoreboardText.text = string.Join("\n", rows.ToArray());
        }

        private void RefreshPlayerBlips()
        {
            if (playerBlips.Count > 0)
                return;

            foreach (RenkaiRoundPlayer contestant in FindObjectsOfType<RenkaiRoundPlayer>(true))
            {
                Image icon = CreateImage("AgentBlip", radarLayer, Vector2.zero, new Vector2(10f, 10f), Color.white);
                icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                playerBlips.Add(new PlayerBlip { Player = contestant, Image = icon });
            }
        }

        private RectTransform radarLayer;

        private Vector2 RadarPosition(Vector3 world)
        {
            const float minX = -54f;
            const float maxX = 54f;
            const float minZ = -74f;
            const float maxZ = 74f;
            float x = Mathf.InverseLerp(minX, maxX, world.x);
            float y = Mathf.InverseLerp(minZ, maxZ, world.z);
            return new Vector2((x - 0.5f) * 240f, (y - 0.5f) * 240f);
        }

        private void OnHitConfirmed(bool headshot)
        {
            if (hitRoutine != null)
                StopCoroutine(hitRoutine);
            hitRoutine = StartCoroutine(HitFeedbackRoutine(headshot));
        }

        private void OnKillFeed(string killer, string victim)
        {
            killFeed.Enqueue(killer + "  >  " + victim);
            while (killFeed.Count > 5)
                killFeed.Dequeue();
            killFeedText.text = string.Join("\n", killFeed.ToArray());
        }

        private void OnRoundBanner(string message)
        {
            if (bannerRoutine != null)
                StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(BannerRoutine(message));
        }

        private IEnumerator HitFeedbackRoutine(bool headshot)
        {
            hitText.gameObject.SetActive(headshot);
            hitText.text = headshot ? "NEURAL COLLAPSE" : string.Empty;
            hitMarker.color = headshot ? new Color(1f, 0.22f, 0.44f, 1f) : new Color(0.93f, 0.98f, 1f, 1f);
            hitGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(headshot ? 0.20f : 0.08f);

            float elapsed = 0f;
            while (elapsed < 0.14f)
            {
                elapsed += Time.unscaledDeltaTime;
                hitGroup.alpha = 1f - elapsed / 0.14f;
                yield return null;
            }
            hitText.gameObject.SetActive(false);
            hitGroup.alpha = 0f;
        }

        private IEnumerator BannerRoutine(string message)
        {
            bannerText.text = message;
            bannerGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(1.25f);

            float elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.unscaledDeltaTime;
                bannerGroup.alpha = 1f - elapsed / 0.35f;
                yield return null;
            }
        }

        private void Build()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            Color panel = new Color(0.018f, 0.034f, 0.066f, 0.86f);
            Color panelSoft = new Color(0.022f, 0.046f, 0.086f, 0.66f);
            Color white = new Color(0.93f, 0.97f, 1f, 1f);
            Color cyan = new Color(0.20f, 0.66f, 1f, 1f);
            Color violet = new Color(0.78f, 0.37f, 1f, 1f);

            BuildTopBar(panel, white, cyan, violet);
            BuildRadar(panelSoft, cyan, violet);
            BuildObjective(panel, white, violet);
            BuildVitals(panel, white, cyan);
            BuildWeapon(panel, white, cyan);
            BuildAbilities(panel, white, cyan, violet);
            BuildFeedback(white);
            BuildBanner(panel, white);
            BuildKillFeed(panelSoft, white);
            BuildScoreboard(panel, white, cyan, violet);
        }

        private void BuildTopBar(Color panel, Color white, Color cyan, Color violet)
        {
            GameObject root = CreatePanel("TOP_MATCH", transform, new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(820f, 84f), panel);
            CreateImage("AlphaRail", root.transform, new Vector2(-255f, -37f), new Vector2(186f, 3f), cyan);
            CreateImage("OmegaRail", root.transform, new Vector2(255f, -37f), new Vector2(186f, 3f), violet);
            CreateImage("CenterDivider", root.transform, new Vector2(0f, -4f), new Vector2(2f, 55f), new Color(0.48f, 0.58f, 0.72f, 0.46f));
            locationText = CreateText("Location", root.transform, new Vector2(-365f, 22f), new Vector2(190f, 20f), 11, TextAnchor.MiddleLeft, cyan);
            roundText = CreateText("Round", root.transform, new Vector2(0f, 23f), new Vector2(170f, 22f), 16, TextAnchor.MiddleCenter, white);
            timerText = CreateText("Timer", root.transform, new Vector2(0f, -9f), new Vector2(150f, 36f), 32, TextAnchor.MiddleCenter, white);
            attackersScoreText = CreateText("AttackersScore", root.transform, new Vector2(-204f, -2f), new Vector2(74f, 52f), 42, TextAnchor.MiddleCenter, cyan);
            defendersScoreText = CreateText("DefendersScore", root.transform, new Vector2(204f, -2f), new Vector2(74f, 52f), 42, TextAnchor.MiddleCenter, violet);
            attackersAliveText = CreateText("AttackersAlive", root.transform, new Vector2(-274f, -25f), new Vector2(170f, 18f), 12, TextAnchor.MiddleCenter, cyan);
            defendersAliveText = CreateText("DefendersAlive", root.transform, new Vector2(274f, -25f), new Vector2(170f, 18f), 12, TextAnchor.MiddleCenter, violet);
        }

        private void BuildRadar(Color panel, Color cyan, Color violet)
        {
            GameObject root = CreatePanel("TACTICAL_GRID", transform, new Vector2(0f, 1f), new Vector2(24f, -28f), new Vector2(270f, 300f), panel, new Vector2(0f, 1f));
            CreateText("Title", root.transform, new Vector2(0f, 133f), new Vector2(230f, 24f), 12, TextAnchor.MiddleLeft, cyan).text = "CELESTIAL NETWORK // LOCAL GRID";
            radarLayer = new GameObject("RadarLayer", typeof(RectTransform)).GetComponent<RectTransform>();
            radarLayer.SetParent(root.transform, false);
            radarLayer.anchorMin = radarLayer.anchorMax = new Vector2(0.5f, 0.5f);
            radarLayer.anchoredPosition = new Vector2(0f, -8f);
            radarLayer.sizeDelta = new Vector2(250f, 250f);

            for (int i = 1; i < 4; i++)
            {
                float t = i / 4f;
                CreateImage("GridV", radarLayer, new Vector2((t - 0.5f) * 240f, 0f), new Vector2(1f, 240f), new Color(cyan.r, cyan.g, cyan.b, 0.20f));
                CreateImage("GridH", radarLayer, new Vector2(0f, (t - 0.5f) * 240f), new Vector2(240f, 1f), new Color(cyan.r, cyan.g, cyan.b, 0.20f));
            }

            coreBlip = CreateImage("CoreBlip", radarLayer, Vector2.zero, new Vector2(13f, 13f), violet);
            for (int i = 0; i < 2; i++)
                nexusBlips.Add(CreateImage("NexusBlip", radarLayer, Vector2.zero, new Vector2(10f, 10f), i == 0 ? cyan : violet));
        }

        private void BuildObjective(Color panel, Color white, Color violet)
        {
            GameObject root = CreatePanel("OBJECTIVE", transform, new Vector2(1f, 0.62f), new Vector2(-34f, 0f), new Vector2(330f, 102f), panel, new Vector2(1f, 0.5f));
            objectiveTitleText = CreateText("Title", root.transform, new Vector2(-122f, 25f), new Vector2(190f, 22f), 16, TextAnchor.MiddleLeft, violet);
            objectiveStateText = CreateText("State", root.transform, new Vector2(-122f, -5f), new Vector2(260f, 35f), 13, TextAnchor.MiddleLeft, white);
            GameObject background = CreatePanel("ProgressBackground", root.transform, new Vector2(0.5f, 0.5f), new Vector2(12f, -34f), new Vector2(270f, 9f), new Color(0.08f, 0.13f, 0.21f, 1f));
            objectiveProgress = CreateFill("Progress", background.transform, violet);
        }

        private void BuildVitals(Color panel, Color white, Color cyan)
        {
            GameObject root = CreatePanel("PLAYER_VITALS", transform, Vector2.zero, new Vector2(42f, 72f), new Vector2(400f, 138f), panel, Vector2.zero);
            Image emblem = CreateImage("KairiEmblem", root.transform, new Vector2(-150f, 10f), new Vector2(78f, 78f), new Color(0.10f, 0.22f, 0.40f, 1f));
            CreateText("K", emblem.transform, Vector2.zero, new Vector2(78f, 78f), 36, TextAnchor.MiddleCenter, cyan).text = "K";
            agentText = CreateText("Agent", root.transform, new Vector2(-25f, 44f), new Vector2(190f, 22f), 20, TextAnchor.MiddleLeft, white);
            healthText = CreateText("Health", root.transform, new Vector2(-62f, 4f), new Vector2(84f, 46f), 42, TextAnchor.MiddleCenter, white);
            armorText = CreateText("Armor", root.transform, new Vector2(39f, 2f), new Vector2(58f, 30f), 22, TextAnchor.MiddleCenter, cyan);
            CreateText("HealthLabel", root.transform, new Vector2(104f, 37f), new Vector2(140f, 16f), 12, TextAnchor.MiddleLeft, cyan).text = "VITAL LINK";
            GameObject healthBack = CreatePanel("HealthBack", root.transform, new Vector2(0.5f, 0.5f), new Vector2(76f, 0f), new Vector2(178f, 12f), new Color(0.07f, 0.12f, 0.19f, 1f));
            healthFill = CreateFill("HealthFill", healthBack.transform, cyan);
            GameObject armorBack = CreatePanel("ArmorBack", root.transform, new Vector2(0.5f, 0.5f), new Vector2(76f, -28f), new Vector2(178f, 7f), new Color(0.07f, 0.12f, 0.19f, 1f));
            armorFill = CreateFill("ArmorFill", armorBack.transform, new Color(0.42f, 0.82f, 1f, 1f));
        }

        private void BuildWeapon(Color panel, Color white, Color cyan)
        {
            GameObject root = CreatePanel("WEAPON_STATE", transform, Vector2.one, new Vector2(-42f, -74f), new Vector2(416f, 138f), panel, Vector2.one);
            weaponText = CreateText("Weapon", root.transform, new Vector2(-145f, 41f), new Vector2(260f, 22f), 18, TextAnchor.MiddleLeft, cyan);
            ammoText = CreateText("Ammo", root.transform, new Vector2(-102f, -4f), new Vector2(132f, 56f), 48, TextAnchor.MiddleCenter, white);
            reserveText = CreateText("Reserve", root.transform, new Vector2(35f, -5f), new Vector2(96f, 30f), 23, TextAnchor.MiddleCenter, new Color(0.72f, 0.80f, 0.91f, 1f));
            reloadText = CreateText("Reload", root.transform, new Vector2(91f, 39f), new Vector2(178f, 18f), 13, TextAnchor.MiddleLeft, cyan);
            GameObject reloadBack = CreatePanel("ReloadBack", root.transform, new Vector2(0.5f, 0.5f), new Vector2(68f, -41f), new Vector2(212f, 6f), new Color(0.07f, 0.12f, 0.19f, 1f));
            reloadFill = CreateFill("ReloadFill", reloadBack.transform, cyan);
            reloadText.gameObject.SetActive(false);
        }

        private void BuildAbilities(Color panel, Color white, Color cyan, Color violet)
        {
            GameObject root = CreatePanel("KAIRI_PROTOCOL", transform, new Vector2(0.5f, 0f), new Vector2(0f, 54f), new Vector2(620f, 88f), panel, new Vector2(0.5f, 0f));
            string[] keys = { "Q", "E", "C", "X" };
            string[] labels = { "RIFT", "DECOY", "LEAP", "ECLIPSE" };
            abilityFills = new Image[4];
            abilityLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                float x = -210f + i * 140f;
                Image plate = CreateImage("AbilityPlate", root.transform, new Vector2(x, 5f), new Vector2(108f, 68f), new Color(0.06f, 0.12f, 0.22f, 1f));
                abilityFills[i] = CreateFill("AbilityCharge", plate.transform, i == 3 ? violet : cyan);
                CreateImage("AbilityRail", plate.transform, new Vector2(0f, -30f), new Vector2(84f, 3f), i == 3 ? violet : cyan);
                abilityLabels[i] = CreateText("AbilityKey", plate.transform, new Vector2(0f, 11f), new Vector2(60f, 26f), 24, TextAnchor.MiddleCenter, white);
                abilityLabels[i].text = keys[i];
                CreateText("AbilityName", plate.transform, new Vector2(0f, -16f), new Vector2(96f, 16f), 11, TextAnchor.MiddleCenter, cyan).text = labels[i];
            }
        }

        private void BuildFeedback(Color white)
        {
            crosshairRoot = new GameObject("Crosshair", typeof(RectTransform)).GetComponent<RectTransform>();
            crosshairRoot.SetParent(transform, false);
            crosshairRoot.anchorMin = crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRoot.sizeDelta = Vector2.one * 12f;
            CreateImage("CrossN", crosshairRoot, new Vector2(0f, 9f), new Vector2(2f, 6f), white);
            CreateImage("CrossS", crosshairRoot, new Vector2(0f, -9f), new Vector2(2f, 6f), white);
            CreateImage("CrossE", crosshairRoot, new Vector2(9f, 0f), new Vector2(6f, 2f), white);
            CreateImage("CrossW", crosshairRoot, new Vector2(-9f, 0f), new Vector2(6f, 2f), white);

            GameObject feedback = new GameObject("CombatFeedback", typeof(RectTransform), typeof(CanvasGroup));
            feedback.transform.SetParent(transform, false);
            RectTransform rt = feedback.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(110f, 96f);
            hitGroup = feedback.GetComponent<CanvasGroup>();
            hitGroup.alpha = 0f;
            hitMarker = CreateImage("HitMarker", feedback.transform, Vector2.zero, new Vector2(30f, 3f), white);
            hitMarker.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            CreateImage("HitMarkerB", feedback.transform, Vector2.zero, new Vector2(30f, 3f), white).rectTransform.localRotation = Quaternion.Euler(0f, 0f, -45f);
            hitText = CreateText("HitText", feedback.transform, new Vector2(0f, -32f), new Vector2(170f, 20f), 13, TextAnchor.MiddleCenter, white);
        }

        private void BuildBanner(Color panel, Color white)
        {
            GameObject root = CreatePanel("ROUND_BANNER", transform, new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(560f, 66f), panel);
            bannerGroup = root.AddComponent<CanvasGroup>();
            bannerGroup.alpha = 0f;
            bannerText = CreateText("Text", root.transform, Vector2.zero, new Vector2(520f, 46f), 24, TextAnchor.MiddleCenter, white);
        }

        private void BuildKillFeed(Color panel, Color white)
        {
            GameObject root = CreatePanel("KILL_FEED", transform, new Vector2(1f, 1f), new Vector2(-32f, -130f), new Vector2(380f, 148f), panel, new Vector2(1f, 1f));
            killFeedText = CreateText("Entries", root.transform, Vector2.zero, new Vector2(350f, 130f), 14, TextAnchor.UpperRight, white);
        }

        private void BuildScoreboard(Color panel, Color white, Color cyan, Color violet)
        {
            scoreboardRoot = CreatePanel("TACTICAL_TELEMETRY", transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 620f), new Color(0.009f, 0.020f, 0.040f, 0.97f));
            CreateText("Title", scoreboardRoot.transform, new Vector2(-420f, 260f), new Vector2(760f, 40f), 25, TextAnchor.MiddleLeft, white).text = "RENKAI // KUROKAGE  TACTICAL TELEMETRY";
            CreateText("Hint", scoreboardRoot.transform, new Vector2(370f, 260f), new Vector2(160f, 30f), 12, TextAnchor.MiddleRight, cyan).text = "HOLD TAB";
            scoreboardText = CreateText("Rows", scoreboardRoot.transform, new Vector2(-410f, 190f), new Vector2(830f, 440f), 18, TextAnchor.UpperLeft, white);
            scoreboardRoot.SetActive(false);
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color, Vector2? pivot = null)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return image;
        }

        private static Image CreateFill(string name, Transform parent, Color color)
        {
            Image fill = CreateImage(name, parent, Vector2.zero, Vector2.one, color);
            RectTransform rt = fill.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            return fill;
        }

        private static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = KurokageUiFont.Default;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return text;
        }

        private static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int total = Mathf.CeilToInt(seconds);
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private static string AgentDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "KAIRI";
            int separator = value.IndexOf("//", StringComparison.Ordinal);
            return separator > 0 ? value.Substring(0, separator).Trim() : value;
        }
    }
}
