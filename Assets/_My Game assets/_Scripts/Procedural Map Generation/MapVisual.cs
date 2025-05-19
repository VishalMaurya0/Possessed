using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapVisual : NetworkBehaviour
{
    [Header("References")]
    public GenerateMap generateMap;
    public ProceduralMapDataSO proceduralMapDataSO;
    public GameObject wallContainer;
    public GameObject pillarContainer;
    public GameObject tileContainer;
    public GameObject propContainer;

    [Header("Extracted Values")]
    int rowCells;
    int columnCells;

    private void Start()
    {
        
    }

    ////======== For Building Blocks =======//

    public void GenerateBuildingBlocks()
    {
        rowCells = generateMap.mapCells.GetLength(0);
        columnCells = generateMap.mapCells.GetLength(1);

        for (int i = 0; i < rowCells; i++)
        {
            for (int j = 0; j < columnCells; j++)
            {
                MapCell cell = generateMap.mapCells[i, j];

                //======= wall spawning ======//
                for (int k = 0; k < cell.wall.Length; k++)
                {
                    if (cell.wall[k] == Type.NoWall)
                    {
                        if (cell.wallGTemporary[k] != null)
                        {
                            cell.wallGTemporary[k].GetComponent<NetworkObject>().Despawn();
                            Destroy(cell.WallGameobject[k]);
                            cell.wallGTemporary[k] = null;
                        }
                        continue;
                    }

                    // ==== making Gameobjects of wall, tile ==== //
                    List<PropsProbablity> wallPrefabsList = GetBuildingBlock(cell.wall[k]);

                    GameObject prefab = FindPrefabWithTheirProbablity(wallPrefabsList);



                    if (prefab == null)
                    {
                        continue;
                    }

                    if (cell.WallGameobject[k] == null)
                    {
                        GameObject newWall = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                        NetworkObject netObj = newWall.GetComponent<NetworkObject>();

                        // Spawn without setting the transform from prefab (we'll set it manually after parenting)
                        netObj.Spawn(true);

                        // Set parent via Netcode-safe API
                        netObj.TrySetParent(wallContainer.transform, false); // worldPositionStays = false for exact positioning

                        // Set transform
                        newWall.transform.position = cell.GetWallPosition(k, cell);
                        newWall.transform.rotation = cell.GetWallRotation(k);

                        newWall.SetActive(true);

                        cell.WallGameobject[k] = newWall;

                        // Sync opposite wall
                        int oppositeIndex = (k + 2) % 4;
                        if (cell.adjCell[k] != null && cell.adjCell[k].WallGameobject[oppositeIndex] == null)
                        {
                            cell.adjCell[k].WallGameobject[oppositeIndex] = newWall;
                        }
                    }
                }

                //======= pillars spawning ======//
                for (int k = 0; k < cell.wall.Length; k++)
                {
                    if (cell.pillar[k] == Type.NoPillar)
                    {
                        if (cell.PillarGameobject[k] != null)
                        {
                            Destroy(cell.PillarGameobject[k]);
                            cell.PillarGameobject[k] = null;
                        }
                        continue;
                    }

                    List<PropsProbablity> pillarPrefabsList = GetBuildingBlock(cell.pillar[k]);

                    GameObject prefab = FindPrefabWithTheirProbablity(pillarPrefabsList);



                    if (prefab == null)
                    {
                        continue;
                    }

                    if (cell.PillarGameobject[k] == null)
                    {
                        GameObject newPillar = Instantiate(prefab, pillarContainer.transform);
                        NetworkObject netobj = newPillar.GetComponent<NetworkObject>();
                        netobj.Spawn();
                        netobj.TrySetParent(pillarContainer.transform, false);
                        newPillar.transform.position = cell.GetPillarPosition(k, cell);
                        newPillar.transform.rotation = cell.GetPillarRotation(k);
                        newPillar.SetActive(true); // Ensure it's visible

                        cell.PillarGameobject[k] = newPillar;

                        // Sync opposite pillars
                        int adjIndex = k;
                        int oppositeIndex = (k + 1) % 4;
                        if (cell.adjCell[adjIndex] != null && cell.adjCell[adjIndex].PillarGameobject[oppositeIndex] == null)
                        {
                            cell.adjCell[adjIndex].PillarGameobject[oppositeIndex] = newPillar;
                        }

                        adjIndex = (k + 1) % 4;
                        oppositeIndex = (k + 3) % 4;
                        if (cell.adjCell[adjIndex] != null && cell.adjCell[adjIndex].PillarGameobject[oppositeIndex] == null)
                        {
                            cell.adjCell[adjIndex].PillarGameobject[oppositeIndex] = newPillar;
                        }

                        oppositeIndex = (k + 2) % 4;
                        if (cell.adjCell[adjIndex] != null && cell.adjCell[adjIndex].adjCell[k] != null && cell.adjCell[adjIndex].adjCell[k].PillarGameobject[oppositeIndex] == null)
                        {
                            cell.adjCell[adjIndex].adjCell[k].PillarGameobject[oppositeIndex] = newPillar;
                        }
                    }
                }

                //======= Tile Spawning ======//
                List<PropsProbablity> floorTilePrefabsList = GetBuildingBlock(cell.floorTile);

                GameObject newPrefab = FindPrefabWithTheirProbablity(floorTilePrefabsList);

                if (newPrefab != null)
                {
                    GameObject obj = Instantiate(newPrefab, tileContainer.transform);
                    NetworkObject netobj = obj.GetComponent<NetworkObject>();
                    netobj.Spawn();
                    netobj.TrySetParent(tileContainer.transform, false);
                    obj.transform.position = cell.position;
                    obj.name = $"Cell ({i}, {j})";
                    cell.FloorTileGameobject = obj;
                }
            }
        }
    }



    ////======== For Room Props =======//
    public void GenerateRoomProps()
    {
        rowCells = generateMap.mapCells.GetLength(0);
        columnCells = generateMap.mapCells.GetLength(1);

        for (int i = 0; i < rowCells; i++)
        {
            for (int j = 0; j < columnCells; j++)
            {
                MapCell cell = generateMap.mapCells[i, j];

                if (!cell.inRoom)
                    continue;

                GameObject propPrefab = FindPropGameObj(cell);
                if (propPrefab == null) continue;
                GameObject propGameobject = Instantiate(propPrefab, propContainer.transform);
                NetworkObject netobj = propGameobject.GetComponent<NetworkObject>();
                netobj.Spawn();
                netobj.TrySetParent(propContainer.transform, false);
                cell.PropGameobject = propGameobject;
                propGameobject.transform.position = cell.position;
                //TODO Rotation
                int iter = 0;
                for (int k = 0; k < cell.wall.Length; k++)
                {
                    if (cell.wall[k] == Type.Walls)
                    {
                        propGameobject.transform.Rotate(new Vector3(0, (k-iter)*(-90f), 0)); 
                        if (Random.Range(0, 2) == 0)
                        {
                            iter = 0;
                            break;
                        }
                        iter += k;
                    }
                }
            }
        }
    }

    private GameObject FindPropGameObj(MapCell cell)
    {
        Type prop = cell.prop;
        int roomType = generateMap.rooms[cell.roomID].roomType;

        if (prop == Type.NoProp)
            return null;


        AllProps allProps = GetRoomProps(roomType, prop);
        PropsVariation propsVariation = allProps.Props[Random.Range(0, allProps.Props.Count)];
        return FindPrefabWithTheirProbablity(propsVariation.Prop);
    }

    private GameObject FindPrefabWithTheirProbablity(List<PropsProbablity> PrefabsList)
    {
        if (PrefabsList == null || PrefabsList.Count <= 0)
        {
            return null;
        }


        int totalChances = 0;
        foreach (var prefab in PrefabsList)
        {
            totalChances += prefab.chancesIn100;
        }

        int random = Random.Range(0, totalChances);

        foreach (var prefab in PrefabsList)
        {
            random -= prefab.chancesIn100;
            if (random <= 0)
            {
                return prefab.prop;
            }
        }

        return null;
    }




    public List<PropsProbablity> GetBuildingBlock(Type type)
    {
        BuildingBlocks blocks = proceduralMapDataSO.MapMakingPrefabs.BuildingBlocks;

        switch (type)
        {
            case Type.Walls: return blocks.Walls;
            case Type.Windows: return blocks.Windows;
            case Type.Gates: return blocks.Gates;
            case Type.FloorTiles: return blocks.FloorTiles;
            case Type.RoomFloorTiles: return blocks.RoomFloorTiles;
            case Type.RoofTiles: return blocks.RoofTiles;
            case Type.RoomRoofTiles: return blocks.RoomRoofTiles;
            case Type.Pillar: return blocks.Pillars;
            default: return null;
        }
    }
    public AllProps GetRoomProps(int roomType, Type type)
    {
        RoomProps roomProps = proceduralMapDataSO.MapMakingPrefabs.RoomProps;

        switch (type)
        {
            case Type.WallSideProps: return roomProps.RoomTypes[roomType].WallSideProps;
            case Type.RoomCenterProps: return roomProps.RoomTypes[roomType].RoomCenterProps;
            case Type.RoomCornerProps: return roomProps.RoomTypes[roomType].RoomCornerProps;
            case Type.WindowSideProp: return roomProps.RoomTypes[roomType].WindowSideProp;
            case Type.CeilingProps: return roomProps.RoomTypes[roomType].CeilingProps;
            default:
                Debug.LogError("type not found");
                return default;
        }
    }
}