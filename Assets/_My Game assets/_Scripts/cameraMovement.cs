using Unity.VisualScripting;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{

    public Transform cameraTransform;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = cameraTransform.position;
        this.transform.rotation = cameraTransform.rotation;
    }
}
