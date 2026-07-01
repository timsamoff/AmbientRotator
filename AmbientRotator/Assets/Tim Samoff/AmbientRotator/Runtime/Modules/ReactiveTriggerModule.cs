using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach to an object with AmbientRotator. Listens for nearby ReactiveTriggerObject sources and
    /// applies their reaction through ReactionEvaluator - the exact same code path used by
    /// BeatSyncModule, so both systems always behave identically.
    ///
    /// Unlike the previous version, this module never disables or bypasses AmbientRotator: it only
    /// ever calls AddPositionOffset/SetExternalSpin, so AmbientRotator remains the single source of
    /// truth for the transform at all times, and return-to-origin is handled entirely by
    /// AmbientRotator's own decay/ramp logic rather than hand-rolled here.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Reactive Trigger Module")]
    [RequireComponent(typeof(AmbientRotator))]
    public class ReactiveTriggerModule : MonoBehaviour
    {
        [Header("Filter Settings")]
        [Tooltip("Only react to Reactive Trigger Objects with this tag. Leave as 'Untagged' to react to all sources.")]
        [SerializeField] private string sourceTag = "Untagged";

        [Header("Debug")]
        [Tooltip("Logs the strongest active trigger and its strength to the Console.")]
        [SerializeField] private bool debugLogging = false;

        private AmbientRotator parentRotator;
        private Transform cachedTransform;
        private ReactionEvaluator.ReactionState reactionState;
        private float lastSpinRampTime = 1f;

        private void Awake()
        {
            parentRotator = GetComponent<AmbientRotator>();
            cachedTransform = transform;
        }

        private void Update()
        {
            if (parentRotator == null) return;

            ReactiveTriggerObject strongestSource = null;
            float strongestStrength = 0f;
            Vector3 directionFromSource = Vector3.up;

            var sources = ReactiveTriggerObject.ActiveSources;
            for (int i = 0; i < sources.Count; i++)
            {
                ReactiveTriggerObject source = sources[i];
                if (!ReactionEvaluator.PassesTagFilter(sourceTag, source.tag)) continue;
                if ((source.AffectedLayers.value & (1 << gameObject.layer)) == 0) continue;

                float distance = Vector3.Distance(cachedTransform.position, source.Position);
                if (distance > source.ReactionRadius) continue;

                float strength = source.GetStrengthAtPosition(cachedTransform.position);
                if (strength <= 0f) continue;

                if (strength > strongestStrength)
                {
                    strongestStrength = strength;
                    strongestSource = source;
                    Vector3 dir = cachedTransform.position - source.Position;
                    directionFromSource = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.up;
                }
            }

            if (strongestSource != null)
            {
                var output = ReactionEvaluator.Evaluate(
                    strongestSource.Reaction, strongestStrength, Time.deltaTime, directionFromSource, ref reactionState);

                parentRotator.AddPositionOffset(output.positionOffset);
                parentRotator.SetExternalSpin(output.spinAxis, output.spinTargetSpeed, output.spinRampTime);
                lastSpinRampTime = output.spinRampTime;

                if (debugLogging)
                    Debug.Log($"[ReactiveTriggerModule] Reacting to '{strongestSource.name}' at strength {strongestStrength:F2}", this);
            }
            else
            {
                // No trigger active this frame - ramp spin back down to rest using the last reaction's
                // ramp time. Position offset (pulse/wobble/push/attract) decays on its own via
                // AmbientRotator's External Decay setting.
                parentRotator.SetExternalSpin(Vector3.up, 0f, lastSpinRampTime);
            }
        }
    }
}
