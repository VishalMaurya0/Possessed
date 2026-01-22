using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FearMeter : NetworkBehaviour
{
    [Header("Unchangable Data from SO")]
    public PlayerDataSO playerDataSO;
    public LookingCursedIncreasesFear lookingCursedIncreasesFear;
    public float normalFearRate;
    public float watchingGhostFearRate = 0f;
    public float watchingDollFearRate = 0f;
    public float ghostWatchingFearRate = 0f;
    public float regenFearRate = 0f;
    public float maxMoveDistanceWhenGettingPossessed;
    public float revivedFear = 0f;

    [Header("Values")]
    public float fearValue = 0f;
    public float normalFear = 0f;
    public float watchingGhostFear = 0f;
    public float watchingDollFear = 0f;
    public float additionalMovementFearWhenPossessing = 0f;
    public float ghostWatchingFear = 0f;
    public float regenFear = 0f;
    public float instantKillFear = 0f;
    public Vector3 freezePosition;

    [Header("Useful Data")]
    public bool isGhostLooking;
    public bool isLookingDoll;
    public bool isLookingGhost;
    public bool SAFE;
    public bool instantPossess_Trigger;
    public bool dollInstantPossess_Trigger;
    public bool revived;
    private bool freezing;
    bool isDead;

    int noOfDollsVisible = 0;

    [Header("Refe")]
    public PlayerController playerController;

    [Header("UI Elements")]
    public Slider fearBar;

    private void Start()
    {
        lookingCursedIncreasesFear = GetComponent<LookingCursedIncreasesFear>();


        normalFearRate = playerDataSO.normalFearRate;
        watchingGhostFearRate = playerDataSO.watchingGhostFearRate;
        watchingDollFearRate = playerDataSO.watchingDollFearRate;
        ghostWatchingFearRate = playerDataSO.ghostWatchingFearRate;
        regenFearRate = playerDataSO.regenFearRate;
        maxMoveDistanceWhenGettingPossessed = playerDataSO.maxFearDistance;
        revivedFear = playerDataSO.revivedFear;

        if (IsOwner && fearBar == null)
        {
            //Debug.LogError("Galat bar");
            fearBar = GameObject.FindWithTag("FearUI").GetComponent<Slider>();
        }
    }

    private void Update()
    {
        IncreaseFear();
        UpdateFearBarUI();
        noOfDollsVisible = lookingCursedIncreasesFear.noOfDollsVisible;
    }

    private void Freeze()
    {
        if (!freezing)
        {
            freezing = true;
            freezePosition = transform.position;
        }


        float distanceMoved = Vector3.Distance(freezePosition, transform.position);
        if (distanceMoved <= maxMoveDistanceWhenGettingPossessed)
        {
            additionalMovementFearWhenPossessing = Mathf.Lerp(0, 100, distanceMoved / maxMoveDistanceWhenGettingPossessed);
        }
        else
        {
            additionalMovementFearWhenPossessing = 100f;
        }
    }

    private void IncreaseFear()
    {
        fearValue = normalFear + ghostWatchingFear + watchingDollFear + watchingGhostFear + additionalMovementFearWhenPossessing + regenFear + instantKillFear;



        normalFear += normalFearRate * Time.deltaTime;
        if (isLookingDoll) { watchingDollFear += watchingDollFearRate * noOfDollsVisible * Time.deltaTime; }
        if (isLookingGhost) { watchingGhostFear += watchingGhostFearRate * Time.deltaTime; }
        if (isGhostLooking && !SAFE) { ghostWatchingFear += ghostWatchingFearRate * Time.deltaTime; }
        if (isGhostLooking && !SAFE) { Freeze(); }
        else { UnFreeze(); }
        if (SAFE) { regenFear -= regenFearRate*Time.deltaTime; }

        if (isGhostLooking)
        {
            GameManager.Instance.PostProcessEffect(true);
        }
        
        if (isLookingDoll)
        {
            GameManager.Instance.PostProcessEffect(true, noOfDollsVisible/5.0f);
        }
        
        if (isLookingGhost)
        {
            GameManager.Instance.PostProcessEffect(true, 0.5f);
        }
        
        if (!isGhostLooking && !isLookingDoll && !isLookingGhost)
        {
            GameManager.Instance.PostProcessEffect(false);
        }

        //----------------------Possess and Revive-------------------//
        if (instantPossess_Trigger)
        {
            instantPossess_Trigger = false;
            if (SAFE)
                return;
            instantKillFear = 100;
        }
        
        if (dollInstantPossess_Trigger)
        {
            dollInstantPossess_Trigger = false;
            if (SAFE)
                return;
            instantKillFear += 40;
        }

        if (revived) 
        {
            playerController.animator.SetBool("Dead", false);
            normalFear = revivedFear;
            ghostWatchingFear = 0;
            watchingDollFear = 0;
            watchingGhostFear = 0;
            regenFear = 0;
            instantKillFear = 0;
            revived = false;
            isDead = false;
        }


        fearValue = Mathf.Clamp(fearValue, 0, 100);


        if (fearValue >= 100 && !isDead)
        {
            isDead = true;
            playerController.animator.SetBool("Dead", true);
            playerController.animator.SetFloat("DeathIndex", Random.Range(0, 7));




            gameObject.GetComponent<PlayerDeathManager>().DieClientRpc();
        }
    }

    public void UnFreeze()
    {
        freezing = false;
        //additionalMovementFearWhenPossessing = 0f;
    }

    private void UpdateFearBarUI()
    {
        if (fearBar != null)
        {
            fearBar.value = fearValue;
        }
    }
}
