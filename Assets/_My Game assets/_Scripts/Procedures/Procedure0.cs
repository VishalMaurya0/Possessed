//using System;
//using System.Collections.Generic;
//using Unity.Netcode;
//using UnityEngine;

//public class Procedure0 : ProcedureBase
//{
//    ProcedureBase procedureBase;

//    public List<ItemNeeded> itemsNeeded;
//    int totalItems;

//    [Header("Procedure Variables")]
//    bool woodDone;
//    bool fireDone;
//    bool powderDone;

//    void Start()
//    {
//        totalItems = itemsNeeded.Count;
//        procedureBase = GameManager.Instance.procedureBase;
//        if (procedureBase != null)
//        {
//            procedureBase.allProcedures[0] = this;
//            procedureBase.position[0] = transform.position;
//        }
//    }

//    void Update()
//    {
//        if (GetComponentInChildren<triggerProcedurePointScript>().inProgress && Input.GetMouseButtonDown(0))
//        {
//            Inventory inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
//            ItemHolding itemHolding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
//            InventorySlot selectedInventorySlot = inventory.selectedInventorySlot;
//            for (int i = 0; i < totalItems; i++)
//            {
//                if (CheckIfItemMatchedWithInventorySlot(itemsNeeded[i], selectedInventorySlot.itemData))
//                {
//                    //-------Item Mached With Inventory Item------//
//                    itemsNeeded[i].amountToAdd++;
//                    if (IsProcedureUpdated())
//                    {
//                        itemHolding.RemoveItemServerRpc(false);
//                    }else
//                    {
//                        itemsNeeded[i].amountToAdd--;
//                    }
//                }
//            }
//        }
//    }

//    private bool CheckIfItemMatchedWithInventorySlot(ItemNeeded itemToCheckAndAdd, ItemData itemDataInInventory)
//    {
//        if (itemToCheckAndAdd.ItemType == itemDataInInventory?.itemType && itemToCheckAndAdd.ItemType == ItemType.Wood)
//        {
//            if (0 == itemToCheckAndAdd.currentState && itemToCheckAndAdd.currentState == itemDataInInventory.currentState && !woodDone)
//            {
//                return true;
//            }
//            if (2 == itemToCheckAndAdd.currentState && itemToCheckAndAdd.currentState == itemDataInInventory.currentState && !fireDone)
//            {
//                return true;
//            }
//        }
//        if (itemToCheckAndAdd.ItemType == itemDataInInventory?.itemType && itemToCheckAndAdd.ItemType == ItemType.PurePowder && !powderDone)
//        {
//            return true;
//        }
//        return false;
//    }



//    private bool IsProcedureUpdated()
//    {
//        foreach (var item in itemsNeeded)
//        {
//            if(item.amountToAdd > 0 && item.amountToAdd > item.addedAmount)
//            {
//                if (item.ItemType == ItemType.Wood && item.currentState == 0)
//                {
//                    item.addedAmount++;
//                    if (item.addedAmount >= item.requiredAmount)
//                    {
//                        woodDone = true;
//                    }


//                    return true;
//                    // select random wood and activeSelf
//                }

//                if (item.ItemType == ItemType.Wood && item.currentState == 2)
//                {
//                    item.addedAmount++;
//                    if (item.addedAmount >= item.requiredAmount)
//                    {
//                        fireDone = true;
//                    }
//                    return true;
//                    // select burning wood and activeSelf
//                }

//                if (item.ItemType == ItemType.PurePowder && woodDone && fireDone)
//                {
//                    item.addedAmount++;
//                    if (item.addedAmount >= item.requiredAmount)
//                    {
//                        powderDone = true;
//                    }
//                    return true;
//                    // apply pure powder effect
//                }
//            }
//        }
//        return false;
//    }
//}
