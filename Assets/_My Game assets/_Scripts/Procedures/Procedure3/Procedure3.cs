using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecialProcedure : NetworkBehaviour   //+++++++++THIS PROCEDURE IS ON DECAL OBJECT+++++++++//
{
    [Header("Item Visuals")]
    public ProcedureCompletion procedureCompletion;
    public triggerProcedurePointScript triggerScript;
    public List<VisualsTrigger> visualsTrigger;
    public MeshRenderer doll;

    [Header("Procedure Visuals")]
    public Procedure3Visuals procedure3Visuals;

    [Header("Procedure Data")]
    public GameObject Pin;
    NetworkVariable<bool> procedureCompleted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Inventory inventory;
    public ItemType item;

    private void Start()
    {
        visualsTrigger = procedureCompletion.visualsTrigger;
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[SpecialProcedure] NetworkManager not ready or not listening.");
            return;
        }

        if (inventory == null && GameManager.Instance.serverStarted)
        {
            inventory = GameManager.Instance.ownerPlayer.GetComponent<Inventory>();
            gameObject.SetActive(false);
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

        if (triggerScript.inProgress && Input.GetMouseButtonDown(0) && inventory != null)
        {

            var selectedType = inventory.selectedInventorySlot.itemData.itemType;

            if (selectedType == item)
            {
                CheckForIncorrectInputServerRpc();
            }
        }
    }

    private void OnMouseDown()
    {
        if (triggerScript.inProgress && inventory != null)
        {
            var selectedType = inventory.selectedInventorySlot.itemData.itemType;

            if (selectedType == item)
            {
                SpawnPinServerRpc();
                procedureCompletion.CheckOrderCompletionServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPinServerRpc()
    {
        procedureCompleted.Value = true;
        Pin.gameObject.SetActive(true);

        procedureCompletion.totalItemsNeeded.addedAmount[1]++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckForIncorrectInputServerRpc()
    {
        StartCoroutine(DespawnDoll());
    }

    IEnumerator DespawnDoll()
    {
        yield return new WaitForSeconds(0.1f);

        if (!procedureCompleted.Value)
        {
            WrongInput();
        }
    }

    private void WrongInput()
    {
        procedureCompletion.totalItemsNeeded.addedAmount[0] = 0;
        procedureCompletion.currentOrder.Value = 0;
        visualsTrigger[0].trigger[0] = false;
        procedureCompletion.showVisual = new KeyValuePair<bool, int>(true, -1);
    }
}
