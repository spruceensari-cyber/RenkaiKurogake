using System;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public static class KurokageCombatRayResolver
    {
        private const float SameTargetZonePreferenceDistance = 0.55f;

        public static bool TryResolve(Ray ray, float range, Transform shooterRoot, out RaycastHit resolvedHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, range, ~0, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                resolvedHit = default;
                return false;
            }

            Array.Sort(hits, CompareDistance);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                Collider collider = hit.collider;
                if (collider == null) continue;
                if (shooterRoot != null && collider.transform.IsChildOf(shooterRoot)) continue;

                KurokageHitZone zone = collider.GetComponent<KurokageHitZone>();
                if (zone != null)
                {
                    resolvedHit = hit;
                    return true;
                }

                KurokageDecoyHitReceiver decoy = collider.GetComponentInParent<KurokageDecoyHitReceiver>();
                if (decoy != null)
                {
                    resolvedHit = hit;
                    return true;
                }

                RenkaiRoundPlayer player = collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (player != null)
                {
                    if (TryPreferZoneForPlayer(hits, i + 1, player, hit.distance, shooterRoot, out RaycastHit zoneHit))
                    {
                        resolvedHit = zoneHit;
                        return true;
                    }

                    if (!collider.isTrigger)
                    {
                        resolvedHit = hit;
                        return true;
                    }

                    continue;
                }

                // Gameplay/objective triggers must never eat bullets. Only combat triggers above are valid.
                if (collider.isTrigger) continue;

                resolvedHit = hit;
                return true;
            }

            resolvedHit = default;
            return false;
        }

        private static bool TryPreferZoneForPlayer(
            RaycastHit[] hits,
            int startIndex,
            RenkaiRoundPlayer player,
            float baseDistance,
            Transform shooterRoot,
            out RaycastHit resolvedHit)
        {
            for (int i = startIndex; i < hits.Length; i++)
            {
                RaycastHit candidate = hits[i];
                if (candidate.distance - baseDistance > SameTargetZonePreferenceDistance) break;
                if (candidate.collider == null) continue;
                if (shooterRoot != null && candidate.collider.transform.IsChildOf(shooterRoot)) continue;

                KurokageHitZone zone = candidate.collider.GetComponent<KurokageHitZone>();
                if (zone == null) continue;

                RenkaiRoundPlayer candidatePlayer = candidate.collider.GetComponentInParent<RenkaiRoundPlayer>();
                if (candidatePlayer != player) continue;

                resolvedHit = candidate;
                return true;
            }

            resolvedHit = default;
            return false;
        }

        private static int CompareDistance(RaycastHit a, RaycastHit b)
        {
            return a.distance.CompareTo(b.distance);
        }
    }
}
