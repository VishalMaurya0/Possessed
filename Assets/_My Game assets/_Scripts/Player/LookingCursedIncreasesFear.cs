using UnityEngine;

public class LookingCursedIncreasesFear : MonoBehaviour
{
    public PlayerDataSO playerDataSO;
    FearMeter fearMeter;
    public Camera playerCamera;
    public LayerMask obstructionMask;


    public Vector3[] allPos;
    [SerializeField] GhostAI ghostAI;
    [SerializeField] DollAI dollAI;
    Collider dollCollider;


    private void MyStart()
    {
        playerCamera = FindAnyObjectByType<Camera>();
        ghostAI = FindAnyObjectByType<GhostAI>();
        dollAI = FindAnyObjectByType<DollAI>();
        fearMeter = GetComponent<FearMeter>();

        dollCollider = dollAI.GetComponent<Collider>();
    }

    private void Update()
    {
        if (dollAI == null)
        {
            MyStart();
        }
        if (CheckGhostVisibility())
        {
            fearMeter.isLookingGhost = true;
        }
        else
        {
            fearMeter.isLookingGhost = false;   
        }

        if (CheckDollVisibility())
        {
            fearMeter.isLookingDoll = true;
        }
        else
        {
            fearMeter.isLookingDoll = false;
        }
    }

    public bool CheckGhostVisibility()
    {
        Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        Collider ghostCollider = ghostAI.GetComponent<Collider>();

        if (ghostCollider != null && GeometryUtility.TestPlanesAABB(cameraFrustum, ghostCollider.bounds))
        {
            Vector3 directionToGhost = ghostAI.transform.position - playerCamera.transform.position;
            float distanceToGhost = Vector3.Distance(playerCamera.transform.position, ghostAI.transform.position);

            if (Physics.Raycast(playerCamera.transform.position, directionToGhost, out RaycastHit hit, distanceToGhost + 5))
            {
                if (hit.collider.transform == ghostAI.transform)
                {
                    Debug.DrawLine(playerCamera.transform.position, hit.point, Color.green);
                    return true;
                }
            }
            if (Physics.Raycast(playerCamera.transform.position, directionToGhost + ghostAI.ghostData.eyePosition, out RaycastHit hit2, distanceToGhost + 5))
            {
                if (hit2.collider.transform == ghostAI.transform)
                {
                    Debug.DrawLine(playerCamera.transform.position, hit2.point, Color.green);
                    return true;
                }
            }
        }

        return false;
    }


    public bool CheckDollVisibility()
    {
        Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        if (dollCollider != null && GeometryUtility.TestPlanesAABB(cameraFrustum, dollCollider.bounds))
        {
            Vector3 directionToDoll = dollAI.transform.position - playerCamera.transform.position;
            float distanceToDoll = Vector3.Distance(playerCamera.transform.position, dollAI.transform.position);

            if (Physics.Raycast(playerCamera.transform.position, directionToDoll, out RaycastHit hit, distanceToDoll + 5))
            {
                if (hit.collider.transform == dollAI.transform)
                {
                    Debug.DrawLine(playerCamera.transform.position, hit.point, Color.green);
                    return true;
                }
            }
        }

        return false;
    }
}


