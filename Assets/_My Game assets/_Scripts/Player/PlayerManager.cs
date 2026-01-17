using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public GameObject playerVisibleGameobject;

    private void Start()
    {
        if (IsOwner)
        {
            if (playerVisibleGameobject == null)
                playerVisibleGameobject = transform.Find("BrianJohnson").gameObject;


            if (playerVisibleGameobject != null)
            {
                playerVisibleGameobject.SetActive(false);
            }
        }
    }
}
