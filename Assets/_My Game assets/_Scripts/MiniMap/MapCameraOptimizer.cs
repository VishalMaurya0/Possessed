using System.Collections;
using UnityEngine;

public class MapCameraOptimizer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The camera used for the map.")]
    public Camera mapCamera;

    [Tooltip("How long to wait before the FIRST render (let the scene load first).")]
    public float startDelay = 1.0f;

    [Tooltip("If greater than 0, the map will update every X seconds. If 0, it renders ONCE and stops.")]
    public float refreshInterval = 0f; // Set to 0.5f or 1.0f for a refreshing mini-map

    private void Start()
    {
        // if (mapCamera == null) mapCamera = GetComponent<Camera>();

        // IMPORTANT: Disable the camera component. 
        // This stops Unity from calling RenderLoop every frame automatically.
        mapCamera.enabled = false;

        // Start our manual control routine
        StartCoroutine(RenderRoutine());
    }

    private IEnumerator RenderRoutine()
    {
        // 1. Wait for the game to initialize (or the specific wait time you wanted)
        yield return new WaitForSeconds(startDelay);

        // 2. Render immediately after the wait
        RenderMap();

        // 3. If a refresh interval is set, enter a loop
        if (refreshInterval > 0)
        {
            WaitForSeconds wait = new WaitForSeconds(refreshInterval);
            while (true)
            {
                yield return wait;
                RenderMap();
            }
        }
    }

    private void RenderMap()
    {
        // Manually tell the camera to take ONE picture right now
        mapCamera.Render();
        
        // Optional: If you are using a RenderTexture, you don't need to do anything else.
        // The image is now updated.
    }
}