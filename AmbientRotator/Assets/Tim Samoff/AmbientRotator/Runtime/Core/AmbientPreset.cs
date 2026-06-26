using UnityEngine;

namespace AmbientRotator
{
    [CreateAssetMenu(fileName = "NewAmbientPreset", menuName = "Ambient Rotator/Preset")]
    public class AmbientPreset : ScriptableObject
    {
        [Header("Motion Profile")]
        public MotionProfile profile = MotionProfile.Gentle;
        [Range(0f, 5f)]
        public float intensity = 1f;
        [Range(0.1f, 5f)]
        public float speed = 1f;
        
        [Header("Limits")]
        public Vector3 maxAngle = new Vector3(15f, 15f, 15f);
        public bool clampMovement = true;
        
        [Header("Smoothing")]
        [Range(0.01f, 1f)]
        public float smoothTime = 0.1f;
        [Range(1f, 100f)]
        public float maxSpeed = 100f;
        
        [Header("External Influences")]
        public bool responsiveToWind = true;
        [Range(0f, 2f)]
        public float windMultiplier = 1f;
        public bool responsiveToPlayer = false;
        [Range(0f, 10f)]
        public float playerInfluenceRadius = 5f;
        
        [Header("Visual")]
        public Color gizmoColor = Color.cyan;
        public bool showMotionPreview = true;
        
        [Header("Advanced")]
        public bool useUnscaledTime = false;
        public UpdateMethod updateMethod = UpdateMethod.Update;
        
        public void ApplyTo(AmbientRotator target)
        {
            if (target == null) return;
            
            target.SetProfile(profile);
            target.SetIntensity(intensity);
            target.SetSpeed(speed);
        }
        
        public void ApplyToGroup(AmbientGroup group)
        {
            if (group == null) return;
            
            group.SetAllProfiles(profile);
            group.SetMasterIntensity(intensity);
            group.SetMasterSpeed(speed);
        }
    }
}