using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EMFReader : NetworkBehaviour
{
    [Header("References")]
    public MatchItemDataToInventory MatchItemDataToInventory;
    ItemPickup itemPickup;
    Inventory inventory;
    ItemData itemData;
    public GameObject nib;

    [Header("Properties")]
    public int[] EMFReadings; 
    public List<float> power = new();
    public float addedPower = 0;
    private float maxPowerOfEachDecrement = 60;
    public float maxPower = 60;
    public List<GameObject> energySource = new();

    [Header("Visuals")]
    public MeshRenderer onRenderer;
    public float rot;
    public float maxRot = 26;

    [Header("Flicker")]
    public float minFlickerInterval = 0.2f;
    public float maxFlickerInterval = 20;    /// ===== CAN BE TWEAKABLE and CHANGE RESTARTFLICKER =====//
    public bool restartFlicker = false;
    public List<float> flickerDuration = new();
    public bool isFlickering = false;
    public bool isFlickeringExtreme = false;
    public float subtractFlicker = 0;
    public float waitTime;
    private Coroutine flickerCoroutine;

    private int dangerColliders = 0;


    private void OnEnable()
    {
        if (IsServer && flickerCoroutine == null)
            flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    private void OnDisable()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        isFlickering = false;
    }

    private void Awake()
    {
        GameManager.onServerStarted += GameManager_onServerStarted;
        
    }

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            flickerDuration.Add(Random.Range(0.05f, 0.2f));
        }

        StartCoroutine(NextFrame());
        inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();

        MatchItemDataToInventory = GetComponent<MatchItemDataToInventory>();

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
            GameManager.Instance.helpInstructionDisplayTime = 5f;
        }

        EMFReadings = GameManager.Instance.GetComponent<SelectingThreeProcedures>().EMFReadings;
    }


    private void Update()
    {
        if (inventory == null)
        {
            inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
        }


        if (itemData == null) return;
        if (Input.GetMouseButtonUp(1))
        {
            if (inventory == null)
                inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();

            if (inventory != null && inventory.selectedInventorySlot.itemData != null && inventory.selectedInventorySlot.itemData.itemType == itemData.itemType)
            {
                itemData.isOn = !itemData.isOn;
                AudioManager.PlaySound(AudioType.Click);
            }

            if (MatchItemDataToInventory)
                MatchItemDataToInventory.UpdateItemData();
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

        if (restartFlicker)
        {
            restartFlicker = false;
            if (flickerCoroutine != null)
                StopCoroutine(flickerCoroutine);
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
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

        if (isFlickering && !isFlickeringExtreme)
        {
            subtractFlicker = Random.Range(0, addedPower);
            addedPower -= subtractFlicker;
        }

        if (isFlickeringExtreme && isFlickering)
        {
            addedPower = Random.Range(-maxPower, maxPower);
        }


        Visuals();
    }

    private void Visuals()
    {
        nib.transform.localEulerAngles = new(nib.transform.localEulerAngles.x, (-maxRot) + (2 * maxRot * addedPower / maxPowerOfEachDecrement), nib.transform.localEulerAngles.z);
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            waitTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            yield return new WaitForSeconds(waitTime);

            float noOfFlickers = Random.Range(0, flickerDuration.Count);

            for (int i = 0; i < noOfFlickers; i++)
            {
                isFlickering = true;
                yield return new WaitForSeconds(flickerDuration[i]);
                isFlickering = false;
                yield return new WaitForSeconds(flickerDuration[i]/5);
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) { return; }
        if (itemData.isOn && other.CompareTag("Energy"))
        {
            float distance = (other.transform.position - transform.position).magnitude;
            EMFTrigger emfTrigger = other.GetComponent<EMFTrigger>();
            float energyRange = emfTrigger.Range;
            float energy = emfTrigger.Amount;
            maxPowerOfEachDecrement = emfTrigger.MaxDecrement;

            if (!energySource.Contains(other.gameObject))
            {
                energySource.Add(other.gameObject);
                power.Add(0);
            }

            int i = energySource.IndexOf(other.gameObject);
            if (emfTrigger.isActive)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost") || other.CompareTag("Doll"))
        {
            dangerColliders++;
            restartFlicker = true;
            UpdateFlickerMode();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ghost") || other.CompareTag("Doll"))
        {
            dangerColliders = Mathf.Max(0, dangerColliders - 1); // prevent negative values
            UpdateFlickerMode();
        }
    }


    private void UpdateFlickerMode()
    {
        if (dangerColliders > 0)
        {
            maxFlickerInterval = 0.5f;
            isFlickeringExtreme = true;
        }
        else
        {
            maxFlickerInterval = 20f;
            isFlickeringExtreme = false;
        }
    }

}
