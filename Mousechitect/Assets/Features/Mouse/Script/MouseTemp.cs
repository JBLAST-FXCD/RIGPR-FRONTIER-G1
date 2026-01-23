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
        building_loc = new Vector2Int(0, 0);
        building_loc.x = (int)building.transform.localPosition.x;
        building_loc.y = (int)building.transform.localPosition.z;

        postion.x = (int)transform.localPosition.x;
        postion.y = (int)transform.localPosition.z;

        pathfinding.Grid_manager = grid_manager;
        path = pathfinding.Pathfinding(this.postion, building_loc);
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
