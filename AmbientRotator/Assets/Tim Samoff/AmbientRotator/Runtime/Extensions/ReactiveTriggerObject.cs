using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach this to a trigger object. Defines how nearby ReactiveTriggerModule objects should react.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Reactive Trigger Object")]
    public class ReactiveTriggerObject : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [Tooltip("The radius of the trigger sphere.")]
        [SerializeField] private float reactionRadius = 5f;
        
        [Tooltip("Objects that can be affected. Leave empty to affect everything.")]
        [SerializeField] private LayerMask affectedLayers = ~0;

        [Header("Reaction Settings")]
        [Tooltip("How this trigger makes objects react.")]
        [SerializeField] private ReactionConfig reaction = new ReactionConfig();

        [Header("Push/Attract (Optional)")]
        [Tooltip("Push objects away from this trigger.")]
        [SerializeField] private bool pushAway = false;
        [SerializeField, Range(0f, 20f)] private float pushAwayStrength = 5f;
        
        [Tooltip("Pull objects toward this trigger.")]
        [SerializeField] private bool attract = false;
        [SerializeField, Range(0f, 20f)] private float attractStrength = 5f;

        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.red;

        public ReactionConfig Reaction => reaction;
        public float ReactionRadius => reactionRadius;
        public bool PushAway => pushAway;
        public float PushAwayStrength => pushAwayStrength;
        public bool Attract => attract;
        public float AttractStrength => attractStrength;
        public LayerMask AffectedLayers => affectedLayers;
        public Vector3 Position => transform.position;

        private void OnValidate()
        {
            if (reaction != null) reaction.Validate();
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, reactionRadius);
            
            // Show push/pull direction
            if (pushAway || attract)
            {
                Gizmos.color = pushAway ? Color.red : Color.green;
                Gizmos.DrawRay(transform.position, Vector3.up * reactionRadius * 0.3f);
            }
        }

        public float GetStrengthAtPosition(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            return Mathf.Clamp01(1f - (distance / reactionRadius));
        }

        public Vector3 GetDirectionTo(Vector3 targetPosition)
        {
            Vector3 dir = (targetPosition - transform.position).normalized;
            return dir.magnitude < 0.01f ? Vector3.up : dir;
        }
    }
}