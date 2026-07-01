using System.Collections.Generic;
using UnityEngine;

namespace AmbientRotator
{
    /// <summary>
    /// Attach this to an AudioSource. Defines how nearby BeatSyncModule objects should react.
    /// </summary>
    [AddComponentMenu("Ambient Rotator/Beat Sync Object")]
    public class BeatSyncObject : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource musicSource;

        [Header("Reaction Settings")]
        [Tooltip("How this audio source makes objects react.")]
        [SerializeField] private ReactionConfig reaction = new ReactionConfig();

        [Header("Beat Detection")]
        [SerializeField, Range(0f, 0.01f)] private float minBeatThreshold = 0.0001f;
        [SerializeField, Range(0f, 0.01f)] private float maxBeatThreshold = 0.001f;
        [SerializeField, Range(16, 512)] private int spectrumSamples = 256;
        [SerializeField, Range(0.001f, 2f)] private float beatSmoothing = 0.05f;

        [Header("Influence")]
        [Tooltip("How far this audio source affects objects.")]
        [SerializeField, Range(0f, 50f)] private float influenceRadius = 10f;

        [Header("Debug")]
        [SerializeField] private bool showDebugRadius = true;
        [SerializeField] private Color debugColor = Color.blue;

        // --- Runtime ---
        private float[] spectrumData;
        private float currentBeatStrength;
        private float smoothedBeat;
        private float lastBeatTime;
        private bool isInitialized = false;

        // Self-registering list of active sources. BeatSyncModule reads this directly instead of
        // scanning the whole scene with FindObjectsByType every frame.
        private static readonly List<BeatSyncObject> activeSources = new List<BeatSyncObject>();
        public static IReadOnlyList<BeatSyncObject> ActiveSources => activeSources;

        public ReactionConfig Reaction => reaction;
        public float CurrentBeatStrength => currentBeatStrength;
        public float InfluenceRadius => influenceRadius;
        public Vector3 Position => transform.position;

        private void OnEnable()
        {
            activeSources.Add(this);
        }

        private void OnDisable()
        {
            activeSources.Remove(this);
        }

        private void Start()
        {
            spectrumData = new float[spectrumSamples];
            reaction.Validate();

            if (musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
                if (musicSource == null)
                {
                    Debug.LogError($"BeatSyncObject: No AudioSource found on {gameObject.name}!");
                    return;
                }
            }

            if (musicSource.clip == null)
            {
                Debug.LogError($"BeatSyncObject: AudioSource on '{musicSource.gameObject.name}' has no AudioClip!");
                return;
            }

            if (!musicSource.isPlaying && !musicSource.playOnAwake)
            {
                musicSource.Play();
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || musicSource == null || !musicSource.isPlaying) return;

            musicSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);

            float rawAverage = GetRawAverage();
            float beatStrength = DetectBeat(rawAverage);
            smoothedBeat = Mathf.Lerp(smoothedBeat, beatStrength, Time.deltaTime / beatSmoothing);
            currentBeatStrength = smoothedBeat;
        }

        private float GetRawAverage()
        {
            float sum = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                sum += spectrumData[i];
            }
            return sum / spectrumData.Length;
        }

        private float DetectBeat(float rawAverage)
        {
            return Mathf.InverseLerp(minBeatThreshold, maxBeatThreshold, rawAverage);
        }

        public bool IsBeatDetected()
        {
            return currentBeatStrength > 0.2f && Time.time - lastBeatTime > reaction.cooldown;
        }

        public void ConsumeBeat()
        {
            lastBeatTime = Time.time;
        }

        public float GetStrengthAtPosition(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            return Mathf.Clamp01(1f - (distance / influenceRadius));
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugRadius) return;
            Gizmos.color = debugColor;
            Gizmos.DrawWireSphere(transform.position, influenceRadius);
        }
    }
}