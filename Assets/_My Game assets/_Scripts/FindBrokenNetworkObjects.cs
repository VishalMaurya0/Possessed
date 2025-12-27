using UnityEngine;
using Unity.Netcode.Components;

[ExecuteInEditMode]
public class FindBadTransforms : MonoBehaviour
{
    void Start()
    {
        // Find every single NetworkTransform in the scene
        NetworkTransform[] allNetTransforms = FindObjectsOfType<NetworkTransform>(true);
        
        Debug.Log($"🔎 Found {allNetTransforms.Length} NetworkTransform components in the scene.");

        foreach (var netTransform in allNetTransforms)
        {
            GameObject obj = netTransform.gameObject;
            string status = "✅ OK";
            
            // Check 1: Is it Static? (NetworkTransform crashes on static objects)
            if (obj.isStatic)
            {
                Debug.LogError($"❌ CRITICAL ERROR: '{obj.name}' is marked STATIC but has a NetworkTransform! Uncheck 'Static' or remove the component.");
                continue;
            }

            // Check 2: Is it a Manager? (Managers usually shouldn't have this)
            if (obj.GetComponent<MapVisual>() != null || obj.name.Contains("Manager"))
            {
                 Debug.LogWarning($"⚠️ SUSPICIOUS: '{obj.name}' is a Manager but has a NetworkTransform. Do you really need to sync its position?");
            }

            Debug.Log($"{status}: {obj.name}");
        }
    }
}