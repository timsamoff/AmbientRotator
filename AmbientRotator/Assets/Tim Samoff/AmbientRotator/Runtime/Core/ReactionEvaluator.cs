using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Turns a ReactionConfig + strength into concrete motion. This is the single
    /// place reaction math lives - BeatSyncModule and ReactiveTriggerModule both
    /// call this and nothing else, so the two systems can never drift apart.
    /// </summary>
    public static class ReactionEvaluator
    {
        /// <summary>Per-receiver state that must persist between calls (oscillation phase).</summary>
        public struct ReactionState
        {
            public float pulseTimer;
            public float wobbleTimer;
        }

        /// <summary>Result of evaluating a reaction for one frame.</summary>
        public struct ReactionOutput
        {
            /// <summary>Position offset to feed into AmbientRotator.AddPositionOffset.</summary>
            public Vector3 positionOffset;

            /// <summary>Axis to feed into AmbientRotator.SetExternalSpin.</summary>
            public Vector3 spinAxis;

            /// <summary>Target spin speed (deg/sec) to feed into AmbientRotator.SetExternalSpin.</summary>
            public float spinTargetSpeed;

            /// <summary>Ramp time (seconds) to feed into AmbientRotator.SetExternalSpin.</summary>
            public float spinRampTime;
        }

        /// <summary>
        /// Evaluates one frame of a reaction.
        /// </summary>
        /// <param name="reaction">The shared reaction definition.</param>
        /// <param name="strength">0-1 strength of the reaction this frame (falloff, beat strength, etc).</param>
        /// <param name="deltaTime">Time.deltaTime.</param>
        /// <param name="directionFromSource">Normalized direction from the source to the receiver, used for push/attract.</param>
        /// <param name="state">Persistent oscillation state for this receiver - keep one instance per module.</param>
        public static ReactionOutput Evaluate(ReactionConfig reaction, float strength, float deltaTime, Vector3 directionFromSource, ref ReactionState state)
        {
            var output = new ReactionOutput
            {
                spinAxis = reaction.RotationAxis,
                spinRampTime = Mathf.Max(0.01f, reaction.rotationRampTime)
            };

            if (strength <= 0.001f)
                return output;

            if (reaction.pulse && reaction.pulseHeight > 0.01f && reaction.pulseIntensity > 0.01f)
            {
                state.pulseTimer += deltaTime * reaction.pulseSpeed;
                float amplitude = strength * reaction.pulseHeight * (reaction.pulseIntensity / 50f);
                output.positionOffset += Vector3.up * Mathf.Sin(state.pulseTimer) * amplitude;
            }

            if (reaction.wobble && reaction.wobbleAmount > 0.01f && reaction.wobbleIntensity > 0.01f)
            {
                state.wobbleTimer += deltaTime * reaction.wobbleFrequency;
                float amplitude = strength * reaction.wobbleAmount * (reaction.wobbleIntensity / 50f);
                output.positionOffset += new Vector3(
                    Mathf.Sin(state.wobbleTimer) * amplitude,
                    0f,
                    Mathf.Cos(state.wobbleTimer * 0.7f + 1.2f) * amplitude);
            }

            if (reaction.pushAway && reaction.pushAwayStrength > 0.01f)
                output.positionOffset += directionFromSource * strength * reaction.pushAwayStrength;

            if (reaction.attract && reaction.attractStrength > 0.01f)
                output.positionOffset -= directionFromSource * strength * reaction.attractStrength;

            if (reaction.rotate && reaction.rotationIntensity > 0.01f)
                output.spinTargetSpeed = reaction.RotationSpeedDegPerSec * strength;

            return output;
        }

        /// <summary>Shared tag-filter logic used by both reaction modules.</summary>
        public static bool PassesTagFilter(string filterTag, string sourceTag)
        {
            return string.IsNullOrEmpty(filterTag) || filterTag == "Untagged" || filterTag == sourceTag;
        }
    }
}
