using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCraftingDataSO", menuName = "Scriptable Objects/ItemCraftingDataSO")]
public class ItemCraftingDataSO : ScriptableObject
{
    public List<ItemStateCraftingRecipe> itemStateCraftingRecipes;
    
}

[System.Serializable]
public struct ItemStateCraftingRecipe
{
    public int id;
    public string name;
    public ItemState ItemState1;
    public ItemState ItemState2;
    public ItemState CraftedItemState;
}

[System.Serializable]
public class ItemState
{
    public ItemType itemType;
    public bool isContainer;
    public int currentState;
    public int amount = 1;

    public ItemState(ItemData itemdata, int amount)
    {
        ItemDataSO idso = ScriptableObjectFinder.FindItemSO(itemdata);
        if (amount > 0)
        {
            itemType = itemdata.itemType;
            isContainer = idso.isContainer;
            currentState = itemdata.currentState;
            this.amount = amount;
        }
    }
}