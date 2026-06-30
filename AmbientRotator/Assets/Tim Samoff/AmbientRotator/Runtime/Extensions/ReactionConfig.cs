using UnityEngine;
using System;

namespace AmbientRotator
{
    /// <summary>
    /// Configuration for a reaction (Pulse, Rotate, Wobble).
    /// Used by BeatSyncObject and ReactiveTriggerObject sources.
    /// </summary>
    [Serializable]
    public class ReactionConfig
    {
        [Header("Pulse Reaction")]
        [Tooltip("Enable upward pulsing on each reaction.")]
        public bool pulse = true;

        [Tooltip("How high the object jumps. 0 = none, 0.5 = subtle, 1.0 = moderate, 3.0 = dramatic.")]
        [Range(0f, 5f)]
        public float pulseHeight = 1.0f;

        [Tooltip("Speed of the pulse oscillation. Higher = faster pulsing.")]
        [Range(0.1f, 10f)]
        public float pulseSpeed = 2f;

        [Tooltip("Multiplier for pulse intensity. 0 = none, 50 = moderate, 100 = extreme.")]
        [Range(0f, 100f)]
        public float pulseIntensity = 50f;

        [Header("Rotation Reaction")]
        [Tooltip("Enable rotation on each reaction.")]
        public bool rotate = false;

        [Tooltip("Base rotation speed in degrees per second.\n" +
                 "0 = none\n" +
                 "30 = slow spin\n" +
                 "90 = fast spin\n" +
                 "360 = one full rotation per second\n" +
                 "720 = two full rotations per second")]
        [Range(0f, 720f)]
        public float baseRotationSpeed = 90f;

        [Tooltip("How long it takes to ramp up to full speed and ramp down to stop.\n" +
                 "0 = instant (no ramp)\n" +
                 "1 = quick ramp (0.5 sec)\n" +
                 "5 = moderate ramp (2 sec)\n" +
                 "10 = slow ramp (4 sec)\n" +
                 "20 = very slow ramp (8 sec)")]
        [Range(0f, 20f)]
        public float rotationTransitionTime = 2f;

        [Header("Rotation Axes")]
        [Tooltip("Rotate around the X axis (tilt/forward flip).")]
        public bool rotateX = false;

        [Tooltip("Rotate around the Y axis (spin like a top).")]
        public bool rotateY = true;

        [Tooltip("Rotate around the Z axis (roll).")]
        public bool rotateZ = false;

        // Computed rotation axis from checkboxes
        public Vector3 RotationAxis
        {
            get
            {
                Vector3 axis = Vector3.zero;
                if (rotateX) axis.x = 1f;
                if (rotateY) axis.y = 1f;
                if (rotateZ) axis.z = 1f;
                
                // If all are false, default to Y axis
                if (axis.magnitude < 0.001f)
                    axis = Vector3.up;
                    
                return axis.normalized;
            }
        }

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

        public ReactionConfig Clone()
        {
            return new ReactionConfig
            {
                pulse = this.pulse,
                pulseHeight = this.pulseHeight,
                pulseSpeed = this.pulseSpeed,
                pulseIntensity = this.pulseIntensity,
                rotate = this.rotate,
                baseRotationSpeed = this.baseRotationSpeed,
                rotationTransitionTime = this.rotationTransitionTime,
                rotateX = this.rotateX,
                rotateY = this.rotateY,
                rotateZ = this.rotateZ,
                wobble = this.wobble,
                wobbleAmount = this.wobbleAmount,
                wobbleIntensity = this.wobbleIntensity,
                wobbleFrequency = this.wobbleFrequency,
                decaySpeed = this.decaySpeed,
                cooldown = this.cooldown
            };
        }

        public bool IsAnyActive()
        {
            bool pulseActive = pulse && pulseHeight > 0.01f && pulseIntensity > 0.01f;
            bool rotateActive = rotate && baseRotationSpeed > 0.01f;
            bool wobbleActive = wobble && wobbleAmount > 0.01f && wobbleIntensity > 0.01f;
            return pulseActive || rotateActive || wobbleActive;
        }

        public void Validate()
        {
            pulseHeight = Mathf.Clamp(pulseHeight, 0f, 5f);
            pulseSpeed = Mathf.Clamp(pulseSpeed, 0.1f, 10f);
            pulseIntensity = Mathf.Clamp(pulseIntensity, 0f, 100f);
            baseRotationSpeed = Mathf.Clamp(baseRotationSpeed, 0f, 720f);
            rotationTransitionTime = Mathf.Clamp(rotationTransitionTime, 0f, 20f);
            wobbleAmount = Mathf.Clamp(wobbleAmount, 0f, 5f);
            wobbleIntensity = Mathf.Clamp(wobbleIntensity, 0f, 100f);
            wobbleFrequency = Mathf.Clamp(wobbleFrequency, 0.1f, 10f);
            decaySpeed = Mathf.Clamp(decaySpeed, 0.01f, 1f);
            cooldown = Mathf.Clamp(cooldown, 0.001f, 0.5f);
        }
    }
}