using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Multiplayer.Widgets;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("Network Settings")]
    public GameObject ownerPlayer;
    public ulong localID;
    public string ownerPlayerName = "Player";
    public Camera playerCamera;
    public bool serverStarted = false;
    public bool gameStarted = false;
    public bool gameEnd = false;


    [Header("One Time Variables")]
    [Tooltip("Maintained on Server")] public int noOfPlayers;             //----------maintained on server---------//
    public int inventorySlots = 5;
    public float maxWeight = 15;
    public Color playerIndicatorColor;


    [Header("In Game Info")]
    public List<ConnectedClientsData> connectedClientsData = new();
    public NetworkVariable<float> connectedClientsNumber = new NetworkVariable<float>();
    //public Dictionary<ulong, GameObject> connectedClients = new();
    //public Dictionary<ulong, bool> isPlayerAlive = new();
    //public Dictionary<ulong, string> clientsNames = new();
    //public Dictionary<ulong, Color> playerIndicatorColors = new();
    public Dictionary<GameObject, Procedures> completedProcedure = new();
    //public Dictionary<int, float> noiseValues = new();
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
    public TMP_Text HelpInstructions;
    public Animator winLoseAnimator;

    [Header("Host Accessible")]
    public Button PlayButton;
    public Toggle PublicPrivateToggle;
    public GameObject LoadingPanel;

    [Header("Player Names")]
    public TMP_InputField playerName_InputFieldA;
    public TMP_InputField playerName_InputFieldB;
    bool updatingFields = false;
    //public TMP_Text playerNameText;
    //public GameObject parentObject; // Assign this in the Inspector (e.g., the panel that holds the texts)
    //public List<TMP_Text> tmpTextList = new List<TMP_Text>();


    [Header("Win Lose Condition")]
    public TMP_Text winText;


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
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        LeanTween.reset();
        LeanTween.cancelAll();
        if (playerName_InputFieldA != null)
        {
            playerName_InputFieldA.onValueChanged.AddListener(OnInputFieldAChanged);
            playerName_InputFieldB.onValueChanged.AddListener(OnInputFieldBChanged);
            PlayButton.interactable = false;
            PublicPrivateToggle.interactable = false;
        }

    }


    private void OnClientConnected(ulong obj)
    {
        if (!IsServer) return;
        if (!(SceneManager.GetActiveScene().name == "Procedural Generation"))
        {
            connectedClientsData.Add(new ConnectedClientsData(obj, null, true));
            connectedClientsNumber.Value = connectedClientsData.Count;
            //Debug.LogError(connectedClientsData.Count);
            NotifyClientAboutConnectedClientsServerRpc();
        }
    }

    private void OnClientDisconnected(ulong obj)
    {
        if (!IsServer) return;
        if (!(SceneManager.GetActiveScene().name == "Procedural Generation"))
        {
            connectedClientsData.Remove(GetClientThroughID(obj));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyClientAboutConnectedClientsServerRpc()
    {
        if (!IsServer) return;
        ClearConnectedClientsClientRpc();
        for (int i = 0; i < connectedClientsData.Count; i++)
        {
            TransferDataClientRpc(connectedClientsData[i].clientID, connectedClientsData[i].isAlive);
        }
    }


    [ClientRpc]
    private void ClearConnectedClientsClientRpc()
    {
        if (IsServer) return;
        connectedClientsData.Clear();
    }

    [ClientRpc]
    private void TransferDataClientRpc(ulong clientID, bool isAlive)
    {
        if (IsServer) return;
        connectedClientsData.Add(new ConnectedClientsData(clientID, null, isAlive));
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

        if (HelpInstructions != null && HelpInstructions.text != "")
        {
            StartCoroutine(ResetHelpInstructions());
        }
        
        if (!IsServer)
        {
            if (connectedClientsData.Count != connectedClientsNumber.Value)
            {
                NotifyClientAboutConnectedClientsServerRpc();
            }
        }
    }

    IEnumerator ResetHelpInstructions()
    {
        yield return new WaitForSeconds(3);
        HelpInstructions.text = "";
    }

    public void ServerStarted()
    {
        onServerStarted?.Invoke();
    }

    public void CheckForPlayerDead()
    {
        // Check if every player died

        for (int i = 0; i < GameManager.Instance.connectedClientsData.Count; i++)
        {
            ConnectedClientsData connectedClientsData = GameManager.Instance.connectedClientsData[i];
            if (connectedClientsData.isAlive) return;
        }

        // No one is alive

        winLoseAnimator.SetTrigger("Won");  // activate the winLose Panel
        GameManager.Instance.OnWinOrLose(false, true);
    }

    public bool CheckForCorrectProcedures()
    {
        if (completedProcedures.Count < selectedProceduresIndex.Length)
        {
            return false;
        }

        foreach (int index in selectedProceduresIndex)
        {
            if (!completedProcedures.Contains(index))
            {
                return false;
            }
        }

        return true;
    }



    /// <summary>
    /// Player Names
    /// </summary>


    void OnInputFieldAChanged(string value)
    {
        if (updatingFields) return;

        updatingFields = true;
        playerName_InputFieldB.text = value;
        PlayerPrefs.SetString("PlayerName", value);
        PlayerPrefs.Save();

        ownerPlayerName = value;
        //Debug.LogError($"[Client] PlayerNameChanged called with name: {ownerPlayerName}");

        updatingFields = false;
    }

    void OnInputFieldBChanged(string value)
    {
        if (updatingFields) return;

        updatingFields = true;
        playerName_InputFieldA.text = value;
        PlayerPrefs.SetString("PlayerName", value);
        PlayerPrefs.Save();

        ownerPlayerName = value;
        //Debug.LogError($"[Client] PlayerNameChanged called with name: {ownerPlayerName}");

        updatingFields = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost && PlayButton != null)
        {
            PlayButton.interactable = true;
            PublicPrivateToggle.interactable = true;
            Debug.Log("I'm the host, enabling the button.");
        }

        if (IsOwner)
        {
            localID = NetworkManager.Singleton.LocalClientId;
            Debug.Log($"[DummyScript] Set ID: {localID} for client: {NetworkManager.Singleton.LocalClientId}");
        }
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // =============== Win and Lose Condition ===========//
    public void OnWinOrLose(bool win, bool dead)
    {
        if (win && !dead)
        {
            winText.text = $"You have Completed the Correct Procedures :) Remember to Play Again!\r\nYou Won!!!";
        }else if (!win && !dead)
        {
            winText.text = $"You have Completed the InCorrect Procedures :( Give it Another Chance!\r\nYou Lose!!!";
        }

        if (dead)
        {
            winText.text = $"EveryOne Died??\r\nTry Again!!";
        }
    }

    // ===============Starting Game ===========//
    public void StartGame()
    {
        if (IsHost)
        {
            ShowLoadingPanelClientRpc(); // Optional visual effect
            NetworkManager.SceneManager.LoadScene("Procedural Generation", LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void ShowLoadingPanelClientRpc()
    {
        LoadingPanel.SetActive(true); // Optional: show loading UI before transition
    }

    private void HandleClientConnected(ulong clientId)
    {
        //ClientAdded();  // <-- Call it here on the local client
    }

    //public void ClientAdded()
    //{
    //    tmpTextList = GetTMPTextInHierarchyOrder(parentObject);
    //    Debug.LogError($"[Client] ClientAdded called. Found {tmpTextList.Count} TMP_Text elements.");
    //    SetPlayerNamesClientRpc();
    //}


    //public void PlayerNameChanged()
    //{
    //    playerName_InputFieldA.onEndEdit.AddListener((value) =>
    //    {
    //        if (!string.IsNullOrWhiteSpace(value))
    //        {
    //            ownerPlayerName = value;
    //            Debug.LogError($"[Client] PlayerNameChanged called with name: {ownerPlayerName}");
    //            StartCoroutine(SetPlayerName());
    //            playerNameText.text = ownerPlayerName;
    //            PlayerPrefs.SetString("PlayerName", ownerPlayerName);
    //            PlayerPrefs.Save();
    //        }
    //    });
    //}

    //IEnumerator SetPlayerName()
    //{
    //    yield return new WaitForSeconds(2);
    //    GivePlayerNameServerRpc(ownerPlayerName);
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void GivePlayerNameServerRpc(string name, ServerRpcParams rpcReceiveParams = default)
    //{
    //    Debug.LogError($"[Server] GivePlayerNameServerRpc received from client {rpcReceiveParams.Receive.SenderClientId} with name: {name}");

    //    if (!clientsNames.ContainsKey(rpcReceiveParams.Receive.SenderClientId))
    //    {
    //        clientsNames.Add(rpcReceiveParams.Receive.SenderClientId, name);
    //    }
    //    else
    //    {
    //        clientsNames[rpcReceiveParams.Receive.SenderClientId] = name;
    //    }

    //    SendNamesToEveryone();
    //}
    //private void SendNamesToEveryone()
    //{
    //    Debug.Log($"[Server] Sending names to all clients. Count: {clientsNames.Count}");

    //    ResetNamesClientRpc();

    //    foreach (var client in clientsNames)
    //    {
    //        Debug.Log($"[Server] Sending name to clients: {client.Key} -> {client.Value}");
    //        SendNameClientRpc(client.Key, client.Value);
    //    }

    //    SetPlayerNamesClientRpc();
    //}

    //[ClientRpc]
    //private void ResetNamesClientRpc()
    //{
    //    if (IsServer)
    //        return;

    //    Debug.Log("[Client] Resetting local clientsNames dictionary");
    //    clientsNames.Clear();
    //}
    //[ClientRpc]
    //private void SendNameClientRpc(ulong ID, string name)
    //{
    //    Debug.Log($"[Client] Received name from server: {ID} -> {name}");

    //    if (!clientsNames.ContainsKey(ID))
    //    {
    //        clientsNames.Add(ID, name);
    //    }
    //    else
    //    {
    //        clientsNames[ID] = name;
    //    }
    //}
    //[ClientRpc]
    //private void SetPlayerNamesClientRpc()
    //{
    //    Debug.Log($"[Client] Setting player names in UI. Count: {clientsNames.Count}");

    //    int i = 0;
    //    foreach (var client in clientsNames)
    //    {
    //        if (i < tmpTextList.Count)
    //        {
    //            Debug.Log($"[Client] Setting tmpTextList[{i}] = {client.Value}");
    //            tmpTextList[i].text = client.Value;
    //            i++;
    //        }
    //    }
    //}


    //List<TMP_Text> GetTMPTextInHierarchyOrder(GameObject root)
    //{
    //    List<TMP_Text> list = new List<TMP_Text>();

    //    TMP_Text[] allTexts = root.GetComponentsInChildren<TMP_Text>(true);

    //    // Filter only those with GameObject name "TextEncoded"
    //    List<TMP_Text> filtered = new List<TMP_Text>();
    //    foreach (var text in allTexts)
    //    {
    //        if (text.gameObject.name == "TextEncoded")
    //        {
    //            filtered.Add(text);
    //        }
    //    }

    //    // Sort by sibling index to maintain hierarchy order
    //    filtered.Sort((a, b) =>
    //    {
    //        int aIndex = a.transform.GetSiblingIndex();
    //        int bIndex = b.transform.GetSiblingIndex();
    //        return aIndex.CompareTo(bIndex);
    //    });

    //    return filtered;
    //}


    public ConnectedClientsData GetClientThroughID(ulong clientId)
    {
        for (int i = 0; i < connectedClientsData.Count; i++)
        {
            if (clientId == connectedClientsData[i].clientID)
                return connectedClientsData[i];
        }
        return null;
    }
}

[System.Serializable]   
public class ConnectedClientsData
{
    public ulong clientID;
    public GameObject playerGameobject;
    public bool isAlive;
    public string ClientName;
    public Color playerIndicatorColor;
    public float noiseValue;

    public ConnectedClientsData(ulong clientID, GameObject playerGameobject, bool isAlive)
    {
        this.clientID = clientID;
        this.playerGameobject = playerGameobject;
        this.isAlive = isAlive;
        noiseValue = 0;
    }
}