using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    public class ReactiveTrigger : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [Tooltip("The radius of the trigger sphere. Strength is max at center, 0 at edge.")]
        [SerializeField] private float reactionRadius = 5f;
        
        [Tooltip("Trigger objects that can trigger this reaction. Leave empty to react to everything.")]
        [SerializeField] private List<GameObject> triggerObjects = new List<GameObject>();
        
        [Header("Reaction Types (Matches BeatSyncModule)")]
        [Tooltip("Object pulses up and down continuously while trigger is inside radius. Strength = 0 at edge, 1 at center.")]
        [SerializeField] private bool pulse = false;
        
        [Tooltip("How high the object pulses. Multiplied by strength (0-1).")]
        [SerializeField, Range(0f, 1f)] private float pulseHeight = 0.5f;
        
        [Tooltip("Speed of the pulse oscillation. Higher = faster pulsing.")]
        [SerializeField, Range(0.1f, 10f)] private float pulseSpeed = 2f;
        
        [Tooltip("Object rotates continuously while trigger is inside radius. Strength = 0 at edge, 1 at center.")]
        [SerializeField] private bool rotate = false;
        
        [Tooltip("How strongly the object rotates. Positive = clockwise, Negative = counter-clockwise. Higher values = faster spin.")]
        [SerializeField, Range(-100f, 100f)] private float rotationForceMultiplier = 30f;
        
        [Tooltip("Object wobbles side to side continuously while trigger is inside radius. Strength = 0 at edge, 1 at center.")]
        [SerializeField] private bool wobble = false;
        
        [Tooltip("Speed of the wobble motion. Higher = faster wobble.")]
        [SerializeField, Range(0.01f, 20f)] private float wobbleFrequency = 2f;
        
        [Tooltip("How far the object wobbles. Multiplied by strength (0-1).")]
        [SerializeField, Range(0f, 1f)] private float wobbleAmplitude = 0.5f;
        
        [Header("Push/Attract Settings")]
        [Tooltip("Object is pushed away from the triggering object. Strength = 0 at edge, 1 at center.")]
        [SerializeField] private bool pushAway = false;
        
        [Tooltip("How strongly the object is pushed away. Multiplied by distance-based strength.")]
        [SerializeField, Range(0f, 50f)] private float pushAwayStrength = 15f;
        
        [Tooltip("Object is pulled toward the triggering object. Strength = 0 at edge, 1 at center.")]
        [SerializeField] private bool attract = false;
        
        [Tooltip("How strongly the object is pulled toward the trigger. Multiplied by distance-based strength.")]
        [SerializeField, Range(0f, 50f)] private float attractStrength = 10f;
        
        [Header("Return Settings")]
        [Tooltip("How fast the object returns to its original position when the trigger exits. Higher = faster return.")]
        [SerializeField, Range(1f, 20f)] private float returnSpeed = 5f;
        
        [Header("Debug")]
        [Tooltip("Show the trigger radius in the Scene view.")]
        [SerializeField] private bool showGizmos = true;
        
        [Tooltip("Color of the trigger radius gizmo.")]
        [SerializeField] private Color gizmoColor = Color.red;
        
        [Tooltip("Enable debug logging to see trigger events in the console.")]
        [SerializeField] private bool debugLogging = true;
        
        private AmbientRotator parentRotator;
        private Transform cachedTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool hasOriginalPosition = false;
        private float wobbleTimer = 0f;
        private float pulseTimer = 0f;
        private SphereCollider triggerCollider;
        
        private bool isTriggerActive = false;
        private float currentStrength = 0f;
        private Vector3 lastDirection = Vector3.up;
        
        // Rotate tracking - separate from reactions
        private float targetRotationSpeed = 0f;
        private float originalAmbientSpeed = 1f;
        private bool cachedAmbientSpeed = false;
        
        private void Awake()
        {
            cachedTransform = transform;
            parentRotator = GetComponent<AmbientRotator>();
            
            SetupTriggerCollider();
            
            if (parentRotator == null)
            {
                Debug.LogWarning($"ReactiveTrigger on {gameObject.name} requires an AmbientRotator component!");
            }
        }
        
        private void SetupTriggerCollider()
        {
            triggerCollider = GetComponent<SphereCollider>();
            
            if (triggerCollider == null)
            {
                Collider existingCollider = GetComponent<Collider>();
                if (existingCollider != null)
                {
                    DestroyImmediate(existingCollider);
                }
                
                triggerCollider = gameObject.AddComponent<SphereCollider>();
            }
            
            triggerCollider.isTrigger = true;
            triggerCollider.radius = reactionRadius;
        }
        
        private void Start()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            hasOriginalPosition = true;
            targetRotationSpeed = 0f;

        }
        
        private void Update()
        {
            // --- Apply rotation based on target speed ---
            if (rotate && isTriggerActive)
            {
                float rotationAmount = targetRotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotationAmount);
            }
            
            // --- Return to original position/rotation when outside radius ---
            if (!isTriggerActive && hasOriginalPosition)
            {
                // Reset rotation speed when inactive
                targetRotationSpeed = 0f;
                
                // Pulse return
                if (pulse)
                {
                    transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * returnSpeed);
                }
                
                // Wobble return
                if (wobble)
                {
                    transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * returnSpeed);
                }
                
                // Rotate return - ALWAYS return to original rotation when not active
                if (rotate)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * returnSpeed);
                }
                
                // Push Away return
                if (pushAway)
                {
                    transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * returnSpeed);
                }
                
                // Attract return
                if (attract)
                {
                    transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * returnSpeed);
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            isTriggerActive = true;
            
            // Calculate strength based on distance (0 at edge, 1 at center)
            float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
            currentStrength = Mathf.Clamp01(1f - (distance / reactionRadius));
            currentStrength = Mathf.Max(0f, currentStrength);
            
            lastDirection = (cachedTransform.position - other.transform.position).normalized;
            if (lastDirection == Vector3.zero) lastDirection = Vector3.up;
            
            if (debugLogging)
            {
                Debug.Log($"🔵 {other.gameObject.name} entered! Strength: {currentStrength:F2} (Distance: {distance:F2})");
            }
            
            // Reset rotation tracking

            
            // Update target rotation speed
            UpdateTargetSpeed(currentStrength);
            
            // Apply reactions
            ApplyReactions(currentStrength, lastDirection);
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            isTriggerActive = true;
            
            // Recalculate strength every frame (0 at edge, 1 at center)
            float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
            currentStrength = Mathf.Clamp01(1f - (distance / reactionRadius));
            currentStrength = Mathf.Max(0f, currentStrength);
            
            lastDirection = (cachedTransform.position - other.transform.position).normalized;
            if (lastDirection == Vector3.zero) lastDirection = Vector3.up;
            
            // Update target rotation speed based on current strength
            UpdateTargetSpeed(currentStrength);
            
            // Apply continuous reactions
            ApplyReactions(currentStrength, lastDirection);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            isTriggerActive = false;
            currentStrength = 0f;
            
            if (rotate && parentRotator != null && cachedAmbientSpeed)
            {
                parentRotator.Speed = originalAmbientSpeed;
            }
            
            // if (debugLogging)
            // {
            //     Debug.Log($"🔴 {other.gameObject.name} exited! Returning to original position. Total rotation: {accumulatedRotation:F1}°");
            // }
        }
        
        private void UpdateTargetSpeed(float strength)
        {
            if (!rotate || parentRotator == null)
                return;

            float multiplier = 1f + (strength * rotationForceMultiplier * 0.1f);

            parentRotator.Speed = Mathf.Clamp(
                originalAmbientSpeed * multiplier,
                0.1f,
                10f);
        }
        
        private void ApplyReactions(float strength, Vector3 direction)
        {
            // --- Pulse (continuous oscillation) ---
            if (pulse)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulseValue = Mathf.Sin(pulseTimer) * strength * pulseHeight;
                transform.position += Vector3.up * pulseValue;
            }
            
            // --- Wobble (continuous) ---
            if (wobble)
            {
                wobbleTimer += Time.deltaTime * wobbleFrequency;
                float wobbleOffsetAmount = strength * wobbleAmplitude;
                Vector3 wobbleOffset = new Vector3(
                    Mathf.Sin(wobbleTimer) * wobbleOffsetAmount,
                    0,
                    Mathf.Cos(wobbleTimer * 0.7f) * wobbleOffsetAmount
                );
                transform.position += wobbleOffset;
            }
            
            // --- Push Away ---
            if (pushAway)
            {
                transform.position += direction * strength * pushAwayStrength * Time.deltaTime;
            }
            
            // --- Attract ---
            if (attract)
            {
                transform.position -= direction * strength * attractStrength * Time.deltaTime;
            }
        }
        
        private bool ShouldReact(Collider other)
        {
            GameObject obj = other.gameObject;
            
            if (triggerObjects.Count > 0)
            {
                return triggerObjects.Contains(obj);
            }
            
            return true;
        }
        
        public void SetReactionRadius(float radius)
        {
            reactionRadius = Mathf.Max(0.1f, radius);
            
            if (triggerCollider != null)
            {
                triggerCollider.radius = reactionRadius;
            }
        }
        
        public void AddTriggerObject(GameObject obj)
        {
            if (!triggerObjects.Contains(obj))
            {
                triggerObjects.Add(obj);
            }
        }
        
        public void RemoveTriggerObject(GameObject obj)
        {
            if (triggerObjects.Contains(obj))
            {
                triggerObjects.Remove(obj);
            }
        }
        
        public bool IsTriggerActive()
        {
            return isTriggerActive;
        }
        
        public float GetCurrentStrength()
        {
            return currentStrength;
        }
        
        private void OnValidate()
        {
            if (triggerCollider != null)
            {
                triggerCollider.radius = reactionRadius;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, reactionRadius);
            
            // Draw a small dot at center showing where strength = 1
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.1f);
            
            // Draw a ring at 50% radius showing where strength = 0.5
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, reactionRadius * 0.5f);
            
            if (pushAway)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, Vector3.up * reactionRadius * 0.5f);
            }
            
            if (attract)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, Vector3.down * reactionRadius * 0.5f);
            }
            
            if (pulse)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, Vector3.up * reactionRadius * 0.3f);
            }
            
            if (rotate)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, Vector3.forward * reactionRadius * 0.3f);
            }
            
            if (wobble)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, Vector3.right * reactionRadius * 0.3f);
            }
        }
    }
}