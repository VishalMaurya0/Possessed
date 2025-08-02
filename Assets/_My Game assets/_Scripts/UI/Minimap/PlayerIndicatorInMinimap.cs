using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIndicatorInMinimap : NetworkBehaviour
{
    public Image indicator;
    public Image indicator2;

    public GameObject CameraWhichFollow;
    public GameObject Glasses;

    private void Start()
    {
        if (IsOwner)
        {
            Glasses.SetActive(false);
        }

        CameraWhichFollow = GameObject.FindWithTag("Minimap");
        if (IsOwner)
        {
            if (indicator != null && GameDataRuntime.Instance != null)
            {
                indicator.color = GameDataRuntime.Instance.playerIndicatorColor;
                indicator2.color = indicator.color;
            }
            else
            {
                Debug.LogWarning("Indicator reference not set on prefab.");
            }
        }

        if (!IsOwner)
        {


            if (GameDataRuntime.Instance.connectedClientsData != null && GameManager.Instance.connectedClientsData != null)
            {
                int count = Mathf.Min(GameDataRuntime.Instance.connectedClientsData.Count, GameManager.Instance.connectedClientsData.Count);

                for (int i = 0; i < count; i++)
                {
                    if (GameDataRuntime.Instance.connectedClientsData[i] != null &&
                        GameManager.Instance.connectedClientsData[i] != null)
                    {
                        GameManager.Instance.connectedClientsData[i].playerIndicatorColor =
                            GameDataRuntime.Instance.connectedClientsData[i].playerIndicatorColor;
                    }
                }
            }



            indicator.color = GameDataRuntime.Instance.GetClientThroughID(GetComponent<NetworkObject>().OwnerClientId).playerIndicatorColor;
            indicator2.color = indicator.color;
        }
    }

    private void Update()
    {
        if (IsOwner && CameraWhichFollow != null)
            CameraWhichFollow.transform.position = new(transform.position.x, CameraWhichFollow.transform.position.y, transform.position.z);
    }
}
