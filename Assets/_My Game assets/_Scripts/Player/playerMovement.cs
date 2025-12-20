using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
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
    public Transform playerCamera;
    public WallDetection wallDetection;
    public Animator animator;

    private Rigidbody rb;
    public Vector3 collisionNormal;
    private Vector3 movementInput;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private float verticalLookRotation = 0f;
    private float horizontalLookRotation = 0f;
    public PlayerDataSO playerData;

    void Start()
    {
        if (animator == null)       
            animator = GetComponentInChildren<Animator>();

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

        //torchLight = playerData.torchLight;

        sprintKey = playerData.sprintKey;
        crouchKey = playerData.crouchKey;
        torchToggleKey = playerData.torchToggleKey;


        wallDetection = transform.GetChild(1).GetComponent<WallDetection>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentStamina = maxStamina;

        if (torchLight != null) torchLight.enabled = isTorchOn;

        if (playerCamera != null)
            horizontalLookRotation = playerCamera.localEulerAngles.y;

        if (staminaSlider == null)
        {
            staminaSlider = GameObject.FindWithTag("StaminaUI").GetComponent<Slider>();
            staminaImage = staminaSlider.fillRect.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (GameManager.Instance.handleMovement)
        {
            HandleMovementInput();
        }
        HandleTorchToggle();
        if (GameManager.Instance.handlePlayerLookWithMouse)
        {
            HandleMouseMovement();

            // --- NEW LOGIC START ---
            // If we are NOT inputting movement (Standing still)
            if (movementInput.magnitude < 0.1f)
            {
                HandleTurnInPlace(); // Call the new function

                // Set animator speed to 0 just to be safe/clean
                animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            }
            else
            {
                // Existing Animation Logic for when moving...
                float currentSpeed = 0f;
                if (movementInput.magnitude > 0.1f)
                {
                    bool isSprintingInput = Input.GetKey(sprintKey);
                    currentSpeed = isSprintingInput ? 1.0f : 0.5f;
                }
                animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
            }                                                              
        }

    }

    void FixedUpdate()
    {
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
            isCrouching = !isCrouching;
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
        Vector3 camForward = playerCamera.forward;
        Vector3 camRight = playerCamera.right;

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

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina / maxStamina * 100;
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
        animator.SetFloat("Speed", movementDirection.magnitude * (speed / sprintSpeed));

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

        playerCamera.localRotation = Quaternion.Euler(verticalLookRotation, horizontalLookRotation, 0);
    }

    private void HandleTurnInPlace()
    {
        // 1. Get the camera direction, but ignore Up/Down (flatten it)
        Vector3 camForward = playerCamera.forward;
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
