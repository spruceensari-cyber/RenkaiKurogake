using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Renkai.Kurogake;
using Renkai.Kurokage;

public static class KurokageCollisionIntegrityPass
{
    private const string RootName = "KUROKAGE_COLLISION_INTEGRITY";

    private static readonly string[] SolidTokens =
    {
        "wall", "cover", "box", "column", "pylon", "structure", "stabilizer",
        "backplate", "cladding", "foundation", "pillar", "housing", "platform",
        "gate_left", "gate_right", "transit_pylon", "reactor_conduit"
    };

    private static readonly string[] NeverSolidTokens =
    {
        "hologram", "energy", "signal", "light", "guide", "accent", "wayfinding",
        "ring", "skyline", "orbital", "comms_array", "petal", "data_band", "route_mark"
    };

    public static bool ApplySilent()
    {
        GameObject marker = GameObject.Find(RootName);
        if (marker == null) marker = new GameObject(RootName);

        int repairedStatics = RepairGameplayMapColliders();
        int repairedArt = RepairReachableArchitectureColliders();
        int movingSolids = RepairMovingSolids();
        int controllers = ConfigureCharacterControllers();

        KurokageCollisionIntegrityMarker report = marker.GetComponent<KurokageCollisionIntegrityMarker>();
        if (report == null) report = marker.AddComponent<KurokageCollisionIntegrityMarker>();
        report.Configure(repairedStatics, repairedArt, movingSolids, controllers);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(
            "Kurokage Collision Integrity: map=" + repairedStatics +
            ", architecture=" + repairedArt +
            ", moving=" + movingSolids +
            ", controllers=" + controllers
        );
        return controllers > 0 && repairedStatics > 0;
    }

    private static int RepairGameplayMapColliders()
    {
        GameObject map = GameObject.Find("MAP");
        if (map == null) return 0;

        int count = 0;
        foreach (Renderer renderer in map.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null) continue;
            string normalized = renderer.gameObject.name.ToLowerInvariant();

            if (normalized.Contains("accent") || normalized.Contains("route"))
            {
                Collider decorative = renderer.GetComponent<Collider>();
                if (decorative != null) Object.DestroyImmediate(decorative);
                continue;
            }

            if (EnsureCollider(renderer.gameObject, false)) count++;
        }
        return count;
    }

    private static int RepairReachableArchitectureColliders()
    {
        int count = 0;
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            if (!IsInPlayableBounds(renderer.bounds)) continue;
            if (!IntersectsPlayerHeight(renderer.bounds)) continue;
            if (!IsSolidArchitectureName(renderer.gameObject.name)) continue;

            if (EnsureCollider(renderer.gameObject, false)) count++;
        }
        return count;
    }

    private static int RepairMovingSolids()
    {
        int count = 0;
        foreach (KurokageEnvironmentPulse pulse in Object.FindObjectsOfType<KurokageEnvironmentPulse>(true))
        {
            Renderer renderer = pulse.GetComponent<Renderer>();
            if (renderer == null) continue;
            if (!IsInPlayableBounds(renderer.bounds)) continue;
            if (!IntersectsMovementEnvelope(renderer.bounds)) continue;
            if (ContainsNeverSolidToken(pulse.gameObject.name)) continue;

            EnsureCollider(pulse.gameObject, true);

            Rigidbody body = pulse.GetComponent<Rigidbody>();
            if (body == null) body = pulse.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            count++;
        }

        foreach (Renderer renderer in Object.FindObjectsOfType<Renderer>(true))
        {
            if (renderer == null) continue;
            string n = renderer.gameObject.name.ToLowerInvariant();
            bool namedMovingSolid = n.Contains("marble") || n.Contains("moving_orb") || n.Contains("kinetic_orb") || n.Contains("moving_sphere");
            if (!namedMovingSolid) continue;
            if (!IsInPlayableBounds(renderer.bounds) || !IntersectsMovementEnvelope(renderer.bounds)) continue;

            EnsureCollider(renderer.gameObject, true);
            Rigidbody body = renderer.GetComponent<Rigidbody>();
            if (body == null) body = renderer.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            count++;
        }

        return count;
    }

    private static int ConfigureCharacterControllers()
    {
        int count = 0;
        foreach (RenkaiRoundPlayer player in Object.FindObjectsOfType<RenkaiRoundPlayer>(true))
        {
            if (player == null) continue;
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null) continue;

            controller.skinWidth = 0.045f;
            controller.stepOffset = Mathf.Min(0.34f, controller.height * 0.22f);
            controller.slopeLimit = 52f;
            controller.minMoveDistance = 0f;

            if (player.GetComponent<KurokageCharacterCollisionGuard>() == null)
                player.gameObject.AddComponent<KurokageCharacterCollisionGuard>();
            count++;
        }
        return count;
    }

    private static bool EnsureCollider(GameObject go, bool moving)
    {
        Collider existing = go.GetComponent<Collider>();
        if (existing != null)
        {
            existing.enabled = true;
            existing.isTrigger = false;
            return true;
        }

        MeshFilter filter = go.GetComponent<MeshFilter>();
        string meshName = filter != null && filter.sharedMesh != null
            ? filter.sharedMesh.name.ToLowerInvariant()
            : string.Empty;

        if (meshName.Contains("sphere"))
        {
            SphereCollider sphere = go.AddComponent<SphereCollider>();
            sphere.radius = 0.5f;
            return true;
        }

        if (meshName.Contains("cube"))
        {
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = Vector3.one;
            return true;
        }

        if (filter != null && filter.sharedMesh != null)
        {
            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
            meshCollider.convex = moving;
            return true;
        }

        return false;
    }

    private static bool IsSolidArchitectureName(string objectName)
    {
        string n = objectName.ToLowerInvariant();
        if (ContainsNeverSolidToken(n)) return false;
        foreach (string token in SolidTokens)
            if (n.Contains(token)) return true;
        return false;
    }

    private static bool ContainsNeverSolidToken(string objectName)
    {
        string n = objectName.ToLowerInvariant();
        foreach (string token in NeverSolidTokens)
            if (n.Contains(token)) return true;
        return false;
    }

    private static bool IsInPlayableBounds(Bounds bounds)
    {
        Vector3 c = bounds.center;
        return Mathf.Abs(c.x) <= 58f && c.z >= -78f && c.z <= 78f;
    }

    private static bool IntersectsPlayerHeight(Bounds bounds)
    {
        return bounds.max.y >= -0.15f && bounds.min.y <= 2.15f;
    }

    private static bool IntersectsMovementEnvelope(Bounds bounds)
    {
        return bounds.max.y >= -0.15f && bounds.min.y <= 3.35f;
    }
}
