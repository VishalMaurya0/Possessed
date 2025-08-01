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
    bool runStartOnceAfterStartingServer = true;
    private bool sceneInitDone = false;
    private bool subscribeEvents = false;
    private bool subscribeSceneLoadEvents = false;




    public void Awake()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null && !subscribeSceneLoadEvents)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
            subscribeSceneLoadEvents = true;
        }
    }

    private void SubscribeEvents()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            if (!subscribeSceneLoadEvents)
            {
                subscribeSceneLoadEvents = true;
                NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;      // ========this runs when scene load from main menu=========//
            }
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void MyStart()
    {
        if (NetworkManager.Singleton != null)
        {
            // Reinitialize connected clients on server start
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                OnClientConnected(client.Key);
            }            
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton");
        }

        if (IsServer && SceneManager.GetActiveScene().name == "Procedural Generation")
        {
            Debug.Log("[Server] Already in Procedural Generation scene, manually calling OnSceneLoadComplete.");
            HandleSceneInit();

            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                OnClientConnected(client.Key);
            }
        }
    }

    private void HandleSceneInit()
    {
        if (generateMap != null && !generated)
        {
            generated = true;
            generateMap.HandleServerStarted();
        }

        foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientID = clientPair.Key;
            if (clientPair.Value.PlayerObject == null && playerPrefab != null)
            {
                Vector3 spawnPosition = GetSpawnPosition();
                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
                netObj.SpawnAsPlayerObject(clientID);
            }
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
        if (!subscribeEvents && NetworkManager.Singleton.IsListening)
        {
            subscribeEvents = true;
            SubscribeEvents();
        }
        if (NetworkManager.Singleton.IsListening && runStartOnceAfterStartingServer)
        {
            MyStart();
            runStartOnceAfterStartingServer = false;
        }
    }

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneInitDone)
        {
            Debug.LogWarning("[OnSceneLoadComplete] Scene already initialized. Skipping duplicate call.");
            return;
        }
        sceneInitDone = true;
        Debug.Log($"[OnSceneLoadComplete] Triggered for clientId: {clientId}, sceneName: {sceneName}");

        if (!IsServer)
        {
            Debug.Log("[OnSceneLoadComplete] Not the server. Exiting.");
            return;
        }


        if (sceneName == "Procedural Generation")
        {
            HandleSceneInit();
        }

        if (sceneName == "Procedural Generation")
        {
            Debug.Log("[Server] Procedural Generation scene loaded.");

            if (generateMap == null)
            {
                Debug.LogWarning("[Server] generateMap is null. Trying to find it in the scene.");
            }

            if (generateMap != null && !generated)
            {
                Debug.Log("[Server] generateMap found and generation not yet triggered. Calling HandleServerStarted().");
                generated = true;
                generateMap.HandleServerStarted();
            }
            else if (generated)
            {
                Debug.LogWarning("[Server] Map has already been generated.");
            }
            else
            {
                Debug.LogError("[Server] generateMap is still null after search.");
            }








            Debug.Log("[Server] Spawning players if not already spawned...");
            foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
            {
                ulong clientID = clientPair.Key;
                Debug.Log($"[Server] Checking player for clientID: {clientID}");

                if (clientPair.Value.PlayerObject != null)
                {
                    Debug.Log($"[Server] PlayerObject already exists for clientID: {clientID}, skipping spawn.");
                    continue;
                }

                if (playerPrefab == null)
                {
                    Debug.LogError("[Server] playerPrefab is null! Cannot instantiate player.");
                    continue;
                }

                Vector3 spawnPosition = GetSpawnPosition();
                Debug.Log($"[Server] Spawning player for clientID: {clientID} at position: {spawnPosition}");

                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();

                if (netObj == null)
                {
                    Debug.LogError("[Server] Instantiated player prefab does not have a NetworkObject component!");
                    Destroy(playerInstance);
                    continue;
                }

                netObj.SpawnAsPlayerObject(clientID);
                Debug.Log($"[Server] PlayerObject spawned and assigned to clientID: {clientID}");
            }
        }
        else
        {
            Debug.Log($"[OnSceneLoadComplete] Scene '{sceneName}' is not handled in this method.");
        }
    }


    private Vector3 GetSpawnPosition()
    {
        return new Vector3(68, 4, 68) + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
    }






    private void OnClientConnected(ulong clientId)
    {
        if (GameManager.Instance == null || NetworkManager.Singleton == null) return;

        StartCoroutine(HandleLocalCamera(clientId));

        if (!IsServer) return;

        // If we're already in the game scene and player hasn't been spawned yet
        if (SceneManager.GetActiveScene().name == "Procedural Generation")
        {
            Debug.Log($"[Server] Late client connected in Procedural Generation scene: {clientId}");

            if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject == null)
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("[Server] playerPrefab is null! Cannot instantiate player.");
                    return;
                }

                Vector3 spawnPosition = GetSpawnPosition();
                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();

                if (netObj == null)
                {
                    Debug.LogError("[Server] Instantiated player prefab does not have a NetworkObject component!");
                    Destroy(playerInstance);
                    return;
                }

                netObj.SpawnAsPlayerObject(clientId);
                Debug.Log($"[Server] Spawned player for late clientID: {clientId}");
            }
            else
            {
                Debug.Log($"[Server] PlayerObject already exists for late clientID: {clientId}");
            }
        }

        GameManager.Instance.noOfPlayers++;

        UpdateConnectedClients();
    }


    private IEnumerator HandleLocalCamera(ulong clientId)
    {
        // Wait until PlayerObject is spawned
        yield return new WaitUntil(() =>
            NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId) &&
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null);

        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;

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

            if (playerObject != null && GameManager.Instance.GetClientThroughID(clientId) == null)
            {
                GameManager.Instance.connectedClientsData.Add(new ConnectedClientsData(clientId, playerObject.gameObject, true));
                //GameManager.Instance.playerIndicatorColors.Add(client, Color.); TODO 
            }
        }
    }

    private void UpdateAllNoiseValues()
    {
        if (GameManager.Instance == null) return;

        int i = 0;
        foreach (var kvp in GameManager.Instance.connectedClientsData)
        {
            if (kvp.playerGameobject != null)
            {
                NoiseHandler noiseHandler = kvp.playerGameobject.GetComponent<NoiseHandler>();
                if (noiseHandler != null)
                {
                    GameManager.Instance.connectedClientsData[i].noiseValue = noiseHandler.noiseValue;
                }
            }
            i++;
        }
    }
}
