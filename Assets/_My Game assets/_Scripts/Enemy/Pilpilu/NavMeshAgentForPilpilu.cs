using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentForPilpilu : MonoBehaviour
{
    Transform parent;
    //Rigidbody rb;
    NavMeshHit hit;
    NavMeshAgent navMeshAgent;

    private void Start()
    {
        parent = transform.parent;
        //rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        transform.position = parent.transform.position;
        //Vector3 force = parent.position - transform.position;
        //force.y = 0;
        //rb.AddForce(force);
        if (!navMeshAgent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(navMeshAgent.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
                //Debug.Log("Warped agent to nearest NavMesh position");
            }
        }
    }

}
