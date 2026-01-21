using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.VFX;

public class SafePointArea : NetworkBehaviour
{
    [Header("Settings")]
    public float safePointTimerDuration = 10f;
    public float XFactor = 1.5f;

    [Header("State Info (Synced)")]
    public NetworkVariable<bool> active = new NetworkVariable<bool>(true);
    public NetworkVariable<float> safepointTimer = new NetworkVariable<float>(10f);

    private NetworkVariable<ZoneState> networkState = new NetworkVariable<ZoneState>(ZoneState.Idle);
    public List<FearMeter> safePlayers = new List<FearMeter>();
    public int noOfPlayers;

    [Header("Visuals")]
    public TMP_Text timerText;
    public TMP_Text helpText;
    public VisualEffect vfxGraph;

    [Header("Audio")]
    public AudioSource source;
    public AudioClip charging;
    public float slowPitch;
    public AudioClip disCharging;
    public float fastPitch;
    public AudioClip normal;
    public float volume = 1;
    public AudioClip lowBattery;
    public bool playSound;

    [Header("Audio Snapshots")]
    public AudioMixerSnapshot normalSnapshot; // Drag "Original" here
    public AudioMixerSnapshot safeSnapshot;   // Drag "SafeRoom" here
    public float transitionTime = 1.0f;       // How long the fade takes


    [Header("Colors")]
    [ColorUsage(true, true)] public Color depletionColor;
    [ColorUsage(true, true)] public Color healingColor;
    [ColorUsage(true, true)] public Color XhealingColor;
    [ColorUsage(true, true)] public Color idleColor;

    public enum ZoneState { Idle, Depleting, BrokenHealing, FastRegen }

    private ZoneState lastVisualState = ZoneState.Idle;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            safepointTimer.Value = safePointTimerDuration;
        }

        UpdateVisuals(networkState.Value, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.isTrigger) return;

        if (other.CompareTag("Player"))
        {
            safePlayers.Add(other.gameObject.GetComponent<FearMeter>());
            noOfPlayers++;
        }

        if (safeSnapshot != null)
            safeSnapshot.TransitionTo(transitionTime);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.isTrigger) return;

        if (other.CompareTag("Player"))
        {
            FearMeter fearMeter = other.gameObject.GetComponent<FearMeter>();
            fearMeter.SAFE = false;
            safePlayers.Remove(fearMeter);
            noOfPlayers--;
        }

        if (normalSnapshot != null)
            normalSnapshot.TransitionTo(transitionTime);
    }

    float timer = 0;
    float toCheckTime = 1;
    private void Update()
    {
        if (IsServer)
        {
            CalculateLogic();
        }

        timer += Time.deltaTime;
        if (safePlayers.Count > 0 && timer > toCheckTime)
        {
            timer = 0;
            foreach (var fearMeter in safePlayers)
            {
                if (fearMeter)
                {
                    if (active.Value)
                        fearMeter.SAFE = true;
                    else
                        fearMeter.SAFE = false;
                }
            }
        }

        ZoneState currentSyncedState = networkState.Value;

        if (currentSyncedState != lastVisualState)
        {
            UpdateVisuals(currentSyncedState);
            lastVisualState = currentSyncedState;
        }

        if (currentSyncedState == ZoneState.Depleting)
        {
            if (safepointTimer.Value < 5 && playSound)
            {
                source.PlayOneShot(lowBattery);
                playSound = false;
            }
        }

        UpdateTimerUI();
    }

    private void CalculateLogic()
    {
        if (!active.Value && safepointTimer.Value >= safePointTimerDuration)
        {
            active.Value = true;
            safepointTimer.Value = safePointTimerDuration;
        }

        if (active.Value && noOfPlayers > 0)
        {
            safepointTimer.Value -= Time.deltaTime * noOfPlayers;
            networkState.Value = ZoneState.Depleting;

            if (safepointTimer.Value <= 0)
            {
                active.Value = false;
                safepointTimer.Value = 0;
            }
        }
        else if (!active.Value)
        {
            safepointTimer.Value += Time.deltaTime;
            networkState.Value = ZoneState.BrokenHealing;
        }
        else if (active.Value && noOfPlayers <= 0 && safepointTimer.Value < safePointTimerDuration)
        {
            safepointTimer.Value += Time.deltaTime * XFactor;
            networkState.Value = ZoneState.FastRegen;
        }
        else
        {
            safepointTimer.Value = safePointTimerDuration;
            networkState.Value = ZoneState.Idle;
        }
    }

    private void UpdateVisuals(ZoneState state, bool force = false)
    {
        switch (state)
        {
            case ZoneState.Depleting:
                PlaySound(State.Discharging);
                vfxGraph.SetVector4("GroundColor", depletionColor);
                vfxGraph.SetFloat("Amount", 1);
                helpText.text = $"Depleting at {noOfPlayers}X rate";

                if (lastVisualState == ZoneState.BrokenHealing)
                {
                    vfxGraph.Reinit();
                }
                break;

            case ZoneState.BrokenHealing:
                PlaySound(State.Charging);
                vfxGraph.SetVector4("GroundColor", healingColor);
                vfxGraph.SetFloat("Amount", 0);
                vfxGraph.Reinit();
                helpText.text = "Healing";
                helpText.color = healingColor;

                playSound = true;
                break;

            case ZoneState.FastRegen:
                PlaySound(State.FastCharging);
                vfxGraph.SetVector4("GroundColor", XhealingColor);
                vfxGraph.SetFloat("Amount", 1);
                helpText.text = "Faster Healing";
                helpText.color = XhealingColor;
                
                playSound = true;
                break;

            case ZoneState.Idle:
                PlaySound(State.Normal);
                vfxGraph.SetVector4("GroundColor", idleColor);
                vfxGraph.SetFloat("Amount", 1);
                helpText.text = "Fully Charged";
                if (lastVisualState == ZoneState.BrokenHealing)
                {
                    vfxGraph.Reinit();
                }
                helpText.color = idleColor;
                
                playSound = true;
                break;
        }

    }

    private float lastDisplayedTime = -1f; // Cache the last value

    private void UpdateTimerUI()
    {
        float currentTime = safepointTimer.Value;

        // Only update if the difference is significant enough to show up in "F1"
        // (We check if the difference is greater than 0.1)
        if (Mathf.Abs(currentTime - lastDisplayedTime) >= 0.1)
        {
            timerText.text = $"{currentTime:F1} secs Left";
            lastDisplayedTime = currentTime;
        }
    }

    public void PlaySound(State state)
    {
        AudioClip targetClip = null;
        float targetPitch = 1f;

        // 1. Determine which Clip and Pitch we WANT
        switch (state)
        {
            case State.Normal:
                targetClip = normal;
                targetPitch = slowPitch; // Or 1, depending on preference
                break;
            case State.Charging:
                targetClip = charging;
                targetPitch = slowPitch;
                break;
            case State.FastCharging:
                targetClip = charging; // Same clip as Charging!
                targetPitch = fastPitch;
                break;
            case State.Discharging:
                targetClip = disCharging;
                targetPitch = slowPitch; // Or 1
                break;
        }

        // 2. Assign Volume (Always apply)
        source.volume = volume;

        // 3. Smart Transition Logic
        // If the source is ALREADY playing the correct clip, just shift the pitch (No Stutter)
        if (source.isPlaying && source.clip == targetClip)
        {
            source.pitch = targetPitch;
            // Ensure looping is ON for continuous states
            //source.loop = true;
        }
        else
        {
            // It's a new sound, so we swap and play
        Debug.LogError("csdfghj");
            source.clip = targetClip;
            source.pitch = targetPitch;
            source.loop = true; // State sounds should usually loop
            source.Play();
        }
    }


    public enum State
    {
        Normal,
        Charging,
        FastCharging,
        Discharging,
    }
}