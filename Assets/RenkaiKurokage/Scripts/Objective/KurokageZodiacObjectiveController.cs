using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageZodiacObjectiveController : MonoBehaviour
    {
        [SerializeField] private ZodiacCoreRuntime core;
        [SerializeField] private ZodiacNexusSite[] sites;
        [SerializeField] private RenkaiRoundPlayer humanPlayer;
        [SerializeField] private float pickupDistance = 2.6f;
        [SerializeField] private float nexusUseDistance = 4.5f;
        [SerializeField] private float defenderSeverDistance = 4.2f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        public ZodiacCoreRuntime Core => core;
        public string ActiveSiteId { get; private set; } = string.Empty;
        public string StatusText { get; private set; } = "CORE READY";

        private bool roundResultSent;
        private RenkaiRoundPlayer[] cachedPlayers;
        private float nextRosterRefresh;

        private void Awake()
        {
            ResolveReferences();
            RefreshRoster();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (core != null) core.ObjectiveReset += OnObjectiveReset;
        }

        private void OnDisable()
        {
            if (core != null) core.ObjectiveReset -= OnObjectiveReset;
        }

        private void ResolveReferences()
        {
            if (core == null) core = FindObjectOfType<ZodiacCoreRuntime>();
            if (sites == null || sites.Length == 0) sites = FindObjectsOfType<ZodiacNexusSite>(true);

            if (humanPlayer == null)
            {
                foreach (RenkaiRoundPlayer p in FindObjectsOfType<RenkaiRoundPlayer>(true))
                {
                    if (p.isHumanPlayer)
                    {
                        humanPlayer = p;
                        break;
                    }
                }
            }
        }

        private void RefreshRoster()
        {
            cachedPlayers = FindObjectsOfType<RenkaiRoundPlayer>(true);
            nextRosterRefresh = Time.time + 1f;
        }

        private void Update()
        {
            if (core == null || humanPlayer == null) return;
            if (Time.time >= nextRosterRefresh) RefreshRoster();

            if (core.State == ZodiacLinkState.Idle)
            {
                StatusText = "ZODIAC CORE AVAILABLE // F TO ACQUIRE";
                if (Vector3.Distance(humanPlayer.transform.position, core.transform.position) <= pickupDistance && Input.GetKeyDown(interactKey))
                {
                    core.SetCarried(humanPlayer.transform);
                    StatusText = "CORE ACQUIRED";
                }
            }
            else if (core.State == ZodiacLinkState.Carried)
            {
                StatusText = "CORE CARRIED - MOVE TO NEXUS";
                ZodiacNexusSite nearest = FindNearestSite(humanPlayer.transform.position, nexusUseDistance);
                if (nearest != null)
                {
                    StatusText = "HOLD POSITION - PRESS F TO LINK " + nearest.SiteId;
                    if (Input.GetKeyDown(interactKey))
                    {
                        ActiveSiteId = nearest.SiteId;
                        core.transform.SetParent(null, true);
                        core.transform.position = nearest.transform.position + Vector3.up * 1.1f;
                        if (core.BeginLink()) StatusText = "LINK INITIATED // " + ActiveSiteId;
                    }
                }
            }
            else if (core.State == ZodiacLinkState.Linking)
            {
                StatusText = "LINKING " + Mathf.RoundToInt(core.Progress01 * 100f) + "%";
            }
            else if (core.State == ZodiacLinkState.Synchronized)
            {
                StatusText = "SYNCHRONIZATION " + Mathf.RoundToInt(core.Progress01 * 100f) + "%";
                TryStartDefenderSever();
            }
            else if (core.State == ZodiacLinkState.Severing)
            {
                StatusText = "SEVER IN PROGRESS " + Mathf.RoundToInt(core.Progress01 * 100f) + "%";
            }
            else if (core.State == ZodiacLinkState.Completed)
            {
                StatusText = "SYNCHRONIZATION COMPLETE";
                SendRoundResult(RenkaiTeam.Attackers, "Zodiac synchronization complete");
            }
            else if (core.State == ZodiacLinkState.Severed)
            {
                StatusText = "LINK SEVERED";
                SendRoundResult(RenkaiTeam.Defenders, "Zodiac link severed");
            }
        }

        private ZodiacNexusSite FindNearestSite(Vector3 position, float maxDistance)
        {
            ZodiacNexusSite best = null;
            float bestDistance = maxDistance;
            foreach (ZodiacNexusSite site in sites)
            {
                if (site == null) continue;
                float d = Vector3.Distance(position, site.transform.position);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = site;
                }
            }
            return best;
        }

        private void TryStartDefenderSever()
        {
            if (core.State != ZodiacLinkState.Synchronized) return;
            if (cachedPlayers == null || cachedPlayers.Length == 0) RefreshRoster();

            foreach (RenkaiRoundPlayer p in cachedPlayers)
            {
                if (p == null || !p.isAlive || p.team != RenkaiTeam.Defenders) continue;
                if (Vector3.Distance(p.transform.position, core.transform.position) <= defenderSeverDistance)
                {
                    core.BeginSever();
                    return;
                }
            }
        }

        private void OnObjectiveReset()
        {
            roundResultSent = false;
            ActiveSiteId = string.Empty;
            StatusText = "CORE READY";
            RefreshRoster();
        }

        private void SendRoundResult(RenkaiTeam winner, string reason)
        {
            if (roundResultSent) return;
            RenkaiRoundManager manager = FindObjectOfType<RenkaiRoundManager>();
            if (manager != null)
            {
                roundResultSent = true;
                manager.EndRound(winner, reason);
            }
        }
    }
}
