using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;
    public ItemDataSO ItemDataSO;
    public DummyScriptForClassifyingItems dsfci;
    private ItemHolding it;
    public Inspection inspection;
    public NetworkObject networkObject;

    private void Start()
    {
        dsfci = GetComponent<DummyScriptForClassifyingItems>();
        if (GameManager.Instance.serverStarted && GameManager.Instance.ownerPlayer != null)
        {
            it = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
        }
        networkObject = GetComponent<NetworkObject>();

        // Setup Item Data
        if (itemData != null)
        {
            if (itemData.itemType == ItemType.Photo)
            {
                itemData = new ItemData(itemData.amount, itemData.currentState, itemData.photoType, itemData.photoId);
                var images = GetComponentsInChildren<Image>();
                if (images.Length > 0)
                    images[0].sprite = GameManager.Instance.GetPhotoSprite(itemData.photoType, itemData.photoId);
            }
            else if (!dsfci)
            {
                itemData = new ItemData(itemData);
            }else if (dsfci.ItemData != null)
            {
                itemData = dsfci.ItemData;
            }
        }

        if (dsfci && dsfci.ItemData != null)
        {
            itemData = dsfci.ItemData;
        }

        inspection = GetComponent<Inspection>();
    }

    private void Update()
    {
        // Don't allow pickup if we are dead/despawning
        if (!IsSpawned) return;

        if (Input.GetKeyDown(KeyCode.E) && (!inspection.isInspecting))
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

        // We get the ItemHolding component of the PLAYER who picked it up
        var playerItemHolding = player.GetComponent<ItemHolding>();

        if (inventoryManager == null || playerItemHolding == null) return;

        bool shouldDespawn = false;

        // --- Logic to Add to Inventory ---
        if (itemData.itemType == ItemType.Photo)
        {
            inventoryManager.AddPhoto(itemData);
            shouldDespawn = true;
        }
        else
        {
            int remainingItem = inventoryManager.AddItem(itemData);
            inventoryManager.UpdateInventoryToClient();

            if (remainingItem == 0)
            {
                shouldDespawn = true;
            }
            else
            {
                itemData.amount = remainingItem;
                ReduceItemCountClientRPC(itemData, remainingItem);

                // Reset UI for the player who picked it up (Server Side Logic)
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
                };
                ResetPlayerUIClientRpc(clientRpcParams);
            }
        }

        // --- THE FIX: Hard Despawn ---
        if (shouldDespawn)
        {
            // 1. Get the ID before we destroy it
            ulong objectId = networkObject.NetworkObjectId;

            // 2. Tell the specific client to reset their UI (Unzoom, enable mouse, etc)
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
            };
            ResetPlayerUIClientRpc(clientRpcParams);

            // 3. Force destroy on all clients immediately (Fixes the Ghost Object)
            ForceDespawnClientRpc(objectId);

            // 4. Finally, Despawn on Server
            networkObject.Despawn();
        }
    }

    [ClientRpc]
    private void ReduceItemCountClientRPC(ItemData itemData, int remain)
    {
        this.itemData.amount = remain;
    }

    [ClientRpc]
    private void ResetPlayerUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // This runs ONLY on the local machine of the player who picked up the item
        if (GameManager.Instance.ownerPlayer != null)
        {
            var holding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
            if (holding != null)
            {
                holding.SetEverythingNormal(false);
            }
        }
    }

    [ClientRpc]
    private void ForceDespawnClientRpc(ulong networkObjectId)
    {
        // Check if this object exists in the Netcode system
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {
            // If the standard Despawn hasn't killed it yet, we kill the GameObject manually
            if (obj != null && obj.gameObject != null)
            {
                Destroy(obj.gameObject);
            }
        }
        else
        {
            // If Netcode lost track of it (due to reparenting), try to destroy 'this' object if the IDs match
            if (NetworkObjectId == networkObjectId)
            {
                Destroy(gameObject);
            }
        }
    }


    #region FOR DATA MATCHING ACROSS CLIENTS

    public void SetItemData(ItemData newData)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can set item data!");
            return;
        }

        itemData = newData;

        SetItemDataClientRpc(newData);
    }

    [ClientRpc]
    private void SetItemDataClientRpc(ItemData newData)
    {
        this.itemData = newData;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            RequestDataServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDataServerRpc(ServerRpcParams rpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
            }
        };

        // Send the current data ONLY to that specific new player
        SetItemDataClientRpc(itemData, clientRpcParams);
    }

    [ClientRpc]
    private void SetItemDataClientRpc(ItemData newData, ClientRpcParams clientRpcParams = default)
    {
        this.itemData = newData;

        // Update your visuals here (e.g. materials, text)
        Debug.Log($"Client received item: {itemData.itemType}");
    }


    #endregion
}