using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Network Settings")]
    public GameObject ownerPlayer;

    public int noOfPlayers;             //----------maintained on server---------//
    public int inventorySlots = 5;
    public float maxWeight = 15;
    public bool serverStarted = false;
    public bool gameStarted = false;
    public bool gameEnd = false;

    public Dictionary<ulong, GameObject> connectedClients = new();
    public Dictionary<int, float> noiseValues = new();
    public int[] selectedProceduresIndex;
    public List<int> completedProcedures;
    public float timeInSecElapsed = 0;

    public bool lockCurser = false;
    public bool handlePlayerLookWithMouse = true;
    public bool handleMovement = true;
    public bool itemScrollingLock = false;
    public bool bakeNavMeshAgain = false;

    public ProcedureBase procedureBase;
    

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

    public void OnServerStarted()
    {
        onServerStarted?.Invoke();
    }
}
