using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AmbientRotator
{
    public class AmbientGroup : MonoBehaviour
    {
        [Header("Group Settings")]
        [Tooltip("The AmbientRotator objects this group controls together. Auto-populated on Start if Auto Find Members is enabled below.")]
        [SerializeField] private List<AmbientRotator> members = new List<AmbientRotator>();

        [Tooltip("Automatically finds every AmbientRotator in this object's children on Start, replacing the Members list above with what it finds.")]
        [SerializeField] private bool autoFindMembers = true;

        [Tooltip("Random phase stagger applied to each member's motion when Sync Phase (below) is off, in degrees.\n" +
                 "0 = no stagger, all members effectively in sync\n" +
                 "20 = subtle, natural-looking variation\n" +
                 "90+ = very staggered, members clearly out of step with each other")]
        [SerializeField, Range(0f, 180f)] private float spread = 20f;

        [Tooltip("When enabled, ApplyWindToAll() pushes wind force into every member. Only has an effect if you call ApplyWindToAll() yourself, e.g. from a wind trigger or gameplay event - it isn't called automatically.")]
        [SerializeField] private bool inheritWind = true;

        [Tooltip("When enabled, all members share the exact same motion phase (Spread above is ignored). When disabled, each member gets a random phase offset (within Spread) for a more organic, less uniform look.")]
        [SerializeField] private bool syncPhase = false;

        [Header("Master Controls")]
        [Tooltip("Overall motion amplitude applied to every member. 0 = motionless, 1 = normal, 2 = double intensity.")]
        [SerializeField, Range(0f, 2f)] private float masterIntensity = 1f;

        [Tooltip("Overall motion speed applied to every member. 1 = normal speed, 2 = double speed.")]
        [SerializeField, Range(0f, 2f)] private float masterSpeed = 1f;

        [Header("Group Behavior")]
        [Tooltip("Enable a rippling wave effect that travels through the group over time, based on each member's order in the Members list (index 0 leads).")]
        [SerializeField] private bool waveMotion = false;

        [Tooltip("How fast the ripple travels through the group.")]
        [SerializeField] private float waveSpeed = 0.5f;

        [Tooltip("How strong the ripple's push is on each member.")]
        [SerializeField] private float waveAmplitude = 0.3f;

        [Header("Events")]
        [Tooltip("Invoked when the group starts or resumes motion (StartAll or InitializeGroup).")]
        public UnityEngine.Events.UnityEvent OnGroupStart;

        [Tooltip("Invoked when the group's motion is stopped (StopAll).")]
        public UnityEngine.Events.UnityEvent OnGroupStop;
        
        private void Start()
        {
            if (autoFindMembers)
            {
                AutoFindMembers();
            }
            
            InitializeGroup();
        }
        
        private void Update()
        {
            if (waveMotion)
            {
                ApplyWaveMotion();
            }
        }
        
        public void AddMember(AmbientRotator member)
        {
            if (!members.Contains(member))
            {
                members.Add(member);
                InitializeMember(member);
            }
        }
        
        public void RemoveMember(AmbientRotator member)
        {
            if (members.Contains(member))
            {
                members.Remove(member);
            }
        }
        
        public void AutoFindMembers()
        {
            var found = GetComponentsInChildren<AmbientRotator>();
            members = found.ToList();
            
            var self = GetComponent<AmbientRotator>();
            if (self != null && members.Contains(self))
            {
                members.Remove(self);
            }
        }
        
        public void InitializeGroup()
        {
            foreach (var member in members)
            {
                InitializeMember(member);
            }
            
            OnGroupStart?.Invoke();
        }
        
        private void InitializeMember(AmbientRotator member)
        {
            if (member == null) return;
            
            member.SetIntensity(masterIntensity);
            member.SetSpeed(masterSpeed);

            // This was previously dead - "offset" was computed but never applied anywhere.
            if (!syncPhase)
            {
                member.PhaseOffset = Random.Range(-spread, spread);
            }

            // Previously only called inside the "!syncPhase" branch above, so members were never
            // started when Sync Phase was enabled unless their own Auto Start handled it.
            member.StartMotion();
        }
        
        public void ApplyWindToAll(Vector3 windDirection)
        {
            foreach (var member in members)
            {
                if (member != null && inheritWind)
                {
                    member.ApplyForce(windDirection * 0.1f);
                }
            }
        }
        
        public void SetAllProfiles(MotionProfile profile)
        {
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.SetProfile(profile);
                }
            }
        }
        
        public void SetMasterIntensity(float intensity)
        {
            masterIntensity = Mathf.Clamp(intensity, 0f, 2f);
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.SetIntensity(masterIntensity);
                }
            }
        }
        
        public void SetMasterSpeed(float speed)
        {
            masterSpeed = Mathf.Clamp(speed, 0.1f, 2f);
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.SetSpeed(masterSpeed);
                }
            }
        }
        
        public void PauseAll()
        {
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.PauseMotion();
                }
            }
        }
        
        public void ResumeAll()
        {
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.ResumeMotion();
                }
            }
        }
        
        public void StopAll()
        {
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.StopMotion();
                }
            }
            OnGroupStop?.Invoke();
        }
        
        public void StartAll()
        {
            foreach (var member in members)
            {
                if (member != null)
                {
                    member.StartMotion();
                }
            }
            OnGroupStart?.Invoke();
        }
        
        private void ApplyWaveMotion()
        {
            float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmplitude;
            
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] != null)
                {
                    float memberWave = wave * (1f + (i / (float)members.Count) * 0.5f);
                    members[i].ApplyForce(Vector3.up * memberWave * 0.1f);
                }
            }
        }
        
        public List<AmbientRotator> GetMembers()
        {
            return members;
        }
        
        public int MemberCount => members.Count;
        
        private void OnDrawGizmosSelected()
        {
            if (members.Count == 0) return;
            
            Gizmos.color = Color.yellow;
            Vector3 center = Vector3.zero;
            foreach (var member in members)
            {
                if (member != null)
                {
                    center += member.transform.position;
                }
            }
            center /= members.Count;
            
            Gizmos.DrawWireSphere(center, 0.5f);
            
            Gizmos.color = Color.cyan;
            for (int i = 0; i < members.Count - 1; i++)
            {
                if (members[i] != null && members[i + 1] != null)
                {
                    Gizmos.DrawLine(members[i].transform.position, members[i + 1].transform.position);
                }
            }
        }
    }
}