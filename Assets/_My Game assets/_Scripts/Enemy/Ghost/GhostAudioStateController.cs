using UnityEngine;

public class GhostAudioStateController : MonoBehaviour
{
    [Header("Components")]
    public AudioReverbZone reverbZone;
    public AudioSource[] ghostVoices; // Drag your 2 Audio Sources here

    [System.Serializable]
    public struct GhostAudioProfile
    {
        [Header("Reverb Zone Settings")]
        public float minDistance;
        public float maxDistance;
        public float roomLevel;     // -10000 to 0 (Volume of reverb)
        public float decayTime;     // Duration of echo
        public float reflections;   // Metallic ringing (-10000 to 1000)

        [Header("Voice Settings")]
        public float pitch;         // Speed of sound (1 = normal, 1.2 = fast/screaming)
        public float volume;        // Loudness
        public float voiceMaxDist;  // How far the roar travels
    }

    [Header("Profiles")]
    public GhostAudioProfile normalState;
    public GhostAudioProfile huntingState;

    [Header("Debug / Status")]
    public bool isHunting = false;
    private float transitionSpeed = 2.0f; // How fast the sound morphs (seconds)
    private float currentLerp = 0f;
    public GhostAI GhostAI;

    void Start()
    {
        if (GhostAI == null)
        {
            GhostAI = GameObject.FindAnyObjectByType<GhostAI>();
        }
        // Initialize current lerp based on starting state
        currentLerp = isHunting ? 1f : 0f;
    }

    float timer = 0.0876543f;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.3f)
        {
            timer = 0;
        }
        else
        {
            return;
        }

        isHunting = GhostAI.isHunting;
        // 1. Smoothly calculate the "Blend" value (0 = Normal, 1 = Hunting)
        float target = isHunting ? 1f : 0f;
        currentLerp = Mathf.MoveTowards(currentLerp, target, Time.deltaTime * transitionSpeed);

        // 2. Apply the blended values
        ApplySettings(currentLerp);
    }

    void ApplySettings(float t)
    {
        // Lerp Reverb Zone
        reverbZone.minDistance = Mathf.Lerp(normalState.minDistance, huntingState.minDistance, t);
        reverbZone.maxDistance = Mathf.Lerp(normalState.maxDistance, huntingState.maxDistance, t);
        reverbZone.room = Mathf.RoundToInt(Mathf.Lerp(normalState.roomLevel, huntingState.roomLevel, t));
        reverbZone.decayTime = Mathf.Lerp(normalState.decayTime, huntingState.decayTime, t);
        reverbZone.reflections = Mathf.RoundToInt(Mathf.Lerp(normalState.reflections, huntingState.reflections, t));

        // Lerp Audio Sources (Speed, Volume, Range)
        foreach (var audioSrc in ghostVoices)
        {
            audioSrc.pitch = Mathf.Lerp(normalState.pitch, huntingState.pitch, t);
            audioSrc.volume = Mathf.Lerp(normalState.volume, huntingState.volume, t);
            audioSrc.maxDistance = Mathf.Lerp(normalState.voiceMaxDist, huntingState.voiceMaxDist, t);
        }
    }

    // Call this from your AI script
    public void SetHunting(bool hunting)
    {
        isHunting = hunting;
    }
}