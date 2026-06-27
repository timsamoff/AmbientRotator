using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
        [Header("Quick Presets")]
        [SerializeField] private MotionProfile profile = MotionProfile.Gentle;

        [Header("Core Settings")]
        [SerializeField] private float intensity = 1f;
        [SerializeField] private float speed = 1f;
        [SerializeField] private float phaseOffset = 0f;

        [Header("Movement Limits")]
        [SerializeField] private Vector3 maxAngle = new Vector3(360f, 360f, 360f);
        [SerializeField] private bool clampMovement = false;

        [Header("Decision Making")]
        [SerializeField] private float decisionDuration = 3f;
        [SerializeField] private bool completeRotation = true;

        [Header("Start Delay")]
        [SerializeField] private bool useStartDelay = false;
        [SerializeField] private float startDelayMin = 0.5f;
        [SerializeField] private float startDelayMax = 2f;
        [SerializeField] private bool randomizeStartDelay = false;

        [Header("Custom Profile")]
        [SerializeField] private bool useCustomProfile = false;
        [SerializeField] private CustomMotionProfile customProfile;
        [SerializeField] private bool blendProfiles = false;
        [SerializeField] private CustomMotionProfile secondaryProfile;
        [SerializeField, Range(0f, 1f)] private float blendWeight = 0.5f;
        [SerializeField] private float blendSpeed = 0.5f;

        [Header("Advanced")]
        [SerializeField] private UpdateMethod updateMethod = UpdateMethod.Update;
        [SerializeField] private bool useUnscaledTime = false;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool useLocalRotation = true;

        [Header("Smoothing")]
        [SerializeField, Range(0.01f, 1f)] private float smoothTime = 1f;
        [SerializeField] private float maxSpeed = 100f;

        [Header("Events")]
        public UnityEvent<Quaternion> OnRotationChanged;
        public UnityEvent OnMotionComplete;
        public UnityEvent OnPause;
        public UnityEvent OnResume;

        protected Transform cachedTransform;
        protected Quaternion initialRotation;
        protected Quaternion targetRotation;
        protected Quaternion currentVelocity;
        protected Vector3 currentOffset;
        protected float currentTime;
        protected bool isPaused = false;
        protected bool isPreviewing = false;
        protected Coroutine motionCoroutine;

        protected Vector3 externalForce;
        protected float externalForceDecay = 0.95f;

        protected WindSystem windSystem;
        protected bool windInitialized = false;

        private float currentDecisionTime = 0f;
        private Vector3 currentDecisionTarget;
        private Vector3 previousDecisionTarget;
        private float actualStartDelay = 0f;
        private bool hasStarted = false;

        public bool UseCustomProfile => useCustomProfile;
        public CustomMotionProfile CustomProfile => customProfile;
        public bool BlendProfiles => blendProfiles;
        public CustomMotionProfile SecondaryProfile => secondaryProfile;
        public float BlendWeight => blendWeight;
        public float BlendSpeed => blendSpeed;

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

        public void SetUseCustomProfile(bool value) => useCustomProfile = value;
        public void SetBlendProfiles(bool value) => blendProfiles = value;
        public void SetSecondaryProfile(CustomMotionProfile profile) => secondaryProfile = profile;
        public void SetBlendWeight(float value) => blendWeight = Mathf.Clamp01(value);
        public void SetBlendSpeed(float value) => blendSpeed = value;

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
            ApplyPresetValues();
            if (startDelayMin < 0f) startDelayMin = 0f;
            if (startDelayMax < startDelayMin) startDelayMax = startDelayMin;
        }

        private void ApplyPresetValues()
        {
            switch (profile)
            {
                case MotionProfile.Subtle:
                    intensity = 0.3f;
                    speed = 0.3f;
                    decisionDuration = 4f;
                    break;
                case MotionProfile.Gentle:
                    intensity = 0.6f;
                    speed = 0.5f;
                    decisionDuration = 3f;
                    break;
                case MotionProfile.Organic:
                    intensity = 1.0f;
                    speed = 0.7f;
                    decisionDuration = 2.5f;
                    break;
                case MotionProfile.Dynamic:
                    intensity = 1.5f;
                    speed = 1.0f;
                    decisionDuration = 2f;
                    break;
                case MotionProfile.Chaotic:
                    intensity = 2.0f;
                    speed = 1.5f;
                    decisionDuration = 1.5f;
                    break;
            }
            maxAngle = new Vector3(360f, 360f, 360f);
            clampMovement = false;
            smoothTime = 1f;
        }

        private void Start()
        {
            actualStartDelay = useStartDelay ? (randomizeStartDelay ? Random.Range(startDelayMin, startDelayMax) : startDelayMin) : 0f;
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

        public void StartPreview()
        {
            isPreviewing = true;
            StartMotion();
        }

        public void EndPreview()
        {
            isPreviewing = false;
            ResetToInitial();
        }

        private void MakeNewDecision()
        {
            Vector3 newTarget = new Vector3(Random.Range(-maxAngle.x, maxAngle.x), Random.Range(-maxAngle.y, maxAngle.y), Random.Range(-maxAngle.z, maxAngle.z));
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
                if (!isPaused && hasStarted)
                {
                    UpdateMotion();
                    if (updateMethod == UpdateMethod.FixedUpdate) yield return new WaitForFixedUpdate();
                    else yield return new WaitForEndOfFrame();
                }
                else yield return null;
            }
        }

        protected virtual void UpdateMotion()
        {
            currentDecisionTime += Time.deltaTime;
            if (currentDecisionTime >= decisionDuration) MakeNewDecision();
            float time = (useUnscaledTime ? Time.unscaledTime : Time.time) * speed + phaseOffset;
            float decisionProgress = Mathf.Clamp01(currentDecisionTime / decisionDuration);
            Vector3 targetOffset = completeRotation ? Vector3.Lerp(previousDecisionTarget, currentDecisionTarget, Mathf.SmoothStep(0f, 1f, decisionProgress)) : CalculateMotion(time);
            
            if (windInitialized && windSystem != null) targetOffset += windSystem.GetWindForce(cachedTransform.position) * 0.01f;
            if (externalForce.magnitude > 0.01f)
            {
                targetOffset += externalForce * Time.deltaTime;
                externalForce *= externalForceDecay;
            }
            else externalForce = Vector3.zero;

            if (clampMovement) targetOffset = Vector3.ClampMagnitude(targetOffset, maxAngle.magnitude);
            float smoothSpeed = Mathf.Min(1f / smoothTime, maxSpeed);
            currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * smoothSpeed);
            Quaternion targetQuat = initialRotation * Quaternion.Euler(currentOffset);
            if (useLocalRotation) cachedTransform.localRotation = targetQuat;
            else cachedTransform.rotation = targetQuat;
            OnRotationChanged?.Invoke(targetQuat);
        }

        protected Vector3 CalculateMotion(float time)
        {
            if (useCustomProfile && customProfile != null) return CalculateCustomMotion(time);
            switch (profile)
            {
                case MotionProfile.Subtle: return new Vector3(Mathf.Sin(time * 0.3f + 1.2f) * 0.5f, Mathf.Sin(time * 0.4f + 0.8f) * 0.3f, Mathf.Cos(time * 0.35f + 2.1f) * 0.4f) * intensity * 0.5f;
                case MotionProfile.Gentle: return new Vector3(Mathf.Sin(time * 0.5f + 0.7f) * 1.2f, Mathf.Sin(time * 0.6f + 1.3f) * 0.8f, Mathf.Cos(time * 0.45f + 0.5f) * 1.0f) * intensity * 0.8f;
                case MotionProfile.Organic: return new Vector3(Mathf.Sin(time * 0.7f + 1.2f) * 1.5f + Mathf.Sin(time * 1.3f + 0.3f) * 0.8f, Mathf.Sin(time * 0.5f + 0.8f) * 1.0f + Mathf.Cos(time * 1.1f + 1.7f) * 0.5f, Mathf.Cos(time * 0.9f + 2.1f) * 1.2f + Mathf.Sin(time * 0.7f + 0.9f) * 0.6f) * intensity;
                case MotionProfile.Dynamic: return new Vector3(Mathf.Sin(time * 1.2f + 0.2f) * 3.0f + Mathf.Sin(time * 2.3f + 1.5f) * 1.5f, Mathf.Sin(time * 1.5f + 1.8f) * 2.5f + Mathf.Cos(time * 1.9f + 0.7f) * 1.0f, Mathf.Cos(time * 1.8f + 0.3f) * 2.8f + Mathf.Sin(time * 1.4f + 2.2f) * 1.2f) * intensity * 1.2f;
                case MotionProfile.Chaotic: return new Vector3(Mathf.PerlinNoise(time * 0.5f, time * 0.3f) * 4f - 2f, Mathf.PerlinNoise(time * 0.7f + 1.5f, time * 0.4f + 0.8f) * 4f - 2f, Mathf.PerlinNoise(time * 0.6f + 2.1f, time * 0.5f + 1.2f) * 4f - 2f) * intensity * 1.5f;
                default: return Vector3.zero;
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

        private void OnDrawGizmosSelected()
        {
            if (cachedTransform == null) return;
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(cachedTransform.position, maxAngle.magnitude * 0.1f);
            }
        }
    }
}