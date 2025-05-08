using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Room;

public class GenerateMap : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public MapVisualTemp mapVisualTemp;
    public MapVisual mapVisual;
    public ProceduralMapDataSO mapVisualDataSO;


    [Header("Inputs")]
    public GameObject CellObj;
    public int totalRooms;
    public int roomMinLength;
    public int roomMaxLength;
    public int max3X3Rooms;
    public int current3X3Rooms;
    public int typeOfRooms;
    public bool generateAgain = false;
    public bool generatePathAlso = true;
    public bool generateActual;
    public bool generateTemp;


    [Header("Map Properties")]
    public MapCell[,] mapCells;
    public MapCell centreMapCell;
    public List<MapCell> InitialGateRooms = new ();
    public List<Room> rooms = new ();
    readonly List<MapCell> roomGates = new ();
    readonly List<MapCell> newIncrements = new ();





    private void OnValidate()
    {
        generateAgain = true;
    }


    private void Start()
    {
        typeOfRooms = mapVisualDataSO.typeOfRooms;
        generateAgain = false;
        mapVisualTemp = GetComponent<MapVisualTemp>();
        mapVisual = GetComponent<MapVisual>();
        mapVisualTemp.InitializeWallPool();        //Pooling the wallls Genereated==//
        grid = GetComponent<Grid>();
        CreateInitialMap();          //===== generates the mapCells according to grid ======//
        ReferenceAdjacentCells();
        StartGeneration();
    }
    private void Update()
    {
        if (generateAgain)
        {
            generateAgain = false;
            GenerateAgain();
        }
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
        CentreGeneration();               //======= 3x3 centre generated =====//
        ChooseGate();                        //======= gate are chosen in all direction in the center randomly ========//
        GenerateRooms(roomMinLength, roomMaxLength, totalRooms);    //======= generate the rooms =========//
        if (generatePathAlso)
        {
            GeneratePath();
        }
        CreatePillars();
        CreateTiles();
        CreateWindows();
        SpawnProcedures();
        GenerateProps();



        ////======== Visuals & NavMesh ========////
        if (generateTemp)
        {
            mapVisualTemp.UpdateVisual();
        }
        if (generateActual)
        {
            mapVisual.GenerateBuildingBlocks();
            mapVisual.GenerateRoomProps();
        }
        GameManager.Instance.bakeNavMeshAgain = true;
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

                if (i < 1) cell.wall[0] = Type.NoWall; // Right
                if (i > -1) cell.wall[2] = Type.NoWall; // Left
                if (j > -1) cell.wall[3] = Type.NoWall; // Top
                if (j < 1) cell.wall[1] = Type.NoWall; // Bottom

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

        System.Random rand = new ();

        for (int i = -1; i < 2; i += 2)   ///choosing direction Left and Right
        {
            int j = rand.Next(-1, 2);   // -1 , 0, 1 
            MapCell cell = mapCells[i + centerRow, j + centerCol];
            switch (i)
            {
                case -1: RemoveWalls(cell, -1, -1, 2, -1); InitialGateRooms.Add(cell.adjCell[2]); cell.adjCell[2].inRoom = true; break;
                case 1: RemoveWalls(cell, 2, -1, -1, -1); InitialGateRooms.Add(cell.adjCell[0]); cell.adjCell[0].inRoom = true; break;
            }

        }

        for (int i = -1; i < 2; i += 2)   ///choosing direction North and south
        {
            int j = rand.Next(-1, 2);   // -1 , 0, 1 
            MapCell cell = mapCells[j + centerRow, i + centerCol];
            switch (i)
            {
                case -1: RemoveWalls(cell, -1, -1, -1, 2); InitialGateRooms.Add(cell.adjCell[3]); cell.adjCell[3].inRoom = true; break;
                case 1: RemoveWalls(cell, -1, 2, -1, -1); InitialGateRooms.Add(cell.adjCell[1]); cell.adjCell[1].inRoom = true; break;
            }
        }
    }





    private void GenerateRooms(int roomMinLength, int roomMaxLength, int totalRooms)
    {
        int totalLoops = totalRooms * 500;
        int currentLoop = 0;
        while (rooms.Count < totalRooms && currentLoop < totalLoops)
        {
            CreateARoom(roomMinLength, roomMaxLength);
            currentLoop++;
        }


        //===== Removing boundaries ====//
        foreach (MapCell cell in InitialGateRooms)
        {
            cell.inRoom = false;
        }

        //======== Removing Outer boundary for MapCells grid where room should not spawn ======//
        int rows = mapCells.GetLength(0);
        int cols = mapCells.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // Check if it's an outer cell (first/last row or first/last column)
                if (i == 0 || i == rows - 1 || j == 0 || j == cols - 1)
                {
                    mapCells[i, j].inRoom = false;
                }
            }
        }

        Debug.LogWarning($"totalRooms = {rooms.Count}");
    }
    private void CreateARoom(int roomMinLength, int roomMaxLength)
    {
        int length = Random.Range(roomMinLength, roomMaxLength + 1);
        int width = Random.Range(roomMinLength, roomMaxLength + 1);


        //======= check for no of 3x3 rooms =========//
        if (current3X3Rooms >= max3X3Rooms)
        {
            if (length <= 3)
            {
                length++;
            }
            if (width <= 3)
            {
                width++;
            }
        }

        Vector2 start = new (Random.Range(0, grid.length), Random.Range(0, grid.width));

        bool flag = false;

        // Ensure the room fits within the grid
        if (start.x + length > mapCells.GetLength(0) || start.y + width > mapCells.GetLength(1))
        {
            return; // Room would go out of bounds, so we skip it
        }

        //======== Create Outer boundary for MapCells grid where rooms should not spawn ======//
        int rows = mapCells.GetLength(0);
        int cols = mapCells.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // Mark only the outermost layer (one row/column on each side)
                if (i == 0 || i == rows - 1 || j == 0 || j == cols - 1)
                {
                    mapCells[i, j].inRoom = true;
                }
            }
        }


        // Check if any cell in the room is already occupied
        for (int i = (int)start.x - 1; i < (int)start.x + length + 1; i++)
        {
            for (int j = (int)start.y - 1; j < (int)start.y + width + 1; j++)
            {
                if (i >= 0 && i < mapCells.GetLength(0) && j >= 0 && j < mapCells.GetLength(1))
                {
                    if (mapCells[i, j].inRoom)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag) break;
        }

        if (flag)
        {
            return; // Skip room generation if it's overlapping
        }

        //========Create the room=======//


        if (length <= 3 || width <= 3)
        {
            current3X3Rooms++;
        }

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

        //===========Make A Random Gate==========//

        int gateIndexX = Random.Range(x, x + length);
        int gateIndexY = Random.Range(y, y + width);
        MapCell gateCell = null;
        int gateDir = -1;

        switch (Random.Range(0, 4))
        {
            case 0: RemoveWalls(mapCells[x + length - 1, gateIndexY], 2, -1, -1, -1); gateCell = mapCells[x + length - 1, gateIndexY]; gateDir = 0; break;
            case 1: RemoveWalls(mapCells[gateIndexX, y + width - 1], -1, 2, -1, -1); gateCell = mapCells[gateIndexX, y + width - 1]; gateDir = 1; break;
            case 2: RemoveWalls(mapCells[x, gateIndexY], -1, -1, 2, -1); gateCell = mapCells[x, gateIndexY]; gateDir = 2; break;
            case 3: RemoveWalls(mapCells[gateIndexX, y], -1, -1, -1, 2); gateCell = mapCells[gateIndexX, y]; gateDir = 3; break;
        }


        // ========= Store It =====//
        Room newRoom = new (start, length, width, gateCell, gateDir, this);
        newRoom.roomType = Random.Range(0, typeOfRooms);

        // ======== store corner cells =======//
        newRoom.cornercells[0] = mapCells[x + length - 1, y + width - 1];
        newRoom.cornercells[1] = mapCells[x, y + width - 1];
        newRoom.cornercells[2] = mapCells[x, y];
        newRoom.cornercells[3] = mapCells[x + length - 1, y];

        rooms.Add(newRoom);

        for (int i = (int)start.x; i < (int)start.x + length; i++)
        {
            for (int j = (int)start.y; j < (int)start.y + width; j++)
            {
                mapCells[i, j].roomID = rooms.Count - 1; // Reference room to the cells
            }
        }
    }





    private void GeneratePath()
    {
        if (InitialGateRooms == null || InitialGateRooms.Count == 0)
        {
            return;
        }

        Increment(InitialGateRooms[0]);


        while (newIncrements.Count > 0)
        {
            MapCell cell = newIncrements[Random.Range(0, newIncrements.Count)];
            Increment(cell);
        }
        
        foreach (Room room in rooms)
        {
            roomGates.Add(room.gateCell.adjCell[room.gateDir]);
        }

        //while (roomGates.Count > 0)
        //{
        //    MapCell mapCell = roomGates[Random.Range(0, roomGates.Count)];
        //    Increment(mapCell, roomGates, true);
        //}
    }
    private void Increment(MapCell cell, List<MapCell> newIncrementsInside = null, bool forced = false)
    {
        if (cell == null)
        {
            return;
        }
        if (newIncrementsInside == null) 
            newIncrementsInside = newIncrements;


        int initialDir = -1;
        List<int> incrementDir = new ();

        //====== Get all Wall Directions =====//
        for (int i = 0; i < cell.wall.Length; i++)
        {
            if (cell.wall[i] == Type.Walls)
            {
                incrementDir.Add(i);
            }

            if (cell.wall[i] == Type.NoWall && initialDir == -1)
            {
                initialDir = (i + 2) % 4;
            }

            if (cell.wall[i] == Type.Gates && initialDir == -1)
            {
                initialDir = (i + 2) % 4;
            }
        }

        //============ if no wall direction found =======//
        if (incrementDir.Count == 0)
        {
            if (newIncrementsInside.Contains(cell))
            {
                newIncrementsInside.Remove(cell);
            }
            return;
        }

        //////////============= select a random dir all found directions ==============////////////////
        int SelectRandomDir()                   
        {
            // ======= if no direction left ===========//
            if (incrementDir.Count == 0)
            {

                if (newIncrementsInside.Contains(cell))
                {
                    newIncrementsInside.Remove(cell);
                }

                return -1;
            }    


            int randomIndex = Random.Range(0, incrementDir.Count);
            int randomDir = incrementDir[randomIndex];
            incrementDir.RemoveAt(randomIndex);

            return randomDir;
        }           

        int randomDir = SelectRandomDir();

        if (!forced)
        {
            while
                (cell.adjCell[randomDir] == null
                || cell.adjCell[randomDir].visited
                || cell.adjCell[randomDir].inRoom)
            {
                randomDir = SelectRandomDir();
                if (randomDir == -1)
                {
                    if (newIncrementsInside.Contains(cell))
                    {
                        newIncrementsInside.Remove(cell);
                    }
                    return;
                }
            }
        }else
        {
            while
                (cell.adjCell[randomDir] == null
                || cell.adjCell[randomDir].visited)
            {
                randomDir = SelectRandomDir();
                if (randomDir == -1)
                {
                    if (newIncrementsInside.Contains(cell))
                    {
                        newIncrementsInside.Remove(cell);
                    }
                    return;
                }
            }
        }

        cell.wall[randomDir] = Type.NoWall;
        //if (forced && ran)

        int oppositeDir = (randomDir + 2) % 4;

        if (cell.adjCell[randomDir] != null)
        {
            cell.adjCell[randomDir].wall[oppositeDir] = Type.NoWall;
            cell.adjCell[randomDir].visited = true;
            cell.visited = true;



            //==== Save Cell if Turned =====//
            if (initialDir != randomDir && !newIncrementsInside.Contains(cell))
            {
                bool addCell = false;

                for (int i = 0; i < cell.wall.Length; i++)
                {
                    // Ensure adjCell is valid before accessing
                    if (cell.adjCell != null && i < cell.adjCell.Length && cell.adjCell[i] != null)
                    {
                        if (!cell.adjCell[i].visited)
                        {
                            addCell = true;
                            break;
                        }
                    }
                }
                if (addCell)
                {
                    newIncrementsInside.Add(cell);
                }
            }




            Increment(cell.adjCell[randomDir], newIncrementsInside, forced);
            
            
        }
    }






    private void CreatePillars()
    {
        int rowCells = mapCells.GetLength(0);
        int columnCells = mapCells.GetLength(1);

        for (int i = 0; i < rowCells; i++)
        {
            for (int j = 0; j < columnCells; j++)
            {
                // ======= every Cell =-=====//
                MapCell cell = mapCells[i, j];


                for (int k = 0; k < cell.wall.Length; k ++)
                {
                    // ========= every wall ========//
                    
                    Type wallType = cell.wall[k];

                    if (wallType != Type.NoWall)
                    {
                        int l = k + 1;
                        l = l % 4;
                        // ====== if Some Wall is present ====== //
                        if (cell.wall[l] == Type.NoWall
                            && (cell.adjCell[k] == null || cell.adjCell[k].wall[l] == Type.NoWall)
                            && (cell.adjCell[l] == null || cell.adjCell[l].wall[k] != Type.NoWall))
                        {
                            // ====== if No Wall is present at positive and neg adjacent but wall is present at top adj (No pillar) ====== //
                            cell.pillar[k] = Type.NoPillar;
                        }

                        l = k + 3; 
                        l = l % 4;
                        // ====== if Some Wall is present ====== //
                        if (cell.wall[l] == Type.NoWall
                            && (cell.adjCell[k] == null || cell.adjCell[k].wall[l] == Type.NoWall)
                            && (cell.adjCell[l] == null || cell.adjCell[l].wall[k] != Type.NoWall))
                        {
                            // ====== if No Wall is present at positive and neg adjacent but wall is present at top adj (No pillar) ====== //
                            cell.pillar[l] = Type.NoPillar;
                        }

                        if (cell.pillar[k] != Type.NoPillar)
                        {
                            cell.pillar[k] = Type.Pillar;
                        }
                            //Vector3 pillarPosK = WallGameobject.transform.position + new Vector3(0, grid.cellLength / 2, 0);
                        //Vector3 pillarPosL = WallGameobject.transform.position - new Vector3(0, grid.cellLength / 2, 0);
                    }

                }
            }
        }
    }
    private void CreateTiles()
    {
        int rowCells = mapCells.GetLength(0);
        int columnCells = mapCells.GetLength(1);

        for (int i = 0; i < rowCells; i++)
        {
            for (int j = 0; j < columnCells; j++)
            {
                // ======= every Cell =-=====//
                MapCell cell = mapCells[i, j];
                if (cell == null) continue;

                if (cell.inRoom)
                {
                    cell.floorTile = Type.RoomFloorTiles;
                    cell.roofTile = Type.RoomRoofTiles;
                }
                else
                {
                    cell.floorTile = Type.FloorTiles;
                    cell.roofTile = Type.RoofTiles;
                }
            }
        }

    }
    private void CreateWindows()
    {
        for (int i = 0; i < rooms.Count; i++)
        {

            //==Room==//
            for (int j = 0; j < rooms[i].BoundaryCells.Count; j++)
            {
                //==BoundaryWall==//
                MapCell[] boundaryCell = rooms[i].BoundaryCells[j];

                //=========== check for Gates Presence =======//
                bool flag = false;
                for (int k = 0; k < boundaryCell.Length; k++)
                {
                    if (boundaryCell[k].wall[rooms[i].gateDir] == Type.Gates)
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    continue;
                }

                //======= if Gate is Not Present =======//
                if (boundaryCell.Length <= 2)
                {
                    continue;
                }

                int windowIndex = Random.Range(1, boundaryCell.Length - 1);   //==== not choosing corner cells =====//
                MapCell targetCell = boundaryCell[windowIndex];
                
                for (int k = 0; k < targetCell.wall.Length; k++)
                {
                    if (targetCell.wall[k] != Type.NoWall)
                    {
                        targetCell.wall[k] = Type.Windows;
                        int oppIndex = (k + 2) % 4;
                        targetCell.adjCell[k].wall[oppIndex] = Type.Windows;
                        break;
                    }
                }
            }
        }
    }







    private void SpawnProcedures(int spawnAttempts = 0)
    {
        //========= All Rooms =========//

        List<Room> allRooms = new ();
        foreach (Room room in rooms)
        {
            allRooms.Add(room);
        }

        //========= selected 8 rooms =========//

        List<Room> selectedRooms = new List<Room>();
        int noOfTrials = 0;
        int totalTrials = 1000;
        while (selectedRooms.Count < 8 && noOfTrials < totalTrials)
        {
            noOfTrials++;
            int index = Random.Range(0, allRooms.Count);
            if (allRooms[index].length <= 3 || allRooms[index].width <= 3)
            {
                continue;
            }
            selectedRooms.Add(allRooms[index]);
            allRooms.RemoveAt(index);
        }

        //========= all procedures =========//

        List<Procedures> procedures = new ();                                  //TODO champt Gpt//
        foreach (string name in System.Enum.GetNames(typeof(Procedures)))
        {
            procedures.Add((Procedures)System.Enum.Parse(typeof(Procedures), name));
        }

        //========= Assign each procedure to a room =========//

        int maxAttempts = 1000;
        int attempts = 0;

        while (procedures.Count > 0 && attempts < maxAttempts)
        {
            attempts++;

            int roomIndex = Random.Range(0, selectedRooms.Count);
            int procedureIndex = Random.Range(0, procedures.Count);

            if (IsCompatibleToSpawnAProcedureAndSpawnIt(selectedRooms[roomIndex], procedureIndex))
            {
                procedures.RemoveAt(procedureIndex);
            }
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogError("Failed to place all procedures after maximum attempts.");
        


            if (procedures.Count > 0)
            {
                foreach (Room room in selectedRooms)
                {
                    if (room.Procedures.Count > 0)
                    {
                        foreach (ProcedureLocation procedure in room.Procedures.ToList())
                        {
                            foreach (MapCell cell in procedure.cell)
                            {
                                cell.spawnNoProcedures = false;
                                cell.spaceOccupied = false;
                            }
                            room.Procedures.Remove(procedure);
                        }
                    }
                }

                if (spawnAttempts < 5)
                    SpawnProcedures(spawnAttempts + 1);
            }
        }



        bool IsCompatibleToSpawnAProcedureAndSpawnIt(Room room, int procedureIndex)
        {
            int i = 5000;
            bool posFound = false;
            Vector2 pos = new ();


            while (i > 0 && !posFound)
            {
                i--;
                pos = new Vector2(Random.Range((int)room.start.x, (int)room.start.x + room.length), Random.Range((int)room.start.y, (int)room.start.y + room.width));

                bool flag = false;
                if (mapCells[(int)pos.x, (int)pos.y].spawnNoProcedures) flag = true;
                if (mapCells[(int)pos.x + 1, (int)pos.y].spawnNoProcedures) flag = true;
                if (mapCells[(int)pos.x + 1, (int)pos.y + 1].spawnNoProcedures) flag = true;
                if (mapCells[(int)pos.x, (int)pos.y + 1].spawnNoProcedures) flag = true; 

                if (!flag)
                {
                    posFound = true;
                }

            }

            if (!posFound)
            {
                return false;
            }


            Room.ProcedureLocation procedureLocation = new ();
            procedureLocation.Procedure = procedures[procedureIndex];
            procedureLocation.cell = new ();
            procedureLocation.cell.Add(mapCells[(int)pos.x, (int)pos.y]);
            procedureLocation.cell.Add(mapCells[(int)pos.x + 1, (int)pos.y]);
            procedureLocation.cell.Add(mapCells[(int)pos.x + 1, (int)pos.y + 1]);
            procedureLocation.cell.Add(mapCells[(int)pos.x, (int)pos.y + 1]);

            foreach (var cell in procedureLocation.cell)
            {
                cell.spawnNoProcedures = true;
                cell.spaceOccupied = true;
            }

            foreach (var procedure in GameManager.Instance.AllProcedures)
            {
                if (procedure.procedureData.procedure == procedureLocation.Procedure)
                {
                    procedureLocation.ProcedureCompletion = procedure;
                    procedureLocation.ProcedureCompletion.procedurePrefab = procedure.gameObject;
                    SpawnProcedure(procedureLocation);
                }
            }

            room.Procedures.Add(procedureLocation);


            return true;
        }

        void SpawnProcedure(ProcedureLocation procedureLocation)
        {
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < procedureLocation.cell.Count; i++)
            {
                pos.x += procedureLocation.cell[i].position.x;
                pos.z += procedureLocation.cell[i].position.z;
            }
            pos = pos / 4;

            procedureLocation.ProcedureCompletion.procedurePrefab.transform.position = pos;
            //TODO Rotation//
        }
    }
    private void GenerateProps()
    {
        GenerateCornerProps();
        GenerateWallSideProps();
        GenerateWindowSideProps();
    }

    private void GenerateCornerProps()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            //====== Generate at corners =====//

            List<MapCell> selectedCornerCells = new();
            for (int j = 0; j < room.cornercells.Length; j++)
            {
                selectedCornerCells.Add(room.cornercells[j]);
            }

            //====select only 2-4 of them===//

            int remove = Random.Range(0, 3);
            for (int j = 0; j < remove; j++)
            {
                selectedCornerCells.RemoveAt(Random.Range(0, selectedCornerCells.Count));
            }



            //========= Check for Gate or Window Cell ========//
            HashSet<int> removeCell = new();
            for (int a = 0; a < selectedCornerCells.Count; a++)
            {
                for (int j = 0; j < selectedCornerCells[a].wall.Length; j++)
                {
                    if (selectedCornerCells[a].wall[j] != Type.NoWall && selectedCornerCells[a].wall[j] != Type.Walls)
                    {
                        removeCell.Add(a);
                        break; // No need to check other walls for this cell
                    }
                }
            }

            // Remove from the end to avoid shifting issues
            List<int> sortedRemove = removeCell.OrderByDescending(i => i).ToList();
            foreach (int index in sortedRemove)
            {
                selectedCornerCells.RemoveAt(index);
            }




            //====== Now Spawn =========//
            foreach (var cell in selectedCornerCells)
            {
                if (cell.spaceOccupied)
                    continue;

                cell.prop = Type.RoomCornerProps;
                cell.spaceOccupied = true;
            }
        }
    }
    private void GenerateWallSideProps()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            List<MapCell> selectedEdgeCells = new();


            //====== Generate at edges =====//
            for (int j = 0; j < room.BoundaryCells.Count; j++)
            {
                for (int k = 0; k < room.BoundaryCells[j].Length; k++)
                {
                    MapCell cell = room.BoundaryCells[j][k];
                    selectedEdgeCells.Add(cell);
                }
            }


            if (selectedEdgeCells.Count == 0)
            {
                Debug.LogWarning($"Room {i} has no edge cells selected. Skipping wall prop generation.");
                continue;
            }

            //========= Check for Gate or Window Cell, Corner Cell, or Occupied Cell ========//
            HashSet<int> removeCell = new();
            for (int a = 0; a < selectedEdgeCells.Count; a++)
            {
                for (int j = 0; j < selectedEdgeCells[a].wall.Length; j++)
                {
                    if (selectedEdgeCells[a].wall[j] != Type.NoWall && selectedEdgeCells[a].wall[j] != Type.Walls)
                    {
                        removeCell.Add(a);
                        break;
                    }
                }

                if (room.cornercells.Contains(selectedEdgeCells[a]))
                {
                    removeCell.Add(a);
                }

                if (selectedEdgeCells[a].spaceOccupied)
                {
                    removeCell.Add(a);
                }
            }

            // Sort in descending order to safely remove
            List<int> sortedIndices = removeCell.OrderByDescending(i => i).ToList();
            foreach (int index in sortedIndices)
            {
                selectedEdgeCells.RemoveAt(index);
            }

            if (selectedEdgeCells.Count == 0)
            {
                Debug.LogWarning($"Room {i} has no valid edge cells after filtering. No props placed.");
                continue;
            }


            //====== Randomly deselect 1/3rd ========//
            int removeCells = selectedEdgeCells.Count / 3;
            for (int j = 0; j < removeCells; j++)
            {
                selectedEdgeCells.RemoveAt(Random.Range(0, selectedEdgeCells.Count));
            }

            //========= Assign Props =========//
            foreach (var cell in selectedEdgeCells)
            {
                cell.prop = Type.WallSideProps;
                cell.spaceOccupied = true;
            }
        }
    }
    private void GenerateWindowSideProps()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            List<MapCell> selectedCells = new();


            for (int j = (int)room.start.x; j < room.start.x + room.length; j++)
            {
                for (int k = (int)room.start.y; k < room.start.y + room.width; k++)
                {
                    MapCell cell = mapCells[j, k];

                    for (int l = 0; l < cell.wall.Length; l++)
                    {
                        if (cell.wall[l] == Type.Windows)
                        {
                            selectedCells.Add(cell);
                            break;
                        }
                    }
                }
            }


            if (selectedCells.Count == 0)
            {
                Debug.LogWarning($"Room {i} has no cells selected. Skipping window prop generation.");
                continue;
            }


            //====== Randomly deselect 1 in a 50% chance ========//
            if (Random.Range(0, 2) == 0)
                selectedCells.RemoveAt(Random.Range(0, selectedCells.Count));


            //========= Assign Props =========//
            foreach (var cell in selectedCells)
            {
                cell.prop = Type.WindowSideProp;
                cell.spaceOccupied = true;
            }
        }
    }








    void GenerateAgain()
    {
        // Clear data structures
        InitialGateRooms.Clear();
        rooms.Clear();
        newIncrements.Clear();
        roomGates.Clear();

        // Return all walls to pool and reset cell references
        int rows = mapCells.GetLength(0);
        int cols = mapCells.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                MapCell cell = mapCells[i, j];
                // Return walls to pool
                for (int k = 0; k < mapCells[i, j].wallGTemporary.Length; k++)
                {
                    cell.wall[k] = Type.Walls;
                    if (cell.wallGTemporary[k] != null)
                    {
                        mapVisualTemp.ReturnWallToPool(cell.wallGTemporary[k]);
                        cell.wallGTemporary[k] = null;


                        int oppositeIndex = (k + 2) % 4; // 0↔2 (right-left), 1↔3 (top-bottom)
                        if (cell.adjCell[k] != null)
                            cell.adjCell[k].wallGTemporary[oppositeIndex] = null;
                    }
                }
                // Reset cell state
                cell.inRoom = false;
                cell.visited = false;
            }
        }
                        


        // Reinitialize the map
        CentreGeneration();
        ChooseGate();
        GenerateRooms(roomMinLength, roomMaxLength, totalRooms);

        if (generatePathAlso)
        {
            GeneratePath();
        }

        if (generateTemp)
        {
            mapVisualTemp.UpdateVisual();
        }

        GameManager.Instance.bakeNavMeshAgain = true;
    }



    /////////=========== function for removing and adding walls ==========//
    void RemoveWalls(MapCell cell, int right, int top, int left, int bottom)
    {
        int[] directions = { right, top, left, bottom };

        for (int i = 0; i < 4; i++)
        {
            if (directions[i] > -1)
            {
                switch (directions[i])
                {
                    case 0: cell.wall[i] = Type.NoWall; break;
                    case 1: cell.wall[i] = Type.Walls; break;
                    case 2: cell.wall[i] = Type.Gates; break;
                    case 3: cell.wall[i] = Type.Windows; break;
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
}

[System.Serializable]
public class MapCell
{
    public Vector3 position;
    public int width;
    public Vector2 id;
    public bool visited;
    public bool inRoom;
    public int roomID;


    // 0 => right , 1 => top , 2 => left , 3 => bottom
    [System.NonSerialized]
    public MapCell[] adjCell = new MapCell[4];

    public Type[] wall = new Type[4];

    public Type[] pillar = new Type[4];

    public Type floorTile;

    public Type roofTile;

    public Type prop;

    public bool spawnNoProcedures;
    public bool spaceOccupied;

    /// <summary>
    /// /========== All GameObjects In A Cell ============= ///
    /// </summary>

    public GameObject[] WallGameobject = new GameObject[4];

    public GameObject[] PillarGameobject = new GameObject[4];

    public GameObject FloorTileGameobject;

    public GameObject RoofTileGameobject;

    public GameObject PropGameobject;
    
    //********************** Temporary ************************//

    public GameObject cellObjectTemporary;
    public GameObject[] wallGTemporary = new GameObject[4];
    
    //********************** Temporary ************************//


    public MapCell(Cell cell, GameObject cellObject = null)
    {
        position = cell.position;
        width = cell.width;
        id.x = cell.position.x / 2;
        id.y = cell.position.z / 2;
        for (int i = 0; i < wall.Length; i++)
        {
            wall[i] = Type.Walls;
        }
        cellObjectTemporary = cellObject;
    }
    public Vector3 GetWallPosition(int wallIndex, MapCell cell)
    {
        Vector3 position = cell.position;
        float halfSize = cell.width / 2f;

        return (wallIndex) switch
        {
            0 => position + new Vector3(halfSize, 0, 0),
            1 => position + new Vector3(0, 0, halfSize),
            2 => position - new Vector3(halfSize, 0, 0),
            3 => position - new Vector3(0, 0, halfSize),
            _ => position,
        };
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

    public Vector3 GetPillarPosition(int pillarIndex, MapCell cell)
    {
        Vector3 pillarPos0 = cell.position + new Vector3(width / 2, 0, width / 2);
        Vector3 pillarPos1 = cell.position + new Vector3((-1) * width / 2, 0, width / 2);
        Vector3 pillarPos2 = cell.position - new Vector3(width / 2, 0, width / 2);
        Vector3 pillarPos3 = cell.position - new Vector3((-1) * width / 2, 0, width / 2);

        switch (pillarIndex)
        {
            case 0: return pillarPos0;
            case 1: return pillarPos1;
            case 2: return pillarPos2;
            case 3: return pillarPos3;
        }
        return pillarPos0;
    }

    public Quaternion GetPillarRotation(int wallIndex)
    {
        float randomRotaionY = new float[] { 0f, 90f, 180f, -90f }[Random.Range(0, 4)];

        return Quaternion.Euler(new Vector3(0,randomRotaionY,0));
    }
}

[System.Serializable]
public class Room
{
    public Vector2 start;
    public int length;
    public int width;
    [System.NonSerialized] public MapCell gateCell;
    public int gateDir;
    public int roomType;


    [System.NonSerialized] public MapCell[] rightCells;
    [System.NonSerialized] public MapCell[] topCells;
    [System.NonSerialized] public MapCell[] leftCells;
    [System.NonSerialized] public MapCell[] bottomCells;
    [System.NonSerialized] public List<MapCell[]> BoundaryCells = new();


    [System.NonSerialized] public MapCell[] cornercells = new MapCell[4];


    public List<ProcedureLocation> Procedures = new List<ProcedureLocation>();


    public Room(Vector2 start, int length, int width, MapCell gateCell, int gateDir, GenerateMap generateMap)
    {
        this.start = start;
        this.length = length;
        this.width = width;
        this.gateCell = gateCell;
        this.gateDir = gateDir;
        rightCells = new MapCell[width];
        topCells = new MapCell[length];
        leftCells = new MapCell[width];
        bottomCells = new MapCell[length];

        InitializeCells(generateMap);

        MarkSpawnProofArea(generateMap);
    }

    private void MarkSpawnProofArea(GenerateMap generateMap)
    {
        for (int i = (int)start.x; i < (int)start.x + length; i++)
        {
            generateMap.mapCells[i, (int)start.y].spawnNoProcedures = true;
            generateMap.mapCells[i, (int)start.y + width - 1].spawnNoProcedures = true;
        }
        
        for (int i = (int)start.y; i < (int)start.y + width; i++)
        {
            generateMap.mapCells[(int)start.x, i].spawnNoProcedures = true;
            generateMap.mapCells[(int)start.x + length - 1, i].spawnNoProcedures = true;
        }
    }

    private void InitializeCells(GenerateMap generateMap)
    {
        for (int i = 0; i < rightCells.Length; i++)
        {
            rightCells[i] = generateMap.mapCells[(int)start.x + length - 1, (int)start.y + i];
            leftCells[i] = generateMap.mapCells[(int)start.x, (int)start.y + i];
        }
        
        for (int i = 0; i < topCells.Length; i++)
        {
            topCells[i] = generateMap.mapCells[(int)start.x + i, (int)start.y + width - 1];
            bottomCells[i] = generateMap.mapCells[(int)start.x + i, (int)start.y];
        }

        BoundaryCells.Add(rightCells);
        BoundaryCells.Add(topCells);
        BoundaryCells.Add(leftCells);
        BoundaryCells.Add(bottomCells);
        
    }

    [System.Serializable]
    public struct ProcedureLocation
    {
        public Procedures Procedure;
        public ProcedureCompletion ProcedureCompletion;
        [System.NonSerialized] public List<MapCell> cell;
    }
}
