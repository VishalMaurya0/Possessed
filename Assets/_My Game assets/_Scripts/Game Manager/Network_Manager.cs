using UnityEngine;
using Unity.Netcode;

public class Network_Manager : NetworkBehaviour
{
    public CameraMovement cameraMovement;  // Fixed naming convention
    public GameObject ownerPlayer;
    private bool runOnce = true;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null!");
        }
    }

    new private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        if (!IsServer || GameManager.Instance == null) return;

        GameManager.Instance.noOfPlayers--;
        GetAllConnectedClients();
    }

    private void Update()
    {
        if (IsServer)
        {
            GetAllNoiseValue();
        }
    }

    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton == null || GameManager.Instance == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(obj, out var client) && client.PlayerObject != null)
        {
            GameObject player = client.PlayerObject.gameObject;
            GameManager.Instance.noOfPlayers++;
            GameManager.Instance.noiseValues[GameManager.Instance.noOfPlayers - 1] = 0;

            if (runOnce && cameraMovement != null)
            {
                runOnce = false;
                Transform cameraTransform = player.transform.childCount > 0 ? player.transform.GetChild(0) : null;

                if (cameraTransform != null)
                {
                    cameraMovement.cameraTransform = cameraTransform;
                    ownerPlayer = player;
                    GameManager.Instance.ownerPlayer = player;
                    GameManager.Instance.serverStarted = true;
                    GameManager.Instance.ServerStarted();
                }
                else
                {
                    Debug.LogError("Player does not have a child transform for the camera!");
                }
            }
        }
        GetAllConnectedClients();
    }

    public void GetAllConnectedClients()
    {
        if (NetworkManager.Singleton == null || GameManager.Instance == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            var playerObject = client.Value.PlayerObject;

            if (playerObject != null)
            {
                Vector3 position = playerObject.transform.position;
                Debug.Log($"Client ID: {clientId}, Position: {position}");

                if (!GameManager.Instance.connectedClients.ContainsKey(clientId))
                {
                    GameManager.Instance.connectedClients.Add(clientId, playerObject.gameObject);
                }
            }
        }
    }

    public void GetAllNoiseValue()
    {
        if (GameManager.Instance == null) return;

        int i = 0;
        foreach (var client in GameManager.Instance.connectedClients)
        {
            if (client.Value != null)
            {
                NoiseHandler noiseHandler = client.Value.GetComponent<NoiseHandler>();
                if (noiseHandler != null)
                {
                    GameManager.Instance.noiseValues[i] = noiseHandler.noiseValue;
                }
            }
            i++;
        }
    }
}
