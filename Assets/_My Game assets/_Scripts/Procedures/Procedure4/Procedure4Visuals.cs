using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Procedure4Visuals : NetworkBehaviour
{
    [Header("References")]
    public GameObject doll;
    public GameObject dollShadow;
    public GameObject PowderVFX;
    public MeshRenderer Gem;
    public Material glow;
    public Material nonGlow;


    [Header("Item Visuals")]
    public ProcedureCompletion procedureCompletion;
    public List<VisualsTrigger> visualsTrigger;

    [Header("Properties")]
    public float timer;
    public float burningTime;
    public bool startTime;

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

        if (startTime)
        {
            timer += Time.deltaTime;
            if (timer > burningTime)
            {
                timer = 0;
                startTime = false;
                Gem.material = nonGlow;
                procedureCompletion.totalItemsNeeded.addedAmount[1] = 0;
                procedureCompletion.totalItemsNeeded.addedAmount[2] = 0;
                dollShadow.SetActive(false);
                dollShadow.GetComponent<Renderer>().material.SetFloat("_SwitchColor", 0f);
                if (IsServer)
                procedureCompletion.currentOrder.Value = 1;
            }
        }
    }


    [ClientRpc]
    private void CompleteVisualsClientRpc(int i)
    {
        if (i == 0)
        {
            doll.SetActive(true);
        }
        if (i == 1)
        {
            Gem.sharedMaterial = glow;
            dollShadow.SetActive(true);
            startTime = true;
        }
        if (i == 2)
        {
            dollShadow.GetComponent<Renderer>().material.SetFloat("_SwitchColor", 1f);
        }
        if (i == 3)
        {
            PowderVFX.gameObject.SetActive(true);
        }
    }
}
