using Unity.Netcode;
using UnityEngine;

public class PushableObject : NetworkBehaviour
{
    // Cooldown to prevent spamming the Server with requests every frame
    private float lastOwnershipRequestTime;
    private float requestCooldown = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Check if the thing touching me is a Player
        // (Make sure your Player prefab has the tag "Player")
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. If I (the local client) am NOT the owner yet...
            if (!IsOwner && Time.time > lastOwnershipRequestTime + requestCooldown)
            {
                // 3. ...Ask the server to give me the object.
                RequestOwnershipServerRpc();
                lastOwnershipRequestTime = Time.time;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        // 4. The Server agrees and transfers ownership to the client who asked
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        if (NetworkObject.OwnerClientId != senderClientId)
        {
            NetworkObject.ChangeOwnership(senderClientId);
        }
    }

    
}