using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SafePointManager : NetworkBehaviour {
    
    public int totalSafePoints;
    public NetworkVariable <int> activatedSafePoints = new NetworkVariable<int> (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public GameObject safePointPefab;
    public List<SafePointArea> safePointAreas = new List<SafePointArea>();

    [Header("Visual")]
    public TMP_Text safePointCounterText;
    public TMP_Text totalSafePointText;

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (activatedSafePoints.Value < totalSafePoints)
            {
                ActivateSafePointServerRpc();
                AudioManager.PlaySound(AudioType.Correct);
            }else
            {
                GameManager.Instance.HelpInstructions.text = "All Safe Points Activated";
                GameManager.Instance.helpInstructionDisplayTime = 3f;
                AudioManager.PlaySound(AudioType.Wrong);
            }

            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        safePointCounterText.text = activatedSafePoints.Value.ToString();
        totalSafePointText.text = totalSafePoints.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateSafePointServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (activatedSafePoints.Value < totalSafePoints)
        {
            activatedSafePoints.Value += 1;
            ulong id = serverRpcParams.Receive.SenderClientId;
            Vector3 pos = GameManager.Instance.GetClientThroughID(id).playerGameobject.transform.position;
            pos.y = 0f;
            GameObject obj = Instantiate(safePointPefab, pos, Quaternion.identity);
            obj.GetComponent<NetworkObject>().Spawn();
            safePointAreas.Add(obj.GetComponentInChildren<SafePointArea>());
        }
    }
}