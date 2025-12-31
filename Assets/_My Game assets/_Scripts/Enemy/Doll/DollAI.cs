using System;
using System.Collections;
using System.Collections.Generic; // Added for List optimization
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DollAI : NetworkBehaviour
{
    [Header("References")]
    // We use a List for easier management, but keep arrays if you prefer specific indexing
    public Transform[] players;
    public PlayerDataSO[] playerDataSOs;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Settings")]
    public float playerDetectionRange = 50.0f; // Adjusted to reasonable default
    public float viewAngle = 60f; // Adjusted to reasonable default (30 is very narrow)
    public float attackRange = 1.5f;
    public Transform[] patrolPoints;
    public float reactionTimeOfDoll = 0f;

    [Header("Values")]
    public GameObject playerInSight;
    public Vector3 posOfPlayer;

    private enum DollState { Idle, Chasing, Frozen, Attacking }
    [SerializeField] private DollState currentState = DollState.Idle; // Serialized for debug
    bool isFreezing = false;

    [Header("Optimization")]
    public float aiUpdateInterval = 0.1f; // How often AI "thinks"
    private float aiTimer;
    private float playerListCheckTimer;

    // Cache LayerMasks to avoid calculating them every loop
    private int sightLayerMask;
    private int viewBlockLayerMask;

    void Start()
    {
        // Randomize initial animation to avoid all dolls looking identical
        if (animator)
        {
            animator.SetFloat("IdleIndex", UnityEngine.Random.Range(0, 2));
            animator.SetFloat("CrawlIndex", UnityEngine.Random.Range(0, 3));
        }

        // OPTIMIZATION: Cache LayerMasks once on startup
        // Include all layers except "IgnoreRaycast"
        sightLayerMask = ~(1 << LayerMask.NameToLayer("IgnoreRaycast"));

        // Blocked by everything except IgnoreRaycast, Player, and Trigger
        viewBlockLayerMask = ~((1 << LayerMask.NameToLayer("IgnoreRaycast")) |
                               (1 << LayerMask.NameToLayer("Player")) |
                               (1 << LayerMask.NameToLayer("Trigger")));

        // OPTIMIZATION: Stagger AI updates so 20 dolls don't think on the same frame
        aiTimer = UnityEngine.Random.Range(0f, aiUpdateInterval);

        if (!IsServer) { return; }
    }

    private void UpdateConnectedPlayers()
    {
        var clients = GameManager.Instance.connectedClientsData;

        if (players == null || players.Length != clients.Count)
        {
            players = new Transform[clients.Count];
            playerDataSOs = new PlayerDataSO[clients.Count];

            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].playerGameobject != null)
                {
                    players[i] = clients[i].playerGameobject.transform;
                    var controller = players[i].GetComponent<PlayerController>();
                    if (controller != null)
                        playerDataSOs[i] = controller.playerData;
                }
            }
        }
    }

    void Update()
    {
        if (!IsServer || !GameManager.Instance.serverStarted) return;

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        playerListCheckTimer += Time.deltaTime;
        if (playerListCheckTimer > 5.0f)
        {
            UpdateConnectedPlayers();
            playerListCheckTimer = 0;
        }

        // 3. AI Logic Throttling
        // The AI only "thinks" when aiTimer exceeds interval. 
        // Movement/Animation continues smoothly, but decision making is throttled.
        aiTimer += Time.deltaTime;
        if (aiTimer >= aiUpdateInterval)
        {
            aiTimer = 0; // Reset timer
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
                // Attacking usually needs immediate execution, but check is fine here
                if (currentState == DollState.Attacking) AttackPlayer();
                break;
        }
    }

    void HandleIdleState()
    {
        animator.SetBool("Idle", true);

        if (IsPlayerInSight())
        {
            if (IsPlayerLookingAtDoll())
            {
                StartCoroutine(FreezeRoutine());
            }
            else
            {
                currentState = DollState.Chasing;
            }
        }
    }

    void HandleChaseState()
    {
        animator.SetBool("Idle", false);

        if (IsPlayerLookingAtDoll())
        {
            if (!isFreezing)
            {
                StartCoroutine(FreezeRoutine());
            }
            return;
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
        // Optimization: This now runs 5-10 times a second instead of 60+
        // This is plenty fast enough for a "statue" mechanic
        if (!IsPlayerLookingAtDoll())
        {
            currentState = DollState.Chasing;
            agent.isStopped = false;
            animator.speed = 0.6f; // Restore anim speed
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
        }

        currentState = DollState.Idle;
    }

    IEnumerator FreezeRoutine()
    {
        // Guard clause to prevent double freezing
        if (currentState == DollState.Frozen || isFreezing) yield break;

        isFreezing = true;

        // Small delay before freezing (Reaction time)
        if (reactionTimeOfDoll > 0)
            yield return new WaitForSeconds(reactionTimeOfDoll);

        animator.speed = 0;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        currentState = DollState.Frozen;
        isFreezing = false;
    }

    bool IsPlayerInSight()
    {
        if (players == null) return false;

        for (int i = 0; i < players.Length; i++)
        {
            Transform targetPlayer = players[i];
            if (targetPlayer == null) continue;

            // Use direct index access instead of Array.IndexOf
            if (!GameManager.Instance.connectedClientsData[i].isAlive) continue;

            if (GameManager.Instance.connectedClientsData[i].playerGameobject.GetComponent<FearMeter>().SAFE) continue;

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 targetPos = targetPlayer.position;
            float distanceToPlayer = Vector3.Distance(origin, targetPos);

            // Optimization: Simple distance check before Raycast
            if (distanceToPlayer > playerDetectionRange) continue;

            Vector3 directionToPlayer = (targetPos - origin).normalized;

            // Debug can remain, but consider commenting out for final build
            // Debug.DrawRay(origin, directionToPlayer * (distanceToPlayer + 5f), Color.yellow);

            if (Physics.Raycast(origin, directionToPlayer, out RaycastHit hit, distanceToPlayer + 5f, sightLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == targetPlayer.gameObject)
                {
                    playerInSight = targetPlayer.gameObject;
                    posOfPlayer = playerInSight.transform.position;
                    return true;
                }
            }
        }

        // Only clear playerInSight if NO ONE is in sight. 
        // Note: Your original logic cleared it immediately if loop finished.
        playerInSight = null;
        return false;
    }

    bool IsPlayerLookingAtDoll()
    {
        if (GameManager.Instance.gameEnd) return false;
        if (players == null) return false;

        for (int i = 0; i < players.Length; i++)
        {
            Transform targetPlayer = players[i];
            if (targetPlayer == null) continue;

            // Use direct index access
            if (!GameManager.Instance.connectedClientsData[i].isAlive) continue;

            // Access data safely
            if (playerDataSOs[i] == null) continue;

            Vector3 eyePosition = targetPlayer.position + playerDataSOs[i].eyePosition;
            Vector3 directionToDoll = (transform.position - eyePosition).normalized;
            float distanceToDoll = Vector3.Distance(eyePosition, transform.position);

            if (distanceToDoll < playerDetectionRange)
            {
                // Optimization: GetChild(0) is usually Camera. Make sure this structure is consistent!
                // It is better to cache the Camera transform in UpdateConnectedPlayers if possible.
                Transform playerCam = targetPlayer.GetChild(0);

                float angle = Vector3.Angle(playerCam.forward, directionToDoll);
                if (angle < viewAngle)
                {
                    if (Physics.Raycast(eyePosition, directionToDoll, out RaycastHit hit, distanceToDoll + 5, viewBlockLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.gameObject == this.gameObject)
                        {
                            return true; // As soon as ONE player sees us, we return true
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
        float distanceToPlayer = Vector3.Distance(transform.position, playerInSight.transform.position);
        return distanceToPlayer <= attackRange;
    }
}