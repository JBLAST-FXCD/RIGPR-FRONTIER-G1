using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    [SerializeField] protected GridManager grid_manager;

    [SerializeField] protected PathFinding pathfinding;

    protected string mouse_id;

    //Varibles for paths and LERP.
    protected Vector2Int postion;
    Vector2Int building_loc;
    public List<BaseNode> path;
    public Vector2Int Postion {  get { return postion; } }

    public string GetMouseID()
    {
        return mouse_id;
    }

    // GetVectors Updated by Iain    27/01/26 
    // GetVectors Updated by Anthony 23/01/26 
    public void GetVectors(ParentBuilding building)
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

        path = pathfinding.Pathfinding(postion, building_loc);

        if (path != null)
            StartCoroutine(FollowPath());
    }
    public void GetVectors(GameObject building)
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

        path = pathfinding.Pathfinding(postion, building_loc);

        if(path != null)
            StartCoroutine(FollowPath());
    }

    protected IEnumerator FollowPath()
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 loc = new Vector3(path[i].postion.x, 0.5f, path[i].postion.y);
            float speed = new PathNode(path[i].postion, grid_manager).Speed;

            if (path[i].speed == speed)
            {
                float time_elapsed = 0;
                while (this.transform.position != loc)
                {
                    //Smoothing function
                    time_elapsed += speed * Time.deltaTime;
                    float x = Mathf.Clamp(time_elapsed, 0, Mathf.PI);
                    float t = 0.5f * Mathf.Sin(x - Mathf.PI / 2) + 0.5f;

                    this.transform.position = Vector3.Lerp(this.transform.position, loc, t);
                    yield return new WaitForFixedUpdate();
                }
                i++;
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
    }
}
