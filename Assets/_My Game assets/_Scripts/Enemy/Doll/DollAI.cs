using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DollAI : NetworkBehaviour
{
    [Header("References")]
    public List<Transform> players;
    public PlayerDataSO[] playerDataSOs;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Settings")]
    public NetworkVariable<bool> reversed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float playerDetectionRange = 50.0f;
    public float viewAngle = 60f;
    public float attackRange = 1.5f;
    public Transform[] patrolPoints;
    public float reactionTimeOfDoll = 0f;

    [Header("Values")]
    public GameObject playerInSight;
    public Vector3 posOfPlayer;

    private enum DollState { Idle, Chasing, Frozen, Attacking }
    [SerializeField] private DollState currentState = DollState.Idle;
    bool isFreezing = false;

    [Header("Optimization")]
    public float aiUpdateInterval = 0.1f;
    private float aiTimer;
    private float playerListCheckTimer;
    public List<FearMeter> fearMeters = new List<FearMeter>();

    private int sightLayerMask;
    private int viewBlockLayerMask;

    void Start()
    {
        // 1. RANDOMIZE SETTING ON START
        if (IsServer)
        {
            reversed.Value = UnityEngine.Random.value > 0.5f;
            //Debug.Log($"Doll Spawned. Reversed Mode: {reversed}");
        }

        if (animator)
        {
            animator.SetFloat("IdleIndex", UnityEngine.Random.Range(0, 2));
            animator.SetFloat("CrawlIndex", UnityEngine.Random.Range(0, 3));
        }

        sightLayerMask = ~(1 << LayerMask.NameToLayer("IgnoreRaycast"));

        viewBlockLayerMask = ~((1 << LayerMask.NameToLayer("IgnoreRaycast")) |
                               (1 << LayerMask.NameToLayer("Player")) |
                               (1 << LayerMask.NameToLayer("Trigger")));

        aiTimer = UnityEngine.Random.Range(0f, aiUpdateInterval);

        if (!IsServer) { return; }
    }

    private void UpdateConnectedPlayers()
    {
        var clients = GameManager.Instance.connectedClientsData;

        if (players == null || players.Count != clients.Count)
        {
            players.Clear();
            playerDataSOs = new PlayerDataSO[clients.Count];

            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].playerGameobject != null)
                {
                    while (players.Count <= i)
                    {
                        players.Add(null);
                    }
                    players[i] = clients[i].playerGameobject.transform;
                    var controller = players[i].GetComponent<PlayerController>();
                    if (controller != null)
                        playerDataSOs[i] = controller.playerData;
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!IsServer || !GameManager.Instance.serverStarted) return;

        playerListCheckTimer += Time.deltaTime;
        if (playerListCheckTimer > 5.0f)
        {
            UpdateConnectedPlayers();
            playerListCheckTimer = 0;
        }

        aiTimer += Time.deltaTime;
        if (aiTimer >= aiUpdateInterval)
        {
            aiTimer = 0;
            RunAIStateMachine();
        }
    }

    void RunAIStateMachine()
    {
        switch (currentState)
        {
            case DollState.Frozen:
                HandleFrozenState();
                break;
            case DollState.Chasing:
                HandleChaseState();
                break;
            case DollState.Idle:
                HandleIdleState();
                break;
            case DollState.Attacking:
                if (currentState == DollState.Attacking) AttackPlayer();
                break;
        }
    }

    void HandleIdleState()
    {
        animator.SetBool("Idle", true);
        agent.isStopped = true;

        bool inSight = IsPlayerInSight();

        // If we don't even see a player (in range/raycast), just stay idle.
        if (!inSight) return;

        bool isLookingAtMe = IsPlayerLookingAtDoll();

        // LOGIC SPLIT FOR REVERSED MODE
        if (reversed.Value)
        {
            // REVERSE MODE: 
            // Only chase if they ARE looking at me.
            if (isLookingAtMe)
            {
                currentState = DollState.Chasing;
            }
            // If they are not looking, stay Idle.
        }
        else
        {
            // NORMAL MODE:
            // If they look at me -> Freeze.
            if (isLookingAtMe)
            {
                StartCoroutine(FreezeRoutine());
            }
            // If they see me (IsPlayerInSight is true) but AREN'T looking -> Chase.
            else
            {
                currentState = DollState.Chasing;
            }
        }
    }

    void HandleChaseState()
    {
        animator.SetBool("Idle", false);
        bool isLookingAtMe = IsPlayerLookingAtDoll();

        // LOGIC SPLIT FOR REVERSED MODE
        if (reversed.Value)
        {
            // REVERSE MODE:
            // If they STOP looking at me, I should freeze/stop.
            if (!isLookingAtMe)
            {
                if (!isFreezing)
                {
                    StartCoroutine(FreezeRoutine());
                }
                return;
            }
        }
        else
        {
            // NORMAL MODE:
            // If they START looking at me, I should freeze.
            if (isLookingAtMe)
            {
                if (!isFreezing)
                {
                    StartCoroutine(FreezeRoutine());
                }
                return;
            }
        }

        // 2. Update Pathfinding
        if (playerInSight != null)
        {
            posOfPlayer = playerInSight.transform.position;
            agent.isStopped = false;
            animator.speed = 0.6f;
            agent.SetDestination(posOfPlayer);
        }

        // 3. Attack Check
        if (IsPlayerInAttackRange())
        {
            currentState = DollState.Attacking;
        }
    }

    void HandleFrozenState()
    {
        bool isLookingAtMe = IsPlayerLookingAtDoll();
        bool shouldUnfreeze = false;

        // LOGIC SPLIT FOR REVERSED MODE
        if (reversed.Value)
        {
            // REVERSE MODE: Unfreeze if they ARE looking at me
            if (isLookingAtMe) shouldUnfreeze = true;
        }
        else
        {
            // NORMAL MODE: Unfreeze if they are NOT looking at me
            if (!isLookingAtMe) shouldUnfreeze = true;
        }

        if (shouldUnfreeze)
        {
            currentState = DollState.Chasing;
            agent.isStopped = false;
            animator.speed = 0.6f;
        }
    }

    void AttackPlayer()
    {
        if (playerInSight == null)
        {
            currentState = DollState.Idle;
            return;
        }

        FearMeter fearMeter = playerInSight.GetComponent<FearMeter>();
        if (fearMeter != null)
        {
            fearMeter.instantPossess_Trigger = true;
            Debug.Log("Dead");
        }

        currentState = DollState.Idle;
    }

    IEnumerator FreezeRoutine()
    {
        if (currentState == DollState.Frozen || isFreezing) yield break;

        isFreezing = true;

        if (reactionTimeOfDoll > 0)
            yield return new WaitForSeconds(reactionTimeOfDoll);

        // Re-check logic before applying freeze (in case they looked away fast)
        bool isLookingAtMe = IsPlayerLookingAtDoll();
        bool shouldFreeze = false;

        if (reversed.Value)
        {
            // REVERSE: Freeze if they are NOT looking
            if (!isLookingAtMe) shouldFreeze = true;
        }
        else
        {
            // NORMAL: Freeze if they ARE looking
            if (isLookingAtMe) shouldFreeze = true;
        }

        if (!shouldFreeze)
        {
            isFreezing = false;
            yield break;
        }

        animator.speed = 0;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        currentState = DollState.Frozen;
        isFreezing = false;
    }

    // --- Helper Functions (Unchanged) ---

    bool IsPlayerInSight()
    {
        if (players == null) return false;

        for (int i = 0; i < players.Count; i++)
        {
            Transform targetPlayer = players[i];
            if (targetPlayer == null) continue;

            if (!GameManager.Instance.connectedClientsData[i].isAlive) continue;

            bool foundFearMeter = false;
            foreach (var fearMeter in fearMeters)
            {
                if (fearMeter.gameObject == targetPlayer.gameObject)
                {
                    foundFearMeter = true;
                    if (fearMeter.SAFE) continue;
                }
            }
            if (!foundFearMeter)
            {
                FearMeter fm = targetPlayer.GetComponent<FearMeter>();
                if (fm != null)
                {
                    fearMeters.Add(fm);
                    if (fm.SAFE) continue;
                }
            }

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 targetPos = targetPlayer.position;
            float distanceToPlayer_sq = (origin - targetPos).sqrMagnitude;

            if (distanceToPlayer_sq > playerDetectionRange * playerDetectionRange) continue;

            Vector3 directionToPlayer = (targetPos - origin).normalized;

            if (Physics.Raycast(origin, directionToPlayer, out RaycastHit hit, distanceToPlayer_sq, sightLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == targetPlayer.gameObject)
                {
                    playerInSight = targetPlayer.gameObject;
                    posOfPlayer = playerInSight.transform.position;
                    return true;
                }
            }
        }

        playerInSight = null;
        return false;
    }

    bool IsPlayerLookingAtDoll()
    {
        if (GameManager.Instance.gameEnd) return false;
        if (players == null) return false;

        Collider dollCollider = GetComponent<Collider>();
        Vector3 dollTargetPos = (dollCollider != null) ? dollCollider.bounds.center : transform.position + Vector3.up * 1.5f;

        for (int i = 0; i < players.Count; i++)
        {
            Transform targetPlayer = players[i];
            if (targetPlayer == null) continue;

            if (!GameManager.Instance.connectedClientsData[i].isAlive) continue;
            if (playerDataSOs[i] == null) continue;

            Vector3 eyePosition = targetPlayer.position + playerDataSOs[i].eyePosition;
            Vector3 directionToDollCenter = (dollTargetPos - eyePosition).normalized;
            float distanceToDoll_sq = (eyePosition - dollTargetPos).sqrMagnitude;

            if (distanceToDoll_sq < playerDetectionRange * playerDetectionRange)
            {
                Transform playerCam = targetPlayer.GetChild(0);
                float angle = Vector3.Angle(playerCam.forward, directionToDollCenter);
                float effectiveViewAngle = (distanceToDoll_sq < 62.5f) ? 8f : viewAngle;

                if (angle < effectiveViewAngle)
                {
                    if (Physics.Raycast(eyePosition, directionToDollCenter, out RaycastHit hit, distanceToDoll_sq + 5, viewBlockLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.gameObject == this.gameObject || hit.collider.transform.root == transform.root)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    bool IsPlayerInAttackRange()
    {
        if (playerInSight == null) return false;
        float distanceToPlayer = (transform.position - playerInSight.transform.position).sqrMagnitude;
        return distanceToPlayer <= attackRange * attackRange;
    }
}