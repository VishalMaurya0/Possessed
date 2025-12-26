using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private float pickupRange = 3f;
    // [SerializeField] private LayerMask itemLayer; // Set this to the layer your items are on

    private void Update()
    {
        // Only the local player controls input
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }
    }

    private void TryPickup()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            // Try to get the ItemPickup component on the object we hit
            if (hit.collider.TryGetComponent<ItemPickup>(out var item))
            {
                // Call a public method on the item to start the RPC
                item.RequestPickup(); 
            }
        }
    }
}