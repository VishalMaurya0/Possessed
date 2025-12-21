using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSpawningSettingsSO", menuName = "Scriptable Objects/ItemSpawningSettingsSO")]
public class ItemSpawningSettingsSO : ScriptableObject
{
    public List<ItemSpawningData_inCell> itemSpawningDataInCellList = new List<ItemSpawningData_inCell>();
    public List<ItemSpawningData_inProps> itemSpawningDataInPropsList = new List<ItemSpawningData_inProps>();
}

[System.Serializable]
public class ItemSpawningData_inCell
{
    public ItemData itemData;
    public bool inRoomOnly = false;
    public int minAmountToSpawn = 1;
    public int maxAmountToSpawn = 1;
    [HideInInspector] public int amountToSpawn;
}

[System.Serializable]
public class ItemSpawningData_inProps
{
    public ItemData itemData;
    public int propID;
    public int minAmountToSpawn = 1;
    public int maxAmountToSpawn = 1;
    [HideInInspector] public int amountToSpawn;
}

