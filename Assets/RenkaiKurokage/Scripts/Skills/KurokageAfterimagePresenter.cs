using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renkai.Kurokage
{
    public sealed class KurokageAfterimagePresenter : MonoBehaviour
    {
        [SerializeField] private int maxSnapshotsPerDash = 3;
        [SerializeField] private float snapshotLifetime = 0.28f;
        [SerializeField] private Color afterimageColor = new Color(0.16f, 0.54f, 1f, 0.72f);

        private Material cachedMaterial;
        private readonly List<GameObject> activeSnapshots = new List<GameObject>();

        public void SpawnDashSequence(float dashDuration)
        {
            StartCoroutine(DashSequence(dashDuration));
        }

        public void ClearAll()
        {
            StopAllCoroutines();
            for (int i = activeSnapshots.Count - 1; i >= 0; i--)
                if (activeSnapshots[i] != null) Destroy(activeSnapshots[i]);
            activeSnapshots.Clear();
        }

        private IEnumerator DashSequence(float dashDuration)
        {
            int count = Mathf.Clamp(maxSnapshotsPerDash, 1, 3);
            float interval = dashDuration / count;
            for (int i = 0; i < count; i++)
            {
                SpawnSnapshot();
                yield return new WaitForSeconds(interval);
            }
        }

        private void SpawnSnapshot()
        {
            Transform visualRoot = transform.Find("AGENT_VISUAL");
            if (visualRoot == null) return;

            GameObject snapshotRoot = new GameObject("KAIRI_DASH_AFTERIMAGE");
            snapshotRoot.transform.position = Vector3.zero;
            snapshotRoot.transform.rotation = Quaternion.identity;
            activeSnapshots.Add(snapshotRoot);

            foreach (SkinnedMeshRenderer source in visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Mesh baked = new Mesh();
                source.BakeMesh(baked);

                GameObject part = new GameObject("AfterimagePart");
                part.transform.SetParent(snapshotRoot.transform, false);
                part.transform.position = source.transform.position;
                part.transform.rotation = source.transform.rotation;
                part.transform.localScale = source.transform.lossyScale;

                MeshFilter filter = part.AddComponent<MeshFilter>();
                filter.sharedMesh = baked;
                MeshRenderer renderer = part.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = GetMaterial();
            }

            foreach (MeshRenderer source in visualRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null) continue;

                GameObject part = new GameObject("AfterimageStaticPart");
                part.transform.SetParent(snapshotRoot.transform, false);
                part.transform.position = source.transform.position;
                part.transform.rotation = source.transform.rotation;
                part.transform.localScale = source.transform.lossyScale;

                MeshFilter filter = part.AddComponent<MeshFilter>();
                filter.sharedMesh = sourceFilter.sharedMesh;
                MeshRenderer renderer = part.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = GetMaterial();
            }

            StartCoroutine(FadeAndDestroy(snapshotRoot));
        }

        private IEnumerator FadeAndDestroy(GameObject snapshot)
        {
            float elapsed = 0f;
            Vector3 baseScale = snapshot.transform.localScale;
            while (elapsed < snapshotLifetime && snapshot != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / snapshotLifetime);
                snapshot.transform.localScale = Vector3.Lerp(baseScale, baseScale * 0.92f, t);
                yield return null;
            }

            activeSnapshots.Remove(snapshot);
            if (snapshot != null) Destroy(snapshot);
        }

        private Material GetMaterial()
        {
            if (cachedMaterial != null) return cachedMaterial;

            bool srp = GraphicsSettings.currentRenderPipeline != null;
            Shader shader = srp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            cachedMaterial = new Material(shader);
            if (cachedMaterial.HasProperty("_BaseColor")) cachedMaterial.SetColor("_BaseColor", afterimageColor);
            if (cachedMaterial.HasProperty("_Color")) cachedMaterial.SetColor("_Color", afterimageColor);
            if (cachedMaterial.HasProperty("_EmissionColor"))
            {
                cachedMaterial.EnableKeyword("_EMISSION");
                cachedMaterial.SetColor("_EmissionColor", afterimageColor * 3.4f);
            }
            return cachedMaterial;
        }
    }
}
