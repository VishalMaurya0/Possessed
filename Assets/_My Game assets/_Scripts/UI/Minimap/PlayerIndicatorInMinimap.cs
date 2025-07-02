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
        CameraWhichFollow = GameObject.FindWithTag("Minimap");
    }

    private void Update()
    {
        if (IsOwner && CameraWhichFollow != null)
            CameraWhichFollow.transform.position = new(transform.position.x, CameraWhichFollow.transform.position.y, transform.position.z);
    }
}
