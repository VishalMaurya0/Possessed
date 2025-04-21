using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ProcedureCompletion : ProcedureBase
{
    [Header("References")]
    ProcedureBase procedureBase;
    public ProcedureData procedureData;
    public GameObject procedurePrefab;

    [Header("Procedure Specific Variables")]
    public TotalItemsNeeded totalItemsNeeded = new();
    int totalItems;

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

    new private void OnDestroy()          //===========CHAMPT GPT=============//
    {
        isShuttingDown = true;
    }

    void Start()
    {
        Debug.Log("Initializing procedure...");
        for (int i = 0; i < procedureData.itemsNeeded.Count; i++)
        {
            totalItemsNeeded.itemNeeded.Add(procedureData.itemsNeeded[i]); // Copying itemsNeeded
            totalItemsNeeded.addedAmount.Add(0);
        }

        totalItems = totalItemsNeeded.itemNeeded.Count;
        Debug.Log($"Total items needed: {totalItems}");

        procedureBase = GameManager.Instance.procedureBase;
        GameManager.Instance.AllProcedures.Add(this);

        if (procedureBase != null)
        {
            procedureBase.allProcedures[procedureData.procedureIndex] = this;
            procedureBase.position[procedureData.procedureIndex] = transform.position;
            Debug.Log($"Procedure registered at index {procedureData.procedureIndex}");
        }

        InitializeVisuals();
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

        var triggerScript = GetComponentInChildren<triggerProcedurePointScript>();
        if (triggerScript == null)
        {
            Debug.LogWarning("triggerProcedurePointScript is missing!");
            return;
        }

        if (triggerScript.inProgress && Input.GetMouseButtonDown(0) && timer.Value <= 0)
        {
            Debug.Log("Input detected. Checking inventory...");

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
                Debug.LogWarning("No item selected in inventory!");
                return;
            }

            Debug.Log($"Selected item: {selectedInventorySlot.itemData.itemType}");

            for (int i = 0; i < totalItems; i++)
            {
                if (totalItemsNeeded.itemNeeded[i].orderId == currentOrder.Value)
                {
                    Debug.Log($"Checking item match for order {currentOrder.Value}...");
                    CheckIfItemMatchedWithInventorySlot(totalItemsNeeded.itemNeeded[i], selectedInventorySlot.itemData, inventory, i);
                    CheckOrderCompletionServerRpc();
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
    private void CheckOrderCompletionServerRpc()
    {
        Debug.Log($"Checking if order {currentOrder.Value} is complete...");
        for (int i = 0; i < totalItems; i++)
        {
            if (totalItemsNeeded.itemNeeded[i].orderId == currentOrder.Value)
            {
                Debug.Log($"Item {i}: required = {totalItemsNeeded.itemNeeded[i].requiredAmount}, added = {totalItemsNeeded.addedAmount[i]}");

                if (totalItemsNeeded.itemNeeded[i].requiredAmount != totalItemsNeeded.addedAmount[i])
                {
                    Debug.Log($"Order {currentOrder.Value} is not complete yet.");
                    return;
                }
            }
        }

        currentOrder.Value++;
        Debug.Log($"Order {currentOrder.Value - 1} completed. Moving to order {currentOrder.Value}.");

        if (totalItemsNeeded.itemNeeded[totalItems - 1].orderId < currentOrder.Value)
        {
            isCompleted.Value = true;
            GameManager.Instance.completedProcedures.Add(procedureData.procedureIndex);
            Debug.Log("All orders completed!");
        }
    }

    private void CheckIfItemMatchedWithInventorySlot(ItemNeeded itemToCheckAndAdd, ItemData itemDataInInventory, Inventory inventory, int i)
    {
        Debug.Log($"Matching item: {itemDataInInventory?.itemType} with required item: {itemToCheckAndAdd?.ItemType}");

        if (itemToCheckAndAdd.ItemType == itemDataInInventory?.itemType)
        {
            ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(itemDataInInventory);
            bool isContainer = itemDataSO.isContainer;

            if (!isContainer)
            {
                Debug.Log($"Non-container item detected: {itemDataInInventory.itemType}");
                if (itemToCheckAndAdd.currentState == itemDataInInventory.currentState)
                {
                    if (totalItemsNeeded.addedAmount[i] < totalItemsNeeded.itemNeeded[i].requiredAmount)
                    {
                        AddNonContainerItem(itemDataInInventory, inventory, i);
                    }
                }
            }
            else if (isContainer && itemDataInInventory.currentState != itemDataSO.noOfStates - 1)
            {
                Debug.Log($"Container item detected: {itemDataInInventory.itemType}");
                if (totalItemsNeeded.addedAmount[i] < totalItemsNeeded.itemNeeded[i].requiredAmount)
                {
                    AddContainerItem(inventory, itemDataInInventory, i);
                }
            }
        }
    }


    private void AddContainerItem(Inventory inventory, ItemData itemDataInInventory, int i)
    {
        Debug.Log($"Adding container item: {itemDataInInventory.itemType}, State: {itemDataInInventory.currentState}");
        inventory.ChangeStateOfItemServerRpc(inventory.inventorySlots.IndexOf(inventory.selectedInventorySlot), 1);
        
        AddAmountInProcedureServerRpc(i);
        Debug.Log($"Added amount: {totalItemsNeeded.addedAmount[i]} / {totalItemsNeeded.itemNeeded[i].requiredAmount}");
    }

    

    private void AddNonContainerItem(ItemData itemDataInInventory, Inventory inventory, int i)
    {
        Debug.Log($"Adding non-container item: {itemDataInInventory.itemType}, State: {itemDataInInventory.currentState}");
        inventory.RemoveSelectedItemServerRpc(false);

        AddAmountInProcedureServerRpc(i);
        Debug.Log($"Added amount: {totalItemsNeeded.addedAmount[i]} / {totalItemsNeeded.itemNeeded[i].requiredAmount}");
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
}
    
            

[System.Serializable]
public class VisualsTrigger
{
    public List<bool> trigger = new();
}