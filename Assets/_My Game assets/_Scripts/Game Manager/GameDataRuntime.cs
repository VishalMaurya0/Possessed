using System.Collections.Generic;
using UnityEngine;

public class GameDataRuntime : MonoBehaviour
{
    public Color playerIndicatorColor;
    public List<ConnectedClientsData> connectedClientsData = new();

    [Header("Main Menu (Host Accessible)")]
    public bool useRandomSeed = true;
    public int seed;

    public static GameDataRuntime Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        useRandomSeed = true;

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public ConnectedClientsData GetClientThroughID(ulong clientId)
    {
        for (int i = 0; i < connectedClientsData.Count; i++)
        {
            if (clientId == connectedClientsData[i].clientID)
                return connectedClientsData[i];
        }
        return null;
    }
}
