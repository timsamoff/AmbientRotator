using UnityEngine;
using System;

namespace AmbientRotator
{
    /// <summary>
    /// Single source of truth for what a reaction does and how it feels.
    /// Shared identically by BeatSyncObject and ReactiveTriggerObject - whatever
    /// is defined here is exactly what both systems will produce, with no
    /// per-system special casing.
    /// </summary>
    [Serializable]
    public class ReactionConfig
    {
        /// <summary>Maximum spin speed in degrees/second, reached at rotationIntensity = 100.</summary>
        public const float MaxRotationSpeed = 720f;

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

        [Tooltip("How intense the rotation reaction is, from calm to chaotic.\n" +
                 "0 = no rotation\n" +
                 "25 = slow, gentle spin\n" +
                 "50 = moderate spin\n" +
                 "75 = fast spin\n" +
                 "100 = hectic, chaotic spin")]
        [Range(0f, 100f)]
        public float rotationIntensity = 40f;

        [Tooltip("How long it takes to ramp up to full spin speed when entering, and to smoothly slow to a stop when exiting. In seconds.\n" +
                 "0 = instant\n" +
                 "0.5 = snappy\n" +
                 "2 = smooth\n" +
                 "5+ = slow and heavy")]
        [Range(0f, 10f)]
        public float rotationRampTime = 1f;

        /// <summary>Rotation speed in degrees/second implied by rotationIntensity.</summary>
        public float RotationSpeedDegPerSec => (rotationIntensity / 100f) * MaxRotationSpeed;

        [Header("Rotation Axes")]
        [Tooltip("Rotate around the X axis (tilt/forward flip).")]
        public bool rotateX = false;

        [Tooltip("Rotate around the Y axis (spin like a top).")]
        public bool rotateY = true;

        [Tooltip("Rotate around the Z axis (roll).")]
        public bool rotateZ = false;

        /// <summary>Computed rotation axis from the checkboxes above. Defaults to Y if none are checked.</summary>
        public Vector3 RotationAxis
        {
            get
            {
                Vector3 axis = Vector3.zero;
                if (rotateX) axis.x = 1f;
                if (rotateY) axis.y = 1f;
                if (rotateZ) axis.z = 1f;

                if (axis.sqrMagnitude < 0.001f)
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

        [Header("Push / Attract")]
        [Tooltip("Pushes affected objects away.\n" +
                 "Reactive Trigger: applies continuously while inside the trigger, strongest at the center.\n" +
                 "Beat Sync: applies as an outward kick on every detected beat.")]
        public bool pushAway = false;

        [Tooltip("How strongly affected objects are pushed away. 0 = none, 20 = very strong.")]
        [Range(0f, 20f)]
        public float pushAwayStrength = 5f;

        [Tooltip("Pulls affected objects inward.\n" +
                 "Reactive Trigger: applies continuously while inside the trigger, strongest at the center.\n" +
                 "Beat Sync: applies as an inward pull on every detected beat.")]
        public bool attract = false;

        [Tooltip("How strongly affected objects are pulled inward. 0 = none, 20 = very strong.")]
        [Range(0f, 20f)]
        public float attractStrength = 5f;

        [Header("Reaction Timing")]
        [Tooltip("How quickly the reaction fades once triggered. Lower = snappier.")]
        [Range(0.01f, 1f)]
        public float decaySpeed = 0.1f;

        [Tooltip("Minimum time between reactions (Beat Sync only).")]
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
                rotationIntensity = this.rotationIntensity,
                rotationRampTime = this.rotationRampTime,
                rotateX = this.rotateX,
                rotateY = this.rotateY,
                rotateZ = this.rotateZ,
                wobble = this.wobble,
                wobbleAmount = this.wobbleAmount,
                wobbleIntensity = this.wobbleIntensity,
                wobbleFrequency = this.wobbleFrequency,
                pushAway = this.pushAway,
                pushAwayStrength = this.pushAwayStrength,
                attract = this.attract,
                attractStrength = this.attractStrength,
                decaySpeed = this.decaySpeed,
                cooldown = this.cooldown
            };
        }

        public bool IsAnyActive()
        {
            bool pulseActive = pulse && pulseHeight > 0.01f && pulseIntensity > 0.01f;
            bool rotateActive = rotate && rotationIntensity > 0.01f;
            bool wobbleActive = wobble && wobbleAmount > 0.01f && wobbleIntensity > 0.01f;
            bool pushPullActive = (pushAway && pushAwayStrength > 0.01f) || (attract && attractStrength > 0.01f);
            return pulseActive || rotateActive || wobbleActive || pushPullActive;
        }

        public void Validate()
        {
            pulseHeight = Mathf.Clamp(pulseHeight, 0f, 5f);
            pulseSpeed = Mathf.Clamp(pulseSpeed, 0.1f, 10f);
            pulseIntensity = Mathf.Clamp(pulseIntensity, 0f, 100f);
            rotationIntensity = Mathf.Clamp(rotationIntensity, 0f, 100f);
            rotationRampTime = Mathf.Clamp(rotationRampTime, 0f, 10f);
            wobbleAmount = Mathf.Clamp(wobbleAmount, 0f, 5f);
            wobbleIntensity = Mathf.Clamp(wobbleIntensity, 0f, 100f);
            wobbleFrequency = Mathf.Clamp(wobbleFrequency, 0.1f, 10f);
            pushAwayStrength = Mathf.Clamp(pushAwayStrength, 0f, 20f);
            attractStrength = Mathf.Clamp(attractStrength, 0f, 20f);
            decaySpeed = Mathf.Clamp(decaySpeed, 0.01f, 1f);
            cooldown = Mathf.Clamp(cooldown, 0.001f, 0.5f);
        }
    }
}
