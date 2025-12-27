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
        // Only run on Server (for networked items) or Owner (for local debris)
        if (!IsServer && !IsOwner)
        {
            gameObject.SetActive (false);
            enabled = false;
            return;
        }

        if (IsServer)
        {
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                myCollider.isTrigger = true;
                InitialScan(myCollider);
            }
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
        // Determine if we are allowed to touch this object
        bool isNetworkedItem = props.GetComponent<NetworkObject>() != null;
        if (!isNetworkedItem)  isNetworkedItem = props.networkTransform != null;

        if (isNetworkedItem)
        {

            // Only Server wakes up networked items
            if (props.networkTransform.OwnerClientId == GameManager.Instance.OwnerClientId || IsServer)
                SetItemPhysics(props, enter);
        }
        else
        {
            // Owner wakes up local debris
            if (IsOwner) SetItemPhysics(props, enter);
        }
    }
    private void SetItemPhysics(DummyFotRigidbodyProps item, bool enablePhysics)
    {
        if (enablePhysics)
        {
            if (_watchedItems.Add(item)) item.noOfWatchers++;
        }
        else
        {
            if (_watchedItems.Remove(item)) item.noOfWatchers--;
        }

        if (item.noOfWatchers < 0) item.noOfWatchers = 0;

        // Use the helper method on the prop itself
        bool shouldWakeUp = item.noOfWatchers > 0;
        item.SetSleepState(shouldWakeUp);
    }
}