using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProcedureBase : NetworkBehaviour
{
    public List<ProcedureBase> allProcedures = new();



    public List<Vector3> position = new();
    public List<Vector3> rotation = new();


    private void Awake()
    {
        for (int i = 0; i < 8; i++)
        {
            allProcedures.Add(null);
            position.Add(Vector3.zero);
            rotation.Add(Vector3.zero);
        }
    }
}



[System.Serializable]
public class ItemNeeded
{
    public int orderId;
    public ItemType ItemType;
    public int currentState;
    public int requiredAmount;
    public int timeToWaitAfterAddingAAmount;
}

[System.Serializable]
public struct TotalItemsNeeded
{
    public List<ItemNeeded> itemNeeded;
    public List<int> addedAmount;
}