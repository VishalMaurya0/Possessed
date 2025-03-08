using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemCrafting : MonoBehaviour
{
    public ItemCraftingDataSO itemCraftingDataSO;
    public Inventory inventory;
    public ItemData currentItemData;

    [Header("Function Dependent")]
    ItemState itemState1;
    ItemState itemState2;
    public ItemState finalItemState;
    public InventorySlot itemState2InventorySlot;

    private void MyStart()
    {
        Debug.Log("[ItemCrafting] MyStart called.");
        inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
        if (inventory != null && inventory.selectedInventorySlot != null)
        {
            currentItemData = inventory.selectedInventorySlot.itemData;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.serverStarted && inventory == null)
        {
            Debug.LogWarning("[ItemCrafting] Inventory is null on server start. Calling MyStart again.");
            MyStart();
        }

        if (Input.GetMouseButtonDown(1)) // Right Mouse Button
        {
            currentItemData = inventory.selectedInventorySlot.itemData;
            Debug.Log("[ItemCrafting] Right mouse button pressed. Checking craftability...");
            if (IsItemCraftable(out List<int> ids))
            {
                Debug.Log($"[ItemCrafting] Item is craftable. Valid Recipe IDs: {string.Join(", ", ids)}");

                if (IsOtherItemAvailableInInventory(ids, out int id))
                {
                    Debug.Log($"[ItemCrafting] Other item found in inventory for Recipe ID: {id}");
                    finalItemState = CraftItemServerRpc(id, inventory.selectedInventorySlot, itemState2InventorySlot, inventory);
                    Debug.Log($"[ItemCrafting] Item crafted successfully. Final Item State: {finalItemState.itemType}");
                }
                else
                {
                    Debug.LogWarning("[ItemCrafting] Required secondary item not found in inventory.");
                }
            }
            else
            {
                Debug.LogWarning("[ItemCrafting] Item is not craftable.");
            }
        }
    }

    public bool IsItemCraftable(out List<int> ids)
    {
        ids = new List<int>();

        if (currentItemData == null)
        {
            Debug.LogError("[ItemCrafting] Current Item Data is null. Cannot proceed with crafting.");
            return false;
        }

        itemState1 = new ItemState(currentItemData, inventory.selectedInventorySlot.quantity);
        int totalStates = ScriptableObjectFinder.FindItemSO(currentItemData).noOfStates;
        Debug.Log($"[ItemCrafting] Checking recipes for Item: {currentItemData.itemType} with State: {itemState1.currentState}");

        foreach (var recipe in itemCraftingDataSO.itemStateCraftingRecipes)
        {
            if (recipe.ItemState1.itemType == itemState1.itemType &&
                !recipe.ItemState1.isContainer &&
                recipe.ItemState1.currentState == itemState1.currentState &&
                itemState1.amount >= recipe.ItemState1.amount)
            {
                ids.Add(recipe.id);
                Debug.Log($"[ItemCrafting] Found matching recipe (Non-Container) ID: {recipe.id}");
            }

            if (recipe.ItemState1.itemType == itemState1.itemType &&
                recipe.ItemState1.isContainer &&
                itemState1.currentState < totalStates - 1 &&
                itemState1.amount >= recipe.ItemState1.amount)
            {
                ids.Add(recipe.id);
                Debug.Log($"[ItemCrafting] Found matching recipe (Container) ID: {recipe.id}");
            }
        }

        if (ids.Count > 0)
        {
            Debug.Log("[ItemCrafting] Craftable recipe(s) found.");
            return true;
        }

        Debug.LogWarning("[ItemCrafting] No craftable recipes found.");
        ids.Clear();
        return false;
    }

    private bool IsOtherItemAvailableInInventory(List<int> ids, out int id)
    {
        Debug.Log("[ItemCrafting] Checking for secondary item in inventory...");
        foreach (int recipeId in ids)
        {
            var recipe = itemCraftingDataSO.itemStateCraftingRecipes[recipeId];
            ItemType requiredType = recipe.ItemState2.itemType;
            bool isContainer = recipe.ItemState2.isContainer;
            int requiredState = recipe.ItemState2.currentState;
            int requiredAmount = recipe.ItemState2.amount;

            

            foreach (var slot in inventory.inventorySlots)
            {
                ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(slot.itemData);
                if (slot.itemData != null &&
                    slot.itemData.itemType == requiredType &&
                    !itemDataSO.isContainer &&
                    slot.itemData.currentState == requiredState)
                {
                    if (slot.quantity >= requiredAmount)
                    {
                        itemState2 = new ItemState(slot.itemData, slot.quantity);
                        itemState2InventorySlot = slot;
                        id = recipeId;
                        Debug.Log($"[ItemCrafting] Secondary Item Found (Non-Container) in Slot. Recipe ID: {id}");
                        return true;
                    }
                }

                if (slot.itemData != null &&
                    slot.itemData.itemType == requiredType &&
                    itemDataSO.isContainer )
                {
                    int remainStates = itemDataSO.noOfStates - 1 - slot.itemData.currentState;
                    if (slot.quantity*remainStates >= requiredAmount)
                    {
                        itemState2 = new ItemState(slot.itemData, slot.quantity);
                        itemState2InventorySlot = slot;
                        id = recipeId;
                        Debug.Log($"[ItemCrafting] Secondary Item Found (Container) in Slot. Recipe ID: {id}");
                        return true;
                    }
                }
            }
        }

        Debug.LogWarning("[ItemCrafting] No valid secondary item found in inventory.");
        itemState2 = null;
        id = -1;
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private ItemState CraftItemServerRpc(int id, InventorySlot selectedInventorySlot, InventorySlot itemState2InventorySlot, Inventory inventory)
    {
        Debug.Log($"[ItemCrafting] Crafting item with Recipe ID: {id}");

        ItemDataSO idso = ScriptableObjectFinder.FindItemSO(selectedInventorySlot.itemData);
        ItemState A = itemCraftingDataSO.itemStateCraftingRecipes[id].ItemState1;
        ItemState B = itemCraftingDataSO.itemStateCraftingRecipes[id].ItemState2;
        ItemState C = itemCraftingDataSO.itemStateCraftingRecipes[id].CraftedItemState;


        ItemData craftedItem = new ItemData(idso, C.amount, C.currentState);

        if (!A.isContainer)
        {
            inventory.RemoveSelectedItemServerRpc(false, A.amount);
        }
        else
        {
            inventory.ChangeStateOfItemServerRpc(inventory.inventorySlots.IndexOf(selectedInventorySlot), A.amount);
        }


        if (!C.isContainer)
        {
            inventory.TryAddItemServerRpc(craftedItem, true);
            Debug.Log("[ItemCrafting] Crafted item added to inventory.");
        }
        else
        {
            Debug.Log("[ItemCrafting] Crafted container item handled separately.");
        }

        

        if (!B.isContainer)
        {
            inventory.RemoveItemServerRpc(inventory.inventorySlots.IndexOf(itemState2InventorySlot), B.amount);
        }
        else
        {
            inventory.ChangeStateOfItemServerRpc(inventory.inventorySlots.IndexOf(itemState2InventorySlot),  B.amount);
        }



        finalItemState = new ItemState(craftedItem, craftedItem.amount);
        Debug.Log($"[ItemCrafting] Final Item State: {finalItemState.itemType}");

        return finalItemState;
    }
}
