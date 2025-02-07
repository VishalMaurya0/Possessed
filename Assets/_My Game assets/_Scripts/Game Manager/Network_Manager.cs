using UnityEngine;
using Unity.Netcode;

public class Network_Manager : NetworkBehaviour
{
    public cameraMovement cameraMovement;
    public GameObject ownerPlayer;
    bool runOnce = true;

    
    void Start()
    {
        
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        if (!IsServer) return;
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
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(obj, out var client)) 
        {
            GameObject player = client.PlayerObject.gameObject;
            GameManager.Instance.noOfPlayers++;
            GameManager.Instance.noiseValues.Add(GameManager.Instance.noOfPlayers - 1, 0);
            if (runOnce)
            {
                runOnce = false;
                cameraMovement.cameraTransform = player.transform.GetChild(0).transform;
                ownerPlayer = player;
                GameManager.Instance.ownerPlayer = player;
                GameManager.Instance.serverStarted = true;
            }
        }
        GetAllConnectedClients();
    }

    public void GetAllConnectedClients()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            var playerObject = client.Value.PlayerObject;

            if (playerObject != null)
            {
                Vector3 position = playerObject.transform.position;
                Debug.Log($"Client ID: {clientId}, Position: {position}");

                bool alreadyThere = false;
                foreach (var clientt in GameManager.Instance.connectedClients)
                {
                    if (clientt.Key == clientId)
                    { alreadyThere = true; }
                }
                if (!alreadyThere)
                {
                    GameManager.Instance.connectedClients.Add(clientId, playerObject.gameObject);
                }
            }
        }
    }

    public void GetAllNoiseValue()
    {
        int i = 0;
        foreach (var client in GameManager.Instance.connectedClients)
        {
            GameObject player = client.Value;
            float noiseValue = player.GetComponent<NoiseHandler>().noiseValue;
            GameManager.Instance.noiseValues[i] = noiseValue;
            i++;
        }
    }
}
