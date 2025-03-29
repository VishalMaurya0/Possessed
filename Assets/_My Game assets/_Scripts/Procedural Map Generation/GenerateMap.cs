using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    [Header("References")]
    public Grid grid;

    [Header("Inputs")]
    public GameObject CellObj;
    public List<GameObject> walls;
    public int roomMinLength;
    public int roomMaxLength;
    public int totalRooms;

    [Header("Map Properties")]
    public MapCell[,] mapCells;
    private MapCell centreMapCell;
    private List<MapCell> InitialGateRooms = new List<MapCell>();

    public bool updateVisual;

    private void Start()
    {
        grid = GetComponent<Grid>();
        CreateInitialMap();
        ReferenceAdjacentCells();
        StartGeneration();
    }

    private void CreateInitialMap()
    {
        int rows = grid.length / grid.cellLength;
        int cols = grid.width / grid.cellLength;
        mapCells = new MapCell[rows, cols]; // Allocate fixed-size array

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Cell cell = grid.cells[j + (i * cols)];
                mapCells[i, j] = new MapCell(cell);
            }
        }
    }

    private void ReferenceAdjacentCells()
    {
        int rowCount = mapCells.GetLength(0);
        int columnCount = mapCells.GetLength(1);

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                MapCell cell = mapCells[i, j];

                if (i + 1 < rowCount) cell.adjCell[0] = mapCells[i + 1, j]; // Right
                if (j + 1 < columnCount) cell.adjCell[1] = mapCells[i, j + 1]; // Top
                if (i - 1 >= 0) cell.adjCell[2] = mapCells[i - 1, j]; // Left
                if (j - 1 >= 0) cell.adjCell[3] = mapCells[i, j - 1]; // Bottom
            }
        }
    }


    private void StartGeneration()
    {
        CentreGeneration();
        ChooseGate();
        GenerateRooms(roomMinLength, roomMaxLength, totalRooms);
        GeneratePath();
    }

    private void Update()
    {
        if (updateVisual)
        {
            UpdateVisual();
        }
    }
    private void UpdateVisual()
    {
        updateVisual = false;

        int rowCount = mapCells.GetLength(0);
        int columnCount = mapCells.GetLength(1);

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                GameObject obj = Instantiate(CellObj, transform);
                obj.transform.position = mapCells[i, j].position;
                obj.transform.localScale = Vector3.one * (grid.cellLength - 0.2f);
                obj.name = $"Cell ({j}, {i})";
                mapCells[i, j].cellObject = obj;
            }
        }

        //Wall Visual
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                MapCell cell = mapCells[i, j];

                for (int k = 0; k < cell.wall.Length; k++)
                {
                    if (cell.wall[k] == WallType.noWall) Destroy(cell.wallG[k]);

                    if (cell.wallG[k] == null && cell.wall[k] != WallType.noWall)
                    {
                        // Get the wall prefab from the list based on wall type
                        GameObject wallPrefab = walls[(int)cell.wall[k]];

                        if (wallPrefab != null)
                        {
                            // Instantiate the wall at the appropriate position & rotation
                            GameObject newWall = Instantiate(wallPrefab, cell.GetWallPosition(k, cell), cell.GetWallRotation(k));

                            // Store the reference
                            cell.wallG[k] = newWall;
                            int oppositeIndex = (k + 2) % 4; // 0↔2 (right-left), 1↔3 (top-bottom)

                            if (cell.adjCell[k] != null)
                            {
                                cell.adjCell[k].wallG[oppositeIndex] = newWall;
                            }
                        }
                    }
                }
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



    private void CentreGeneration()
    {
        int centerRow = grid.length / grid.cellLength / 2;
        int centerCol = grid.width / grid.cellLength / 2;

        centreMapCell = mapCells[centerRow, centerCol];

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                MapCell cell = mapCells[centerRow + i, centerCol + j];

                if (i < 1) cell.wall[0] = WallType.noWall; // Right
                if (i > -1) cell.wall[2] = WallType.noWall; // Left
                if (j > -1) cell.wall[3] = WallType.noWall; // Top
                if (j < 1) cell.wall[1] = WallType.noWall; // Bottom

                cell.inRoom = true;
            }
        }
    }
    private void ChooseGate()
    {
        int rowCount = mapCells.GetLength(0);
        int colCount = mapCells.GetLength(1);

        int centerRow = rowCount / 2;
        int centerCol = colCount / 2;

        System.Random rand = new System.Random();

        for (int i = -1; i < 2; i += 2)   ///choosing direction Left and Right
        {
            int j = rand.Next(-1, 2);   // -1 , 0, 1 
            MapCell cell = mapCells[i + centerRow, j + centerCol];
            switch(i)
            {
                case -1: RemoveWalls(cell, -1, -1, 2, -1); break;
                case 1: RemoveWalls(cell, 2, -1, -1, -1); break;
            }
            InitialGateRooms.Add(cell);
            cell.inRoom = true;
        }
        
        for (int i = -1; i < 2; i += 2)   ///choosing direction North and south
        {
            int j = rand.Next(-1, 2);   // -1 , 0, 1 
            MapCell cell = mapCells[j + centerRow, i + centerCol];
            switch(i)
            {
                case -1: RemoveWalls(cell, -1, -1, -1, 2); break;
                case 1: RemoveWalls(cell, -1, 2, -1, -1); break;
            }
            InitialGateRooms.Add(cell);
            cell.inRoom = true;
        }
    }




    private void GenerateRooms(int roomMinLength, int roomMaxLength, int totalRooms)
    {
        for (int i = 0; i < totalRooms; i++)
        {
            CreateARoom(roomMinLength, roomMaxLength);
        }
    }
    private void CreateARoom(int roomMinLength, int roomMaxLength)
    {
        int length = Random.Range(roomMinLength, roomMaxLength + 1);
        int width = Random.Range(roomMinLength, roomMaxLength + 1);

        Vector2 start = new Vector2(Random.Range(0, grid.length), Random.Range(0, grid.width));

        bool flag = false;

        // Ensure the room fits within the grid
        if (start.x + length > mapCells.GetLength(0) || start.y + width > mapCells.GetLength(1))
        {
            return; // Room would go out of bounds, so we skip it
        }

        // Check if any cell in the room is already occupied
        for (int i = (int)start.x; i < (int)start.x + length; i++)
        {
            for (int j = (int)start.y; j < (int)start.y + width; j++)
            {
                if (mapCells[i, j].inRoom)
                {
                    flag = true;
                    break;
                }
            }
            if (flag) break;
        }

        if (flag) return; // Skip room generation if it's overlapping

        //========Create the room=======//

        for (int i = (int)start.x; i < (int)start.x + length; i++)
        {
            for (int j = (int)start.y; j < (int)start.y + width; j++)
            {
                mapCells[i, j].inRoom = true; // Mark cell as part of the room
            }
        }

        // Removing walls to create the room structure
        int x = (int)start.x;
        int y = (int)start.y;

        //========remove walls for bootom cells
        for (int l = x; l < x + length; l++)
        {
            RemoveWalls(mapCells[l, y], 0, 0, 0, 1);
        }
        for (int w = y; w < y + width; w++)
        {
            RemoveWalls(mapCells[x, w], 0, 0, 1, 0);
        }
        x = (int)start.x;
        y = (int)start.y + width - 1;
        for (int l = x; l < x + length; l++)
        {
            RemoveWalls(mapCells[l, y], 0, 1, 0, 0);
        }
        x = (int)start.x + length - 1;
        y = (int)start.y;
        for (int w = y; w < y + width; w++)
        {
            RemoveWalls(mapCells[x, w], 1, 0, 0, 0);
        }


        //========= remove centre walls ====//
        x = (int)start.x;
        y = (int)start.y;
        for (int l = x + 1; l < x + length - 1; l++)
        {
            for (int w = y + 1; w < y + width - 1; w++)
            {
                RemoveWalls(mapCells[l, w], 0, 0, 0, 0);
            }
        }


        //========= add corner walls ========//
        RemoveWalls(mapCells[x, y], 0, 0, 1, 1);
        RemoveWalls(mapCells[x + length - 1, y], 1, 0, 0, 1);
        RemoveWalls(mapCells[x, y + width - 1], 0, 1, 1, 0);
        RemoveWalls(mapCells[x + length - 1, y + width - 1], 1, 1, 0, 0);
    }





    private void GeneratePath()
    {
        foreach (var cell in InitialGateRooms)
        {
            Increment(cell);
        }
    }
    private void Increment(MapCell cell)
    {
        List<int> incrementDir = new List<int>();
        for (int i = 0; i < cell.wall.Length; i++)
        {
            if (cell.wall[i] == WallType.wall) 
                incrementDir.Add(i);
        }


    }
}

[System.Serializable]
public class MapCell
{
    public Vector3 position;
    public int width;
    public Vector2 id;
    public bool visited;
    public bool inRoom;


    // 0 => right , 1 => top , 2 => left , 3 => bottom

    public MapCell[] adjCell = new MapCell[4];

    public WallType[] wall = new WallType[4];

    public GameObject[] wallG = new GameObject[4];

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
            case 0: return position + new Vector3(halfSize, 1, 0);
            case 1: return position + new Vector3(0, 1, halfSize);
            case 2: return position - new Vector3(halfSize, -1, 0);
            case 3: return position - new Vector3(0, -1, halfSize);
            default: return position;
        }
    }

    public Quaternion GetWallRotation(int wallIndex)
    {
        switch (wallIndex)
        {
            case 0: return Quaternion.identity;
            case 1: return Quaternion.Euler(0, 90, 0);
            case 2: return Quaternion.Euler(0, 180, 0);
            case 3: return Quaternion.Euler(0, -90, 0);
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
