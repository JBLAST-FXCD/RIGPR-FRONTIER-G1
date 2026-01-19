using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinding
{
    protected GridManager grid_manager;

    protected int test;
    protected int chunk;
    protected Vector2Int start, end, mouse_loc, building_loc;

    protected PathNode[,] nodes;
    protected List<Vector2Int> open_nodes;
    protected Vector2Int current_loc;
    List<PathNode> route;

    public GridManager Grid_manager { get { return grid_manager; } set { grid_manager = value; } }

    public PathFinding()
    {
        test  = 0;
        chunk = 16;

        start        = new Vector2Int(0, 0);
        end          = new Vector2Int(0, 0); 
        mouse_loc    = new Vector2Int(0, 0);
        building_loc = new Vector2Int(0, 0);

        open_nodes = new List<Vector2Int>();
        route = new List<PathNode>();
    }

    protected Vector2Int FindChunk(Vector2 loc)
    {
        Vector2Int rv = new Vector2Int();

        if (loc.x < 0)
            rv.x = Mathf.FloorToInt(loc.x / chunk) * 16;
        else
            rv.x = Mathf.CeilToInt(loc.x / chunk) * 16;
        if (loc.y < 0)
            rv.y = Mathf.FloorToInt(loc.y / chunk) * 16;
        else
            rv.y = Mathf.CeilToInt(loc.y / chunk) * 16;

        return rv;
    }

    protected void SearchNode(int x, int y)
    {
        if (nodes[x, y].Searched == false)
        {
            nodes[x, y].Searched = true;
            open_nodes.Add(new Vector2Int(x, y));
        }

        int cheeper = nodes[current_loc.x, current_loc.y].Total_cost + nodes[x, y].Cost;
        if (cheeper < nodes[x, y].Total_cost)
        {
            nodes[x, y].Total_cost = cheeper;
            nodes[x, y].Previous_node = nodes[current_loc.x, current_loc.y];
        }
    }

    public List<PathNode> Pathfinding(MouseTemp mouse, Vector2Int building)
    {

        start = FindChunk(mouse.Postion);
        end = FindChunk(building);

        start.x = start.x > end.x ? start.x : end.x;
        start.y = start.y > end.y ? start.y : end.y;

        end.x = end.x < start.x ? end.x : start.x;
        end.y = end.y < start.y ? end.y : start.y; 

        mouse_loc = mouse.Postion;
        building_loc = building;

        int i = end.x;
        int j = end.y;

        while(end.x < 0)
        {
            end.x += chunk;
            start.x += chunk;
            mouse_loc.x += chunk;
            building_loc.x += chunk;
        }
        while (end.x > 0)
        {
            end.x -= chunk;
            start.x -= chunk;
            mouse_loc.x -= chunk;
            building_loc.x -= chunk;
        }

        while (end.y < 0)
        {
            end.y += chunk;
            start.y += chunk;
            mouse_loc.y += chunk;
            building_loc.y += chunk;
        }
        while (end.x > 0)
        {
            end.y -= chunk;
            start.y -= chunk;
            mouse_loc.y -= chunk;
            building_loc.y -= chunk;
        }

        nodes = new PathNode[start.x, start.y];

        for (int x = end.x; x < start.x; x++)
        {
            int temp = j;
            for (int y = end.y; y < start.y; y++)
            {
                nodes[x, y] = new PathNode(new Vector2Int(i, j), Grid_manager);
                j++;
            }
            j = temp;
            i++;
        }

        nodes[building_loc.x, building_loc.y].Total_cost = 0;
        nodes[building_loc.x, building_loc.y].Searched = true;
        open_nodes.Add(new Vector2Int(building_loc.x, building_loc.y));

        while (open_nodes.Count > 0)
        {
            test++;
            current_loc = open_nodes[0];

            if (current_loc == mouse_loc)
            {
                for (int n = 0; n < open_nodes.Count; n++)
                {
                    if (nodes[open_nodes[n].x, open_nodes[n].y].Total_cost >= nodes[current_loc.x, current_loc.y].Total_cost)
                        open_nodes.RemoveAt(n);
                }

                if (open_nodes.Count == 0) 
                {
                    PathNode current_node = nodes[current_loc.x, current_loc.y];
                    route.Add(current_node);

                    while (current_node.Previous_node != null)
                    {
                        route.Add(current_node.Previous_node);
                        current_node = current_node.Previous_node;
                    }

                    return route;
                }
                else
                {
                    nodes[current_loc.x, current_loc.y].Searched = false;
                    test++;
                    current_loc = open_nodes[0];
                }
            }

            if (current_loc.x + 1 < start.x)
            {
                SearchNode(current_loc.x + 1, current_loc.y);
            }
            if (current_loc.y + 1 < start.y)
            {
                SearchNode(current_loc.x, current_loc.y + 1);
            }
            if (current_loc.x - 1 > 0)
            {
                SearchNode(current_loc.x - 1, current_loc.y);
            }
            if (current_loc.y - 1 >  0)
            {
                SearchNode(current_loc.x, current_loc.y - 1);
            }

            open_nodes.RemoveAt(0);
        }

        if (nodes[mouse_loc.x, mouse_loc.y].Previous_node != null)
        {
            PathNode current_node = nodes[mouse_loc.x, mouse_loc.y];
            route.Add(current_node);

            while (current_node.Previous_node != null)
            {
                route.Add(current_node.Previous_node);
                current_node = current_node.Previous_node;
            }

            return route;
        }
        else
            return null;
    }
}