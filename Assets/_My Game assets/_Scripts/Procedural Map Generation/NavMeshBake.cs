using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour
{
    public List<NavMeshSurface> surfaces = new();


    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            surfaces.Add(transform.GetChild(i).GetComponent<NavMeshSurface>());
        }
    }

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
        foreach (NavMeshSurface surface in surfaces)
        {
            surface.BuildNavMesh();
        }

    }
}
