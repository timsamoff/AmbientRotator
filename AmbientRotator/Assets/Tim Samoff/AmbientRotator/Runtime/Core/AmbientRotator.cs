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

    public enum InfluenceMode
    {
        AmbientOnly,      // Normal ambient motion only
        ExternalDriven,   // External forces override decisions
        Blended          // External forces blend with decisions
    }

    [RequireComponent(typeof(Transform))]
    [CanEditMultipleObjects]
    public class AmbientRotator : MonoBehaviour
    {
        [Header("Motion Profile")]
        [Tooltip("The motion style that defines the object's personality.")]
        [SerializeField] private MotionProfile profile = MotionProfile.Gentle;

        [Tooltip("How much the object moves. Higher values = more dramatic motion.")]
        [SerializeField, Range(0f, 10f)] private float intensity = 1f;

        [Tooltip("How fast the object moves. Higher values = faster motion.")]
        [SerializeField, Range(0.1f, 10f)] private float speed = 1f;

        [Tooltip("The starting point in the motion cycle. Use this to offset multiple objects.")]
        [SerializeField, Range(-360f, 360f)] private float phaseOffset = 0f;

        [Tooltip("Maximum rotation angle in degrees for each axis (X, Y, Z).")]
        [SerializeField] private Vector3 maxAngle = new Vector3(360f, 360f, 360f);

        [Tooltip("When enabled, prevents the object from rotating beyond the Max Angle limits.")]
        [SerializeField] private bool clampMovement = false;

        [Tooltip("How long the object commits to a direction before changing its mind.")]
        [SerializeField, Range(0.1f, 10f)] private float decisionDuration = 3f;

        [Tooltip("When enabled, the object fully completes its rotation before changing direction.")]
        [SerializeField] private bool completeRotation = true;

        [Header("Start Delay")]
        [Tooltip("Enable to add a delay before the object starts moving.")]
        [SerializeField] private bool useStartDelay = false;

        [Tooltip("The minimum delay in seconds before the object starts moving.")]
        [SerializeField, Range(0f, 10f)] private float startDelayMin = 0.5f;

        [Tooltip("The maximum delay in seconds before the object starts moving.")]
        [SerializeField, Range(0f, 10f)] private float startDelayMax = 2f;

        [Tooltip("When enabled, the start delay is randomly chosen between Min and Max.")]
        [SerializeField] private bool randomizeStartDelay = false;

        [Header("Custom Profiles")]
        [Tooltip("Enable to use a custom motion profile defined with Animation Curves.")]
        [SerializeField] private bool useCustomProfile = false;

        [Tooltip("The custom motion profile asset that defines the object's movement.")]
        [SerializeField] private CustomMotionProfile customProfile;

        [Tooltip("Enable to blend between two custom profiles for more complex motion.")]
        [SerializeField] private bool blendProfiles = false;

        [Tooltip("The secondary custom profile to blend with the primary one.")]
        [SerializeField] private CustomMotionProfile secondaryProfile;

        [Tooltip("How much the secondary profile influences the motion.")]
        [SerializeField, Range(0f, 1f)] private float blendWeight = 0.5f;

        [Tooltip("How fast the blend between profiles oscillates.")]
        [SerializeField, Range(0f, 10f)] private float blendSpeed = 0.5f;

        [Header("External Influence")]
        [Tooltip("How external forces interact with ambient motion.")]
        [SerializeField] private InfluenceMode influenceMode = InfluenceMode.Blended;

        [Tooltip("How quickly external offsets decay. Lower = snappier, Higher = smoother.")]
        [SerializeField, Range(0.01f, 0.99f)] private float externalDecay = 0.95f;

        [Tooltip("Maximum external offset magnitude. Prevents instability.")]
        [SerializeField, Range(0.1f, 90f)] private float maxExternalOffset = 45f;

        [Header("Smoothing")]
        [Tooltip("Which Unity update method to use.")]
        [SerializeField] private UpdateMethod updateMethod = UpdateMethod.Update;

        [Tooltip("When enabled, motion continues even when the game is paused.")]
        [SerializeField] private bool useUnscaledTime = false;

        [Tooltip("When enabled, the object starts moving automatically when the scene starts.")]
        [SerializeField] private bool autoStart = true;

        [Tooltip("When enabled, rotation is applied relative to the parent object.")]
        [SerializeField] private bool useLocalRotation = true;

        [Tooltip("How smoothly the object transitions between positions.")]
        [SerializeField, Range(0.01f, 1f)] private float smoothTime = 1f;

        [Tooltip("The maximum speed at which the object can move.")]
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

        // --- External Influence State ---
        private Vector3 externalPositionOffset;
        private Vector3 externalRotationOffset;
        private float externalSpeedMultiplier = 1f;
        private Vector3 positionVelocity;
        private Vector3 rotationVelocity;

        // --- Decision State ---
        private float currentDecisionTime = 0f;
        private Vector3 currentDecisionTarget;
        private Vector3 previousDecisionTarget;
        private float actualStartDelay = 0f;
        private bool hasStarted = false;

        // --- Wind System ---
        protected WindSystemModule windSystem;
        protected bool windInitialized = false;

        // --- Track if preset was just applied ---
        private bool isApplyingPreset = false;

        // --- Public Properties ---
        public bool IsPaused => isPaused;
        public MotionProfile CurrentProfile => profile;
        public float CurrentIntensity => intensity;
        public Vector3 CurrentOffset => currentOffset;
        public Vector3 ExternalPositionOffset => externalPositionOffset;
        public Vector3 ExternalRotationOffset => externalRotationOffset;
        public float ExternalSpeedMultiplier => externalSpeedMultiplier;
        
        public Vector3 MaxAngle { get => maxAngle; set => maxAngle = value; }
        public float SmoothTime { get => smoothTime; set => smoothTime = Mathf.Clamp(value, 0.01f, 1f); }
        public float Speed { get => speed; set => speed = Mathf.Clamp(value, 0.1f, 10f); }
        public float PhaseOffset { get => phaseOffset; set => phaseOffset = value; }
        public float DecisionDuration { get => decisionDuration; set => decisionDuration = Mathf.Max(0.1f, value); }
        public bool ClampMovement { get => clampMovement; set => clampMovement = value; }
        public float MaxSpeed { get => maxSpeed; set => maxSpeed = Mathf.Clamp(value, 0f, 1000f); }
        public InfluenceMode CurrentInfluenceMode { get => influenceMode; set => influenceMode = value; }

        // --- External API for Modules ---

        // Adds a position offset to the object. Will decay over time.
        public void AddPositionOffset(Vector3 offset)
        {
            externalPositionOffset += offset;
            externalPositionOffset = Vector3.ClampMagnitude(externalPositionOffset, maxExternalOffset);
        }

        // Adds a rotation offset to the object. Will decay over time.
        public void AddRotationOffset(Vector3 offset)
        {
            externalRotationOffset += offset;
            externalRotationOffset = Vector3.ClampMagnitude(externalRotationOffset, maxExternalOffset);
        }

        // Sets the speed multiplier for external influences.
        public void SetSpeedMultiplier(float multiplier)
        {
            externalSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 10f);
        }

        // Applies a force to the object (for backward compatibility).
        public void ApplyForce(Vector3 force)
        {
            externalPositionOffset += force * Time.deltaTime;
            externalPositionOffset = Vector3.ClampMagnitude(externalPositionOffset, maxExternalOffset);
        }

        // Resets all external influences to zero.
        public void ResetExternalInfluences()
        {
            externalPositionOffset = Vector3.zero;
            externalRotationOffset = Vector3.zero;
            externalSpeedMultiplier = 1f;
            positionVelocity = Vector3.zero;
            rotationVelocity = Vector3.zero;
        }

        // Gradually returns to the base state.
        public void ReturnToBase(float speed = 1f)
        {
            if (motionCoroutine != null)
                StartCoroutine(ReturnCoroutine(speed));
        }

        private IEnumerator ReturnCoroutine(float speed)
        {
            while (externalPositionOffset.magnitude > 0.01f || 
                   externalRotationOffset.magnitude > 0.01f ||
                   Mathf.Abs(externalSpeedMultiplier - 1f) > 0.01f)
            {
                externalPositionOffset = Vector3.Lerp(externalPositionOffset, Vector3.zero, Time.deltaTime * speed);
                externalRotationOffset = Vector3.Lerp(externalRotationOffset, Vector3.zero, Time.deltaTime * speed);
                externalSpeedMultiplier = Mathf.Lerp(externalSpeedMultiplier, 1f, Time.deltaTime * speed);
                yield return null;
            }
        }

        public void SetProfile(MotionProfile newProfile)
        {
            isApplyingPreset = true;
            profile = newProfile;
            ApplyPresetValues();
            isApplyingPreset = false;
        }

        public void SetIntensity(float newIntensity)
        {
            intensity = Mathf.Clamp(newIntensity, 0f, 10f);
            if (!isApplyingPreset && profile != MotionProfile.Custom)
            {
                profile = MotionProfile.Custom;
            }
        }
        
        public void SetSpeed(float newSpeed)
        {
            speed = Mathf.Clamp(newSpeed, 0.1f, 10f);
            if (!isApplyingPreset && profile != MotionProfile.Custom)
            {
                profile = MotionProfile.Custom;
            }
        }

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

        public void ResetToInitial()
        {
            if (useLocalRotation) cachedTransform.localRotation = initialRotation;
            else cachedTransform.rotation = initialRotation;
            currentOffset = Vector3.zero;
            ResetExternalInfluences();
            MakeNewDecision();
        }

        // --- Unity Lifecycle ---

        protected virtual void Awake()
        {
            cachedTransform = transform;
            initialRotation = useLocalRotation ? cachedTransform.localRotation : cachedTransform.rotation;
            
            // Randomize phase offset if not set
            if (phaseOffset == 0f)
                phaseOffset = Random.Range(0f, 360f);

            // Find wind system
            if (windSystem == null)
            {
                windSystem = FindAnyObjectByType<WindSystemModule>();
                windInitialized = windSystem != null;
            }

            // Only apply presets at startup if not Custom
            if (profile != MotionProfile.Custom)
            {
                ApplyPresetValues();
            }
            MakeNewDecision();
        }

        private void OnValidate()
        {
            maxAngle.x = Mathf.Clamp(maxAngle.x, 0f, 360f);
            maxAngle.y = Mathf.Clamp(maxAngle.y, 0f, 360f);
            maxAngle.z = Mathf.Clamp(maxAngle.z, 0f, 360f);

            if (startDelayMin < 0f) startDelayMin = 0f;
            if (startDelayMax < startDelayMin) startDelayMax = startDelayMin;
            maxSpeed = Mathf.Clamp(maxSpeed, 0f, 1000f);
            externalDecay = Mathf.Clamp(externalDecay, 0.01f, 0.99f);
            maxExternalOffset = Mathf.Clamp(maxExternalOffset, 0.1f, 90f);
        }

        public void ApplyPresetValues()
        {
            switch (profile)
            {
                case MotionProfile.Subtle:
                    intensity = 0.3f;
                    speed = 0.3f;
                    decisionDuration = 4f;
                    maxAngle = new Vector3(30f, 30f, 30f);
                    clampMovement = true;
                    smoothTime = 0.8f;
                    maxSpeed = 50f;
                    break;
                    
                case MotionProfile.Gentle:
                    intensity = 0.6f;
                    speed = 0.5f;
                    decisionDuration = 3f;
                    maxAngle = new Vector3(60f, 60f, 60f);
                    clampMovement = true;
                    smoothTime = 0.6f;
                    maxSpeed = 80f;
                    break;
                    
                case MotionProfile.Organic:
                    intensity = 1.0f;
                    speed = 0.7f;
                    decisionDuration = 2.5f;
                    maxAngle = new Vector3(90f, 90f, 90f);
                    clampMovement = true;
                    smoothTime = 0.4f;
                    maxSpeed = 120f;
                    break;
                    
                case MotionProfile.Dynamic:
                    intensity = 1.5f;
                    speed = 1.0f;
                    decisionDuration = 2f;
                    maxAngle = new Vector3(180f, 180f, 180f);
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

        // --- Internal Logic ---

        private void MakeNewDecision()
        {
            if (influenceMode == InfluenceMode.ExternalDriven && 
                externalPositionOffset.magnitude > 0.1f)
                return;

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
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float time = (useUnscaledTime ? Time.unscaledTime : Time.time) * speed + phaseOffset;

            // --- Calculate base ambient motion ---
            currentDecisionTime += deltaTime;
            if (currentDecisionTime >= decisionDuration) MakeNewDecision();

            float decisionProgress = Mathf.Clamp01(currentDecisionTime / decisionDuration);

            Vector3 targetOffset = completeRotation
                ? Vector3.Lerp(previousDecisionTarget, currentDecisionTarget, Mathf.SmoothStep(0f, 1f, decisionProgress))
                : CalculateMotion(time);

            // --- Apply external speed modulation ---
            float effectiveSpeed = speed * externalSpeedMultiplier;
            
            // Recalculate time with effective speed for smooth transitions
            float effectiveTime = (useUnscaledTime ? Time.unscaledTime : Time.time) * effectiveSpeed + phaseOffset;

            // --- Apply wind ---
            if (windInitialized && windSystem != null)
            {
                targetOffset += windSystem.GetWindForce(cachedTransform.position) * 0.01f;
            }

            // --- Apply external position offset ---
            targetOffset += externalPositionOffset;

            // --- Clamp if needed ---
            if (clampMovement)
                targetOffset = Vector3.ClampMagnitude(targetOffset, maxAngle.magnitude);

            // --- Smooth position ---
            float smoothSpeed = Mathf.Min(1f / smoothTime, maxSpeed);
            currentOffset = Vector3.SmoothDamp(
                currentOffset,
                targetOffset,
                ref positionVelocity,
                smoothTime,
                maxSpeed,
                deltaTime
            );

            // --- Decay external influences ---
            externalPositionOffset *= externalDecay;
            externalRotationOffset *= externalDecay;
            externalSpeedMultiplier = Mathf.Lerp(externalSpeedMultiplier, 1f, deltaTime * 5f);

            // Clamp to prevent instability
            externalPositionOffset = Vector3.ClampMagnitude(externalPositionOffset, maxExternalOffset);
            externalRotationOffset = Vector3.ClampMagnitude(externalRotationOffset, maxExternalOffset);

            // --- Compose final rotation ---
            Quaternion baseQuat = initialRotation * Quaternion.Euler(currentOffset);
            Quaternion externalQuat = Quaternion.Euler(externalRotationOffset);
            Quaternion targetQuat = baseQuat * externalQuat;

            // --- Apply rotation ---
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
            // Only draw gizmos if this specific object is selected
            // Prevents drawing for all objects when multiple are selected
            if (!UnityEditor.Selection.Contains(gameObject)) return;
            
            // Limit gizmo drawing to prevent graphics ring buffer overflow
            // Only draw detailed gizmos when few objects are selected
            int selectedCount = UnityEditor.Selection.gameObjects.Length;
            if (selectedCount > 5) 
            {
                // Draw simplified gizmos when many objects are selected
                DrawSimpleGizmos();
                return;
            }
            
            DrawDetailedGizmos();
        }

        private void DrawSimpleGizmos()
        {
            Transform t = gameObject.transform;
            if (t == null) return;
            
            Vector3 pos = t.position;
            float radius = CalculateGizmoRadius() * 0.5f;
            
            // Only draw wireframe arcs, no solid arcs (much cheaper)
            Handles.color = new Color(1f, 0f, 0f, 0.1f);
            Handles.DrawWireArc(pos, t.right, t.up, maxAngle.x, radius);
            Handles.DrawWireArc(pos, t.right, t.up, -maxAngle.x, radius);
            
            Handles.color = new Color(0f, 1f, 0f, 0.1f);
            Handles.DrawWireArc(pos, t.up, t.forward, maxAngle.y, radius);
            Handles.DrawWireArc(pos, t.up, t.forward, -maxAngle.y, radius);
            
            Handles.color = new Color(0f, 0.5f, 1f, 0.1f);
            Handles.DrawWireArc(pos, t.forward, t.up, maxAngle.z, radius);
            Handles.DrawWireArc(pos, t.forward, t.up, -maxAngle.z, radius);
            
            // Draw a small axis indicator
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pos, t.right * radius * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(pos, t.up * radius * 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pos, t.forward * radius * 0.5f);
        }

        private void DrawDetailedGizmos()
        {
            Transform t = gameObject.transform;
            if (t == null) return;
            
            Vector3 pos = t.position;
            float radius = CalculateGizmoRadius();
            
            // X-axis arcs
            Handles.color = new Color(1f, 0f, 0f, 0.05f);
            Handles.DrawSolidArc(pos, t.right, t.up, maxAngle.x, radius);
            Handles.DrawSolidArc(pos, t.right, t.up, -maxAngle.x, radius);
            
            // Y-axis arcs
            Handles.color = new Color(0f, 1f, 0f, 0.05f);
            Handles.DrawSolidArc(pos, t.up, t.forward, maxAngle.y, radius);
            Handles.DrawSolidArc(pos, t.up, t.forward, -maxAngle.y, radius);
            
            // Z-axis arcs
            Handles.color = new Color(0f, 0.5f, 1f, 0.05f);
            Handles.DrawSolidArc(pos, t.forward, t.up, maxAngle.z, radius);
            Handles.DrawSolidArc(pos, t.forward, t.up, -maxAngle.z, radius);
            
            // Wireframe arcs on top
            Handles.color = new Color(1f, 0f, 0f, 0.025f);
            Handles.DrawWireArc(pos, t.right, t.up, maxAngle.x, radius);
            Handles.DrawWireArc(pos, t.right, t.up, -maxAngle.x, radius);
            
            Handles.color = new Color(0f, 1f, 0f, 0.025f);
            Handles.DrawWireArc(pos, t.up, t.forward, maxAngle.y, radius);
            Handles.DrawWireArc(pos, t.up, t.forward, -maxAngle.y, radius);
            
            Handles.color = new Color(0f, 0.5f, 1f, 0.025f);
            Handles.DrawWireArc(pos, t.forward, t.up, maxAngle.z, radius);
            Handles.DrawWireArc(pos, t.forward, t.up, -maxAngle.z, radius);
        }

        private float CalculateGizmoRadius()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                float boundsSize = renderer.bounds.size.magnitude;
                return Mathf.Max(boundsSize * 0.5f, 0.1f);
            }
            
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                float meshSize = meshFilter.sharedMesh.bounds.size.magnitude;
                return Mathf.Max(meshSize * 1.5f * 0.8f, 0.1f);
            }
            
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                float colliderSize = collider.bounds.size.magnitude;
                return Mathf.Max(colliderSize * 1.5f, 0.1f);
            }
            
            float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            return Mathf.Max(scale * 1.5f, 0.1f);
        }
#endif
    }
}