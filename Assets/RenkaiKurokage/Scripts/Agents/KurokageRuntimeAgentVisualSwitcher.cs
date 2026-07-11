using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renkai.Kurokage
{
    [RequireComponent(typeof(KurokageAgentIdentity))]
    public sealed class KurokageRuntimeAgentVisualSwitcher : MonoBehaviour
    {
        private static readonly string[] StaticSignatureNames =
        {
            "PONYTAIL_ROOT", "KAIRI_BLADE_SHEATH", "NOA_SENSOR", "NOA_BACK_RIFLE",
            "REIHA_CHEST_NODE", "REIHA_HIP_BLADE", "DRONE_PIVOT", "SORA_BACK_SHIELD",
            "AIKO_SCARF", "REN_BREAKER_GAUNTLET", "HANA_HALO", "TOMA_EXO_PACK",
            "YORI_MASK", "COAT_PANEL_L", "COAT_PANEL_R"
        };

        private readonly List<GameObject> runtimeObjects = new List<GameObject>();
        private KurokageAgentIdentity identity;
        private Transform visualRoot;
        private Transform modelRoot;
        private Transform runtimeSignature;
        private Material darkMaterial;
        private Material fabricMaterial;
        private Material accentMaterial;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            identity = GetComponent<KurokageAgentIdentity>();
            propertyBlock = new MaterialPropertyBlock();
            ResolveVisual();
        }

        private void OnEnable()
        {
            if (identity == null) identity = GetComponent<KurokageAgentIdentity>();
            identity.AgentChanged += ApplyDefinition;
            ResolveVisual();
            ApplyDefinition(identity.Definition);
        }

        private void OnDisable()
        {
            if (identity != null) identity.AgentChanged -= ApplyDefinition;
        }

        private void ResolveVisual()
        {
            visualRoot = transform.Find("AGENT_VISUAL");
            if (visualRoot == null) return;
            modelRoot = FindDeep(visualRoot, "PROCEDURAL_AGENT_ROOT");
            darkMaterial = FindMaterial(visualRoot, "PELVIS_SHELL") ?? FindAnyMaterial(visualRoot);
            fabricMaterial = FindMaterial(visualRoot, "ABDOMEN") ?? darkMaterial;
            accentMaterial = FindMaterial(visualRoot, "CHEST_CORE") ?? darkMaterial;
        }

        private void ApplyDefinition(KurokageAgentDefinition definition)
        {
            if (definition == null) return;
            if (visualRoot == null || modelRoot == null) ResolveVisual();
            if (visualRoot == null || modelRoot == null) return;

            HideStaticSignatures();
            ClearRuntimeSignature();

            if (definition.Archetype == KurokageAgentArchetype.Kairi)
            {
                SetStaticSignatureActive("PONYTAIL_ROOT", true);
                SetStaticSignatureActive("KAIRI_BLADE_SHEATH", true);
                SetStaticSignatureActive("COAT_PANEL_L", true);
                SetStaticSignatureActive("COAT_PANEL_R", true);
            }
            else
            {
                BuildRuntimeSignature(definition.Archetype);
            }

            ApplyAccent(definition.Accent);
            ApplyProportions(definition.Archetype);

            KurokageProceduralAgentRig rig = visualRoot.GetComponent<KurokageProceduralAgentRig>();
            if (rig != null) rig.Configure(definition.Archetype);
        }

        private void ClearRuntimeSignature()
        {
            for (int i = runtimeObjects.Count - 1; i >= 0; i--)
            {
                GameObject runtimeObject = runtimeObjects[i];
                if (runtimeObject != null) UnityEngine.Object.Destroy(runtimeObject);
            }
            runtimeObjects.Clear();
            if (runtimeSignature != null) UnityEngine.Object.Destroy(runtimeSignature.gameObject);
            runtimeSignature = null;
        }

        private void HideStaticSignatures()
        {
            foreach (string name in StaticSignatureNames)
                SetStaticSignatureActive(name, false);
        }

        private void SetStaticSignatureActive(string name, bool active)
        {
            Transform target = FindDeep(visualRoot, name);
            if (target != null && (runtimeSignature == null || !target.IsChildOf(runtimeSignature)))
                target.gameObject.SetActive(active);
        }

        private void BuildRuntimeSignature(KurokageAgentArchetype archetype)
        {
            GameObject root = new GameObject("RUNTIME_AGENT_SIGNATURE");
            runtimeObjects.Add(root);
            runtimeSignature = root.transform;
            runtimeSignature.SetParent(modelRoot, false);

            Transform head = FindDeep(modelRoot, "head") ?? modelRoot;
            Transform chest = FindDeep(modelRoot, "chest") ?? modelRoot;
            Transform pelvis = FindDeep(modelRoot, "pelvis") ?? modelRoot;
            Transform rightLowerArm = FindDeep(modelRoot, "rightlowerarm") ?? modelRoot;

            switch (archetype)
            {
                case KurokageAgentArchetype.Noa:
                    Part("NOA_SENSOR", PrimitiveType.Cylinder, head, new Vector3(0.20f, 0.05f, 0f), new Vector3(0.055f, 0.018f, 0.055f), accentMaterial, new Vector3(0f, 0f, 90f));
                    Part("NOA_BACK_RIFLE", PrimitiveType.Cube, runtimeSignature, new Vector3(0f, 1.18f, -0.25f), new Vector3(0.10f, 0.47f, 0.08f), darkMaterial, new Vector3(0f, 0f, 18f));
                    CoatPanels();
                    break;
                case KurokageAgentArchetype.Reiha:
                    Part("REIHA_CHEST_NODE", PrimitiveType.Sphere, chest, new Vector3(-0.20f, 0.12f, 0.23f), Vector3.one * 0.065f, accentMaterial);
                    Part("REIHA_HIP_BLADE", PrimitiveType.Cube, pelvis, new Vector3(0.34f, -0.03f, -0.05f), new Vector3(0.045f, 0.43f, 0.06f), darkMaterial, new Vector3(0f, 0f, 22f));
                    break;
                case KurokageAgentArchetype.Mio:
                    Transform dronePivot = Pivot("DRONE_PIVOT", runtimeSignature, new Vector3(0f, 1.34f, 0f));
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * Mathf.PI * 0.5f;
                        Part("MIO_DRONE_" + i, PrimitiveType.Sphere, dronePivot, new Vector3(Mathf.Cos(angle) * 0.48f, 0f, Mathf.Sin(angle) * 0.48f), Vector3.one * 0.075f, accentMaterial);
                    }
                    CoatPanels();
                    break;
                case KurokageAgentArchetype.Sora:
                    Part("SORA_BACK_SHIELD", PrimitiveType.Cube, runtimeSignature, new Vector3(0f, 1.16f, -0.28f), new Vector3(0.38f, 0.36f, 0.10f), darkMaterial);
                    break;
                case KurokageAgentArchetype.Aiko:
                    Part("AIKO_SCARF", PrimitiveType.Cylinder, chest, new Vector3(0f, 0.29f, 0f), new Vector3(0.24f, 0.055f, 0.24f), accentMaterial);
                    CoatPanels();
                    break;
                case KurokageAgentArchetype.Ren:
                    Part("REN_BREAKER_GAUNTLET", PrimitiveType.Cube, rightLowerArm, new Vector3(0f, -0.17f, 0.09f), new Vector3(0.17f, 0.20f, 0.17f), darkMaterial);
                    break;
                case KurokageAgentArchetype.Hana:
                    Part("HANA_HALO", PrimitiveType.Cylinder, head, new Vector3(0f, 0.36f, 0f), new Vector3(0.30f, 0.018f, 0.30f), accentMaterial);
                    CoatPanels();
                    break;
                case KurokageAgentArchetype.Toma:
                    Part("TOMA_EXO_PACK", PrimitiveType.Cube, runtimeSignature, new Vector3(0f, 1.17f, -0.30f), new Vector3(0.42f, 0.34f, 0.15f), darkMaterial);
                    break;
                case KurokageAgentArchetype.Yori:
                    Part("YORI_MASK", PrimitiveType.Cube, head, new Vector3(0f, -0.01f, 0.205f), new Vector3(0.16f, 0.09f, 0.018f), darkMaterial);
                    CoatPanels();
                    break;
            }
        }

        private void ApplyAccent(Color accent)
        {
            foreach (Renderer renderer in visualRoot.GetComponentsInChildren<Renderer>(true))
            {
                string objectName = renderer.gameObject.name;
                if (objectName.IndexOf("CORE", StringComparison.OrdinalIgnoreCase) < 0 &&
                    objectName.IndexOf("EYE", StringComparison.OrdinalIgnoreCase) < 0 &&
                    objectName.IndexOf("NODE", StringComparison.OrdinalIgnoreCase) < 0 &&
                    objectName.IndexOf("ACCENT", StringComparison.OrdinalIgnoreCase) < 0 &&
                    objectName.IndexOf("DRONE", StringComparison.OrdinalIgnoreCase) < 0 &&
                    objectName.IndexOf("HALO", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", accent);
                propertyBlock.SetColor("_BaseColor", accent);
                propertyBlock.SetColor("_EmissionColor", accent * 1.7f);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ApplyProportions(KurokageAgentArchetype archetype)
        {
            Vector3 scale = Vector3.one;
            switch (archetype)
            {
                case KurokageAgentArchetype.Noa: scale = new Vector3(0.96f, 1.02f, 0.96f); break;
                case KurokageAgentArchetype.Reiha: scale = new Vector3(1.04f, 1.01f, 1.04f); break;
                case KurokageAgentArchetype.Mio: scale = new Vector3(0.95f, 0.99f, 0.95f); break;
                case KurokageAgentArchetype.Sora: scale = new Vector3(1.10f, 1.04f, 1.10f); break;
                case KurokageAgentArchetype.Aiko: scale = new Vector3(0.98f, 1f, 0.98f); break;
                case KurokageAgentArchetype.Ren: scale = new Vector3(1.08f, 1.03f, 1.08f); break;
                case KurokageAgentArchetype.Hana: scale = new Vector3(0.97f, 1f, 0.97f); break;
                case KurokageAgentArchetype.Toma: scale = new Vector3(1.12f, 1.05f, 1.12f); break;
                case KurokageAgentArchetype.Yori: scale = new Vector3(1f, 1.01f, 1f); break;
            }
            modelRoot.localScale = scale;
        }

        private void CoatPanels()
        {
            Transform left = Pivot("COAT_PANEL_L", runtimeSignature, new Vector3(-0.14f, 0.82f, -0.16f));
            Part("COAT_MESH_L", PrimitiveType.Cube, left, new Vector3(0f, -0.20f, 0f), new Vector3(0.12f, 0.30f, 0.035f), fabricMaterial, new Vector3(0f, 0f, 8f));
            Transform right = Pivot("COAT_PANEL_R", runtimeSignature, new Vector3(0.14f, 0.82f, -0.16f));
            Part("COAT_MESH_R", PrimitiveType.Cube, right, new Vector3(0f, -0.20f, 0f), new Vector3(0.12f, 0.30f, 0.035f), fabricMaterial, new Vector3(0f, 0f, -8f));
        }

        private Transform Pivot(string name, Transform parent, Vector3 localPosition)
        {
            GameObject go = new GameObject(name);
            runtimeObjects.Add(go);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private GameObject Part(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? rotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            runtimeObjects.Add(go);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(rotation ?? Vector3.zero);
            go.transform.localScale = scale;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.Destroy(collider);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return go;
        }

        private static Material FindMaterial(Transform root, string objectName)
        {
            Transform target = FindDeep(root, objectName);
            Renderer renderer = target != null ? target.GetComponent<Renderer>() : null;
            return renderer != null ? renderer.sharedMaterial : null;
        }

        private static Material FindAnyMaterial(Transform root)
        {
            Renderer renderer = root.GetComponentInChildren<Renderer>(true);
            return renderer != null ? renderer.sharedMaterial : null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }
    }
}
