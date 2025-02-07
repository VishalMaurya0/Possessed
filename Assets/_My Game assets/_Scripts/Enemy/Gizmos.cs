using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshAgentGizmos : MonoBehaviour
{
    private NavMeshAgent agent;

    // Colors for the gizmos
    public Color pathColor = Color.green;
    public Color destinationColor = Color.red;
    public Color stoppingDistanceColor = Color.yellow;
    public Color agentRadiusColor = Color.blue;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnDrawGizmos()
    {
        if (!agent || !agent.isOnNavMesh)
            return;

        // Draw the agent's current path
        if (agent.hasPath)
        {
            DrawPath(agent.path);
        }

        // Draw the agent's stopping distance as a sphere
        Gizmos.color = stoppingDistanceColor;
        Gizmos.DrawWireSphere(agent.transform.position, agent.stoppingDistance);

        // Draw the agent's radius as a wire sphere
        Gizmos.color = agentRadiusColor;
        Gizmos.DrawWireSphere(agent.transform.position, agent.radius);

        // Draw the agent's destination
        Gizmos.color = destinationColor;
        Gizmos.DrawSphere(agent.destination, 0.2f);
    }

    private void DrawPath(NavMeshPath path)
    {
        if (path == null || path.corners.Length < 2)
            return;

        Gizmos.color = pathColor;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            Gizmos.DrawSphere(path.corners[i], 0.1f);
        }
    }
}
