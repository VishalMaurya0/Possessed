using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcedureData", menuName = "Scriptable Objects/ProcedureData")]
public class ProcedureData : ScriptableObject
{
    public int procedureIndex;
    public Procedures procedure;
    public string procedureName;
    public GameObject procedurePrefab;
    public List<ItemNeeded> itemsNeeded;
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


public enum Procedures
{
    Procedure0,
    Procedure1,
    Procedure2,
    Procedure3,
    Procedure4,
    Procedure5,
    Procedure6, 
    Procedure7,
}