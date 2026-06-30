using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach to objects with AmbientRotator. Listens for BeatSyncObject sources nearby.
    /// Applies whatever reaction the source defines.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Beat Sync Module")]
    [RequireComponent(typeof(AmbientRotator))]
    public class BeatSyncModule : MonoBehaviour
    {
        [Header("Filter Settings")]
        [Tooltip("Only react to sources with this tag. Leave 'Untagged' to react to all sources.")]
        [SerializeField] private string sourceTag = "Untagged";

        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;

        private AmbientRotator parentRotator;
        private Transform cachedTransform;
        private float pulseTimer = 0f;

        // Accumulated effects
        private Vector3 accumulatedPulseOffset = Vector3.zero;
        private Vector3 accumulatedRotationOffset = Vector3.zero;
        private Vector3 accumulatedWobbleOffset = Vector3.zero;

        // Rotation smoothing for beat sync
        private float currentRotationSpeed = 0f;
        private float targetRotationSpeed = 0f;

        private void Start()
        {
            parentRotator = GetComponent<AmbientRotator>();
            cachedTransform = transform;

            if (parentRotator == null)
            {
                Debug.LogError($"BeatSyncModule: No AmbientRotator component found on {gameObject.name}!");
            }
        }

        private void Update()
        {
            if (parentRotator == null) return;

            BeatSyncObject[] sources = FindObjectsByType<BeatSyncObject>(FindObjectsInactive.Exclude);
            
            foreach (var source in sources)
            {
                // Filter by tag if specified
                if (!string.IsNullOrEmpty(sourceTag) && sourceTag != "Untagged")
                {
                    if (!source.CompareTag(sourceTag)) continue;
                }

                float distance = Vector3.Distance(cachedTransform.position, source.Position);
                if (distance > source.InfluenceRadius) continue;

                if (source.IsBeatDetected())
                {
                    float strength = source.GetStrengthAtPosition(cachedTransform.position);
                    ApplyReaction(source.Reaction, strength);
                    source.ConsumeBeat();

                    if (debugLogging)
                    {
                        Debug.Log($"Beat from {source.name} at strength {strength:F2}");
                    }
                }
            }

            ApplyAccumulatedEffects();
        }

        private void ApplyReaction(ReactionConfig reaction, float strength)
        {
            if (parentRotator == null) return;

            float scaledStrength = strength * 0.5f;

            // --- Pulse ---
            if (reaction.pulse && reaction.pulseHeight > 0.01f && reaction.pulseIntensity > 0.01f)
            {
                float intensityMultiplier = reaction.pulseIntensity / 50f;
                float targetHeight = scaledStrength * reaction.pulseHeight * intensityMultiplier;
                
                pulseTimer += Time.deltaTime * reaction.pulseSpeed;
                float pulseValue = Mathf.Sin(pulseTimer) * targetHeight;
                
                accumulatedPulseOffset += Vector3.up * pulseValue * Time.deltaTime * 4f;
            }

            // --- Rotation - Uses baseRotationSpeed directly (no intensity) ---
            if (reaction.rotate && reaction.baseRotationSpeed > 0.01f)
            {
                // Calculate target rotation speed based on beat strength
                targetRotationSpeed = reaction.baseRotationSpeed * scaledStrength * 0.1f;
                
                // Smooth the rotation speed using transition time
                float transitionSpeed = 1f / Mathf.Max(reaction.rotationTransitionTime, 0.01f);
                currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, Time.deltaTime * transitionSpeed * 5f);
                
                Vector3 axis = reaction.RotationAxis;
                accumulatedRotationOffset += axis * currentRotationSpeed * Time.deltaTime;
            }

            // --- Wobble ---
            if (reaction.wobble && reaction.wobbleAmount > 0.01f && reaction.wobbleIntensity > 0.01f)
            {
                float intensityMultiplier = reaction.wobbleIntensity / 50f;
                float wobbleScale = scaledStrength * reaction.wobbleAmount * intensityMultiplier * 0.15f;
                
                float wobbleX = Mathf.Sin(Time.time * reaction.wobbleFrequency) * wobbleScale;
                float wobbleZ = Mathf.Cos(Time.time * reaction.wobbleFrequency * 0.7f + 1.2f) * wobbleScale;
                
                accumulatedWobbleOffset += new Vector3(wobbleX, 0, wobbleZ) * Time.deltaTime * 4f;
            }
        }

        private void ApplyAccumulatedEffects()
        {
            if (parentRotator == null) return;

            if (accumulatedPulseOffset.magnitude > 0.0001f)
            {
                parentRotator.AddPositionOffset(accumulatedPulseOffset);
                accumulatedPulseOffset *= 0.95f;
            }

            if (accumulatedWobbleOffset.magnitude > 0.0001f)
            {
                parentRotator.AddPositionOffset(accumulatedWobbleOffset);
                accumulatedWobbleOffset *= 0.95f;
            }

            if (accumulatedRotationOffset.magnitude > 0.0001f)
            {
                parentRotator.AddRotationOffset(accumulatedRotationOffset);
                accumulatedRotationOffset *= 0.95f;
            }

            accumulatedPulseOffset = Vector3.ClampMagnitude(accumulatedPulseOffset, 10f);
            accumulatedWobbleOffset = Vector3.ClampMagnitude(accumulatedWobbleOffset, 10f);
            accumulatedRotationOffset = Vector3.ClampMagnitude(accumulatedRotationOffset, 45f);
        }
    }
}