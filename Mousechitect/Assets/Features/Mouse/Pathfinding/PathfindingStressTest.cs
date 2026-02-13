using System;
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

    protected Vector2Int GetPosition(ParentBuilding building)
    {
        //Try to use the entrance point (preferred for pathfinding)
        Transform entrance = building.Building.transform.Find("EntrancePoint");

        Vector3 target_world = (entrance != null) ? entrance.position : building.transform.position;

        //Convert world space to grid coordinates
        Vector2Int building_loc = new Vector2Int(
            Mathf.RoundToInt(target_world.x),
            Mathf.RoundToInt(target_world.z)
        );

        return building_loc;
    }

    protected void GetVectors(ParentBuilding building, MouseTemp mouse)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = GetPosition(building);

        //Convert world space to grid coordinates
        Vector2Int mouse_loc = new Vector2Int(
            Mathf.RoundToInt(mouse.transform.position.x),
            Mathf.RoundToInt(mouse.transform.position.z)
        );

        //Caculate path.
        MouseTemp.Grid_manager = grid_manager;
        pathfinding.Grid_manager = grid_manager;
        mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc);

        //LERP mouse if fail start pathfinding again.
        if (mouse.Path != null)
        {
            mouse.Moving = true;
            mouse.Rigidbody = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (success == false)
                {
                    mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc);
                }
                else
                {
                    Vector2Int temp = building_loc;
                    building_loc = GetPosition(building);

                    if (temp == building_loc)
                        mouse.Rigidbody = true;
                    else
                        GetVectors(building, mouse);
                }
            }));
        }
    }

    protected void FirstWave()
    {
        buildings = FindObjectsOfType(typeof(ParentBuilding)) as ParentBuilding[];
        mouses = new MouseTemp[buildings.Length * occupants];

        int j = 0;
        for (int i = 0; i < mouses.Length; i++)
        {
            MouseTemp new_mouse = Instantiate(mouse, new Vector3(0.0f, 0.5f, 0.0f), mouse.transform.rotation);

            GetVectors(buildings[j], new_mouse);

            mouses[i] = new_mouse;

            j++;
            if (j == buildings.Length)
                j = 0;
        }

        Invoke(nameof(SecondWave), 30);
    }

    protected void Move(MouseTemp mouse)
    {
        int rand = UnityEngine.Random.Range(0, buildings.Length);

        if (GetPosition(buildings[rand]) == mouse.Position)
        {
            Move(mouse);
        }
        else
        {
            if (mouse.Home != null)
                mouse.Home.MouseLeave(mouse);

            GetVectors(buildings[rand], mouse);
        }
    }

    protected void SecondWave()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        for (int i = 0; i < mouses.Length; i++)
        {
            if (!mouses[i].Moving)
            {
                Move(mouses[i]);
            }
        }

        Invoke(nameof(SecondWave), 30);
    }

    // added by jess 
    private void Start()
    {
        if (UImGui.DebugWindow.Instance != null)
        {
            UImGui.DebugWindow.Instance.RegisterExternalCommand("stresstest", " - Spawns waves of mice to test pathfinding performance.", args =>
            {
                UImGui.DebugWindow.LogToConsole("Starting Stress Test:");
                FirstWave();
            });
        }
    }
}
