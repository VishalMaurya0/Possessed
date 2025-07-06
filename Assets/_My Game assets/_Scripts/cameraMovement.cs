using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform cameraTransform;

    void FixedUpdate()
    {
        if (cameraTransform != null) 
        {
            transform.position = cameraTransform.position;
            transform.rotation = cameraTransform.rotation;
        }
    }
}
