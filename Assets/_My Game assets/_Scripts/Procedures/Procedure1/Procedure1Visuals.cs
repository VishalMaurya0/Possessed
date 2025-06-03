using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Procedure1Visuals : NetworkBehaviour
{
    [Header("Item Visuals")]
    public ProcedureCompletion procedureCompletion;
    public List<VisualsTrigger> visualsTrigger;
    public GameObject SmokeGhost;

    [Header("VFX Visuals")]
    public GameObject chainVFX;
    public List<GameObject> posG = new();
    public Vector3 additionalPos = new(0, -3, 0);
    public List<VisualEffect> visualEffects = new();
    public bool chainSpawned = false;


    private void Start()
    {
        visualsTrigger = procedureCompletion.visualsTrigger;
        SmokeGhost.SetActive(true);
    }

    private void Update()
    {
        if (procedureCompletion.showVisual.Key)
        {
            //CompleteVisualsServerRpc(procedureCompletion.showVisual.Value);
            CompleteVisualsClientRpc(procedureCompletion.showVisual.Value);
            procedureCompletion.showVisual = new();
        }
        if (chainSpawned)
        {
            SetPos();
        }
    }

    private void SetPos()
    {
        for (int i = 0; i < posG.Count; i++)
        {
            visualEffects[i].SetVector3("TargetPosition", posG[i].transform.position);
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
                GameObject obj = transform.GetChild(0).GetChild(j + 1).gameObject;
                obj.SetActive(visualsTrigger[i].trigger[j]);
                if (!posG.Contains(obj))
                    posG.Add(obj);
            }
        }
        if (i == 2)
        {
            transform.GetChild(0).GetChild(procedureCompletion.totalItemsNeeded.itemNeeded[i-1].requiredAmount + 1).gameObject.SetActive(true);
            SpawnChains();
        }
    }

    private void SpawnChains()
    {
        if (chainSpawned == true)
            return;

        chainSpawned = true;
        SmokeGhost.GetComponent<VisualEffect>().SetBool("DontChain", false);

        for (int i = 0; i < posG.Count; i++)
        {
            GameObject vfx = Instantiate(chainVFX, SmokeGhost.transform.position, Quaternion.identity);
            VisualEffect visualEffect = vfx.GetComponent<VisualEffect>();
            visualEffects.Add(visualEffect);
        }
    } 
}
