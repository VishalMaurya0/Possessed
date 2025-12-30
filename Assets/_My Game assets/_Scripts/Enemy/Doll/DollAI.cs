using System;
using System.Collections;
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
    public float reactionTimeOfDoll = 0f;

    [Header("Values")]
    public GameObject playerInSight;
    public Vector3 posOfPlayer;
    private enum DollState { Idle, Chasing, Frozen, Attacking }
    private DollState currentState = DollState.Idle;
    bool isFreezing = false;

    [Header("Optimization")]
    public float timerForCheck = 0;
    public float timeForCheck = 0.2f;
    

    void Start()
    {
        if (!IsServer) { return; }
    }

    private void SetAllConnectedPlayers()
    {
        player = new Transform[GameManager.Instance.connectedClientsData.Count];
        playerDataSO = new PlayerDataSO[player.Length];
        for (int i = 0; i < GameManager.Instance.connectedClientsData.Count; i++)
        {
            player[i] = GameManager.Instance.connectedClientsData.ElementAtOrDefault(i).playerGameobject.transform;
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
        if (player.Length < GameManager.Instance.connectedClientsData.Count)
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

        //if (IsPlayerInSight())
        //{
        //    posOfPlayer = playerInSight.transform.position;
        //}
    }

    void HandleIdleState()
    {
        timerForCheck += Time.deltaTime;
        if (timerForCheck > timeForCheck + 0.3f)
        {
            timerForCheck = 0;
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
    }

    void HandleChaseState()
    {
        timerForCheck += Time.deltaTime;
        if (timerForCheck > timeForCheck)
        {
            timerForCheck = 0;

            if (IsPlayerLookingAtDoll())
            {
                if (!isFreezing)
                {
                    StartCoroutine(Freeze());
                }
                return;
            }

            if (playerInSight != null)
                posOfPlayer = playerInSight.transform.position;

            animator.speed = 0.6f;
            agent.isStopped = false;

            agent.SetDestination(posOfPlayer);

            if (IsPlayerInAttackRange())
            {
                currentState = DollState.Attacking;
            }
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
        // TODO Attack Animations

        // Player Death
  
        FearMeter fearMeter = playerInSight.GetComponent<FearMeter>();
        fearMeter.instantPossess_Trigger = true;
        currentState = DollState.Idle;
    }








    IEnumerator Freeze()
    {
        isFreezing = true;
        yield return new WaitForSeconds(reactionTimeOfDoll);
        animator.speed = 0;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            currentState = DollState.Frozen;
            isFreezing = false;
        }
    }

    bool IsPlayerInSight()
    {
        foreach (var playerr in player)
        {
            if (playerr == null)
            {
                continue;
            }
            // Eliminating Dead Players
            int index = Array.IndexOf(player, playerr);
            if (!GameManager.Instance.connectedClientsData[index].isAlive) continue;


            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 directionToPlayer = (playerr.transform.position - origin).normalized;
            float distanceToPlayer = Vector3.Distance(origin, playerr.transform.position);

            // Include all layers except "IgnoreRaycast"
            int layerMask = ~(1 << LayerMask.NameToLayer("IgnoreRaycast"));    ////Champt Gpt////

            Debug.DrawRay(origin, directionToPlayer * (distanceToPlayer + 5f), Color.yellow);

            if (Physics.Raycast(origin, directionToPlayer, out RaycastHit hit, distanceToPlayer + 5f, layerMask, QueryTriggerInteraction.Ignore))
            {
                //Debug.Log($"Raycast hit: {hit.collider.name} (layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                if (hit.collider.gameObject == playerr.gameObject)
                {
                    playerInSight = playerr.gameObject;
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
        foreach (var playerr in player)
        {
            // Eliminating Dead Players
            int index = Array.IndexOf(player, playerr);
            if (!GameManager.Instance.connectedClientsData[index].isAlive) continue;


            Vector3 eyePosition = playerr.position + playerDataSO[playerr.GetComponentIndex()].eyePosition;
            Vector3 directionToDoll = (transform.position - eyePosition).normalized;
            float distanceToDoll = Vector3.Distance(eyePosition, transform.position);

            Debug.DrawRay(eyePosition, directionToDoll * (distanceToDoll + 5), Color.red, 0.1f);

            if (distanceToDoll < playerDetectionRange)
            {
                float angle = Vector3.Angle(playerr.GetChild(0).transform.forward, directionToDoll);
                if (angle < viewAngle)
                {
                    int layerMask = ~((1 << LayerMask.NameToLayer("IgnoreRaycast")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Trigger")));

                    if (Physics.Raycast(eyePosition, directionToDoll, out RaycastHit hit, distanceToDoll + 5, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.gameObject == this.gameObject)
                        {
                            Debug.DrawLine(eyePosition, hit.point, Color.green); // Visual Debug
                            return true;
                        }
                        else
                        {
                            Debug.DrawLine(eyePosition, hit.point, Color.red); // Visual Debug
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
    //    // 1. Draw Attack Range (Red Sphere)
    //    Gizmos.color = new Color(1, 0, 0, 0.3f);
    //    Gizmos.DrawWireSphere(transform.position, attackRange);

    //    // 2. Draw Detection Range (Cyan Wire)
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawWireSphere(transform.position, playerDetectionRange);

    //    // 3. Draw "Current Target" Line (Yellow)
    //    if (playerInSight != null)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawLine(transform.position + Vector3.up, playerInSight.transform.position + Vector3.up);
    //        Gizmos.DrawWireSphere(playerInSight.transform.position + Vector3.up, 0.5f);
    //    }

    //    // 4. Draw Player View Cones (Purple)
    //    if (player != null)
    //    {
    //        foreach (var p in player)
    //        {
    //            if (p == null) continue;
    //            Transform cam = p.GetChild(0);
    //            if (cam != null)
    //            {
    //                Gizmos.color = Color.magenta;
    //                Vector3 direction = cam.forward;
    //                // Draw a simple line representing the center of their view
    //                Gizmos.DrawRay(cam.position, direction * 5f);

    //                // Optional: Draw the "Angle" limits approximately
    //                // (Visualizing a 3D cone is hard in simple Gizmos, but lines help)
    //                Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * direction;
    //                Vector3 right = Quaternion.Euler(0, viewAngle, 0) * direction;
    //                Gizmos.DrawRay(cam.position, left * 5f);
    //                Gizmos.DrawRay(cam.position, right * 5f);
    //            }
    //        }
    //    }
    //}
}
