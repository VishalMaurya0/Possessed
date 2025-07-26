using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Thermometer : NetworkBehaviour
{
    [Header("Properties")]
    public NetworkVariable<float> power_0_1 = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public List<float> power = new();
    public float addedPower = 0;
    public float avgTemperature = 25;
    public List<GameObject> energySource = new();
    public float maxPower = 15f;
    public int[] ThermometerReadings;

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

        ThermometerReadings = GameManager.Instance.GetComponent<SelectingThreeProcedures>().thermometerReadings;
    }

    private float textUpdateInterval = 0.1f;
    private void Update()
    {
        textUpdateInterval -= Time.deltaTime;

        if (canvas != null)
            canvas.gameObject.SetActive(true);
        if (text != null && textUpdateInterval <= 0)
        {
            textUpdateInterval = 0.1f;
            text.text = addedPower.ToString("F1");
        }
        if (bar != null)
        {
            Vector3 currentScale = bar.transform.localScale;
            float targetY = minPressureScale + (maxPressureScale - minPressureScale) * power_0_1.Value;
            float smoothY = Mathf.Lerp(currentScale.y, targetY, Time.deltaTime * 5f); // 5f is the smoothing speed
            bar.transform.localScale = new Vector3(currentScale.x, smoothY, currentScale.z);
        }

        ManageWorking();
    }

    private float minTemp = -15;

    private void ManageWorking()
    {
        if (!IsServer) return;


        addedPower = 0;
        for (int i = 0; i < power.Count; i++)
        {
            addedPower += power[i];
        }
        addedPower = Mathf.Clamp(avgTemperature - addedPower + Random.Range(-1f, 1f), minTemp, maxPower + avgTemperature * 2);

        power_0_1.Value = addedPower / (maxPower + avgTemperature * 2 + minTemp);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) { return; }
        if (other.CompareTag("Energy"))
        {
            float distance = (other.transform.position - transform.position).magnitude;
            TemperatureTrigger temperatureTrigger = other.GetComponent<TemperatureTrigger>();
            float Range = temperatureTrigger.Range;
            float energy = temperatureTrigger.Amount;

            if (!energySource.Contains(other.gameObject))
            {
                energySource.Add(other.gameObject);
                power.Add(0);
            }

            int i = energySource.IndexOf(other.gameObject);
            if (temperatureTrigger.isActive)
            {
                power[i] = (1 - (distance / Range)) * energy;
            }
            else
            {
                power[i] = 0;
            }

            power[i] = Mathf.Clamp(power[i], 0, maxPower);
        }
    }
}

