using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ScriptableObjectFinder : MonoBehaviour
{
    
    public static List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();

    [MenuItem("Tools/Find ScriptableObjects in Path")]
    public static ScriptableObject[] FindScriptableObjectsInPath()
    {
        // Specify the path you want to search within Assets
        string path = "Assets/_My Game assets/_ScriptableObjects";

        // Get all assets in the specified path
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { path });

        // Loop through all found assets
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (obj != null)
            {
                scriptableObjects.Add(obj);
            }
        }
        Debug.Log($"Found {scriptableObjects.Count} ScriptableObjects in path.");

        return scriptableObjects.ToArray();

    }

    public static ItemDataSO FindItemSO(ItemData itemData)
    {
        if (scriptableObjects.Count != 0)
        {
            foreach (ScriptableObject obj in scriptableObjects)
            {
                if (obj is IIdentifiable identifiable && identifiable.ItemType == itemData.itemType)
                {
                    return obj as ItemDataSO;
                }
            }
        }
        ScriptableObject[] scriptableObjectss = FindScriptableObjectsInPath();
        foreach(ScriptableObject obj in scriptableObjectss)
        {
            if (obj is IIdentifiable identifiable && identifiable.ItemType == itemData.itemType)
            {
                return obj as ItemDataSO;
            }
        }
        return null;
    }
}
