using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Procedure5Visuals : NetworkBehaviour
{
    [Header("References")]
    public GameObject feather;
    public GameObject blood;
    public GameObject ghost;
    public GameObject fire;

    [Header("Properties")]
    [ColorUsage(true, true)] public Color fadedColor;
    [ColorUsage(true, true)] public Color darkColor;
    public Material bloodMaterial;


    [Header("Item Visuals")]
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
            feather.SetActive(true);
        }
        if (i == 1)
        {
            blood.SetActive(true);
            float lerpAmount = procedureCompletion.totalItemsNeeded.addedAmount[1] / procedureCompletion.totalItemsNeeded.itemNeeded[1].requiredAmount;
            if (bloodMaterial == null)
            {
                bloodMaterial = blood.GetComponent<Renderer>().material;
            }
            
            Color amount = Color.Lerp(fadedColor, darkColor, lerpAmount);
            bloodMaterial.SetColor("_WaterColor", amount);
        }
        if (i == 2)
        {
            float lerpAmount = procedureCompletion.totalItemsNeeded.addedAmount[2]/procedureCompletion.totalItemsNeeded.itemNeeded[2].requiredAmount;
            float amount = Mathf.Lerp(2, 0.4f, lerpAmount);
            Debug.LogError(amount);
            bloodMaterial.SetFloat("_FoamCutoff", amount);
        }
        if (i == 3)
        {
            fire.SetActive(true);
        }
    }
}