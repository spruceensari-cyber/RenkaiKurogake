using UnityEngine;

namespace Renkai.Kurokage
{
    public sealed class KurokageZodiacVfxPresenter : MonoBehaviour
    {
        [SerializeField] private ZodiacCoreRuntime core;
        [SerializeField] private float idlePulseSpeed = 2.2f;
        [SerializeField] private float activePulseSpeed = 7.5f;
        [SerializeField] private float rotationSpeed = 42f;

        private Transform ringA;
        private Transform ringB;
        private Transform haloOuter;
        private Transform shellStructure;
        private Renderer innerEnergy;
        private Light coreLight;
        private ZodiacLinkState lastState;
        private MaterialPropertyBlock propertyBlock;
        private bool threshold30;
        private bool threshold70;
        private bool threshold90;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            if (core == null) core = GetComponent<ZodiacCoreRuntime>();
            BindVisuals();
            propertyBlock = new MaterialPropertyBlock();
            lastState = core != null ? core.State : ZodiacLinkState.Idle;
        }

        private void Update()
        {
            if (core == null) return;
            if (ringA == null || ringB == null || haloOuter == null) BindVisuals();

            float progress = Mathf.Clamp01(core.Progress01);
            float escalation = Escalation(progress, core.State);
            float pulseSpeed = Mathf.Lerp(idlePulseSpeed, activePulseSpeed * 1.55f, escalation);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * Mathf.Lerp(0.025f, 0.11f, escalation);
            float spin = rotationSpeed * Mathf.Lerp(0.7f, 2.35f, escalation);

            if (ringA != null)
            {
                ringA.localScale = Vector3.one * pulse;
                ringA.Rotate(Vector3.up, spin * Time.deltaTime, Space.Self);
            }

            if (ringB != null)
            {
                ringB.localScale = Vector3.one * Mathf.Lerp(1.02f, 1.12f, escalation) / Mathf.Max(0.01f, pulse);
                ringB.Rotate(Vector3.forward, -spin * 0.72f * Time.deltaTime, Space.Self);
            }

            if (haloOuter != null)
            {
                float haloPulse = 1f + Mathf.Sin(Time.time * (pulseSpeed * 0.62f) + 1.7f) * Mathf.Lerp(0.015f, 0.08f, escalation);
                haloOuter.localScale = Vector3.one * haloPulse;
                haloOuter.Rotate(Vector3.up, spin * 0.38f * Time.deltaTime, Space.Self);
            }

            if (shellStructure != null)
                shellStructure.Rotate(Vector3.up, spin * 0.16f * Time.deltaTime, Space.Self);

            Color stateColor = StateColor(core.State, progress);
            UpdateEnergyMaterial(stateColor, escalation);

            if (coreLight != null)
            {
                coreLight.intensity = Mathf.Lerp(1.6f, 4.8f, escalation) + Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * Mathf.Lerp(0.2f, 1.2f, escalation);
                coreLight.range = Mathf.Lerp(6f, 10f, escalation);
                coreLight.color = stateColor;
            }

            HandleThresholdEscalation(progress);

            if (core.State != lastState)
            {
                SpawnStateBurst(stateColor, core.State == ZodiacLinkState.Completed ? 0.85f : 0.48f);
                ResetThresholdsIfNeeded();
                lastState = core.State;
            }
        }

        private void BindVisuals()
        {
            Transform art = transform.Find("ZODIAC_CORE_ART");
            if (art == null) return;

            ringA = art.Find("CORE_RING_A");
            ringB = art.Find("CORE_RING_B");
            haloOuter = art.Find("CORE_HALO_OUTER");
            shellStructure = art.Find("CORE_SHELL_STRUCTURE");

            Transform energy = art.Find("CORE_INNER_ENERGY");
            innerEnergy = energy != null ? energy.GetComponent<Renderer>() : null;

            Transform lightTransform = art.Find("ZODIAC_CORE_LIGHT");
            coreLight = lightTransform != null ? lightTransform.GetComponent<Light>() : GetComponentInChildren<Light>(true);
        }

        private void HandleThresholdEscalation(float progress)
        {
            if (core.State != ZodiacLinkState.Synchronized)
            {
                threshold30 = false;
                threshold70 = false;
                threshold90 = false;
                return;
            }

            if (!threshold30 && progress >= 0.30f)
            {
                threshold30 = true;
                SpawnResonanceWave(1.1f, 0.28f, new Color(0.16f, 0.52f, 1f, 1f));
            }

            if (!threshold70 && progress >= 0.70f)
            {
                threshold70 = true;
                SpawnResonanceWave(1.7f, 0.34f, new Color(0.26f, 0.42f, 1f, 1f));
                SpawnResonanceWave(2.2f, 0.42f, new Color(0.42f, 0.22f, 1f, 1f));
            }

            if (!threshold90 && progress >= 0.90f)
            {
                threshold90 = true;
                SpawnResonanceWave(2.8f, 0.55f, new Color(0.86f, 0.94f, 1f, 1f));
                SpawnStateBurst(new Color(0.42f, 0.26f, 1f, 1f), 0.72f);
            }
        }

        private void ResetThresholdsIfNeeded()
        {
            if (core.State != ZodiacLinkState.Synchronized)
            {
                threshold30 = false;
                threshold70 = false;
                threshold90 = false;
            }
        }

        private void SpawnStateBurst(Color color, float scale)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Sphere,
                "ZODIAC_STATE_BURST",
                transform.position,
                Quaternion.identity,
                Vector3.one * scale,
                color,
                4.2f,
                0.24f
            );
        }

        private void SpawnResonanceWave(float radius, float life, Color color)
        {
            KurokageVfxPool.Instance.Spawn(
                KurokageVfxShape.Cylinder,
                "ZODIAC_RESONANCE_WAVE",
                transform.position + Vector3.up * 0.04f,
                Quaternion.identity,
                new Vector3(radius, 0.025f, radius),
                color,
                3.8f,
                life
            );
        }

        private void UpdateEnergyMaterial(Color color, float escalation)
        {
            if (innerEnergy == null) return;
            innerEnergy.GetPropertyBlock(propertyBlock);
            Color emission = color * Mathf.Lerp(2.0f, 5.2f, escalation);
            propertyBlock.SetColor(EmissionColorId, emission);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            innerEnergy.SetPropertyBlock(propertyBlock);
        }

        private static float Escalation(float progress, ZodiacLinkState state)
        {
            if (state == ZodiacLinkState.Linking) return Mathf.Lerp(0.28f, 0.52f, progress);
            if (state == ZodiacLinkState.Synchronized) return Mathf.Lerp(0.45f, 1f, progress);
            if (state == ZodiacLinkState.Severing) return Mathf.Lerp(0.8f, 0.35f, progress);
            if (state == ZodiacLinkState.Completed) return 1f;
            if (state == ZodiacLinkState.Severed) return 0.18f;
            if (state == ZodiacLinkState.Carried) return 0.22f;
            return 0.12f;
        }

        private static Color StateColor(ZodiacLinkState state, float progress)
        {
            if (state == ZodiacLinkState.Severing || state == ZodiacLinkState.Severed)
                return Color.Lerp(new Color(0.55f, 0.20f, 1f, 1f), new Color(0.96f, 0.90f, 1f, 1f), progress);
            if (state == ZodiacLinkState.Completed)
                return new Color(0.84f, 0.98f, 1f, 1f);
            if (state == ZodiacLinkState.Synchronized)
                return Color.Lerp(new Color(0.16f, 0.52f, 1f, 1f), new Color(0.46f, 0.24f, 1f, 1f), Mathf.SmoothStep(0f, 1f, progress));
            if (state == ZodiacLinkState.Linking)
                return new Color(0.18f, 0.58f, 1f, 1f);
            return new Color(0.20f, 0.72f, 1f, 1f);
        }
    }
}
