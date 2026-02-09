using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.RestService;
using UnityEngine;

//jess @ 12/12/2025

//<summary>
// this script defines the data structures used for saving and loading game data.
// when implementing a new feature that requires saving/loading, add the relevant data fields to these classes/ make a new class following the structure in the project.
// </summary>

[System.Serializable]
public class GameData
{
    public PlayerData player_data;
    public BuildingData building_data;
    public ResearchData research_data;
    public PathData path_data;
    public PathMap pathmap;

    public GameData()
    {
        player_data   = new PlayerData();
        building_data = new BuildingData();
        research_data = new ResearchData();
        path_data     = new PathData();
        pathmap       = new PathMap();
    }
}

// to be implemented (excluding camera state)
[System.Serializable]
public class PlayerData
{
    public int money;
    public float play_time;
    public string city_name;

    public int cheese;
    public int population;

    public camera_save_data camera_state;
}

// to be implemented
[System.Serializable]
public class BuildingData
{
    public List<building_save_data> buildings;
    public BuildingData()
    {
        buildings = new List<building_save_data>();
    }
}

[System.Serializable]
public class PathData
{
    public List<path_save_data> paths;
    public PathData()
    {
        paths = new List<path_save_data>();
    }
}

// to be implemented
[System.Serializable]
public struct building_save_data
{
    public string unique_id;
    public int prefab_index;
    public Vector3 position;
    public Quaternion rotation;
    public List<Vector2Int> occupied_cells;

    public int tier;
    public List<string> mouse_ids;
}

[System.Serializable]
public struct path_save_data
{
    public string unique_id;
    public int path_type_index;
    public Vector3 position;
    public Quaternion rotation;
    public float speed_modifier;
    public List<Vector2Int> occupied_cells;
}

[System.Serializable]
public struct camera_save_data
{
    public Vector3 target_position;
    public float yaw;
    public float pitch;
    public float zoom_distance;
}

// to be implemented when research feature is added
[System.Serializable]
public class ResearchData
{
    public List<string> completed_research = new List<string>();

    public float current_research_progress;
}

[System.Serializable]
public class PathMap
{
    public List<route_save_data> paths;
    public PathMap() 
    {
        paths = new List<route_save_data>();
    }
}

[System.Serializable]
public struct route_save_data
{
    public string key;
    public List<BaseNode> values; 
}