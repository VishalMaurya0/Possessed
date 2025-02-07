using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostRoamingState : GhostState
{

    public KeyValuePair<ulong, GameObject> seenPlayer;
    public bool stopPossession__Trigger;


    public GhostState currentGhostRoamSubState;
    public GhostState RoamWanderingState;
    public GhostState RoamPossessingState;
    public GhostState RoamChooseSpawnLocationState;
    public GhostState RoamShowingNearPP;

    public GhostRoamingState(GhostAI ghostAI) : base(ghostAI)
    {
        RoamWanderingState = new RoamWanderingState(ghostAI, this);
        RoamPossessingState = new RoamPossessingState(ghostAI, this);
        RoamChooseSpawnLocationState = new RoamChooseSpawnLocationState(ghostAI, this);
        RoamShowingNearPP = new RoamShowingNearPPState(ghostAI, this);
    }

    public override void EnterState()
    {
        SetCurrentRoamSubState(RoamWanderingState);
        currentGhostRoamSubState.EnterState();
    }

    public override void UpdateState()
    {
        currentGhostRoamSubState.UpdateState();
    }

    public override void ExitState()
    {
        currentGhostRoamSubState.ExitState();
    }

    public void SetCurrentRoamSubState(GhostState state)
    {
        currentGhostRoamSubState?.ExitState();
        currentGhostRoamSubState = state;
        currentGhostRoamSubState?.EnterState();
    }
}













public class RoamWanderingState : GhostState
{
    new readonly GhostAI ghostAI;
    readonly GhostRoamingState roamingState;
    float idleTimer;
    float posFindTimer;
    float showNearPPTimer;
    Vector3 playerPosition;


    public RoamWanderingState(GhostAI ghostAI, GhostRoamingState roamingState) : base(ghostAI)
    {
        this.ghostAI = ghostAI;
        this.roamingState = roamingState;
    }

    public override void EnterState()
    {
        Debug.Log("entering rws");
        SetNewDestination();
    }

    public override void UpdateState()
    {
        if (ghostAI.navMeshAgent.remainingDistance < 1f)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= ghostAI.ghostData.idleDuration)
            {
                idleTimer = 0f;
                SetNewDestination();
            }
        }

        posFindTimer += Time.deltaTime;
        if (posFindTimer >= ghostAI.ghostData.positionFindingDuration)
        {
            posFindTimer = 0f;
            FindPositionOfRandomPlayer();
        }

        if (ghostAI.CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> seenPlayer))
        {
            roamingState.seenPlayer = seenPlayer;
            roamingState.SetCurrentRoamSubState(roamingState.RoamPossessingState);
        }

        showNearPPTimer += Time.deltaTime;
        if (showNearPPTimer >= ghostAI.ghostData.showNearPPDuration - Mathf.Clamp(GameManager.Instance.timeInSecElapsed/6, 0, ghostAI.ghostData.showNearPPDuration - 10))
        {
            showNearPPTimer = 0f;
            roamingState.SetCurrentRoamSubState(roamingState.RoamShowingNearPP);
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Roam Wandering State");
    }

    private void SetNewDestination()
    {
        if (playerPosition == Vector3.zero)
        {
            ghostAI.navMeshAgent.SetDestination(FindRoamingPosition());
        }else
        {
            ghostAI.navMeshAgent.SetDestination(playerPosition);
            playerPosition = Vector3.zero;
        }
    }

    public Vector3 FindRoamingPosition()
    {
        Vector3 randomPosition = Random.insideUnitSphere * ghostAI.ghostData.roamingRadius;
        randomPosition += ghostAI.transform.position;

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, ghostAI.ghostData.endRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return ghostAI.transform.position;
    }

    void FindPositionOfRandomPlayer()
    {
        Vector3[] allPosition = ghostAI.FindPlayersPosition();
        float offsetRadius = ghostAI.ghostData.playerPosOffsetRadius;
        Vector3 offset = new(Random.Range(-offsetRadius, offsetRadius), 0f, Random.Range(-offsetRadius, offsetRadius));
        Vector3 pos = allPosition[Random.Range(0, allPosition.Length)] + offset;
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, ghostAI.ghostData.playerPosOffsetRadius, NavMesh.AllAreas))
        {
            playerPosition = hit.position;
        }
    }
}



















public class RoamPossessingState : GhostState
{
    new readonly GhostAI ghostAI;
    readonly GhostRoamingState roamingState;
    public RoamPossessingState(GhostAI ghostAI, GhostRoamingState roamingState) : base(ghostAI)
    {
        this.ghostAI = ghostAI;
        this.roamingState = roamingState;
    }

    public override void EnterState()
    {
        ghostAI.navMeshAgent.isStopped = true;
        roamingState.seenPlayer.Value.GetComponent<FearMeter>().isGhostLooking = true;
    }

    public override void UpdateState()
    {
        Debug.Log("Possessing");
        CheckForPossessionStop();
        if (roamingState.stopPossession__Trigger)
        {
            roamingState.stopPossession__Trigger = false;
            roamingState.SetCurrentRoamSubState(roamingState.RoamChooseSpawnLocationState);
        }
    }

    public override void ExitState()
    {
        ghostAI.navMeshAgent.isStopped = false;
        roamingState.seenPlayer.Value.GetComponent<FearMeter>().isGhostLooking = false;
    }

    void CheckForPossessionStop()
    {
        if (roamingState.seenPlayer.Value.GetComponent<FearMeter>().fearValue >= 100 || ghostAI.photoClicked)
        {
            roamingState.stopPossession__Trigger = true;                //=======goto #197 ==========//
        }
    }
}













public class RoamChooseSpawnLocationState : GhostState
{
    new readonly GhostAI ghostAI;
    readonly GhostRoamingState roamingState;
    public RoamChooseSpawnLocationState(GhostAI ghostAI, GhostRoamingState roamingState) : base(ghostAI)
    {
        this.ghostAI = ghostAI;
        this.roamingState = roamingState;
    }

    Vector3 spawnLocation = Vector3.zero;
    float spawnCooldownTimer;

    public override void EnterState()
    {
        spawnCooldownTimer = 0;
        while (spawnLocation == Vector3.zero)
        {
            spawnLocation = FindNewLocation();
        }
        ghostAI.transform.position = spawnLocation;
    }

    
    public override void UpdateState()
    {
        spawnCooldownTimer += Time.deltaTime;
        if (spawnCooldownTimer > ghostAI.ghostData.spawnCooldownDuration)
        {
            roamingState.SetCurrentRoamSubState(roamingState.RoamWanderingState);
        }
    }

    public override void ExitState()
    {
        
    }
    private Vector3 FindNewLocation()         //TODO============***Change the roaming radius to new var in this case ***==============//
    {
        Vector3 randomPosition = Random.insideUnitSphere * ghostAI.ghostData.roamingRadius;
        randomPosition += ghostAI.transform.position;

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, ghostAI.ghostData.endRadius, NavMesh.AllAreas))
        {
            if (!AnyPlayerVisible(hit.position))
                return hit.position;
        }
        return Vector3.zero;
    }

    private bool AnyPlayerVisible(Vector3 pos)
    {
        Vector3 eyePos = pos + ghostAI.ghostData.eyePositionFromGround;
        foreach (var player in GameManager.Instance.connectedClients)
        {
            if (Physics.Raycast(eyePos, player.Value.transform.position - eyePos, out RaycastHit hit, Vector3.Distance(pos, player.Value.transform.position)))
            {
                if (hit.collider.gameObject == player.Value)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
















public class RoamShowingNearPPState : GhostState
{
    Vector3 initialPosition;
    Vector3 initialRotation;
    Vector3 initialTargetPos;

    float showingTimer;
    float showingDuration;


    new readonly GhostAI ghostAI;
    readonly GhostRoamingState roamingState;
    public RoamShowingNearPPState(GhostAI ghostAI, GhostRoamingState roamingState) : base(ghostAI)
    {
        this.ghostAI = ghostAI;
        this.roamingState = roamingState;
    }

    public override void EnterState()
    {
        initialPosition = ghostAI.transform.position;
        initialRotation = ghostAI.transform.rotation.eulerAngles;
        initialTargetPos = ghostAI.navMeshAgent.pathEndPosition;
        ghostAI.navMeshAgent.isStopped = true;
        showingDuration = Random.Range(ghostAI.ghostData.shownPPDurationMin, ghostAI.ghostData.shownPPDurationMax);
        ShowNearPP();
    }

    public override void UpdateState()
    {
        showingTimer += Time.deltaTime;
        if (showingTimer > showingDuration)
        {
            showingTimer = 0f;
            roamingState.SetCurrentRoamSubState(roamingState.RoamWanderingState);
        }
    }
    public override void ExitState()
    {
        ghostAI.transform.SetPositionAndRotation(initialPosition, Quaternion.Euler(initialRotation));
        ghostAI.navMeshAgent.SetDestination(initialTargetPos);
        ghostAI.navMeshAgent.isStopped = false;
    }


    private void ShowNearPP()
    {
        int[] threePPIndex = GameManager.Instance.selectedProceduresIndex;
        int selectedIndex = threePPIndex[Random.Range(0, 3)];
        float sr = ghostAI.ghostData.spawnRadiusNearPP;
        Vector3 pos = GameManager.Instance.procedureBase.position[selectedIndex] + new Vector3(Random.Range(-sr, sr), ghostAI.ghostData.height / 2 , Random.Range(-sr, sr));
        Debug.Log(pos);
        ghostAI.transform.position = pos;
        
    }
}
