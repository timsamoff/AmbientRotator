using System.Collections.Generic;
using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach this to a trigger object. Defines how nearby ReactiveTriggerModule objects should react.
    /// Push/Attract live on the shared ReactionConfig (see Reaction) so BeatSyncObject gets the same
    /// fields automatically - there is only one place these are defined.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Reactive Trigger Object")]
    public class ReactiveTriggerObject : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [Tooltip("The radius of the trigger sphere. Reaction strength is 1.0 at the center and fades to 0 at this distance.")]
        [SerializeField] private float reactionRadius = 5f;

        [Tooltip("Which layers can be affected by this trigger. Leave as 'Everything' to affect all objects.")]
        [SerializeField] private LayerMask affectedLayers = ~0;

        [Header("Reaction Settings")]
        [Tooltip("How this trigger makes objects react. Includes Pulse, Rotate, Wobble, and Push/Attract.")]
        [SerializeField] private ReactionConfig reaction = new ReactionConfig();

        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.red;

        // Self-registering list of active sources. ReactiveTriggerModule reads this directly instead
        // of scanning the whole scene with FindObjectsByType every frame.
        private static readonly List<ReactiveTriggerObject> activeSources = new List<ReactiveTriggerObject>();
        public static IReadOnlyList<ReactiveTriggerObject> ActiveSources => activeSources;

        public ReactionConfig Reaction => reaction;
        public float ReactionRadius => reactionRadius;
        public LayerMask AffectedLayers => affectedLayers;
        public Vector3 Position => transform.position;

        private void OnEnable()
        {
            activeSources.Add(this);
        }

        private void OnDisable()
        {
            activeSources.Remove(this);
        }

        private void OnValidate()
        {
            if (reaction != null) reaction.Validate();
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, reactionRadius);

            if (reaction != null && (reaction.pushAway || reaction.attract))
            {
                Gizmos.color = reaction.pushAway ? Color.red : Color.green;
                Gizmos.DrawRay(transform.position, Vector3.up * reactionRadius * 0.3f);
            }
        }

        public float GetStrengthAtPosition(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            return Mathf.Clamp01(1f - (distance / reactionRadius));
        }
    }
}
