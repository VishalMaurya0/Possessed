using System;
using Unity.Netcode;
using UnityEngine;

public class TaskVoodooDoll : NetworkBehaviour
{
    public Tasks task;
    [SerializeField] GameObject voodooDollPrefab;
    [SerializeField] GameObject newVoodooDoll;

    [SerializeField] int dollsNeeded = 3;
    public int dollsAdded = 0;
    FireScriptForVoodooDoll[] fireScriptForVoodooDolls = new FireScriptForVoodooDoll[3];

    void Start()
    {
        for (int i = 0; i < fireScriptForVoodooDolls.Length; i++)
        {
            fireScriptForVoodooDolls[i] = transform.GetChild(i).GetComponent<FireScriptForVoodooDoll>();
        }
        if (GameManager.Instance.serverStarted)
        {
            GameManager.Instance.taskManager.TasksGameobjcts.Add(new System.Collections.Generic.KeyValuePair<Tasks, GameObject>(task, this.gameObject));
        }
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
        foreach (var script in fireScriptForVoodooDolls)
        {
            script.activated = true;
            script.fire.Play();
        }
    }
}
