using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface surface;


    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.bakeNavMeshAgain)
        {
            GameManager.Instance.bakeNavMeshAgain = false;
            Bake();
        }
    }

    public void Bake()
    {
        surface.BuildNavMesh();
    }
}
