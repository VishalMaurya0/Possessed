using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Network Settings")]
    public GameObject ownerPlayer;
    public string ownerPlayerName = "Player";
    public Camera playerCamera;
    public bool serverStarted = false;
    public bool gameStarted = false;
    public bool gameEnd = false;


    [Header("One Time Variables")]
    [Tooltip("Maintained on Server")] public int noOfPlayers;             //----------maintained on server---------//
    public int inventorySlots = 5;
    public float maxWeight = 15;


    [Header("In Game Info")]
    public Dictionary<ulong, GameObject> connectedClients = new();
    public Dictionary<ulong, string> clientsNames = new();
    public Dictionary<GameObject, Procedures> completedProcedure = new();
    public Dictionary<int, float> noiseValues = new();
    public int[] selectedProceduresIndex;
    public List<int> completedProcedures;
    public float timeInSecElapsed = 0;


    [Header("Lock And Unlock")]
    public bool lockCurser = false;
    public bool handlePlayerLookWithMouse = true;
    public bool handleMovement = true;
    public bool itemScrollingLock = false;
    public bool bakeNavMeshAgain = false;

    [Header("References")]
    public ProcedureBase procedureBase;
    public List<ProcedureCompletion> AllProcedures = new();
    public TaskManager taskManager = null;


    [Header("Player Names")]
    public TMP_InputField playerName_InputField;
    public TMP_Text playerNameText;
    public GameObject parentObject; // Assign this in the Inspector (e.g., the panel that holds the texts)
    public List<TMP_Text> tmpTextList = new List<TMP_Text>();


    public static event Action onServerStarted;


    public static GameManager Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        { Destroy(this); }
        Instance = this;

        procedureBase = GetComponent<ProcedureBase>();
        handleMovement = true;
    }

    private void Start()
    {
        LeanTween.reset();
        LeanTween.cancelAll();
    }


    private void Update()
    {
        timeInSecElapsed += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            lockCurser = !lockCurser;
        }
        if (gameStarted && ownerPlayer == null)
        {
            gameEnd = true;
        }
    }

    public void ServerStarted()
    {
        onServerStarted?.Invoke();
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    public void CheckForCorrectProcedures()
    {
        if (completedProcedure.Count < selectedProceduresIndex.Length)
        {
            return;
        }

        int correctOnes = 0;
        for (int i = 0; i < selectedProceduresIndex.Length; i++)
        {
            if (completedProcedures.Contains(selectedProceduresIndex[i]))
            {
                correctOnes++;
            }
        }

        if (correctOnes > selectedProceduresIndex.Length)
        {

        }
    }



    /// <summary>
    /// Player Names
    /// </summary>


    private void HandleClientConnected(ulong clientId)
    {
        if (IsOwner)
            ClientAdded();  // <-- Call it here on the local client
    }

    public void ClientAdded()
    {
        tmpTextList = GetTMPTextInHierarchyOrder(parentObject);
        Debug.Log($"[Client] ClientAdded called. Found {tmpTextList.Count} TMP_Text elements.");
    }


    public void PlayerNameChanged()
    {
        playerName_InputField.onEndEdit.AddListener((value) =>
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                ownerPlayerName = value;
                Debug.LogError($"[Client] PlayerNameChanged called with name: {ownerPlayerName}");
                GivePlayerNameServerRpc(ownerPlayerName);
                playerNameText.text = ownerPlayerName;
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void GivePlayerNameServerRpc(string name, ServerRpcParams rpcReceiveParams = default)
    {
        Debug.LogError($"[Server] GivePlayerNameServerRpc received from client {rpcReceiveParams.Receive.SenderClientId} with name: {name}");

        if (!clientsNames.ContainsKey(rpcReceiveParams.Receive.SenderClientId))
        {
            clientsNames.Add(rpcReceiveParams.Receive.SenderClientId, name);
        }
        else
        {
            clientsNames[rpcReceiveParams.Receive.SenderClientId] = name;
        }

        SendNamesToEveryone();
    }
    private void SendNamesToEveryone()
    {
        Debug.Log($"[Server] Sending names to all clients. Count: {clientsNames.Count}");

        ResetNamesClientRpc();

        foreach (var client in clientsNames)
        {
            Debug.Log($"[Server] Sending name to clients: {client.Key} -> {client.Value}");
            SendNameClientRpc(client.Key, client.Value);
        }

        SetPlayerNamesClientRpc();
    }

    [ClientRpc]
    private void ResetNamesClientRpc()
    {
        if (IsServer)
            return;

        Debug.Log("[Client] Resetting local clientsNames dictionary");
        clientsNames.Clear();
    }
    [ClientRpc]
    private void SendNameClientRpc(ulong ID, string name)
    {
        Debug.Log($"[Client] Received name from server: {ID} -> {name}");

        if (!clientsNames.ContainsKey(ID))
        {
            clientsNames.Add(ID, name);
        }
        else
        {
            clientsNames[ID] = name;
        }
    }
    [ClientRpc]
    private void SetPlayerNamesClientRpc()
    {
        Debug.Log($"[Client] Setting player names in UI. Count: {clientsNames.Count}");

        int i = 0;
        foreach (var client in clientsNames)
        {
            if (i < tmpTextList.Count)
            {
                Debug.Log($"[Client] Setting tmpTextList[{i}] = {client.Value}");
                tmpTextList[i].text = client.Value;
                i++;
            }
        }
    }


    List<TMP_Text> GetTMPTextInHierarchyOrder(GameObject root)
    {
        List<TMP_Text> list = new List<TMP_Text>();

        TMP_Text[] allTexts = root.GetComponentsInChildren<TMP_Text>(true);

        System.Array.Sort(allTexts, (a, b) =>
        {
            int aIndex = a.transform.GetSiblingIndex();
            int bIndex = b.transform.GetSiblingIndex();
            return aIndex.CompareTo(bIndex);
        });

        list.AddRange(allTexts);
        return list;
    }
}
