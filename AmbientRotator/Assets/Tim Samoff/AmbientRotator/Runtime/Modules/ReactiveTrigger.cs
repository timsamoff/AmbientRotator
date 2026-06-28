using UnityEngine;
using System.Collections.Generic;

namespace AmbientRotator
{
    public class ReactiveTrigger : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [Tooltip("The radius of the trigger sphere.")]
        [SerializeField] private float reactionRadius = 5f;
        
        [Tooltip("How strongly the object is pushed away.")]
        [SerializeField] private float pushbackStrength = 2f;
        
        [Tooltip("How quickly the object returns to its original position after being pushed.")]
        [SerializeField] private float recoverySpeed = 3f;
        
        [Tooltip("How quickly the object stops rotating after being triggered.")]
        [SerializeField] private float rotationRecovery = 2f;
        
        [Header("Trigger Objects")]
        [Tooltip("Specific GameObjects that can trigger this reaction. Drag and drop objects here. Leave empty to react to everything.")]
        [SerializeField] private List<GameObject> triggerObjects = new List<GameObject>();
        
        [Header("Reaction Types")]
        [Tooltip("Object pulses upward when triggered. Creates a 'bounce' or 'jump' effect.")]
        [SerializeField] private bool pulse = false;
        
        [Tooltip("How high the object jumps when triggered. 0 = no movement, 0.5 = moderate jump, 1 = large jump.")]
        [SerializeField, Range(0f, 1f)] private float pulseHeight = 0.2f;
        
        [Tooltip("Object is pushed away from the triggering object. Creates a 'repel' effect.")]
        [SerializeField] private bool pushAway = false;
        
        [Tooltip("Object is pulled toward the triggering object. Creates a 'attract' or 'magnet' effect.")]
        [SerializeField] private bool attract = false;
        
        [Tooltip("Object rotates when triggered. Creates a 'spin' or 'twist' effect.")]
        [SerializeField] private bool rotate = false;
        
        [Tooltip("How strongly the object rotates when triggered. Positive = clockwise spin, Negative = counter-clockwise spin. Higher values = faster spin.")]
        [SerializeField, Range(-10f, 10f)] private float rotationForceMultiplier = 1f;
        
        [Tooltip("Object wobbles side to side when triggered. Creates a 'sway' or 'rocking' effect.")]
        [SerializeField] private bool wobble = false;
        
        [Tooltip("Speed of the wobble motion. Higher = faster wobble. Lower = slower, more gentle sway.")]
        [SerializeField, Range(0.1f, 10f)] private float wobbleFrequency = 2f;
        
        [Tooltip("How far the object wobbles when triggered. 0 = no wobble, 0.5 = moderate, 1 = large wobble.")]
        [SerializeField, Range(0f, 1f)] private float wobbleAmplitude = 0.2f;
        
        [Header("Debug")]
        [Tooltip("Show the trigger radius in the Scene view.")]
        [SerializeField] private bool showGizmos = true;
        
        [Tooltip("Color of the trigger radius gizmo.")]
        [SerializeField] private Color gizmoColor = Color.red;
        
        private AmbientRotator parentRotator;
        private Vector3 currentPushback;
        private float currentRotation;
        private Transform cachedTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool hasOriginalPosition = false;
        private float wobbleTimer = 0f;
        
        private void Start()
        {
            cachedTransform = transform;
            parentRotator = GetComponent<AmbientRotator>();
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            hasOriginalPosition = true;
            
            if (parentRotator == null)
            {
                Debug.LogWarning($"ReactiveTrigger on {gameObject.name} requires an AmbientRotator component!");
            }
        }
        
        private void Update()
        {
            // Decay pushback
            if (currentPushback.magnitude > 0.01f)
            {
                currentPushback = Vector3.Lerp(currentPushback, Vector3.zero, Time.deltaTime * recoverySpeed);
                if (parentRotator != null)
                {
                    parentRotator.ApplyForce(currentPushback);
                }
            }
            
            // Decay rotation
            if (currentRotation > 0.01f)
            {
                currentRotation = Mathf.Lerp(currentRotation, 0, Time.deltaTime * rotationRecovery);
                if (parentRotator != null)
                {
                    parentRotator.ApplyForce(Vector3.up * currentRotation);
                }
            }
            
            // Smoothly return position if pulse or wobble is enabled
            if ((pulse || wobble) && hasOriginalPosition)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 3f);
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 3f);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            Vector3 direction = (cachedTransform.position - other.transform.position).normalized;
            float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
            float strength = Mathf.Clamp01(1f - (distance / reactionRadius));
            
            // Pulse
            if (pulse)
            {
                ApplyPulse(strength);
            }
            
            // Push Away
            if (pushAway)
            {
                ApplyPushback(direction * strength * pushbackStrength);
            }
            
            // Attract
            if (attract)
            {
                ApplyPushback(-direction * strength * pushbackStrength * 0.5f);
            }
            
            // Rotate
            if (rotate)
            {
                ApplyRotation(strength);
            }
            
            // Wobble
            if (wobble)
            {
                ApplyWobble(strength);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            if (pushAway || attract)
            {
                Vector3 direction = (cachedTransform.position - other.transform.position).normalized;
                float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
                float strength = Mathf.Clamp01(1f - (distance / reactionRadius));
                
                if (pushAway)
                {
                    ApplyPushback(direction * strength * pushbackStrength * Time.deltaTime);
                }
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
        
        public void ApplyPulse(float strength)
        {
            if (parentRotator != null)
            {
                Vector3 pulseForce = Vector3.up * strength * pushbackStrength;
                parentRotator.ApplyForce(pulseForce);
            }
            
            Vector3 pulseOffset = Vector3.up * strength * pulseHeight;
            transform.position += pulseOffset;
        }
        
        public void ApplyPushback(Vector3 force)
        {
            currentPushback += force;
            currentPushback = Vector3.ClampMagnitude(currentPushback, pushbackStrength * 2f);
            
            if (parentRotator != null)
            {
                parentRotator.ApplyForce(currentPushback);
            }
        }
        
        public void ApplyRotation(float strength)
        {
            float rotationForce = Random.Range(-1f, 1f) * strength * pushbackStrength * rotationForceMultiplier;
            currentRotation += rotationForce;
            
            if (parentRotator != null)
            {
                parentRotator.ApplyForce(Vector3.up * currentRotation);
            }
            
            // Direct rotation for immediate visibility
            float rotationAmount = strength * pushbackStrength * rotationForceMultiplier * 0.5f;
            transform.Rotate(Vector3.up, rotationAmount);
        }
        
        public void ApplyWobble(float strength)
        {
            wobbleTimer += Time.deltaTime * wobbleFrequency;
            
            float wobbleOffsetAmount = strength * wobbleAmplitude;
            Vector3 wobbleOffset = new Vector3(
                Mathf.Sin(wobbleTimer) * wobbleOffsetAmount,
                0,
                Mathf.Cos(wobbleTimer * 0.7f) * wobbleOffsetAmount
            );
            transform.position += wobbleOffset;
            
            if (parentRotator != null)
            {
                Vector3 wobbleForce = new Vector3(
                    Mathf.Sin(wobbleTimer),
                    0,
                    Mathf.Cos(wobbleTimer * 0.7f)
                ) * strength * pushbackStrength * 0.5f;
                parentRotator.ApplyForce(wobbleForce);
            }
        }
        
        public void SetReactionRadius(float radius)
        {
            reactionRadius = Mathf.Max(0.1f, radius);
            var collider = GetComponent<Collider>();
            if (collider != null && collider is SphereCollider)
            {
                ((SphereCollider)collider).radius = reactionRadius;
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
        
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, reactionRadius);
            
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