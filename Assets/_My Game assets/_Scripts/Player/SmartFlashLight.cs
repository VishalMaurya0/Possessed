using UnityEngine;

public class SmartFlashlight : MonoBehaviour
{
    [Header("Settings")]
    public Light flashlight;

    [Tooltip("Brightness when you are very close (inside the Min Distance).")]
    public float minIntensity = 0.5f;

    [Tooltip("Brightness when you are far away (outside the Max Distance).")]
    public float maxIntensity = 2.0f;

    [Header("Distance Thresholds")]
    [Tooltip("If closer than this (e.g. 1 meter), light stays at Min Intensity.")]
    public float minDistance = 1.0f;

    [Tooltip("If further than this (e.g. 4 meters), light is at Max Intensity.")]
    public float maxDistance = 4.0f;

    [Tooltip("How fast the light adjusts.")]
    public float smoothSpeed = 10f;

    [Header("Layer Mask")]
    public LayerMask obstructionMask;

    void Start()
    {
        if (flashlight == null) flashlight = GetComponent<Light>();
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

        // Cast ray only as far as the max distance
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, obstructionMask))
        {
            // If we are closer than the minimum distance, just use min intensity
            if (hit.distance <= minDistance)
            {
                targetIntensity = minIntensity;
            }
            else
            {
                // We are between minDistance and maxDistance.
                // Calculate where we are in that range (0.0 to 1.0)
                // Mathf.InverseLerp takes (min, max, value) and returns percentage
                float rangePercent = Mathf.InverseLerp(minDistance, maxDistance, hit.distance);

                targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, rangePercent);
            }
        }
        else
        {
            // Hitting nothing within range -> Max Brightness
            targetIntensity = maxIntensity;
        }

        // Apply smooth transition
        flashlight.intensity = Mathf.Lerp(flashlight.intensity, targetIntensity, Time.deltaTime * smoothSpeed);
    }
}