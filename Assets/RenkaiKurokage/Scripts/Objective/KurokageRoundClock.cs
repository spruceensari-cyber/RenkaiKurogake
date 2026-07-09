using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageRoundClock : MonoBehaviour
    {
        [SerializeField] private float buyPhaseDuration = 20f;
        [SerializeField] private float roundDuration = 100f;

        public KurokageRoundState State { get; private set; } = KurokageRoundState.PreRound;
        public float TimeRemaining { get; private set; }

        private void Update()
        {
            if (State != KurokageRoundState.BuyPhase && State != KurokageRoundState.Active) return;

            TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);

            if (TimeRemaining <= 0f)
            {
                if (State == KurokageRoundState.BuyPhase) BeginActiveRound();
                else State = KurokageRoundState.PostRound;
            }
        }

        public void BeginBuyPhase()
        {
            State = KurokageRoundState.BuyPhase;
            TimeRemaining = buyPhaseDuration;
        }

        public void BeginActiveRound()
        {
            State = KurokageRoundState.Active;
            TimeRemaining = roundDuration;
        }

        public void EndRound()
        {
            State = KurokageRoundState.PostRound;
            TimeRemaining = 0f;
        }
    }
}
