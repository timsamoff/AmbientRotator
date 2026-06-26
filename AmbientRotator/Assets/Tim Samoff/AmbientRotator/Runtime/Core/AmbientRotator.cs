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
        [Header("Core Settings")]
        [SerializeField] private MotionProfile profile = MotionProfile.Gentle;
        [SerializeField] private float intensity = 1f;
        [SerializeField] private float speed = 1f;
        [SerializeField] private float phaseOffset = 0f;
        
        [Header("Movement Limits")]
        [SerializeField] private Vector3 maxAngle = new Vector3(15f, 15f, 15f);
        [SerializeField] private bool clampMovement = true;
        
        [Header("Custom Profile")]
        [SerializeField] private bool useCustomProfile = false;
        [SerializeField] private CustomMotionProfile customProfile;
        
        [Header("Profile Blending")]
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
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float maxSpeed = 100f;
        
        [Header("Events")]
        public UnityEvent<Quaternion> OnRotationChanged;
        public UnityEvent OnMotionComplete;
        public UnityEvent OnPause;
        public UnityEvent OnResume;
        
        // Private variables
        private Transform cachedTransform;
        private Quaternion initialRotation;
        private Quaternion targetRotation;
        private Quaternion currentVelocity;
        private Vector3 currentOffset;
        private float currentTime;
        private bool isPaused = false;
        private bool isPreviewing = false;
        private Coroutine motionCoroutine;
        
        // External influences
        private Vector3 externalForce;
        private float externalForceDecay = 0.95f;
        
        // Wind system reference
        private WindSystem windSystem;
        private bool windInitialized = false;
        
        // Properties
        public bool IsPaused => isPaused;
        public MotionProfile CurrentProfile => profile;
        public float CurrentIntensity => intensity;
        public Vector3 CurrentOffset => currentOffset;
        
        private void Awake()
        {
            cachedTransform = transform;
            initialRotation = useLocalRotation ? cachedTransform.localRotation : cachedTransform.rotation;
            phaseOffset = Random.Range(0f, 360f);
            
            if (windSystem == null)
            {
                windSystem = FindObjectOfType<WindSystem>();
                windInitialized = windSystem != null;
            }
        }
        
        private void Start()
        {
            if (autoStart)
            {
                StartMotion();
            }
        }
        
        private void OnEnable()
        {
            if (autoStart && motionCoroutine == null)
            {
                StartMotion();
            }
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
            if (motionCoroutine != null)
            {
                StopCoroutine(motionCoroutine);
            }
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
        }
        
        public void SetIntensity(float newIntensity)
        {
            intensity = Mathf.Clamp(newIntensity, 0f, 10f);
        }
        
        public void SetSpeed(float newSpeed)
        {
            speed = Mathf.Clamp(newSpeed, 0.1f, 10f);
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
        
        public void ApplyForce(Vector3 force)
        {
            externalForce += force;
            externalForce = Vector3.ClampMagnitude(externalForce, 45f);
        }
        
        public void ResetToInitial()
        {
            if (useLocalRotation)
                cachedTransform.localRotation = initialRotation;
            else
                cachedTransform.rotation = initialRotation;
                
            currentOffset = Vector3.zero;
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
        
        private IEnumerator MotionLoop()
        {
            while (true)
            {
                if (!isPaused)
                {
                    float time = useUnscaledTime ? Time.unscaledTime : Time.time;
                    currentTime = time * speed + phaseOffset;
                    
                    Vector3 targetOffset = CalculateMotion(currentTime);
                    
                    if (windInitialized && windSystem != null)
                    {
                        Vector3 windForce = windSystem.GetWindForce(cachedTransform.position);
                        targetOffset += windForce * 0.01f;
                    }
                    
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
                    {
                        targetOffset = Vector3.ClampMagnitude(targetOffset, maxAngle.magnitude);
                    }
                    
                    currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
                    
                    Quaternion targetQuat = initialRotation * Quaternion.Euler(currentOffset);
                    
                    if (useLocalRotation)
                        cachedTransform.localRotation = targetQuat;
                    else
                        cachedTransform.rotation = targetQuat;
                    
                    OnRotationChanged?.Invoke(targetQuat);
                }
                
                yield return new WaitForEndOfFrame();
            }
        }
        
        private Vector3 CalculateMotion(float time)
        {
            if (useCustomProfile && customProfile != null)
            {
                return CalculateCustomMotion(time);
            }
            
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
                        Mathf.Sin(time * 0.7f + 1.2f) * 1.5f +
                        Mathf.Sin(time * 1.3f + 0.3f) * 0.8f,
                        Mathf.Sin(time * 0.5f + 0.8f) * 1.0f +
                        Mathf.Cos(time * 1.1f + 1.7f) * 0.5f,
                        Mathf.Cos(time * 0.9f + 2.1f) * 1.2f +
                        Mathf.Sin(time * 0.7f + 0.9f) * 0.6f
                    ) * intensity;
                    
                case MotionProfile.Dynamic:
                    return new Vector3(
                        Mathf.Sin(time * 1.2f + 0.2f) * 3.0f +
                        Mathf.Sin(time * 2.3f + 1.5f) * 1.5f,
                        Mathf.Sin(time * 1.5f + 1.8f) * 2.5f +
                        Mathf.Cos(time * 1.9f + 0.7f) * 1.0f,
                        Mathf.Cos(time * 1.8f + 0.3f) * 2.8f +
                        Mathf.Sin(time * 1.4f + 2.2f) * 1.2f
                    ) * intensity * 1.2f;
                    
                case MotionProfile.Chaotic:
                    return new Vector3(
                        Mathf.PerlinNoise(time * 0.5f, time * 0.3f) * 4f - 2f,
                        Mathf.PerlinNoise(time * 0.7f + 1.5f, time * 0.4f + 0.8f) * 4f - 2f,
                        Mathf.PerlinNoise(time * 0.6f + 2.1f, time * 0.5f + 1.2f) * 4f - 2f
                    ) * intensity * 1.5f;
                    
                case MotionProfile.Custom:
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
                Vector3 secondary = secondaryProfile.Evaluate(time + 1.5f);
                result = Vector3.Lerp(result, secondary, blend * blendWeight);
            }
            
            return result;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(cachedTransform.position, maxAngle.magnitude * 0.1f);
                
                Vector3 pos = cachedTransform.position;
                Vector3 up = cachedTransform.up * maxAngle.magnitude * 0.1f;
                Vector3 right = cachedTransform.right * maxAngle.magnitude * 0.1f;
                Vector3 forward = cachedTransform.forward * maxAngle.magnitude * 0.1f;
                
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pos + up);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pos, pos + right);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(pos, pos + forward);
            }
        }
    }
}