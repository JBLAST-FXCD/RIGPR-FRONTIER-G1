using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse_AI : MonoBehaviour
{
    //External scripts.
    [SerializeField] protected GridManager grid_manager;
    [SerializeField] protected PathFinding pathfinding;

    //Varibles for players to choose which resource is more important.
    //Player will select through UI. Value must be between 0 and 1.
    protected float scrap_weight;
    protected float cheese_weight;
    protected float milk_weight;

    protected List<FactoryBuilding> factories;
    protected List<CommercialBuilding> markets;

    // GetVectors Updated by Iain    30/01/26 
    // GetVectors Updated by Anthony 23/01/26 
    public void GetVectors(ParentBuilding building, MouseTemp mouse)
    {
        //Try to use the entrance point (preferred for pathfinding)
        Transform entrance = building.transform.Find("EntrancePoint");

        Vector3 target_world = (entrance != null) ? entrance.position : building.transform.position;

        //Convert world space to grid coordinates
        Vector2Int building_loc = new Vector2Int(
            Mathf.RoundToInt(target_world.x),
            Mathf.RoundToInt(target_world.z)
        );

        //Allows mice to check caculated speed against current grid.
        mouse.Grid_manager = grid_manager;

        //Convert world space to grid coordinates
        Vector2Int mouse_loc = new Vector2Int(
            Mathf.RoundToInt(mouse.Postion.x),
            Mathf.RoundToInt(mouse.Postion.y)
        );

        //Caculate path.
        pathfinding.Grid_manager = grid_manager;
        mouse.Path = pathfinding.Pathfinding(mouse_loc, building_loc);

        //LERP mouse if fail start pathfinding again.
        if (mouse.Path != null)
            StartCoroutine(mouse.FollowPath( (success) => { if (success == false) { mouse.Path = pathfinding.Pathfinding(mouse_loc, building_loc); } }));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ParentBuilding building = FindObjectOfType(typeof(ParentBuilding)) as ParentBuilding;
            MouseTemp      mouse    = FindObjectOfType(typeof(MouseTemp)) as MouseTemp;

            if (building != null && mouse != null)
            {
                GetVectors(building, mouse);
            }
        }
    }

    protected void PickTask()
    {
        //Making sure theres only has one request and the factory needs milk. 
        FactoryBuilding[] factories_temp = FindObjectsOfType(typeof(FactoryBuilding)) as FactoryBuilding[];

        if (factories_temp != null)
        {
            for (int i = 0; i < factories_temp.Length; i++)
            {
                if (!factories.Contains(factories_temp[i]) && factories_temp[i].IsActive == true)
                    factories.Add(factories_temp[i]);
            }
        }

        CommercialBuilding[] markets_temp = FindObjectsOfType(typeof(CommercialBuilding)) as CommercialBuilding[];

        if(markets_temp != null)
        {
            ResourceManager resources = ResourceManager.instance;

            for (int i = 0;i < markets_temp.Length; i++)
            {
                if (resources.Cheese >= markets[i].Enough_Cheese)
                    markets.Add(markets[i]);
            }
        }
    }
}
