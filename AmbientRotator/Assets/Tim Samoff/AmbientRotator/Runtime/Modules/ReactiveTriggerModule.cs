using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    public class ReactiveTriggerModule : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [Tooltip("The radius of the trigger sphere.")]
        [SerializeField] private float reactionRadius = 5f;
        
        [Tooltip("Objects that can trigger this reaction. Leave empty to react to everything.")]
        [SerializeField] private List<GameObject> triggerObjects = new List<GameObject>();

        [Header("Reaction Configuration")]
        [Tooltip("How the object reacts to triggers. Shares the same settings as BeatSyncModule.")]
        [SerializeField]
        private MotionReaction reaction = new MotionReaction();

        [Header("Push/Attract")]
        [Tooltip("Object is pushed away from the triggering object.")]
        [SerializeField] private bool pushAway = false;
        
        [Tooltip("How strongly the object is pushed away. Higher = further push.")]
        [SerializeField, Range(0f, 20f)] private float pushAwayStrength = 5f;
        
        [Tooltip("Object is pulled toward the triggering object.")]
        [SerializeField] private bool attract = false;
        
        [Tooltip("How strongly the object is pulled toward the trigger. Higher = stronger pull.")]
        [SerializeField, Range(0f, 20f)] private float attractStrength = 5f;

        [Header("Return Speed")]
        [Tooltip("How fast the object returns when the trigger exits.")]
        [SerializeField, Range(1f, 20f)] private float returnSpeed = 5f;

        [Header("Debug")]
        [Tooltip("Show the trigger radius in the Scene view.")]
        [SerializeField] private bool showGizmos = true;
        
        [Tooltip("Color of the trigger radius gizmo.")]
        [SerializeField] private Color gizmoColor = Color.red;
        
        [Tooltip("Enable debug logging.")]
        [SerializeField] private bool debugLogging = true;

        private AmbientRotator parentRotator;
        private Transform cachedTransform;
        private SphereCollider triggerCollider;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        
        private bool isTriggerActive = false;
        private float currentStrength = 0f;
        private Vector3 lastDirection = Vector3.up;
        private GameObject currentTriggerObject;
        
        // Store the accumulated push/pull offset
        private Vector3 pushPullOffset = Vector3.zero;
        
        // Store pulse state
        private float pulseTimer = 0f;
        private Vector3 pulseOffset = Vector3.zero;
        
        // Store wobble state
        private float wobbleTimer = 0f;
        private Vector3 wobbleOffset = Vector3.zero;

        // --- Public Properties ---
        public MotionReaction Reaction => reaction;
        public bool IsTriggerActive => isTriggerActive;
        public float CurrentStrength => currentStrength;

        private void Awake()
        {
            cachedTransform = transform;
            parentRotator = GetComponent<AmbientRotator>();
            originalPosition = cachedTransform.position;
            originalRotation = cachedTransform.rotation;
            
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
            if (reaction != null)
            {
                reaction.Validate();
                reaction.ResetSmoothing();
            }

            if (debugLogging)
            {
                Debug.Log($"=== ReactiveTrigger Initialized ===");
                Debug.Log($"Pulse: {reaction.pulse}, Height: {reaction.pulseHeight}, Intensity: {reaction.pulseIntensity}");
                Debug.Log($"Rotate: {reaction.rotate}, Max Angle: {reaction.maxRotationAngle}°, Intensity: {reaction.rotationIntensity}");
                Debug.Log($"Wobble: {reaction.wobble}, Amount: {reaction.wobbleAmount}, Intensity: {reaction.wobbleIntensity}");
                Debug.Log($"Push Away: {pushAway}, Strength: {pushAwayStrength}");
                Debug.Log($"Attract: {attract}, Strength: {attractStrength}");
            }
        }
        
        private void Update()
        {
            // --- Handle return when not active ---
            if (!isTriggerActive)
            {
                // Reset speed multiplier
                if (parentRotator != null)
                {
                    parentRotator.SetSpeedMultiplier(1f);
                }
                
                // Smoothly return push/pull offset to zero
                if (pushPullOffset.magnitude > 0.001f)
                {
                    pushPullOffset = Vector3.Lerp(pushPullOffset, Vector3.zero, Time.deltaTime * returnSpeed);
                    cachedTransform.position = originalPosition + pushPullOffset;
                }
                
                // Smoothly return pulse offset to zero
                if (pulseOffset.magnitude > 0.001f)
                {
                    pulseOffset = Vector3.Lerp(pulseOffset, Vector3.zero, Time.deltaTime * returnSpeed);
                    cachedTransform.position = originalPosition + pushPullOffset + pulseOffset;
                }
                
                // Smoothly return wobble offset to zero
                if (wobbleOffset.magnitude > 0.001f)
                {
                    wobbleOffset = Vector3.Lerp(wobbleOffset, Vector3.zero, Time.deltaTime * returnSpeed);
                    cachedTransform.position = originalPosition + pushPullOffset + pulseOffset + wobbleOffset;
                }
                
                // Snap when very close
                if (pushPullOffset.magnitude < 0.001f && pulseOffset.magnitude < 0.001f && wobbleOffset.magnitude < 0.001f)
                {
                    if (cachedTransform.position != originalPosition)
                    {
                        cachedTransform.position = originalPosition;
                    }
                    pushPullOffset = Vector3.zero;
                    pulseOffset = Vector3.zero;
                    wobbleOffset = Vector3.zero;
                }
            }
            
            // Update original position if it changed (in case of parent movement)
            if (!isTriggerActive && cachedTransform.position != originalPosition && 
                pushPullOffset.magnitude < 0.001f && pulseOffset.magnitude < 0.001f && wobbleOffset.magnitude < 0.001f)
            {
                originalPosition = cachedTransform.position;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            isTriggerActive = true;
            currentTriggerObject = other.gameObject;
            
            float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
            currentStrength = Mathf.Clamp01(1f - (distance / reactionRadius));
            currentStrength = Mathf.Max(0f, currentStrength);
            
            lastDirection = (cachedTransform.position - other.transform.position).normalized;
            if (lastDirection == Vector3.zero) lastDirection = Vector3.up;
            
            // Update original position when entering (account for existing offsets)
            originalPosition = cachedTransform.position - pushPullOffset - pulseOffset - wobbleOffset;
            
            if (debugLogging)
            {
                Debug.Log($"🔵 {other.gameObject.name} entered! Strength: {currentStrength:F2}, Distance: {distance:F2}");
            }
            
            ApplyReactions(currentStrength, lastDirection);
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            isTriggerActive = true;
            currentTriggerObject = other.gameObject;
            
            float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
            currentStrength = Mathf.Clamp01(1f - (distance / reactionRadius));
            currentStrength = Mathf.Max(0f, currentStrength);
            
            lastDirection = (cachedTransform.position - other.transform.position).normalized;
            if (lastDirection == Vector3.zero) lastDirection = Vector3.up;
            
            ApplyReactions(currentStrength, lastDirection);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            isTriggerActive = false;
            currentStrength = 0f;
            currentTriggerObject = null;
            
            if (debugLogging)
            {
                Debug.Log($"🔴 {other.gameObject.name} exited!");
            }
        }
        
        private void ApplyReactions(float strength, Vector3 direction)
        {
            if (cachedTransform == null) return;

            if (debugLogging && strength > 0.01f)
            {
                Debug.Log($"  Applying reactions with strength: {strength:F2}, Direction: {direction}");
            }

            // --- PULSE (Direct Transform - Continuous Oscillation) ---
            if (reaction != null && reaction.pulse && reaction.pulseHeight > 0.01f && reaction.pulseIntensity > 0.01f)
            {
                float intensityMultiplier = reaction.pulseIntensity / 50f;
                float targetPulseHeight = strength * reaction.pulseHeight * intensityMultiplier * 0.5f;
                
                // Continuous oscillation - both up AND down
                pulseTimer += Time.deltaTime * reaction.pulseSpeed;
                float pulseValue = Mathf.Sin(pulseTimer) * targetPulseHeight;
                
                // Apply pulse offset (can be positive or negative)
                Vector3 targetPulseOffset = Vector3.up * pulseValue;
                pulseOffset = Vector3.Lerp(pulseOffset, targetPulseOffset, Time.deltaTime * 8f);
                
                if (debugLogging && strength > 0.05f)
                {
                    Debug.Log($"  Pulse: {pulseValue:F3}");
                }
            }
            
            // --- WOBBLE (Direct Transform - Continuous) ---
            if (reaction != null && reaction.wobble && reaction.wobbleAmount > 0.01f && reaction.wobbleIntensity > 0.01f)
            {
                float intensityMultiplier = reaction.wobbleIntensity / 50f;
                float wobbleScale = strength * reaction.wobbleAmount * intensityMultiplier * 0.3f;
                
                wobbleTimer += Time.deltaTime * reaction.wobbleFrequency;
                float wobbleX = Mathf.Sin(wobbleTimer) * wobbleScale;
                float wobbleZ = Mathf.Cos(wobbleTimer * 0.7f + 1.2f) * wobbleScale;
                
                Vector3 targetWobbleOffset = new Vector3(wobbleX, 0, wobbleZ);
                wobbleOffset = Vector3.Lerp(wobbleOffset, targetWobbleOffset, Time.deltaTime * 8f);
                
                if (debugLogging && strength > 0.05f)
                {
                    Debug.Log($"  Wobble: ({wobbleX:F3}, 0, {wobbleZ:F3})");
                }
            }
            
            // --- ROTATION (via AmbientRotator - Stronger Effect) ---
            if (reaction != null && reaction.rotate && reaction.maxRotationAngle > 0.01f && reaction.rotationIntensity > 0.01f && parentRotator != null)
            {
                float intensityMultiplier = reaction.rotationIntensity / 50f;
                
                // Apply rotation offset directly to AmbientRotator (like BeatSync)
                float rotationAmount = strength * reaction.maxRotationAngle * intensityMultiplier * 0.1f;
                Vector3 axis = reaction.rotationAxis.normalized;
                if (axis.magnitude < 0.001f) axis = Vector3.up;
                parentRotator.AddRotationOffset(axis * rotationAmount);
                
                // Also modulate speed for continuous rotation feel
                float speedMultiplier = 1f + (strength * reaction.maxRotationAngle * 0.03f * intensityMultiplier);
                parentRotator.SetSpeedMultiplier(speedMultiplier);
                
                if (debugLogging && strength > 0.05f)
                {
                    Debug.Log($"  Rotation: {rotationAmount:F3}°, Speed: {speedMultiplier:F2}x");
                }
            }
            
            // --- Push Away (Direct Transform) ---
            if (pushAway && pushAwayStrength > 0.01f && strength > 0.01f)
            {
                Vector3 targetOffset = direction * strength * pushAwayStrength * 0.5f;
                pushPullOffset = Vector3.Lerp(pushPullOffset, targetOffset, Time.deltaTime * 5f);
            }
            
            // --- Attract (Direct Transform) ---
            if (attract && attractStrength > 0.01f && strength > 0.01f)
            {
                Vector3 targetOffset = -direction * strength * attractStrength * 0.5f;
                pushPullOffset = Vector3.Lerp(pushPullOffset, targetOffset, Time.deltaTime * 5f);
            }
            
            // --- Apply all offsets to transform ---
            cachedTransform.position = originalPosition + pushPullOffset + pulseOffset + wobbleOffset;
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

        public void SetReaction(MotionReaction newReaction)
        {
            if (newReaction != null)
            {
                reaction = newReaction.Clone();
                reaction.Validate();
                reaction.ResetSmoothing();
            }
        }
        
        public GameObject GetCurrentTriggerObject() => currentTriggerObject;
        
        private void OnValidate()
        {
            if (triggerCollider != null)
            {
                triggerCollider.radius = reactionRadius;
            }

            if (reaction != null)
            {
                reaction.Validate();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, reactionRadius);
            
            if (reaction != null)
            {
                if (reaction.pulse)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(transform.position, Vector3.up * reactionRadius * 0.3f);
                }
                
                if (reaction.rotate)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(transform.position, Vector3.forward * reactionRadius * 0.3f);
                }
                
                if (reaction.wobble)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(transform.position, Vector3.right * reactionRadius * 0.3f);
                }
            }
            
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
        }
    }
}