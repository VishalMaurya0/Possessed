using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class GhostAI : NetworkBehaviour
{
    public GhostState Currentstate;
    public GhostState RoamingState;
    public GhostState HuntingState;
    public GhostState DyingState;

    [HideInInspector]public NavMeshAgent navMeshAgent;  //should not reference this directly in inspectors
    public Animator animator;
    public GhostData ghostData;

    // State flags
    public bool isHunting;
    public bool stopHunt;
    public bool photoClicked;

    // Timers
    public float huntToStartTimer = 0;
    float timeBetweenHuntDuration;

    // Optimization: Throttling visibility checks
    private float _visibilityTimer;
    private const float VISIBILITY_CHECK_INTERVAL = 0.15f; // Check approx 6 times per second, not 60+
    private KeyValuePair<ulong, GameObject> _cachedTargetPlayer; // Store the last known target

    public TMP_Text text;

    int visibilityLayerMask; 
    
    void Start()
    {
        visibilityLayerMask = ~(1 << LayerMask.NameToLayer("IgnoreRaycast"));
        _animWalkingID = Animator.StringToHash("Walking");
        _animIdleIndexID = Animator.StringToHash("IdleIndex");
    }

    private void Update()
    {
        if (!IsServer) return;

        repathTimer += Time.deltaTime;
        // Initialization (Safety Check)
        if (navMeshAgent == null) InitializeAI();

        // Animation Handling
        HandleAnimation();

        // State Update
        Currentstate.UpdateState();

        // Hunt Timer Logic
        HandleHuntTimer();
    }

    private void InitializeAI()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        if (RoamingState == null || HuntingState == null || DyingState == null)
        {
            huntToStartTimer = 0;
            timeBetweenHuntDuration = ghostData.timeBetweenHuntDuration + Random.Range(-ghostData.timeBetweenHuntDurationRange, ghostData.timeBetweenHuntDurationRange);
            RoamingState = new GhostRoamingState(this);
            HuntingState = new GhostHuntingState(this);
            DyingState = new GhostDyingState(this);
            ChangeState(RoamingState);
        }
    }

    bool isMovingForOptimization = false;
    int _animWalkingID;
    int _animIdleIndexID;
    private void HandleAnimation()
    {
        if (navMeshAgent.isOnNavMesh)
        {
            // Optimization: compare squared magnitude to avoid square root calc
            bool isMoving = !navMeshAgent.isStopped && navMeshAgent.velocity.sqrMagnitude > 0.1f;

            if (isMovingForOptimization != isMoving) // Only set if changed
            {
                isMovingForOptimization = isMoving;
                animator.SetBool(_animWalkingID, isMoving);
                if (!isMoving) animator.SetFloat(_animIdleIndexID, Random.Range(0, 2));
            }
        }
    }

    private void HandleHuntTimer()
    {
        huntToStartTimer += Time.deltaTime;
        if (huntToStartTimer > timeBetweenHuntDuration && !isHunting)
        {
            huntToStartTimer = 0;
            ChangeState(HuntingState);
        }
        if (stopHunt)
        {
            ChangeState(RoamingState);
            stopHunt = false;
        }
    }

    public void ChangeState(GhostState newState)
    {
        Currentstate?.ExitState();
        Currentstate = newState;
        Currentstate?.EnterState();
        if (text != null) text.text = newState.ToString();
    }

    // ---------------- OPTIMIZED VISIBILITY LOGIC ---------------- //

    public bool CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> player)
    {
        player = default;

        // Optimization: Only run this heavy logic based on interval
        _visibilityTimer += Time.deltaTime;
        if (_visibilityTimer < VISIBILITY_CHECK_INTERVAL)
        {
            // Return the cached result from the last check so other scripts don't break
            if (_cachedTargetPlayer.Value != null)
            {
                if (!_cachedTargetPlayer.Value.GetComponent<FearMeter>().SAFE)
                {
                    player = _cachedTargetPlayer;
                    return true;
                }
            }
            return false;
        }

        _visibilityTimer = 0f; // Reset timer

        if (GameManager.Instance == null || GameManager.Instance.gameEnd || ghostData == null)
            return false;

        float minDisSqr = float.MaxValue;
        KeyValuePair<ulong, GameObject> bestTarget = default;
        bool foundTarget = false;

        // Loop directly through connected clients. No new arrays. No dictionaries.
        foreach (var clientData in GameManager.Instance.connectedClientsData)
        {
            if (!clientData.isAlive || clientData.playerGameobject == null) continue;

            Vector3 targetPos = clientData.playerGameobject.transform.position;
            Vector3 toPlayer = targetPos - transform.position;
            float distSqr = toPlayer.sqrMagnitude;
            float lookDistSqr = ghostData.ghostLookDistance * ghostData.ghostLookDistance;

            // 1. Distance Check (Fastest)
            if (distSqr > lookDistSqr) continue;

            //chck if player is in safe mode
            FearMeter fearMeter = clientData.playerGameobject.GetComponent<FearMeter>();
            if (fearMeter != null && fearMeter.SAFE) continue;

            // 2. Angle Check (Fast)
            Vector3 lookDir = transform.forward;
            Vector3 targetDirNorm = toPlayer.normalized;

            // Optimization: Dot product is faster than Angle. 
            // 40 degrees ~ 0.766 dot product. If dot > 0.766, it's within angle.
            // Using Angle for readability here, but Vector3.Dot is better for raw speed.
            if (Vector3.Angle(lookDir, targetDirNorm) < 40)
            {
                // 3. Raycast Check (Slowest - do this last)
                if (RaycastCheckIfPlayerIsVisible(targetDirNorm, targetPos, clientData.playerGameobject))
                {
                    // We found a visible player. Is he closer than the previous one we found?
                    if (distSqr < minDisSqr)
                    {
                        minDisSqr = distSqr;
                        bestTarget = new KeyValuePair<ulong, GameObject>(clientData.clientID, clientData.playerGameobject);
                        foundTarget = true;
                    }
                }
            }
        }

        if (foundTarget)
        {
            _cachedTargetPlayer = bestTarget;
            player = bestTarget;
            return true;
        }

        _cachedTargetPlayer = default;
        return false;
    }

    // Modified to take the specific target GameObject to avoid searching lists again
    public bool RaycastCheckIfPlayerIsVisible(Vector3 targetDir, Vector3 targetPos, GameObject targetObject)
    {
        Vector3 rayOrigin = transform.position + ghostData.eyePosition;

        // Use a static LayerMask if possible, getting it by name string every frame is slow
        int layerMask = visibilityLayerMask; // Assuming 2 is IgnoreRaycast. Ideally, store this in Start().

        // Check 1: Raycast to center
        if (Physics.Raycast(rayOrigin, targetDir, out RaycastHit hit, ghostData.ghostLookDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            // Optimization: Direct comparison. No list looping.
            if (hit.collider.gameObject == targetObject) return true;
            // If collider is child of player, use: hit.collider.transform.root.gameObject == targetObject
        }

        // Check 2: Raycast to slightly adjusted position (eye level)
        // Optimization: Calculate exact target eye position instead of generic calculation if possible
        Vector3 playerEyePos = targetPos + Vector3.up * 1.6f; // Assuming 1.6m height
        Vector3 dirToEyes = (playerEyePos - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, dirToEyes, out RaycastHit hit2, ghostData.ghostLookDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit2.collider.gameObject == targetObject) return true;
        }

        return false;
    }

    [ClientRpc]
    public void NotifyPlayersAMessageClientRPC(string message, int time)
    {
        // Null check to prevent errors
        if (GameManager.Instance != null && GameManager.Instance.HelpInstructions != null)
        {
            GameManager.Instance.HelpInstructions.text = message;
            GameManager.Instance.helpInstructionDisplayTime = time;
        }
    }

    public float repathTimer = 0;
    public void SetDestination(Vector3 pos)
    {
        if (repathTimer > 1)
        {
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                repathTimer = 0;
                navMeshAgent.SetDestination(pos);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // CompareTag is fast, this is fine.
        if (isHunting && collision.collider.CompareTag("Player"))
        {
            GameObject player = collision.collider.gameObject;
            // TryGetComponent is slightly faster and safer (no null exception if missing)
            if (player.TryGetComponent<FearMeter>(out var fearMeter))
            {
                fearMeter.instantPossess_Trigger = true;
                
                AudioManager.PlaySoundClientRpc(AudioType.ScreamJumpScare);
            }

            ChangeState(RoamingState);
            photoClicked = true;
        }

        if (Currentstate == RoamingState)
        {
            photoClicked = true;
        }
    }
}