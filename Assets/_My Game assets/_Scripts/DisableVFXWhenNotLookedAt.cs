using UnityEngine;
using UnityEngine.VFX;

public class DisableVFXWhenNotLookedAt : MonoBehaviour
{
    public Camera playerCamera;
    public float activeDistance = 15f;
    [Range(0f, 1f)]
    public float viewDotThreshold = 0.6f;
    public VisualEffect vfx;

    private bool isVisible;

    void Awake()
    {
        if (!vfx)
            vfx = GetComponent<VisualEffect>();
    }

    void Update()
    {
        if (!playerCamera)
        {
            playerCamera = GameManager.Instance.playerCamera;
            return;
        }

        Vector3 toTarget = transform.position - playerCamera.transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        // Distance check
        if (sqrDist > activeDistance * activeDistance)
        {
            Disable();
            return;
        }

        // Angle check
        toTarget.Normalize();
        float dot = Vector3.Dot(playerCamera.transform.forward, toTarget);

        if (dot > viewDotThreshold)
            Enable();
        else
            Disable();
    }

    void Enable()
    {
        if (!isVisible)
        {
            vfx.Play();
            isVisible = true;
        }
    }

    void Disable()
    {
        if (isVisible)
        {
            vfx.Stop();
            isVisible = false;
        }
    }
}
