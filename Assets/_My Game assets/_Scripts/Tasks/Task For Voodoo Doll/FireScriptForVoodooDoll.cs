using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class FireScriptForVoodooDoll : NetworkBehaviour
{
    [SerializeField] GameObject dollPrefab;
    GameObject newDoll;
    [SerializeField] int spawnRadius = 20;
    [SerializeField] TaskVoodooDoll taskVoodooDoll__parent;
    public ParticleSystem fire;
    public bool activated = true;

    void Start()
    {
        taskVoodooDoll__parent = GetComponentInParent<TaskVoodooDoll>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (!IsServer)
        {
            Debug.Log("Not server, skipping spawn.");
            return;
        }

        if (collision.gameObject.CompareTag("Doll") && activated)
        {
            activated = false;
            Debug.Log("Collided with Doll! Starting despawn and respawn process.");

            fire.Stop();

            NetworkObject doll = collision.gameObject.GetComponent<NetworkObject>();
            Vector3 storedPos = collision.transform.position;
            Debug.Log("Stored old doll position: " + storedPos);

            if (doll != null)
            {
                Debug.Log("Despawn Doll!");
                doll.Despawn();
            }
            else
            {
                Debug.LogError("NetworkObject missing on Doll!");
            }

            taskVoodooDoll__parent.dollsAdded++;

            if (FindNavMeshPosition(storedPos, out Vector3 result))
            {
                Debug.Log("Found new NavMesh position: " + result);

                newDoll = Instantiate(dollPrefab, result, Quaternion.identity);
                doll = newDoll.GetComponent<NetworkObject>();

                newDoll.transform.position = result;
                Debug.Log("New doll instantiated at: " + newDoll.transform.position);

                doll.Spawn();
                Debug.Log("New doll spawned on server.");
            }
            else
            {
                Debug.LogError("Failed to find valid NavMesh position for new doll.");
            }
        }
    }


    private bool FindNavMeshPosition(Vector3 center, out Vector3 result)
    {
        Debug.Log("Finding NavMesh position near: " + center);

        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection.y = 0f;
            Vector3 potentialPosition = center + randomDirection;

            Debug.Log($"[Attempt {i}] Trying potential position: {potentialPosition}");

            if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Debug.Log($"Valid NavMesh position found: {hit.position}");


                if (PosDirectlyNotVisToPlayers(hit.position))
                {
                    Debug.Log("Position not visible to players. Accepting.");
                    result = hit.position;
                    return true;
                }
                else
                {
                    Debug.Log("Position visible to players. Rejecting.");
                }
            }
            else
            {
                Debug.Log("NavMesh.SamplePosition failed for this attempt.");
            }
        }

        Debug.LogWarning("Could not find suitable NavMesh position after 100 attempts.");
        result = Vector3.zero;
        return false;
    }



    private bool PosDirectlyNotVisToPlayers(Vector3 pos)
    {
        List<GameObject> players = new();
        foreach (var client in GameManager.Instance.connectedClients)
        {
            players.Add(client.Value);
        }

        foreach (var player in players)
        {
            if (Physics.Raycast(pos, (player.transform.position - pos), out RaycastHit info))
            {
                if (info.collider.gameObject == player.gameObject)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
