using UnityEngine;
using System;

namespace AmbientRotator
{
    /// <summary>
    /// Shared reaction configuration for beat-sync and trigger-based reactions.
    /// Both BeatSyncModule and ReactiveTrigger use this for consistent behavior.
    /// </summary>
    [Serializable]
    public class MotionReaction
    {
        // Add this to the Pulse section in MotionReaction.cs
        [Header("Pulse Reaction")]
        [Tooltip("Enable upward pulsing on each reaction.")]
        public bool pulse = true;

        [Tooltip("How high the object jumps. 0 = none, 0.5 = subtle, 1.0 = moderate, 3.0 = dramatic.")]
        [Range(0f, 5f)]
        public float pulseHeight = 1.0f;

        [Tooltip("Speed of the pulse oscillation. Higher = faster pulsing.")]
        [Range(0.1f, 10f)]
        public float pulseSpeed = 2f;  // <-- ADD THIS

        [Tooltip("Multiplier for pulse intensity. 0 = none, 50 = moderate, 100 = extreme.")]
        [Range(0f, 100f)]
        public float pulseIntensity = 50f;

        [Header("Rotation Reaction")]
        [Tooltip("Enable rotation on each reaction.")]
        public bool rotate = false;

        [Tooltip("Maximum rotation angle in degrees per beat. 0-360 degrees.")]
        [Range(0f, 360f)]
        public float maxRotationAngle = 15f;

        [Tooltip("Multiplier for rotation intensity. 0 = none, 25 = subtle, 50 = moderate, 100 = extreme.")]
        [Range(0f, 100f)]
        public float rotationIntensity = 50f;

        [Tooltip("How smooth the rotation is. 0.01 = snappy, 0.1 = smooth, 0.5 = very smooth.")]
        [Range(0.01f, 0.5f)]
        public float rotationSmoothness = 0.08f;

        [Tooltip("Axis to rotate around. (0,1,0) = Y-axis spin, (1,0,0) = X-axis tilt, (0,0,1) = Z-axis roll.")]
        public Vector3 rotationAxis = Vector3.up;

        [Header("Wobble Reaction")]
        [Tooltip("Enable wobbling on each reaction.")]
        public bool wobble = false;

        [Tooltip("How far the object wobbles. 0 = none, 0.5 = subtle, 1.0 = moderate, 3.0 = dramatic.")]
        [Range(0f, 5f)]
        public float wobbleAmount = 1.0f;

        [Tooltip("Multiplier for wobble intensity. 0 = none, 50 = moderate, 100 = extreme.")]
        [Range(0f, 100f)]
        public float wobbleIntensity = 50f;

        [Tooltip("Speed of the wobble motion. Lower = slower sway, Higher = faster shake.")]
        [Range(0.1f, 10f)]
        public float wobbleFrequency = 2f;

        [Header("Reaction Timing")]
        [Tooltip("How quickly the reaction fades. Lower = snappier.")]
        [Range(0.01f, 1f)]
        public float decaySpeed = 0.1f;

        [Tooltip("Minimum time between reactions.")]
        [Range(0.001f, 0.5f)]
        public float cooldown = 0.05f;

        // --- Runtime state for smoothing ---
        [NonSerialized] private float currentRotationVelocity = 0f;

        /// <summary>
        /// Checks if any reaction is actually enabled and has meaningful values.
        /// </summary>
        public bool IsAnyReactionActive()
        {
            bool pulseActive = pulse && pulseHeight > 0.01f && pulseIntensity > 0.01f;
            bool rotateActive = rotate && maxRotationAngle > 0.01f && rotationIntensity > 0.01f;
            bool wobbleActive = wobble && wobbleAmount > 0.01f && wobbleIntensity > 0.01f;
            
            return pulseActive || rotateActive || wobbleActive;
        }

        /// <summary>
        /// Creates a deep copy of this reaction configuration.
        /// </summary>
        public MotionReaction Clone()
        {
            return new MotionReaction
            {
                pulse = this.pulse,
                pulseHeight = this.pulseHeight,
                pulseSpeed = this.pulseSpeed,
                pulseIntensity = this.pulseIntensity,
                rotate = this.rotate,
                maxRotationAngle = this.maxRotationAngle,
                rotationIntensity = this.rotationIntensity,
                rotationSmoothness = this.rotationSmoothness,
                rotationAxis = this.rotationAxis,
                wobble = this.wobble,
                wobbleAmount = this.wobbleAmount,
                wobbleIntensity = this.wobbleIntensity,
                wobbleFrequency = this.wobbleFrequency,
                decaySpeed = this.decaySpeed,
                cooldown = this.cooldown
            };
        }

        /// <summary>
        /// Applies a single reaction impulse to AmbientRotator as offsets.
        /// Use this for beat-sync or one-shot triggers.
        /// </summary>
        public void ApplyImpulse(AmbientRotator rotator, float strength, Vector3? direction = null)
        {
            if (rotator == null) return;
            
            // Skip if no reactions are active
            if (!IsAnyReactionActive()) return;

            Vector3 dir = direction ?? Vector3.up;
            
            // Normalize strength so 0-1 range maps properly
            float normalizedStrength = Mathf.Clamp01(strength);

            // --- PULSE (Position Offset + Force) ---
            if (pulse && pulseHeight > 0.01f && pulseIntensity > 0.01f)
            {
                float intensityMultiplier = pulseIntensity / 50f;
                
                // Apply a large position offset (visible jump)
                float jumpHeight = normalizedStrength * pulseHeight * intensityMultiplier * 0.5f;
                Vector3 pulseOffset = dir * jumpHeight;
                rotator.AddPositionOffset(pulseOffset);
                
                // Also apply a force for more dramatic effect
                float forceAmount = normalizedStrength * pulseHeight * intensityMultiplier * 15f;
                rotator.ApplyForce(dir * forceAmount);
            }

            // --- ROTATION (Rotation Offset) ---
            if (rotate && maxRotationAngle > 0.01f && rotationIntensity > 0.01f)
            {
                float intensityMultiplier = rotationIntensity / 50f;
                float targetRotation = normalizedStrength * maxRotationAngle * intensityMultiplier * 0.1f;
                
                // Smooth the rotation
                float smoothFactor = Mathf.Clamp01(Time.deltaTime / Mathf.Max(rotationSmoothness, 0.01f));
                currentRotationVelocity = Mathf.Lerp(currentRotationVelocity, targetRotation, smoothFactor);
                
                // Apply the smoothed rotation offset
                Vector3 axis = rotationAxis.normalized;
                if (axis.magnitude < 0.001f) axis = Vector3.up;
                rotator.AddRotationOffset(axis * currentRotationVelocity);
            }

            // --- WOBBLE (Position Offset) ---
            if (wobble && wobbleAmount > 0.01f && wobbleIntensity > 0.01f)
            {
                float intensityMultiplier = wobbleIntensity / 50f;
                
                // Apply a larger wobble offset
                float wobbleScale = normalizedStrength * wobbleAmount * intensityMultiplier * 0.3f;
                
                float wobbleX = Mathf.Sin(Time.time * wobbleFrequency) * wobbleScale;
                float wobbleZ = Mathf.Cos(Time.time * wobbleFrequency * 0.7f + 1.2f) * wobbleScale;
                
                Vector3 wobbleOffset = new Vector3(wobbleX, 0, wobbleZ);
                rotator.AddPositionOffset(wobbleOffset);
            }
        }

        /// <summary>
        /// Applies a continuous reaction over time to AmbientRotator.
        /// Use this for trigger-zone or persistent effects.
        /// </summary>
        public void ApplyContinuous(AmbientRotator rotator, float strength, Vector3? direction = null)
        {
            if (rotator == null) return;
            
            // Skip if no reactions are active
            if (!IsAnyReactionActive()) return;

            Vector3 dir = direction ?? Vector3.up;
            float normalizedStrength = Mathf.Clamp01(strength);

            // --- PULSE (continuous oscillation - larger offset) ---
            if (pulse && pulseHeight > 0.01f && pulseIntensity > 0.01f)
            {
                float intensityMultiplier = pulseIntensity / 50f;
                float pulseValue = Mathf.Sin(Time.time * 2f) * normalizedStrength * pulseHeight * intensityMultiplier * 0.5f;
                Vector3 pulseOffset = dir * pulseValue;
                rotator.AddPositionOffset(pulseOffset);
            }

            // --- WOBBLE (continuous - larger offset) ---
            if (wobble && wobbleAmount > 0.01f && wobbleIntensity > 0.01f)
            {
                float intensityMultiplier = wobbleIntensity / 50f;
                float wobbleScale = normalizedStrength * wobbleAmount * intensityMultiplier * 0.5f;
                Vector3 wobbleOffset = new Vector3(
                    Mathf.Sin(Time.time * wobbleFrequency) * wobbleScale,
                    0,
                    Mathf.Cos(Time.time * wobbleFrequency * 0.7f + 1.2f) * wobbleScale
                );
                rotator.AddPositionOffset(wobbleOffset);
            }

            // --- ROTATION (continuous speed modulation) ---
            if (rotate && maxRotationAngle > 0.01f && rotationIntensity > 0.01f)
            {
                float intensityMultiplier = rotationIntensity / 50f;
                float speedMultiplier = 1f + (normalizedStrength * maxRotationAngle * 0.05f * intensityMultiplier);
                rotator.SetSpeedMultiplier(speedMultiplier);
            }
        }

        /// <summary>
        /// Resets the internal smoothing state.
        /// </summary>
        public void ResetSmoothing()
        {
            currentRotationVelocity = 0f;
        }

        /// <summary>
        /// Validates and clamps all values to acceptable ranges.
        /// </summary>
        public void Validate()
        {
            pulseHeight = Mathf.Clamp(pulseHeight, 0f, 5f);
            pulseIntensity = Mathf.Clamp(pulseIntensity, 0f, 100f);
            maxRotationAngle = Mathf.Clamp(maxRotationAngle, 0f, 360f);
            rotationIntensity = Mathf.Clamp(rotationIntensity, 0f, 100f);
            rotationSmoothness = Mathf.Clamp(rotationSmoothness, 0.01f, 0.5f);
            wobbleAmount = Mathf.Clamp(wobbleAmount, 0f, 5f);
            wobbleIntensity = Mathf.Clamp(wobbleIntensity, 0f, 100f);
            wobbleFrequency = Mathf.Clamp(wobbleFrequency, 0.1f, 10f);
            decaySpeed = Mathf.Clamp(decaySpeed, 0.01f, 1f);
            cooldown = Mathf.Clamp(cooldown, 0.001f, 0.5f);
            
            if (rotationAxis.magnitude < 0.001f)
                rotationAxis = Vector3.up;
        }
    }
}