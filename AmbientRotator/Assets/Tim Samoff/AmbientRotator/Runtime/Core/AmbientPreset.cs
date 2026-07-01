using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// A reusable set of AmbientRotator settings, saved as a project asset.
    /// Create via Assets > Create > Ambient Rotator > Preset, then call ApplyTo()/ApplyToGroup()
    /// from your own code to stamp these settings onto a live AmbientRotator or AmbientGroup.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAmbientPreset", menuName = "Ambient Rotator/Preset")]
    public class AmbientPreset : ScriptableObject
    {
        [Header("Motion Profile")]
        [Tooltip("The motion style to apply. See AmbientRotator's Motion Profile for what each style looks like.")]
        public MotionProfile profile = MotionProfile.Gentle;

        [Tooltip("How much the object moves. Higher values = more dramatic motion.")]
        [Range(0f, 5f)]
        public float intensity = 1f;

        [Tooltip("How fast the object moves. Higher values = faster motion.")]
        [Range(0.1f, 5f)]
        public float speed = 1f;

        [Header("Limits")]
        [Tooltip("Maximum rotation angle in degrees for each axis (X, Y, Z).")]
        public Vector3 maxAngle = new Vector3(15f, 15f, 15f);

        [Tooltip("When enabled, prevents the object from rotating beyond Max Angle.")]
        public bool clampMovement = true;

        [Header("Smoothing")]
        [Tooltip("How smoothly the object transitions to new motion targets. Lower = snappier, higher = smoother/heavier.")]
        [Range(0.01f, 1f)]
        public float smoothTime = 0.1f;

        [Tooltip("Maximum speed used while smoothing. Caps how fast the object can catch up to a target.")]
        [Range(1f, 100f)]
        public float maxSpeed = 100f;

        [Header("External Influences")]
        [Tooltip("PLANNED, NOT YET FUNCTIONAL: intended to toggle whether this preset's objects react to wind, but AmbientRotator has no per-object wind on/off switch yet - wind is currently controlled entirely by whether a WindSystemModule exists in the scene. This field is not applied by ApplyTo().")]
        public bool responsiveToWind = true;

        [Tooltip("PLANNED, NOT YET FUNCTIONAL: intended to scale how strongly wind affects this preset's objects, but AmbientRotator has no per-object wind multiplier yet. This field is not applied by ApplyTo().")]
        [Range(0f, 2f)]
        public float windMultiplier = 1f;

        [Tooltip("PLANNED, NOT YET FUNCTIONAL: intended for a player-proximity reaction system, but no such system exists yet - use Reactive Trigger Object/Module for proximity reactions instead. This field is not applied by ApplyTo().")]
        public bool responsiveToPlayer = false;

        [Tooltip("PLANNED, NOT YET FUNCTIONAL: see Responsive To Player above. This field is not applied by ApplyTo().")]
        [Range(0f, 10f)]
        public float playerInfluenceRadius = 5f;

        [Header("Visual")]
        [Tooltip("Reference color for this preset, shown in the Preset Browser. Purely organizational - has no effect on the object's actual gizmos or appearance.")]
        public Color gizmoColor = Color.cyan;

        [Tooltip("Reserved for a future motion preview feature in the Preset Browser. Not currently used.")]
        public bool showMotionPreview = true;

        [Header("Advanced")]
        [Tooltip("When enabled, motion ignores Time.timeScale (keeps animating while the game is paused).")]
        public bool useUnscaledTime = false;

        [Tooltip("Which Unity update loop drives the motion. Update = standard (most objects). FixedUpdate = physics-synced. LateUpdate = after cameras/animation, useful to avoid one-frame lag with camera-follow rigs.")]
        public UpdateMethod updateMethod = UpdateMethod.Update;

        /// <summary>Applies every field this preset can actually control to a live AmbientRotator.</summary>
        public void ApplyTo(AmbientRotator target)
        {
            if (target == null) return;

            target.SetProfile(profile);
            target.SetIntensity(intensity);
            target.SetSpeed(speed);
            target.MaxAngle = maxAngle;
            target.ClampMovement = clampMovement;
            target.SmoothTime = smoothTime;
            target.MaxSpeed = maxSpeed;
            target.UseUnscaledTime = useUnscaledTime;
            target.CurrentUpdateMethod = updateMethod;
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
