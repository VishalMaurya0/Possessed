using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProceduralMapDataSO", menuName = "Scriptable Objects/ProceduralMapDataSO")]
public class ProceduralMapDataSO : ScriptableObject
{
    public MapMakingPrefabs MapMakingPrefabs;
}

[System.Serializable]
public class MapMakingPrefabs
{
    public BuildingBlocks BuildingBlocks;
    public RoomProps RoomProps;
}

[System.Serializable]
public struct BuildingBlocks
{
    public List<PropsProbablity> Walls;
    public List<PropsProbablity> Windows;
    public List<PropsProbablity> Gates;
    public List<PropsProbablity> FloorTiles;
    public List<PropsProbablity> RoomFloorTiles;
    public List<PropsProbablity> RoofTiles;
    public List<PropsProbablity> RoomRoofTiles;
    public List<PropsProbablity> Pillars;
}

[System.Serializable]
public struct RoomProps
{
    public List<PropsProbablity> WallSideProps;
    public List<PropsProbablity> RoomCenterProps;
    public List<PropsProbablity> CeilingProps;
}

[System.Serializable]
public struct PropsProbablity
{
    public GameObject prop;
    public int chancesIn100;
}

public enum Type
{
    NoWall,
    Walls,
    Windows,
    Gates,
    FloorTiles,
    RoomFloorTiles,
    RoofTiles,
    RoomRoofTiles,
    NoPillar,
    Pillar,
    WallSideProps,
    RoomCenterProps,
    CeilingProps,
}