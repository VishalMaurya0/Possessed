using UnityEngine;

public class SkyObjectTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How close the player needs to be (horizontally) to trigger this.")]
    public float triggerRadius = 5f;

    [Tooltip("If true, this script will disable itself after triggering to save performance.")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // Optional: Ensure children are hidden at start
        SetChildrenActive(false); 
    }

    private void Update()
    {
        // 1. Guard clauses: Stop if triggered or player doesn't exist yet
        if (hasTriggered && triggerOnce) return;
        if (GameManager.Instance == null || GameManager.Instance.ownerPlayer == null) return;

        // 2. Get positions
        Vector3 playerPos = GameManager.Instance.ownerPlayer.transform.position;
        Vector3 myPos = transform.position;

        // 3. FLATTEN THE POSITIONS (Ignore Y axis)
        playerPos.y = 0;
        myPos.y = 0;

        // 4. Check Distance
        if (Vector3.Distance(playerPos, myPos) <= triggerRadius)
        {
            ActivateChildren();
        }
    }

    private void ActivateChildren()
    {
        // Loop through all direct children of this object
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        hasTriggered = true;
        Debug.Log($"[SkyObjectTrigger] Player reached {gameObject.name}. Children activated.");
    }

    // Helper to hide children initially if needed
    private void SetChildrenActive(bool state)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(state);
        }
    }

    // =========================================================
    // VISUALIZATION
    // This draws a yellow cylinder in the Scene view so you can 
    // see the trigger zone even if the object is high up.
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Draw the sphere at the object's actual height
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // Draw a line down to the "ground" (assuming ground is roughly 0) to help you align it
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, 0, transform.position.z));

        // Draw a circle on the ground (Y=0) representing the trigger zone
        Vector3 groundPos = new Vector3(transform.position.x, 0, transform.position.z);
        Gizmos.DrawWireSphere(groundPos, triggerRadius);
    }
}