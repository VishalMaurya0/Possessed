using UnityEngine;

public class MirrorCameraRateControl : MonoBehaviour
{
    [Header("Settings")]
    public Camera mirrorCamera; // Assign your specific Mirror Camera here
    public Transform playerTransform; // Assign the Player/Ghost here
    
    [Header("Distances")]
    [Tooltip("Distance at which the camera updates every 1 second")]
    public float partialDistance = 10f; 
    [Tooltip("Distance at which the camera updates normally (Realtime)")]
    public float activeDistance = 3f;

    [Header("State (Read Only)")]
    public bool active;
    public bool partiallyActive;

    private float _timer;
    private const float RefreshRate = 1.0f; // 1 second interval

    void Start()
    {
        if (mirrorCamera == null)
        {
            mirrorCamera = GameObject.FindWithTag("Ghost").GetComponentInChildren<Camera>();
        }

        if (playerTransform == null && GameManager.Instance.serverStarted)
        {
            playerTransform = GameManager.Instance.ownerPlayer.transform;
        }
        
        mirrorCamera.enabled = false; 
    }


    float distCalcTimer = 0.34768f;
    void Update()
    {
        distCalcTimer += Time.deltaTime;
        if (distCalcTimer > 1)
        {
            CalculateDistances();
        }
        HandleRendering();
    }

    void CalculateDistances()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        active = distance < activeDistance;
        partiallyActive = distance < partialDistance && !active;
    }

    void HandleRendering()
    {
        if (active)
        {
            // STATE: ACTIVE (Close range)
            // Enable the camera component so Unity handles standard high-FPS rendering
            if (!mirrorCamera.enabled)
            {
                mirrorCamera.enabled = true;
                _timer = 0; // Reset timer for clean transition
            }
        }
        else if (partiallyActive)
        {
            // STATE: PARTIALLY ACTIVE (Mid range)
            // Disable the camera component so it doesn't auto-render
            if (mirrorCamera.enabled)
            {
                mirrorCamera.enabled = false;
            }

            // Manually render a frame every 1 second
            _timer += Time.deltaTime;
            if (_timer >= RefreshRate)
            {
                mirrorCamera.Render();
                _timer = 0f;
            }
        }
        else
        {
            // STATE: INACTIVE (Far away)
            // Ensure camera is off to save performance
            if (mirrorCamera.enabled)
            {
                mirrorCamera.enabled = false;
            }
        }
    }
}