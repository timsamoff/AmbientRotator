using UnityEngine;

namespace AmbientRotator
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsAmbientRotator : AmbientRotator
    {
        [Header("Physics Settings")]
        [Tooltip("Scales the ambient motion before it's converted into torque/rotation. Higher = more dramatic physical motion.")]
        [SerializeField] private float torqueMultiplier = 1f;

        [Tooltip("How quickly the applied torque catches up to the target torque. Lower = heavier, slower-reacting; higher = snappier.")]
        [SerializeField] private float damping = 0.5f;

        [Tooltip("When enabled, motion is driven through the Rigidbody (via MoveRotation or AddTorque below) so it respects physics and collisions. When disabled, this behaves like a regular AmbientRotator and ignores physics entirely.")]
        [SerializeField] private bool usePhysicsRotation = true;

        [Tooltip("When enabled, motion is applied as a continuous force via Rigidbody.AddTorque - the object can be pushed off course by collisions and other physics. When disabled, rotation is set directly via Rigidbody.MoveRotation, which respects physics for collision purposes but can't be knocked off course by other forces.")]
        [SerializeField] private bool usePhysicsForce = false;

        [Tooltip("Scales the torque applied when Use Physics Force is enabled. Only relevant if Use Physics Force is on.")]
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