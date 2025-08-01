using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerDeathManager : NetworkBehaviour
{
    public PlayerController playerController;

    [ClientRpc]
    public void DieClientRpc()
    {
        GameManager.Instance.handleMovement = false;
    }
}
