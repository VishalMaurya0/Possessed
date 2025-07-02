using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [Header("For Choose Color Horizontal Tabs")]
    public GameObject prefabParent;
    public GameObject colorSelectButtonPrefab;
    public GameObject colorDisplay;
    public GameObject playerListParent;
    public List<Color> allColors = new();
    public List<GameObject> allColorsButtons = new();
    public int currentColor;

    private void Awake()
    {

    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        if (allColors.Count == 0)
        {
            Debug.LogError("No colors found in allColors list!");
            return;
        }

        currentColor = Random.Range(0, allColors.Count);
        GameManager.Instance.playerIndicatorColor = allColors[currentColor];
        Debug.Log($"[Start] Assigned random color index: {currentColor}, color: {allColors[currentColor]}");

        ShowColor();
        StartCoroutine(AdjustLayout());

        for (int i = 0; i < allColors.Count; i++)
        {
            GameObject colorBtn = Instantiate(colorSelectButtonPrefab, prefabParent.transform);
            allColorsButtons.Add(colorBtn);
            colorBtn.SetActive(true);
            colorBtn.GetComponent<Image>().color = allColors[i];

            Color color = allColors[i];
            int a = i;

            colorBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"[Button Clicked] Color index {a}, Color: {color}");
                SetColor(color, a);
            });
        }
    }


    private IEnumerator AdjustLayout()
    {
        yield return null; // Wait 1 frame so layout group updates

        int amount = allColors.Count;
        float width = colorSelectButtonPrefab.GetComponent<RectTransform>().sizeDelta.x;
        float padding = prefabParent.GetComponent<HorizontalLayoutGroup>().padding.left;
        float spacing = prefabParent.GetComponent<HorizontalLayoutGroup>().spacing;
        RectTransform rt = prefabParent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2((padding * 2) + (amount * (width + spacing)) - spacing, rt.sizeDelta.y);
    }
    private void SetColor(Color color, int i)
    {
        Debug.Log($"[SetColor] Setting color index {i}, Color: {color}");
        currentColor = i;
        GameManager.Instance.playerIndicatorColor = color;
        Debug.Log($"Calling NotifyServerRpc from client. IsServer: {IsServer}, IsClient: {IsClient}");
        NotifyServerRpc(i);
        ShowColor();
    }


    [ServerRpc(RequireOwnership = false)]
    private void NotifyServerRpc(int i, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[NotifyServerSeverRpc] Client {clientId} selected color index {i}");

        if (!GameManager.Instance.playerIndicatorColors.ContainsKey(clientId))
        {
            GameManager.Instance.playerIndicatorColors.Add(clientId, allColors[i]);
            Debug.Log($"[NotifyServerSeverRpc] Added color {allColors[i]} for Client {clientId}");
        }
        else
        {
            GameManager.Instance.playerIndicatorColors[clientId] = allColors[i];
        }
        NotifyClients();
    }


    private void NotifyClients()
    {
        NullTheDictionaryClientRpc();
        Debug.Log(GameManager.Instance.playerIndicatorColors.Count);
        foreach (var color in GameManager.Instance.playerIndicatorColors)
        {
            NotifyClientsEachColorClientRpc(color.Key, color.Value);
        }
        UpdateVisualClientRpc();
    }

    [ClientRpc]
    private void NullTheDictionaryClientRpc()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[NullTheDictionaryClientRpc] Resetting client color dictionary.");
            GameManager.Instance.playerIndicatorColors = new Dictionary<ulong, Color>();
        }
    }

    [ClientRpc]
    private void NotifyClientsEachColorClientRpc(ulong id, Color color)
    {
        Debug.Log($"[NotifyClientsEachColorClientRpc] Receiving color for Client {id}: {color}");
        if (!GameManager.Instance.playerIndicatorColors.ContainsKey(id))
        {
            GameManager.Instance.playerIndicatorColors.Add(id, color);
        }
    }


    [ClientRpc]
    private void UpdateVisualClientRpc()
    {
        StartCoroutine(UpdateColorUIAfterDelay());
    }

    private IEnumerator UpdateColorUIAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Small delay to allow UI to populate

        var dsfali = playerListParent.GetComponentsInChildren<DummyScriptForAccessingListItem>();
        for (int i = 0; i < dsfali.Length; i++)
        {
            ulong clientId = NetworkManager.Singleton.ConnectedClientsList[i].ClientId;
            if (GameManager.Instance.playerIndicatorColors.TryGetValue(clientId, out Color c))
            {
                dsfali[i].GetComponent<Image>().color = c;
            }
        }

        GameDataRuntime.Instance.playerIndicatorColors = GameManager.Instance.playerIndicatorColors;
        GameDataRuntime.Instance.playerIndicatorColor = GameManager.Instance.playerIndicatorColor;
    }

    private void ShowColor()
    {
        colorDisplay.GetComponent<Image>().color = GameManager.Instance.playerIndicatorColor;
    }



    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NotifyServerRpc(currentColor);
        }
    }



}
