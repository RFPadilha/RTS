using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All variables that will be active at all times, requiring them to be referenced in multiple scripts
public class Globals
{
    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static BuildingData[] BUILDING_DATA;
    public static Dictionary<string, GameResource> GAME_RESOURCES = new Dictionary<string, GameResource>(){
        { "gold", new GameResource("Gold", 400) },
        { "wood", new GameResource("Wood", 400) },
        { "food", new GameResource("Food", 400) }
    };
}
