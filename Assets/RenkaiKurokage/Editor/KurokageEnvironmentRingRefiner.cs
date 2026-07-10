using UnityEngine;

public static class KurokageEnvironmentRingRefiner
{
    public static bool ApplySilent()
    {
        GameObject environmentRoot = GameObject.Find("KUROKAGE_ENVIRONMENT_ART");
        if (environmentRoot == null) return false;

        Transform[] transforms = environmentRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform target in transforms)
        {
            if (target == null || !target.name.Contains("Ring")) continue;
            Renderer sourceRenderer = target.GetComponent<Renderer>();
            if (sourceRenderer == null) continue;

            Material material = sourceRenderer.sharedMaterial;
            Vector3 worldScale = target.lossyScale;
            float radiusX = Mathf.Max(0.5f, worldScale.x);
            float radiusY = Mathf.Max(0.5f, worldScale.y);
            float thickness = Mathf.Max(0.06f, worldScale.z);

            sourceRenderer.enabled = false;
            Collider sourceCollider = target.GetComponent<Collider>();
            if (sourceCollider != null) Object.DestroyImmediate(sourceCollider);

            Transform oldSegments = target.Find("SEGMENTED_RING");
            if (oldSegments != null) Object.DestroyImmediate(oldSegments.gameObject);

            target.localScale = Vector3.one;
            GameObject segmentRoot = new GameObject("SEGMENTED_RING");
            segmentRoot.transform.SetParent(target, false);

            const int segmentCount = 20;
            float averageRadius = (radiusX + radiusY) * 0.5f;
            float segmentLength = Mathf.Max(0.18f, 2f * Mathf.PI * averageRadius / segmentCount * 0.82f);
            float radialThickness = Mathf.Clamp(averageRadius * 0.045f, 0.10f, 0.65f);

            for (int i = 0; i < segmentCount; i++)
            {
                float angle = i * 360f / segmentCount;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 localPosition = new Vector3(Mathf.Cos(rad) * radiusX, Mathf.Sin(rad) * radiusY, 0f);

                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = "RingSegment_" + i.ToString("00");
                segment.transform.SetParent(segmentRoot.transform, false);
                segment.transform.localPosition = localPosition;
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
                segment.transform.localScale = new Vector3(segmentLength, radialThickness, thickness);

                Renderer renderer = segment.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = material;
                Collider collider = segment.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }
        }

        return true;
    }
}
