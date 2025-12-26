using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class DummyFotRigidbodyProps : MonoBehaviour
{
    public int noOfWatchers = 0;
    public NetworkTransform networkTransform;
    private Rigidbody rb;

    private void OnEnable()
    {
        GameManager.onServerStarted += OnServerStarted;
    }

    private void OnDisable()
    {
        GameManager.onServerStarted -= OnServerStarted;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        // Ignore if this is an ItemPickup (handled elsewhere)
        if (TryGetComponent<ItemPickup>(out var ip)) return;

        // --- THE FIX IS HERE ---
        bool isNetworked = networkTransform != null;
        bool isClient = NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer;

        // If this is a Networked object and we are just a Client, 
        // we MUST be Kinematic. No physics allowed.
        if (isNetworked && isClient)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
            // Disable this script's logic so we don't accidentally turn it on later
            return;
        }

        // --- SERVER or LOCAL DEBRIS LOGIC ---
        // If we reached here, we are either the Server OR it's a local non-networked prop.
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // Schedule sleep to save performance
        Invoke(nameof(FreezeItem), 3.0f);
    }

    private void OnServerStarted()
    {
        // This runs when the Host starts the server
        networkTransform = GetComponent<NetworkTransform>();

        // Since we are the Server (Host), we want physics ON initially
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        //if (networkTransform != null) return;
        if (NetworkManager.Singleton.IsServer)
            Invoke(nameof(FreezeItem), 3.0f);
    }

    // Helper method called by ItemActiveTrigger
    public void SetSleepState(bool wakeUp)
    {
        // Safety check: Clients should never run this for networked items
        bool isNetworked = networkTransform != null;
        bool isClient = NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer;

        if (isNetworked && isClient) return;

        if (rb != null)
        {
            if (wakeUp)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }
            else
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }

        // Optimization: Disable NetworkTransform when sleeping
        if (networkTransform != null)
        {
            networkTransform.enabled = wakeUp;
        }
    }

    private void FreezeItem()
    {
        if (noOfWatchers > 0) return;
        SetSleepState(false);
    }
}