using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LookingCursedIncreasesFear : MonoBehaviour
{
    public PlayerDataSO playerDataSO;
    FearMeter fearMeter;
    public Camera playerCamera;

    [Header("Settings")]
    public LayerMask obstructionMask; // Make sure this includes Walls, Default, AND the Enemy Layer!
    public float checkInterval = 0.2f; // Optimization: Don't run this every frame

    [Header("References")]
    [SerializeField] GhostAI ghostAI;
    [SerializeField] List<DollAI> dollAI = new List<DollAI>(); // Initialize to avoid null errors

    // Cache the frustum planes to save memory allocation
    private Plane[] cameraFrustum;
    public int noOfDollsVisible = 0;
    private float timer = 0;

    private void Start()
    {
        playerCamera = GameManager.Instance.playerCamera;
        ghostAI = FindAnyObjectByType<GhostAI>();
        fearMeter = GetComponent<FearMeter>();

        DollsAdded();
    }

    public void DollsAdded()
    {
        dollAI.Clear();
        // FindObjectsByType is slow, only call this when necessary
        dollAI.AddRange(FindObjectsByType<DollAI>(FindObjectsSortMode.None));

        // Remove nulls just in case
        dollAI.RemoveAll(item => item == null);
    }

    private void Update()
    {
        // FAILSAFE: Recover references if lost
        if (playerCamera == null) playerCamera = GameManager.Instance.playerCamera;
        if (fearMeter == null) fearMeter = GetComponent<FearMeter>();

        // OPTIMIZATION: Run visibility checks 5 times a second, not 60+
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0;

        // Calculate Frustum once per check
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        fearMeter.isLookingGhost = CheckGhostVisibility();
        fearMeter.isLookingDoll = CheckDollVisibility();
    }

    public bool CheckGhostVisibility()
    {
        if (ghostAI == null) return false;

        Collider ghostCollider = ghostAI.GetComponent<Collider>();
        if (ghostCollider == null) return false;

        // 1. Frustum Check (Is it on screen?)
        if (GeometryUtility.TestPlanesAABB(cameraFrustum, ghostCollider.bounds))
        {
            // 2. Line of Sight Check (Raycast)
            // TARGETING CENTER (Better than pivot/feet)
            Vector3 targetCenter = ghostCollider.bounds.center;

            // TARGETING EYES (If defined)
            Vector3 targetEyes = ghostAI.transform.position + ghostAI.ghostData.eyePosition;

            if (CanSeeTarget(targetCenter, ghostAI.transform) || CanSeeTarget(targetEyes, ghostAI.transform))
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckDollVisibility()
    {
        noOfDollsVisible = 0;

        foreach (var doll in dollAI)
        {
            if (doll == null) continue;

            Collider col = doll.GetComponent<Collider>();
            if (col == null) continue;

            // 1. Frustum Check
            if (GeometryUtility.TestPlanesAABB(cameraFrustum, col.bounds))
            {
                // 2. Line of Sight Check
                // Aim for the center of the collider (usually chest/torso)
                if (CanSeeTarget(col.bounds.center, doll.transform))
                {
                    noOfDollsVisible++;
                }
            }
        }

        return noOfDollsVisible > 0;
    }

    private bool CanSeeTarget(Vector3 targetPos, Transform targetTransform)
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = targetPos - origin; // Correct Vector Math: Destination - Origin
        float distance = direction.magnitude;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance + 1f, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform == targetTransform || hit.collider.transform.IsChildOf(targetTransform))
            {
                Debug.DrawLine(origin, hit.point, Color.green, 0.2f);
                return true;
            }
            else
            {
                 //Uncomment to debug what is blocking the view
                 Debug.Log("Blocked by: " + hit.collider.name);
            }
        }
        return false;
    }
}