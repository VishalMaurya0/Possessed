using UnityEngine;

[CreateAssetMenu(fileName = "GhostData", menuName = "Scriptable Objects/GhostData")]
public class GhostData : ScriptableObject
{
    [Header("Ghost Data")]
    public float ghostLookDistance = 10f;
    public float height = 2f;
    public Vector3 eyePosition = Vector3.up * 2;
    public Vector3 eyePositionFromGround = Vector3.up * 4;
    public float timeBetweenHuntDuration = 250f;
    public float timeBetweenHuntDurationRange = 100f;
    public float thresholdVelocity = 0.2f;




    [Header("RoamingState")]
    public float showNearPPDuration = 5f;

    [Header("Wander")]
    public float roamingRadius = 100f;
    public float endRadius = 100f;
    public float idleDuration = 2f;
    public int positionFindingDuration = 60;
    public float playerPosOffsetRadius = 5;

    [Header("ShowingNearPP")]
    public float shownPPDurationMin = 0.2f;
    public float shownPPDurationMax = 2f;
    public float spawnRadiusNearPP = 1f;

    [Header("ChooseSpawn")]
    public float spawnCooldownDuration = 5f;



    [Header("HuntingState")]
    public float averageHuntDuration = 15f;
    public float timeAfterWhichHuntHuntDurDoubles = 1200;
    public float proceduresAfterWhichHuntHuntDurDoubles = 2;

    [Header("Wander")]
    public float huntRoamingRadius = 10;
    public float huntEndRadius = 5f;
    public int timeAfterWhichGhostStartWalkingToCentre = 60;
    public float maxNoiseClamp = 200;
}
