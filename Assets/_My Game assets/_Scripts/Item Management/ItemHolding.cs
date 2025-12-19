using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemHolding : NetworkBehaviour
{
    [Header("Zoom Settings")]
    public bool isZoomed;
    [SerializeField] private Vector3 zoomPos;
    [SerializeField] private Quaternion zoomRotation;


    [Header("Camera Settings")]
    public Camera playerCamera;

    [Header("Throw Settings")]
    [SerializeField]private float throwForce = 5;

    [Header("Held Item")]
    public ItemData heldItemData;
    public GameObject spawnedObject;
    public GameObject itemPrefab;
    public bool isInspecting;

    [Header("Showing Held Items")]
    public Transform heldItemPosition;
    public Transform heldItemParent;
    public List<GameObject> heldItemPrefabs; 

    [Header("UI")]
    public InventoryUI inventoryUI;
    public InventorySlotTracker inventorySlotTracker;

    [Header("Temporary Values For Throwing Specific No Of Items")]
    public ItemData temporaryItemData;


    Inventory Inventory;

    void Start()
    {
        Inventory = GetComponent<Inventory>();
        playerCamera = Camera.main;
        inventoryUI = FindAnyObjectByType<InventoryUI>();
        inventorySlotTracker = FindAnyObjectByType<InventorySlotTracker>();
        for (int i = 0; i < 5; i++)
        {
            heldItemPrefabs.Add(null);
        }
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
            GetZoomPosAndRot(out Vector3 pos, out Quaternion rot); 
            SpawnItemInstanceServerRpc(heldItemData, 1, false, default, pos, rot);
        }

        if (Input.GetKeyDown(KeyCode.Q) && isZoomed)
        {
            HandleUnZoom();
        }
    }

    public void HandleUnZoom()         //---------dont call directly ------//
    {
        isZoomed = false;

        isInspecting = false;
        Debug.Log("unzooming");

        if (spawnedObject != null)
        {
            spawnedObject = null;
            //DespawnObjectServerRpc(new NetworkObjectReference(spawnedObject.GetComponent<NetworkObject>()));
        }
        SetEverythingNormal(false);
    }

    void HandleThrow()
    {
        if (heldItemData == null) return;
        if (Input.GetKeyDown(KeyCode.G))
        {
            isInspecting = false;

            Debug.Log("unzooming");
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
            GetZoomPosAndRot(out Vector3 pos, out Quaternion rot);

            SpawnItemInstanceServerRpc(heldItemData, 1, true, playerCamera.transform.GetChild(0).transform.forward, pos, rot);
            spawnedObject = null;
            heldItemData.amount--;
            Debug.Log("removing 1");
            Inventory.RemoveSelectedItemServerRpc(false);             //remove one item from inventory slot//
            SetEverythingNormal(false);
            
        }
    }

    void ThrowEntireStack()
    {
        GetZoomPosAndRot(out Vector3 pos, out Quaternion rot);

        SpawnItemInstanceServerRpc(heldItemData, heldItemData.amount, true, playerCamera.transform.GetChild(0).transform.forward, pos, rot);
        spawnedObject = null;
        Inventory.RemoveSelectedItemServerRpc(true);
        SetEverythingNormal(false);
    }


    public void ThrowSpecificNoOfItems___InventoryNotUpdated(int quantity, ItemData itemData = null)
    {
        if (heldItemData == null && itemData == null) return;
        if (itemData == null) { itemData = heldItemData; }
        GetZoomPosAndRot(out Vector3 pos, out Quaternion rot); 
        SpawnItemInstanceServerRpc(itemData, quantity, true, playerCamera.transform.GetChild(0).transform.forward, pos, rot);
    }


    //TODO
    [ServerRpc(RequireOwnership = false)]
    void SpawnItemInstanceServerRpc(ItemData item, int quan, bool toThrow, Vector3 throwDirection, Vector3 spawnPos, Quaternion spawnRot, ServerRpcParams rpcParams = default)
    {
        if (item == null) { return; }

        //GameObject player = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.gameObject;       //----------Get the player who is throwing the item
        GameObject itemInstance = Instantiate(ScriptableObjectFinder.FindItemSO(item).itemPrefab, spawnPos, spawnRot);//----------Instantiate it
        itemInstance.GetComponent<NetworkObject>().Spawn(true);                                                                        //-----------spawn

        ItemData newItemData = itemInstance.GetComponent<ItemPickup>().itemData;//----------get itemdata of spawned object and set values
        newItemData.amount = quan;
        newItemData.currentState = item.currentState;
        spawnedObject = itemInstance;


        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();//-----------reference object for easy transfer across clients
        NotifyClientsAboutNewItemClientRpc(new NetworkObjectReference(networkObject), newItemData);
        if (toThrow)
        {
            spawnedObject.GetComponent<Rigidbody>().AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
        else
        {
            networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
            ZoomSpawnedClientRpc(new NetworkObjectReference(networkObject), rpcParams.Receive.SenderClientId); //========================== remove that from inventory  ======//
        }
    }

    [ClientRpc]
    private void NotifyClientsAboutNewItemClientRpc(NetworkObjectReference refe, ItemData newItemData)
    {
        spawnedObject = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        spawnedObject.GetComponent<ItemPickup>().itemData = newItemData;
    }

    [ClientRpc]
    void ZoomSpawnedClientRpc(NetworkObjectReference refe, ulong id)
    {
        if (!IsOwner) { return; }
        spawnedObject = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        spawnedObject.GetComponent<Inspection>().GrantPermissionToStartInspectionServerRpc(id);
        Inventory.RemoveSelectedItemServerRpc(false, 1, true);
    }
    
    private void GetZoomPosAndRot(out Vector3 pos, out Quaternion rot)
    {
        Transform target = playerCamera.transform.GetChild(0).transform;
        pos = target.position;
        rot = target.rotation;
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnObjectServerRpc(NetworkObjectReference refe)
    {
        GameObject obj = refe.TryGet(out NetworkObject networkObject) ? networkObject.gameObject : null;
        networkObject.Despawn();
    }

    


    public void HoldingItem(ItemData itemData, int quant, int currentState, bool animateInventory, bool lockMovement)
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

        HandleHeldItems();

        if (!lockMovement)
            SetEverythingNormal(animateInventory);
    }

    public void SetEverythingNormal(bool animateInventory)
    {
        isZoomed = false;
        GameManager.Instance.handlePlayerLookWithMouse = true;
        GameManager.Instance.handleMovement = true;
        GameManager.Instance.lockCurser = true;
        GameManager.Instance.itemScrollingLock = false;
        if (animateInventory)
            inventorySlotTracker.UpdateTracker(false);       //============== Update The Tracker which tracks inventory and store left, centre and right slots ===========//
        else 
            inventorySlotTracker.UpdateTracker(true);        //============== Update The Tracker without animating ===========//
    }

    private void HandleHeldItems()
    {
        int slotCount = Inventory.inventorySlots.Count;

        List<bool> itemTypeFlag = new(slotCount);

        // Fill the lists with default false values
        for (int i = 0; i < slotCount; i++)
        {
            itemTypeFlag.Add(false);
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (Inventory.inventorySlots[i] == null && heldItemPrefabs[i] != null)
            {
                itemTypeFlag[i] = true;
                continue;
            }

            if (Inventory.inventorySlots[i] != null && heldItemPrefabs[i] == null)
            {
                itemTypeFlag[i] = true;
                continue;
            }

            if (heldItemPrefabs[i] != null && heldItemPrefabs[i].TryGetComponent<DummyScriptForClassifyingItems>(out var dummy))
            {
                if (Inventory.inventorySlots[i].itemData == null || 
                    Inventory.inventorySlots[i].itemData.itemType != 
                    dummy.ItemData.itemType)
                    itemTypeFlag[i] = true;
            }
        }

        bool typeFlag = itemTypeFlag.Contains(true);

        if (typeFlag)
        {
            HandleNewHeldItems(itemTypeFlag);
        }

        CheckForCorrectInventorySelectedSlot();
    }

    private void CheckForCorrectInventorySelectedSlot()    // Disables all the held item except the current slotNo
    {
        int slot = Inventory.slotNo.Value;

        for (int i = 0; i < heldItemPrefabs.Count; i++)
        {
            if (i != slot)
            {
                heldItemPrefabs[i]?.SetActive(false);
            }
            if (i == slot)
            {
                heldItemPrefabs[i]?.SetActive(true);
            }
        }
    }

    private void HandleNewHeldItems(List<bool> itemTypeFlag)     // spawns the held item
    {
        for (int i = 0; i < itemTypeFlag.Count; i++)
        {
            if (itemTypeFlag[i])
            {
                Destroy(heldItemPrefabs[i]);
                heldItemPrefabs[i] = null;

                if (Inventory.inventorySlots[i].itemData == null) continue;

                ItemDataSO idso = ScriptableObjectFinder.FindItemSO(Inventory.inventorySlots[i].itemData);
                GameObject obj = heldItemPrefabs[i] = Instantiate(idso.dummyItemPrefab);
                //obj.GetComponent<NetworkObject>().Spawn();
                var dummy = obj.AddComponent<DummyScriptForClassifyingItems>();
                dummy.toFollow = heldItemPosition;
                dummy.ItemData = Inventory.inventorySlots[i].itemData;
            }
        }
    }

}
