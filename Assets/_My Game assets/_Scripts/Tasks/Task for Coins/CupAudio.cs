using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class CupAudio : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Timing")]
    public float currentPlayTime;


    Coroutine playRoutine;

    void Awake()
    {
        if (currentPlayTime == 0)
            currentPlayTime = 1;

        audioSource = GetComponent<AudioSource>();
    }

    [ClientRpc]
    public void OnDragStartClientRpc()
    {
        if (audioSource.clip == null) return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        audioSource.Stop();

        float maxStart = audioSource.clip.length - currentPlayTime;
        if (maxStart <= 0f) return;

        audioSource.time = UnityEngine.Random.Range(0f, maxStart);
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.Play();

        playRoutine = StartCoroutine(StopAfterTime(currentPlayTime));
    }


    // Call this when drag ends
    void OnDragEnd()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        audioSource.Stop();
    }

    void PlayRandomSlice()
    {
        if (audioSource.clip == null) return;

        float maxStart = audioSource.clip.length - currentPlayTime;
        if (maxStart <= 0f) return;

        float randomTime = Random.Range(0f, maxStart);

        audioSource.time = randomTime;
        audioSource.Play();

        playRoutine = StartCoroutine(StopAfterTime(currentPlayTime));
    }

    IEnumerator StopAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        StopSound();
    }

    void StopSound()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        audioSource.Stop();
    }
}
