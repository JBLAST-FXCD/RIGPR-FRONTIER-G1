using System.Collections.Generic;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    [SerializeField] protected GridManager grid_manager;

    [SerializeField] protected PathFinding pathfinding;

    protected string mouse_id;
    protected Vector2Int postion;
    Vector2Int building_loc;
    public List<BaseNode> path;
    float time_elapsed;
    int i;

    public Vector2Int Postion {  get { return postion; } }

    public MouseTemp()
    {
        i = 0;
        time_elapsed = 0;
    }

    public string GetMouseID()
    {
        return mouse_id;
    }

    protected void GetVectors(GameObject building)
    {
        // Try to use the entrance point (preferred for pathfinding)
        Transform entrance = building.transform.Find("EntrancePoint");

        Vector3 target_world = (entrance != null) ? entrance.position : building.transform.position;

        // Convert world space to grid coordinates
        building_loc = new Vector2Int(
            Mathf.RoundToInt(target_world.x),
            Mathf.RoundToInt(target_world.z)
        );

        postion = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        pathfinding.Grid_manager = grid_manager;

        // Reset path follow state whenever we calculate a new path
        i = 0;
        time_elapsed = 0.0f;

        path = pathfinding.Pathfinding(postion, building_loc);

        Debug.Log(path == null
    ? $"No path from {postion} to {building_loc}. Target speed={grid_manager.GetCellMoveSpeed(building_loc)}"
    : $"Path found: {path.Count} nodes to {building_loc}. Target speed={grid_manager.GetCellMoveSpeed(building_loc)}");

    }

    protected void Move(float speed, Vector3 loc)
    {
        if (time_elapsed < 1)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, loc, time_elapsed * speed * Time.deltaTime);
            time_elapsed += speed * Time.deltaTime;
        }
        else
        {
            this.transform.position = loc;
            time_elapsed = 0;
            i++;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameObject building = GameObject.FindGameObjectWithTag("BuildingTest");

            Debug.Log($"BuildingTest found: {building.name} at {building.transform.position}");


            if (building != null)
            {
                GetVectors(building);
            }
        }

        if (path != null && i < path.Count)
        {
            Vector3 loc = new Vector3(path[i].postion.x, 0.5f, path[i].postion.y);
            float speed = new PathNode(path[i].postion, grid_manager).Speed;

            if (path[i].speed == speed)
            {
                Move(speed, loc);
            }
            else
            {
                path = null;
                i = 0;

                pathfinding.RemoveValue(this.postion, building_loc);

                GameObject building = GameObject.FindGameObjectWithTag("BuildingTest");
                if (building != null)
                {
                    GetVectors(building);
                }
            }
        }
    }
}
