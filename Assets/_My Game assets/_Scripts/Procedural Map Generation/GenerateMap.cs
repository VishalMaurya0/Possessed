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
    private MapCell[,] mapGrid;
    public MapCell centreMapCell;

    public bool updateVisual;

    private void Start()
    {
        grid = GetComponent<Grid>();
        CreateInitialMap();
        ReferenceAdjacentCells();
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

    private void ChooseGate()
    {
        int centerRow = grid.length / grid.cellLength / 2;
        int centerCol = grid.width / grid.cellLength / 2;

        System.Random rand = new System.Random();
        int[] gateOffsets = { -1, 0, 1 };

        // North Gate
        int northGateCol = centerCol + gateOffsets[rand.Next(0, 3)];
        mapGrid[centerRow - 2, northGateCol].wall[3] = WallType.gate;
        mapGrid[centerRow - 1, northGateCol].wall[1] = WallType.gate;

        // South Gate
        int southGateCol = centerCol + gateOffsets[rand.Next(0, 3)];
        mapGrid[centerRow + 2, southGateCol].wall[1] = WallType.gate;
        mapGrid[centerRow + 1, southGateCol].wall[3] = WallType.gate;

        // East Gate
        int eastGateRow = centerRow + gateOffsets[rand.Next(0, 3)];
        mapGrid[eastGateRow, centerCol + 2].wall[2] = WallType.gate;
        mapGrid[eastGateRow, centerCol + 1].wall[0] = WallType.gate;

        // West Gate
        int westGateRow = centerRow + gateOffsets[rand.Next(0, 3)];
        mapGrid[westGateRow, centerCol - 2].wall[0] = WallType.gate;
        mapGrid[westGateRow, centerCol - 1].wall[2] = WallType.gate;
    }

    private void CentreGeneration()
    {
        int centerRow = grid.length / grid.cellLength / 2;
        int centerCol = grid.width / grid.cellLength / 2;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                MapCell cell = mapGrid[centerRow + i, centerCol + j];

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
        Vector3 position = cell.position;
        float halfSize = cell.width / 2f;

        switch (wallIndex)
        {
            case 0: return position + new Vector3(halfSize, 0, 0);
            case 1: return position + new Vector3(0, 0, halfSize);
            case 2: return position - new Vector3(halfSize, 0, 0);
            case 3: return position - new Vector3(0, 0, halfSize);
            default: return position;
        }
    }

    public Quaternion GetWallRotation(int wallIndex)
    {
        switch (wallIndex)
        {
            case 0: return Quaternion.Euler(0, 90, 0);
            case 1: return Quaternion.identity;
            case 2: return Quaternion.Euler(0, -90, 0);
            case 3: return Quaternion.Euler(0, 180, 0);
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
