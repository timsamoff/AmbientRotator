using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AmbientRotator
{
    public class AmbientGroup : MonoBehaviour
    {
        [Header("Group Settings")]
        [SerializeField] private List<AmbientRotator> members = new List<AmbientRotator>();
        [SerializeField] private bool autoFindMembers = true;
        [SerializeField] private float spread = 0.2f;
        [SerializeField] private bool inheritWind = true;
        [SerializeField] private bool syncPhase = false;
        
        [Header("Master Controls")]
        [SerializeField, Range(0f, 2f)] private float masterIntensity = 1f;
        [SerializeField, Range(0f, 2f)] private float masterSpeed = 1f;
        
        [Header("Group Behavior")]
        [SerializeField] private bool waveMotion = false;
        [SerializeField] private float waveSpeed = 0.5f;
        [SerializeField] private float waveAmplitude = 0.3f;
        
        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnGroupStart;
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
            
            if (!syncPhase)
            {
                float offset = Random.Range(-spread, spread);
                member.StartMotion();
            }
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