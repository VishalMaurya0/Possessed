using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EMFReader : NetworkBehaviour
{
    [Header("References")]
    ItemPickup itemPickup;
    Inventory inventory;
    ItemData itemData;
    public GameObject nib;

    [Header("Properties")]
    public int[] EMFReadings; 
    public List<float> power = new();
    public float addedPower = 0;
    public float maxPower = 60;
    public List<GameObject> energySource = new();

    [Header("Visuals")]
    public MeshRenderer onRenderer;
    public float rot;
    public float maxRot = 26;



    private void Start()
    {
        StartCoroutine(NextFrame());
        GameManager.onServerStarted += GameManager_onServerStarted;
    }

    private void GameManager_onServerStarted()
    {
        inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
    }

    IEnumerator NextFrame()
    {
        yield return new WaitForEndOfFrame();
        itemPickup = GetComponent<ItemPickup>();
        itemData = itemPickup?.itemData;
        if (itemData == null)
        {
            var dsfci = GetComponent<DummyScriptForClassifyingItems>();
            itemData = dsfci.ItemData;
            dsfci.makeItSpringy = false;
            GameManager.Instance.HelpInstructions.text = $"Holding the item taking itemData from DummyScript, Found : {itemData.itemType}";
        }

        EMFReadings = GameManager.Instance.GetComponent<SelectingThreeProcedures>().EMFReadings;
    }


    private void Update()
    {
        if (itemData == null) return;
        if (Input.GetMouseButtonUp(1))
        {
            if (inventory == null)
                inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();

            if (inventory?.selectedInventorySlot.itemData.itemType == itemData.itemType)
                itemData.isOn = !itemData.isOn;
        }

        if (itemData.isOn)
        {
            onRenderer.material.EnableKeyword("_EMISSION");
        }
        else
        {
            onRenderer.material.DisableKeyword("_EMISSION");
        }

        if (!itemData.isOn)
        {
            return;
        }


        ManageWorking();
    }

    private void ManageWorking()
    {
        if (!IsServer) return;


        addedPower = 0;
        for (int i = 0; i < power.Count; i++)
        {
            addedPower += power[i];
        }
        addedPower = Mathf.Clamp(addedPower, 0, maxPower);

        Visuals();
    }

    private void Visuals()
    {
        nib.transform.localEulerAngles = new(nib.transform.localEulerAngles.x, (maxRot) - (2 * maxRot * addedPower / maxPower), nib.transform.localEulerAngles.z);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) { return; }
        if (itemData.isOn && other.CompareTag("Energy"))
        {
            float distance = (other.transform.position - transform.position).magnitude;
            EMFTrigger energyTrigger = other.GetComponent<EMFTrigger>();
            float energyRange = energyTrigger.Range;
            float energy = energyTrigger.Amount;

            if (!energySource.Contains(other.gameObject))
            {
                energySource.Add(other.gameObject);
                power.Add(0);
            }

            int i = energySource.IndexOf(other.gameObject);
            if (energyTrigger.isActive)
            {
                power[i] = (1 - (distance / energyRange)) * energy;
            }
            else
            {
                power[i] = 0;
            }

            power[i] = Mathf.Clamp(power[i], 0, maxPower);
        }
    }
}
