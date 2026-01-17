#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq; // Needed for ElementAtOrDefault
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[CustomEditor(typeof(AudioSO))]
public class SoundsSOEditor : Editor
{
    private void OnEnable()
    {
        AudioSO scriptableObject = (AudioSO)target;
        List<SoundFXList> currentList = scriptableObject.allSounds;

        if (currentList == null)
            return;

        string[] enumNames = Enum.GetNames(typeof(AudioType));

        // 1. SAFE DICTIONARY CREATION
        // We use a dictionary to save existing data. We check ContainsKey to prevent crashes from duplicates.
        Dictionary<string, SoundFXList> savedData = new Dictionary<string, SoundFXList>();

        for (int i = 0; i < currentList.Count; i++)
        {
            if (currentList[i] != null && !string.IsNullOrEmpty(currentList[i].Name))
            {
                if (!savedData.ContainsKey(currentList[i].Name))
                {
                    savedData.Add(currentList[i].Name, currentList[i]);
                }
            }
        }

        // 2. DETECT CHANGES
        // We compare the list size OR if the names match the enum order exactly.
        bool requiresUpdate = currentList.Count != enumNames.Length;

        // Even if sizes match, check if names match (handles Reordering or Renaming)
        if (!requiresUpdate)
        {
            for (int i = 0; i < enumNames.Length; i++)
            {
                if (currentList[i].Name != enumNames[i])
                {
                    requiresUpdate = true;
                    break;
                }
            }
        }

        // 3. REBUILD LIST IF NEEDED
        if (requiresUpdate)
        {
            List<SoundFXList> newList = new List<SoundFXList>();

            for (int i = 0; i < enumNames.Length; i++)
            {
                string enumName = enumNames[i];
                SoundFXList itemToAdd;

                // A. Try to find existing data by Name (Best for reordering/removing)
                if (savedData.ContainsKey(enumName))
                {
                    itemToAdd = savedData[enumName];
                }
                // B. Fallback: If name changed but index might be same (Optional fallback)
                else if (i < currentList.Count && currentList[i].Name == enumName)
                {
                    itemToAdd = currentList[i];
                }
                // C. Create New (New enum added)
                else
                {
                    itemToAdd = new SoundFXList();
                }

                // Ensure the internal data is synced
                itemToAdd.Name = enumName;
                itemToAdd.Type = (AudioType)i;

                // Initialize volume if it's a fresh object
                if (itemToAdd.volume == 0) itemToAdd.volume = 1f;

                newList.Add(itemToAdd);
            }

            // 4. APPLY & SAVE
            scriptableObject.allSounds = newList;
            EditorUtility.SetDirty(target); // CRITICAL: Tells Unity the data changed so it saves
        }
    }
}
#endif