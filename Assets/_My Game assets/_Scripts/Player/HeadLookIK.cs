using Unity.Netcode;
using UnityEngine;

public class HeadLookIK : NetworkBehaviour
{
    public Transform targetCamera;
    public float lookWeight = 1.0f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        //if (!IsOwner)
        //{
        //    return;
        //}
        if (anim && targetCamera)
        {
            // Set the weight (1 = fully look, 0 = don't look)
            anim.SetLookAtWeight(lookWeight, 0.3f, 0.8f, 1.0f);
            
            // Look 10 units in front of the camera
            anim.SetLookAtPosition(targetCamera.position + targetCamera.forward * 10f);
        }
    }
}