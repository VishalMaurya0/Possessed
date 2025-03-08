using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public int maxSlots;
    public float maxWeight;
    public List<InventorySlot> inventorySlots = new();
    public InventorySlot selectedInventorySlot;
    public ItemHolding itemHolding;
    public InventorySlotTracker InventorySlotTracker;


    public NetworkVariable<int> slotNo = new (default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> weight = new (default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> slots = new (default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private void Start()
    {
        if (IsOwner || IsServer)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                inventorySlots.Add(new InventorySlot());
            }
        }
        if (GameManager.Instance.serverStarted)
        {
            itemHolding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
            maxSlots = GameManager.Instance.inventorySlots;
            maxWeight = GameManager.Instance.maxWeight;
            InventorySlotTracker = FindAnyObjectByType<InventorySlotTracker>();
        }
    }

    private void Update()
    {
        if (itemHolding == null && GameManager.Instance.serverStarted)
        {
            itemHolding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
        }

        if (IsOwner)
        {
            HandleSlotSelection();
        }
    }

    private void HandleSlotSelection()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ScrollUp();
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel"); // Get scroll input

        if (scroll < 0f) // Scroll up
        {
            ScrollUp();
        }
        else if (scroll > 0f) // Scroll down
        {
            ScrollDown();
        }
    }


    void ScrollUp()
    {
        slotNo.Value++;
        if (slotNo.Value >= maxSlots)
        {
            slotNo.Value = 0;
        }
        SelectInventorySlot(slotNo.Value);

        InventorySlotTracker.UpdateTracker(false);
    }


    void ScrollDown()
    {
        slotNo.Value--;
        if (slotNo.Value < 0)
        {
            slotNo.Value = maxSlots - 1;
        }
        SelectInventorySlot(slotNo.Value);

        InventorySlotTracker.UpdateTracker(false);
    }



    public int AddItem(ItemData itemDataOriginal)
    {
        if (!IsServer) { return -1; }
        if (itemDataOriginal.amount <= 0)
        {
            return 0;
        }
        ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(itemDataOriginal);
        ItemData itemData = new ItemData(itemDataSO, itemDataOriginal.amount, itemDataOriginal.currentState);



        
        if (weight.Value + itemDataSO.weight >= maxWeight)
        {
            Debug.Log("Connot hold this much Weight");
            return itemData.amount;
        }
        Debug.Log($"Trying to add {itemData.amount} items");
        //---------------------------------IF ALREADY AVAILABLE FILL THE SLOT-------------------------------//

        if (itemDataSO.isStackable)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.itemData != null)
                {
                    if (slot.itemData.itemType == itemData.itemType && slot.itemData.currentState == itemData.currentState && slot.quantity < itemDataSO.maxStackSize)
                    {
                        while (itemData.amount > 0)
                        {
                            slot.quantity++;
                            SelectInventorySlot(slotNo.Value);
                            weight.Value += itemDataSO.weight;
                            itemData.amount--;

                            if (itemData.amount <= 0)
                            {
                                return itemData.amount;
                            }
                            else if (slot.quantity >= itemDataSO.maxStackSize)
                            {
                                break;
                            }
                            if (weight.Value + itemDataSO.weight >= maxWeight)
                            {
                                Debug.Log("Connot hold this much Weight");
                                return itemData.amount;
                            }
                        }
                    }
                }
            }
        }

        //-------------------------IF NOT AVAILABLE IN INVENTORY--------------------------//

        if (slots.Value + itemDataSO.inventorySlots > maxSlots)
        {
            Debug.Log("{itemData.itemName} is too big throw some items to collect this");
            return itemData.amount;
        }
        if (weight.Value + itemDataSO.weight > maxWeight)
        {
            Debug.Log("Connot hold this much Weight");
            return itemData.amount;
        }

        //------------------------ADD IN NEW SLOT----------------------------------//

        foreach (var slot in inventorySlots)
        {

            if (slot.itemData == null)
            {
                if (slots.Value + itemDataSO.inventorySlots > maxSlots)
                {
                    Debug.Log($"{itemDataSO.name} is too big throw some items to collect this");
                    return itemData.amount;
                }
                if (weight.Value + itemDataSO.weight > maxWeight)
                {
                    Debug.Log("Connot hold this much Weight");
                    return itemData.amount;
                }


                Debug.Log($"Add new : {itemData.amount} amount of {itemData.itemType}");
                slot.itemData = new ItemData(itemDataSO, 1, itemData.currentState);
                slot.quantity = 1;
                weight.Value += itemDataSO.weight;
                slots.Value += itemDataSO.inventorySlots;
                itemData.amount--;
                SelectInventorySlot(slotNo.Value);
                if (itemData.amount <= 0)
                { return itemData.amount; }

                while (itemData.amount > 0)
                {
                    if (weight.Value + itemDataSO.weight >= maxWeight)
                    {
                        Debug.Log("Connot hold this much Weight");
                        return itemData.amount;
                    }

                    slot.quantity++;
                    SelectInventorySlot(slotNo.Value);
                    weight.Value += itemDataSO.weight;
                    itemData.amount--;
                    Debug.Log(itemData.amount);

                    if (itemData.amount <= 0)
                    {
                        return itemData.amount;
                    }
                    else if (slot.quantity >= itemDataSO.maxStackSize)
                    {
                        break;
                    }
                }
            }
        }



        Debug.Log("Inventory is full!");
        return itemData.amount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryAddItemServerRpc(ItemData itemData, bool forced)
    {
        int remainingQuantity = AddItem(itemData);
        if (remainingQuantity > 0 && forced)
        {
            itemHolding.ThrowSpecificNoOfItems___InventoryNotUpdated(selectedInventorySlot.quantity);
            RemoveItemSelected(true);
            int againremain = AddItem(itemData);
            if (againremain > 0)
            {
                itemHolding.ThrowSpecificNoOfItems___InventoryNotUpdated(againremain);
            }
        }
        if (remainingQuantity > 0 && !forced)
        {
            itemHolding.ThrowSpecificNoOfItems___InventoryNotUpdated(remainingQuantity, itemData);
        }

        UpdateInventoryToClient();                       //=====================================update the inventory to client when the item get added to inventory====================================//
    }


    [ServerRpc(RequireOwnership = false)]
    public void RemoveSelectedItemServerRpc(bool full, int quantity = 1)
    {
        if (full)
        {
            RemoveItemSelected(full);
        }
        else
        {
            RemoveItemSelected(full, quantity);
        }
    }

    private void RemoveItemSelected(bool full, int quantity = 1)
    {
        SelectInventorySlot(slotNo.Value);
        if (selectedInventorySlot.itemData == null)
        {
            return;
        }
        ItemDataSO selectedItemDataSO = ScriptableObjectFinder.FindItemSO(selectedInventorySlot.itemData);
        if (!full)
        {
            selectedInventorySlot.quantity -= quantity;
            weight.Value -= selectedItemDataSO.weight * quantity;
            
            if (selectedInventorySlot.quantity <= 0)
            {
                selectedInventorySlot.itemData = null;
                selectedInventorySlot.quantity = 0;
                slots.Value -= selectedItemDataSO.inventorySlots;
            }
            UpdateInventoryToClient();
            SelectInventorySlot(slotNo.Value);
        }
        else
        {
            slots.Value -= (selectedItemDataSO.inventorySlots) * selectedInventorySlot.quantity;
            weight.Value -= (selectedItemDataSO.weight) * selectedInventorySlot.quantity;

            selectedInventorySlot.itemData = null;
            selectedInventorySlot.quantity = 0;
            UpdateInventoryToClient();
            SelectInventorySlot(slotNo.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveItemServerRpc(int slotNo, int quantity)
    {
        if (inventorySlots[slotNo].itemData == null)
        {
            Debug.Log("No item.");
            return;
        }
        if (inventorySlots[slotNo].quantity < quantity) { return; }


        ItemDataSO selectedItemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlots[slotNo].itemData);
        inventorySlots[slotNo].quantity -= quantity;
        weight.Value -= selectedItemDataSO.weight * quantity;

        if (inventorySlots[slotNo].quantity <= 0)
        {   
            inventorySlots[slotNo].itemData = null;
            inventorySlots[slotNo].quantity = 0;
            slots.Value -= selectedItemDataSO.inventorySlots;
        }
        UpdateInventoryToClient();                       //=====================================update the inventory to client when the item get updated to inventory====================================//
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeStateOfItemServerRpc(int slotNumber, int changeInState)
    {
        if (inventorySlots[slotNumber].itemData == null)
        {
            Debug.Log($"Slot {slotNumber} has no itemData. Exiting function.");
            return;
        }

        Debug.Log($"Changing state of item in slot {slotNumber} by {changeInState}.");

        ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlots[slotNumber].itemData);
        InventorySlot slot = inventorySlots[slotNumber];
        int totalAmountInItem = itemDataSO.noOfStates - 1;
        int currentState = slot.itemData.currentState;

        Debug.Log($"Total states in item: {totalAmountInItem}, Current state: {currentState}, Slot quantity: {slot.quantity}.");

        // Validate changeInState
        if (changeInState < 0 || changeInState > slot.quantity * totalAmountInItem)
        {
            Debug.LogError($"Invalid changeInState value: {changeInState}. Maximum allowed: {slot.quantity * totalAmountInItem}.");
            return;
        }

        // Avoid division by zero
        if (totalAmountInItem == currentState)
        {
            Debug.LogError("Division by zero: totalAmountInItem equals currentState.");
            return;
        }

        if (slot.quantity > 1)
        {
            Debug.Log($"Slot contains multiple items ({slot.quantity}). Computing state changes.");
            int change = changeInState;
            int noOfEmptyItems = change / (totalAmountInItem - currentState);
            int changeOnFinal = change - (noOfEmptyItems) * (totalAmountInItem - currentState);
            int noOfChangingItems = changeOnFinal > 0 ? 1 : 0;
            int noOfUnchangedItems = Mathf.Max(0, slot.quantity - noOfEmptyItems - noOfChangingItems);

            Debug.Log($"Change: {change}, Empty items: {noOfEmptyItems}, Change on final: {changeOnFinal}, Unchanged items: {noOfUnchangedItems}.");

            if (noOfChangingItems > 0)
            {
                if (noOfUnchangedItems > 0)
                {
                    Debug.Log($"Adding {noOfUnchangedItems} unchanged items.");
                    slot.itemData.amount = noOfUnchangedItems;
                    int remainn = AddItem(slot.itemData);
                    if (remainn > 0)
                    {
                        itemHolding.ThrowSpecificNoOfItems___InventoryNotUpdated(remainn, slot.itemData);
                    }
                }
                Debug.Log($"Changing final item state to max: {totalAmountInItem} and updating empty items.");
                slot.itemData.currentState = totalAmountInItem;
                slot.itemData.amount = noOfEmptyItems;
                int remain = AddItem(slot.itemData);
                if (remain > 0)
                {
                    itemHolding.ThrowSpecificNoOfItems___InventoryNotUpdated(remain, slot.itemData);
                }
                Debug.Log($"Setting remaining item state to: {changeOnFinal}.");
                slot.itemData.currentState = changeOnFinal;
                slot.quantity = 1;
            }
            else
            {
                if (noOfUnchangedItems > 0)
                {
                    Debug.Log($"Changing final item state to max: {totalAmountInItem} for unchanged items.");
                    slot.itemData.currentState = totalAmountInItem;
                    slot.itemData.amount = noOfEmptyItems;
                    int remain = AddItem(slot.itemData);
                    if (remain > 0)
                    {
                        itemHolding.ThrowSpecificNoOfItems___InventoryNotUpdated(remain, slot.itemData);
                    }
                    slot.itemData.currentState = currentState;
                    slot.quantity = noOfUnchangedItems;
                }
                else
                {
                    Debug.Log($"Changing final item state to max: {totalAmountInItem} with no unchanged items.");
                    slot.itemData.currentState = totalAmountInItem;
                }
            }
        }
        else if (slot.quantity == 1)
        {
            Debug.Log("Slot contains only one item. Checking state change feasibility.");
            if (changeInState <= totalAmountInItem)
            {
                Debug.Log($"Changing single item's state to {changeInState}.");
                slot.itemData.currentState = currentState + changeInState;
            }
            else
            {
                Debug.Log("Not enough amount in container to change state.");
                return;
            }
        }
        UpdateInventoryToClient();                       //=====================================update the inventory to client when the item get updated to inventory====================================//
    }


    [ClientRpc]
    private void NotifyClientsAboutSpawnedItemClientRpc(NetworkObjectReference refe, ItemData newItemData)
    {
        GameObject spawnedObject = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        spawnedObject.GetComponent<ItemPickup>().itemData = newItemData;
    }



    public void SelectInventorySlot(int slotNo)
    {
        selectedInventorySlot = inventorySlots[slotNo];

        //-------------------------------Notify Item Holding Script---------------------------------//
        if (!IsOwner) { return; }

        if (selectedInventorySlot.itemData != null)
        {
            itemHolding.HoldingItem(selectedInventorySlot.itemData, selectedInventorySlot.quantity, selectedInventorySlot.itemData.currentState);
        }
        else
        {
            itemHolding.HoldingItem(null, 0, 0);
        }

    }

    public void UpdateInventoryToClient()
    {
        Debug.Log("[Inventory] Updating inventory to all clients...");
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            UpdateSlotClientRpc(inventorySlots[i].itemData, inventorySlots[i].quantity, i);
        }
    }

    [ClientRpc]
    public void UpdateSlotClientRpc(ItemData itemData, int quantity, int i)
    {
        if (!IsOwner) { return; }
        Debug.Log($"[Inventory] Updating slot {i} on client. Item: {itemData?.itemType}, Quantity: {quantity}");
        inventorySlots[i].itemData = itemData;
        inventorySlots[i].quantity = quantity;
        SelectInventorySlot(slotNo.Value);
    }
}






// Inventory Slot Class
[System.Serializable]
public class InventorySlot
{
    [SerializeReference] public ItemData itemData;
    public int quantity = 0;
}