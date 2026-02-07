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
    float startingHuntDuration;
    public float baseIgnorance;
    public float posChaseIgnorance;
    public bool sightChasing = false;
    public GameObject seenPlayer;




    public float ignoreNoises = 10f;
    public int maxNoiseIndex = -1;
    public Vector3 huntChaseTheNoisePosition = Vector3.zero;



    public override void EnterState()
    {
        ghostAI.NotifyPlayersAMessageClientRPC("Ghost is Highly Active, Be Careful!", 3);
        huntDurationTimer = 0;
        SetCurrentHuntSubState(huntWanderState);
        ghostAI.isHunting = true;
        startingHuntDuration = ghostAI.ghostData.startingHuntDuration;
        huntDuration = startingHuntDuration * ((GameManager.Instance.completedProcedures.Count / ghostAI.ghostData.proceduresAfterWhichHuntHuntDurDoubles) + 1) * ((GameManager.Instance.timeInSecElapsed / ghostAI.ghostData.timeAfterWhichHuntHuntDurDoubles) + 1);

        baseIgnorance = ghostAI.ghostData.baseIgnorance;
        posChaseIgnorance = ghostAI.ghostData.posChaseIgnorance;

        PlaySoundClientRpc(true);
    }

    float checkPlayerVisibilityTimer = 0;
    public override void UpdateState()
    {
        currentGhostHuntSubState.UpdateState();
        if (checkPlayerVisibilityTimer > 0.0f)
        {
            checkPlayerVisibilityTimer = 0;
            if (ghostAI.CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> player) && !sightChasing)
            {
                sightChasing = true;
                SetCurrentHuntSubState(HuntSightChaseState);
            }
        }
        huntDurationTimer += Time.deltaTime;
        if (huntDurationTimer > huntDuration)
        {
            Debug.Log("timeeeee");
            ghostAI.huntToStartTimer = 0;
            ghostAI.stopHunt = true;
        }


        if (ignoreNoises > baseIgnorance)
        {
            ignoreNoises -= ghostAI.ghostData.noiseForgettingRate * Time.deltaTime;
        }

    }


    public override void ExitState()
    {
        currentGhostHuntSubState.ExitState();
        ghostAI.isHunting = false;

        PlaySoundClientRpc(false);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(bool play)
    {
        if (play)
            ghostAI.intenseMusic_hunting.Play();
        else
            ghostAI.intenseMusic_hunting.Stop();

    }

    public void SetCurrentHuntSubState(GhostState state)
    {
        ghostAI.ghostDebugText2.text = state.ToString();
        currentGhostHuntSubState?.ExitState();
        currentGhostHuntSubState = state;
        currentGhostHuntSubState?.EnterState();
    }



    public void FindMaxNoiseIndexAndSetChasePosition()
    {
        float maxNoise = 0f;
        for (int i = 0; i < GameManager.Instance.connectedClientsData.Count; i++)
        {
            if (GameManager.Instance.connectedClientsData[i].noiseValue >= maxNoise && GameManager.Instance.connectedClientsData[i].noiseValue >= ignoreNoises)
            {
                maxNoise = GameManager.Instance.connectedClientsData[i].noiseValue;
                maxNoiseIndex = i;
            }
        }
        if (maxNoiseIndex != -1)
        {
            FindPosOfNoise();
        }else
        {
            huntChaseTheNoisePosition = Vector3.zero;
        }
    }
    public void FindPosOfNoise()
    {
        GameObject chasePlayer = GameManager.Instance.connectedClientsData.ElementAtOrDefault(maxNoiseIndex).playerGameobject;
        Vector3 chasePosition = Vector3.zero;
        if (chasePlayer != null)
            chasePosition = chasePlayer.transform.position;


        //--------------------------- Adjust positionPresitionRadius based on noise value-----------------------------//
        NoiseHandler noiseHandler = GameManager.Instance.connectedClientsData.ElementAtOrDefault(maxNoiseIndex).noiseHandler;
        if (noiseHandler == null)
        {
            noiseHandler = chasePlayer.GetComponent<NoiseHandler>();
            GameManager.Instance.connectedClientsData.ElementAtOrDefault(maxNoiseIndex).noiseHandler = noiseHandler;
        }
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

    private float noiseUpdateTimer = 0f;
    private float noiseUpdateInterval = 0.3f;

    public override void EnterState()
    {
        centrePosToChase = FindCentreOfPlayersPosition();
    }

    public override void UpdateState()
    {
        noiseUpdateTimer += Time.deltaTime;

        if (noiseUpdateTimer >= noiseUpdateInterval)
        {
            huntingState.FindMaxNoiseIndexAndSetChasePosition();
            noiseUpdateTimer = 0f; 
        }

        if (huntingState.ignoreNoises > huntingState.baseIgnorance && huntingState.huntChaseTheNoisePosition != Vector3.zero)
        {
            HuntNoisePosition();
            return;
        }

        if (huntingState.huntChaseTheNoisePosition == Vector3.zero)
        {
            if (atCentreOfPlayers)
                HuntRoam();
            else
                HuntToCentre();
        }
    }

    private void HuntRoam()
    {
        if (!ghostAI.navMeshAgent.pathPending && ghostAI.navMeshAgent.remainingDistance < 1)
        {
            if (ghostAI.repathTimer > 1)
            {
                ghostAI.SetDestination(FindHuntRoamPosition());
            }
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
        if (centrePosToChase == Vector3.zero)
        {
            Debug.LogError("UNOPTIMISED");
            centrePosToChase = FindCentreOfPlayersPosition();
            ghostAI.SetDestination(centrePosToChase);
        }
        if (centrePosToChase != Vector3.zero)
            atCentreOfPlayers = true;
    }

    void HuntNoisePosition()
    {
        ghostAI.SetDestination(huntingState.huntChaseTheNoisePosition);
        if (!ghostAI.navMeshAgent.pathPending && ghostAI.navMeshAgent.remainingDistance < 1)
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
        if (GameManager.Instance.gameEnd || GameManager.Instance.connectedClientsData.Count == 0)
        {
            return ghostAI.transform.position;
        }

        Vector3 addAll = Vector3.zero;


        int count = GameManager.Instance.connectedClientsData.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject player = GameManager.Instance.connectedClientsData.ElementAtOrDefault(i).playerGameobject;
            if (player != null)
                addAll += player.transform.position;
        }

        Vector3 centrePos = addAll / count;

        if (NavMesh.SamplePosition(centrePos, out NavMeshHit hit, ghostAI.ghostData.endRadius, NavMesh.AllAreas))
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

    float timer;
    float timeToCheckPlayerVisibility = 0f; //removed bcoz already added in func

    float timerForLoosingSeenPlayer = 0;
    float timeForLoosingSeenPlayer = 0.2f;
    bool startTimerForLoosingSeenPlayer = false;

    public override void EnterState()
    {

    }

    public override void UpdateState()
    {
        timer += Time.deltaTime;
        if (timer > timeToCheckPlayerVisibility)
        {
            timer = 0;
            if (ghostAI.CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> player))
            {
                startTimerForLoosingSeenPlayer = false;
                timerForLoosingSeenPlayer = 0;
                huntingState.seenPlayer = player.Value;
            }
            else
            {
                startTimerForLoosingSeenPlayer = true;
            }
        }
                
        if (startTimerForLoosingSeenPlayer)
        {
            timerForLoosingSeenPlayer += Time.deltaTime;
            if (timerForLoosingSeenPlayer > timeForLoosingSeenPlayer)
            {
                startTimerForLoosingSeenPlayer = false;
                timerForLoosingSeenPlayer = 0;
                huntingState.seenPlayer = null;
            }
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


    float repathTimer = 0;
    private void ChasePlayer()
    {
        repathTimer += Time.deltaTime;
        if (repathTimer > 0.2f)
        {
            repathTimer = 0;
            ghostAI.SetDestination(huntingState.seenPlayer.transform.position);
        }
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