using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinding
{
    static GridManager grid_manager;
    public static GridManager Grid_manager { get { return grid_manager; } set { grid_manager = value; } }

    static int current_cost;
    static int shortest_path;
    static int chunk;

    static PathFinding()
    {
        current_cost = 0;
        shortest_path = 0;
        chunk = 16;
    }

    static Vector2Int FindChunk(Vector2Int loc)
    {
        Vector2Int rv = new Vector2Int();

        rv.x = loc.x % chunk;
        rv.y = loc.y % chunk;

        return rv;
    }

    public static void Pathfinding(MouseTemp mice, Vector2Int building)
    {
        //mark the distance of every tile to the whole
        //if encounter wall recaulate
        //fllow path of least numbers

        //Calculating the size of the map to search
        Vector2Int start = FindChunk(mice.Postion);
        Vector2Int end   = FindChunk(building);

        float min = Mathf.Min(start.sqrMagnitude, end.sqrMagnitude);
        Vector2Int low = min == start.magnitude ? mice.Postion : building;
        Vector2Int max = min != start.magnitude ? mice.Postion : building;

        Vector2Int size = max - low;
        PathNode[,] costs = new PathNode[size.x, size.y];

        for (int x = low.x; x < max.x; x++)
        {
            for (int y = low.y; y < max.y; y++)
            {
                costs[x, y] = new PathNode(new Vector2Int(x,y), grid_manager);
            }
        }
    }
}