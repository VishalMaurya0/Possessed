//Author: Small Hedge Games
//Updated: 13/06/2024

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSO SO;
    private static AudioManager instance = null;
    private AudioSource audioSource;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
        }
    }

    public static void PlaySound(AudioType sound, AudioSource source = null, float volume = 1)
    {
        var randomClip = FindAudioFX(sound, out SoundFXList soundList);

        if (source)
        {
            source.outputAudioMixerGroup = soundList.mixer;
            source.clip = randomClip;
            source.volume = volume * soundList.volume;
            source.Play();
        }
        else
        {
            instance.audioSource.outputAudioMixerGroup = soundList.mixer;
            instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
        }
    }

    public static AudioClip FindAudioFX(AudioType sound, out SoundFXList soundFX)
    {
        var fx = instance.SO.allSounds.Find(fx => fx.Type == sound);
        List<AudioClip> clips = fx.audioClips;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
        soundFX = fx;
        return randomClip;
    }
}
