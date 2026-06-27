using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AmbientRotator
{
    public enum MotionProfile
    {
        Subtle,
        Gentle,
        Organic,
        Dynamic,
        Chaotic,
        Custom
    }

    public enum UpdateMethod
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    [RequireComponent(typeof(Transform))]
    public class AmbientRotator : MonoBehaviour
    {
        [Tooltip("The motion style that defines the object's personality. Subtle = barely noticeable, Gentle = soft sway, Organic = natural and alive, Dynamic = bold and energetic, Chaotic = erratic and unpredictable, Custom = use your own defined profile.")]
        [SerializeField] private MotionProfile profile = MotionProfile.Gentle;

        [Tooltip("How much the object moves. Higher values = more dramatic motion. Start with 1.0 and adjust from there.")]
        [SerializeField, Range(0f, 10f)] private float intensity = 1f;

        [Tooltip("How fast the object moves. Higher values = faster motion. Start with 1.0 and adjust from there.")]
        [SerializeField, Range(0.1f, 10f)] private float speed = 1f;

        [Tooltip("The starting point in the motion cycle. Use this to offset multiple objects so they move out of sync with each other. Negative values shift the motion backward.")]
        [SerializeField, Range(-360f, 360f)] private float phaseOffset = 0f;

        [Tooltip("Maximum rotation angle in degrees for each axis (X, Y, Z). With 360, the object can complete full rotations. Each component is clamped between 0 and 360.")]
        [SerializeField] private Vector3 maxAngle = new Vector3(360f, 360f, 360f);

        [Tooltip("When enabled, prevents the object from rotating beyond the Max Angle limits. Turn off for full 360-degree rotation.")]
        [SerializeField] private bool clampMovement = false;

        [Tooltip("How long (in seconds) the object commits to a direction before changing its mind. Longer values = more deliberate, slower-changing motion.")]
        [SerializeField, Range(0.1f, 10f)] private float decisionDuration = 3f;

        [Tooltip("When enabled, the object fully completes its rotation before changing direction. When disabled, it changes direction mid-motion.")]
        [SerializeField] private bool completeRotation = true;

        [Tooltip("Enable to add a delay before the object starts moving. Useful for staggering multiple objects.")]
        [SerializeField] private bool useStartDelay = false;

        [Tooltip("The minimum delay in seconds before the object starts moving. Only used when Randomize Start Delay is enabled.")]
        [SerializeField, Range(0f, 10f)] private float startDelayMin = 0.5f;

        [Tooltip("The maximum delay in seconds before the object starts moving. Only used when Randomize Start Delay is enabled.")]
        [SerializeField, Range(0f, 10f)] private float startDelayMax = 2f;

        [Tooltip("When enabled, the start delay is randomly chosen between Min and Max. When disabled, the delay is exactly the Min value.")]
        [SerializeField] private bool randomizeStartDelay = false;

        [Tooltip("Enable to use a custom motion profile defined with Animation Curves for complete control over motion.")]
        [SerializeField] private bool useCustomProfile = false;

        [Tooltip("The custom motion profile asset that defines the object's movement. Create one via Assets > Create > Ambient Rotator > Custom Profile.")]
        [SerializeField] private CustomMotionProfile customProfile;

        [Tooltip("Enable to blend between two custom profiles for more complex motion patterns.")]
        [SerializeField] private bool blendProfiles = false;

        [Tooltip("The secondary custom profile to blend with the primary one.")]
        [SerializeField] private CustomMotionProfile secondaryProfile;

        [Tooltip("How much the secondary profile influences the motion. 0 = primary only, 0.5 = equal blend, 1 = secondary only.")]
        [SerializeField, Range(0f, 1f)] private float blendWeight = 0.5f;

        [Tooltip("How fast the blend between profiles oscillates. Higher values = faster switching between profiles.")]
        [SerializeField, Range(0f, 10f)] private float blendSpeed = 0.5f;

        [Tooltip("Which Unity update method to use. Update = normal frame update, FixedUpdate = physics update, LateUpdate = after all other updates.")]
        [SerializeField] private UpdateMethod updateMethod = UpdateMethod.Update;

        [Tooltip("When enabled, motion continues even when the game is paused. Useful for UI animations or menu backgrounds.")]
        [SerializeField] private bool useUnscaledTime = false;

        [Tooltip("When enabled, the object starts moving automatically when the scene starts. Disable to control motion manually via script.")]
        [SerializeField] private bool autoStart = true;

        [Tooltip("When enabled, rotation is applied relative to the parent object. When disabled, rotation is applied in world space.")]
        [SerializeField] private bool useLocalRotation = true;

        [Tooltip("How smoothly the object transitions between positions. Higher values = smoother, more fluid motion. Lower values = snappier, more responsive motion.")]
        [SerializeField, Range(0.01f, 1f)] private float smoothTime = 1f;

        [Tooltip("The maximum speed at which the object can move. Limits how fast the object can rotate. Higher values = faster motion.")]
        [SerializeField, Range(0f, 1000f)] private float maxSpeed = 100f;

        [Header("Events")]
        public UnityEvent<Quaternion> OnRotationChanged;
        public UnityEvent OnMotionComplete;
        public UnityEvent OnPause;
        public UnityEvent OnResume;

        // --- Runtime State ---
        protected Transform cachedTransform;
        protected Quaternion initialRotation;
        protected Vector3 currentOffset;
        protected float currentTime;
        protected bool isPaused = false;
        protected Coroutine motionCoroutine;

        protected Vector3 externalForce;
        protected float externalForceDecay = 0.95f;

        protected WindSystem windSystem;
        protected bool windInitialized = false;

        // --- Decision State ---
        private float currentDecisionTime = 0f;
        private Vector3 currentDecisionTarget;
        private Vector3 previousDecisionTarget;
        private float actualStartDelay = 0f;
        private bool hasStarted = false;

        // --- Public Properties ---
        public bool IsPaused => isPaused;
        public MotionProfile CurrentProfile => profile;
        public float CurrentIntensity => intensity;
        public Vector3 CurrentOffset => currentOffset;
        public Vector3 MaxAngle { get => maxAngle; set => maxAngle = value; }
        public float SmoothTime { get => smoothTime; set => smoothTime = Mathf.Clamp(value, 0.01f, 1f); }
        public float Speed { get => speed; set => speed = Mathf.Clamp(value, 0.1f, 10f); }
        public float PhaseOffset { get => phaseOffset; set => phaseOffset = value; }
        public float DecisionDuration { get => decisionDuration; set => decisionDuration = Mathf.Max(0.1f, value); }
        public bool ClampMovement { get => clampMovement; set => clampMovement = value; }
        public float MaxSpeed { get => maxSpeed; set => maxSpeed = Mathf.Clamp(value, 0f, 1000f); }

        public bool UseCustomProfile => useCustomProfile;
        public CustomMotionProfile CustomProfile => customProfile;
        public bool BlendProfiles => blendProfiles;
        public CustomMotionProfile SecondaryProfile => secondaryProfile;
        public float BlendWeight => blendWeight;
        public float BlendSpeed => blendSpeed;

        public void SetUseCustomProfile(bool value) => useCustomProfile = value;
        public void SetBlendProfiles(bool value) => blendProfiles = value;
        public void SetSecondaryProfile(CustomMotionProfile profile) => secondaryProfile = profile;
        public void SetBlendWeight(float value) => blendWeight = Mathf.Clamp01(value);
        public void SetBlendSpeed(float value) => blendSpeed = value;

        // --- Unity Lifecycle ---
        protected virtual void Awake()
        {
            cachedTransform = transform;
            initialRotation = useLocalRotation ? cachedTransform.localRotation : cachedTransform.rotation;
            phaseOffset = Random.Range(0f, 360f);

            if (windSystem == null)
            {
                windSystem = FindAnyObjectByType<WindSystem>();
                windInitialized = windSystem != null;
            }

            ApplyPresetValues();
            MakeNewDecision();
        }

        private void OnValidate()
        {
            // Clamp maxAngle components between 0 and 360
            maxAngle.x = Mathf.Clamp(maxAngle.x, 0f, 360f);
            maxAngle.y = Mathf.Clamp(maxAngle.y, 0f, 360f);
            maxAngle.z = Mathf.Clamp(maxAngle.z, 0f, 360f);

            if (startDelayMin < 0f) startDelayMin = 0f;
            if (startDelayMax < startDelayMin) startDelayMax = startDelayMin;
            maxSpeed = Mathf.Clamp(maxSpeed, 0f, 1000f);
        }

        public void ApplyPresetValues()
        {
            switch (profile)
            {
                case MotionProfile.Subtle:
                    intensity = 0.3f;
                    speed = 0.3f;
                    decisionDuration = 4f;
                    maxAngle = new Vector3(360f, 360f, 360f);
                    clampMovement = false;
                    smoothTime = 0.8f;
                    maxSpeed = 50f;
                    break;
                    
                case MotionProfile.Gentle:
                    intensity = 0.6f;
                    speed = 0.5f;
                    decisionDuration = 3f;
                    maxAngle = new Vector3(360f, 360f, 360f);
                    clampMovement = false;
                    smoothTime = 0.6f;
                    maxSpeed = 80f;
                    break;
                    
                case MotionProfile.Organic:
                    intensity = 1.0f;
                    speed = 0.7f;
                    decisionDuration = 2.5f;
                    maxAngle = new Vector3(360f, 360f, 360f);
                    clampMovement = false;
                    smoothTime = 0.4f;
                    maxSpeed = 120f;
                    break;
                    
                case MotionProfile.Dynamic:
                    intensity = 1.5f;
                    speed = 1.0f;
                    decisionDuration = 2f;
                    maxAngle = new Vector3(360f, 360f, 360f);
                    clampMovement = false;
                    smoothTime = 0.3f;
                    maxSpeed = 150f;
                    break;
                    
                case MotionProfile.Chaotic:
                    intensity = 2.0f;
                    speed = 1.5f;
                    decisionDuration = 1.5f;
                    maxAngle = new Vector3(360f, 360f, 360f);
                    clampMovement = false;
                    smoothTime = 0.2f;
                    maxSpeed = 200f;
                    break;
                    
                case MotionProfile.Custom:
                default:
                    break;
            }
        }

        private void Start()
        {
            actualStartDelay = useStartDelay
                ? (randomizeStartDelay ? Random.Range(startDelayMin, startDelayMax) : startDelayMin)
                : 0f;

            if (autoStart) StartMotion();
        }

        private void OnEnable()
        {
            if (autoStart && motionCoroutine == null) StartMotion();
        }

        private void OnDisable()
        {
            if (motionCoroutine != null)
            {
                StopCoroutine(motionCoroutine);
                motionCoroutine = null;
            }
        }

        // --- Public API ---
        public void StartMotion()
        {
            if (motionCoroutine != null) StopCoroutine(motionCoroutine);
            motionCoroutine = StartCoroutine(MotionLoop());
        }

        public void StopMotion()
        {
            if (motionCoroutine != null)
            {
                StopCoroutine(motionCoroutine);
                motionCoroutine = null;
            }
        }

        public void PauseMotion()
        {
            if (!isPaused)
            {
                isPaused = true;
                OnPause?.Invoke();
            }
        }

        public void ResumeMotion()
        {
            if (isPaused)
            {
                isPaused = false;
                OnResume?.Invoke();
            }
        }

        public void SetProfile(MotionProfile newProfile)
        {
            profile = newProfile;
            ApplyPresetValues();
        }

        public void SetIntensity(float newIntensity) => intensity = Mathf.Clamp(newIntensity, 0f, 10f);
        public void SetSpeed(float newSpeed) => speed = Mathf.Clamp(newSpeed, 0.1f, 10f);

        public void SetCustomProfile(CustomMotionProfile profile)
        {
            customProfile = profile;
            useCustomProfile = profile != null;
        }

        public void SetProfileBlending(CustomMotionProfile profile, float weight)
        {
            secondaryProfile = profile;
            blendWeight = Mathf.Clamp01(weight);
            blendProfiles = profile != null && weight > 0;
        }

        public void ApplyForce(Vector3 force)
        {
            externalForce += force;
            externalForce = Vector3.ClampMagnitude(externalForce, 45f);
        }

        public void ResetToInitial()
        {
            if (useLocalRotation) cachedTransform.localRotation = initialRotation;
            else cachedTransform.rotation = initialRotation;
            currentOffset = Vector3.zero;
            MakeNewDecision();
        }

        // --- Internal Logic ---
        private void MakeNewDecision()
        {
            Vector3 newTarget = new Vector3(
                Random.Range(-maxAngle.x, maxAngle.x),
                Random.Range(-maxAngle.y, maxAngle.y),
                Random.Range(-maxAngle.z, maxAngle.z)
            );
            previousDecisionTarget = currentDecisionTarget;
            currentDecisionTarget = newTarget;
            currentDecisionTime = 0f;
        }

        private IEnumerator MotionLoop()
        {
            if (actualStartDelay > 0f) yield return new WaitForSeconds(actualStartDelay);
            hasStarted = true;

            while (true)
            {
                if (!isPaused && hasStarted && cachedTransform != null)
                {
                    UpdateMotion();
                    if (updateMethod == UpdateMethod.FixedUpdate)
                        yield return new WaitForFixedUpdate();
                    else
                        yield return new WaitForEndOfFrame();
                }
                else
                {
                    yield return null;
                }
            }
        }

        protected virtual void UpdateMotion()
        {
            currentDecisionTime += Time.deltaTime;
            if (currentDecisionTime >= decisionDuration) MakeNewDecision();

            float time = (useUnscaledTime ? Time.unscaledTime : Time.time) * speed + phaseOffset;
            float decisionProgress = Mathf.Clamp01(currentDecisionTime / decisionDuration);

            Vector3 targetOffset = completeRotation
                ? Vector3.Lerp(previousDecisionTarget, currentDecisionTarget, Mathf.SmoothStep(0f, 1f, decisionProgress))
                : CalculateMotion(time);

            if (windInitialized && windSystem != null)
                targetOffset += windSystem.GetWindForce(cachedTransform.position) * 0.01f;

            if (externalForce.magnitude > 0.01f)
            {
                targetOffset += externalForce * Time.deltaTime;
                externalForce *= externalForceDecay;
            }
            else
            {
                externalForce = Vector3.zero;
            }

            if (clampMovement)
                targetOffset = Vector3.ClampMagnitude(targetOffset, maxAngle.magnitude);

            float smoothSpeed = Mathf.Min(1f / smoothTime, maxSpeed);
            currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * smoothSpeed);

            Quaternion targetQuat = initialRotation * Quaternion.Euler(currentOffset);
            if (useLocalRotation)
                cachedTransform.localRotation = targetQuat;
            else
                cachedTransform.rotation = targetQuat;

            OnRotationChanged?.Invoke(targetQuat);
        }

        protected Vector3 CalculateMotion(float time)
        {
            if (useCustomProfile && customProfile != null)
                return CalculateCustomMotion(time);

            switch (profile)
            {
                case MotionProfile.Subtle:
                    return new Vector3(
                        Mathf.Sin(time * 0.3f + 1.2f) * 0.5f,
                        Mathf.Sin(time * 0.4f + 0.8f) * 0.3f,
                        Mathf.Cos(time * 0.35f + 2.1f) * 0.4f
                    ) * intensity * 0.5f;

                case MotionProfile.Gentle:
                    return new Vector3(
                        Mathf.Sin(time * 0.5f + 0.7f) * 1.2f,
                        Mathf.Sin(time * 0.6f + 1.3f) * 0.8f,
                        Mathf.Cos(time * 0.45f + 0.5f) * 1.0f
                    ) * intensity * 0.8f;

                case MotionProfile.Organic:
                    return new Vector3(
                        Mathf.Sin(time * 0.7f + 1.2f) * 1.5f + Mathf.Sin(time * 1.3f + 0.3f) * 0.8f,
                        Mathf.Sin(time * 0.5f + 0.8f) * 1.0f + Mathf.Cos(time * 1.1f + 1.7f) * 0.5f,
                        Mathf.Cos(time * 0.9f + 2.1f) * 1.2f + Mathf.Sin(time * 0.7f + 0.9f) * 0.6f
                    ) * intensity;

                case MotionProfile.Dynamic:
                    return new Vector3(
                        Mathf.Sin(time * 1.2f + 0.2f) * 3.0f + Mathf.Sin(time * 2.3f + 1.5f) * 1.5f,
                        Mathf.Sin(time * 1.5f + 1.8f) * 2.5f + Mathf.Cos(time * 1.9f + 0.7f) * 1.0f,
                        Mathf.Cos(time * 1.8f + 0.3f) * 2.8f + Mathf.Sin(time * 1.4f + 2.2f) * 1.2f
                    ) * intensity * 1.2f;

                case MotionProfile.Chaotic:
                    return new Vector3(
                        Mathf.PerlinNoise(time * 0.5f, time * 0.3f) * 4f - 2f,
                        Mathf.PerlinNoise(time * 0.7f + 1.5f, time * 0.4f + 0.8f) * 4f - 2f,
                        Mathf.PerlinNoise(time * 0.6f + 2.1f, time * 0.5f + 1.2f) * 4f - 2f
                    ) * intensity * 1.5f;

                default:
                    return Vector3.zero;
            }
        }

        private Vector3 CalculateCustomMotion(float time)
        {
            Vector3 result = customProfile.Evaluate(time);
            if (blendProfiles && secondaryProfile != null)
            {
                float blend = Mathf.PingPong(Time.time * blendSpeed, 1f);
                result = Vector3.Lerp(result, secondaryProfile.Evaluate(time + 1.5f), blend * blendWeight);
            }
            return result;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Use Handles in OnDrawGizmos (it works!)
            Transform t = gameObject.transform;
            if (t == null) return;
            
            Vector3 pos = t.position;
            
            float baseRadius = 0.8f;
            float scale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
            float radius = baseRadius * scale * 2.5f;
            
            // --- Solid translucent discs using Handles ---
            Handles.color = new Color(1f, 0f, 0f, 0.025f);
            Handles.DrawSolidArc(pos, t.right, t.up, maxAngle.x, radius);
            Handles.DrawSolidArc(pos, t.right, t.up, -maxAngle.x, radius);
            
            Handles.color = new Color(0f, 1f, 0f, 0.025f);
            Handles.DrawSolidArc(pos, t.up, t.forward, maxAngle.y, radius);
            Handles.DrawSolidArc(pos, t.up, t.forward, -maxAngle.y, radius);
            
            Handles.color = new Color(0f, 0.5f, 1f, 0.025f);
            Handles.DrawSolidArc(pos, t.forward, t.up, maxAngle.z, radius);
            Handles.DrawSolidArc(pos, t.forward, t.up, -maxAngle.z, radius);
            
            // --- Wireframe arcs on top ---
            Handles.color = new Color(1f, 0f, 0f, 0.05f);
            Handles.DrawWireArc(pos, t.right, t.up, maxAngle.x, radius);
            Handles.DrawWireArc(pos, t.right, t.up, -maxAngle.x, radius);
            
            Handles.color = new Color(0f, 1f, 0f, 0.05f);
            Handles.DrawWireArc(pos, t.up, t.forward, maxAngle.y, radius);
            Handles.DrawWireArc(pos, t.up, t.forward, -maxAngle.y, radius);
            
            Handles.color = new Color(0f, 0.5f, 1f, 0.05f);
            Handles.DrawWireArc(pos, t.forward, t.up, maxAngle.z, radius);
            Handles.DrawWireArc(pos, t.forward, t.up, -maxAngle.z, radius);
        }
        #endif
    }
}