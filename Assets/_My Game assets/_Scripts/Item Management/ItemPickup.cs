using Unity.Netcode;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;
    public ItemDataSO ItemDataSO;
    private ItemHolding it;
    public NetworkObject networkObject;

    private void Start()
    {
        if (GameManager.Instance.serverStarted)
        {
            it = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
        }
        itemData = new ItemData(ItemDataSO,itemData.amount,itemData.currentState);
        networkObject = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) 
            && (GetComponent<NetworkObject>().OwnerClientId == NetworkManager.ServerClientId || GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId) )
        {
            TryPickupItem();
        }
    }

    private void TryPickupItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                PickupItemServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupItemServerRpc(ServerRpcParams rpcParams = default)
    {
        var player = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        player.TryGetComponent<Inventory>(out var inventoryManager);
        it = player.GetComponent<ItemHolding>();

        if (inventoryManager != null)
        {
            int remainingItem = inventoryManager.AddItem(itemData);
            inventoryManager.UpdateInventoryToClient();
            Debug.Log(remainingItem.ToString());
            if (remainingItem == 0)
            {
                networkObject.Despawn();
                it.SetEverythingNormal(false);
            }
            else
            {
                itemData.amount = remainingItem;
                ReduceItemCountClientRPC(itemData, remainingItem);
                it.SetEverythingNormal(false);
            }
        }
    }

    [ClientRpc]
    private void ReduceItemCountClientRPC(ItemData itemData, int remain)
    {
        if (!IsOwner) return;
        itemData.amount = remain;
    }
}
