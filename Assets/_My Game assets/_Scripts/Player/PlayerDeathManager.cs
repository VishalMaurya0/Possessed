using Unity.Netcode;
using UnityEngine;

public class PlayerDeathManager : NetworkBehaviour
{
    public PlayerController playerController;
    public GameObject ashes;
    public GameObject playerCharacter;
    public GameObject playerIndicator;

    [ClientRpc]
    public void DieClientRpc()
    {
        GameManager.Instance.handleMovement = false;

        ServerChangesServerRpc();
        
        playerIndicator.SetActive(false);
        playerCharacter.SetActive(false);
        ashes.SetActive(true);
        // remove noise TODO
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerChangesServerRpc()
    {

    }
}
