using System.Collections.Generic;
using UnityEngine;

public class DollSounds : MonoBehaviour
{
    public GameObject parentSource;
    private List<AudioSource> childSources = new List<AudioSource>();
    private bool isRunning;

    private void Awake()
    {
        if (parentSource == null)
            parentSource = gameObject;

        childSources.AddRange(parentSource.GetComponentsInChildren<AudioSource>(false));
    }

    public void StartSound()
    {
        if (isRunning)
            return;

        isRunning = true;

        foreach (var source in childSources)
        {
            if (source.clip == null)
                continue;

            source.loop = true;

            float delay = Random.Range(0f, 3f); // controlled randomness
            source.PlayDelayed(delay);
        }
    }

    public void StopSound()
    {
        if (!isRunning)
            return;

        isRunning = false;

        foreach (var source in childSources)
        {
            source.Stop();
        }
    }
}
