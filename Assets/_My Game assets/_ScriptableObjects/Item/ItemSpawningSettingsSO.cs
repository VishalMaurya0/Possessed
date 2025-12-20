using UnityEngine;

[CreateAssetMenu(fileName = "ItemSpawningSettingsSO", menuName = "Scriptable Objects/ItemSpawningSettingsSO")]
public class ItemSpawningSettingsSO : ScriptableObject
{
    public List<ItemSpawningData> itemSpawningDataList = new List<ItemSpawningData>();
}


public class ItemSpawningData
{
    public ItemData itemData;
    public bool inRoomOnly = false;
    public int minAmountToSpawn = 1;
    public int maxAmountToSpawn = 1;
    [HideInInspector] public int amountToSpawn;
}