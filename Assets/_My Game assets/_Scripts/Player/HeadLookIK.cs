using Unity.Netcode;
using UnityEngine;

public class HeadLookIK : NetworkBehaviour
{
    public Transform targetCamera;
    public PlayerController playerController;
    public float lookWeight = 1.0f;
    private Animator anim;

    public Vector3 crouchPos;
    public Vector3 NormalPos;

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

    private void Update()
    {
        Vector3 target = playerController.isCrouching ? crouchPos : NormalPos;

        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 5f);
    }
}