using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class ItemActiveTrigger : NetworkBehaviour
{
    // Keep track of which objects THIS player has woken up
    private HashSet<DummyFotRigidbodyProps> _watchedItems = new HashSet<DummyFotRigidbodyProps>();

    public override void OnNetworkSpawn()
    {
        // LOGIC: 
        // 1. Server must run to handle Networked Items.
        // 2. Owner must run to handle Local Debris/Props.
        // 3. Other Clients (proxy players) should NOT run to save performance.
        if (!IsServer && !IsOwner)
        {
            enabled = false;
            return;
        }

        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            myCollider.isTrigger = true;
            InitialScan(myCollider);
        }
    }

    public override void OnNetworkDespawn()
    {
        // Cleanup on disconnect
        foreach (var item in new List<DummyFotRigidbodyProps>(_watchedItems))
        {
            if (item != null)
            {
                // We pass 'true' for forceRelease to ignore checks, 
                // but we must respect the authority check inside SetItemPhysics logic.
                // Simpler approach: Just force decrement directly if we own the lock.
                SetItemPhysics(item, false);
            }
        }
        _watchedItems.Clear();
    }

    private void InitialScan(Collider myCollider)
    {
        Collider[] hits = Physics.OverlapBox(myCollider.bounds.center, myCollider.bounds.extents, transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit == myCollider) continue;

            if (hit.TryGetComponent(out DummyFotRigidbodyProps props))
            {
                HandleObjectDetection(props, true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DummyFotRigidbodyProps props))
        {
            HandleObjectDetection(props, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out DummyFotRigidbodyProps props))
        {
            HandleObjectDetection(props, false);
        }
    }

    // --- SEPARATION LOGIC HERE ---
    private void HandleObjectDetection(DummyFotRigidbodyProps props, bool enter)
    {
        // Check if this is a Networked Item
        bool isNetworkedItem = props.TryGetComponent(out ItemPickup itemPickup);

        if (isNetworkedItem)
        {
            // CASE 1: Networked Item -> ONLY SERVER allowed to touch
            if (IsServer)
            {
                SetItemPhysics(props, enter);
            }
        }
        else
        {
            // CASE 2: Local Prop (No ItemPickup) -> ONLY OWNER allowed to touch
            // (These are client-side debris/objects not synced over network)
            if (IsOwner || IsServer)
            {
                SetItemPhysics(props, enter);
            }
        }
    }

    private void SetItemPhysics(DummyFotRigidbodyProps item, bool enablePhysics)
    {
        // Ref Counting Logic
        if (enablePhysics)
        {
            if (_watchedItems.Add(item))
            {
                item.noOfWatchers++;
            }
        }
        else
        {
            if (_watchedItems.Remove(item))
            {
                item.noOfWatchers--;
            }
        }

        // Physics Application Logic
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (item.noOfWatchers > 0)
        {
            // If it is asleep, wake it up
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }
            if (item.networkTransform != null)
            {
                item.networkTransform.enabled = true;
            }
        }
        else
        {
            // If no one is watching, put to sleep
            // Ensure we don't go negative
            if (item.noOfWatchers < 0) item.noOfWatchers = 0;

            if (item.noOfWatchers == 0 && !rb.isKinematic)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
            if (item.networkTransform != null)
            {
                item.networkTransform.enabled = false;
            }
        }
    }
}