using System;
using Unity.Netcode;
using UnityEngine;

public class ProcedureCompletion : ProcedureBase
{
    ProcedureBase procedureBase;
    public ProcedureData procedureData;

    public TotalItemsNeeded totalItemsNeeded = new();
    int totalItems;

    [Header("Procedure Variables")]
    public NetworkVariable<int> currentOrder = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isCompleted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> timer = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool isShuttingDown = false;

    new private void OnDestroy()
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

        if (procedureBase != null)
        {
            procedureBase.allProcedures[procedureData.procedureIndex] = this;
            procedureBase.position[procedureData.procedureIndex] = transform.position;
            Debug.Log($"Procedure registered at index {procedureData.procedureIndex}");
        }
    }

    void Update()
    {
        if (isShuttingDown || !IsOwner || !NetworkManager.Singleton.IsListening)
        {
            return; // Prevent modifying NetworkVariables after shutdown
        }

        if (IsServer)
        {
            timer.Value -= Time.deltaTime;
        }

        if (GetComponentInChildren<triggerProcedurePointScript>().inProgress && Input.GetMouseButtonDown(0) && timer.Value <= 0)
        {
            Debug.Log("Input detected. Checking inventory...");
            Inventory inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
            ItemHolding itemHolding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
            InventorySlot selectedInventorySlot = inventory.selectedInventorySlot;

            if (selectedInventorySlot.itemData == null)
            {
                Debug.Log("No item selected in the inventory.");
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
        Debug.Log($"Matching item: {itemDataInInventory.itemType} with required item: {itemToCheckAndAdd.ItemType}");

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
    }
}
