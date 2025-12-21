using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawningInPropsManager : MonoBehaviour
{
    public static ItemSpawningInPropsManager instance;
    public ItemSpawningSettingsSO itemSpawningSettingsSO;
    public List<ItemSpawningInPropsManagerRuntimeData> itemSpawningInPropsManagerRuntimeDatas = new ();

    public GameObject itemsContainer;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else  if (instance != this) {
            Destroy(this);
        }
    }

    public void MakeItemSpawningInPropsManagerRuntimeData(PropID propID)
    {
        ItemSpawningInPropsManagerRuntimeData runtimeDataPropID = null;
        while (itemSpawningInPropsManagerRuntimeDatas.Count <= propID.propID)
        {
            runtimeDataPropID = new ItemSpawningInPropsManagerRuntimeData();
            itemSpawningInPropsManagerRuntimeDatas.Add(runtimeDataPropID);
        }

        runtimeDataPropID = itemSpawningInPropsManagerRuntimeDatas[propID.propID];

        if (runtimeDataPropID != null)
        {
            runtimeDataPropID.propID = propID.propID;
            runtimeDataPropID.propIDScriptList.Add(propID);
        }
    }


    public void StartItemSpawn()
    {
        if (itemSpawningSettingsSO == null) return;
        if (itemSpawningInPropsManagerRuntimeDatas.Count <= 0)
        {
            Debug.LogError("PropItems did not spawned, first add itemDataRuntime");
            return;
        }

        for (int i = 0; i < itemSpawningSettingsSO.itemSpawningDataInPropsList.Count; i++)
        {
            ItemSpawningData_inProps itemSpawningDataInProps_1 = itemSpawningSettingsSO.itemSpawningDataInPropsList[i];
            if (itemSpawningDataInProps_1 == null) continue;

            itemSpawningDataInProps_1.amountToSpawn = Random.Range(itemSpawningDataInProps_1.minAmountToSpawn, itemSpawningDataInProps_1.maxAmountToSpawn + 1);

            ItemSpawningInPropsManagerRuntimeData itemSpawningInPropsManagerRuntimeData = itemSpawningInPropsManagerRuntimeDatas[itemSpawningDataInProps_1.propID];
            List<PropID> propIDScriptList = itemSpawningInPropsManagerRuntimeData.propIDScriptList;
            int propID = itemSpawningInPropsManagerRuntimeData.propID;

            int amountToSpawn = itemSpawningDataInProps_1.amountToSpawn;

            int safetyCounter = 1000;
            while (amountToSpawn > 0 && safetyCounter --> 1000)
            {
                if (propIDScriptList.Count <= 0) break;

                int randomIndex = Random.Range(0, propIDScriptList.Count);
                PropID propIDScript = propIDScriptList[randomIndex];
                if (propIDScript.positions.Count <= 0)
                {
                    propIDScriptList.RemoveAt(randomIndex);
                    continue;
                }

                int randomPositionIndex = Random.Range(0, propIDScript.positions.Count);
                Transform position = propIDScript.positions[randomPositionIndex];
                if (position == null)
                {
                    propIDScriptList.RemoveAt(randomIndex);
                    continue;
                }

                GameObject obj = Instantiate(ScriptableObjectFinder.FindItemSO(itemSpawningDataInProps_1.itemData).itemPrefab, position.position, position.rotation);
                NetworkObject netobj = obj.GetComponent<NetworkObject>();
                netobj.Spawn();
                netobj.TrySetParent(itemsContainer.transform, false);
                obj.GetComponent<ItemPickup>().itemData = new ItemData(itemSpawningDataInProps_1.itemData);



                propIDScript.positions.RemoveAt(randomPositionIndex);
                amountToSpawn--;
            }
                
        }
    }
}

[System.Serializable]
public class ItemSpawningInPropsManagerRuntimeData
{
    public int propID;
    public List<PropID> propIDScriptList = new ();
}
