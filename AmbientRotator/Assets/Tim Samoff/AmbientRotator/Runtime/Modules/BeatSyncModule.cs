using UnityEngine;
using System.Collections;

namespace AmbientRotator
{
    public class BeatSyncModule : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField]
        [Tooltip("The AudioSource component that is playing your music. Drag a GameObject with an AudioSource here, or leave empty to auto-find one.")]
        private AudioSource musicSource;

        [Header("Reaction Intensity")]
        [SerializeField]
        [Range(0f, 100f)]
        [Tooltip("How strongly the object reacts to each beat. Start with 10-20 for visible pulses. Higher values = more dramatic motion.")]
        private float beatReactionIntensity = 10f;

        [SerializeField]
        [Range(0.001f, 2f)]
        [Tooltip("How smoothly the beat detection transitions. Lower values (0.01-0.1) = snappier, more responsive. Higher values (0.5-2) = smoother, softer motion.")]
        private float beatSmoothing = 0.02f;

        [Header("Beat Detection")]
        [SerializeField]
        [Tooltip("Minimum audio level to register as a beat. Lower = more sensitive (catches quiet beats). Higher = less sensitive (only catches loud beats). Start with 0.01-0.05.")]
        private float minBeatThreshold = 0.01f;

        [SerializeField]
        [Tooltip("Maximum audio level for a beat. Used to normalize beat strength. Higher = requires louder audio to reach full reaction strength.")]
        private float maxBeatThreshold = 0.5f;

        [SerializeField]
        [Tooltip("Number of audio samples to analyze per frame. Higher values (128-256) = more accurate but slower. Lower values (16-64) = faster but less accurate. Must be a power of 2.")]
        private int spectrumSamples = 64;

        [SerializeField]
        [Tooltip("Minimum time between beat triggers. Prevents double-triggering on the same beat. Lower = more responsive. Higher = prevents rapid triggering.")]
        private float beatCooldown = 0.05f;

        [Header("Reaction Types")]
        [SerializeField]
        [Tooltip("Object pulses upward on each beat. This creates a 'bounce' or 'jump' effect.")]
        private bool pulse = true;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How high the object jumps on each beat. 0 = no movement, 0.5 = moderate jump, 1 = large jump. Works best with Beat Reaction Intensity above 10.")]
        private float pulseHeight = 0.2f;

        [SerializeField]
        [Tooltip("Object rotates on each beat. This creates a 'spin' or 'twist' effect.")]
        private bool rotate = false;

        [SerializeField]
        [Range(-10f, 10f)]
        [Tooltip("How strongly the object rotates on each beat. Positive = clockwise spin, Negative = counter-clockwise spin. Higher values = faster spin.")]
        private float rotationForceMultiplier = 1f;

        [SerializeField]
        [Range(0.01f, 0.5f)]
        [Tooltip("How smooth the rotation is. Lower values (0.01) = smoother, more fluid. Higher values (0.5) = jerkier, more snappy.")]
        private float rotationSmoothness = 0.1f;

        [SerializeField]
        [Tooltip("Object wobbles side to side on each beat. This creates a 'sway' or 'rocking' effect.")]
        private bool wobble = false;

        [SerializeField]
        [Range(0.1f, 10f)]
        [Tooltip("Speed of the wobble motion. Higher = faster wobble. Lower = slower, more gentle sway.")]
        private float wobbleFrequency = 2f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How far the object wobbles on each beat. 0 = no wobble, 0.5 = moderate, 1 = large wobble.")]
        private float wobbleAmplitude = 0.2f;

        private AmbientRotator parentRotator;
        private float[] spectrumData;
        private float currentBeatStrength;
        private float smoothedBeat;
        private float lastBeatTime;
        private bool hasWarned = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float currentRotationVelocity = 0f;

        private void Start()
        {
            parentRotator = GetComponent<AmbientRotator>();
            spectrumData = new float[spectrumSamples];
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            if (musicSource == null)
            {
                musicSource = FindAnyObjectByType<AudioSource>();
                if (musicSource == null)
                {
                    Debug.LogError("BeatSyncModule: No AudioSource found in scene! Please assign one manually.");
                    hasWarned = true;
                    return;
                }
            }

            if (musicSource.clip == null)
            {
                Debug.LogError($"BeatSyncModule: AudioSource on '{musicSource.gameObject.name}' has no AudioClip assigned!");
                hasWarned = true;
                return;
            }

            if (!musicSource.isPlaying && !musicSource.playOnAwake)
            {
                Debug.LogWarning($"BeatSyncModule: AudioSource on '{musicSource.gameObject.name}' is not playing. Call musicSource.Play() or enable Play On Awake.");
            }

            Debug.Log($"BeatSyncModule initialized with: {musicSource.clip.name} on {musicSource.gameObject.name}");
        }

        private void Update()
        {
            if (musicSource == null)
            {
                if (!hasWarned)
                {
                    Debug.LogError("BeatSyncModule: No AudioSource assigned!");
                    hasWarned = true;
                }
                return;
            }

            if (!musicSource.isPlaying)
            {
                return;
            }

            musicSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);

            float beatStrength = DetectBeat();
            smoothedBeat = Mathf.Lerp(smoothedBeat, beatStrength, Time.deltaTime / beatSmoothing);
            currentBeatStrength = smoothedBeat;

            if (currentBeatStrength > 0.2f && Time.time - lastBeatTime > beatCooldown)
            {
                OnBeatDetected(currentBeatStrength);
                lastBeatTime = Time.time;
            }

            // Smoothly return position if pulse is enabled
            if (pulse)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 5f);
            }

            // Smoothly return rotation if rotate is enabled
            if (rotate)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 5f);
            }
        }

        private float DetectBeat()
        {
            float average = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                average += spectrumData[i];
            }
            average /= spectrumData.Length;

            return Mathf.InverseLerp(minBeatThreshold, maxBeatThreshold, average);
        }

        private void OnBeatDetected(float strength)
        {
            if (parentRotator == null)
            {
                Debug.LogWarning("BeatSyncModule: No AmbientRotator component found on this GameObject!");
                return;
            }

            if (pulse)
            {
                Vector3 pulseForce = Vector3.up * strength * beatReactionIntensity;
                parentRotator.ApplyForce(pulseForce);

                Vector3 pulseOffset = Vector3.up * strength * pulseHeight;
                transform.position += pulseOffset;
            }

            if (rotate)
            {
                // --- Calculate target rotation amount ---
                float targetRotation = strength * beatReactionIntensity * rotationForceMultiplier * 0.1f;
                
                // --- Smooth the rotation ---
                currentRotationVelocity = Mathf.Lerp(currentRotationVelocity, targetRotation, Time.deltaTime / rotationSmoothness);
                
                // Apply the smoothed rotation
                transform.Rotate(Vector3.up, currentRotationVelocity);
            }

            if (wobble)
            {
                float wobbleOffsetAmount = strength * beatReactionIntensity * wobbleAmplitude * 0.01f;
                Vector3 wobbleOffset = new Vector3(
                    Mathf.Sin(Time.time * wobbleFrequency) * wobbleOffsetAmount,
                    0,
                    Mathf.Cos(Time.time * wobbleFrequency * 0.7f) * wobbleOffsetAmount
                );
                transform.position += wobbleOffset;
            }
        }

        public void SetAudioSource(AudioSource source)
        {
            musicSource = source;
            hasWarned = false;

            if (source != null && source.clip != null)
            {
                Debug.Log($"BeatSyncModule: AudioSource set to '{source.gameObject.name}' with clip '{source.clip.name}'");
            }
        }

        public void SetBeatIntensity(float intensity)
        {
            beatReactionIntensity = Mathf.Clamp(intensity, 0f, 100f);
        }

        public float GetCurrentBeatStrength()
        {
            return currentBeatStrength;
        }

        public bool IsBeatDetected()
        {
            return currentBeatStrength > 0.2f;
        }

        private void OnValidate()
        {
            beatReactionIntensity = Mathf.Clamp(beatReactionIntensity, 0f, 100f);
            beatSmoothing = Mathf.Clamp(beatSmoothing, 0.001f, 2f);
            minBeatThreshold = Mathf.Clamp(minBeatThreshold, 0f, 1f);
            maxBeatThreshold = Mathf.Clamp(maxBeatThreshold, 0f, 1f);
            spectrumSamples = Mathf.Clamp(spectrumSamples, 16, 512);
            beatCooldown = Mathf.Clamp(beatCooldown, 0.001f, 0.5f);
            wobbleFrequency = Mathf.Clamp(wobbleFrequency, 0.1f, 10f);
            rotationForceMultiplier = Mathf.Clamp(rotationForceMultiplier, -10f, 10f);
            pulseHeight = Mathf.Clamp(pulseHeight, 0f, 1f);
            wobbleAmplitude = Mathf.Clamp(wobbleAmplitude, 0f, 1f);
            rotationSmoothness = Mathf.Clamp(rotationSmoothness, 0.01f, 0.5f);

            if (minBeatThreshold > maxBeatThreshold)
            {
                maxBeatThreshold = minBeatThreshold + 0.1f;
            }

            if (!IsPowerOfTwo(spectrumSamples))
            {
                spectrumSamples = NextPowerOfTwo(spectrumSamples);
            }

            if (spectrumData == null || spectrumData.Length != spectrumSamples)
            {
                spectrumData = new float[spectrumSamples];
            }
        }

        private bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

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