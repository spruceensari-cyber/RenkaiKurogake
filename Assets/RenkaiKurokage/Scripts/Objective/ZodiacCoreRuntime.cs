using System;
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
        public Transform Carrier { get; private set; }

        public event Action<Transform> CorePickedUp;
        public event Action<Vector3> CoreDropped;
        public event Action LinkStarted;
        public event Action LinkCompleted;
        public event Action SynchronizationCompleted;
        public event Action SeverStarted;
        public event Action SeverCompleted;
        public event Action ObjectiveReset;

        private float stateStartTime;
        private Vector3 resetPosition;
        private Quaternion resetRotation;
        private Transform resetParent;

        private void Awake()
        {
            resetPosition = transform.position;
            resetRotation = transform.rotation;
            resetParent = transform.parent;
        }

        private void Update()
        {
            float elapsed = Time.time - stateStartTime;

            if (State == ZodiacLinkState.Linking)
            {
                Progress01 = Mathf.Clamp01(elapsed / linkDuration);
                if (Progress01 >= 1f)
                {
                    SetState(ZodiacLinkState.Synchronized);
                    LinkCompleted?.Invoke();
                }
            }
            else if (State == ZodiacLinkState.Synchronized)
            {
                Progress01 = Mathf.Clamp01(elapsed / syncDuration);
                if (Progress01 >= 1f)
                {
                    SetState(ZodiacLinkState.Completed);
                    SynchronizationCompleted?.Invoke();
                }
            }
            else if (State == ZodiacLinkState.Severing)
            {
                Progress01 = Mathf.Clamp01(elapsed / severDuration);
                if (Progress01 >= 1f)
                {
                    SetState(ZodiacLinkState.Severed);
                    SeverCompleted?.Invoke();
                }
            }
        }

        public void SetCarried(Transform carrier)
        {
            if (carrier == null) return;

            Carrier = carrier;
            transform.SetParent(carrier, false);
            transform.localPosition = new Vector3(0f, 1.15f, -0.38f);
            transform.localRotation = Quaternion.identity;
            SetState(ZodiacLinkState.Carried);
            CorePickedUp?.Invoke(carrier);
        }

        public void Drop(Vector3 worldPosition)
        {
            if (State != ZodiacLinkState.Carried || Carrier == null) return;

            Carrier = null;
            transform.SetParent(resetParent, true);
            transform.position = worldPosition;
            transform.rotation = resetRotation;
            SetState(ZodiacLinkState.Idle);
            CoreDropped?.Invoke(worldPosition);
        }

        public bool BeginLink()
        {
            if (State != ZodiacLinkState.Carried && State != ZodiacLinkState.Idle) return false;

            Carrier = null;
            SetState(ZodiacLinkState.Linking);
            LinkStarted?.Invoke();
            return true;
        }

        public bool BeginSever()
        {
            if (State != ZodiacLinkState.Synchronized) return false;
            SetState(ZodiacLinkState.Severing);
            SeverStarted?.Invoke();
            return true;
        }

        public void CancelCurrentAction()
        {
            if (State == ZodiacLinkState.Linking) SetState(ZodiacLinkState.Carried);
            else if (State == ZodiacLinkState.Severing) SetState(ZodiacLinkState.Synchronized);
        }

        public void ResetObjective()
        {
            Carrier = null;
            transform.SetParent(resetParent, true);
            transform.position = resetPosition;
            transform.rotation = resetRotation;
            State = ZodiacLinkState.Idle;
            Progress01 = 0f;
            stateStartTime = Time.time;
            ObjectiveReset?.Invoke();
        }

        private void SetState(ZodiacLinkState next)
        {
            State = next;
            Progress01 = 0f;
            stateStartTime = Time.time;
        }
    }
}
