using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; // Only include this namespace inside the Editor
#endif

public class ScriptableObjectFinder : MonoBehaviour
{
    public List<ItemDataSO> allItems = new List<ItemDataSO>();

    public static ScriptableObjectFinder Instance;

    private void Awake()
    {
        Instance = this;
    }

    // 2. This is the RUNTIME method (Works in Build)
    // It only looks at the list, it does not touch the AssetDatabase.
    public ItemDataSO FindItemSO(ItemData itemData)
    {
        foreach (var item in allItems)
        {
            // Assuming your ItemDataSO has an 'ItemType' or similar property
            if (item != null && item.itemType == itemData.itemType)
            {
                return item;
            }
        }

        Debug.LogError($"ItemDataSO not found for type: {itemData.itemType}");
        return null;
    }

    public ItemDataSO FindItemSO(int id)
    {
        foreach (var item in allItems)
        {
            if (item == null) continue;

            if ((int)item.itemType == id)
            {
                return item;
            }
        }

        Debug.LogError($"ItemDataSO not found for ID: {id}");
        return null;
    }

    // 3. This is the EDITOR method (Used to populate the list before building)
#if UNITY_EDITOR
    [ContextMenu("Load All Items From Path")] // Adds a right-click menu option to the component
    public void LoadAssetsFromPath()
    {
        allItems.Clear();

        string path = "Assets/_My Game assets/_ScriptableObjects";
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { path }); // Specific type search

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);

            if (item != null)
            {
                allItems.Add(item);
            }
        }

        Debug.Log($"Loaded {allItems.Count} items. Don't forget to Save the Scene/Prefab!");

        // Mark object as "dirty" so Unity knows to save the changes to the list
        EditorUtility.SetDirty(this);
    }
#endif
}