using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class triggerProcedurePointScript : NetworkBehaviour
{

    public bool inProgress;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == GameManager.Instance.ownerPlayer && !inProgress)
        {
            GameManager.Instance.HelpInstructions.text = "Left Click To Place An Item";
        }

        if (other.gameObject == GameManager.Instance.ownerPlayer)
        {
            inProgress = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject == GameManager.Instance.ownerPlayer)
        {
            inProgress = false;
        }
    }
}
