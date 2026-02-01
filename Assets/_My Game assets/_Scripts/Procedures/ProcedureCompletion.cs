using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ProcedureCompletion : ProcedureBase
{
    [Header("References")]
    ProcedureBase procedureBase;
    public ProcedureDataSO procedureDataSO;
    public GameObject procedurePrefab;
    public triggerProcedurePointScript triggerScript;
    public Animator winLoseAnimator;
    public GameObject safePointPrefab; 

    [Header("Procedure Specific Variables")]
    public Transform VFXPosition;
    public int procedureID; 
    public TotalItemsNeeded totalItemsNeeded = new();
    int totalItems;
    public int barometerReading, temperatureReading, EMFReading, EnergyDetectorReading;

    [Header("Visuals")]
    public List<VisualsTrigger> visualsTrigger = new();
    public KeyValuePair<bool, int> showVisual = new();

    [Header("Procedure Network Variables")]
    public NetworkVariable<int> currentOrder = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isCompleted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> timer = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("UI References")]
    public TMP_Text timerText;

    [Header("Champt GPT")]
    private bool isShuttingDown = false;
    public bool temp = false;

    new private void OnDestroy()          //===========CHAMPT GPT=============//
    {
        isShuttingDown = true;
    }

    void Awake()
    {
        for (int i = 0; i < procedureDataSO.itemsNeeded.Count; i++)
        {
            totalItemsNeeded.itemNeeded.Add(procedureDataSO.itemsNeeded[i]); // Copying itemsNeeded
            totalItemsNeeded.addedAmount.Add(0);
        }

        totalItems = totalItemsNeeded.itemNeeded.Count;
        //Debug.Log($"Total items needed: {totalItems}");


        InitializeVisuals();
    }

    private void Start()
    {
        AddProcedureToGameManager();

        procedureBase = GameManager.Instance.procedureBase;   // TODO is this doing anything????

        if (procedureBase != null)
        {
            procedureBase.allProcedures[procedureDataSO.procedureIndex] = this;
            procedureBase.position[procedureDataSO.procedureIndex] = transform.position;
            //Debug.Log($"Procedure registered at index {procedureData.procedureIndex}");
        }
    }

    void AddProcedureToGameManager()
    {
        while (GameManager.Instance.AllProcedures.Count <= procedureDataSO.procedureIndex)
        {
            GameManager.Instance.AllProcedures.Add(null);
        }
        GameManager.Instance.AllProcedures[procedureID] = this;
    }

    private void InitializeVisuals()
    {
        visualsTrigger.Clear();
        for (int i = 0; i < totalItemsNeeded.itemNeeded.Count; i++)
        {
            visualsTrigger.Add(new VisualsTrigger());
            for (int j = 0; j < totalItemsNeeded.itemNeeded[i].requiredAmount; j++)
            {
                visualsTrigger[i].trigger.Add(false);
            }
        }
    }

    void Update()
    {
        if (isShuttingDown || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        if (temp)
        {
            temp = false;
            CheckForGameComopletioon();
        }

        if (IsServer)
        {
            if (timer.Value > 0)
            {
                timer.Value -= Time.deltaTime;
            }
            if (timer.Value <= 0)
            {
                timer.Value = 0;
            }
        }

        if (triggerScript == null)
        {
            triggerScript = GetComponentInChildren<triggerProcedurePointScript>();
            if (triggerScript == null)
            {
                Debug.LogWarning("triggerProcedurePointScript is missing!");
                return;
            }
        }

        if (triggerScript.inProgress)
        {
            if (triggerScript.inProgress && Input.GetMouseButtonDown(0) && timer.Value <= 0)
            {
                //Debug.Log("Input detected. Checking inventory...");

                if (GameManager.Instance.ownerPlayer == null)
                {
                    Debug.LogWarning("ownerPlayer is null in GameManager!");
                    return;
                }

                Inventory inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
                if (inventory == null)
                {
                    Debug.LogWarning("Inventory component is missing!");
                    return;
                }

                InventorySlot selectedInventorySlot = inventory.selectedInventorySlot;
                if (selectedInventorySlot == null || selectedInventorySlot.itemData == null)
                {
                    GameManager.Instance.HelpInstructions.text = "No item selected in inventory!";
                    GameManager.Instance.helpInstructionDisplayTime = 3f;
                    AudioManager.PlaySound(AudioType.Wrong);
                    Debug.LogWarning("No item selected in inventory!");
                    return;
                }

                //Debug.Log($"Selected item: {selectedInventorySlot.itemData.itemType}");

                for (int i = 0; i < totalItems; i++)
                {
                    if (totalItemsNeeded.itemNeeded[i].orderId == currentOrder.Value)
                    {
                        //Debug.Log($"Checking item match for order {currentOrder.Value}...");
                        if (CheckIfItemMatchedWithInventorySlot(totalItemsNeeded.itemNeeded[i], selectedInventorySlot.itemData, inventory, i))
                        {
                            AudioManager.PlaySound(AudioType.Click);
                            CheckOrderCompletionServerRpc();
                            break;
                        }
                    }
                }
            }
        }

        TimerTextUI();
    }

    private void TimerTextUI()
    {
        timerText?.SetText($"{timer.Value} secs");
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckOrderCompletionServerRpc(ServerRpcParams rpcParams = default)
    {
        //Debug.Log($"Checking if order {currentOrder.Value} is complete...");
        for (int i = 0; i < totalItems; i++)
        {
            if (totalItemsNeeded.itemNeeded[i].orderId == currentOrder.Value)
            {
                //Debug.Log($"Item {i}: required = {totalItemsNeeded.itemNeeded[i].requiredAmount}, added = {totalItemsNeeded.addedAmount[i]}");

                if (totalItemsNeeded.itemNeeded[i].requiredAmount != totalItemsNeeded.addedAmount[i])
                {
                    //Debug.Log($"Order {currentOrder.Value} is not complete yet.");
                    return;
                }
            }
        }

        ShowVFXClientRPC();
        currentOrder.Value++;
        //Debug.Log($"Order {currentOrder.Value - 1} completed. Moving to order {currentOrder.Value}.");

        CheckForProcedureCompletionServerRPC();

    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckForProcedureCompletionServerRPC(bool forceComplete = false, ServerRpcParams rpcParams = default)
    {
        if (forceComplete || totalItemsNeeded.itemNeeded[totalItems - 1].orderId < currentOrder.Value)
        {
            isCompleted.Value = true;
            GameManager.Instance.completedProcedures.Add(procedureDataSO.procedureIndex);
            if (!GameManager.Instance.completedProcedure.ContainsValue(procedureDataSO.procedure))
                GameManager.Instance.completedProcedure.Add(NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.gameObject, procedureDataSO.procedure);
            //Debug.Log("All orders completed!");

            AudioManager.PlaySoundClientRpc(AudioType.HorroRiser);
        }

        
        CheckForGameComopletioon();
    }


    private bool CheckIfItemMatchedWithInventorySlot(ItemNeeded itemToCheckAndAdd, ItemData itemDataInInventory, Inventory inventory, int i)
    {
        //Debug.Log($"Matching item: {itemDataInInventory?.itemType} with required item: {itemToCheckAndAdd?.ItemType}");

        if (itemToCheckAndAdd.ItemType == itemDataInInventory?.itemType)
        {
            ItemDataSO itemDataSO = ScriptableObjectFinder.Instance.FindItemSO(itemDataInInventory);
            bool isContainer = itemDataSO.isContainer;

            if (!isContainer)
            {
                //Debug.Log($"Non-container item detected: {itemDataInInventory.itemType}");
                if (itemToCheckAndAdd.currentState == itemDataInInventory.currentState)
                {
                    if (totalItemsNeeded.addedAmount[i] < totalItemsNeeded.itemNeeded[i].requiredAmount)
                    {
                        AddNonContainerItem(itemDataInInventory, inventory, i);
                        return true;
                    }
                }
                else
                {
                    GameManager.Instance.HelpInstructions.text =
                        $"Incorrect state! Required: {itemToCheckAndAdd.currentState}";
                    GameManager.Instance.helpInstructionDisplayTime = 3f;
                }
            }
            else if (isContainer && itemDataInInventory.currentState != itemDataSO.noOfStates - 1)
            {
                //Debug.Log($"Container item detected: {itemDataInInventory.itemType}");
                if (totalItemsNeeded.addedAmount[i] < totalItemsNeeded.itemNeeded[i].requiredAmount)
                {
                    AddContainerItem(inventory, itemDataInInventory, i);
                    return true;
                }
            }else
            {
                GameManager.Instance.HelpInstructions.text =
                    $"There's Nothing in the {itemToCheckAndAdd.ItemType}!!!";
                GameManager.Instance.helpInstructionDisplayTime = 3f;
                return false;
            }
        }
        else
        {
            GameManager.Instance.HelpInstructions.text =
                $"Wrong item! Required: {itemToCheckAndAdd.ItemType}, You have: {itemDataInInventory?.itemType}";
            GameManager.Instance.helpInstructionDisplayTime = 3f;
            return false;
        }

        return false;
    }


    private void CheckForGameComopletioon()
    {

        if (GameManager.Instance.completedProcedure.Count >= 3)
        {
            winLoseAnimator.SetTrigger("Won");  // activate the winLose Panel
            if (GameManager.Instance.CheckForCorrectProcedures())
            {
                GameManager.Instance.OnWinOrLose(true, false);
            }else
            {
                GameManager.Instance.OnWinOrLose(false, false);
            }
        }
    }

    private void AddContainerItem(Inventory inventory, ItemData itemDataInInventory, int i)
    {
        //Debug.Log($"Adding container item: {itemDataInInventory.itemType}, State: {itemDataInInventory.currentState}");
        inventory.ChangeStateOfItemServerRpc(inventory.inventorySlots.IndexOf(inventory.selectedInventorySlot), 1);
        
        AddAmountInProcedureServerRpc(i);
        //Debug.Log($"Added amount: {totalItemsNeeded.addedAmount[i]} / {totalItemsNeeded.itemNeeded[i].requiredAmount}");
    }

    

    private void AddNonContainerItem(ItemData itemDataInInventory, Inventory inventory, int i)
    {
        //Debug.Log($"Adding non-container item: {itemDataInInventory.itemType}, State: {itemDataInInventory.currentState}");
        inventory.RemoveSelectedItemServerRpc(false);

        AddAmountInProcedureServerRpc(i);
        //Debug.Log($"Added amount: {totalItemsNeeded.addedAmount[i]} / {totalItemsNeeded.itemNeeded[i].requiredAmount}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddAmountInProcedureServerRpc(int i)
    {
        InformClientsAboutChangeClientRpc(i);
        timer.Value = totalItemsNeeded.itemNeeded[i].timeToWaitAfterAddingAAmount;
    }

    [ClientRpc]
    private void InformClientsAboutChangeClientRpc(int i)
    {
        totalItemsNeeded.addedAmount[i]++;
        SetVisualTrigger(i);
    }
    
    

    private void SetVisualTrigger(int i)
    {
        for (int j = 0; j < totalItemsNeeded.addedAmount[i]; j++)
        {
            visualsTrigger[i].trigger[j] = true;
        }
        showVisual = new KeyValuePair<bool, int> (true, i);
    }

    [ClientRpc]
    public void ShowVFXClientRPC()
    {
        AudioManager.PlaySound(AudioType.Correct);
        procedureBase.Completed(VFXPosition.position);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnSafePointServerRpc(int time)
    {
        StartCoroutine(SpawnSafePoint(time));
    }
    public IEnumerator SpawnSafePoint(int time)
    {
        GameObject obj = Instantiate(safePointPrefab, transform);
        obj.transform.GetChild(0).gameObject.SetActive(false);
        obj.transform.GetChild(1).gameObject.SetActive(false);
        obj.GetComponent<NetworkObject>().Spawn();

        yield return new WaitForSeconds(time);

        Destroy(obj);
    }
}
    
            

[System.Serializable]
public class VisualsTrigger
{
    public List<bool> trigger = new();
}