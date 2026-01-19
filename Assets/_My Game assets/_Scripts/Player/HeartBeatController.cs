using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioReverbFilter))]
public class HeartbeatController : MonoBehaviour
{
    [Header("References")]
    public FearMeter fearMeter; // Drag your Player/FearMeter here
    public GhostAI ghostAI;

    [Header("Heartbeat Settings")]
    [Range(0f, 3f)] public float minPitch = 0.8f;   // Resting heart rate (approx 60bpm)
    [Range(0f, 3f)] public float maxPitch = 1.8f;   // Panic heart rate (approx 140bpm)
    [Range(0f, 1f)] public float maxVolume = 1.0f; 

    [Header("Panic Thresholds")]
    [Tooltip("If current Fear is 40% but Ghost is looking, we boost intensity to this value.")]
    public float ghostLookingIntensityOverride = 0.85f; 
    public float dollLookingIntensityOverride = 0.6f;

    // Internal Variables
    private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;
    private AudioReverbFilter reverbFilter;
    private float currentIntensity = 0f;

    void Start()
    {
        if (ghostAI == null)
        {
            ghostAI = GameObject.FindAnyObjectByType<GhostAI>();
        }
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        reverbFilter = GetComponent<AudioReverbFilter>();

        // Start playing immediately, volume is controlled in Update
        if (!audioSource.isPlaying) audioSource.Play();
    }

    float timer = 0.432432f;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.2f)
        {
            timer = 0;
        }else
        {
            return;
        }
        if (fearMeter == null) return;

        // 1. Calculate the Base Intensity (0.0 to 0.5)
        float normalizedFear = fearMeter.fearValue / 100f * 0.5f;

        // 2. Calculate "Rate-Based" Panic (The "How fast is it increasing" factor)
        // If the ghost is looking, the fear rate is massive, so we override the intensity.
        float situationalPanic = 0f;

        if (fearMeter.isGhostLooking && ghostAI.isHunting)
        {
            situationalPanic = ghostLookingIntensityOverride;
        }
        else if (fearMeter.isLookingDoll || fearMeter.isLookingGhost || fearMeter.isGhostLooking)
        {
            situationalPanic = dollLookingIntensityOverride;
        }
        
        situationalPanic = situationalPanic + normalizedFear/2;

        // 3. Determine Final Target Intensity
        // We take the HIGHER of the two: actual fear OR situational panic
        float targetIntensity = Mathf.Max(normalizedFear, situationalPanic);

        // Smoothly lerp to the new intensity to prevent jarring sound jumps
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 3f);

        ApplyAudioEffects(currentIntensity);
    }

    void ApplyAudioEffects(float intensity)
    {
        // --- VOLUME ---
        // Heartbeat is silent at 0 fear, audible at 20%, loud at 100%
        // We use an animation curve logic: x*x makes it ramp up slower initially
        audioSource.volume = Mathf.Clamp01(intensity * intensity) * maxVolume;

        // --- PITCH (Speed) ---
        // Simple linear interpolation between resting and panic speed
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, intensity);

        // --- LOW PASS FILTER (The "Muffled" Effect) ---
        // Low Intensity = Muffled (Inside chest) -> 1000Hz
        // High Intensity = Clear (Pounding in ears) -> 22000Hz
        lowPassFilter.cutoffFrequency = Mathf.Lerp(1000f, 22000f, intensity);

        // --- REVERB (The "Insanity/Echo" Effect) ---
        // Only kick in reverb when fear is VERY high (> 70%)
        // This makes the player feel like they are disassociating
        if (intensity > 0.7f)
        {
            // Map 0.7-1.0 intensity to -1000 (audible) to 200 (loud) reverb
            float reverbVal = Mathf.Lerp(-1000f, 200f, (intensity - 0.5f) * 2.0f);
            reverbFilter.reverbLevel = reverbVal;
        }
        else
        {
            // Mute reverb completely if not panicked
            reverbFilter.reverbLevel = -10000f; 
        }
    }
}