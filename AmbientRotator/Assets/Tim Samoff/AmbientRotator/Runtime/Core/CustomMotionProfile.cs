using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    [CreateAssetMenu(fileName = "NewCustomProfile", menuName = "Ambient Rotator/Custom Profile")]
    public class CustomMotionProfile : ScriptableObject
    {
        [Header("Motion Curves")]
        public AnimationCurve xCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        public AnimationCurve yCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        public AnimationCurve zCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        
        [Header("Curve Settings")]
        public float curveDuration = 2f;
        public bool loopCurve = true;
        public bool randomizeStart = true;
        
        [Header("Layered Motion")]
        public List<MotionLayer> layers = new List<MotionLayer>();
        
        [Header("Secondary Motion")]
        public bool useSecondaryMotion = false;
        public float secondaryIntensity = 0.5f;
        public AnimationCurve secondaryXCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        public AnimationCurve secondaryYCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        public AnimationCurve secondaryZCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        
        [Header("Preview")]
        public bool showPreview = false;
        public float previewSpeed = 1f;
        
        [System.Serializable]
        public class MotionLayer
        {
            public string layerName = "New Layer";
            public AnimationCurve curveX = AnimationCurve.EaseInOut(0, 0, 1, 0);
            public AnimationCurve curveY = AnimationCurve.EaseInOut(0, 0, 1, 0);
            public AnimationCurve curveZ = AnimationCurve.EaseInOut(0, 0, 1, 0);
            public float intensity = 1f;
            public float speed = 1f;
            public float phaseOffset = 0f;
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