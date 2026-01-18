using System;
using UnityEngine;

public class MouseTemp : MonoBehaviour
{
    [SerializeField] GridManager grid_manager;

    protected string mouse_id;
    protected Vector2Int postion;
    Vector2Int building_loc;

    public Vector2Int Postion {  get { return postion; } }

    public string GetMouseID()
    {
        return mouse_id;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject building = GameObject.FindGameObjectWithTag("BuildingTest");
        building_loc = new Vector2Int(0, 0);
        building_loc.x = (int)building.transform.localPosition.x;
        building_loc.y = (int)building.transform.localPosition.z;

        postion.x = (int)transform.localPosition.x;
        postion.y = (int)transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PathFinding finding = new PathFinding();
            finding.Grid_manager = grid_manager;
            finding.Pathfinding(this, building_loc);
        }
    }
}
