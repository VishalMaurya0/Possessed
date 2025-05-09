using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public List<KeyValuePair<Tasks, GameObject>> TasksGameobjcts = new ();

    private void Update()
    {
        if (GameManager.Instance.serverStarted)
        {
            GameManager.Instance.taskManager = this;
        }
    }
}

public enum Tasks
{
    VoodooDollTask,
    BloodBottleTask,
}