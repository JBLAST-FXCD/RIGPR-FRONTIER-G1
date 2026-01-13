using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [SerializeField] private GridManager grid_manager;

    protected PathNode current_node;
    protected PathNode previous_node;

    protected int current_cost;
    protected int shortest_path;
    protected int chunk = 16;

    public PathFinding()
    {
        current_cost = 0;
        shortest_path = 0;
    }

    protected Vector2Int FindChunk(Vector2Int loc)
    {
        Vector2Int rv = new Vector2Int();

        rv.x = loc.x % chunk;
        rv.y = loc.y % chunk;

        return rv;
    }

    public void Pathfinding(MouseTemp mice, Vector2Int building)
    {
        //mark the distance of every tile to the whole
        //if encounter wall recaulate
        //fllow path of least numbers

        //Calculating the size of the map to search
        Vector2Int start = FindChunk(mice.Postion);
        Vector2Int end   = FindChunk(building);

        float min = Mathf.Min(start.magnitude, end.magnitude);
        Vector2Int low = min == start.magnitude ? start : end;
        Vector2Int max = min != start.magnitude ? end   : start;

        Vector2Int size = max - low;
        int[,] costs = new int[size.x, size.y];
    }
}