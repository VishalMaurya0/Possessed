using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TaskManager : NetworkBehaviour
{
    [SerializeField] public List<TaskEntry> AllTasks = new ();

    private void Update()
    {
        if (GameManager.Instance.serverStarted && GameManager.Instance.taskManager == null)
        {
            GameManager.Instance.taskManager = this;
        }
    }
}

public enum TasksEnum
{
    VoodooDollTask,
    BloodBottleTask,
    CursedCoinTask,
    PurePowderTask,
    CandleTask,
}



[Serializable]
public class TaskEntry
{
    public TasksEnum taskType;
    public GameObject taskPrefab;
}