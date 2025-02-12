using Unity.Netcode;
using UnityEngine;

public class SpecialProcedure : NetworkBehaviour   //+++++++++THIS PROCEDURE IS ON PLAYER OBJECT+++++++++//
{

    Inventory ownerInventory;

    [Header("Procedure Variables")]
    public NetworkVariable<bool> isCompleted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        if (ownerInventory == null && IsOwner)
        {
            ownerInventory = GetComponent<Inventory>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && IsOwner)
        {
            Debug.Log("button");
            if (ownerInventory.selectedInventorySlot.itemData.itemType == ItemType.VoodooDoll)
            {
                Debug.Log("doll");
                if (ownerInventory.selectedInventorySlot.itemData.currentState == 2 && !isCompleted.Value)
                {
                    SpecialProcedureDoneServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpecialProcedureDoneServerRpc()
    {
        isCompleted.Value = true;
        GameManager.Instance.completedProcedures.Add(7);               //TODO===========VALUE USED 7 ================//
    }
}
