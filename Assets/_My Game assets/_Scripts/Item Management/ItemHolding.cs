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
    private bool[] _isSlotPendingSpawn;

    [Header("UI")]
    public InventoryUI inventoryUI;
    public InventorySlotTracker inventorySlotTracker;

    [Header("Temporary Values For Throwing Specific No Of Items")]
    public ItemData temporaryItemData;


    Inventory Inventory;

    private void Awake()
    {
        Inventory = GetComponent<Inventory>();
    }

    void Start()
    {
        playerCamera = Camera.main;
        inventoryUI = FindAnyObjectByType<InventoryUI>();
        inventorySlotTracker = FindAnyObjectByType<InventorySlotTracker>();
        for (int i = 0; i < 5; i++)
        {
            heldItemPrefabs.Add(null);
        }
        if (Inventory.inventorySlots != null)
        {
            _isSlotPendingSpawn = new bool[Inventory.inventorySlots.Count];
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
        GameObject itemInstance = Instantiate(ScriptableObjectFinder.Instance.FindItemSO(item).itemPrefab, spawnPos, spawnRot);//----------Instantiate it
        itemInstance.GetComponent<NetworkObject>().Spawn(true);                                                                        //-----------spawn

        //----------get itemdata of spawned object and set values
        ItemData newItemData = itemInstance.GetComponent<ItemPickup>().itemData = new ItemData(item);
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
            itemPrefab = ScriptableObjectFinder.Instance.FindItemSO(itemData).itemPrefab;
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
        //if (!IsOwner) return;


        int slotCount = Inventory.inventorySlots.Count;

        List<bool> itemTypeFlag = new(slotCount);

        if (_isSlotPendingSpawn == null || _isSlotPendingSpawn.Length != slotCount)
            _isSlotPendingSpawn = new bool[slotCount];

        // Fill the lists with default false values
        for (int i = 0; i < slotCount; i++)
        {
            itemTypeFlag.Add(false);
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (_isSlotPendingSpawn[i]) continue;


            if (Inventory.inventorySlots[i] == null && heldItemPrefabs[i] != null)
            {
                itemTypeFlag[i] = true;
                continue;
            }

            if (Inventory.inventorySlots[i] != null && heldItemPrefabs[i] == null && Inventory.inventorySlots[i].itemData != null)
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
            if (heldItemPrefabs[i] == null) continue;
            if (i != slot)
            {
                heldItemPrefabs[i].SetActive(false);
            }
            if (i == slot)
            {
                heldItemPrefabs[i].SetActive(true);
            }
        }
    }

    private void HandleNewHeldItems(List<bool> itemTypeFlag)
    {
        for (int i = 0; i < itemTypeFlag.Count; i++)
        {
            if (itemTypeFlag[i])
            {
                if (heldItemPrefabs[i] != null)
                {
                    NetworkObject oldNetObj = heldItemPrefabs[i].GetComponent<NetworkObject>();

                    if (oldNetObj != null)
                    {
                        if (IsOwner)
                            RequestDespawnServerRPC(oldNetObj.NetworkObjectId);
                    }
                    else
                    {
                        Destroy(heldItemPrefabs[i]);
                    }

                    heldItemPrefabs[i] = null;
                }

                if (Inventory.inventorySlots[i].itemData == null) continue;

                if (IsOwner)
                {
                    ItemData itemData = Inventory.inventorySlots[i].itemData;
                    _isSlotPendingSpawn[i] = true;
                    PermissionToSpawnServerRPC((int)itemData.itemType, i);
                }
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRPC(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            netObj.Despawn();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void PermissionToSpawnServerRPC(int itemID, int slotIndex, ServerRpcParams serverRpcParams = default)
    {
        ItemDataSO idso = ScriptableObjectFinder.Instance.FindItemSO(itemID);
        if (idso == null || idso.dummyItemPrefab == null) return;

        GameObject obj = Instantiate(idso.dummyItemPrefab);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();

        netObj.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

        InformClientClientRPC(new NetworkObjectReference(netObj), slotIndex, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void InformClientClientRPC(NetworkObjectReference netObjRef, int slotIndex, ulong id = 0)
    {
        if (netObjRef.TryGet(out NetworkObject netObj))
        {
            GameObject obj = netObj.gameObject;
            heldItemPrefabs[slotIndex] = obj;

            var dummy = obj.AddComponent<DummyScriptForClassifyingItems>();
            dummy.toFollow = heldItemPosition;
            dummy.ItemData = Inventory.inventorySlots[slotIndex].itemData;
            dummy.playerID = id;

            //Debug.LogError(dummy.ItemData.GetHashCode());
            //Debug.LogError(Inventory.inventorySlots[slotIndex].itemData.GetHashCode());
            if (_isSlotPendingSpawn != null && slotIndex < _isSlotPendingSpawn.Length)
            {
                _isSlotPendingSpawn[slotIndex] = false;
            }

            CheckForCorrectInventorySelectedSlot();
        }
    }

    public override void OnNetworkSpawn()
    {
        // Now Inventory is guaranteed to be assigned
        if (Inventory != null && Inventory.slotNo != null)
        {
            Inventory.slotNo.OnValueChanged += OnSlotChanged;

            // OPTIONAL: Manually call it once to sync the initial state
            OnSlotChanged(0, Inventory.slotNo.Value);
        }
        else
        {
            Debug.LogError("Inventory or slotNo is NULL in OnNetworkSpawn!");
        }
    }

    public override void OnNetworkDespawn()
    {
        // Good practice to unsubscribe
        if (Inventory != null && Inventory.slotNo != null)
        {
            Inventory.slotNo.OnValueChanged -= OnSlotChanged;
        }
    }

    // This function runs on ALL clients automatically when the variable changes
    private void OnSlotChanged(int oldVal, int newVal)
    {
        CheckForCorrectInventorySelectedSlot();
    }
}
