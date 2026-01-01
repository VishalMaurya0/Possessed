using UnityEngine;
using System.Collections;

public class SkyObjectTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float triggerRadius = 5f;
    public bool triggerOnce = true;

    [Tooltip("Time in seconds between checks. 0.3 = ~3 times per second.")]
    public float checkInterval = 0.3f;

    [Header("Debug Info")]
    public int id = -1;
    public bool hasTriggered = false;

    private void Start()
    {
        if (MiniMapManager.Instance != null && MiniMapManager.Instance.refeDone)
        {
            MiniMapManager.Instance.RegisterTrigger(this);
        }

        SetChildrenActive(false);

        StartCoroutine(CheckDistanceRoutine());
    }

    private IEnumerator CheckDistanceRoutine()
    {
        // OPTIMIZATION: Wait a random tiny amount before starting the loop.
        // This ensures not all 20 objects check on the exact same frame (Load Balancing).
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));

        // Cache the wait so we don't create garbage memory every loop
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            // 1. Guard Clauses
            // If we triggered and only want to trigger once, stop this coroutine entirely.
            if (hasTriggered && triggerOnce) yield break;

            // If game isn't ready, wait and try again next loop
            if (GameManager.Instance != null && GameManager.Instance.ownerPlayer != null)
            {
                PerformCheck();
            }

            // Pause here for 0.3 seconds before looping again
            yield return wait;
        }
    }

    private void PerformCheck()
    {
        Vector3 playerPos = GameManager.Instance.ownerPlayer.transform.position;
        Vector3 myPos = transform.position;

        playerPos.y = 0;
        myPos.y = 0;

        if ((playerPos - myPos).sqrMagnitude <= triggerRadius * triggerRadius)
        {
            AttemptActivation();
        }
    }

    private void AttemptActivation()
    {
        if (hasTriggered) return;

        if (MiniMapManager.Instance != null)
        {
            MiniMapManager.Instance.RequestActivateSkyObject(id);
        }
    }

    public void ActivateVisuals()
    {
        SetChildrenActive(true);
        hasTriggered = true;
    }

    private void SetChildrenActive(bool state)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(state);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        Vector3 groundPos = new Vector3(transform.position.x, 0, transform.position.z);
        Gizmos.DrawWireSphere(groundPos, triggerRadius);
        Gizmos.DrawLine(transform.position, groundPos);
    }
}