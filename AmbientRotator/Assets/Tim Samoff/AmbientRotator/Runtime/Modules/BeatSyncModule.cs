using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AmbientRotator
{
    public class BeatSyncModule : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField]
        private AudioSource musicSource;

        [Header("Reaction Intensity")]
        [SerializeField]
        [Range(0f, 100f)]
        [Tooltip("How strongly the object reacts to each beat.")]
        private float beatReactionIntensity = 50f;

        [SerializeField]
        [Range(0.001f, 2f)]
        [Tooltip("How smoothly the beat detection transitions.")]
        private float beatSmoothing = 0.05f;

        [Header("Beat Detection")]
        [SerializeField]
        [Range(0f, 0.01f)]
        [Tooltip("Minimum audio level to register as a beat.")]
        private float minBeatThreshold = 0.0001f;

        [SerializeField]
        [Range(0f, 0.01f)]
        [Tooltip("Maximum audio level for a beat.")]
        private float maxBeatThreshold = 0.001f;

        [SerializeField]
        [Range(16, 512)]
        [Tooltip("Number of audio samples to analyze. Must be power of 2.")]
        private int spectrumSamples = 256;

        [Header("Reaction Configuration")]
        [SerializeField]
        [Tooltip("How the object reacts to beats.")]
        private MotionReaction reaction = new MotionReaction();

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Enable debug logging.")]
        private bool debugLogging = true;

        // --- Runtime State ---
        private AmbientRotator parentRotator;
        private float[] spectrumData;
        private float currentBeatStrength;
        private float smoothedBeat;
        private float lastBeatTime;

        // --- Public Properties ---
        public MotionReaction Reaction => reaction;
        public float CurrentBeatStrength => currentBeatStrength;

        private void Start()
        {
            parentRotator = GetComponent<AmbientRotator>();
            spectrumData = new float[spectrumSamples];

            if (parentRotator == null)
            {
                Debug.LogError($"BeatSyncModule: No AmbientRotator component found on {gameObject.name}!");
                return;
            }

            reaction.Validate();
            reaction.ResetSmoothing();

            if (musicSource == null)
            {
                musicSource = FindAnyObjectByType<AudioSource>();
                if (musicSource == null)
                {
                    Debug.LogError("BeatSyncModule: No AudioSource found in scene!");
                    return;
                }
            }

            if (musicSource.clip == null)
            {
                Debug.LogError($"BeatSyncModule: AudioSource on '{musicSource.gameObject.name}' has no AudioClip!");
                return;
            }

            if (!musicSource.isPlaying && !musicSource.playOnAwake)
            {
                Debug.LogWarning($"BeatSyncModule: AudioSource on '{musicSource.gameObject.name}' is not playing.");
                musicSource.Play();
            }

            if (debugLogging)
            {
                Debug.Log($"=== BeatSyncModule Initialized ===");
                Debug.Log($"Audio: {musicSource.clip.name}");
                Debug.Log($"Beat Intensity: {beatReactionIntensity}");
                Debug.Log($"Pulse: {reaction.pulse}, Height: {reaction.pulseHeight}, Intensity: {reaction.pulseIntensity}");
                Debug.Log($"Rotate: {reaction.rotate}, Max Angle: {reaction.maxRotationAngle}°, Intensity: {reaction.rotationIntensity}");
                Debug.Log($"Wobble: {reaction.wobble}, Amount: {reaction.wobbleAmount}, Intensity: {reaction.wobbleIntensity}, Freq: {reaction.wobbleFrequency}");
            }
        }

        private void Update()
        {
            // If no reactions are active, skip all processing
            if (!reaction.IsAnyReactionActive())
            {
                currentBeatStrength = 0f;
                smoothedBeat = 0f;
                lastBeatTime = 0f;
                return;
            }

            if (parentRotator == null || musicSource == null || !musicSource.isPlaying)
            {
                return;
            }

            musicSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);

            float rawAverage = GetRawAverage();
            float beatStrength = DetectBeat(rawAverage);
            smoothedBeat = Mathf.Lerp(smoothedBeat, beatStrength, Time.deltaTime / beatSmoothing);
            currentBeatStrength = smoothedBeat;

            // Trigger on beat
            if (currentBeatStrength > 0.2f && Time.time - lastBeatTime > reaction.cooldown)
            {
                if (debugLogging)
                {
                    Debug.Log($"🎵 BEAT! Strength: {currentBeatStrength:F2}, Raw: {rawAverage:F6}");
                }
                OnBeatDetected(currentBeatStrength);
                lastBeatTime = Time.time;
            }
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

        private void OnBeatDetected(float strength)
        {
            if (parentRotator == null) return;
            if (!reaction.IsAnyReactionActive()) return;

            // Scale strength by intensity (0-100 maps to 0-10)
            float scaledStrength = strength * (beatReactionIntensity / 10f);
            
            if (debugLogging)
            {
                Debug.Log($"  Applying reaction with scaled strength: {scaledStrength:F2}");
                Debug.Log($"  Pulse: {reaction.pulse}, Rotate: {reaction.rotate}, Wobble: {reaction.wobble}");
            }
            
            // Apply the reaction as offsets to AmbientRotator
            reaction.ApplyImpulse(parentRotator, scaledStrength);
        }

        public void SetAudioSource(AudioSource source) => musicSource = source;
        public void SetBeatIntensity(float intensity) => beatReactionIntensity = Mathf.Clamp(intensity, 0f, 100f);
        public bool IsBeatDetected() => currentBeatStrength > 0.2f;
        
        public void SetReaction(MotionReaction newReaction)
        {
            if (newReaction != null)
            {
                reaction = newReaction.Clone();
                reaction.Validate();
                reaction.ResetSmoothing();
            }
        }

        private void OnValidate()
        {
            beatReactionIntensity = Mathf.Clamp(beatReactionIntensity, 0f, 100f);
            beatSmoothing = Mathf.Clamp(beatSmoothing, 0.001f, 2f);
            minBeatThreshold = Mathf.Clamp(minBeatThreshold, 0f, 0.01f);
            maxBeatThreshold = Mathf.Clamp(maxBeatThreshold, 0f, 0.01f);
            spectrumSamples = Mathf.Clamp(spectrumSamples, 16, 512);

            if (minBeatThreshold > maxBeatThreshold)
            {
                maxBeatThreshold = minBeatThreshold + 0.0001f;
            }

            if (!IsPowerOfTwo(spectrumSamples))
            {
                spectrumSamples = NextPowerOfTwo(spectrumSamples);
            }

            if (spectrumData == null || spectrumData.Length != spectrumSamples)
            {
                spectrumData = new float[spectrumSamples];
            }

            if (reaction != null)
            {
                reaction.Validate();
            }
        }

        private bool IsPowerOfTwo(int x) => (x & (x - 1)) == 0;

        private int NextPowerOfTwo(int x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x++;
            return x;
        }
    }
}