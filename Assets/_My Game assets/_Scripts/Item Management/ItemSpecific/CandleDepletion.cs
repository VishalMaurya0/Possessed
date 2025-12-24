using UnityEngine;

public class CandleDepletion : MonoBehaviour
{
    public ItemPickup itemPickup;

    public float burningTime = 200f;
    public float timer = 0f;
    public bool isBurning = false;

    ItemData ItemData;
    DummyScriptForClassifyingItems dsfci;

        Inventory inventory = null;
    public float timerOfUpdate;

    private void Update() {
        
        if (itemPickup == null)
            itemPickup = gameObject.GetComponent<ItemPickup>();

        if (dsfci == null)
            dsfci = GetComponent<DummyScriptForClassifyingItems>();

        if (itemPickup != null)
        {
            if (ItemData == null)
            {
                ItemData = itemPickup.itemData;
                timer = ItemData.photoId;
            }
        }

        if (ItemData != null)
        {
            isBurning = ItemData.currentState == 1;
            if (isBurning)
            {
                timer += Time.deltaTime;
                if (timer >= burningTime)
                {
                    itemPickup.itemData.currentState = 2;
                    isBurning = false;
                }
                ItemData.photoId = (int)timer;
                Debug.LogError(ItemData.photoId);
            }
        }

        if (!inventory && dsfci && dsfci.playerID == GameManager.Instance.OwnerClientId)
        {
            inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
        }

            timerOfUpdate -= Time.deltaTime;
        if (inventory && timerOfUpdate < 0 && dsfci && dsfci.playerID == GameManager.Instance.OwnerClientId)
        {
            timerOfUpdate = 2;
            InventorySlot slot = inventory.inventorySlots[inventory.slotNo.Value];
            if (slot != null && slot.itemData.itemType == ItemData.itemType)
            {
                slot.itemData.photoId = ItemData.photoId;
            }
        }
        //Debug.LogError(inventory.inventorySlots[inventory.slotNo.Value].itemData == ItemData);
    }
}
