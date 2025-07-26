using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Barometer : NetworkBehaviour
{
    [Header("Properties")]
    public NetworkVariable<float> power_0_1 = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public List<float> power = new();
    public float addedPower = 0;
    public float avgPressure = 1;
    public List<GameObject> energySource = new();
    public float maxPower = 0.9f;
    public int[] barometerReadings;

    [Header("References")]
    public ItemPickup itemPickup;
    public ItemData itemData;
    public Inventory inventory;
    public Canvas canvas;
    public TMP_Text text;

    [Header("Visuals")]
    public GameObject bar;
    public float minPressureScale;
    public float maxPressureScale;

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
            GameManager.Instance.HelpInstructions.text = $"Holding the item taking itemData from DummyScript, Found : {itemData}";
        }

        barometerReadings = GameManager.Instance.GetComponent<SelectingThreeProcedures>().barometerReadings;
    }

    private void Update()
    {

        if (Input.GetMouseButtonUp(1))
        {
            if (inventory == null)
                inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();

            if (inventory?.selectedInventorySlot.itemData.itemType == itemData.itemType)
                itemData.isOn = !itemData.isOn;
        }

        if (itemData.isOn)
        {
            canvas?.gameObject.SetActive(true);
            if (text != null)
                text.text = addedPower.ToString("F2");
        }
        else
        {
            if (canvas != null)
                canvas.gameObject.SetActive(false);
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
        addedPower = Mathf.Clamp(avgPressure - addedPower + Random.Range(-.1f, .1f), 0, maxPower);

        power_0_1.Value = addedPower / maxPower;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) { return; }
        if (itemData.isOn && other.CompareTag("Energy"))
        {
            float distance = (other.transform.position - transform.position).magnitude;
            EnergyTrigger energyTrigger = other.GetComponent<EnergyTrigger>();
            float energyRange = energyTrigger.energyRange;
            float energy = energyTrigger.EnergyAmount;

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

