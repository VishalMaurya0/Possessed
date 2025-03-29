using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Inputs")]
    public int length;
    public int width;
    public int cellLength;

    [Header("Grid Properties")]
    public List<Cell> cells = new();

    private void Awake()
    {
        cells.Clear();
        FixLengthAndWidth();
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for (int i = 0; i < length; i += cellLength)
        {
            for (int j = 0; j < width; j += cellLength)
            {
                Vector3 cellPosition = new Vector3(i, 0, j);
                cells.Add(CreateCell(cellPosition));
            }
        }
    }

    private Cell CreateCell(Vector3 cellPosition)
    {
        Cell cell = new Cell(cellPosition, cellLength);
        return cell;
    }

    private void FixLengthAndWidth()
    {
        length -= length % cellLength;
        width -= width % cellLength;
    }
}

[System.Serializable]
public class Cell
{
    public Vector3 position;
    public int width;
    public int rank;

    public Cell(Vector3 position, int cellLength, int rank = 0)
    {
        this.width = cellLength;
        this.position = position;
        this.rank = rank;
    }
}