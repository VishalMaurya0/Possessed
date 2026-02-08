using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 6.0f;
    public float crouchSpeed = 1.5f;
    public float rotationSpeed = 5.0f;

    [Header("Mouse Settings")]
    public float lookSensitivity = 2.0f;
    public float maxLookAngle = 80f; // Limit looking up/down

    [Header("Turn In Place Settings")]
    public float turnInPlaceThreshold = 75f; // How far (in degrees) you can look before body turns
    public float turnInPlaceSpeed = 5.0f;    // How fast the body catches up

    [Header("Stamina Settings")]
    public float maxStamina = 10.0f;
    public float staminaRecoveryRate = 2.0f;
    public float XfasterStaminaRecoveryRate = 1.3f;
    public float staminaDepletionRate = 2.0f;
    private float currentStamina;
    private bool staminaBuildingStage = false;
    public Slider staminaSlider;
    public Image staminaImage;
    public Color normalStaminaColor;
    public Color buildingStaminaColor;

    [Header("Crouch Settings")] 
    public CapsuleCollider playerCollider;
    public float crouchHeight = 1.0f;
    public float standingHeight = 2.0f;
    public float crouchTransitionSpeed = 10f; // How fast we go up/down
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    public Vector3 standingCenter = new Vector3(0, 0.9f, 0);
    public float camCrouchHeight = 0;
    public float camNormalHeight = 1f;
    public LayerMask crouchLayerMask;

    [Header("Torch Settings")]
    public Light torchLight;
    public AudioClip torchToggleSound;
    private bool isTorchOn = true;

    [Header("Input Settings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode torchToggleKey = KeyCode.F;

    [Header("--References--")] 
    public Transform playerVisual;
    public Transform player_GhostCamera;
    public WallDetection wallDetection;
    public Animator animator;

    private Rigidbody rb;
    public Vector3 collisionNormal;
    private Vector3 movementInput;
    public bool isCrouching = false;
    private bool isSprinting = false;
    private float verticalLookRotation = 0f;
    private float horizontalLookRotation = 0f;
    public PlayerDataSO playerData;

    private NetworkVariable<Quaternion> netVisualRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    ); 
    [Header("IK Settings")]
    private NetworkVariable<Quaternion> netLookRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );
    private NetworkVariable<float> netAnimSpeed = new NetworkVariable<float>(
    0f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner
);


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            //if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            //if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
            return;
        }

        if (player_GhostCamera != null)
            horizontalLookRotation = player_GhostCamera.localEulerAngles.y;

        if (staminaSlider == null)
        {
            GameObject uiObj = GameObject.FindWithTag("StaminaUI");
            if (uiObj != null)
            {
                staminaSlider = uiObj.GetComponent<Slider>();
                if (staminaSlider != null) staminaImage = staminaSlider.fillRect.GetComponent<Image>();
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentStamina = maxStamina;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Wall detection safety check
        if (transform.childCount > 1 && wallDetection == null)
            wallDetection = transform.GetChild(1).GetComponent<WallDetection>();

        if (playerData != null) ApplyDataSO();

        lookSensitivity = GameManager.Instance.mouseSensitivity;

        //if (IsOwner)
        //{ 
        //    NetworkObject playerNetObj = playerCamera.GetComponent<NetworkObject>();
        //    if (playerNetObj != null && !playerNetObj.IsOwner)
        //    {
        //        playerNetObj.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        //    }
        //    playerNetObj.Spawn();
        //    playerNetObj.TrySetParent(transform, false);
        //}
    }

    private void ApplyDataSO()
    {
        walkSpeed = playerData.walkSpeed;
        sprintSpeed = playerData.sprintSpeed;
        crouchSpeed = playerData.crouchSpeed;
        rotationSpeed = playerData.rotationSpeed;
        lookSensitivity = playerData.lookSensitivity;
        maxLookAngle = playerData.maxLookAngle;
        maxStamina = playerData.maxStamina;
        staminaRecoveryRate = playerData.staminaRecoveryRate;
        XfasterStaminaRecoveryRate = playerData.XfasterStaminaRecoveryRate;
        staminaDepletionRate = playerData.staminaDepletionRate;
        sprintKey = playerData.sprintKey;
        crouchKey = playerData.crouchKey;
        torchToggleKey = playerData.torchToggleKey;
        crouchHeight = playerData.crouchHeight;
        standingHeight = playerData.standingHeight;
        crouchTransitionSpeed = playerData.crouchTransitionSpeed;
        crouchCenter = playerData.crouchCenter;
        standingCenter = playerData.standingCenter;
        camCrouchHeight = playerData.camCrouchHeight;
        camNormalHeight = playerData.canNormalHeight;

}

    void Update()
    {
        float currentSpeed = 0f;

        if (IsOwner)
        {
            HandleCrouch();
            if (GameManager.Instance.handleMovement)
            {
                HandleMovementInput();
            }
            HandleTorchToggle();

            if (GameManager.Instance.handlePlayerLookWithMouse)
            {
                HandleMouseMovement();

                // Turn in place logic
                if (movementInput.magnitude < 0.1f) HandleTurnInPlace();
                else
                {
                    // Animation speed logic
                    bool isSprintingInput = Input.GetKey(sprintKey);
                    currentSpeed = isSprintingInput ? 1.0f : 0.5f;
                    // (You can also set animator speed here if you want)
                }

                // SYNC 1: Send Visual Rotation to Network
                if (playerVisual != null)
                {
                    netVisualRotation.Value = playerVisual.rotation;
                }

                if (Mathf.Abs(Quaternion.Angle(netLookRotation.Value, player_GhostCamera.localRotation)) > 1f)
                {
                    netLookRotation.Value = player_GhostCamera.localRotation;
                }
            }
        }

        else // If we are NOT the owner
        {
            // Read the Network Variable and rotate the body smoothly
            if (playerVisual != null)
            {
                playerVisual.rotation = Quaternion.Slerp(
                    playerVisual.rotation,
                    netVisualRotation.Value,
                    rotationSpeed * Time.deltaTime
                );
            }

            player_GhostCamera.localRotation = netLookRotation.Value;
        }

        // --- 5. ANIMATOR SYNC ---
        if (animator != null)
        {
            if (IsOwner)
            {
                float effectiveSpeed = 0f;
                if (rb != null)
                {
                    effectiveSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
                    if (effectiveSpeed/sprintSpeed < 0.4) effectiveSpeed = 0;
                }

                netAnimSpeed.Value = effectiveSpeed;

                animator.SetFloat("Speed", effectiveSpeed * (1f / sprintSpeed), 0.1f, Time.deltaTime);
            }
            else
            {
                float networkSpeed = netAnimSpeed.Value;

                animator.SetFloat("Speed", networkSpeed * (1f / sprintSpeed), 0.1f, Time.deltaTime);
            }
        }
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float targetCamHeight = isCrouching ? camCrouchHeight : camNormalHeight;
        Vector3 targetCenter = isCrouching ? crouchCenter : standingCenter;

        float step = Time.deltaTime * crouchTransitionSpeed;

        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, step);
        playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, step);

        Vector3 currentCamPos = player_GhostCamera.localPosition;
        currentCamPos.y = Mathf.Lerp(currentCamPos.y, targetCamHeight, step);
        player_GhostCamera.localPosition = currentCamPos;

        if (Mathf.Abs(playerCollider.height - targetHeight) < 0.001f)
        {
            playerCollider.height = targetHeight;
            playerCollider.center = targetCenter;

            Vector3 finalCamPos = player_GhostCamera.localPosition;
            finalCamPos.y = targetCamHeight;
            player_GhostCamera.localPosition = finalCamPos;
        }
    }

    private bool CheckCeiling()
    {
        Vector3 origin = transform.position;
        float distance = standingHeight + 0.1f; // A bit of buffer

        bool hitCeiling = Physics.SphereCast(origin, playerCollider.radius, Vector3.up, out RaycastHit hit, distance, crouchLayerMask);

        return hitCeiling;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (GameManager.Instance.handleMovement)
        {
            MovePlayerFU();
        }
    }

    private void HandleMovementInput()
    {
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementInput = new Vector3(horizontal, 0, vertical).normalized;

        
        if (Input.GetKeyDown(crouchKey))
        {
            isCrouching = true;
        }
        
        if (Input.GetKeyUp(crouchKey))
        {
            Debug.LogError("disabling");
            if (!CheckCeiling())
            {
                isCrouching = false;
            }
        }

        if (Input.GetKeyDown(sprintKey))
        {
            isSprinting = true;
        }else
        {
            isSprinting = false;
        }
    }

    private void MovePlayerFU()
    {
        float speed = walkSpeed;
        // --- 1. MOVEMENT CALCULATION (Camera Relative) ---
        // We calculate direction based on where the CAMERA is facing, not the player.
        Vector3 camForward = player_GhostCamera.forward;
        Vector3 camRight = player_GhostCamera.right;

        // Flatten Y so looking up/down doesn't slow you down
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Calculate the direction we want to move in world space
        Vector3 movementDirection = (camForward * movementInput.z + camRight * movementInput.x).normalized;

        if (currentStamina <= 0)
        {
            staminaBuildingStage = true;
        }
        else if (currentStamina >= maxStamina) 
        {
            staminaBuildingStage = false; 
        }

        if (isCrouching)
        {
            speed = crouchSpeed;
        }
        else
        {
            if (Input.GetKey(sprintKey) && currentStamina > 0 && !staminaBuildingStage && movementInput.magnitude > 0)
            {
                speed = sprintSpeed;
                currentStamina -= staminaDepletionRate * Time.deltaTime;
            }
            else if (staminaBuildingStage)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime * XfasterStaminaRecoveryRate;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina / maxStamina * 100;
        //Debug.LogError(currentStamina);
        //Debug.LogError(staminaSlider.value);
        if (staminaBuildingStage)
        {
            staminaImage.color = buildingStaminaColor;
        }else
        {
            staminaImage.color = normalStaminaColor;
        }

            //Vector3 movement = (transform.forward * movementInput.z + transform.right * movementInput.x).normalized;

            collisionNormal = wallDetection.wallNormal;
        




        if (movementDirection != Vector3.zero)
        {
            // Check if we're colliding
            if (collisionNormal != Vector3.zero && Vector3.Dot(movementDirection, collisionNormal) < 0)
            {
                // Slide along the collision normal
                Vector3 slideDirection = Vector3.ProjectOnPlane(movementDirection, collisionNormal);
                movementDirection = slideDirection.normalized;
            }
        }


        //Debug.DrawRay(transform.position, collisionNormal * 10f, Color.green);
        // Apply movement
        rb.linearVelocity = new Vector3(movementDirection.x * speed, rb.linearVelocity.y, movementDirection.z * speed);

        // --- 4. VISUAL ROTATION (The Magic Part) ---
        // We rotate ONLY the 'playerVisual' child, not the main transform
        if (movementDirection != Vector3.zero && playerVisual != null)
        {
            // Calculate where we should look
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            // Smoothly rotate the visual model towards that direction
            playerVisual.rotation = Quaternion.Slerp(playerVisual.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // --- 5. ANIMATOR ---
        //animator.SetFloat("Speed", movementDirection.magnitude * (speed / sprintSpeed));

    }
    //private void OnCollisionStay(Collision collision)
    //{
    //    // Average all contact point normals
    //    collisionNormal = Vector3.zero;
    //    foreach (ContactPoint contact in collision.contacts)
    //    {
    //        if (contact.normal )
    //        collisionNormal += contact.normal;
    //        Debug.DrawRay(collision.contacts[0].point, collisionNormal * 10f, Color.red);
    //    }

    //    if (collision.contacts.Length > 0)
    //    {
    //        collisionNormal.Normalize();

    //        // Visualize the average normal at the first contact point
    //    }
    //}


    //// Called when collision stops
    //private void OnCollisionExit(Collision collision)
    //{
    //    collisionNormal = Vector3.zero;
    //}


    private void HandleTorchToggle()
    {
            if (!IsOwner) return;
        if (Input.GetKeyDown(torchToggleKey) && torchLight != null)
        {
            isTorchOn = !isTorchOn;
            torchLight.enabled = isTorchOn;

            // Play torch toggle sound
            if (torchToggleSound != null)
            {
                AudioSource.PlayClipAtPoint(torchToggleSound, transform.position);
            }
        }
    }

    private void HandleMouseMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        horizontalLookRotation += mouseX;

        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        player_GhostCamera.localRotation = Quaternion.Euler(verticalLookRotation, horizontalLookRotation, 0);
    }

    private void HandleTurnInPlace()
    {
        // 1. Get the camera direction, but ignore Up/Down (flatten it)
        Vector3 camForward = player_GhostCamera.forward;
        camForward.y = 0;
        camForward.Normalize();

        // 2. Calculate the angle difference between Body and Camera
        float angleDifference = Vector3.Angle(playerVisual.forward, camForward);

        // 3. If the angle is too big, start rotating the body towards the camera
        if (angleDifference > turnInPlaceThreshold)
        {
            // Calculate the target rotation (facing the same way as camera)
            Quaternion targetRotation = Quaternion.LookRotation(camForward);

            // Smoothly rotate the body towards that target
            // We use a lower speed here so it feels like a "corrective" shuffle
            playerVisual.rotation = Quaternion.Slerp(
                playerVisual.rotation,
                targetRotation,
                turnInPlaceSpeed * Time.deltaTime
            );
        }
    }

    void OnDrawGizmos()
    {
        // Visualize stamina in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + Vector3.up * 2.0f, new Vector3(currentStamina / maxStamina, 0.1f, 0.1f));
    }
}
