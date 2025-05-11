using Unity.Netcode;
using UnityEngine;

public class ChestUnlock_BloodBottleTask : NetworkBehaviour
{
    public Tasks task;
    [SerializeField] GameObject spawnedObject;
    [SerializeField] StatueTask[] statues = new StatueTask[4];
    public int[] savedCode = new int[4];
    public int[] currentCode = new int[4];
    bool randomized = false;


    CodeShowingScript codeShowingScript;

    void Start()
    {
        codeShowingScript = GetComponent<CodeShowingScript>();
        if (GameManager.Instance.serverStarted)
        {
            GameManager.Instance.taskManager.TasksGameobjcts.Add(new System.Collections.Generic.KeyValuePair<Tasks, GameObject>(task, this.gameObject));
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsServer) { return; }
        if (!randomized)
        {
            InitializeCode();
            randomized = true;
        }
    }

    private void InitializeCode()
    {
        for (int i = 0; i < savedCode.Length; i++)
        {
            savedCode[i] = Random.Range(0, 4);
        }
        codeShowingScript.SetText();
    }


    public void UpdateCurrentCode()
    {
        for (int i = 0; i < currentCode.Length; i++)
        {
            currentCode[i] = statues[i].value.Value;
        }
        CheckForCorrectCode();
    }

    private void CheckForCorrectCode()
    {
        for (int i = 0; i < currentCode.Length; i++)
        {
            if (currentCode[i] != savedCode[i])
            {
                return;
            }
        }
        //TODO TODO Unlock the chest here TODO TODO //
        Debug.Log
            
            
            
            ("Chest Unlock");

        GameObject spawned = Instantiate(spawnedObject, this.transform.position, Quaternion.identity);
        NetworkObject obj = spawned.GetComponent<NetworkObject>();
        if (obj != null)
        {
            obj.Spawn();    
        }
    }
}
