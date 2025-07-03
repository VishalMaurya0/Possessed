using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIndicatorInMinimap : NetworkBehaviour
{
    public Image indicator;
    public Image indicator2;

    public GameObject CameraWhichFollow;

    private void Start()
    {
        CameraWhichFollow = GameObject.FindWithTag("Minimap");
        if (IsOwner)
        {
            indicator.color = GameDataRuntime.Instance.playerIndicatorColor;
            indicator2.color = indicator.color;
        }

        if (!IsOwner)
        {
            indicator.color = GameDataRuntime.Instance.playerIndicatorColors[GetComponent<NetworkObject>().OwnerClientId];
            indicator2.color = indicator.color;
        }
    }

    private void Update()
    {
        if (IsOwner && CameraWhichFollow != null)
            CameraWhichFollow.transform.position = new(transform.position.x, CameraWhichFollow.transform.position.y, transform.position.z);
    }
}
