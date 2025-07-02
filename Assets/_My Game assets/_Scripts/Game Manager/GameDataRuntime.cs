using System.Collections.Generic;
using UnityEngine;

public class GameDataRuntime : MonoBehaviour
{
    public Color playerIndicatorColor;
    public Dictionary<ulong, Color> playerIndicatorColors = new();


    public static GameDataRuntime Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
