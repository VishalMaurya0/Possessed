using TMPro;
using UnityEditor;
using UnityEngine;

public class ItemAmountUI : MonoBehaviour
{
    Inventory inventory;
    TMP_Text amountText;
    TMP_Text totalAmountText;
    TMP_Text itemNameText;
    TMP_Text amountInContainerText;
    TMP_Text stateText;


    public string Equip = "Press 'E' to Equip an Item";
    public string Null = "_";

    private void Start()
    {
        amountText = transform.GetChild(0).GetComponent<TMP_Text>();
        totalAmountText = transform.GetChild(2).GetComponent<TMP_Text>();
        itemNameText = transform.GetChild(3).GetComponent<TMP_Text>();
        amountInContainerText = transform.GetChild(4).GetComponent<TMP_Text>();
        stateText = transform.GetChild(5).GetComponent<TMP_Text>();
        itemNameText.SetText(Equip);
    }

    private void Update()
    {
        if (inventory == null && GameManager.Instance.serverStarted && !GameManager.Instance.gameEnd && GameManager.Instance.ownerPlayer != null)
        {
            inventory = GameManager.Instance.ownerPlayer?.GetComponent<Inventory>();
        }
    }


    public void UpdateTotalAmountAndNameUI()
    {
        if (!inventory) return;
        ItemData itemData = inventory.inventorySlots[inventory.slotNo.Value].itemData;
        if (itemData != null)
        {
            ItemDataSO idso = ScriptableObjectFinder.Instance.FindItemSO(itemData);
            totalAmountText.SetText($"{idso.maxStackSize}");
            itemNameText.SetText(idso.itemName);

            int amountInContainer;
            if (idso.isContainer)
            {
                amountInContainerText.gameObject.SetActive(true);
                amountInContainer = (idso.states.Length - 1) - inventory.selectedInventorySlot.itemData.currentState;
                amountInContainerText.SetText($"{amountInContainer}");
                stateText.SetText("");
            }else
            {
                amountInContainerText.gameObject.SetActive(false);
                stateText.SetText($"State: {itemData.currentState}");
            }
            UpdateCurrentAmountUI();
        }else
        {
            totalAmountText.SetText(Null);
            amountText.SetText(Null);
            itemNameText.SetText(Equip);
            amountInContainerText.SetText(Null);
            stateText.SetText(Null);
        }

    }

    public void UpdateCurrentAmountUI()
    {
        amountText.SetText($"{inventory.selectedInventorySlot.quantity}");
    }
}
