using System.Collections.Generic;
using UnityEngine;

public class Procedure0Visuals : MonoBehaviour
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
            CompleteVisuals(procedureCompletion.showVisual.Value);
            procedureCompletion.showVisual = new();
        }
    }

    private void CompleteVisuals(int i)
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
            transform.GetChild(procedureCompletion.totalItemsNeeded.addedAmount[i-1]).gameObject.SetActive(true);
        }
    }

}
