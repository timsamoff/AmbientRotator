using UnityEngine;

namespace AmbientRotator
{
    public class ReactiveTrigger : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [SerializeField] private float reactionRadius = 5f;
        [SerializeField] private float pushbackStrength = 2f;
        [SerializeField] private float recoverySpeed = 3f;
        [SerializeField] private float rotationRecovery = 2f;
        
        [Header("Target Types")]
        [SerializeField] private LayerMask targetLayers = -1;
        [SerializeField] private bool reactToPlayer = true;
        [SerializeField] private bool reactToProjectiles = false;
        [SerializeField] private bool reactToPhysics = false;
        
        [Header("Reaction Types")]
        [SerializeField] private bool pushAway = true;
        [SerializeField] private bool attract = false;
        [SerializeField] private bool spin = false;
        [SerializeField] private float spinSpeed = 5f;
        
        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.red;
        
        private AmbientRotator parentRotator;
        private Vector3 currentPushback;
        private float currentSpin;
        private Transform cachedTransform;
        
        private void Start()
        {
            cachedTransform = transform;
            parentRotator = GetComponent<AmbientRotator>();
            
            if (parentRotator == null)
            {
                Debug.LogWarning($"ReactiveTrigger on {gameObject.name} requires an AmbientRotator component!");
            }
        }
        
        private void Update()
        {
            if (currentPushback.magnitude > 0.01f)
            {
                currentPushback = Vector3.Lerp(currentPushback, Vector3.zero, Time.deltaTime * recoverySpeed);
                if (parentRotator != null)
                {
                    parentRotator.ApplyForce(currentPushback);
                }
            }
            
            if (currentSpin > 0.01f)
            {
                currentSpin = Mathf.Lerp(currentSpin, 0, Time.deltaTime * rotationRecovery);
                if (parentRotator != null)
                {
                    parentRotator.ApplyForce(Vector3.up * currentSpin);
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            Vector3 direction = (cachedTransform.position - other.transform.position).normalized;
            float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
            float strength = 1f - (distance / reactionRadius);
            
            if (pushAway)
            {
                ApplyPushback(direction * strength * pushbackStrength);
            }
            
            if (attract)
            {
                ApplyPushback(-direction * strength * pushbackStrength * 0.5f);
            }
            
            if (spin)
            {
                ApplySpin(strength * spinSpeed);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!ShouldReact(other)) return;
            
            if (pushAway || attract)
            {
                Vector3 direction = (cachedTransform.position - other.transform.position).normalized;
                float distance = Vector3.Distance(cachedTransform.position, other.transform.position);
                float strength = 1f - (distance / reactionRadius);
                
                if (pushAway)
                {
                    ApplyPushback(direction * strength * pushbackStrength * Time.deltaTime);
                }
            }
        }
        
        private bool ShouldReact(Collider other)
        {
            if (((1 << other.gameObject.layer) & targetLayers) == 0)
                return false;
            
            if (reactToPlayer && other.CompareTag("Player"))
                return true;
                
            if (reactToProjectiles && other.CompareTag("Projectile"))
                return true;
                
            if (reactToPhysics && other.GetComponent<Rigidbody>() != null)
                return true;
                
            return false;
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
        
        public void ApplySpin(float speed)
        {
            currentSpin += speed;
            if (parentRotator != null)
            {
                parentRotator.ApplyForce(Vector3.up * currentSpin);
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
        }
    }
}