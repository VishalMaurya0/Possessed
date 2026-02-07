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
    public AudioSource intenseMusic_hunting;

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
    [Header("Ghost Debug texts")]
    public TMP_Text ghostDebugText1;
    public TMP_Text ghostDebugText2;

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
        ghostDebugText1.text = newState.ToString();
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
            Vector3 eyePos = transform.position + ghostData.eyePosition;
            Vector3 toPlayer = targetPos - eyePos;
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

            Vector3 dir = toPlayer.normalized;
            Vector3 flatLookDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 flatTargetDir = new Vector3(dir.x, 0f, dir.z).normalized;

            float halfFovRad = ghostData.viewAngle * 0.5f * Mathf.Deg2Rad;
            float cosHalfFov = Mathf.Cos(halfFovRad);

#if UNITY_EDITOR
            float drawDist = ghostData.ghostLookDistance;

            Quaternion leftRot = Quaternion.AngleAxis(-ghostData.viewAngle * 0.5f, Vector3.up);
            Quaternion rightRot = Quaternion.AngleAxis(ghostData.viewAngle * 0.5f, Vector3.up);

            Debug.DrawRay(eyePos, leftRot * flatLookDir * drawDist, Color.yellow);
            Debug.DrawRay(eyePos, rightRot * flatLookDir * drawDist, Color.yellow);

            // Forward direction
            Debug.DrawRay(eyePos, flatLookDir * drawDist, Color.white);
#endif

#if UNITY_EDITOR
            Color rayColor = Color.red;
#endif

            bool inFOV = Vector3.Dot(flatLookDir, flatTargetDir) > cosHalfFov;

#if UNITY_EDITOR
            if (inFOV)
                rayColor = Color.cyan;
#endif

            /// if player is close dont check the angle
            float closeRangeSqr = 2.0f * 2.0f; // 2 meters

            if (distSqr < closeRangeSqr)
            {
                inFOV = true;
            }


            if (inFOV)
            {
                //Debug.LogError("infov");
#if UNITY_EDITOR
                Debug.DrawLine(eyePos, targetPos, rayColor);
#endif

                if (RaycastCheckIfPlayerIsVisible(dir, targetPos, clientData.playerGameobject))
                {

                    if (distSqr < minDisSqr)
                    {
                        minDisSqr = distSqr;
                        bestTarget = new KeyValuePair<ulong, GameObject>(
                            clientData.clientID,
                            clientData.playerGameobject
                        );
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
            Debug.DrawRay(rayOrigin, targetDir * ghostData.ghostLookDistance, Color.blue);

        // Check 2: Raycast to slightly adjusted position (eye level)
        // Optimization: Calculate exact target eye position instead of generic calculation if possible
        Vector3 playerEyePos = targetPos + Vector3.up * 0.9f; // Assuming .9m height
        Vector3 dirToEyes = (playerEyePos - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, dirToEyes, out RaycastHit hit2, ghostData.ghostLookDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit2.collider.gameObject == targetObject) return true;
        }
        Debug.DrawRay(rayOrigin, dirToEyes * ghostData.ghostLookDistance, Color.blue);

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