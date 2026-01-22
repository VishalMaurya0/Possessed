using Unity.Netcode;
using UnityEngine;

public class HeadLookIK : NetworkBehaviour
{
    //public Transform targetCamera;
    public PlayerController playerController;
    public float lookWeight = 1.0f;
    private Animator anim;
    public AudioSource AudioSource;

    public Vector3 crouchPos;
    public Vector3 NormalPos;

    [Header("Setup")]
    public Transform headBone; // DRAG YOUR HEAD BONE HERE MANUALLY
    public Transform targetCamera;

    [Header("Settings")]
    public float lookSpeed = 5f;
    public Vector3 offsetRotation; // Adjust if head is facing wrong way (e.g. 90, 0, 0)

    // SYNC: Only the owner writes, everyone reads
    private Vector3 networkLookPos = Vector3.zero;

    // STORED MEMORY (This saves us from the Animator)
    private Vector3 currentLookTarget;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Owner updates the sync variable
        if (targetCamera != null)
        {
            networkLookPos = targetCamera.position + (targetCamera.forward * 10f);
        }
    }

    // IMPORTANT: Use LateUpdate so we override the animation
    void LateUpdate()
    {
        Vector3 rawNetworkTarget = networkLookPos;
        if (rawNetworkTarget == Vector3.zero) return;

        // 1. SMOOTH THE TARGET (Not the Bone)
        // We move our "invisible target" slowly towards the real network target
        currentLookTarget = Vector3.Lerp(currentLookTarget, rawNetworkTarget, Time.deltaTime * lookSpeed);

        // 2. CALCULATE ROTATION
        Vector3 direction = currentLookTarget - headBone.position;
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 3. ADD OFFSET
        targetRotation *= Quaternion.Euler(offsetRotation);

        // 4. HARD SET (No Slerp here!)
        // We overwrite the Animator completely for this frame.
        headBone.rotation = targetRotation;
    }

    public void PlayFootstepSound()
    {
        AudioManager.PlaySound(AudioType.Walk, AudioSource);
    }

    //void OnAnimatorIK(int layerIndex)
    //{
    //    //if (!IsOwner)
    //    //{
    //    //    return;
    //    //}
    //    if (anim && targetCamera)
    //    {
    //        anim.SetLookAtWeight(lookWeight, 0.3f, 0.8f, 1.0f);
            
    //        anim.SetLookAtPosition(targetCamera.position + targetCamera.forward * 10f);
    //    }

    //}

    //private void Update()
    //{
    //    Vector3 target = playerController.isCrouching ? crouchPos : NormalPos;

    //    transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * 5f);
    //}
}