using UnityEngine;

public class SmartFlashlight : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Assign your Spotlight here. If empty, it grabs the component on this object.")]
    public Light flashlight;

    [Tooltip("The normal brightness when looking at open space.")]
    public float maxIntensity = 2.0f;

    [Tooltip("The lowest brightness when touching a wall.")]
    public float minIntensity = 0.5f;

    [Tooltip("At what distance does it start dimming?")]
    public float dimDistance = 2.0f;

    [Tooltip("How fast the light adjusts (higher = snappier, lower = smoother).")]
    public float smoothSpeed = 10f;

    [Header("Layer Mask")]
    [Tooltip("What layers should the light react to? (Uncheck 'Player' and 'Triggers')")]
    public LayerMask obstructionMask;

    void Start()
    {
        // Auto-assign if not set manually
        if (flashlight == null) flashlight = GetComponent<Light>();
        
        // Ensure mask is set to 'Default' if the user forgot to set it
        if (obstructionMask == 0) obstructionMask = LayerMask.GetMask("Default");
    }

    void Update()
    {
        AdjustBrightness();
    }

    void AdjustBrightness()
    {
        float targetIntensity = maxIntensity;

        RaycastHit hit;
        // Shoot a ray forward from the flashlight's position
        if (Physics.Raycast(transform.position, transform.forward, out hit, dimDistance, obstructionMask))
        {
            // Calculate a value between 0 and 1 based on distance
            // 0 = touching wall, 1 = at full dimDistance
            float distanceFactor = Mathf.Clamp01(hit.distance / dimDistance);

            // Interpolate intensity based on that factor
            targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, distanceFactor);
        }

        // Smoothly move current intensity towards the target intensity
        flashlight.intensity = Mathf.Lerp(flashlight.intensity, targetIntensity, Time.deltaTime * smoothSpeed);
    }
}