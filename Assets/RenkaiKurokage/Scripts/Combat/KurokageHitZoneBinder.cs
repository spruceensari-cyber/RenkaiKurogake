using System;
using System.Collections.Generic;
using UnityEngine;
using Renkai.Kurogake;

namespace Renkai.Kurokage
{
    public sealed class KurokageHitZoneBinder : MonoBehaviour
    {
        [SerializeField] private bool bindOnAwake = true;
        [SerializeField] private float headMultiplier = 1f;
        [SerializeField] private float torsoMultiplier = 1f;
        [SerializeField] private float limbMultiplier = 0.8f;

        public int BoundZoneCount { get; private set; }

        private static readonly string[] HeadKeys = { "head", "neckhead", "headtop" };
        private static readonly string[] TorsoKeys = { "upperchest", "chest", "spine2", "spine1", "spine", "hips", "pelvis" };
        private static readonly string[] LimbKeys = { "upperarm", "lowerarm", "forearm", "hand", "thigh", "upleg", "leg", "calf", "foot", "shoulder" };

        private void Awake()
        {
            if (bindOnAwake) Bind();
        }

        public void Bind()
        {
            BoundZoneCount = 0;
            RenkaiRoundPlayer owner = GetComponentInParent<RenkaiRoundPlayer>();
            if (owner == null) return;

            Transform visualRoot = owner.transform.Find("AGENT_VISUAL");
            if (visualRoot == null) return;

            Transform head = FindFirst(visualRoot, HeadKeys);
            Transform chest = FindFirst(visualRoot, new[] { "upperchest", "chest", "spine2", "spine1" });
            Transform pelvis = FindFirst(visualRoot, new[] { "hips", "pelvis", "spine" });

            if (head != null) CreateZone(head, "HITBOX_HEAD", KurokageHitZoneType.Head, headMultiplier, new Vector3(0.22f, 0.22f, 0.22f), true);
            if (chest != null) CreateZone(chest, "HITBOX_CHEST", KurokageHitZoneType.Torso, torsoMultiplier, new Vector3(0.36f, 0.48f, 0.22f), false);
            if (pelvis != null) CreateZone(pelvis, "HITBOX_PELVIS", KurokageHitZoneType.Torso, torsoMultiplier, new Vector3(0.34f, 0.30f, 0.22f), false);

            CreateFirstLimbGroup(visualRoot, "LEFT_ARM", new[] { "leftupperarm", "l_upperarm", "upperarm_l", "leftarm" });
            CreateFirstLimbGroup(visualRoot, "RIGHT_ARM", new[] { "rightupperarm", "r_upperarm", "upperarm_r", "rightarm" });
            CreateFirstLimbGroup(visualRoot, "LEFT_LEG", new[] { "leftupleg", "leftthigh", "l_thigh", "upleg_l" });
            CreateFirstLimbGroup(visualRoot, "RIGHT_LEG", new[] { "rightupleg", "rightthigh", "r_thigh", "upleg_r" });

            if (BoundZoneCount < 3)
            {
                foreach (Transform t in visualRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (BoundZoneCount >= 7) break;
                    string normalized = Normalize(t.name);
                    if (Matches(normalized, HeadKeys) || Matches(normalized, TorsoKeys)) continue;
                    if (Matches(normalized, LimbKeys))
                        CreateZone(t, "HITBOX_LIMB_" + BoundZoneCount, KurokageHitZoneType.Limb, limbMultiplier, new Vector3(0.16f, 0.34f, 0.16f), false);
                }
            }
        }

        private void CreateFirstLimbGroup(Transform root, string label, string[] keys)
        {
            Transform limb = FindFirst(root, keys);
            if (limb != null)
                CreateZone(limb, "HITBOX_" + label, KurokageHitZoneType.Limb, limbMultiplier, new Vector3(0.18f, 0.42f, 0.18f), false);
        }

        private void CreateZone(Transform bone, string zoneName, KurokageHitZoneType type, float multiplier, Vector3 size, bool sphere)
        {
            Transform existing = bone.Find(zoneName);
            GameObject zoneGo = existing != null ? existing.gameObject : new GameObject(zoneName);
            zoneGo.transform.SetParent(bone, false);
            zoneGo.transform.localPosition = Vector3.zero;
            zoneGo.transform.localRotation = Quaternion.identity;

            foreach (Collider c in zoneGo.GetComponents<Collider>())
                Destroy(c);

            if (sphere)
            {
                SphereCollider collider = zoneGo.AddComponent<SphereCollider>();
                collider.radius = size.x;
                collider.isTrigger = true;
            }
            else
            {
                BoxCollider collider = zoneGo.AddComponent<BoxCollider>();
                collider.size = size;
                collider.isTrigger = true;
            }

            KurokageHitZone zone = zoneGo.GetComponent<KurokageHitZone>();
            if (zone == null) zone = zoneGo.AddComponent<KurokageHitZone>();
            zone.Configure(type, multiplier);
            BoundZoneCount++;
        }

        private static Transform FindFirst(Transform root, string[] keys)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                string normalized = Normalize(t.name);
                if (Matches(normalized, keys)) return t;
            }
            return null;
        }

        private static bool Matches(string normalized, string[] keys)
        {
            foreach (string key in keys)
                if (normalized.Contains(Normalize(key))) return true;
            return false;
        }

        private static string Normalize(string value)
        {
            return value.ToLowerInvariant().Replace("mixamorig:", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
        }
    }
}
