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
        [Tooltip("The direction the wind blows. Driven by the axis sliders below - edit those, not this directly.")]
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
        [Tooltip("Maximum number of objects to process per frame. Lower = better performance but objects update less often. Higher = smoother but more CPU usage per frame.")]
        [SerializeField, Range(10, 200)] private int maxProcessPerFrame = 50;

        [Tooltip("Automatically find all Wind Affected Objects in the scene on startup.")]
        [SerializeField] private bool autoFindObjects = true;

        [Tooltip("Objects beyond this distance from the camera won't be updated (saves performance). 0 = unlimited distance.")]
        [SerializeField, Range(0f, 500f)] private float maxUpdateDistance = 100f;

        // --- Data ---
        // A single list of structs (not several parallel lists) - one representation of the data,
        // touched once per object per update instead of six times.
        private static WindSystemModule instance;
        private readonly List<WindAffectedData> objects = new List<WindAffectedData>();
        private readonly Dictionary<Transform, int> indexLookup = new Dictionary<Transform, int>();
        private int currentIndex = 0;
        private float timeOffset;

        // Wind state
        private float currentStrength;
        private Vector3 currentDirection;
        private Camera mainCamera;

        private struct WindAffectedData
        {
            public Transform transform;
            public Vector3 originalPosition;
            public Quaternion originalRotation;
            public float sensitivity;
            public float multiplier;
            public float randomOffset;
            // Real time this object was last processed. Since objects are only updated once every
            // few frames (round-robin via maxProcessPerFrame), lerps need the *actual* elapsed time
            // since last update, not a single frame's worth - otherwise motion stutters and lags
            // behind proportionally to how many objects are registered.
            public float lastUpdateTime;
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

            UpdateDirectionFromAxes();

            if (autoFindObjects)
            {
                FindAllWindObjects();
            }
        }

        private void FindAllWindObjects()
        {
            WindAffectedObject[] affectedObjects = FindObjectsByType<WindAffectedObject>(FindObjectsInactive.Exclude);

            foreach (var obj in affectedObjects)
            {
                RegisterObject(obj.transform, obj.Sensitivity, obj.Multiplier, obj.RandomOffset);
            }
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

            Vector3 turbDirection = baseDirection + new Vector3(turbX, 0, turbZ) * turbulenceScale;

            currentDirection = turbDirection.sqrMagnitude > 0.001f * 0.001f
                ? turbDirection.normalized
                : Vector3.right;
        }

        private void ProcessBatch()
        {
            if (objects.Count == 0) return;

            int start = currentIndex;
            int end = Mathf.Min(start + maxProcessPerFrame, objects.Count);

            Vector3 cameraPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
            float now = Time.time;

            for (int i = start; i < end; i++)
            {
                WindAffectedData data = objects[i];

                if (mainCamera != null && maxUpdateDistance > 0)
                {
                    float distance = Vector3.Distance(cameraPos, data.transform.position);
                    if (distance > maxUpdateDistance)
                    {
                        // Too far to bother animating - ease back to rest and skip.
                        data.transform.position = Vector3.Lerp(data.transform.position, data.originalPosition, Time.deltaTime * 2f);
                        data.transform.rotation = Quaternion.Slerp(data.transform.rotation, data.originalRotation, Time.deltaTime * 2f);
                        data.lastUpdateTime = now;
                        objects[i] = data;
                        continue;
                    }
                }

                UpdateSingleObject(ref data, now);
                objects[i] = data;
            }

            currentIndex = end;
            if (currentIndex >= objects.Count)
            {
                currentIndex = 0;
            }
        }

        private void UpdateSingleObject(ref WindAffectedData data, float now)
        {
            // Elapsed time since this specific object was last touched, not just Time.deltaTime -
            // objects only get processed once every few frames under batching.
            float elapsed = data.lastUpdateTime > 0f ? now - data.lastUpdateTime : Time.deltaTime;
            data.lastUpdateTime = now;

            float windEffect = currentStrength * data.sensitivity * data.multiplier * 0.5f;
            Vector3 windForce = currentDirection * windEffect;

            float noiseX = Mathf.Sin(now * 0.7f + data.randomOffset) * 0.3f;
            float noiseZ = Mathf.Cos(now * 0.5f + data.randomOffset * 0.7f) * 0.3f;
            windForce += new Vector3(noiseX, 0, noiseZ) * windEffect * 0.5f;

            Transform t = data.transform;

            if (windForce.magnitude > 0.0001f)
            {
                Vector3 targetPos = data.originalPosition + windForce * elapsed * 10f;
                t.position = Vector3.Lerp(t.position, targetPos, elapsed * 4f);

                float tiltAngle = Mathf.Min(windForce.magnitude * 15f, 30f);
                Vector3 tiltAxis = Vector3.Cross(Vector3.up, windForce.normalized);
                if (tiltAxis.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetTilt = Quaternion.AngleAxis(tiltAngle, tiltAxis);
                    t.rotation = Quaternion.Slerp(t.rotation, data.originalRotation * targetTilt, elapsed * 4f);
                }
            }
            else
            {
                t.position = Vector3.Lerp(t.position, data.originalPosition, elapsed * 2f);
                t.rotation = Quaternion.Slerp(t.rotation, data.originalRotation, elapsed * 2f);
            }
        }

        // --- Public API ---
        // randomOffset < 0 means "generate one automatically" - pass a specific value (as
        // WindAffectedObject does) to control this object's wind timing deliberately.
        public void RegisterObject(Transform objTransform, float sensitivity = 1f, float multiplier = 1f, float randomOffset = -1f)
        {
            if (indexLookup.ContainsKey(objTransform)) return;

            var data = new WindAffectedData
            {
                transform = objTransform,
                originalPosition = objTransform.position,
                originalRotation = objTransform.rotation,
                sensitivity = Mathf.Clamp(sensitivity, 0f, 2f),
                multiplier = Mathf.Clamp(multiplier, 0f, 5f),
                randomOffset = randomOffset >= 0f ? randomOffset : Random.Range(0f, 100f),
                lastUpdateTime = 0f
            };

            indexLookup[objTransform] = objects.Count;
            objects.Add(data);
        }

        public void UnregisterObject(Transform objTransform)
        {
            if (!indexLookup.TryGetValue(objTransform, out int index)) return;

            // Swap-remove: move the last element into the removed slot so this is O(1) instead of
            // shifting every subsequent element down (what List.RemoveAt would otherwise do).
            int lastIndex = objects.Count - 1;
            WindAffectedData lastData = objects[lastIndex];

            objects[index] = lastData;
            indexLookup[lastData.transform] = index;

            objects.RemoveAt(lastIndex);
            indexLookup.Remove(objTransform);
        }

        /// <summary>
        /// Current wind force, applied uniformly across the whole scene. Note: this does not yet
        /// vary by position - true localized wind zones aren't implemented. If you need per-area
        /// wind, that's the next feature to add here rather than something already supported.
        /// </summary>
        public Vector3 GetWindForce(Vector3 position)
        {
            return currentDirection * currentStrength * 0.01f;
        }

        // --- Direction Axis Controls ---
        private void UpdateDirectionFromAxes()
        {
            baseDirection = new Vector3(directionX, directionY, directionZ);

            if (baseDirection.sqrMagnitude < 0.001f * 0.001f)
            {
                baseDirection = Vector3.right;
                directionX = 1f;
                directionY = 0f;
                directionZ = 0f;
            }
        }

        // --- Editor Only ---
        private void OnDrawGizmosSelected()
        {
            if (mainCamera == null) return;

            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawWireSphere(mainCamera.transform.position, maxUpdateDistance);

            Gizmos.color = new Color(0, 0.5f, 1, 0.5f);
            Vector3 start = transform.position;
            Vector3 end = start + currentDirection * 5f;
            Gizmos.DrawLine(start, end);

            Vector3 dir = (end - start).normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
            Vector3 up = Vector3.Cross(right, dir).normalized;

            float headSize = 0.5f;
            Gizmos.DrawLine(end, end - dir * headSize + right * headSize * 0.5f);
            Gizmos.DrawLine(end, end - dir * headSize - right * headSize * 0.5f);
            Gizmos.DrawLine(end, end - dir * headSize + up * headSize * 0.5f);
            Gizmos.DrawLine(end, end - dir * headSize - up * headSize * 0.5f);
        }

        // --- Validation ---
        private void OnValidate()
        {
            baseStrength = Mathf.Clamp(baseStrength, 0f, 5f);
            gustFrequency = Mathf.Clamp(gustFrequency, 0.1f, 10f);
            gustAmplitude = Mathf.Clamp(gustAmplitude, 0f, 2f);
            turbulence = Mathf.Clamp(turbulence, 0f, 1f);
            turbulenceFrequency = Mathf.Clamp(turbulenceFrequency, 0.1f, 5f);
            turbulenceScale = Mathf.Clamp(turbulenceScale, 0f, 0.5f);
            maxProcessPerFrame = Mathf.Clamp(maxProcessPerFrame, 10, 200);
            maxUpdateDistance = Mathf.Clamp(maxUpdateDistance, 0f, 500f);

            directionX = Mathf.Clamp(directionX, -1f, 1f);
            directionY = Mathf.Clamp(directionY, -1f, 1f);
            directionZ = Mathf.Clamp(directionZ, -1f, 1f);

            baseDirection = new Vector3(directionX, directionY, directionZ);

            if (baseDirection.sqrMagnitude < 0.001f * 0.001f)
            {
                baseDirection = Vector3.right;
                directionX = 1f;
                directionY = 0f;
                directionZ = 0f;
            }
        }
    }
}
