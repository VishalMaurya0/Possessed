using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Procedure6 : NetworkBehaviour
{
    [Header("Item Visuals")]
    public ProcedureCompletion procedureCompletion;
    public triggerProcedurePointScript triggerScript;

    [Header("Procedure Visuals")]
    //public Procedure3Visuals procedure3Visuals;
    public Animator animator;

    [Header("Procedure Data")]
    //NetworkVariable<bool> procedureCompleted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Inventory inventory;
    bool isInspecting = false;
    public List<ItemType> items = new();
    
    [Header("Server Data")]
    public List<KeyValuePair<ulong, ItemType>> inspectionItems = new();


    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (procedureCompletion.isCompleted.Value)
            return;

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[SpecialProcedure] NetworkManager not ready or not listening.");
            return;
        }

        if (inventory == null && GameManager.Instance.serverStarted)
        {
            inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
        }

        if (triggerScript == null)
        {
            triggerScript = procedureCompletion.triggerScript;
            if (triggerScript == null)
            {
                Debug.LogWarning("[SpecialProcedure] triggerProcedurePointScript is missing!");
                return;
            }
        }

        if (triggerScript.inProgress && Input.GetKeyDown(KeyCode.F) && inventory != null && !isInspecting)
        {
            //Debug.LogError("F pressed");
            if (inventory.selectedInventorySlot == null || inventory.selectedInventorySlot.itemData == null)
            {
                GameManager.Instance.HelpInstructions.text = "No item selected for inspection.";
                GameManager.Instance.helpInstructionDisplayTime = 3f;
                return;
            }
            var selectedType = inventory.selectedInventorySlot.itemData.itemType;
            if (items.Contains(selectedType))
            {
                isInspecting = true;
                inventory.itemHolding.isInspecting = true;
                //Debug.LogError("Started Inspecting");
                CheckForInspectionServerRPC(inventory.selectedInventorySlot.itemData);
            }
        }
        if (triggerScript.inProgress && (Input.GetKeyDown(KeyCode.G) || Input.GetMouseButtonDown(0)))
        {
            isInspecting = false;
            GameManager.Instance.HelpInstructions.text = "You need to inspect the items! Press F";
            GameManager.Instance.helpInstructionDisplayTime = 3f;
        }

        if (triggerScript.inProgress && isInspecting && inventory != null)
        {
            if (inventory.itemHolding != null)
            {
                if (!inventory.itemHolding.isInspecting)
                {
                    isInspecting = false;
                    CheckForInspectionServerRPC(null);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckForInspectionServerRPC(ItemData itemData, ServerRpcParams rpcParams = default)
    {
        ulong id = rpcParams.Receive.SenderClientId;

        //check for the id match the check if the item is already in the list
        for (int i = 0; i < inspectionItems.Count; i++)
        {
            if (inspectionItems[i].Key == id)
            {
                inspectionItems.Remove(inspectionItems[i]);
                break;
            }
        }

        if (itemData != null)
        {
            KeyValuePair<ulong, ItemType> inspectionItem = new KeyValuePair<ulong, ItemType>(id, itemData.itemType);
            inspectionItems.Add(inspectionItem);
            procedureCompletion.ShowVFXClientRPC();
        }
        Debug.LogError("Inspection Items Count: " + inspectionItems.Count);

        ShowAnimClientRPC(inspectionItems.Count);

        if (inspectionItems.Count == 4)
        {
            CheckCompletionOfProcedure();
        }

        void CheckCompletionOfProcedure()
        {
            for (int i = 0; i < inspectionItems.Count; i++)
            {
                if (!items.Contains(inspectionItems[i].Value))
                {
                    GameManager.Instance.HelpInstructions.text = "You need to inspect the correct items!";
                    GameManager.Instance.helpInstructionDisplayTime = 3f;
                    return;
                }
            }
            procedureCompletion.ShowVFXClientRPC();
            procedureCompletion.CheckForProcedureCompletionServerRPC(true);
        }
    }

    [ClientRpc]
    private void ShowAnimClientRPC(int val)
    {
        animator.SetInteger("Value", val);
    }
}
