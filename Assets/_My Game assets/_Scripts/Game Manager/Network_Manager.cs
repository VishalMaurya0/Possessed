using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Network_Manager : NetworkBehaviour
{
    [Header("References")]
    public GameObject playerPrefab;
    public GenerateMap generateMap;
    public CameraMovement cameraMovement;

    private bool generated = false;
    private bool runOnce = true;


    private void Awake()
    {
#if UNITY_EDITOR
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            NetworkManager.Singleton.StartHost();
        }
#endif
    }



    private void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Reinitialize connected clients on server start
            
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                OnClientConnected(client.Key);
            }            
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton or SceneManager is null!");
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            UpdateAllNoiseValues();
        }
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (!IsServer) return;

        if (sceneName == "Procedural Generation")
        {
            Debug.Log("Server: Procedural Generation scene loaded.");

            if (generateMap != null && !generated)
            {
                generated = true;
                generateMap.HandleServerStarted();
            }
            else
            {
                Debug.LogWarning("GenerateMap not found in scene or already generated.");
            }

            // Spawn players for connected clients if not already spawned
            foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
            {
                ulong clientID = clientPair.Key;
                if (clientPair.Value.PlayerObject != null) continue;

                GameObject playerInstance = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
            }
        }
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(68, 4, 68) + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
    }






    private void OnClientConnected(ulong clientId)
    {


        if (GameManager.Instance == null || NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            GameObject player = client.PlayerObject.gameObject;

            if (runOnce && cameraMovement != null && NetworkManager.Singleton.LocalClientId == clientId)
            {
                runOnce = false;

                Transform cameraTransform_ = player.transform.childCount > 0 ? player.transform.GetChild(0) : null;
                if (cameraTransform_ != null)
                {
                    cameraMovement.cameraTransform = cameraTransform_;
                    GameManager.Instance.ownerPlayer = player;
                    GameManager.Instance.serverStarted = true;
                    GameManager.Instance.ServerStarted();
                }
                else
                {
                    Debug.LogError("Player has no child camera transform!");
                }
            }
            
            if (!IsServer) return;

            GameManager.Instance.noOfPlayers++;
            GameManager.Instance.noiseValues[(int)client.ClientId] = 0;
        }

        UpdateConnectedClients();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer || GameManager.Instance == null) return;

        GameManager.Instance.noOfPlayers--;
        UpdateConnectedClients();
    }

    private void UpdateConnectedClients()
    {
        if (GameManager.Instance == null || NetworkManager.Singleton == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            var playerObject = client.Value.PlayerObject;

            if (playerObject != null && !GameManager.Instance.connectedClients.ContainsKey(clientId))
            {
                GameManager.Instance.connectedClients.Add(clientId, playerObject.gameObject);
            }
        }
    }

    private void UpdateAllNoiseValues()
    {
        if (GameManager.Instance == null) return;

        int i = 0;
        foreach (var kvp in GameManager.Instance.connectedClients)
        {
            if (kvp.Value != null)
            {
                NoiseHandler noiseHandler = kvp.Value.GetComponent<NoiseHandler>();
                if (noiseHandler != null)
                {
                    GameManager.Instance.noiseValues[i] = noiseHandler.noiseValue;
                }
            }
            i++;
        }
    }
}
