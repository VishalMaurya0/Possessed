using UnityEngine;
using UnityEngine.UI;

public class FearMeter : MonoBehaviour
{
    [Header("Unchangable Data")]
    public PlayerDataSO playerDataSO;
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
    public bool instantPossess;
    public bool revived;
    private bool freezing;

    [Header("UI Elements")]
    public Slider fearBar;

    private void Start()
    {
        normalFearRate = playerDataSO.normalFearRate;
        watchingGhostFearRate = playerDataSO.watchingGhostFearRate;
        watchingDollFearRate = playerDataSO.watchingDollFearRate;
        ghostWatchingFearRate = playerDataSO.ghostWatchingFearRate;
        regenFearRate = playerDataSO.regenFearRate;
        maxMoveDistanceWhenGettingPossessed = playerDataSO.maxFearDistance;
        revivedFear = playerDataSO.revivedFear;

        fearBar = FindAnyObjectByType<Slider>();
    }

    private void Update()
    {
        IncreaseFear();
        UpdateFearBarUI();
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
        if (isLookingDoll) { watchingDollFear += watchingDollFearRate * Time.deltaTime; }
        if (isLookingGhost) { watchingGhostFear += watchingGhostFearRate * Time.deltaTime; }
        if (isGhostLooking && !SAFE) { ghostWatchingFear += ghostWatchingFearRate * Time.deltaTime; }
        if (isGhostLooking && !SAFE) { Freeze(); }
        else { UnFreeze(); }
        if (SAFE) { regenFear -= regenFearRate*Time.deltaTime; }

        //----------------------Possess and Revive-------------------//
        if (instantPossess) 
        {
            instantPossess = false;
            instantKillFear = 100; 
        }

        if (revived) 
        {
            normalFear = revivedFear;
            ghostWatchingFear = 0;
            watchingDollFear = 0;
            watchingGhostFear = 0;
            regenFear = 0;
            instantKillFear = 0;
            revived = false;
        }


        fearValue = Mathf.Clamp(fearValue, 0, 100);

    }

    public void UnFreeze()
    {
        freezing = false;
        additionalMovementFearWhenPossessing = 0f;
    }

    private void UpdateFearBarUI()
    {
        if (fearBar != null)
        {
            fearBar.value = fearValue;
        }
    }
}
