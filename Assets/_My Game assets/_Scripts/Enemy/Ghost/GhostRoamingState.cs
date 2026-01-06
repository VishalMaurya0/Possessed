using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Netcode;
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
        //Debug.Log("entering rws");
        SetNewDestination();
    }

    float visibilityCheckTimer;
    public override void UpdateState()
    {
        if (GameManager.Instance.gameEnd) return;
        if (ghostAI.navMeshAgent.isOnNavMesh && !ghostAI.navMeshAgent.pathPending && (ghostAI.navMeshAgent.remainingDistance < 1f || ghostAI.navMeshAgent.isStopped))
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

        visibilityCheckTimer += Time.deltaTime;
        if (visibilityCheckTimer > 0.5f)
        {
            visibilityCheckTimer = 0f;
            if (ghostAI.CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> seenPlayer))
            {
                roamingState.seenPlayer = seenPlayer;
                roamingState.SetCurrentRoamSubState(roamingState.RoamPossessingState);
                return;
            }
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
        //Debug.Log("Exiting Roam Wandering State");
    }

    private void SetNewDestination()
    {
        if (playerPosition == Vector3.zero && ghostAI.navMeshAgent.isOnNavMesh)
        {
            //Debug.LogError("running");
            playerPosition = FindRoamingPosition();
            ghostAI.SetDestination(playerPosition);
        }else if (ghostAI.navMeshAgent.isOnNavMesh)
        {
            ghostAI.SetDestination(playerPosition);
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

    private List<Vector3> _cachedAlivePositions = new List<Vector3>();
    void FindPositionOfRandomPlayer()
    {
        // Don't create a new array via FindPlayersPosition(). 
        // Access the GameManager list directly.
        var clients = GameManager.Instance.connectedClientsData;

        if (clients.Count == 0) return;

        // Filter alive players first
        // Note: Creating a temporary list here is cheaper than a specialized method, 
        // but ideally, GameManager maintains a list of 'AlivePlayers' separate from 'ConnectedClients'.
        _cachedAlivePositions.Clear();
        foreach (var client in clients)
        {
            if (client.isAlive && client.playerGameobject != null)
                _cachedAlivePositions.Add(client.playerGameobject.transform.position);
        }

        if (_cachedAlivePositions.Count == 0) return;

        float offsetRadius = ghostAI.ghostData.playerPosOffsetRadius;
        Vector3 offset = new Vector3(Random.Range(-offsetRadius, offsetRadius), 0f, Random.Range(-offsetRadius, offsetRadius));

        Vector3 targetPos = _cachedAlivePositions[Random.Range(0, _cachedAlivePositions.Count)] + offset;

        // SamplePosition is expensive. Ensure we are actually near navmesh?
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, ghostAI.ghostData.playerPosOffsetRadius, NavMesh.AllAreas))
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

    FearMeter fearMeter;

    public override void EnterState()
    {
        NotifyPlayersAboutPossessionClientRPC(roamingState.seenPlayer.Key);
        if (ghostAI.navMeshAgent.isOnNavMesh)
        {
            ghostAI.navMeshAgent.isStopped = true;
            fearMeter = roamingState.seenPlayer.Value.GetComponent<FearMeter>();
            fearMeter.isGhostLooking = true;
        }

        ghostAI.animator.SetBool("Possess", true);
        ghostAI.animator.SetFloat("PossessIndex", Random.Range(0, 2));

    }

    [ClientRpc]
    private void NotifyPlayersAboutPossessionClientRPC(ulong id)
    {
        if (GameManager.Instance.localID == id) return;
        GameManager.Instance.HelpInstructions.text = "Ghost is Possessing Someone, Find and Distract the Ghost";
    }

    public override void UpdateState()
    {
        CheckForPossessionStop();
        if (roamingState.stopPossession__Trigger)
        {
            roamingState.stopPossession__Trigger = false;
            ghostAI.NotifyPlayersAMessageClientRPC($"Ghost is regaining its Powers and gone for {ghostAI.ghostData.spawnCooldownDuration} secs", 4);
            roamingState.SetCurrentRoamSubState(roamingState.RoamChooseSpawnLocationState);
        }
    }

    public override void ExitState()
    {
        ghostAI.navMeshAgent.isStopped = false;
        fearMeter.isGhostLooking = false;
        
        
        ghostAI.animator.SetBool("Possess", false);
    }

    void CheckForPossessionStop()
    {
        if (fearMeter == null)
        {
            Debug.LogError("fear meter null");
            roamingState.stopPossession__Trigger = true;
            return;
        }
        if (fearMeter.fearValue >= 100 || ghostAI.photoClicked || fearMeter.SAFE)
        {
            roamingState.stopPossession__Trigger = true;
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
    private Vector3 FindNewLocation()     
    {
        Vector3 randomPosition = Random.insideUnitSphere * ghostAI.ghostData.spawnRadiusAfterCaught;
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
        foreach (var player in GameManager.Instance.connectedClientsData)
        {
            if (!player.isAlive) continue;
            if (Physics.Raycast(eyePos, player.playerGameobject.transform.position - eyePos, out RaycastHit hit, (pos - player.playerGameobject.transform.position).sqrMagnitude))
            {
                if (hit.collider.gameObject == player.playerGameobject)
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


        ghostAI.animator.SetBool("ShowPP", true);
        ghostAI.animator.SetFloat("ShowPPIndex", Random.Range(0, 3));
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
        ghostAI.SetDestination(initialTargetPos);
        ghostAI.navMeshAgent.isStopped = false;


        ghostAI.animator.SetBool("ShowPP", false);
    }


    private void ShowNearPP()
    {
        int[] threePPIndex = GameManager.Instance.selectedProceduresIndex;
        int selectedIndex = threePPIndex[Random.Range(0, 3)];
        float sr = ghostAI.ghostData.spawnRadiusNearPP;
        Vector3 pos = GameManager.Instance.procedureBase.position[selectedIndex] + new Vector3(Random.Range(-sr, sr), ghostAI.ghostData.height / 2 , Random.Range(-sr, sr));
        Debug.Log(pos + "Show near PP");
        ghostAI.transform.position = pos;
        
    }
}
