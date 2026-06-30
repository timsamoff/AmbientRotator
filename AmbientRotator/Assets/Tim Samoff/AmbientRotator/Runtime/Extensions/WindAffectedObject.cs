using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach to grass blades, leaves, etc. that should sway in wind.
    /// The WindSystemModule will handle all the heavy lifting.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Wind Affected Object")]
    public class WindAffectedObject : MonoBehaviour
    {
        [Header("Wind Response")]
        [Tooltip("How much this object reacts to wind.\n" +
                 "0 = no reaction\n" +
                 "0.5 = subtle sway\n" +
                 "1.0 = normal sway\n" +
                 "1.5 = exaggerated sway\n" +
                 "2.0 = extreme sway")]
        [SerializeField, Range(0f, 2f)] private float sensitivity = 1f;
        
        [Tooltip("Multiplier for wind force on this object.\n" +
                 "0 = no force\n" +
                 "1 = normal force\n" +
                 "3 = strong force\n" +
                 "5 = extreme force")]
        [SerializeField, Range(0f, 5f)] private float multiplier = 1f;
        
        [Tooltip("Random offset for this object's wind timing. Auto-generated, but can be manually set for specific timing.\n" +
                 "Range: 0-100. Higher values = different timing offset.")]
        [SerializeField] private float randomOffset = 0f;
        
        public float Sensitivity => sensitivity;
        public float Multiplier => multiplier;
        public float RandomOffset => randomOffset;
        
        private AmbientRotator ambientRotator;
        private bool isRegistered = false;
        
        private void Start()
        {
            // Generate random offset if not set (allow 0 to be a valid value)
            if (randomOffset == 0f)
            {
                randomOffset = Random.Range(0f, 100f);
            }
            
            // Check for AmbientRotator on the same object
            ambientRotator = GetComponent<AmbientRotator>();
            
            // Register with the manager
            if (WindSystemModule.Instance != null)
            {
                WindSystemModule.Instance.RegisterObject(transform, sensitivity, multiplier);
                isRegistered = true;
            }
            else
            {
                Debug.LogWarning($"WindAffectedObject on '{gameObject.name}': No WindSystemModule found in scene. Please add one to any GameObject.");
            }
        }
        
        private void OnDestroy()
        {
            // Unregister when destroyed
            if (isRegistered && WindSystemModule.Instance != null)
            {
                WindSystemModule.Instance.UnregisterObject(transform);
            }
        }
        
        // Runtime property setters
        public void SetSensitivity(float value)
        {
            sensitivity = Mathf.Clamp(value, 0f, 2f);
            UpdateRegistration();
        }
        
        public void SetMultiplier(float value)
        {
            multiplier = Mathf.Clamp(value, 0f, 5f);
            UpdateRegistration();
        }
        
        public void SetRandomOffset(float value)
        {
            randomOffset = value; // No clamp - any value is valid for timing offset
            UpdateRegistration();
        }
        
        private void UpdateRegistration()
        {
            if (isRegistered && WindSystemModule.Instance != null)
            {
                WindSystemModule.Instance.UnregisterObject(transform);
                WindSystemModule.Instance.RegisterObject(transform, sensitivity, multiplier);
            }
        }
    }
}