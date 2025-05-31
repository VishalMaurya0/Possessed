using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Procedure2Visuals : NetworkBehaviour
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
            for (int j = 0; j < visualsTrigger[i].trigger.Count; j++)
            {
                transform.GetChild(j).gameObject.SetActive(visualsTrigger[i].trigger[j]);
            }
        }
        if (i == 1)
        {
            transform.GetChild(visualsTrigger[i-1].trigger.Count).gameObject.SetActive(true);
        }
    }

}
