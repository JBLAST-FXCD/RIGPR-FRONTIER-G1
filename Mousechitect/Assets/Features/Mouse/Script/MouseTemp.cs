using System;
using System.Collections.Generic;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    [SerializeField] GridManager grid_manager;

    protected string mouse_id;
    protected Vector2Int postion;
    Vector2Int building_loc;
    List<PathNode> path;
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameObject building = GameObject.FindGameObjectWithTag("BuildingTest");

            if (building != null)
            {
                building_loc = new Vector2Int(0, 0);
                building_loc.x = (int)building.transform.localPosition.x;
                building_loc.y = (int)building.transform.localPosition.z;

                postion.x = (int)transform.localPosition.x;
                postion.y = (int)transform.localPosition.z;

                PathFinding finding = new PathFinding();
                finding.Grid_manager = grid_manager;
                path = finding.Pathfinding(this, building_loc);
            }
        }

        if (path != null && i < path.Count)
        {
            Vector3 loc = new Vector3(path[i].Postion.x, 0.5f, path[i].Postion.y);
            if (time_elapsed < 1)
            {
                this.transform.position = Vector3.Lerp(this.transform.position, loc, time_elapsed * path[i].Speed * Time.deltaTime);
                time_elapsed += path[i].Speed * Time.deltaTime;
            }
            else
            {
                this.transform.position = loc;
                time_elapsed = 0;
                i++;
            }
        }
    }
}
