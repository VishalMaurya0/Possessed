using Unity.Netcode.Components;
using UnityEngine;

public class DummyFotRigidbodyProps : MonoBehaviour
{
    public int noOfWatchers = 0;
    public NetworkTransform networkTransform;


    private void Start() {
        if (TryGetComponent<ItemPickup>(out var ip))
            return;


        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        networkTransform = GetComponent<NetworkTransform>();

        Invoke(nameof(FreezeItem), 3.0f);
        
    }

    private void FreezeItem()
    {
        // Only freeze if no player is currently holding it or standing near it
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (networkTransform != null)
        {
            networkTransform.enabled = false;
        }
    }
}
