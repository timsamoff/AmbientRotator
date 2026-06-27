using UnityEngine;
using System.Collections;

namespace AmbientRotator
{
    public class BeatSyncModule : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private float beatReactionIntensity = 2f;
        [SerializeField] private float beatSmoothing = 0.5f;
        
        [Header("Beat Detection")]
        [SerializeField] private float minBeatThreshold = 0.1f;
        [SerializeField] private float maxBeatThreshold = 0.5f;
        [SerializeField] private int spectrumSamples = 64;
        [SerializeField] private float[] frequencyBands = { 0f, 0.1f, 0.2f, 0.4f };
        
        [Header("Reaction Types")]
        [SerializeField] private bool pulse = true;
        [SerializeField] private bool rotate = false;
        [SerializeField] private bool wobble = false;
        [SerializeField] private float wobbleFrequency = 2f;
        
        private AmbientRotator parentRotator;
        private float[] spectrumData;
        private float currentBeatStrength;
        private float smoothedBeat;
        private float lastBeatTime;
        private float beatCooldown = 0.1f;
        
        private void Start()
        {
            parentRotator = GetComponent<AmbientRotator>();
            spectrumData = new float[spectrumSamples];
            
            if (musicSource == null)
            {
                // Fix: Use FindAnyObjectByType instead of FindObjectOfType
                musicSource = FindAnyObjectByType<AudioSource>();
                if (musicSource == null)
                {
                    Debug.LogWarning("BeatSyncModule: No AudioSource found!");
                }
            }
        }
        
        private void Update()
        {
            if (musicSource == null || !musicSource.isPlaying) return;
            
            musicSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);
            
            float beatStrength = DetectBeat();
            smoothedBeat = Mathf.Lerp(smoothedBeat, beatStrength, Time.deltaTime / beatSmoothing);
            currentBeatStrength = smoothedBeat;
            
            if (currentBeatStrength > 0.2f && Time.time - lastBeatTime > beatCooldown)
            {
                OnBeatDetected(currentBeatStrength);
                lastBeatTime = Time.time;
            }
        }
        
        private float DetectBeat()
        {
            float average = 0f;
            for (int i = 0; i < spectrumSamples; i++)
            {
                average += spectrumData[i];
            }
            average /= spectrumSamples;
            
            return Mathf.InverseLerp(minBeatThreshold, maxBeatThreshold, average);
        }
        
        private void OnBeatDetected(float strength)
        {
            if (parentRotator == null) return;
            
            if (pulse)
            {
                Vector3 pulseForce = Vector3.up * strength * beatReactionIntensity;
                parentRotator.ApplyForce(pulseForce);
            }
            
            if (rotate)
            {
                float rotationForce = Random.Range(-1f, 1f) * strength * beatReactionIntensity;
                parentRotator.ApplyForce(Vector3.up * rotationForce);
            }
            
            if (wobble)
            {
                Vector3 wobbleForce = new Vector3(
                    Mathf.Sin(Time.time * wobbleFrequency),
                    0,
                    Mathf.Cos(Time.time * wobbleFrequency * 0.7f)
                ) * strength * beatReactionIntensity * 0.5f;
                parentRotator.ApplyForce(wobbleForce);
            }
        }
        
        public void SetAudioSource(AudioSource source)
        {
            musicSource = source;
        }
        
        public void SetBeatIntensity(float intensity)
        {
            beatReactionIntensity = Mathf.Max(0, intensity);
        }
        
        public float GetCurrentBeatStrength()
        {
            return currentBeatStrength;
        }
        
        public bool IsBeatDetected()
        {
            return currentBeatStrength > 0.2f;
        }
    }
}