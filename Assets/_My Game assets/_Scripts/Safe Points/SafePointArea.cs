using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            FearMeter fearMeter = other.gameObject.GetComponent<FearMeter>();
            fearMeter.SAFE = false;
            safePlayers.Remove(fearMeter);
            noOfPlayers--;
        }
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
                vfxGraph.SetVector4("GroundColor", depletionColor);
                vfxGraph.SetFloat("Amount", 1);
                helpText.text = $"Depleting at {noOfPlayers}X rate";
                if (lastVisualState == ZoneState.BrokenHealing)
                {
                    vfxGraph.Reinit();
                }
                break;

            case ZoneState.BrokenHealing:
                vfxGraph.SetVector4("GroundColor", healingColor);
                vfxGraph.SetFloat("Amount", 0);
                vfxGraph.Reinit();
                helpText.text = "Healing";
                helpText.color = healingColor;
                break;

            case ZoneState.FastRegen:
                vfxGraph.SetVector4("GroundColor", XhealingColor);
                vfxGraph.SetFloat("Amount", 1);
                helpText.text = "Faster Healing";
                helpText.color = XhealingColor;
                break;

            case ZoneState.Idle:
                vfxGraph.SetVector4("GroundColor", idleColor);
                vfxGraph.SetFloat("Amount", 1);
                helpText.text = "Fully Charged";
                if (lastVisualState == ZoneState.BrokenHealing)
                {
                    vfxGraph.Reinit();
                }
                helpText.color = idleColor;
                break;
        }

    }

    private void UpdateTimerUI()
    {
        timerText.text = $"{safepointTimer.Value:F1} secs Left";
    }
}