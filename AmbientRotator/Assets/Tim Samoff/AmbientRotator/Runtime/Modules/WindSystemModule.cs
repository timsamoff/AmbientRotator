using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    /// <summary>
    /// Place ONE of these in your scene. It manages ALL wind-affected objects.
    /// Automatically finds and optimizes grass blades, leaves, etc.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Wind System Module")]
    public class WindSystemModule : MonoBehaviour
    {
        [Header("Wind Settings")]
        [Tooltip("Base strength of the wind. 0 = no wind, 0.3 = gentle breeze, 0.5 = moderate wind, 1.0 = strong wind, 2.0+ = storm force.")]
        [SerializeField, Range(0f, 5f)] private float baseStrength = 0.3f;
        
        [Tooltip("How often wind gusts occur. 0.5 = slow gusts, 2 = moderate gusts, 5+ = rapid gusts.")]
        [SerializeField, Range(0.1f, 10f)] private float gustFrequency = 2f;
        
        [Tooltip("How strong the gusts are. 0 = no gusts, 0.2 = gentle gusts, 0.5 = strong gusts, 1.0+ = extreme gusts.")]
        [SerializeField, Range(0f, 2f)] private float gustAmplitude = 0.2f;
        
        [Header("Wind Direction")]
        [Tooltip("The direction the wind blows. Use the individual axis sliders below.")]
        [SerializeField] private Vector3 baseDirection = Vector3.right;
        
        [Header("Direction Axis Controls")]
        [Tooltip("Left (-1) to Right (1). 0 = no wind on X axis.")]
        [SerializeField, Range(-1f, 1f)] private float directionX = 1f;
        
        [Tooltip("Down (-1) to Up (1). 0 = no wind on Y axis.")]
        [SerializeField, Range(-1f, 1f)] private float directionY = 0f;
        
        [Tooltip("Back (-1) to Forward (1). 0 = no wind on Z axis.")]
        [SerializeField, Range(-1f, 1f)] private float directionZ = 0f;
        
        [Header("Turbulence")]
        [Tooltip("How much random variation the wind has. 0 = smooth wind, 0.2 = some variation, 0.5 = chaotic wind.")]
        [SerializeField, Range(0f, 1f)] private float turbulence = 0.2f;
        
        [Tooltip("How quickly the turbulence changes. 0.1 = slow changes, 0.5 = moderate, 2+ = rapid changes.")]
        [SerializeField, Range(0.1f, 5f)] private float turbulenceFrequency = 0.5f;
        
        [Tooltip("How much the turbulence affects direction. 0 = no directional change, 0.05 = subtle, 0.2 = significant.")]
        [SerializeField, Range(0f, 0.5f)] private float turbulenceScale = 0.05f;

        [Header("Performance")]
        [Tooltip("Maximum number of objects to process per frame. Lower = better performance but more lag. Higher = smoother but more CPU usage.")]
        [SerializeField, Range(10, 200)] private int maxProcessPerFrame = 50;
        
        [Tooltip("Automatically find all wind-affected objects in the scene on startup.")]
        [SerializeField] private bool autoFindObjects = true;
        
        [Tooltip("Objects beyond this distance from the camera won't be updated (saves performance). 0 = unlimited distance.")]
        [SerializeField, Range(0f, 500f)] private float maxUpdateDistance = 100f;

        // --- Data ---
        private static WindSystemModule instance;
        private List<WindAffectedData> objects = new List<WindAffectedData>();
        private int currentIndex = 0;
        private float timeOffset;
        
        // Wind state
        private float currentStrength;
        private Vector3 currentDirection;
        private Camera mainCamera;
        
        // Cached transforms for performance
        private List<Transform> transforms = new List<Transform>();
        private List<Vector3> originalPositions = new List<Vector3>();
        private List<Quaternion> originalRotations = new List<Quaternion>();
        private List<float> sensitivities = new List<float>();
        private List<float> multipliers = new List<float>();
        private List<float> randomOffsets = new List<float>();

        private struct WindAffectedData
        {
            public Transform transform;
            public Vector3 originalPosition;
            public Quaternion originalRotation;
            public float sensitivity;
            public float multiplier;
            public float randomOffset;
        }

        public static WindSystemModule Instance => instance;

        private void Awake()
        {
            // If there's already an instance, destroy THIS component, not the whole GameObject
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"WindSystemModule: Duplicate found on '{gameObject.name}'. Destroying this component. Keeping instance on '{instance.gameObject.name}'.");
                Destroy(this);
                return;
            }
            
            instance = this;
            timeOffset = Random.Range(0f, 100f);
            mainCamera = Camera.main;
            
            // Sync the direction axes to baseDirection
            UpdateDirectionFromAxes();
            
            if (autoFindObjects)
            {
                FindAllWindObjects();
            }
        }

        private void FindAllWindObjects()
        {
            // Use the simplest FindObjectsByType - no sorting needed for performance
            WindAffectedObject[] affectedObjects = FindObjectsByType<WindAffectedObject>(FindObjectsInactive.Exclude);
            
            foreach (var obj in affectedObjects)
            {
                AddObject(obj.transform, obj.Sensitivity, obj.Multiplier);
            }
            
            Debug.Log($"WindSystemModule: Found {objects.Count} wind-affected objects.");
        }

        public void AddObject(Transform transform, float sensitivity, float multiplier)
        {
            var data = new WindAffectedData
            {
                transform = transform,
                originalPosition = transform.position,
                originalRotation = transform.rotation,
                sensitivity = Mathf.Clamp(sensitivity, 0f, 2f),
                multiplier = Mathf.Clamp(multiplier, 0f, 5f),
                randomOffset = Random.Range(0f, 100f)
            };
            
            objects.Add(data);
            
            // Also add to cached lists for faster iteration
            transforms.Add(transform);
            originalPositions.Add(transform.position);
            originalRotations.Add(transform.rotation);
            sensitivities.Add(data.sensitivity);
            multipliers.Add(data.multiplier);
            randomOffsets.Add(data.randomOffset);
        }

        private void Update()
        {
            UpdateWind();
            ProcessBatch();
        }

        private void UpdateWind()
        {
            float time = Time.time + timeOffset;
            
            float gust = Mathf.Sin(time * gustFrequency) * gustAmplitude;
            
            float turbX = Mathf.PerlinNoise(time * turbulenceFrequency + 0.5f, 0) * turbulence * 2 - turbulence;
            float turbZ = Mathf.PerlinNoise(0, time * turbulenceFrequency + 0.5f) * turbulence * 2 - turbulence;
            
            currentStrength = Mathf.Max(0, baseStrength + gust);
            
            // Apply turbulence to direction
            Vector3 turbDirection = baseDirection + new Vector3(turbX, 0, turbZ) * turbulenceScale;
            
            // Only normalize if magnitude is significant
            if (turbDirection.magnitude > 0.001f)
            {
                currentDirection = turbDirection.normalized;
            }
            else
            {
                currentDirection = Vector3.right;
            }
        }

        private void ProcessBatch()
        {
            if (objects.Count == 0) return;
            
            // Process a batch of objects
            int start = currentIndex;
            int end = Mathf.Min(start + maxProcessPerFrame, objects.Count);
            
            // Cache camera position for distance checks
            Vector3 cameraPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
            
            for (int i = start; i < end; i++)
            {
                // Distance check
                if (mainCamera != null && maxUpdateDistance > 0)
                {
                    float distance = Vector3.Distance(cameraPos, transforms[i].position);
                    if (distance > maxUpdateDistance)
                    {
                        // Too far - return to original and skip
                        transforms[i].position = Vector3.Lerp(transforms[i].position, 
                            originalPositions[i], Time.deltaTime * 2f);
                        transforms[i].rotation = Quaternion.Slerp(transforms[i].rotation, 
                            originalRotations[i], Time.deltaTime * 2f);
                        continue;
                    }
                }
                
                UpdateSingleObject(i);
            }
            
            // Advance the index
            currentIndex = end;
            if (currentIndex >= objects.Count)
            {
                currentIndex = 0;
            }
        }

        private void UpdateSingleObject(int index)
        {
            Transform t = transforms[index];
            Vector3 originalPos = originalPositions[index];
            Quaternion originalRot = originalRotations[index];
            
            float sensitivity = sensitivities[index];
            float multiplier = multipliers[index];
            float randomOffset = randomOffsets[index];
            
            // Calculate wind force with local variation
            float windEffect = currentStrength * sensitivity * multiplier * 0.5f;
            Vector3 windForce = currentDirection * windEffect;
            
            // Add noise variation
            float noiseX = Mathf.Sin(Time.time * 0.7f + randomOffset) * 0.3f;
            float noiseZ = Mathf.Cos(Time.time * 0.5f + randomOffset * 0.7f) * 0.3f;
            windForce += new Vector3(noiseX, 0, noiseZ) * windEffect * 0.5f;
            
            if (windForce.magnitude > 0.0001f)
            {
                // Sway
                Vector3 targetPos = originalPos + windForce * Time.deltaTime * 10f;
                t.position = Vector3.Lerp(t.position, targetPos, Time.deltaTime * 4f);
                
                // Tilt
                float tiltAngle = Mathf.Min(windForce.magnitude * 15f, 30f);
                Vector3 tiltAxis = Vector3.Cross(Vector3.up, windForce.normalized);
                if (tiltAxis.magnitude > 0.01f)
                {
                    Quaternion targetTilt = Quaternion.AngleAxis(tiltAngle, tiltAxis);
                    t.rotation = Quaternion.Slerp(t.rotation, 
                        originalRot * targetTilt, Time.deltaTime * 4f);
                }
            }
            else
            {
                // Return to rest
                t.position = Vector3.Lerp(t.position, originalPos, Time.deltaTime * 2f);
                t.rotation = Quaternion.Slerp(t.rotation, originalRot, Time.deltaTime * 2f);
            }
        }

        // --- Public API ---
        public void RegisterObject(Transform transform, float sensitivity = 1f, float multiplier = 1f)
        {
            AddObject(transform, sensitivity, multiplier);
        }

        public void UnregisterObject(Transform transform)
        {
            int index = transforms.IndexOf(transform);
            if (index >= 0)
            {
                objects.RemoveAt(index);
                transforms.RemoveAt(index);
                originalPositions.RemoveAt(index);
                originalRotations.RemoveAt(index);
                sensitivities.RemoveAt(index);
                multipliers.RemoveAt(index);
                randomOffsets.RemoveAt(index);
            }
        }

        public Vector3 GetWindForce(Vector3 position)
        {
            return currentDirection * currentStrength * 0.01f;
        }

        // --- Direction Axis Controls ---
        private void UpdateDirectionFromAxes()
        {
            baseDirection = new Vector3(directionX, directionY, directionZ);
            
            // If all axes are 0, default to right
            if (baseDirection.magnitude < 0.001f)
            {
                baseDirection = Vector3.right;
                directionX = 1f;
                directionY = 0f;
                directionZ = 0f;
            }
        }

        private void UpdateAxesFromDirection()
        {
            directionX = Mathf.Clamp(baseDirection.x, -1f, 1f);
            directionY = Mathf.Clamp(baseDirection.y, -1f, 1f);
            directionZ = Mathf.Clamp(baseDirection.z, -1f, 1f);
        }

        // --- Editor Only ---
        private void OnDrawGizmosSelected()
        {
            if (mainCamera != null)
            {
                // Draw a circle showing max update distance
                Gizmos.color = new Color(0, 1, 0, 0.1f);
                Gizmos.DrawWireSphere(mainCamera.transform.position, maxUpdateDistance);
                
                // Draw wind direction arrow
                Gizmos.color = new Color(0, 0.5f, 1, 0.5f);
                Vector3 start = transform.position;
                Vector3 end = start + currentDirection * 5f;
                Gizmos.DrawLine(start, end);
                
                // Arrow head
                Vector3 dir = (end - start).normalized;
                Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
                if (right.magnitude < 0.01f) right = Vector3.right;
                Vector3 up = Vector3.Cross(right, dir).normalized;
                
                float headSize = 0.5f;
                Gizmos.DrawLine(end, end - dir * headSize + right * headSize * 0.5f);
                Gizmos.DrawLine(end, end - dir * headSize - right * headSize * 0.5f);
                Gizmos.DrawLine(end, end - dir * headSize + up * headSize * 0.5f);
                Gizmos.DrawLine(end, end - dir * headSize - up * headSize * 0.5f);
            }
        }

        // --- Validation ---
        private void OnValidate()
        {
            // Clamp all values to their ranges
            baseStrength = Mathf.Clamp(baseStrength, 0f, 5f);
            gustFrequency = Mathf.Clamp(gustFrequency, 0.1f, 10f);
            gustAmplitude = Mathf.Clamp(gustAmplitude, 0f, 2f);
            turbulence = Mathf.Clamp(turbulence, 0f, 1f);
            turbulenceFrequency = Mathf.Clamp(turbulenceFrequency, 0.1f, 5f);
            turbulenceScale = Mathf.Clamp(turbulenceScale, 0f, 0.5f);
            maxProcessPerFrame = Mathf.Clamp(maxProcessPerFrame, 10, 200);
            maxUpdateDistance = Mathf.Clamp(maxUpdateDistance, 0f, 500f);
            
            // Clamp direction axes
            directionX = Mathf.Clamp(directionX, -1f, 1f);
            directionY = Mathf.Clamp(directionY, -1f, 1f);
            directionZ = Mathf.Clamp(directionZ, -1f, 1f);
            
            // Update baseDirection from axes (the Vector3 field will show the result)
            baseDirection = new Vector3(directionX, directionY, directionZ);
            
            // If all axes are 0, default to right
            if (baseDirection.magnitude < 0.001f)
            {
                baseDirection = Vector3.right;
                directionX = 1f;
                directionY = 0f;
                directionZ = 0f;
            }
        }
    }
}