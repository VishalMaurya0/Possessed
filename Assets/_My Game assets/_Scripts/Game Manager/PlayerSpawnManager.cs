using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Check if player already spawned (prevent double spawning)
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            return;

        // Spawn manually
        GameObject playerInstance = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPosition()
    {
        // Change this as needed (e.g., spawn points list)
        return new Vector3(68, 4, 68) + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
    }
}
