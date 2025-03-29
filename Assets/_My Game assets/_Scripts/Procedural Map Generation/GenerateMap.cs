using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    [Header("References")]
    public Grid grid;

    [Header("Inputs")]
    public GameObject CellObj;
    public List<GameObject> walls;

    [Header("Map Properties")]
    public List<RowCell> mapCells = new List<RowCell>();
    private MapCell[,] mapGrid;
    public MapCell centreMapCell;

    public bool updateVisual;

    private void Start()
    {
        grid = GetComponent<Grid>();
        CreateInitialMap();
        ReferenceAdjacentCells();
        //StartGeneration();

        //Debug.Log(mapCells[2].rowCells[0].leftCell);
        //Debug.Log(mapCells[2].rowCells[0].rightCell.position);
        //Debug.Log(mapCells[2].rowCells[0].topCell.position);
        //Debug.Log(mapCells[2].rowCells[0].bottomCell.position);
    }

    private void Update()
    {
        if (updateVisual)
        {
            //UpdateVisual();
        }
    }

    private void ReferenceAdjacentCells()
    {
        int rowCount = mapGrid.GetLength(0);
        int columnCount = mapGrid.GetLength(1);

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                MapCell cell = mapGrid[i, j];

                if (j + 1 < columnCount) cell.adjCell[0] = mapGrid[i, j + 1]; // Right
                if (i - 1 >= 0) cell.adjCell[1] = mapGrid[i - 1, j]; // Top
                if (j - 1 >= 0) cell.adjCell[2] = mapGrid[i, j - 1]; // Left
                if (i + 1 < rowCount) cell.adjCell[3] = mapGrid[i + 1, j]; // Bottom
            }
        }
    }


    private void UpdateVisual()
    {
        updateVisual = false;

        int rowCount = mapCells.Count;
        int columnCount = mapCells[0].rowCells.Count;

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                GameObject obj = Instantiate(CellObj, transform);
                obj.transform.position = mapCells[i].rowCells[j].position;
                obj.transform.localScale = Vector3.one * (grid.cellLength - 0.2f);
                obj.name = $"Cell ({j}, {i})";
                mapCells[i].rowCells[j].cellObject = obj;
            }
        }

        //Wall Visual

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                MapCell cell = mapCells[i].rowCells[j];

                for (int k = 0; k < cell.wall.Length; k++)
                {
                    if (cell.wallG[k] == null && cell.wall[k] != WallType.noWall)
                    {
                        // Get the wall prefab from the list based on wall type
                        GameObject wallPrefab = walls[(int)cell.wall[k]];

                        if (wallPrefab != null)
                        {
                            // Instantiate the wall at the appropriate position & rotation
                            GameObject newWall = GameObject.Instantiate(wallPrefab, cell.GetWallPosition(k, cell), cell.GetWallRotation(k));

                            // Store the reference
                            cell.wallG[k] = newWall;
                            int oppositeIndex = (k + 2) % 4; // 0↔2 (right-left), 1↔3 (top-bottom)
                            cell.adjCell[k].wallG[oppositeIndex] = newWall;
                        }
                    }
                }
            }
        }
    }




    private void CreateInitialMap()
    {
        int rows = grid.length / grid.cellLength;
        int cols = grid.width / grid.cellLength;
        mapGrid = new MapCell[rows, cols]; // Allocate fixed-size array

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Cell cell = grid.cells[j + (i * cols)];
                mapGrid[i, j] = new MapCell(cell);
            }
        }
    }


    void RemoveWalls(MapCell cell, int right, int top, int left, int bottom)
    {
        int[] directions = { right, top, left, bottom };

        for (int i = 0; i < 4; i++)
        {
            if (directions[i] > -1)
            {
                switch (directions[i])
                {
                    case 0: cell.wall[i] = WallType.noWall; break;
                    case 1: cell.wall[i] = WallType.wall; break;
                    case 2: cell.wall[i] = WallType.gate; break;
                    case 3: cell.wall[i] = WallType.window; break;
                }

                // Update adjacent cell's corresponding wall
                if (cell.adjCell[i] != null)
                {
                    int oppositeIndex = (i + 2) % 4; // 0↔2 (right-left), 1↔3 (top-bottom)
                    cell.adjCell[i].wall[oppositeIndex] = cell.wall[i];
                }
            }
        }
    }




    private void StartGeneration()
    {
        CentreGeneration();
        ChooseGate();
        GeneratePath();
    }

    private void GeneratePath()
    {
        List<MapCell> startCells = new List<MapCell>();
        //startCells.Add(centreMapCell.leftCell.leftCell);
        //startCells.Add(centreMapCell.rightCell.rightCell);
        //startCells.Add(centreMapCell.topCell.topCell);
        //startCells.Add(centreMapCell.bottomCell.bottomCell);

        foreach (var cell in startCells)
        {

        }
    }

    private void ChooseGate()
    {
        int centerRow = grid.length / grid.cellLength / 2;
        int centerCol = grid.width / grid.cellLength / 2;

        System.Random rand = new System.Random();
        int[] gateOffsets = { -1, 0, 1 };

        // North Gate
        int northGateCol = centerCol + gateOffsets[rand.Next(0, 3)];
        mapCells[centerRow - 2].rowCells[northGateCol].wall[3] = WallType.gate;
        mapCells[centerRow - 1].rowCells[northGateCol].wall[1] = WallType.gate;

        // South Gate
        int southGateCol = centerCol + gateOffsets[rand.Next(0, 3)];
        mapCells[centerRow + 2].rowCells[southGateCol].wall[1] = WallType.gate;
        mapCells[centerRow + 1].rowCells[southGateCol].wall[3] = WallType.gate;

        // East Gate
        int eastGateRow = centerRow + gateOffsets[rand.Next(0, 3)];
        mapCells[eastGateRow].rowCells[centerCol + 2].wall[2] = WallType.gate;
        mapCells[eastGateRow].rowCells[centerCol + 1].wall[0] = WallType.gate;

        // West Gate
        int westGateRow = centerRow + gateOffsets[rand.Next(0, 3)];
        mapCells[westGateRow].rowCells[centerCol - 2].wall[0] = WallType.gate;
        mapCells[westGateRow].rowCells[centerCol - 1].wall[2] = WallType.gate;
    }


    private void CentreGeneration()
    {
        int centerRow = grid.length / grid.cellLength / 2;
        int centerCol = grid.width / grid.cellLength / 2;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                MapCell cell = mapCells[centerRow + i].rowCells[centerCol + j];

                if (j < 1) cell.wall[0] = WallType.noWall; // Right
                if (j > -1) cell.wall[2] = WallType.noWall; // Left
                if (i > -1) cell.wall[1] = WallType.noWall; // Top
                if (i < 1) cell.wall[3] = WallType.noWall; // Bottom
            }
        }
    }
}


[System.Serializable]
public class MapCell
{
    public Vector3 position;
    public int width;
    public Vector2 id;
    
    // 0 => right , 1 => top , 2 => left , 3 => bottom

    public MapCell[] adjCell = new MapCell[4];

    public WallType[] wall = new WallType[4];

    public GameObject[] wallG = new GameObject[4];

    public bool visited;

    public GameObject cellObject;

    public MapCell(Cell cell, GameObject cellObject = null)
    {
        position = cell.position;
        width = cell.width;
        id.x = cell.position.x / 2;
        id.y = cell.position.z / 2;
        for (int i = 0; i < wall.Length; i++)
        {
            wall[i] = WallType.wall;
        }
        this.cellObject = cellObject;
    }


    public Vector3 GetWallPosition(int wallIndex, MapCell cell)
    {
        Vector3 position = cell.position; // Assuming 'position' is the cell's center
        float halfSize = cell.width / 2f; // Adjust for correct wall placement

        switch (wallIndex)
        {
            case 0: return position + new Vector3(halfSize, 0, 0); // Right
            case 1: return position + new Vector3(0, 0, halfSize); // Top
            case 2: return position - new Vector3(halfSize, 0, 0); // Left
            case 3: return position - new Vector3(0, 0, halfSize); // Bottom
            default: return position;
        }
    }

    public Quaternion GetWallRotation(int wallIndex)
    {
        switch (wallIndex)
        {
            case 0: return Quaternion.Euler(0, 90, 0); // Right wall
            case 1: return Quaternion.identity; // Top wall
            case 2: return Quaternion.Euler(0, -90, 0); // Left wall
            case 3: return Quaternion.Euler(0, 180, 0); // Bottom wall
            default: return Quaternion.identity;
        }
    }
}

public enum WallType
{
    noWall,
    wall,
    gate,
    window,
}

[System.Serializable]
public class RowCell
{
    public List<MapCell> rowCells = new List<MapCell>();
}
