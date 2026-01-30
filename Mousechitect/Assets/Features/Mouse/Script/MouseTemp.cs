using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    protected string mouse_id;

    //Varibles for paths and LERP.
    protected Vector2Int postion;
    protected List<BaseNode> path;

    //Grind manager is for checking calulated speed vs current speed.
    protected GridManager grid_manager;

    public Vector2Int Postion {  get { return postion; } }
    public List<BaseNode> Path { get { return path; } set { path = value; } }
    public GridManager Grid_manager { set { grid_manager = value; } }

    public string GetMouseID()
    {
        return mouse_id;
    }

    public IEnumerator FollowPath(Action<bool> callback)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 current_loc = this.transform.position;
            Vector3 new_loc = new Vector3(path[i].postion.x, 0, path[i].postion.y);
            float speed = new PathNode(path[i].postion, grid_manager).Speed;

            if (path[i].speed == speed)
            {
                float time_elapsed = 0;

                while (this.transform.position != new_loc)
                {
                    time_elapsed += Time.deltaTime * speed;
                    this.transform.position = Vector3.Lerp(current_loc, new_loc, time_elapsed);
                    yield return new WaitForFixedUpdate();
                }
            }
            else
                callback(false);
        }
        callback(true);
    }
}
