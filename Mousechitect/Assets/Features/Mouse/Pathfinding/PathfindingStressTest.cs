using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StressTest : MonoBehaviour
{
    [SerializeField] protected MouseTemp mouse;
    [SerializeField] protected GridManager grid_manager;
    [SerializeField] protected PathFinding pathfinding;

    ParentBuilding[] buildings;
    MouseTemp[] mouses;

    int occupants;

    StressTest()
    {
        occupants = 32;
    }

    public void MoveMouse(MouseTemp mouse, ParentBuilding building)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = building.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

        //Caculate path.
        List<BaseNode> path = pathfinding.FindPath(mouse.Position, building_loc);

        if(path == null)
            path = pathfinding.CreatePath(mouse_loc, building_loc);
        mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc);

        //LERP mouse if fail start pathfinding again.
        if (mouse.Path != null && !mouse.Moving)
        {
            mouse.Moving = true;
            mouse.Collider = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (!success)
                {
                    MoveMouse(mouse, building);
                }
                else
                {
                    mouse.Moving = false;
                    mouse.Collider = true;
                    pathfinding.SavePath(mouse_loc, building_loc, path);
                }
            }));
        }
    }

    protected IEnumerator FirstWave()
    {
        buildings = FindObjectsOfType(typeof(ParentBuilding)) as ParentBuilding[];
        mouses = new MouseTemp[buildings.Length * occupants];

        int j = 0;
        for (int i = 0; i < mouses.Length; i++)
        {
            MouseTemp new_mouse = Instantiate(mouse, new Vector3(0.0f, 0.5f, 0.0f), mouse.transform.rotation);

            MoveMouse(new_mouse, buildings[j]);

            mouses[i] = new_mouse;

            j++;
            if (j == buildings.Length)
                j = 0;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(SecondWave());
    }

    protected void MoveIndex(MouseTemp mouse)
    {
        int rand = UnityEngine.Random.Range(0, buildings.Length);

        if (buildings[rand].GetPosition() == mouse.Position)
        {
            MoveIndex(mouse);
        }
        else
        {
            if (mouse.Home != null)
                mouse.Home.MouseLeave(mouse);

            MoveMouse(mouse, buildings[rand]);
        }
    }

    protected IEnumerator SecondWave()
    {
        yield return new WaitForSeconds(30);

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        for (int i = 0; i < mouses.Length; i++)
        {
            if (!mouses[i].Moving)
            {
                MoveIndex(mouses[i]);
                yield return new WaitForEndOfFrame();
            }
        }

        StartCoroutine(SecondWave());
    }

    // added by jess 
    private void Start()
    {
        MouseTemp.Grid_manager = grid_manager;
        pathfinding.Grid_manager = grid_manager;

        if (UImGui.DebugWindow.Instance != null)
        {
            UImGui.DebugWindow.Instance.RegisterExternalCommand("stresstest", " - Spawns waves of mice to test pathfinding performance.", args =>
            {
                UImGui.DebugWindow.LogToConsole("Starting Stress Test:");
                StartCoroutine(FirstWave());
            });
        }
    }
}
