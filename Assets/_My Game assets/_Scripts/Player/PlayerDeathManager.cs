using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeathManager : NetworkBehaviour
{
    public GameObject ashes;
    public GameObject playerCharacter;
    public GameObject playerIndicator;

    [Header("Revive Settings")]
    public float reviveTime = 15;
    public float reviveTimer = 0;
    public bool isInTrigger = false;
    public NetworkObject targetDownedPlayer = null;
    public Animator animator;

    [ClientRpc]
    public void DieClientRpc()
    {
        if (IsOwner || IsServer)
        {
            ulong clientId = gameObject.GetComponent<NetworkObject>().OwnerClientId;
            GameManager.Instance.GetClientThroughID(clientId).isAlive = false;
            if (IsOwner)
                GameManager.Instance.handleMovement = false;
        }
        ServerChangesServerRpc();
        
        //playerCharacter.SetActive(false);
        //ashes.SetActive(true);
        // remove noise TODO


        GameManager.Instance.NotifyClientAboutConnectedClientsServerRpc();


        GameManager.Instance.CheckIfEveryPlayerDied();

        GameManager.Instance.alivePlayers--;
        playerIndicator.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerChangesServerRpc()
    {

    }


    private void Update()
    {
        if (!isInTrigger || targetDownedPlayer == null) return;

        if (Input.GetKey(KeyCode.R))
        {
            reviveTimer += Time.deltaTime;
            GameManager.Instance.HelpInstructions.text = $"Reviving... {reviveTimer:F1}s";
            GameManager.Instance.helpInstructionDisplayTime = 3f;

            if (reviveTimer >= reviveTime)
            {
                reviveTimer = 0f;
                targetDownedPlayer.GetComponent<PlayerDeathManager>().NotifyReviveClientServerRpc();
                GameManager.Instance.HelpInstructions.text = "";
                GameManager.Instance.helpInstructionDisplayTime = 0f;
                targetDownedPlayer = null;
                isInTrigger = false;
            }
        }
        else
        {
            // Player let go of R, cancel revive
            if (reviveTimer > 0f)
            {
                GameManager.Instance.HelpInstructions.text = "Hold R to Revive";
                GameManager.Instance.helpInstructionDisplayTime = 3f;
            }
            reviveTimer = 0f;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!other.CompareTag("Player")) return;
    //    if (!IsOwner) return;

    //    NetworkObject obj = other.GetComponentInParent<NetworkObject>();
    //    if (obj == null) return;

    //    ulong id = obj.OwnerClientId;
    //    var data = GameManager.Instance.GetClientThroughID(id);
    //    if (data.isAlive) return;

    //    GameManager.Instance.HelpInstructions.text = "Hold R to Revive";
    //    GameManager.Instance.helpInstructionDisplayTime = 3f;
    //    isInTrigger = true;
    //    targetDownedPlayer = obj;
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!IsOwner) return;

    //    if (other.CompareTag("Player"))
    //    {
    //        GameManager.Instance.HelpInstructions.text = "";
    //        // GameManager.Instance.helpInstructionDisplayTime = 0f;
    //        isInTrigger = false;
    //        reviveTimer = 0f;
    //        targetDownedPlayer = null;
    //    }
    //}


    [ServerRpc(RequireOwnership = false)]
    private void NotifyReviveClientServerRpc()
    {
        ReviveClientRpc();
    }

    [ClientRpc]
    private void ReviveClientRpc()
    {
        if (IsOwner){
            GameManager.Instance.handleMovement = true;
        }
            animator.SetBool("Dead", false);

        ulong clientId = gameObject.GetComponent<NetworkObject>().OwnerClientId;
        GameManager.Instance.GetClientThroughID(clientId).isAlive = true;


        playerIndicator.SetActive(true);
        playerCharacter.SetActive(true);
        ashes.SetActive(false);

        if (IsServer)
        {
            GameManager.Instance.connectedClientsData[(int)clientId].playerGameobject.GetComponent<FearMeter>().revived = true;
        }
    }
}
