using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    protected GridManager grid_manager;
    public GridManager Grid_manager { get { return grid_manager; } set { grid_manager = value; } }

    protected int chunk;
    protected PathNode[,] nodes;
    protected Vector2Int begining, destination, low, max, size;
    protected int i, j;

    public PathFinding()
    {
        chunk = 16;
    }

    Vector2Int FindChunk(Vector2Int loc)
    {
        Vector2Int rv = new Vector2Int();

        rv.x = loc.x % chunk;
        rv.y = loc.y % chunk;

        return rv;
    }

    void SetLowest()
    {
        //The nodes map needs the lowest vetor to be at 0,0 of the array.
        //Swaps numbers if size is a negative number and makes sure i and j start at the lowest location.
        // i and j are for the real location.
        if (size.x < 0)
        {
            i             = max.x;
            size.x        = -size.x;
            destination.x = 0;
            begining.x    = size.x;
        }
        else
        {
            i             = low.x;
            destination.x = size.x;
            begining.x    = 0;
        }
        if (size.y < 0)
        {
            j             = max.y;
            size.y        = -size.y;
            destination.y = 0;
            begining.y    = size.y;
        }
        else
        {
            j             = low.y;
            size.y        = -size.y;
            destination.y = size.y;
            begining.y    = 0;
        }
    }

    public void Pathfinding(MouseTemp mice, Vector2Int building)
    {
        //mark the distance of every tile to the whole
        //if encounter wall recaulate
        //fllow path of least numbers

        //Calculating the size of the map to search
        Vector2Int start = FindChunk(mice.Postion);
        Vector2Int end   = FindChunk(building);

        if (start.sqrMagnitude < end.sqrMagnitude)
        {
            low = mice.Postion;
            max = building;
        }
        else
        {
            low = building;
            max = mice.Postion;
        }

        size = max - low;
        SetLowest();

        nodes = new PathNode[size.x, size.y];

        // x and y are for iterating the array.
        for (int x = 0; x < size.x; x++)
        {
            int temp = j;
            for (int y = 0; y < size.y; y++)
            {
                nodes[x, y] = new PathNode(new Vector2Int(i,j), grid_manager);
                j++;
            }
            j = temp;
            i++;
        }

        Debug.Log("");
    }
}