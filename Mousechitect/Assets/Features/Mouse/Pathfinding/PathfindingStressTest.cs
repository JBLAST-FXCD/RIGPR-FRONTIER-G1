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
            StartCoroutine(mouse.FollowPath((success) => { if (success == false) { mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc); } }));
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

    protected void SecondWave(MouseTemp mouse, ParentBuilding building)
    {
        building.MouseLeave(mouse);
    }

    protected void SecondWave()
    {
        int j = 0;
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        for (int i = 0; i < mouses.Length; i++)
        {
            int rand = UnityEngine.Random.Range(0, buildings.Length);
            if (buildings[j].CheckOccupants(mouses[i]))
            {
                buildings[j].MouseLeave(mouses[i]);

                GetVectors(buildings[rand], mouses[i]);
            }

            j++;
            if (j == buildings.Length)
                j = 0;
        }

        Invoke(nameof(SecondWave), 30);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            FirstWave();
        }
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
