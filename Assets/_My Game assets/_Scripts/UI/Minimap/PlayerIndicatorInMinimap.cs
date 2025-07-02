using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIndicatorInMinimap : NetworkBehaviour
{
    public Image indicator;

    private void Start()
    {
        if (IsOwner)
        {
            indicator.color = GameDataRuntime.Instance.playerIndicatorColor;
        }

        if (!IsOwner)
        {
            indicator.color = GameDataRuntime.Instance.playerIndicatorColors[GetComponent<NetworkObject>().OwnerClientId];
        }
    }
}
