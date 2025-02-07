using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public class ItemDataSO : ScriptableObject, IIdentifiable
{
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    public ItemType itemType;
    public bool isContainer;
    public int noOfStates;
    public string[] states;
    public GameObject[] statesPrefab;
    public int currentState;
    public bool isStackable;
    public int maxStackSize;

    public float inventorySlots = 1;
    public float weight;
    ItemType IIdentifiable.ItemType => itemType;
}

public interface IIdentifiable
{
    ItemType ItemType { get; }
}
