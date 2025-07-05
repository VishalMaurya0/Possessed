using System;
using UnityEngine;

public class EnergyDetector : MonoBehaviour
{
    public ItemPickup itemPickup;
    public ItemData itemData;
    public Material outer;
    public Material inner;
    public Color noEffectColor;
    public Color fullEffectColor;

    private void Start()
    {
        itemPickup = GetComponent<ItemPickup>();
        itemData = itemPickup?.itemData;
        if (itemData == null)
        {
            itemData = GetComponent<DummyScriptForClassifyingItems>().ItemData;
            GameManager.Instance.HelpInstructions.text = $"Holding the item taking itemData from DummyScript, Found : {itemData}";
        }
    }

    private void Update()
    {
        if (itemPickup == null) return;

        if (itemData.isOn)
        {
            outer.SetFloat("_Outer", 1);
        }else
        {
            outer.SetFloat("_Outer", 0);
        }

        if (!itemData.isOn) return;

        ManageWorking();
    }

    private void ManageWorking()
    {

    }
}
