using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageCollisionIntegrityMarker : MonoBehaviour
    {
        [SerializeField] private int gameplayColliders;
        [SerializeField] private int architectureColliders;
        [SerializeField] private int movingSolids;
        [SerializeField] private int protectedControllers;

        public int GameplayColliders => gameplayColliders;
        public int ArchitectureColliders => architectureColliders;
        public int MovingSolids => movingSolids;
        public int ProtectedControllers => protectedControllers;

        public void Configure(int gameplay, int architecture, int moving, int controllers)
        {
            gameplayColliders = Mathf.Max(0, gameplay);
            architectureColliders = Mathf.Max(0, architecture);
            movingSolids = Mathf.Max(0, moving);
            protectedControllers = Mathf.Max(0, controllers);
        }
    }
}
