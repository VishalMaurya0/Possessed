using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
        networkObject = GetComponent<NetworkObject>();

        if (itemData != null)
        {
            if (itemData.itemType == ItemType.Photo)
            {
                itemData = new ItemData(itemData.amount, itemData.currentState, itemData.photoType, itemData.photoId);
                GetComponentsInChildren<Image>()[0].sprite = GameManager.Instance.GetPhotoSprite(itemData.photoType, itemData.photoId);
            }
            else
            {
                itemData = new ItemData(ItemDataSO, itemData.amount, itemData.currentState);
            }
        }
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
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(rpcParams.Receive.SenderClientId)) return;

        var player = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        player.TryGetComponent<Inventory>(out var inventoryManager);
        it = player.GetComponent<ItemHolding>();

        if (inventoryManager == null || it == null)
        {
            Debug.LogError($"Pickup Failed: Missing Inventory or ItemHolding on Player {rpcParams.Receive.SenderClientId}");
            return;
        }

        if (inventoryManager != null)
        {
            // -------Special handling for Photo items
            if (itemData.itemType == ItemType.Photo)
            {
                inventoryManager.AddPhoto(itemData);
                //it.UpdatePhotoAlbumClientRPC(itemData.currentState);
                networkObject.ChangeOwnership(NetworkManager.ServerClientId);
                networkObject.Despawn();
                it.SetEverythingNormal(false);
                return;
            }


            int remainingItem = inventoryManager.AddItem(itemData);
            inventoryManager.UpdateInventoryToClient();
            Debug.Log(remainingItem.ToString());
            if (remainingItem == 0)
            {
                networkObject.ChangeOwnership(NetworkManager.ServerClientId);
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
        itemData.amount = remain;
    }

}
