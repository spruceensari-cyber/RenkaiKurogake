using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class ZodiacCoreRuntime : MonoBehaviour
    {
        [SerializeField] private float linkDuration = 4f;
        [SerializeField] private float syncDuration = 38f;
        [SerializeField] private float severDuration = 7f;

        public ZodiacLinkState State { get; private set; } = ZodiacLinkState.Idle;
        public float Progress01 { get; private set; }

        private float stateStartTime;

        private void Update()
        {
            float elapsed = Time.time - stateStartTime;

            if (State == ZodiacLinkState.Linking)
            {
                Progress01 = Mathf.Clamp01(elapsed / linkDuration);
                if (Progress01 >= 1f) SetState(ZodiacLinkState.Synchronized);
            }
            else if (State == ZodiacLinkState.Synchronized)
            {
                Progress01 = Mathf.Clamp01(elapsed / syncDuration);
                if (Progress01 >= 1f) SetState(ZodiacLinkState.Completed);
            }
            else if (State == ZodiacLinkState.Severing)
            {
                Progress01 = Mathf.Clamp01(elapsed / severDuration);
                if (Progress01 >= 1f) SetState(ZodiacLinkState.Severed);
            }
        }

        public void SetCarried(Transform carrier)
        {
            if (carrier == null) return;
            transform.SetParent(carrier, false);
            transform.localPosition = Vector3.zero;
            SetState(ZodiacLinkState.Carried);
        }

        public bool BeginLink()
        {
            if (State != ZodiacLinkState.Carried && State != ZodiacLinkState.Idle) return false;
            SetState(ZodiacLinkState.Linking);
            return true;
        }

        public bool BeginSever()
        {
            if (State != ZodiacLinkState.Synchronized) return false;
            SetState(ZodiacLinkState.Severing);
            return true;
        }

        public void CancelCurrentAction()
        {
            if (State == ZodiacLinkState.Linking) SetState(ZodiacLinkState.Carried);
            else if (State == ZodiacLinkState.Severing) SetState(ZodiacLinkState.Synchronized);
        }

        private void SetState(ZodiacLinkState next)
        {
            State = next;
            Progress01 = 0f;
            stateStartTime = Time.time;
        }
    }
}
