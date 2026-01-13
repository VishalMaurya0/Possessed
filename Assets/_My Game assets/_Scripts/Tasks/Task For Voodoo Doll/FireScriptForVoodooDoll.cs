using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class FireScriptForVoodooDoll : NetworkBehaviour
{
    [SerializeField] GameObject dollPrefab;
    GameObject newDoll;
    [SerializeField] int spawnRadius = 80;
    [SerializeField] TaskVoodooDoll taskVoodooDoll__parent;
    [SerializeField] Vector3 centerPos = new Vector3 (68, 0, 68);
    public ParticleSystem fire;
    public bool activated = true;

    void Start()
    {
        taskVoodooDoll__parent = GetComponentInParent<TaskVoodooDoll>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (!IsServer)
        {
            Debug.Log("Not server, skipping spawn.");
            return;
        }

        if (collision.gameObject.CompareTag("Doll") && activated)
        {
            activated = false;
            //Debug.Log("Collided with Doll! Starting despawn and respawn process.");

            fire.Stop();

            NetworkObject doll = collision.gameObject.GetComponent<NetworkObject>();
            Vector3 storedPos = collision.transform.position;
            //Debug.Log("Stored old doll position: " + storedPos);

            if (doll != null)
            {
                //Debug.Log("Despawn Doll!");
                doll.gameObject.SetActive(false);
                taskVoodooDoll__parent.dollsInMap--;
            }
            else
            {
                Debug.LogError("NetworkObject missing on Doll!");
            }

            taskVoodooDoll__parent.dollsAdded++;

            if (FindNavMeshPosition(centerPos, out Vector3 result))
            {
                //Debug.Log("Found new NavMesh position: " + result);

                doll.transform.position = result;
                //newDoll = Instantiate(dollPrefab, result, Quaternion.identity);
                //doll = newDoll.GetComponent<NetworkObject>();
                doll.gameObject.SetActive(true);

                //newDoll.transform.position = result;
                //Debug.Log("New doll instantiated at: " + newDoll.transform.position);

                //doll.Spawn();
                //Debug.Log("New doll spawned on server.");
            }
            else
            {
                Debug.LogError("Failed to find valid NavMesh position for new doll.");
            }
        }
    }


    public bool SpawnADoll()
    {
                if (!IsServer) return false;
        if (FindNavMeshPosition(centerPos, out Vector3 result))
        {
            newDoll = Instantiate(dollPrefab, result, Quaternion.identity);
            NetworkObject doll = newDoll.GetComponent<NetworkObject>();
            doll.Spawn();
            doll.TrySetParent(taskVoodooDoll__parent.enemiesContainer.transform, false);
            taskVoodooDoll__parent.dollsInMap++;
            return true;
        }
        return false;
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


            if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {


                if (PosDirectlyNotVisToPlayers(hit.position))
                {
                    result = hit.position;
                    return true;
                }
            }
        }
        result = Vector3.zero;
        return false;
    }



    private bool PosDirectlyNotVisToPlayers(Vector3 pos)
    {
        List<GameObject> players = new();
        foreach (var client in GameManager.Instance.connectedClientsData)
        {
            players.Add(client.playerGameobject);
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
