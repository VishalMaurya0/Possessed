using UnityEngine;

public class DisableVFXWhenNotLookedAt : MonoBehaviour
{
    public Camera playerCamera;
    public float maxDistance = 100f;
    public LayerMask lookLayer;


    private bool isVisible = false;

    void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = GameManager.Instance.playerCamera;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, lookLayer))
        {
            if (hit.transform == transform)
            {
                if (!isVisible)
                {
                    gameObject.SetActive(true);
                    isVisible = true;
                }
                return;
            }
        }

        if (isVisible)
        {
            gameObject.SetActive(false);
            isVisible = false;
        }
    }
}
