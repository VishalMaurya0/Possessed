using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LookingCursedIncreasesFear : MonoBehaviour
{
    public PlayerDataSO playerDataSO;
    FearMeter fearMeter;
    public Camera playerCamera;
    public LayerMask obstructionMask;


    public Vector3[] allPos;
    [SerializeField] GhostAI ghostAI;
    [SerializeField] List<DollAI> dollAI;
    [SerializeField] List<Collider> dollCollider;
    public int noOfDollsVisible = 0;


    private void MyStart()    
    {
        playerCamera = FindAnyObjectByType<Camera>();
        ghostAI = FindAnyObjectByType<GhostAI>();
        fearMeter = GetComponent<FearMeter>();

        DollsAdded();
        
    }

    private void DollsAdded()          //=========== run when new doll spawned ==========//
    {
        dollAI.Clear();
        dollAI.AddRange(FindObjectsByType<DollAI>(FindObjectsSortMode.None));
        dollCollider.Clear();
        foreach (var dollAi in dollAI)
        {
            dollCollider.Add(dollAi.gameObject.GetComponent<Collider>());
        }
    }

    private void Update()
    {
        if (dollAI == null || playerCamera == null || dollCollider == null)
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
        noOfDollsVisible = 0;

        for (int i = 0; i < dollAI.Count; i++)
        {
            if (dollCollider != null && GeometryUtility.TestPlanesAABB(cameraFrustum, dollCollider.ElementAtOrDefault(i).bounds))
            {
                Vector3 directionToDoll = dollAI.ElementAtOrDefault(i).transform.position - playerCamera.transform.position;
                float distanceToDoll = Vector3.Distance(playerCamera.transform.position, dollAI.ElementAtOrDefault(i).transform.position);

                if (Physics.Raycast(playerCamera.transform.position, directionToDoll, out RaycastHit hit, distanceToDoll + 5))
                {
                    if (hit.collider.transform == dollAI.ElementAtOrDefault(i).transform)
                    {
                        Debug.DrawLine(playerCamera.transform.position, hit.point, Color.green);
                        noOfDollsVisible++;
                    }
                }
            }
        }
        if (noOfDollsVisible > 0)
        {
            return true;
        }
        return false;
    }
}


