using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EnergyDetector : NetworkBehaviour
{
    [Header("Properties")]
    public NetworkVariable<float> power_0_1 = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public List<float> power = new();
    public float addedPower = 0;
    public List<GameObject> energySource = new();
    private float maxPowerOfEachDecrement = 50;
    public float maxPower = 50;
    public int[] energyDetectorReadings;

    [Header("References")]
    public ItemPickup itemPickup;
    public ItemData itemData;
    public Inventory inventory;
    public Canvas canvas;
    public TMP_Text text;

    [Header("Visuals")]
    public Material outer;
    public Material inner;
    [ColorUsage(true, true)] public Color noEffectColor;
    [ColorUsage(true, true)] public Color midEffectColor;
    [ColorUsage(true, true)] public Color fullEffectColor;

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
            GameManager.Instance.helpInstructionDisplayTime = 5f;
        }

        energyDetectorReadings = GameManager.Instance.GetComponent<SelectingThreeProcedures>().EnergyDetectorReadings;
    }

    private void Update()
    {

        if (Input.GetMouseButtonUp(1) && itemPickup.dsfci)
        {
            if (inventory == null)
                inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();

            if (inventory != null && inventory.selectedInventorySlot.itemData != null && inventory.selectedInventorySlot.itemData.itemType == itemData.itemType)
                itemData.isOn = !itemData.isOn;
        }

        if (itemData.isOn)
        {
            outer.SetFloat("_Outer", 1);
            canvas?.gameObject.SetActive(true);
            if (text != null)
                text.text = addedPower.ToString("F2");
        }
        else
        {
            outer.SetFloat("_Outer", 0);
            inner.SetColor("_Color", noEffectColor);
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


        if (power_0_1.Value < 0.5)
            inner.SetColor("_Color", Color.Lerp(noEffectColor, midEffectColor, power_0_1.Value * 2));
        else
            inner.SetColor("_Color", Color.Lerp(midEffectColor, fullEffectColor, power_0_1.Value * 2 - 1));

        addedPower = 0;
        for (int i = 0; i < power.Count; i++)
        {
            addedPower += power[i];
        }
        addedPower = Mathf.Clamp(addedPower, 0, maxPower);

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
            float energy = energyTrigger.Amount;
            maxPowerOfEachDecrement = energyTrigger.MaxDecrement;

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

            power[i] = Mathf.Clamp(power[i], 0, maxPowerOfEachDecrement);
        }
    }
}
