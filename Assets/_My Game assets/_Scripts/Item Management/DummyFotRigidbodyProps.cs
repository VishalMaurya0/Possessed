using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class DummyFotRigidbodyProps : MonoBehaviour
{
    public int noOfWatchers = 0;
    public NetworkTransform networkTransform;
    public NetworkObject netObj;
    private Rigidbody rb;

    // Timer to prevent rapid freezing/unfreezing
    private Coroutine _sleepCoroutine;

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
        netObj = GetComponent<NetworkObject>();
    }

    private void Start()
    {
        if (TryGetComponent<ItemPickup>(out var ip)) return;

        bool isNetworked = networkTransform != null;
        bool isClient = NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer;

        // 1. Client Logic (Visuals only)
        if (isNetworked && isClient)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = true; // Keep collisions so players don't walk through it
            }
            return;
        }

        // 2. Server / Local Logic
        // Ensure physics is ON at start so it falls to the ground
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // Allow it to fall for 3 seconds before first sleep check
        CancelSleep();
        Invoke(nameof(AttemptFreeze), 3.0f);
    }

    private void OnServerStarted()
    {
        networkTransform = GetComponent<NetworkTransform>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            CancelSleep();
            Invoke(nameof(AttemptFreeze), 3.0f);
        }
    }

    public void SetSleepState(bool wakeUp)
    {
        // Safety: Clients ignore physics commands for networked items
        bool isNetworked = networkTransform != null;
        bool isClient = NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer;
        if (isNetworked && isClient) return;

        // If wakeUp is TRUE, we must wake up IMMEDIATELY
        if (wakeUp)
        {
            CancelSleep(); // Stop any pending freeze
            ApplyPhysicsState(true);
        }
        else
        {
            // If wakeUp is FALSE, don't freeze instantly. 
            // Give it a buffer (e.g. 1.0 second) to see if it enters another trigger 
            // or if it's just jittering on the edge.
            if (_sleepCoroutine == null)
            {
                _sleepCoroutine = StartCoroutine(SleepWithDelay(3.0f));
            }
        }
    }

    private System.Collections.IEnumerator SleepWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // If we are still here and watchers are 0, then freeze.
        ApplyPhysicsState(false);
        _sleepCoroutine = null;
    }

    private void CancelSleep()
    {
        if (_sleepCoroutine != null)
        {
            StopCoroutine(_sleepCoroutine);
            _sleepCoroutine = null;
        }
        CancelInvoke(nameof(AttemptFreeze));
    }

    private void ApplyPhysicsState(bool isAwake)
    {
        if (rb == null) return;

        if (isAwake)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            if (networkTransform) networkTransform.enabled = true;
        }
        else
        {
            // Only freeze if no one is watching
            if (noOfWatchers > 0) return;

            rb.isKinematic = true;
            rb.detectCollisions = true; // Still solid
            if (networkTransform) networkTransform.enabled = false;
        }
    }

    private void AttemptFreeze()
    {
        ApplyPhysicsState(false);
    }
}