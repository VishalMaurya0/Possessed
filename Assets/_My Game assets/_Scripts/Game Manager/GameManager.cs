using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Network Settings")]
    public GameObject ownerPlayer;

    public int noOfPlayers;             //----------maintained on server---------//
    public bool serverStarted = false;
    public bool gameStarted = false;

    public Dictionary<ulong, GameObject> connectedClients = new();
    public Dictionary<int, float> noiseValues = new();
    public int[] selectedProceduresIndex;
    public List<int> completedProcedures;
    public float timeInSecElapsed = 0;

    public bool lockCurser = false;
    public bool handlePlayerLookWithMouse = true;
    public ProcedureBase procedureBase;
    




    public static GameManager Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        { Destroy(this); }
        Instance = this;

        procedureBase = GetComponent<ProcedureBase>();
    }
    private void Update()
    {
        timeInSecElapsed += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            lockCurser = !lockCurser;
        }
    }

}
