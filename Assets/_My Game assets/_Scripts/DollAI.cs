using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DollAI : NetworkBehaviour
{
    [Header("References")]
    public Transform[] player;
    public PlayerDataSO[] playerDataSO;
    private NavMeshAgent agent;
    Animator animator;

    [Header("Settings")]
    public float playerDetectionRange = 500f;
    public float viewAngle = 30f;
    public float attackRange = 1.5f;
    public Transform[] patrolPoints;

    [Header("Values")]
    public GameObject playerInSight;
    public Vector3 posOfPlayer;
    private enum DollState { Idle, Chasing, Frozen, Attacking }
    private DollState currentState = DollState.Idle;


    void Start()
    {
        if (!IsServer) { return; }
    }

    private void SetAllConnectedPlayers()
    {
        player = new Transform[GameManager.Instance.connectedClients.Count];
        playerDataSO = new PlayerDataSO[player.Length];
        for (int i = 0; i < GameManager.Instance.connectedClients.Count; i++)
        {
            player[i] = GameManager.Instance.connectedClients.ElementAtOrDefault(i).Value.transform;
            playerDataSO[i] = player[i].GetComponent<PlayerController>().playerData;
        }
    }

    void Update()
    {
        if (!GameManager.Instance.serverStarted || !IsServer) return;
        if (agent == null || animator == null)  
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
        }
        if (player.Length < GameManager.Instance.connectedClients.Count)
        {
            SetAllConnectedPlayers();
        }

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
                AttackPlayer();
                break;
        }

        if (IsPlayerInSight())
        {
            posOfPlayer = playerInSight.transform.position;
        }
    }

    void HandleIdleState()
    {
        if (IsPlayerInSight())
        {
            if (IsPlayerLookingAtDoll())
            {
                Freeze();
            }
            else
            {
                currentState = DollState.Chasing;
            }
        }
    }

    void HandleChaseState()
    {
        if (IsPlayerLookingAtDoll())
        {
            Freeze();
            return;
        }

        animator.speed = 0.6f;
        agent.isStopped = false;
        agent.SetDestination(posOfPlayer);

        if (IsPlayerInAttackRange())
        {
            currentState = DollState.Attacking;
        }
    }

    void HandleFrozenState()
    {
        if (!IsPlayerLookingAtDoll())
        {
            currentState = DollState.Chasing;
            agent.isStopped = false;
        }
    }
    
    void AttackPlayer()
    {
        //Debug.Log("Doll attacked and possessed the player!");
        // Add attack mechanics here
    }







    
    void Freeze()
    {
        animator.speed = 0;
        agent.isStopped = true;
        currentState = DollState.Frozen;
    }

    bool IsPlayerInSight()
    {
        foreach (var playerr in player)
        {
            if (playerr == null)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 directionToPlayer = (playerr.transform.position - origin).normalized;
            float distanceToPlayer = Vector3.Distance(origin, playerr.transform.position);

            // Include all layers except "IgnoreRaycast"
            int layerMask = ~(1 << LayerMask.NameToLayer("IgnoreRaycast"));    ////Champt Gpt////

            Debug.DrawRay(origin, directionToPlayer * (distanceToPlayer + 5f), Color.yellow);

            if (Physics.Raycast(origin, directionToPlayer, out RaycastHit hit, distanceToPlayer + 5f, layerMask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"Raycast hit: {hit.collider.name} (layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                if (hit.collider.gameObject == playerr.gameObject)
                {
                    playerInSight = playerr.gameObject;
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
        foreach (var playerr in player)
        {
            Vector3 eyePosition = playerr.position + playerDataSO[playerr.GetComponentIndex()].eyePosition;
            Vector3 directionToDoll = (transform.position - eyePosition).normalized;
            float distanceToDoll = Vector3.Distance(eyePosition, transform.position);

            Debug.DrawRay(eyePosition, directionToDoll * (distanceToDoll + 5), Color.red, 0.1f);

            if (distanceToDoll < playerDetectionRange)
            {
                float angle = Vector3.Angle(playerr.transform.forward, directionToDoll);
                if (angle < viewAngle)
                {
                    if (Physics.Raycast(eyePosition, directionToDoll, out RaycastHit hit, distanceToDoll + 5))
                    {
                        if (hit.collider.gameObject == this.gameObject)
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
        float distanceToPlayer = Vector3.Distance(transform.position, playerInSight ? playerInSight.transform.position : Vector3.zero);
        return distanceToPlayer <= attackRange;
    }

    

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, detectionRange);

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, attackRange);

    //    Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2);
    //    Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRange);
    //    Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRange);
    //}

    //Vector3 DirectionFromAngle(float angleInDegrees)
    //{
    //    return Quaternion.Euler(0, angleInDegrees, 0) * transform.forward;
    //}
}
