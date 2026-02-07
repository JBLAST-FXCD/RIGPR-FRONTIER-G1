using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Mouse_AI : MonoBehaviour
{
    //External scripts.
    [SerializeField] protected GridManager grid_manager;
    [SerializeField] protected PathFinding pathfinding;
    [SerializeField] protected MilkManager milkmanager;

    //Varibles for players to choose which resource is more important.
    //Player will select through UI. Value must be between 0 and 1.
    protected float scrap_weight;
    protected float cheese_weight;
    protected float milk_weight;

    protected float overtime_multiple;
    protected ResourceManager resources;

    protected List<Task> tasks;
    //protected Dictionary<Vector2Int, Task> tasks;

    protected enum Building
    {
        factory, market, tank
    }

    protected struct Task
    {
        public Vector2Int position;
        public float weight;
        public int[] amounts;
        public CheeseTypes[] cheese_types;
        public Building building;
    }

    public Mouse_AI() 
    {
        scrap_weight  = 0.5f;
        cheese_weight = 0.5f;
        milk_weight   = 0.5f;

        overtime_multiple = 1.1f;

        tasks = new List<Task>();
    }

    // GetVectors Updated by Iain    30/01/26 
    // GetVectors Updated by Anthony 23/01/26 
    public void GetVectors(ParentBuilding building, MouseTemp mouse)
    {
        //Try to use the entrance point (preferred for pathfinding)
        Transform entrance = building.Building.transform.Find("EntrancePoint");

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
            Mathf.RoundToInt(mouse.transform.position.x),
            Mathf.RoundToInt(mouse.transform.position.z)
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            PickTasks();
        }
    }

    protected Vector2Int GetPosition(Vector3 position)
    {
        Vector2Int rv = new Vector2Int();

        rv.x = (int)position.x;
        rv.y = (int)position.z;

        return rv;
    }

    protected bool TasksContainPosition(Vector2Int position)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].position == position)
            {
                //Update weight over time
                Task temp = tasks[i];
                temp.weight *= overtime_multiple;
                tasks[i] = temp;

                return true;
            }
        }
        return false;
    }

    protected void FactoryTask(FactoryBuilding factory, Vector2Int position)
    {
        if (factory.IsActive && factory.Stored_milk < factory.Milk_capacity)
        {
            Task new_taks = new Task();

            new_taks.position = position;
            new_taks.weight = 1 * cheese_weight;
            new_taks.building = Building.factory;
            new_taks.amounts = new int[1];
            new_taks.amounts[0] = factory.Milk_capacity - factory.Stored_milk;

            tasks.Add(new_taks);
        }
    }

    protected void CommercialTask(CommercialBuilding market, Vector2Int position)
    {
        List<CheeseTypes> keys = new List<CheeseTypes>();
        List<int> amounts = new List<int>();

        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
        {
            int cheese_amount = market.CheeseAmount(c);
            bool enough = resources.CanAfford(c, cheese_amount);
            if (enough == true)
            {
                keys.Add(c);
                amounts.Add(cheese_amount);
            }
        }

        Task new_taks = new Task();

        new_taks.position = position;
        new_taks.weight = 1 * scrap_weight;
        new_taks.building = Building.market;
        new_taks.amounts = amounts.ToArray();
        new_taks.cheese_types = keys.ToArray();

        tasks.Add(new_taks);
    }

    protected void TankTask(MilkTank tank, Vector2Int position)
    {
        Task new_taks = new Task();

        new_taks.position = position;
        new_taks.weight = 1 * milk_weight;
        new_taks.building = Building.tank;
        new_taks.amounts = new int[1];
        new_taks.amounts[0] = tank.max_capacity - tank.current_milk_amount;

        tasks.Add(new_taks);
    }

    protected void CheckTasks(ParentBuilding[] buildings, Building building)
    {
        if (buildings != null)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                Vector2Int position = GetPosition(buildings[i].transform.position);

                //Weight gets upadted if the task exsists else create task
                if (TasksContainPosition(position) == false)
                {
                    switch (building) 
                    {
                        case Building.factory:
                            FactoryTask((FactoryBuilding)buildings[i], position);
                            break;
                        case Building.market:
                            CommercialTask((CommercialBuilding)buildings[i], position);
                            break;
                        case Building.tank:
                            TankTask((MilkTank)buildings[i], position);
                            break;
                    }
                }
            }
        }
    }

    protected bool PickTasks()
    {
        resources = ResourceManager.instance;

        //Get total cost to find cost of average cheese
        float total_scrap_cost = 0;
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
            total_scrap_cost += Cheese.GetCheese(c).scrap_cost;

        float total_milk_cost = 0;
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
            total_milk_cost += Cheese.GetCheese(c).milk_cost;

        // Jess hasn't impolemted the total milk into resource manager get.
        int milk = milkmanager.GetTotalMilk();

        //Calculate average cheese for each resource and total cheese.
        //resource diveided by average cost to find how many average cheese are there in that recourse.
        float scrap_cheese = resources.Scrap / (total_scrap_cost / Enum.GetValues(typeof(CheeseTypes)).Length);
        float milk_cheese  = milk / (total_milk_cost / Enum.GetValues(typeof(CheeseTypes)).Length);
        float total_cheese = resources.Total_cheese;


        //Add total and divided to know which resource is effectively greater as desimal
        float total = scrap_cheese + milk_cheese + total_cheese;

        //make decimal because allows for the weight to be 50/50 player input and current resources,
        //if input are all equal and resources are all equal.
        scrap_cheese /= total;
        milk_cheese  /= total;
        total_cheese /= total;

        //Circular system to increase weight of next resouce in sycal
        //If lots of cheese sell for scrap
        scrap_weight += total_cheese;
        //If lof of scrap get more milk because cheese must be low
        milk_weight += scrap_cheese;
        //If lots of milk make cheese
        cheese_weight += milk_cheese;

        FactoryBuilding[] factories = FindObjectsOfType(typeof(FactoryBuilding)) as FactoryBuilding[];

        CheckTasks(factories, Building.factory);

        CommercialBuilding[] markets = FindObjectsOfType(typeof(CommercialBuilding)) as CommercialBuilding[];

        CheckTasks(markets, Building.market);

        MilkTank[] tanks = FindObjectsOfType(typeof(MilkTank)) as MilkTank[];

        CheckTasks(tanks, Building.tank);

        if (tasks.Count > 0) 
        {
            //Sort list in wieght order.
            for (int i = 0; i < tasks.Count - 1; i++)
            {
                for (int j = 0; j < tasks.Count - i -1; j++)
                {
                    if (tasks[j].weight < tasks[j + 1].weight) 
                    {
                        Task temp = tasks[j];
                        tasks[j] = tasks[j + 1];
                        tasks[j + 1] = temp;
                    } 
                }
            }

            return true;
        }
        return false;
    }

    protected bool PickMouseInBuilding()
    {


        return false;
    }
}