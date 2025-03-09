using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform cameraTransform;

    void Update()
    {
        if (cameraTransform != null) 
        {
            transform.position = cameraTransform.position;
            transform.rotation = cameraTransform.rotation;
        }
    }
}
