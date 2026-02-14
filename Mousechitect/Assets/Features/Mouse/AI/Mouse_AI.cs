using System;
using System.Collections.Generic;
using System.Linq;
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
    protected Dictionary<ParentBuilding, MouseTemp> mice;

    protected struct Task
    {
        public ParentBuilding building;
        public float weight;
        public int[] amounts;
        public CheeseTypes[] cheese_types;
        public BuildingType building_type;
    }

    public Mouse_AI() 
    {
        scrap_weight  = 0.5f;
        cheese_weight = 0.5f;
        milk_weight   = 0.5f;

        overtime_multiple = 1.1f;

        tasks = new List<Task>();
        mice = new Dictionary<ParentBuilding, MouseTemp>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ParentBuilding building = FindObjectOfType(typeof(ParentBuilding)) as ParentBuilding;
            MouseTemp      mouse    = FindObjectOfType(typeof(MouseTemp)) as MouseTemp;

            if (building != null && mouse != null)
                MoveMouse(mouse, building);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            MouseTemp.Grid_manager = grid_manager;
            pathfinding.Grid_manager = grid_manager;

            BehaviourTree();
        }
    }

    protected bool FindBuildings()
    {
        factories = FindObjectsOfType(typeof(FactoryBuilding)) as FactoryBuilding[];
        markets = FindObjectsOfType(typeof(CommercialBuilding)) as CommercialBuilding[];
        tanks = FindObjectsOfType(typeof(MilkTank)) as MilkTank[];
        collectors = FindObjectsOfType(typeof(MilkCollector)) as MilkCollector[];

        if (factories.Any() || markets.Any() || collectors.Any())
            return true;
        return false;
    }

    protected bool TasksContainBuilding(ParentBuilding building)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].building == building)
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

    protected void FactoryTask(FactoryBuilding factory, ParentBuilding building)
    {
        if (factory.IsActive && factory.Stored_milk < factory.Milk_capacity)
        {
            Task new_taks = new Task();

            new_taks.building = building;
            new_taks.weight = 1 * cheese_weight;
            new_taks.building_type = BuildingType.factory;
            new_taks.amounts = new int[1];
            new_taks.amounts[0] = factory.Milk_capacity - factory.Stored_milk;

            tasks.Add(new_taks);
        }
    }

    protected void CommercialTask(CommercialBuilding market, ParentBuilding building)
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

        new_taks.building = building;
        new_taks.weight = 1 * scrap_weight;
        new_taks.building_type = BuildingType.market;
        new_taks.amounts = amounts.ToArray();
        new_taks.cheese_types = keys.ToArray();

        tasks.Add(new_taks);
    }

    protected void TankTask(MilkTank tank, ParentBuilding building)
    {
        Task new_taks = new Task();

        new_taks.building = building;
        new_taks.weight = 1 * milk_weight;
        new_taks.building_type = BuildingType.tank;
        new_taks.amounts = new int[1];
        new_taks.amounts[0] = tank.max_capacity - tank.current_milk_amount;

        tasks.Add(new_taks);
    }

    protected void CheckTasks(ParentBuilding[] buildings, BuildingType building_type)
    {
        if (buildings != null)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                ParentBuilding building = buildings[i];

                //Weight gets upadted if the task exsists else create task
                if (TasksContainBuilding(building) == false)
                {
                    switch (building_type) 
                    {
                        case BuildingType.factory:
                            FactoryTask((FactoryBuilding)buildings[i], building);
                            break;
                        case BuildingType.market:
                            CommercialTask((CommercialBuilding)buildings[i], building);
                            break;
                        case BuildingType.tank:
                            TankTask((MilkTank)buildings[i], building);
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

        CheckTasks(factories, BuildingType.factory);
        CheckTasks(markets, BuildingType.market);
        CheckTasks(tanks, BuildingType.tank);

        if (tasks.Count > 0) 
        {
            //Sort list in wieght order.
            for (int i = 0; i < tasks.Count - 1; i++)
            {
                for (int j = 0; j < tasks.Count - i -1; j++)
                {
                    if (tasks[j].weight > tasks[j + 1].weight) 
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

    protected int CheckIfCloser(Vector2Int destination, Vector2Int location, float current_mag, int i)
    {
        int rv = 0;

        float new_mag = (destination - location).sqrMagnitude;
        if (new_mag < current_mag)
        {
            current_mag = new_mag;
            rv = i;
        }

        return rv;
    }

    //Only use this funtion with MilkTanks or MilkCollectors.
    //For finding closest build with mice.
    protected MouseTemp FindClosestMouse(Task task, ParentBuilding[] buildings, BuildingType building)
    {
        float current_mag = float.MaxValue;
        int index = 0;

        if (buildings != null && buildings.Length > 0)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                IMilkContainer container;
                switch (building)
                {
                    case BuildingType.tank:
                        container = (MilkTank)buildings[i];
                        break;
                    default:
                        container = (MilkCollector)buildings[i];
                        break;
                }

                if (container.CURRENT_MILK_AMOUNT >= task.amounts[0] && buildings[i].Mouse_occupants.Count > 0)
                    index = CheckIfCloser(task.building.GetPosition(), buildings[i].GetPosition(), current_mag, i);
            }

            if(buildings[index].Mouse_occupants.Count > 0)
            {
                MouseTemp mouse = buildings[index].Mouse_occupants[0];
                buildings[index].MouseLeave(mouse);
                return mouse;
            }
        }

        return null;
    }

    //For finding closest mice to building.
    protected MouseTemp FindClosestMouse(Vector2Int building, MouseTemp[] mice)
    {
        float current_mag = float.MaxValue;
        int index = 0;

        if (mice != null && mice.Length > 0)
        {
            for (int i = 0; i < mice.Length; i++)
            {
                if (mice[i].Moving == false)
                    index = CheckIfCloser(building, mice[i].Position, current_mag, i);
            }

            if(mice[index].Home != null)
                mice[index].Home.MouseLeave(mice[index]); 

            return mice[index];
        }

        return null;
    }

    protected ParentBuilding FindClosestBuilding(Task task, ParentBuilding[] buildings, BuildingType buildingtype)
    {
        float current_mag = float.MaxValue;
        int index = 0;

        if (buildings.Any())
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                IMilkContainer container;
                switch (buildingtype)
                {
                    case BuildingType.tank:
                        container = (MilkTank)buildings[i];
                        break;
                    default:
                        container = (MilkCollector)buildings[i];
                        break;
                }

                if (container.CURRENT_MILK_AMOUNT >= task.amounts[0])
                    index = CheckIfCloser(task.building.GetPosition(), buildings[i].GetPosition(), current_mag, i);
            }

            return buildings[index];
        }

        return null;
    }

    protected MouseTemp FindMilk(Task task)
    {
        MouseTemp mouse_tank = FindClosestMouse(task, tanks, BuildingType.tank);

        MouseTemp mouse_collector = FindClosestMouse(task, collectors, BuildingType.collector);

        if (mouse_tank == null)
        {
            if (mouse_collector == null)
                return null;
            return mouse_collector;
        }
        else if (mouse_collector == null)
            return mouse_tank;
        //Checking which building is closser.
        Vector2Int task_loc = task.building.GetPosition();
        if ((task_loc - mouse_tank.Position).sqrMagnitude < (task_loc - mouse_collector.Position).sqrMagnitude)
            return mouse_tank;
        else
            return mouse_collector;
    }

    protected bool PickMouseInBuilding()
    {
        for (int i = tasks.Count - 1; i >= 0; i--) 
        {
            switch (tasks[i].building_type)
            {
                case BuildingType.factory:
                    MouseTemp factory_mouse = FindMilk(tasks[i]);

                    if (factory_mouse != null)
                    {
                        mice.TryAdd(tasks[i].building, factory_mouse);
                        factory_mouse.transform.gameObject.SetActive(true);
                        tasks.RemoveAt(i);
                    }
                    break;
                case BuildingType.tank:
                    MouseTemp tank_mouse = FindClosestMouse(tasks[i], collectors, BuildingType.collector);

                    if (tank_mouse != null)
                    {
                        mice.TryAdd(tasks[i].building, tank_mouse);
                        tank_mouse.transform.gameObject.SetActive(true);
                        tasks.RemoveAt(i);
                    }
                    break;
            }
        }
        if (mice.Count > 0)
        {
            MoveSingleRoute();
            return true;
        }
        return false;
    }

    protected void GetRoute(MouseTemp mouse, Vector2Int building_loc)
    {
        mouse.Path = null;
        mouse.Path = pathfinding.FindPath(mouse.Position, building_loc);

        if (mouse.Path == null)
            mouse.Path = pathfinding.CreatePath(mouse.Position, building_loc);

        if(mouse.Path != null)
            mouse.Moving = true;
    }

    protected void MoveSingleRoute()
    {
        foreach (var (key, value) in mice)
        {
            GetRoute(value, key.GetPosition());
        }
    } 

    protected ParentBuilding FindContainer(Task task)
    {
        ParentBuilding tank = FindClosestBuilding(task, tanks, BuildingType.tank);
        ParentBuilding collector = FindClosestBuilding(task, collectors, BuildingType.collector);

        if (tank == null)
        {
            if (collector == null)
                return null;
            return collector;
        }
        else if (collector == null)
            return tank;
        //Checking which building is closser.
        Vector2Int task_loc = task.building.GetPosition();
        if ((task_loc - tank.GetPosition()).sqrMagnitude < (task_loc - collector.GetPosition()).sqrMagnitude)
            return tank;
        else
            return collector;
    }

    protected void MoveMouse(MouseTemp mouse, ParentBuilding building, ParentBuilding next_building)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = building.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

        GetRoute(mouse, building_loc);

        //LERP mouse if fail start pathfinding again.
        if (mouse.Path != null)
        {
            mouse.Moving = true;
            mouse.Rigidbody = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (!success)
                {
                    MoveMouse(mouse, building, next_building);
                }
                else
                {
                    mouse.Moving = false;
                    GetRoute(mouse, next_building.GetPosition());
                    mice.TryAdd(next_building, mouse);
                }
            }));
        }
    }

    protected void MoveMouse(MouseTemp mouse, ParentBuilding building)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = building.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

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
                if (!success)
                {
                    MoveMouse(mouse, building);
                }
                else
                {
                    mouse.Moving = false;
                    mouse.Rigidbody = true;
                }
            }));
        }
    }

    protected bool PickMouse()
    {
        List<MouseTemp> mouses = new List<MouseTemp>();
        mouses.AddRange(FindObjectsOfType(typeof(MouseTemp)) as MouseTemp[]);

        for (int i = 0; i < mouses.Count; i++)
        {
            if (mouses[i].Moving == true)
                mouses.RemoveAt(i);
        }

        MouseTemp mouse = gameObject.AddComponent<MouseTemp>();

        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            switch (tasks[i].building_type)
            {
                case BuildingType.factory:
                    ParentBuilding milk_building = FindContainer(tasks[i]);

                    if (milk_building != null)
                    {
                        mouse = FindClosestMouse(milk_building.GetPosition(), mouses.ToArray());

                        if (mouse != null)
                        {
                            mouses.Remove(mouse);
                            mouse.Moving = true;
                            MoveMouse(mouse, milk_building, tasks[i].building);
                        }
                    }
                    break;
                case BuildingType.market:
                        mouse = FindClosestMouse(tasks[i].building.GetPosition(), mouses.ToArray());

                        if (mouse != null)
                        {
                            mouses.Remove(mouse);
                            mouse.Moving = true;
                            GetRoute(mouse, tasks[i].building.GetPosition());
                            mice.TryAdd(tasks[i].building, mouse);
                        }
                    break;
                case BuildingType.tank:
                    if (collectors.Any())
                    {
                        ParentBuilding colletor_building = FindClosestBuilding(tasks[i], collectors, BuildingType.collector);

                        if(colletor_building != null)
                        {
                            mouse = FindClosestMouse(colletor_building.GetPosition(), mouses.ToArray());

                            if (mouse != null)
                            {
                                mouses.Remove(mouse);
                                mouse.Moving = true;
                                MoveMouse(mouse, colletor_building, tasks[i].building);
                            }
                        }
                    }
                    break;
            }
        }

        if (mice.Count > 0)
            return true;
        return false;
    }

    protected void Pathfinding()
    {
        foreach (var (key, value) in mice) 
        {
            value.Moving = false;
            value.Home.MouseLeave(value);

            //LERP mouse if fail start pathfinding again.
            if (value.Path != null)
            {
                value.Moving = true;
                value.Rigidbody = false;
                StartCoroutine(value.FollowPath((success) =>
                {
                    if (!success)
                    {
                        MoveMouse(value, key);
                    }
                    else
                    {
                        value.Moving = false;
                        value.Rigidbody = true;
                        mice.Remove(key);
                        value.Moving = false;
                        pathfinding.SavePath(value.Position, key.GetPosition(), value.Path);
                    }
                }));
            }
        }
    }

    protected void BehaviourTree()
    {
        //Sequence
        if (FindBuildings())
        {
            if (PickTasks())
            {
                //Selector
                if (PickMouseInBuilding() == false && PickMouse() == false)
                {

                }
                else
                    Pathfinding();
            }
        }
    }
}