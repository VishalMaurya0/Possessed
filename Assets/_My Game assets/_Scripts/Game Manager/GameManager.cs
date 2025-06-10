using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Network Settings")]
    public GameObject ownerPlayer;
    public bool serverStarted = false;
    public bool gameStarted = false;
    public bool gameEnd = false;


    [Header("One Time Variables")]
    [Tooltip("Maintained on Server")] public int noOfPlayers;             //----------maintained on server---------//
    public int inventorySlots = 5;
    public float maxWeight = 15;


    [Header("In Game Info")]
    public Dictionary<ulong, GameObject> connectedClients = new();
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
}
