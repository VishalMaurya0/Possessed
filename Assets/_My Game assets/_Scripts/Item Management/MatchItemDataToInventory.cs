using Unity.Netcode;
using UnityEngine;

public class MatchItemDataToInventory : MonoBehaviour
{
    public DummyScriptForClassifyingItems dsfci;
    public ulong currentPlayerID;
    Inventory inventory;
    ItemData itemData;

    private void Start()
    {
        dsfci = GetComponent<DummyScriptForClassifyingItems>();
    }

    public void UpdateItemData()
    {
        inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
        if (dsfci == null) dsfci = GetComponent<DummyScriptForClassifyingItems>();
        try {
            itemData = dsfci.ItemData;
            UpdateServerRPC(itemData.isOn, inventory.slotNo.Value);
        } catch
        {
            Debug.LogError("not updated item data isOn value");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateServerRPC(bool isOn, int slotNo, ServerRpcParams serverRpcParams = default)
    {
        if (inventory == null || currentPlayerID != serverRpcParams.Receive.SenderClientId)
        {
            inventory = GameManager.Instance.connectedClientsData.Find(data => data.clientID == serverRpcParams.Receive.SenderClientId).playerGameobject.GetComponent<Inventory>();
            currentPlayerID = serverRpcParams.Receive.SenderClientId;
        }
        inventory.inventorySlots[slotNo].itemData.isOn = isOn;
    }
}
