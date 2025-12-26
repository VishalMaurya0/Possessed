using UnityEngine;

public class DetectorForFallingObjects : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        other.transform.position += new Vector3(0, -other.transform.position.y + 3, 0);
    }
}
