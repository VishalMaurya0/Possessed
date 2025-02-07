using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcedureData", menuName = "Scriptable Objects/ProcedureData")]
public class ProcedureData : ScriptableObject
{
    public int procedureIndex;
    public string procedureName;
    public List<ItemNeeded> itemsNeeded;
}
