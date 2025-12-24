using System;
using UnityEngine;
using UnityEngine.Events;

public class LogListener : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The specific error text to listen for from the internal widget.")]
    public string errorKeyword = "Lobby not found";

    //[Header("Actions")]
    public event Action OnJoinFailed;

    private void OnEnable()
    {
        // Subscribe to Unity's global log system
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // We only care about Errors or Exceptions
        if (type == LogType.Error || type == LogType.Exception)
        {
            // Check if the log contains the keyword related to the Join failure
            // You might need to check your Console to see exactly what the Widget prints
            if (logString.Contains("SessionNotFound") || 
                logString.Contains("LobbyNotFound") || 
                logString.Contains("SessionException") || 
                logString.Contains("lobby not found") || 
                logString.Contains(errorKeyword)) 
            {
                Debug.Log("Interceptor caught the internal error! Running custom function...");
                
                // Trigger your custom function
                OnJoinFailed?.Invoke();
            }
        }
    }
}