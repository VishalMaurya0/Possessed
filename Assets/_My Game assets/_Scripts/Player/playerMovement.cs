using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("Stamina Settings")]
    public float maxStamina = 10.0f;
    public float staminaRecoveryRate = 2.0f;
    public float XfasterStaminaRecoveryRate = 1.3f;
    public float staminaDepletionRate = 2.0f;
    private float currentStamina;
    private bool staminaBuildingStage = false;

    [Header("Torch Settings")]
    public Light torchLight;
    public AudioClip torchToggleSound;
    private bool isTorchOn = true;

    [Header("Input Settings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode torchToggleKey = KeyCode.F;

    [Header("--References--")]
    public Transform playerCamera;


    private Rigidbody rb;
    public Vector3 collisionNormal;
    private Vector3 movementInput;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private float verticalLookRotation = 0f;
    public PlayerDataSO playerData;

    void Start()
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

        torchLight = playerData.torchLight;

        sprintKey = playerData.sprintKey;
        crouchKey = playerData.crouchKey;
        torchToggleKey = playerData.torchToggleKey;


    rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentStamina = maxStamina;

        if (torchLight != null) torchLight.enabled = isTorchOn;
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

        Vector3 movement = (transform.forward * movementInput.z + transform.right * movementInput.x).normalized;

        if (movement != Vector3.zero)
        {
            // Check if we're colliding
            if (collisionNormal != Vector3.zero && Vector3.Dot(movement, collisionNormal) < 0)
            {
                // Slide along the collision normal
                Vector3 slideDirection = Vector3.ProjectOnPlane(movement, collisionNormal);
                movement = slideDirection.normalized;
            }
        }

        // Apply movement

        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);
    }
    private void OnCollisionStay(Collision collision)
    {
        // Average all contact points' normals
        collisionNormal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            collisionNormal += contact.normal;
        }
        collisionNormal.Normalize();
    }

    // Called when collision stops
    private void OnCollisionExit(Collision collision)
    {
        collisionNormal = Vector3.zero;
    }


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
        transform.Rotate(Vector3.up * mouseX);
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        playerCamera.localRotation = Quaternion.Euler(verticalLookRotation, playerCamera.localRotation.y, playerCamera.localRotation.z);
    }

    void OnDrawGizmos()
    {
        // Visualize stamina in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + Vector3.up * 2.0f, new Vector3(currentStamina / maxStamina, 0.1f, 0.1f));
    }
}
