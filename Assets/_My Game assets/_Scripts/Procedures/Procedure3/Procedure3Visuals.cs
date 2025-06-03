using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class Procedure3Visuals : NetworkBehaviour
{
    [Header("Item Visuals")]
    public ProcedureCompletion procedureCompletion;
    public List<VisualsTrigger> visualsTrigger;
    public MeshRenderer dollVFX;
    public GameObject realDoll;
    public GameObject decalHeart;

    [Header("Procedure Visuals")]
    public SpecialProcedure procedure3;


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
        if (i == -1)
        {
            dollVFX.enabled = true;
            realDoll.SetActive(false);
            decalHeart.SetActive(false);
        }
        if (i == 0)
        {
            dollVFX.enabled = false;
            realDoll.SetActive(true);
            decalHeart.SetActive(true);
            //NetworkObject obj = decalHeart.GetComponent<NetworkObject>();
            //obj.Spawn();
            //obj.setpare
        }
    }

}
