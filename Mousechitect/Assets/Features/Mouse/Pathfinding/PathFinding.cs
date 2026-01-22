using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Iain Benner 21/01/2026

/// <summary>
/// Takes in two vectors and decides which chunks to search and finds the fastest route between the two vectors in the area.
/// The algorithm uses weights and flood fill the find a quick route efficiently not guaranteed to find fastest route every time.
/// </summary>
public class PathFinding: MonoBehaviour, ISaveable
{
    //Used for calculating cost with the speed of the grid.
    protected GridManager grid_manager;

    //Varibles for the algorthim.
    protected int chunk;
    protected Vector2Int start, end, mouse_loc, building_loc, current_loc;
    protected List<Vector2Int> open_nodes;
    protected PathNode[,] nodes;

    //Varible for saving routes.
    protected Dictionary<string, List<BaseNode>> solutions;

    public PathFinding()
    {
        chunk = 16;
        open_nodes = new List<Vector2Int>();

        solutions = new Dictionary<string, List<BaseNode>>();
    }

    public GridManager Grid_manager { get { return grid_manager; } set { grid_manager = value; } }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
        {
            solutions.Clear();
        }
    }

    //chunks are calculated because the route can go around the vector which can't be calulated if they in the corner of the grid.
    protected Vector2Int FindChunk(Vector2 loc)
    {
        Vector2Int rv = new Vector2Int();

        if (loc.x < 0)
            rv.x = Mathf.FloorToInt(loc.x / chunk) * chunk;
        else
            rv.x = Mathf.CeilToInt(loc.x  / chunk) * chunk;
        if (loc.y < 0)
            rv.y = Mathf.FloorToInt(loc.y / chunk) * chunk;
        else
            rv.y = Mathf.CeilToInt(loc.y  / chunk) * chunk;

        return rv;
    }

    //Open list is for nodes too search.
    //Cheeper means faster route even if the mice travels over more units.
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

    //For saving known route in dictionary.
    protected string GenKey(Vector2Int mouse, Vector2Int building)
    {
        string rv = string.Empty;

        string first  = mouse.ToString();
        string second = building.ToString();

        rv = first + second;

        return rv;
    }

    //If grid changes the solutions need to be removed so they can be recalculated.
    public void RemoveValue(Vector2Int mouse, Vector2Int building)
    {
        string key = GenKey(mouse, building);
        solutions.Remove(key);
    }

    //Returns all the base nodes in the path nodes creating route.
    protected List<BaseNode> CreateRoute(Vector2Int mouse, Vector2Int building)
    {
        List<BaseNode> route = new List<BaseNode>();

        //Start with mouse and decend throught the costs (flood fill).
        PathNode current_node = nodes[mouse_loc.x, mouse_loc.y];
        route.Add(new BaseNode(current_node.Postion, current_node.Speed));

        //Building node is null indacating the end
        while (current_node.Previous_node != null)
        {
            route.Add(new BaseNode(current_node.Previous_node.Postion, current_node.Previous_node.Speed));
            current_node = current_node.Previous_node;
        }

        //Save
        string key = GenKey(mouse, building);
        solutions.Add(key, route);

        return route;
    }

    public List<BaseNode> Pathfinding(Vector2Int mouse, Vector2Int building)
    {
        //Load route if it been calculated before.
        List<BaseNode> route;
        string key = GenKey(mouse, building);
        if (solutions.TryGetValue(key, out route))
        {
            return route;
        }
        
        //Find grid to search.
        start = FindChunk(mouse);
        end   = FindChunk(building);

        //Making sure end is lowest point and start is largest
        start.x = start.x > end.x ? start.x : end.x;
        start.y = start.y > end.y ? start.y : end.y;

        end.x = end.x < start.x ? end.x : start.x;
        end.y = end.y < start.y ? end.y : start.y;

        //Making sure grid is aleast one chunk wide or long.
        end.x = end.x == start.x ? end.x - chunk: end.x;
        end.y = end.y == start.y ? end.y - chunk : end.y;

        //Save where nodes begin for initialising array.
        int i = end.x;
        int j = end.y;

        //Save location to know where they are in array.
        mouse_loc    = mouse;
        building_loc = building;

        //Move location because array needs to start a 0,0.
        while (end.x < 0)
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

        //initialise nodes arrray.
        nodes = new PathNode[start.x, start.y];

        for (int x = 0; x < start.x; x++)
        {
            int temp = j;
            for (int y = 0; y < start.y; y++)
            {
                nodes[x, y] = new PathNode(new Vector2Int(i, j), Grid_manager);
                j++;
            }
            j = temp;
            i++;
        }

        //The starting node most not be searched.
        nodes[building_loc.x, building_loc.y].Total_cost = int.MinValue;
        nodes[building_loc.x, building_loc.y].Searched = true;
        //The building node is the start because previous_node get loop throught in revers meaning mouse will be the start.
        open_nodes.Add(new Vector2Int(building_loc.x, building_loc.y));

        while (open_nodes.Count > 0)
        {
            //Open nodes are node that need to be searched.
            current_loc = open_nodes[0];

            //Checking if route is finished.
            if (current_loc == mouse_loc)
            {
                //Remove any route that is longer then current.
                for (int n = 0; n < open_nodes.Count; n++)
                {
                    if (nodes[open_nodes[n].x, open_nodes[n].y].Total_cost >= nodes[current_loc.x, current_loc.y].Total_cost)
                        open_nodes.RemoveAt(n);
                }

                if (open_nodes.Count == 0) 
                    return CreateRoute(mouse, building);
                else
                {
                    //If searching continues allow mouse tile to be used again.
                    nodes[current_loc.x, current_loc.y].Searched = false;
                    current_loc = open_nodes[0];
                }
            }

            //Find neighbours and search node.
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

            //Finishes searching node.
            open_nodes.RemoveAt(0);
        }

        if (nodes[mouse_loc.x, mouse_loc.y].Previous_node != null)
            return CreateRoute(mouse, building);
        //If route can't be found mouse need to sleep as per GDD.
        else
            return null;
    }

    public void PopulateSaveData(GameData data)
    {
        foreach (var (key, value) in solutions)
        {
            route_save_data path = new route_save_data();
            path.key    = key;
            path.values = value;
            data.pathmap.paths.Add(path);
        }
    }

    public void LoadFromSaveData(GameData data)
    {
        List<route_save_data> paths = data.pathmap.paths;

        for (int n = 0; n < paths.Count; n++) 
        {
            solutions.Add(paths[n].key, paths[n].values);
        }
    }
}