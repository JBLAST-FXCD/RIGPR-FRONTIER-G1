using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    protected string mouse_id;

    //Varibles for paths and LERP.
    protected List<BaseNode> path;

    //Grind manager is for checking calulated speed vs current speed.
    protected GridManager grid_manager;

    public string Mouse_id { get { return mouse_id; } }

    public List<BaseNode> Path { get { return path; } set { path = value; } }
    public GridManager Grid_manager { set { grid_manager = value; } }

    protected int SetRotation(Vector3 current_loc, Vector3 new_loc)
    {
        if (current_loc.x > new_loc.x)
            return 90;
        else if (current_loc.x < new_loc.x)
            return 270;
        else if (current_loc.z < new_loc.z)
            return 180;
        else
            return 0;
    }

    public IEnumerator FollowPath(Action<bool> callback)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 current_loc = this.transform.position;
            Vector3 new_loc = new Vector3(path[i].postion.x, 0, path[i].postion.y);
            float speed = new PathNode(path[i].postion, grid_manager).Speed;

            this.transform.eulerAngles = new Vector3(0, SetRotation(current_loc, new_loc), 0);

            yield return new WaitForEndOfFrame();

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
