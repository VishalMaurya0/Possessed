using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Procedure2Visuals : NetworkBehaviour
{
    [Header("Visuals")]
    public GameObject Ghost;
    public GameObject Portal;
    public Animator animator;
    public NetworkVariable<bool> jailed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public GameObject watchingPlayer;

    [Header("For Showing Items")]
    public ProcedureCompletion procedureCompletion;
    public List<VisualsTrigger> visualsTrigger;

    private void Start()
    {
        visualsTrigger = procedureCompletion.visualsTrigger;
    }

    private void Update()
    {
        if (procedureCompletion.showVisual.Key)
        {
            //CompleteVisualsServerRpc(procedureCompletion.showVisual.Value);
            CompleteVisualsClientRpc(procedureCompletion.showVisual.Value);
            procedureCompletion.showVisual = new();
        }


        if (jailed.Value)
        {
            if (watchingPlayer != null)
            {
                float rotationSpeed = 5f; // adjust as needed
                Vector3 direction = watchingPlayer.transform.position - Ghost.transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Ghost.transform.rotation = Quaternion.Slerp(Ghost.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }

            }
        }
    }

    public void SpawnGhost()
    {
        animator.SetTrigger("Spawn");
        Ghost.SetActive(true);
    }

    public void Jailed()
    {
        for (int i = 0; i < GameManager.Instance.completedProcedure.Count; i++)
        {
            if (GameManager.Instance.completedProcedure.ElementAtOrDefault(i).Value == procedureCompletion.procedureData.procedure)
            {
                watchingPlayer = GameManager.Instance.completedProcedure.ElementAtOrDefault(i).Key;
            }
        }
        jailed.Value = true;
    }

    [ClientRpc]
    private void CompleteVisualsClientRpc(int i)
    {
        if (i == 0)
        {
            for (int j = 0; j < visualsTrigger[i].trigger.Count; j++)
            {
                transform.GetChild(j).gameObject.SetActive(visualsTrigger[i].trigger[j]);
            }
        }
        if (i == 1)
        {
            transform.GetChild(visualsTrigger[i-1].trigger.Count).gameObject.SetActive(true);
            StartCoroutine(StartProcedure());
        }
    }

    IEnumerator StartProcedure()
    {
        yield return new WaitForSeconds(8);
        Portal.SetActive(true);
    }

}
