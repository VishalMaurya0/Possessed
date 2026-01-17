#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[CustomEditor(typeof(AudioSO))]
public class SoundsSOEditor : Editor
{
    private void OnEnable()
    {
        ref List<SoundFXList> soundList = ref ((AudioSO)target).allSounds;

        if (soundList == null)
            return;

        string[] names = Enum.GetNames(typeof(AudioType));
        bool differentSize = names.Length != soundList.Count;

        Dictionary<string, SoundFXList> sounds = new();

        if (differentSize)
        {
            for (int i = 0; i < soundList.Count; ++i)
            {
                sounds.Add(soundList[i].Name, soundList[i]);
            }
        }

        if (soundList.Count != names.Length)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (!sounds.ContainsKey(names[i]))
                {
                    soundList.Add(new SoundFXList());
                }
            }
        }

        if (soundList.Count != names.Length)
        {
            soundList.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                soundList.Add(new SoundFXList());
            }
        }
        for (int i = 0; i < soundList.Count; i++)
        {
            string currentName = names[i];
            soundList[i].Name = currentName;
            if (soundList[i].volume == 0) soundList[i].volume = 1;

            if (differentSize)
            {
                if (sounds.ContainsKey(currentName))
                {
                    SoundFXList current = sounds[currentName];
                    UpdateElement(soundList[i], current.volume, current.audioClips, current.mixer);
                }
                else
                    UpdateElement(soundList[i], 1, new(), null);

                static void UpdateElement(SoundFXList element, float volume, List<AudioClip> sounds, AudioMixerGroup mixer)
                {
                    element.volume = volume;
                    element.audioClips = sounds;
                    element.mixer = mixer;
                }
            }
        }
    }
}
#endif