using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach to an object with AmbientRotator. Listens for nearby BeatSyncObject sources and
    /// applies their reaction through ReactionEvaluator - the exact same code path used by
    /// ReactiveTriggerModule, so both systems always behave identically.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Beat Sync Module")]
    [RequireComponent(typeof(AmbientRotator))]
    public class BeatSyncModule : MonoBehaviour
    {
        [Header("Filter Settings")]
        [Tooltip("Only react to Beat Sync Objects with this tag. Leave as 'Untagged' to react to all sources.")]
        [SerializeField] private string sourceTag = "Untagged";

        [Header("Debug")]
        [Tooltip("Logs each detected beat and its strength to the Console.")]
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

            BeatSyncObject strongestSource = null;
            float strongestStrength = 0f;
            Vector3 directionFromSource = Vector3.up;

            var sources = BeatSyncObject.ActiveSources;
            for (int i = 0; i < sources.Count; i++)
            {
                BeatSyncObject source = sources[i];
                if (!ReactionEvaluator.PassesTagFilter(sourceTag, source.tag)) continue;

                float distance = Vector3.Distance(cachedTransform.position, source.Position);
                if (distance > source.InfluenceRadius) continue;
                if (!source.IsBeatDetected()) continue;

                float strength = source.GetStrengthAtPosition(cachedTransform.position);
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
                strongestSource.ConsumeBeat();

                var output = ReactionEvaluator.Evaluate(
                    strongestSource.Reaction, strongestStrength, Time.deltaTime, directionFromSource, ref reactionState);

                parentRotator.AddPositionOffset(output.positionOffset);
                parentRotator.SetExternalSpin(output.spinAxis, output.spinTargetSpeed, output.spinRampTime);
                lastSpinRampTime = output.spinRampTime;

                if (debugLogging)
                    Debug.Log($"[BeatSyncModule] Beat from '{strongestSource.name}' at strength {strongestStrength:F2}", this);
            }
            else
            {
                // No active beat this frame - ramp spin back down to rest using the last reaction's ramp
                // time. Position offset (pulse/wobble/push/attract) decays on its own via AmbientRotator's
                // External Decay setting, so nothing else needs to happen here.
                parentRotator.SetExternalSpin(Vector3.up, 0f, lastSpinRampTime);
            }
        }
    }
}
