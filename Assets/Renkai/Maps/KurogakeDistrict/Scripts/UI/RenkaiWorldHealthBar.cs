using UnityEngine;

namespace Renkai.Kurogake
{
    public class RenkaiWorldHealthBar : MonoBehaviour
    {
        public RenkaiRoundPlayer target;
        public Transform fill;
        public TextMesh nameText;
        [SerializeField] private bool visibleInWorld = false;

        public bool VisibleInWorld => visibleInWorld;

        private Camera cam;
        private Renderer[] worldRenderers;

        private void Awake()
        {
            worldRenderers = GetComponentsInChildren<Renderer>(true);
            ApplyVisibility();
        }

        private void Start()
        {
            cam = Camera.main;
            RefreshNow();
        }

        private void LateUpdate()
        {
            if (!visibleInWorld || target == null) return;

            if (cam == null) cam = Camera.main;

            transform.position = target.transform.position + Vector3.up * 2.45f;

            if (cam != null)
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

            if (fill != null)
            {
                Vector3 s = fill.localScale;
                s.x = Mathf.Clamp01(target.Health01());
                fill.localScale = s;
            }

            if (nameText != null)
                nameText.text = target.agentName;
        }

        public void SetWorldVisible(bool visible)
        {
            visibleInWorld = visible;
            ApplyVisibility();
        }

        public void RefreshNow()
        {
            if (target == null)
                target = GetComponentInParent<RenkaiRoundPlayer>();

            if (nameText != null && target != null)
                nameText.text = target.agentName;
        }

        private void ApplyVisibility()
        {
            if (worldRenderers == null || worldRenderers.Length == 0)
                worldRenderers = GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in worldRenderers)
            {
                if (renderer != null)
                    renderer.enabled = visibleInWorld;
            }
        }
    }
}
