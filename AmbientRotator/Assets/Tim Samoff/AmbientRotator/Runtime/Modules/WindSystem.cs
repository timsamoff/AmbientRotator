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
        [SerializeField] private float baseStrength = 1f;
        [SerializeField] private float gustFrequency = 2f;
        [SerializeField] private float gustAmplitude = 0.5f;
        [SerializeField] private Vector3 baseDirection = Vector3.right;
        
        [Header("Turbulence")]
        [SerializeField] private float turbulence = 0.3f;
        [SerializeField] private float turbulenceFrequency = 0.5f;
        [SerializeField] private float turbulenceScale = 0.1f;
        
        [Header("Visual")]
        [SerializeField] private bool showDebugArrow = true;
        [SerializeField] private Color arrowColor = Color.white;
        [SerializeField] private float arrowLength = 2f;
        
        [Header("Wind Zones")]
        [SerializeField] private List<WindZone> windZones = new List<WindZone>();
        
        [System.Serializable]
        public class WindZone
        {
            public Vector3 center;
            public float radius = 10f;
            public float strength = 1f;
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
                DontDestroyOnLoad(gameObject);
                timeOffset = Random.Range(0f, 100f);
            }
            else
            {
                Destroy(gameObject);
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
            Vector3 force = currentDirection * currentStrength;
            
            foreach (var zone in windZones)
            {
                float distance = Vector3.Distance(position, zone.center);
                if (distance < zone.radius)
                {
                    float influence = 1f - (distance / zone.radius);
                    force += zone.direction * zone.strength * influence;
                }
            }
            
            float variation = Mathf.PerlinNoise(position.x * 0.1f + timeOffset, position.z * 0.1f + timeOffset);
            force *= (0.8f + variation * 0.4f);
            
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