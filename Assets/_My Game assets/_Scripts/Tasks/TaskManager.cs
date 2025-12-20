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

    public GameObject GetTask(TasksEnum taskType)
    {
        foreach (var task in AllTasks)
        {
            if (task.taskType == taskType)
            {
                return task.taskPrefab;
            }
        }
        return null;
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