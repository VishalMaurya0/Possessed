using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Revival : NetworkBehaviour
{
    public PlayerDeathManager playerDeathManager;

    [Header("Detection Settings")]
    public float refreshRate = 1.0f; // How often to check (in seconds)
    public float detectionRadius = 3.0f; // Range to check for downed players

    private float timer = 0f;

    private void Start()
    {
        if (playerDeathManager == null)
        {
            playerDeathManager = GetComponentInParent<PlayerDeathManager>();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        timer += Time.deltaTime;

        if (timer >= refreshRate)
        {
            timer = 0f;
            CheckForDownedPlayers();
        }
    }

    private void CheckForDownedPlayers()
    {
        // 1. Scan the area for colliders
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        bool validTargetFound = false;

        foreach (var other in hits)
        {
            if (!other.CompareTag("Player")) continue;
            // Get the NetworkObject from the player we found
            NetworkObject obj = other.GetComponentInParent<NetworkObject>();
            if (obj == null) continue;

            // Skip ourselves (optional, depending on your layer setup)
            if (obj.OwnerClientId == OwnerClientId) continue;

            // Check if this player is actually dead
            ulong id = obj.OwnerClientId;
            var data = GameManager.Instance.GetClientThroughID(id);

            Debug.LogError("Found a player collider during revival check.");
            // If they are dead (isAlive == false), we found a target
            if (!data.isAlive)
            {
                // -- LOGIC FROM OnTriggerEnter --
                GameManager.Instance.HelpInstructions.text = "Hold R to Revive";
                GameManager.Instance.helpInstructionDisplayTime = 3f;
                playerDeathManager.isInTrigger = true;
                playerDeathManager.targetDownedPlayer = obj;

                validTargetFound = true;
                break; // Stop checking after finding the first valid downed player
            }
        }

        // 2. If we searched everyone and found NO valid targets, but we thought we were in a trigger...
        // We need to run the "Exit" logic.
        if (!validTargetFound && playerDeathManager.isInTrigger)
        {
            // -- LOGIC FROM OnTriggerExit --
            GameManager.Instance.HelpInstructions.text = "";
            playerDeathManager.isInTrigger = false;
            //playerDeathManager.reviveTimer = 0f;
            playerDeathManager.targetDownedPlayer = null;
        }
    }

    // Visualization to help you see the range in the Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}