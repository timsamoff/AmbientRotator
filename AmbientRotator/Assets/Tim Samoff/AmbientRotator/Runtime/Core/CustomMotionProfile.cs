using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    /// <summary>
    /// A reusable, fully custom motion definition built from Animation Curves, saved as a project
    /// asset. Create via Assets > Create > Ambient Rotator > Custom Profile, then assign it to an
    /// AmbientRotator's "Custom Profile" field and enable "Use Custom Profile" to use it instead of
    /// the built-in motion profiles.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCustomProfile", menuName = "Ambient Rotator/Custom Profile")]
    public class CustomMotionProfile : ScriptableObject
    {
        [Header("Motion Curves")]
        [Tooltip("Rotation offset on the X axis over one cycle (0-1 on the curve maps to 0-Curve Duration seconds).")]
        public AnimationCurve xCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Tooltip("Rotation offset on the Y axis over one cycle.")]
        public AnimationCurve yCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Tooltip("Rotation offset on the Z axis over one cycle.")]
        public AnimationCurve zCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Header("Curve Settings")]
        [Tooltip("How long, in seconds, one full pass through the curves above takes.")]
        public float curveDuration = 2f;

        [Tooltip("When enabled, the curves repeat continuously. When disabled, motion holds at the curve's final value once Curve Duration is reached.")]
        public bool loopCurve = true;

        [Tooltip("PLANNED, NOT YET FUNCTIONAL: intended to give each object using this profile a random starting point in the curve so multiple objects don't move in perfect sync. Not currently read anywhere - for staggering, use AmbientGroup's Spread setting instead.")]
        public bool randomizeStart = true;

        [Header("Layered Motion")]
        [Tooltip("Additional curve layers that add on top of the main curves above - each with its own speed, intensity, and phase offset. Useful for combining a slow primary sway with a faster secondary detail motion.")]
        public List<MotionLayer> layers = new List<MotionLayer>();

        [Header("Secondary Motion")]
        [Tooltip("Enable a second, fixed-relationship motion layer (0.7x speed, offset by 1.5s) that adds on top of the main curves - a quick way to add texture without configuring a full custom layer above.")]
        public bool useSecondaryMotion = false;

        [Tooltip("How strongly the secondary motion affects the result.")]
        public float secondaryIntensity = 0.5f;

        [Tooltip("Secondary motion's X axis curve.")]
        public AnimationCurve secondaryXCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Tooltip("Secondary motion's Y axis curve.")]
        public AnimationCurve secondaryYCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Tooltip("Secondary motion's Z axis curve.")]
        public AnimationCurve secondaryZCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Header("Preview")]
        [Tooltip("Not currently used - the Custom Profile editor keeps its own preview toggle instead of reading this field.")]
        public bool showPreview = false;

        [Tooltip("Playback speed multiplier used by the Custom Profile editor's Play Preview button.")]
        public float previewSpeed = 1f;

        [System.Serializable]
        public class MotionLayer
        {
            [Tooltip("Display name for this layer, shown in the Custom Profile editor's layer list.")]
            public string layerName = "New Layer";

            [Tooltip("This layer's X axis curve.")]
            public AnimationCurve curveX = AnimationCurve.EaseInOut(0, 0, 1, 0);

            [Tooltip("This layer's Y axis curve.")]
            public AnimationCurve curveY = AnimationCurve.EaseInOut(0, 0, 1, 0);

            [Tooltip("This layer's Z axis curve.")]
            public AnimationCurve curveZ = AnimationCurve.EaseInOut(0, 0, 1, 0);

            [Tooltip("How strongly this layer's curve values contribute to the final motion.")]
            public float intensity = 1f;

            [Tooltip("Playback speed of this layer relative to Curve Duration. 1 = same speed as the main curves, 2 = twice as fast.")]
            public float speed = 1f;

            [Tooltip("Time offset (in seconds) for this layer, so it doesn't start in perfect sync with the main curves.")]
            public float phaseOffset = 0f;

            [Tooltip("PLANNED, NOT YET FUNCTIONAL: intended to let this layer apply in world space instead of local space, but Evaluate() does not currently read this field.")]
            public bool useWorldSpace = false;
        }

        public Vector3 Evaluate(float time)
        {
            float normalizedTime = loopCurve ? time % curveDuration : Mathf.Clamp(time, 0, curveDuration);
            float t = normalizedTime / curveDuration;
            
            Vector3 result = new Vector3(
                xCurve.Evaluate(t),
                yCurve.Evaluate(t),
                zCurve.Evaluate(t)
            );
            
            foreach (var layer in layers)
            {
                float layerTime = (time * layer.speed + layer.phaseOffset) % curveDuration;
                float layerT = layerTime / curveDuration;
                
                result += new Vector3(
                    layer.curveX.Evaluate(layerT) * layer.intensity,
                    layer.curveY.Evaluate(layerT) * layer.intensity,
                    layer.curveZ.Evaluate(layerT) * layer.intensity
                );
            }
            
            if (useSecondaryMotion)
            {
                float secondaryTime = (time * 0.7f + 1.5f) % curveDuration;
                float secondaryT = secondaryTime / curveDuration;
                
                result += new Vector3(
                    secondaryXCurve.Evaluate(secondaryT) * secondaryIntensity,
                    secondaryYCurve.Evaluate(secondaryT) * secondaryIntensity,
                    secondaryZCurve.Evaluate(secondaryT) * secondaryIntensity
                );
            }
            
            return result;
        }
        
        public void ResetCurves()
        {
            xCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
            yCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
            zCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
            layers.Clear();
        }
    }
}
