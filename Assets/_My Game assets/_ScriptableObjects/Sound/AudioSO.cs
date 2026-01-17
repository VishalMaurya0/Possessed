using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioSO", menuName = "Scriptable Objects/AudioSO")]
public class AudioSO : ScriptableObject
{
    public List<SoundFXList> allSounds;
}

[System.Serializable]
public class SoundFXList
{
    [HideInInspector]public string Name;
    public AudioType Type;
    public List<AudioClip> audioClips;
    [Range(0, 1)] public float volume;
    public AudioMixerGroup mixer;
    //
}

public enum AudioType
{
    Walk,
    ItemPickup,
    ItemThrow,
    ItemInspect,
    ItemCraft,
    ItemDrop,
    Q,
    GhostWalk,
    GhostRoar,
    Click,
    PanelOpen,
    PanelClose,
    Correct,
    Wrong,
}