using System.Collections.Generic;
using UnityEngine;

public class MapVisualTemp : MonoBehaviour
{

    [Header("References")]
    public GenerateMap generateMap;
    public ProceduralMapDataSO proceduralMapDataSO;


    public List<GameObject> walls;

    [Header("Pool Objects")]
    public Transform wallsContainer;
    private Dictionary<string, Queue<GameObject>> wallPool = new Dictionary<string, Queue<GameObject>>();



    private void Start()
    {
        generateMap = GetComponent<GenerateMap>();
    }


    public void InitializeWallPool()
    {
        wallPool.Clear();
        foreach (GameObject wallPrefab in walls)
        {
            if (wallPrefab != null)
                wallPool[wallPrefab.name] = new Queue<GameObject>();
        }
    }


    public GameObject GetWallFromPool(GameObject wallPrefab)
    {
        string wallName = wallPrefab.name;

        if (wallPool.ContainsKey(wallName))
        {
            if (wallPool[wallName].Count > 0)
            {
                GameObject wall = wallPool[wallName].Dequeue();
                wall.SetActive(true);
                return wall;
            }
        }
        else
        {
            wallPool[wallName] = new Queue<GameObject>();
        }

        // Create new wall if pool is empty
        GameObject newWall = Instantiate(wallPrefab, wallsContainer);
        newWall.name = wallName;
        return newWall;
    }


    public void ReturnWallToPool(GameObject wall)
    {
        if (wall == null) return;

        wall.SetActive(false);
        string wallName = wall.name.Split('(')[0].Trim(); // Remove any "(Clone)" suffix

        if (!wallPool.ContainsKey(wallName))
        {
            wallPool[wallName] = new Queue<GameObject>();
        }

        wallPool[wallName].Enqueue(wall);
    }


    public void UpdateVisual()
    {
        if (generateMap == null)
            generateMap = GetComponent<GenerateMap>();
        generateMap.generateAgain = false;

        int rowCount = generateMap.mapCells.GetLength(0);
        int columnCount = generateMap.mapCells.GetLength(1);

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                if (generateMap.mapCells[i, j].cellObjectTemporary == null)
                {
                    GameObject obj;
                    obj = Instantiate(generateMap.CellObj, transform);
                    obj.transform.position = generateMap.mapCells[i, j].position;
                    obj.transform.localScale = obj.transform.localScale * generateMap.mapCells[i, j].width;
                    obj.name = $"Cell ({j}, {i})";
                    generateMap.mapCells[i, j].cellObjectTemporary = obj;
                }
            }
        }

        // Wall Visual
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                MapCell cell = generateMap.mapCells[i, j];

                for (int k = 0; k < cell.wall.Length; k++)
                {
                    if (cell.wall[k] == Type.NoWall)
                    {
                        if (cell.wallGTemporary[k] != null)
                        {
                            ReturnWallToPool(cell.wallGTemporary[k]);
                            cell.wallGTemporary[k] = null;
                        }
                        continue;
                    }

                    // Ensure wallPrefab is correctly selected
                    GameObject wallPrefab = walls[(int)cell.wall[k]];
                    if (wallPrefab == null)
                    {
                        continue;
                    }

                    // Get wall from pool only if it doesn't exist
                    if (cell.wallGTemporary[k] == null)
                    {
                        GameObject newWall = GetWallFromPool(wallPrefab);
                        newWall.transform.position = cell.GetWallPosition(k, cell);
                        newWall.transform.rotation = cell.GetWallRotation(k);
                        newWall.SetActive(true); // Ensure it's visible

                        cell.wallGTemporary[k] = newWall;

                        // Sync opposite wall
                        int oppositeIndex = (k + 2) % 4;
                        if (cell.adjCell[k] != null && cell.adjCell[k].wallGTemporary[oppositeIndex] == null)
                        {
                            cell.adjCell[k].wallGTemporary[oppositeIndex] = newWall;
                        }
                    }
                }
            }
        }
    }
}
