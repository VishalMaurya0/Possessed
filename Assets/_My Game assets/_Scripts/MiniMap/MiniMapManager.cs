using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using UnityEngine.UI; // Needed for sorting

public class MiniMapManager : NetworkBehaviour
{
    public static MiniMapManager Instance;

    public List<SkyObjectTrigger> allTriggers = new List<SkyObjectTrigger>();

    public bool refeDone = false;

    public GameObject TriggerPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void Update()
    {
        if (!IsSpawned) return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3 pos = GameManager.Instance.ownerPlayer.transform.position;

            RequestSpawnMarkerServerRpc(pos);
        }
    }

    #region DYNAMIC SPAWNING LOGIC

    // ----------------------------------------------------
    // DYNAMIC SPAWNING LOGIC
    // ----------------------------------------------------

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnMarkerServerRpc(Vector3 pos)
    {
        SpawnMarkerClientRpc(pos);
    }

    [ClientRpc]
    private void SpawnMarkerClientRpc(Vector3 pos)
    {
        GameObject newObj = Instantiate(TriggerPrefab, pos, Quaternion.identity);

        Image iconImage = newObj.transform.GetChild(0).GetComponent<Image>();
        if (iconImage != null && GameDataRuntime.Instance != null)
        {
            iconImage.color = GameDataRuntime.Instance.playerIndicatorColor;
        }

        SkyObjectTrigger trigger = newObj.GetComponent<SkyObjectTrigger>();
        if (trigger != null)
        {
            RegisterTrigger(trigger);
        }
    }
    #endregion

    #region Sorting GEMINI
    public override void OnNetworkSpawn()
    {
        // 1. Find all triggers in the scene
        // We convert the array to a List to make it easier to manage
        allTriggers = FindObjectsByType<SkyObjectTrigger>(FindObjectsSortMode.None).ToList();

        // 2. SORT THEM! (Crucial for Sync)
        // We must ensure the list order is identical on Server and Client.
        // Sorting by X position (then Z) is a reliable way to do this.
        allTriggers.Sort((a, b) =>
        {
            int compareX = a.transform.position.x.CompareTo(b.transform.position.x);
            if (compareX == 0)
                return a.transform.position.z.CompareTo(b.transform.position.z);
            return compareX;
        });
        refeDone = true;

        for (int i = 0; i < allTriggers.Count; i++)
        {
            allTriggers[i].id = i;
        }

    }
    #endregion

    #region TRIGGER & ACTIVATION LOGIC
    public void RegisterTrigger(SkyObjectTrigger trigger)
    {
        if (!allTriggers.Contains(trigger))
        {
            allTriggers.Add(trigger);

            trigger.id = allTriggers.Count - 1;

        }
        else
        {
            trigger.id = allTriggers.IndexOf(trigger);
        }
    }

    public void RequestActivateSkyObject(int id)
    {
        if (IsServer)
        {
            ActivateSkyObjectClientRpc(id);
        }
        else
        {
            ActivateSkyObjectServerRpc(id);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateSkyObjectServerRpc(int id)
    {
        ActivateSkyObjectClientRpc(id);
    }

    [ClientRpc]
    private void ActivateSkyObjectClientRpc(int id)
    {
        if (id >= 0 && id < allTriggers.Count)
        {
            allTriggers[id].ActivateVisuals();
        }
    }

    #endregion
}