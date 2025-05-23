using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class InventorySlotTracker : MonoBehaviour
{
    public Inventory inventory;
    public LeftSlot leftSlot;
    public RightSlot rightSlot;
    public CurrentSlot currentSlot;
    InventoryUI inventoryUI;


    public bool updateTracker;


    /// <summary>
    /// Reference When Sever Starts
    /// </summary>
    void OnEnable()
    {
        GameManager.onServerStarted += OnServerStartedCallback;
    }

    private void OnDisable()
    {
        GameManager.onServerStarted -= OnServerStartedCallback;
    }

    private void OnServerStartedCallback()
    {
        if (GameManager.Instance == null || GameManager.Instance.ownerPlayer == null)
        {
            Debug.LogError("GameManager instance or ownerPlayer is NULL!");
            return;
        }

        inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
        inventoryUI = FindAnyObjectByType<InventoryUI>();

        leftSlot = new LeftSlot();
        rightSlot = new RightSlot();
        currentSlot = new CurrentSlot();
    }

    /// <summary>
    /// .........
    /// </summary>




    private void Update()
    {
        if (updateTracker)
        {
            updateTracker = false;
            UpdateTracker(true);
        }
    }




    public void UpdateTracker(bool spawnIcons)
    {
        int currentIndex = inventory.slotNo.Value;

        for (int i = leftSlot.slots.Count - 1, a = 0; i >= 0; i--)        //======= LOOP STARTS (o)ooo (((starts as 3,2,1,0 but set the value of 0,1,2,3))) ===========//  i = no of elements right of (0,1,2,3)
        {
            if (currentIndex > i)        //========= if no of left elements in inventory > no of elements rightward of (0,1,2,3)th element ====//
            {
                if (inventory.inventorySlots[a].itemData == null)   //========== if it is null then null it and add a by 1 ==========//   a = inventory's ath element
                {
                    leftSlot.slots[leftSlot.slots.Count - 1 - i].isFull = false;
                    leftSlot.slots[leftSlot.slots.Count - 1 - i].inventorySlot = null;
                    a++;
                }
                else            //========= if inventory is not null put it =========//
                {
                    leftSlot.slots[leftSlot.slots.Count - 1 - i].isFull = true;
                    leftSlot.slots[leftSlot.slots.Count - 1 - i].inventorySlot = inventory.inventorySlots[a];
                    a++;
                }
            }
            else      //============ null the rest ============// a does not increment
            {
                leftSlot.slots[leftSlot.slots.Count - 1 - i].isFull = false;
                leftSlot.slots[leftSlot.slots.Count - 1 - i].inventorySlot = null;
            }
        }

        for (int i = 0, a = currentIndex + 1; i < rightSlot.slots.Count; i++)
        {
            if (a < inventory.inventorySlots.Count)
            {
                if (inventory.inventorySlots[a].itemData == null)
                {
                    rightSlot.slots[i].isFull = false;
                    rightSlot.slots[i].inventorySlot = null;
                    a++;
                }
                else
                {
                    rightSlot.slots[i].isFull = true;
                    rightSlot.slots[i].inventorySlot = inventory.inventorySlots[a];
                    a++;
                }
            }
            else
            {
                rightSlot.slots[i].isFull = false;
                rightSlot.slots[i].inventorySlot = null;
            }
        }

        currentSlot.slot.inventorySlot = inventory.inventorySlots[inventory.slotNo.Value];
        currentSlot.slot.isFull = (inventory.inventorySlots[inventory.slotNo.Value].itemData != null);

        inventoryUI.InitializeIcon();
        inventoryUI.SetIcon(spawnIcons);
    }
}







[System.Serializable]
public class LeftSlot
{
    public List<SlotItem> slots = new List<SlotItem>();

    public LeftSlot()
    {
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.inventorySlots - 1; i++)
            {
                slots.Add(new SlotItem());
            }
        }
    }
}






[System.Serializable]
public class CurrentSlot
{
    public SlotItem slot = new SlotItem();
}







[System.Serializable]
public class RightSlot
{
    public List<SlotItem> slots = new List<SlotItem>();

    public RightSlot()
    {
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.inventorySlots - 1; i++)
            {
                slots.Add(new SlotItem());
            }
        }
    }
}







[System.Serializable]
public class SlotItem
{
    public InventorySlot inventorySlot;
    public bool isFull;

    public SlotItem()
    {
        this.inventorySlot = null;
        isFull = false;
    }

}
