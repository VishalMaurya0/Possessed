using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Procedure1Visuals : NetworkBehaviour
{
    
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
    }

    [ClientRpc]
    private void CompleteVisualsClientRpc(int i)
    {
        if (i == 0)
        {
            transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
        if (i == 1)
        {
            for (int j = 0; j < visualsTrigger[i].trigger.Count; j++)
            {
                transform.GetChild(0).GetChild(j + 1).gameObject.SetActive(visualsTrigger[i].trigger[j]);
            }
        }
        if (i == 2)
        {
            transform.GetChild(0).GetChild(procedureCompletion.totalItemsNeeded.itemNeeded[i-1].requiredAmount + 1).gameObject.SetActive(true);
        }
    }

}
