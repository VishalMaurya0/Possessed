using UnityEngine;

public class WallDetection : MonoBehaviour
{
    public Vector3 wallNormal { get; private set; }
    public bool isTouchingWall { get; private set; }

    private void OnTriggerStay(Collider other)
    {
        wallNormal = Vector3.zero;
        // Only process if it's part of the environment (add your own tag/layer check if needed)
        if (other.attachedRigidbody == null)
        {
            // Try to find the closest point and direction between the two colliders
            Vector3 closestPoint = other.ClosestPoint(transform.position);

            Ray ray = new Ray(transform.position, (closestPoint - transform.position).normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            {
                isTouchingWall = true;
                wallNormal = hit.normal;
                Debug.DrawRay(hit.point, wallNormal * 2f, Color.green);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isTouchingWall = false;
        wallNormal = Vector3.zero;
    }
}



