using System;
using Unity.Netcode;
using UnityEngine;

public class TaskVoodooDoll : NetworkBehaviour
{
    public TasksEnum task;
    [SerializeField] GameObject voodooDollPrefab;
    [SerializeField] GameObject newVoodooDoll;

    [SerializeField] int dollsNeeded = 3;
    public int dollsAdded = 0;
    FireScriptForVoodooDoll[] fireScriptForVoodooDolls = new FireScriptForVoodooDoll[3];

    public int dollsInMap = 0;
    public int dollsToSpawnInMap = 17;
    public GameObject enemiesContainer;

    void Start()
    {
        for (int i = 0; i < fireScriptForVoodooDolls.Length; i++)
        {
            fireScriptForVoodooDolls[i] = transform.GetChild(i).GetComponent<FireScriptForVoodooDoll>();
        }
    }



    void Update()
    {
        if (dollsAdded >= dollsNeeded && IsServer)
        {
            SpawnNewDoll();
        }

        if (dollsInMap < dollsToSpawnInMap && IsServer)
        {
            fireScriptForVoodooDolls[0].SpawnADoll();
        }
    }

    private void SpawnNewDoll()
    {
        AudioManager.PlaySound(AudioType.Correct);
        newVoodooDoll = Instantiate(voodooDollPrefab, this.transform.position, Quaternion.identity, this.transform);
        NetworkObject obj = newVoodooDoll.GetComponent<NetworkObject>();
        obj.Spawn();
        dollsAdded = 0;
        foreach (var script in fireScriptForVoodooDolls)
        {
            script.activated = true;
            script.fire.Play();
            script.fireSound.Play();
        }
    }
}
