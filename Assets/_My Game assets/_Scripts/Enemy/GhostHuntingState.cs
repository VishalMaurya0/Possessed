using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class GhostHuntingState : GhostState
{
    public GhostState currentGhostHuntSubState;
    public GhostState huntWanderState;
    public GhostState HuntSightChaseState;
    public GhostState HuntPosChaseState;

    public GhostHuntingState(GhostAI ghostAI) : base(ghostAI)
    {
        huntWanderState = new HuntWanderState(ghostAI, this);
        HuntSightChaseState = new HuntSightChaseState(ghostAI, this);
        HuntPosChaseState = new HuntPosChaseState(ghostAI, this);
    }

    [Header("Hunt Settings")]
    float huntDuration;
    float huntDurationTimer = 0;
    float averageHuntDuration;
    public float baseIgnorance = 10f;
    public float posChaseIgnorance = 20;
    public bool sightChasing = false;
    public GameObject seenPlayer;




    public float ignoreNoises = 10f;
    public int maxNoiseIndex = -1;
    public Vector3 huntChaseTheNoisePosition = Vector3.zero;


    public override void EnterState()
    {
        huntDurationTimer = 0;
        SetCurrentHuntSubState(huntWanderState);
        ghostAI.isHunting = true;
        averageHuntDuration = ghostAI.ghostData.averageHuntDuration;
        huntDuration = averageHuntDuration * (GameManager.Instance.completedProcedures.Count / ghostAI.ghostData.proceduresAfterWhichHuntHuntDurDoubles + 1) * (GameManager.Instance.timeInSecElapsed / ghostAI.ghostData.timeAfterWhichHuntHuntDurDoubles + 1);
    }

    public override void UpdateState()
    {
        
        currentGhostHuntSubState.UpdateState();
        if (ghostAI.CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> player) && !sightChasing)
        {
            sightChasing = true;
            SetCurrentHuntSubState(HuntSightChaseState);
            Debug.Log(player);
        }

        huntDurationTimer += Time.deltaTime;
        if (huntDurationTimer > huntDuration)
        {
            Debug.Log("timeeeee");
            ghostAI.huntToStartTimer = 0;
            ghostAI.stopHunt = true;
        }
    }

    public override void ExitState()
    {
        currentGhostHuntSubState.ExitState();
        ghostAI.isHunting = false;
    }
    public void SetCurrentHuntSubState(GhostState state)
    {

        currentGhostHuntSubState?.ExitState();
        currentGhostHuntSubState = state;
        currentGhostHuntSubState?.EnterState();
    }



    public void FindMaxNoiseIndexAndSetChasePosition()
    {
        float maxNoise = 0f;
        for (int i = 0; i < GameManager.Instance.connectedClients.Count; i++)
        {
            if (GameManager.Instance.noiseValues[i] > maxNoise && GameManager.Instance.noiseValues[i] > ignoreNoises)
            {
                maxNoise = GameManager.Instance.noiseValues[i];
                maxNoiseIndex = i;
            }
        }
        if (maxNoiseIndex != -1)
        {
            FindPosOfNoise();
        }
    }
    public void FindPosOfNoise()
    {
        GameObject chasePlayer = GameManager.Instance.connectedClients.ElementAtOrDefault(maxNoiseIndex).Value;
        Vector3 chasePosition = Vector3.zero;
        if (chasePlayer == null)
        chasePosition = chasePlayer.transform.position;


        //--------------------------- Adjust positionPresitionRadius based on noise value-----------------------------//
        NoiseHandler noiseHandler = chasePlayer.GetComponent<NoiseHandler>();
        float noiseValue = noiseHandler.noiseValue;
        float positionPrecitionRadius = noiseHandler.positionPresitionRadius;
        float a = noiseValue / ghostAI.ghostData.maxNoiseClamp;
        float clamped = Mathf.Clamp01(a);
        clamped = 1 - clamped;
        positionPrecitionRadius = positionPrecitionRadius * clamped;

        huntChaseTheNoisePosition = chasePosition + new Vector3(Random.Range(-positionPrecitionRadius, positionPrecitionRadius), 0, Random.Range(-positionPrecitionRadius, positionPrecitionRadius));


        //-------------------------- Set the max noise index and ignorance----------------------//
        maxNoiseIndex = -1;
        ignoreNoises = noiseValue;
    }
}
















public class HuntWanderState : GhostState
{
    public new readonly GhostAI ghostAI;
    readonly GhostHuntingState huntingState;
    public HuntWanderState(GhostAI ghostAI, GhostHuntingState huntingState) : base(ghostAI) 
    {
        this.ghostAI = ghostAI;
        this.huntingState = huntingState;
    }

    Vector3 centrePosToChase;
    bool atCentreOfPlayers;
    bool againChaseCentre;       //=======global one time var========//
    float againChaseCentreTimer;

    public override void EnterState()
    {
        centrePosToChase = FindCentreOfPlayersPosition();
    }

    public override void UpdateState()
    {
        if (huntingState.huntChaseTheNoisePosition == Vector3.zero)
        {
            if (atCentreOfPlayers)
                HuntRoam();
            else
                HuntToCentre();
        }
        else
        {
            HuntNoisePosition();
        }
        huntingState.FindMaxNoiseIndexAndSetChasePosition();
    }

    private void HuntRoam()
    {
        if (ghostAI.navMeshAgent.remainingDistance < 1)
        {
            ghostAI.navMeshAgent.SetDestination(FindHuntRoamPosition());
        }
        if (!againChaseCentre)
            return;




        againChaseCentreTimer += Time.deltaTime;
        if (againChaseCentreTimer > ghostAI.ghostData.timeAfterWhichGhostStartWalkingToCentre)
        {
            atCentreOfPlayers = false;
            againChaseCentreTimer = 0;
        }
    }

    void HuntToCentre()
    {
        centrePosToChase = FindCentreOfPlayersPosition();
        ghostAI.navMeshAgent.SetDestination(centrePosToChase);
        if (centrePosToChase != Vector3.zero)
            atCentreOfPlayers = true;
    }

    void HuntNoisePosition()
    {
        ghostAI.navMeshAgent.SetDestination(huntingState.huntChaseTheNoisePosition);
        if (ghostAI.navMeshAgent.remainingDistance < 1)
        {
            huntingState.huntChaseTheNoisePosition = Vector3.zero;
            huntingState.ignoreNoises = huntingState.baseIgnorance;
        }
    }
    
    public override void ExitState()
    {
        
    }

    public Vector3 FindCentreOfPlayersPosition()
    {
        if (GameManager.Instance.gameEnd)
        {
            return default;
        }
        Vector3[] playersPosition = new Vector3[GameManager.Instance.connectedClients.Count];
        Vector3 addAll = Vector3.zero;
        for (int i = 0; i < playersPosition.Length; i++)
        {
            playersPosition[i] = GameManager.Instance.connectedClients.ElementAtOrDefault(i).Value.transform.position;
            addAll += playersPosition[i];
        }
        Vector3 centrePos = addAll/playersPosition.Length;


        Vector3 huntRoamPosition = centrePos;

        if (NavMesh.SamplePosition(huntRoamPosition, out NavMeshHit hit, ghostAI.ghostData.endRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return ghostAI.transform.position;
    }

    public Vector3 FindHuntRoamPosition()
    {
        Vector3 randomPosition = Random.insideUnitSphere * ghostAI.ghostData.huntRoamingRadius;
        randomPosition += ghostAI.transform.position;

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, ghostAI.ghostData.huntEndRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return ghostAI.transform.position;
    }
}




















public class HuntSightChaseState : GhostState
{
    public new readonly GhostAI ghostAI;
    readonly GhostHuntingState huntingState;
    public HuntSightChaseState(GhostAI ghostAI, GhostHuntingState huntingState) : base(ghostAI) 
    {
        this.ghostAI = ghostAI;
        this.huntingState = huntingState;
    }

    public override void EnterState()
    {

    }

    public override void UpdateState()
    {
        if (ghostAI.CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> player))
        {
            huntingState.seenPlayer = player.Value;
        }
        else
        {
            huntingState.seenPlayer = null;
        }

        if (huntingState.seenPlayer != null)
        {
            ChasePlayer();
        }
        else
        {
            huntingState.sightChasing = false;
            huntingState.ignoreNoises = huntingState.posChaseIgnorance;
            huntingState.SetCurrentHuntSubState(huntingState.huntWanderState);
        }
    }


    private void ChasePlayer()
    {
        ghostAI.navMeshAgent.SetDestination(huntingState.seenPlayer.transform.position);
    }

    public override void ExitState()
    {

    }
}






















public class HuntPosChaseState : GhostState
{
    public new readonly GhostAI ghostAI;
    readonly GhostHuntingState huntingState;

    public HuntPosChaseState(GhostAI ghostAI, GhostHuntingState huntingState) : base(ghostAI)
    {
        this.ghostAI = ghostAI;
        this.huntingState = huntingState;
    }


    public override void EnterState()
    {
        Debug.Log("Entering Roam Wandering State");
    }

    public override void UpdateState()
    {
        Debug.Log("Updating Roam Wandering State");
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Roam Wandering State");
    }
}