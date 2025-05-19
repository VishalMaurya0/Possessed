using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public List<KeyValuePair<TasksEnum, GameObject>> TasksGameobjcts = new ();

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