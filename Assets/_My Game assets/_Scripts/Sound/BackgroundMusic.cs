using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Layer 1: The Constant Base")]
    public AudioSource baseDroneSource; // Drag a source with your "Base Music" here (Loop = true)

    [Header("Layer 2: The Shifting Horror Track")]
    public AudioSource musicSource; // Drag an empty AudioSource here (Loop = true)
    public AudioClip horrorTrackClip;
    public float musicShiftInterval = 30f; // Time between music shifts
    public float maxVolume = 1.0f;

    [Header("Layer 3: The Shifting Ambiance")]
    public AudioSource ambianceSource; // Drag ANOTHER empty AudioSource here (Loop = true)
    public AudioClip ambianceTrackClip;
    public float ambianceShiftInterval = 25f; // Different time so they don't sync up!

    [Header("Layer 4: The Stingers (Screams/Insects)")]
    public AudioSource stingerSource; // Drag an empty AudioSource here (Loop = false)
    public AudioClip[] insectClips;
    public AudioClip[] screamClips;
    public float stingerRateMin = 5f;
    public float stingerRateMax = 15f;

    [Header("Global Settings")]
    public float transitionTime = 3f; // How long the fade takes

    private void Start()
    {
        // 1. Start Base Layer
        if (!baseDroneSource.isPlaying) baseDroneSource.Play();

        // 2. Start Horror Music Layer
        if (horrorTrackClip != null)
        {
            musicSource.clip = horrorTrackClip;
            musicSource.loop = true;
            musicSource.Play();
            // Start the shifter for Music
            StartCoroutine(AudioShifterRoutine(musicSource, musicShiftInterval, 0.85f, 1.15f, maxVolume));
        }

        // 3. Start Ambiance Layer
        if (ambianceTrackClip != null)
        {
            ambianceSource.clip = ambianceTrackClip;
            ambianceSource.loop = true;
            ambianceSource.Play();
            // Start the shifter for Ambiance (Notice the different pitch range)
            StartCoroutine(AudioShifterRoutine(ambianceSource, ambianceShiftInterval, 0.7f, 1.3f));
        }

        // 4. Start Random Stingers
        StartCoroutine(RandomStingerRoutine());
    }

    // --- GENERIC SHIFTER (Works for both Music and Ambiance) ---
    // This single function now handles both layers independently!
    IEnumerator AudioShifterRoutine(AudioSource source, float interval, float minPitch, float maxPitch, float maxVolume = 0.7f)
    {
        while (true)
        {

            // Calculate new random settings
            float newPitch = Random.Range(minPitch, maxPitch);
            float newVolume = Random.Range(0.05f, maxVolume);
            float newPan = Random.Range(-0.5f, 0.5f);

            // Smoothly Transition
            float timer = 0;
            float startPitch = source.pitch;
            float startVol = source.volume;
            float startPan = source.panStereo;

            while (timer < transitionTime)
            {
                timer += Time.deltaTime;
                float t = timer / transitionTime;

                // Lerp towards new settings
                source.pitch = Mathf.Lerp(startPitch, newPitch, t);
                source.volume = Mathf.Lerp(startVol, newVolume, t);
                source.panStereo = Mathf.Lerp(startPan, newPan, t);

                yield return null;
            }


            // Wait for X seconds (add small random variance so they don't loop perfectly)
            float waitTime = interval + Random.Range(-5f, 5f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // --- LOGIC FOR RANDOM SCREAMS / INSECTS ---
    IEnumerator RandomStingerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(stingerRateMin, stingerRateMax));

            float random = Random.value;
            bool isScream = random > 0.7f;
            bool isRiser = random > 0.985f;

            AudioClip clipToPlay;
            float vol = 1f;

            if (isRiser)
            {
                AudioManager.PlaySoundClientRpc(AudioType.HorroRiser);
                continue;
            }
            else if (isScream && screamClips.Length > 0)
            {
                clipToPlay = screamClips[Random.Range(0, screamClips.Length)];
                vol = 0.8f;
            }
            else if (insectClips.Length > 0)
            {
                clipToPlay = insectClips[Random.Range(0, insectClips.Length)];
                vol = 0.3f;
            }
            else continue;

            stingerSource.PlayOneShot(clipToPlay, vol);
        }
    }
}