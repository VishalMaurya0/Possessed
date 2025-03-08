using System;
using Unity.Netcode;
using UnityEngine;

public class ItemHolding : NetworkBehaviour
{
    [Header("Zoom Settings")]
    public bool isZoomed;
    public float moveSpeed = 0.01f;
    public float zoomDistFromCam = 2.5f;
    private Vector3 zoomPos;

    [Header("Camera Settings")]
    public Camera playerCamera;

    [Header("Throw Settings")]
    [SerializeField]private float throwForce = 5;

    [Header("Held Item")]
    public ItemData heldItemData;
    public GameObject spawnedObject;
    public GameObject itemPrefab;

    [Header("UI")]
    public InventoryUI inventoryUI;
    public InventorySlotTracker inventorySlotTracker;

    [Header("Temporary Values For Throwing Specific No Of Items")]
    public ItemData temporaryItemData;


    Inventory Inventory;

    void Start()
    {
        Inventory = GetComponent<Inventory>();
        playerCamera = GameObject.FindAnyObjectByType<Camera>();
        inventoryUI = FindAnyObjectByType<InventoryUI>();
        inventorySlotTracker = FindAnyObjectByType<InventorySlotTracker>();
    }

    void Update()
    {
        if (!IsOwner) { return; }
        HandleZoom();
        HandleThrow();
    }

    void HandleZoom()
    {
        if (Input.GetKeyDown(KeyCode.F) && heldItemData != null && !isZoomed)
        {
            isZoomed = true;
            SpawnItemInstanceServerRpc(heldItemData, 1, false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && isZoomed)
        {
            HandleUnZoom();
        }
    }

    public void HandleUnZoom()         //---------dont call directly ------//
    {
        isZoomed = false;
        SetEverythingNormal();
        if (spawnedObject != null)
        {
            //DespawnObjectServerRpc(new NetworkObjectReference(spawnedObject.GetComponent<NetworkObject>()));
        }
    }

    void HandleThrow()
    {
        if (heldItemData == null) return;
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (isZoomed)
            {
                spawnedObject.GetComponent<Inspection>().EndInspection();
                //ThrowOneItem();
            }
            else
                ThrowEntireStack();
        }
    }

    void ThrowOneItem()
    {
        if (heldItemData?.amount > 0)
        {
            SpawnItemInstanceServerRpc(heldItemData, 1, true);
            spawnedObject = null;
            heldItemData.amount--;
            Debug.Log("removing 1");
            Inventory.RemoveSelectedItemServerRpc(false);             //remove one item from inventory slot//
            SetEverythingNormal();
            
        }
    }

    void ThrowEntireStack()
    {

        SpawnItemInstanceServerRpc(heldItemData, heldItemData.amount, true);
        spawnedObject = null;
        Inventory.RemoveSelectedItemServerRpc(true);
        SetEverythingNormal();
    }


    public void ThrowSpecificNoOfItems___InventoryNotUpdated(int quantity, ItemData itemData = null)
    {
        if (heldItemData == null && itemData == null) return;
        if (itemData == null) { itemData = heldItemData; }
        SpawnItemInstanceServerRpc(itemData, quantity, true);
    }


    //TODO
    [ServerRpc(RequireOwnership = false)]
    void SpawnItemInstanceServerRpc(ItemData item, int quan = 1, bool toThrow = false, ServerRpcParams rpcParams = default)
    {
        if (item == null) { return; }

        GameObject player = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.gameObject;       //----------Get the player who is throwing the item
        GameObject itemInstance = Instantiate(ScriptableObjectFinder.FindItemSO(item).itemPrefab, ZoomPos(player), Quaternion.identity);//----------Instantiate it
        itemInstance.GetComponent<NetworkObject>().Spawn(true);                                                                        //-----------spawn


        ItemData newItemData = itemInstance.GetComponent<ItemPickup>().itemData;//----------get itemdata of spawned object and set values
        newItemData.amount = quan;
        newItemData.currentState = item.currentState;
        spawnedObject = itemInstance;


        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();//-----------reference object for easy transfer across clients
        NotifyClientsAboutNewItemClientRpc(new NetworkObjectReference(networkObject), newItemData);
        if (toThrow)
        {
            spawnedObject.GetComponent<Rigidbody>().AddForce(player.transform.GetChild(1).transform.forward * throwForce, ForceMode.Impulse);
        }
        else
        {
            networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
            ZoomSpawnedClientRpc(new NetworkObjectReference(networkObject)); //========================== remove that from inventory  ======//
        }
    }

    [ClientRpc]
    private void NotifyClientsAboutNewItemClientRpc(NetworkObjectReference refe, ItemData newItemData)
    {
        spawnedObject = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        spawnedObject.GetComponent<ItemPickup>().itemData = newItemData;
    }

    [ClientRpc]
    void ZoomSpawnedClientRpc(NetworkObjectReference refe)
    {
        if (!IsOwner) { return; }
        spawnedObject = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        spawnedObject.GetComponent<Inspection>().StartInspection();
        Inventory.RemoveSelectedItemServerRpc(false, 1);
    }
    

    private Vector3 ZoomPos(GameObject player)
    {
        zoomPos = player.transform.GetChild(1).transform.position;
        return zoomPos;
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnObjectServerRpc(NetworkObjectReference refe)
    {
        GameObject obj = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        networkObject.Despawn();
    }

    


    public void HoldingItem(ItemData itemData, int quant, int currentState)
    {
        heldItemData = itemData;
        if (itemData != null)
        {
            itemPrefab = ScriptableObjectFinder.FindItemSO(itemData).itemPrefab;
        }
        else
        {
            itemPrefab = null;
        }
        if (heldItemData != null)
        {
            heldItemData.amount = quant;
            heldItemData.currentState = currentState;
        }
    }

    public void SetEverythingNormal()
    {
        GameManager.Instance.handlePlayerLookWithMouse = true;
        GameManager.Instance.handleMovement = true;
        GameManager.Instance.lockCurser = true;
        inventorySlotTracker.UpdateTracker(true);
    }

}
