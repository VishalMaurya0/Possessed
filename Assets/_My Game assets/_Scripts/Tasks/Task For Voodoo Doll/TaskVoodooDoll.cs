using System;
using Unity.Netcode;
using UnityEngine;

public class TaskVoodooDoll : NetworkBehaviour
{
    [SerializeField] GameObject voodooDollPrefab;
    [SerializeField] GameObject newVoodooDoll;

    [SerializeField] int dollsNeeded = 3;
    public int dollsAdded = 0;

    void Start()
    {
        
    }


    void Update()
    {
        if (dollsAdded >= dollsNeeded && IsServer)
        {
            SpawnNewDoll();
        }
    }

    private void SpawnNewDoll()
    {
        newVoodooDoll = Instantiate(voodooDollPrefab, this.transform.position, Quaternion.identity, this.transform);
        NetworkObject obj = newVoodooDoll.GetComponent<NetworkObject>();
        obj.Spawn();
        dollsAdded = 0;
    }
}
