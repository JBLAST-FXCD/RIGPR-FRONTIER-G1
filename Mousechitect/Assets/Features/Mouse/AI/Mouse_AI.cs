using System;
using System.Collections.Generic;
using UnityEngine;

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

    protected FactoryBuilding[] factories;
    protected CommercialBuilding[] markets;
    protected MilkTank[] tanks;
    protected MilkCollector[] collectors;

    protected ResourceManager resources;

    protected List<Task> tasks;
    protected List<MouseTask> mouse_tasks;

    protected enum Building
    {
        factory, market, tank, collector
    }

    protected struct Task
    {
        public Vector2Int position;
        public float weight;
        public int[] amounts;
        public CheeseTypes[] cheese_types;
        public Building building;
    }

    protected struct MouseTask
    {
        public MouseTemp mouse;
        public Vector2Int position;
        public int task_index;
    }

    public Mouse_AI() 
    {
        scrap_weight  = 0.5f;
        cheese_weight = 0.5f;
        milk_weight   = 0.5f;

        overtime_multiple = 1.1f;

        tasks = new List<Task>();
        mouse_tasks = new List<MouseTask>();
    }

    // GetVectors Updated by Iain    30/01/26 
    // GetVectors Updated by Anthony 23/01/26 
    public void GetVectors(ParentBuilding building, MouseTemp mouse)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = GetPosition(building);

        //Allows mice to check caculated speed against current grid.
        mouse.Grid_manager = grid_manager;

        //Convert world space to grid coordinates
        Vector2Int mouse_loc = new Vector2Int(
            Mathf.RoundToInt(mouse.transform.position.x),
            Mathf.RoundToInt(mouse.transform.position.z)
        );

        //Caculate path.
        pathfinding.Grid_manager = grid_manager;
        mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc);

        //LERP mouse if fail start pathfinding again.
        if (mouse.Path != null)
            StartCoroutine(mouse.FollowPath( (success) => { if (success == false) { mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc); } }));
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
            PickMouseInBuilding();
            MoveSingleRoute();
        }
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

    protected void FindBuildings()
    {
        factories = FindObjectsOfType(typeof(FactoryBuilding)) as FactoryBuilding[];
        markets = FindObjectsOfType(typeof(CommercialBuilding)) as CommercialBuilding[];
        tanks = FindObjectsOfType(typeof(MilkTank)) as MilkTank[];
        collectors = FindObjectsOfType(typeof(MilkCollector)) as MilkCollector[];
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
                Vector2Int position = GetPosition(buildings[i]);

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

        FindBuildings();

        CheckTasks(factories, Building.factory);
        CheckTasks(markets, Building.market);
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

    //Only use this funtion with MilkTanks or MilkCollectors
    protected MouseTask CheckMilkContainer(Task task, ParentBuilding[] buildings, Building building)
    {
        float current_mag = float.MaxValue;
        int index = 0;

        MouseTask mouse_taks = new MouseTask();

        if (buildings != null && buildings.Length > 0)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                IMilkContainer container;
                switch (building)
                {
                    case Building.tank:
                        container = (MilkTank)buildings[i];
                        break;
                    default:
                        container = (MilkCollector)buildings[i];
                        break;
                }

                if (container.CURRENT_MILK_AMOUNT >= task.amounts[0] && buildings[i].Mouse_occupants.Count > 0)
                {
                    float new_mag = (task.position - GetPosition(buildings[i])).sqrMagnitude;
                    if (new_mag < current_mag)
                    {
                        current_mag = new_mag;
                        index = i;
                    }
                }
            }

            mouse_taks.mouse = buildings[index].Mouse_occupants[0];
            mouse_taks.position = GetPosition(buildings[index]);

            return mouse_taks;
        }

        mouse_taks.mouse = null;
        return mouse_taks;
    }
    
    protected MouseTask FindMilk(Task task)
    {
        MouseTask tank      = CheckMilkContainer(task, tanks, Building.tank);
        MouseTask collector = CheckMilkContainer(task, collectors, Building.collector);

        if (tank.mouse == null)
        {
            if (collector.mouse == null)
                return new MouseTask(); //NULL
            return collector;
        }
        else if (collector.mouse == null)
            return tank;
        //Checking which building is closser.
        if ((task.position - tank.position).sqrMagnitude < (task.position - collector.position).sqrMagnitude)
            return tank;
        else
            return collector;
    }

    protected bool PickMouseInBuilding()
    {
        for (int i = 0; i < tasks.Count; i++) 
        {
            switch (tasks[i].building)
            {
                case Building.factory:
                    MouseTask factory_task = FindMilk(tasks[i]);

                    if (factory_task.mouse != null)
                    {
                        factory_task.task_index = i;
                        mouse_tasks.Add(factory_task);
                    }
                    break;
                case Building.tank:
                    MouseTask tank_task = CheckMilkContainer(tasks[i], collectors, Building.collector);

                    if (tank_task.mouse != null)
                    {
                        tank_task.task_index = i;
                        mouse_tasks.Add(tank_task);
                    }
                    break;
            }
        }
        if (mouse_tasks.Count > 0)
            return true;
        return false;
    }

    protected bool MoveSingleRoute()
    {
        for (int i = mouse_tasks.Count - 1; i >= 0; i--)
        {
            MouseTemp mouse = mouse_tasks[i].mouse;
            mouse.Path = null;

            //Convert world space to grid coordinates
            Vector2Int mouse_loc = new Vector2Int(
                Mathf.RoundToInt(mouse.transform.position.x),
                Mathf.RoundToInt(mouse.transform.position.z)
            );

            Vector2Int building_loc = tasks[mouse_tasks[i].task_index].position;

            mouse.Path = pathfinding.FindPath(mouse_tasks[i].position, building_loc);

            if (mouse.Path == null)
                mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc);

            //LERP mouse if fail start pathfinding again.
            if (mouse.Path != null)
                StartCoroutine(mouse.FollowPath((success) => { 
                    if (success == false)
                    { 
                        mouse.Path = pathfinding.CreatePath(mouse_loc, building_loc); 
                    }
                    else 
                    {
                        pathfinding.SavePath(mouse_loc, building_loc, mouse.Path);
                    }
                }));
        }

        return false;
    }
}