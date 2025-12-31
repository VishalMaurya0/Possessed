using UnityEngine;
using UnityEngine.Rendering; // Required for URP

public class DisableFog : MonoBehaviour
{
    private bool wasFogEnabled;

    void OnEnable()
    {
        // Subscribe to the render loop events
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid errors
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        // Check if the camera rendering right now is THIS camera
        if (cam == GetComponent<Camera>())
        {
            // 1. Remember the global state
            wasFogEnabled = RenderSettings.fog;
            // 2. Turn fog off temporarily
            RenderSettings.fog = false;
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == GetComponent<Camera>())
        {
            // 3. Restore fog for the next camera/frame
            RenderSettings.fog = wasFogEnabled;
        }
    }
}