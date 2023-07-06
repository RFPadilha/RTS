using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

//All variables that will be active at all times, requiring them to be referenced in multiple scripts
public class Globals
{
    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static NavMeshSurface NAV_MESH_SURFACE;

    public static BuildingData[] BUILDING_DATA;

    public static int FLAT_TERRAIN_LAYER_MASK = 1 << 10;
    public static Dictionary<string, GameResource> GAME_RESOURCES = new Dictionary<string, GameResource>(){
        { "gold", new GameResource("Gold", 400) },
        { "wood", new GameResource("Wood", 400) },
        { "food", new GameResource("Food", 400) }
    };
    public static void UpdateNavMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }
}
