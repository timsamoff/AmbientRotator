using UnityEngine;

namespace AmbientRotator
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsAmbientRotator : AmbientRotator
    {
        [Header("Physics Settings")]
        [SerializeField] private float torqueMultiplier = 1f;
        [SerializeField] private float damping = 0.5f;
        [SerializeField] private bool usePhysicsRotation = true;
        [SerializeField] private bool usePhysicsForce = false;
        [SerializeField] private float forceMultiplier = 0.1f;
        
        private Rigidbody rb;
        private Vector3 targetTorque;
        private Vector3 currentTorque;
        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            
            if (rb == null)
            {
                Debug.LogError("PhysicsAmbientRotator requires a Rigidbody component!");
            }
        }
        
        protected override void UpdateMotion()
        {
            if (!usePhysicsRotation)
            {
                base.UpdateMotion();
                return;
            }
            
            // CalculateMotion is now protected, so we can access it
            Vector3 motionOffset = CalculateMotion(currentTime);
            targetTorque = motionOffset * torqueMultiplier;
            
            currentTorque = Vector3.Lerp(currentTorque, targetTorque, Time.deltaTime * damping);
            
            if (usePhysicsForce)
            {
                rb.AddTorque(currentTorque * forceMultiplier, ForceMode.Force);
            }
            else
            {
                Quaternion targetRotation = initialRotation * Quaternion.Euler(currentTorque);
                rb.MoveRotation(targetRotation);
            }
        }
        
        public void AddTorque(Vector3 torque)
        {
            if (rb != null)
            {
                rb.AddTorque(torque, ForceMode.Impulse);
            }
        }
        
        public void SetPhysicsSettings(float torque, float dampingValue)
        {
            torqueMultiplier = Mathf.Max(0, torque);
            damping = Mathf.Clamp(dampingValue, 0.01f, 1f);
        }
    }
}