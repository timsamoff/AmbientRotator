using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    public class WindSystem : MonoBehaviour
    {
        private static WindSystem instance;
        public static float GlobalWindStrength => instance?.currentStrength ?? 0f;
        public static Vector3 GlobalWindDirection => instance?.currentDirection ?? Vector3.right;
        
        [Header("Wind Settings")]
        [Tooltip("Base wind strength. Higher values = stronger wind effect.")]
        [SerializeField] private float baseStrength = 0.3f;
        
        [Tooltip("How often wind gusts occur. Higher = more frequent gusts.")]
        [SerializeField] private float gustFrequency = 2f;
        
        [Tooltip("How strong the gusts are. Higher = more dramatic gusts.")]
        [SerializeField] private float gustAmplitude = 0.2f;
        
        [Tooltip("The main direction the wind blows. (X, Y, Z)")]
        [SerializeField] private Vector3 baseDirection = Vector3.right;
        
        [Header("Turbulence")]
        [Tooltip("How much random variation the wind has. Higher = more chaotic wind.")]
        [SerializeField] private float turbulence = 0.2f;
        
        [Tooltip("How quickly the turbulence changes. Higher = faster changing wind.")]
        [SerializeField] private float turbulenceFrequency = 0.5f;
        
        [Tooltip("How much the turbulence affects direction. Higher = more directional variation.")]
        [SerializeField] private float turbulenceScale = 0.05f;
        
        [Header("Visual")]
        [Tooltip("Show a debug arrow in the Scene view showing wind direction and strength.")]
        [SerializeField] private bool showDebugArrow = true;
        
        [Tooltip("Color of the debug arrow.")]
        [SerializeField] private Color arrowColor = Color.white;
        
        [Tooltip("Length of the debug arrow. Longer = visually stronger wind.")]
        [SerializeField] private float arrowLength = 2f;
        
        [Header("Wind Zones")]
        [Tooltip("Localized areas with different wind behavior.")]
        [SerializeField] private List<WindZone> windZones = new List<WindZone>();
        
        [System.Serializable]
        public class WindZone
        {
            [Tooltip("Center position of the wind zone.")]
            public Vector3 center;
            
            [Tooltip("Radius of the wind zone's influence.")]
            public float radius = 10f;
            
            [Tooltip("Wind strength multiplier within this zone.")]
            public float strength = 1f;
            
            [Tooltip("Wind direction within this zone.")]
            public Vector3 direction = Vector3.right;
        }
        
        private float currentStrength;
        private Vector3 currentDirection;
        private float timeOffset;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                timeOffset = Random.Range(0f, 100f);
                Debug.Log("WindSystem initialized.");
            }
            else
            {
                Debug.LogWarning("Multiple WindSystems found. Using the first one.");
            }
        }
        
        private void Update()
        {
            UpdateWind();
        }
        
        private void UpdateWind()
        {
            float gust = Mathf.Sin(Time.time * gustFrequency + timeOffset) * gustAmplitude;
            
            float turbX = Mathf.PerlinNoise(Time.time * turbulenceFrequency + timeOffset, 0) * turbulence * 2 - turbulence;
            float turbZ = Mathf.PerlinNoise(0, Time.time * turbulenceFrequency + timeOffset) * turbulence * 2 - turbulence;
            
            currentStrength = Mathf.Max(0, baseStrength + gust);
            
            Vector3 turbDirection = baseDirection + new Vector3(turbX, 0, turbZ) * turbulenceScale;
            currentDirection = turbDirection.normalized;
        }
        
        public Vector3 GetWindForce(Vector3 position)
        {
            Vector3 force = currentDirection * currentStrength * 0.01f;
            
            foreach (var zone in windZones)
            {
                float distance = Vector3.Distance(position, zone.center);
                if (distance < zone.radius)
                {
                    float influence = 1f - (distance / zone.radius);
                    force += zone.direction * zone.strength * influence * 0.005f;
                }
            }
            
            float variation = Mathf.PerlinNoise(position.x * 0.1f + timeOffset, position.z * 0.1f + timeOffset);
            force *= (0.8f + variation * 0.2f);
            
            return force;
        }
        
        public void AddWindZone(Vector3 center, float radius, float strength, Vector3 direction)
        {
            var zone = new WindZone
            {
                center = center,
                radius = radius,
                strength = strength,
                direction = direction
            };
            windZones.Add(zone);
        }
        
        public void RemoveWindZone(Vector3 center)
        {
            windZones.RemoveAll(z => z.center == center);
        }
        
        public void SetWindStrength(float strength)
        {
            baseStrength = Mathf.Max(0, strength);
        }
        
        public void SetWindDirection(Vector3 direction)
        {
            baseDirection = direction.normalized;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showDebugArrow) return;
            
            Gizmos.color = arrowColor;
            Vector3 start = transform.position;
            Vector3 end = start + currentDirection * arrowLength;
            Gizmos.DrawLine(start, end);
            
            Vector3 dir = (end - start).normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, dir).normalized;
            
            float arrowHeadSize = 0.3f;
            Gizmos.DrawLine(end, end - dir * arrowHeadSize + right * arrowHeadSize);
            Gizmos.DrawLine(end, end - dir * arrowHeadSize - right * arrowHeadSize);
            Gizmos.DrawLine(end, end - dir * arrowHeadSize + up * arrowHeadSize);
            Gizmos.DrawLine(end, end - dir * arrowHeadSize - up * arrowHeadSize);
        }
    }
}