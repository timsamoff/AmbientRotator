using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach to objects with AmbientRotator. Listens for ReactiveTriggerObject sources nearby.
    /// When triggered, rotation smoothly ramps from current speed to target speed.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Reactive Trigger Module")]
    [RequireComponent(typeof(AmbientRotator))]
    public class ReactiveTriggerModule : MonoBehaviour
    {
        [Header("Filter Settings")]
        [Tooltip("Only react to sources with this tag. Leave 'Untagged' to react to all sources.")]
        [SerializeField] private string sourceTag = "Untagged";

        [Header("Return Settings")]
        [Tooltip("How fast the object returns to its normal state when the trigger exits.")]
        [SerializeField, Range(1f, 20f)] private float returnSpeed = 5f;

        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;

        private AmbientRotator parentRotator;
        private Transform cachedTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool isAnyTriggerActive = false;

        // Store the original AmbientRotator state
        private float originalSpeed = 1f;
        private bool wasAmbientRotatorEnabled = true;

        // Pulse state (position offset)
        private float pulseTimer = 0f;
        private float wobbleTimer = 0f;

        // Store the strongest reaction values from all active sources
        private float strongestPulse = 0f;
        private float strongestTargetRotationSpeed = 0f;
        private float strongestWobble = 0f;
        private Vector3 strongestRotationAxis = Vector3.up;
        private float strongestPulseSpeed = 1f;
        private float strongestWobbleFrequency = 1f;
        private float strongestPulseIntensity = 50f;
        private float strongestWobbleIntensity = 50f;
        private float strongestPulseHeight = 1f;
        private float strongestWobbleAmount = 1f;
        private float strongestTransitionTime = 2f;

        // Push/Pull offset (position)
        private Vector3 pushPullOffset = Vector3.zero;

        // Track rotation state
        private bool isRotating = false;
        private float rotationSpeed = 0f; // Current actual rotation speed
        private bool hasInheritedSpeed = false;

        // Track original rotation for smooth return
        private Quaternion targetReturnRotation = Quaternion.identity;

        private void Start()
        {
            parentRotator = GetComponent<AmbientRotator>();
            cachedTransform = transform;
            originalPosition = cachedTransform.position;
            originalRotation = cachedTransform.rotation;
            targetReturnRotation = originalRotation;

            if (parentRotator == null)
            {
                Debug.LogError($"ReactiveTriggerModule: No AmbientRotator component found on {gameObject.name}!");
                return;
            }

            originalSpeed = parentRotator.Speed;
            wasAmbientRotatorEnabled = parentRotator.enabled;
            
            // Initialize rotation speed from AmbientRotator's current speed
            // This is the starting point for smooth ramping
            rotationSpeed = parentRotator.Speed * 0.5f;
            hasInheritedSpeed = true;
            
            if (debugLogging)
            {
                Debug.Log($"  Initial rotation speed inherited: {rotationSpeed:F1} deg/sec from AmbientRotator");
            }
        }

        private void Update()
        {
            if (parentRotator == null) return;

            // Reset state each frame
            isAnyTriggerActive = false;
            pushPullOffset = Vector3.zero;
            
            // Reset strongest values
            strongestPulse = 0f;
            strongestTargetRotationSpeed = 0f;
            strongestWobble = 0f;
            strongestRotationAxis = Vector3.up;
            strongestTransitionTime = 2f;
            isRotating = false;

            ReactiveTriggerObject[] sources = FindObjectsByType<ReactiveTriggerObject>(FindObjectsInactive.Exclude);
            
            foreach (var source in sources)
            {
                // Filter by tag
                if (!string.IsNullOrEmpty(sourceTag) && sourceTag != "Untagged")
                {
                    if (!source.CompareTag(sourceTag)) continue;
                }

                float distance = Vector3.Distance(cachedTransform.position, source.Position);
                if (distance > source.ReactionRadius) continue;

                if ((source.AffectedLayers & (1 << gameObject.layer)) == 0) continue;

                float strength = source.GetStrengthAtPosition(cachedTransform.position);
                
                // Calculate direction FROM trigger TO this object (for push away)
                Vector3 directionFromTrigger = (cachedTransform.position - source.Position).normalized;
                if (directionFromTrigger.magnitude < 0.01f) directionFromTrigger = Vector3.up;

                isAnyTriggerActive = true;

                // Track strongest reactions
                TrackStrongestReactions(source.Reaction, strength);

                // Push/Pull - accumulate from all sources
                ApplyPushPull(source, strength, directionFromTrigger);

                if (debugLogging && strength > 0.1f)
                {
                    Debug.Log($"Trigger from {source.name} at strength {strength:F2}");
                }
            }

            // --- Apply reactions ---
            if (isAnyTriggerActive)
            {
                ApplyReactions();
            }
            else
            {
                // Return to normal when no triggers are active
                ReturnToNormal();
            }
        }

        private void TrackStrongestReactions(ReactionConfig reaction, float strength)
        {
            // Track strongest pulse (position offset)
            if (reaction.pulse && reaction.pulseHeight > 0.01f && reaction.pulseIntensity > 0.01f)
            {
                float intensityMultiplier = reaction.pulseIntensity / 50f;
                float effectiveStrength = strength * reaction.pulseHeight * intensityMultiplier;
                if (effectiveStrength > strongestPulse)
                {
                    strongestPulse = effectiveStrength;
                    strongestPulseHeight = reaction.pulseHeight;
                    strongestPulseSpeed = reaction.pulseSpeed;
                    strongestPulseIntensity = reaction.pulseIntensity;
                }
            }

            // Track strongest rotation with transition time
            if (reaction.rotate && reaction.baseRotationSpeed > 0.01f)
            {
                // Calculate target speed based on strength
                float targetSpeed = reaction.baseRotationSpeed * strength;
                
                if (targetSpeed > strongestTargetRotationSpeed)
                {
                    strongestTargetRotationSpeed = targetSpeed;
                    strongestRotationAxis = reaction.RotationAxis;
                    strongestTransitionTime = reaction.rotationTransitionTime;
                    isRotating = true;
                }
            }

            // Track strongest wobble (position offset)
            if (reaction.wobble && reaction.wobbleAmount > 0.01f && reaction.wobbleIntensity > 0.01f)
            {
                float intensityMultiplier = reaction.wobbleIntensity / 50f;
                float effectiveStrength = strength * reaction.wobbleAmount * intensityMultiplier;
                if (effectiveStrength > strongestWobble)
                {
                    strongestWobble = effectiveStrength;
                    strongestWobbleAmount = reaction.wobbleAmount;
                    strongestWobbleFrequency = reaction.wobbleFrequency;
                    strongestWobbleIntensity = reaction.wobbleIntensity;
                }
            }
        }

        private void ApplyPushPull(ReactiveTriggerObject source, float strength, Vector3 directionFromTrigger)
        {
            // Push Away: move in the direction FROM the trigger TO this object
            if (source.PushAway && strength > 0.01f)
            {
                pushPullOffset += directionFromTrigger * strength * source.PushAwayStrength * 0.5f;
            }

            // Attract: move in the direction FROM this object TO the trigger (opposite of push)
            if (source.Attract && strength > 0.01f)
            {
                pushPullOffset += -directionFromTrigger * strength * source.AttractStrength * 0.5f;
            }
        }

        private void ApplyReactions()
        {
            // --- INHERIT CURRENT SPEED FROM AMBIENT ROTATOR (if not already) ---
            if (!hasInheritedSpeed && parentRotator != null && parentRotator.enabled)
            {
                // Inherit the current rotation speed from AmbientRotator
                rotationSpeed = parentRotator.Speed * 0.5f;
                hasInheritedSpeed = true;
                
                if (debugLogging)
                {
                    Debug.Log($"  Inherited speed: {rotationSpeed:F1} deg/sec from AmbientRotator");
                }
            }

            // --- DISABLE AMBIENT ROTATOR - Complete takeover ---
            if (parentRotator != null && parentRotator.enabled)
            {
                parentRotator.enabled = false;
                wasAmbientRotatorEnabled = true;
            }

            // --- Position Offset: Pulse ---
            Vector3 positionOffset = Vector3.zero;

            if (strongestPulse > 0.01f)
            {
                float intensityMultiplier = strongestPulseIntensity / 50f;
                float targetHeight = strongestPulse * 0.5f;
                
                pulseTimer += Time.deltaTime * strongestPulseSpeed;
                float pulseValue = Mathf.Sin(pulseTimer) * targetHeight;
                
                positionOffset += Vector3.up * pulseValue;
            }

            // --- Position Offset: Wobble ---
            if (strongestWobble > 0.01f)
            {
                float intensityMultiplier = strongestWobbleIntensity / 50f;
                float wobbleScale = strongestWobble * 0.3f;
                
                wobbleTimer += Time.deltaTime * strongestWobbleFrequency;
                float wobbleX = Mathf.Sin(wobbleTimer) * wobbleScale;
                float wobbleZ = Mathf.Cos(wobbleTimer * 0.7f + 1.2f) * wobbleScale;
                
                positionOffset += new Vector3(wobbleX, 0, wobbleZ);
            }

            // --- Position Offset: Push/Pull ---
            positionOffset += pushPullOffset;

            // Apply position offsets directly
            if (positionOffset.magnitude > 0.001f)
            {
                Vector3 targetPos = originalPosition + positionOffset;
                cachedTransform.position = Vector3.Lerp(cachedTransform.position, targetPos, Time.deltaTime * 8f);
            }

            // --- ROTATION: SMOOTH RAMP FROM CURRENT SPEED TO TARGET ---
            if (isRotating && strongestTargetRotationSpeed > 0.01f)
            {
                // Update target return rotation to current rotation
                targetReturnRotation = cachedTransform.rotation;
                
                // Calculate transition speed based on transition time
                // Shorter time = faster ramp, Longer time = slower ramp
                float transitionSpeed = 1f / Mathf.Max(strongestTransitionTime, 0.01f);
                
                // Smoothly transition from current rotation speed to target speed
                // Using lerp with time-based speed for smooth but responsive ramping
                float lerpFactor = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime * 2f);
                rotationSpeed = Mathf.Lerp(rotationSpeed, strongestTargetRotationSpeed, lerpFactor);
                
                // Apply the rotation
                float rotationAmount = rotationSpeed * Time.deltaTime;
                cachedTransform.Rotate(strongestRotationAxis, rotationAmount, Space.World);
                
                if (debugLogging && rotationSpeed > 5f)
                {
                    Debug.Log($"  🔥 ROTATION: {rotationSpeed:F1} deg/sec (Target: {strongestTargetRotationSpeed:F1}) [Transition: {strongestTransitionTime:F1}s]");
                }
            }
        }

        private void ReturnToNormal()
        {
            // --- Smoothly decelerate rotation back to 0 ---
            if (rotationSpeed > 0.5f)
            {
                // Use the transition time for smooth deceleration
                float transitionSpeed = 1f / Mathf.Max(strongestTransitionTime, 0.01f);
                float lerpFactor = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime * 2f);
                rotationSpeed = Mathf.Lerp(rotationSpeed, 0f, lerpFactor);
                
                // Apply decaying rotation
                if (rotationSpeed > 0.5f)
                {
                    float rotationAmount = rotationSpeed * Time.deltaTime;
                    cachedTransform.Rotate(strongestRotationAxis, rotationAmount, Space.World);
                    
                    // Update target return rotation
                    targetReturnRotation = cachedTransform.rotation;
                    
                    if (debugLogging)
                    {
                        Debug.Log($"  🔄 ROTATION DECAY: {rotationSpeed:F1} deg/sec");
                    }
                }
            }
            else
            {
                rotationSpeed = 0f;
                hasInheritedSpeed = false; // Reset for next trigger
                
                // --- Re-enable AmbientRotator ---
                if (parentRotator != null && !parentRotator.enabled && wasAmbientRotatorEnabled)
                {
                    parentRotator.enabled = true;
                    parentRotator.SetSpeed(originalSpeed);
                    wasAmbientRotatorEnabled = false;
                }

                // Return position
                if (Vector3.Distance(cachedTransform.position, originalPosition) > 0.001f)
                {
                    cachedTransform.position = Vector3.Lerp(cachedTransform.position, originalPosition, Time.deltaTime * returnSpeed);
                }
                else
                {
                    cachedTransform.position = originalPosition;
                }

                // --- SMOOTHLY BLEND ROTATION BACK TO ORIGINAL ---
                if (Quaternion.Angle(cachedTransform.rotation, originalRotation) > 0.01f)
                {
                    cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, originalRotation, Time.deltaTime * returnSpeed);
                    targetReturnRotation = cachedTransform.rotation;
                }
                else
                {
                    cachedTransform.rotation = originalRotation;
                    targetReturnRotation = originalRotation;
                }

                // Reset timers
                pulseTimer = 0f;
                wobbleTimer = 0f;
            }
        }

        // Update original position if object moved externally
        private void LateUpdate()
        {
            if (!isAnyTriggerActive && rotationSpeed < 0.01f)
            {
                if (Vector3.Distance(cachedTransform.position, originalPosition) < 0.001f)
                {
                    originalPosition = cachedTransform.position;
                    originalRotation = cachedTransform.rotation;
                    targetReturnRotation = originalRotation;
                }
            }
        }

        public void SetReturnSpeed(float speed)
        {
            returnSpeed = Mathf.Clamp(speed, 1f, 20f);
        }
    }
}