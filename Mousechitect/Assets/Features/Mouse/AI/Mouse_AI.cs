using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Create by Iain Benner 15/02/2026

/// <summary>
/// AI controls where all the mice need to go, whilst the pathfinding controls how they get there.
/// weights are created with player input and current resources to decide which tasks to do first.
/// allows mice to move wherever they are in the right building, for the task or not.
/// </summary>

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

    //How much the tast weight is increased per cycle if it is not completed.
    protected float overtime_multiple;

    //How long the next cycle happens after the last cycle is completed seconds.
    protected float tree_cycle;

    //Buildings for creating tasks.
    protected FactoryBuilding[] factories;
    protected CommercialBuilding[] markets;
    protected MilkTank[] tanks;
    protected MilkCollector[] collectors;

    protected ResourceManager resources;

    protected List<Task> tasks;
    protected Dictionary<ParentBuilding, MouseTemp> mice_route;


    //struct used to convert all classes into a manageable system
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
        tree_cycle = 15;

        tasks = new List<Task>();
        mice_route = new Dictionary<ParentBuilding, MouseTemp>();
    }

    //Get all the building that will be used to create tasks.
    //Tanks are not necessary because the collectors produce the milk.
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

    //Weight gets upadted if the task exsists else create task.
    protected bool TasksContainsBuilding(ParentBuilding building)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].building == building)
            {
                //Update weight per cycle
                Task temp = tasks[i];
                temp.weight *= overtime_multiple;
                tasks[i] = temp;

                return true;
            }
        }
        return false;
    }

    //Collector deos not have task because milk does not get put in it.
    //The weights applied all read have all factors combined.
    protected void FactoryTask(FactoryBuilding factory, ParentBuilding building)
    {
        if (factory.IsActive && factory.CURRENT_MILK_AMOUNT < factory.MAX_MILK_CAPACITY)
        {
            Task new_taks = new Task();

            new_taks.building = building;
            new_taks.weight = 1 * cheese_weight;
            new_taks.building_type = BuildingType.factory;
            new_taks.amounts = new int[1];
            new_taks.amounts[0] = factory.MAX_MILK_CAPACITY - factory.CURRENT_MILK_AMOUNT;

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
        new_taks.amounts[0] = tank.MAX_MILK_CAPACITY - tank.CURRENT_MILK_AMOUNT;

        tasks.Add(new_taks);
    }

    //Checks whether a task needs to be created or updated for all buildings that require a resource added.
    protected void CheckTasks(ParentBuilding[] buildings, BuildingType building_type)
    {
        if (buildings != null)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                ParentBuilding building = buildings[i];

                //Weight gets upadted if the task exsists else create task
                if (TasksContainsBuilding(building) == false)
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

    //Will create and update tasks with the latest weights and sort the tasks into a list.
    protected bool Tasks()
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

        //Checks if task needs to updated or created.
        CheckTasks(factories, BuildingType.factory);
        CheckTasks(markets, BuildingType.market);
        CheckTasks(tanks, BuildingType.tank);

        if (tasks.Count > 0) 
        {
            //Sort list in wieght order from lowest to hights because we will loop over the list backwords.
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

    //returns clostest index based on magnitude
    protected int CheckIfCloser(Vector2Int destination, Vector2Int location, float current_mag, int index, int i)
    {
        int rv = index;

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
    protected MouseTemp FindClosestMouseInBuilding(Task task, ParentBuilding[] buildings, BuildingType building)
    {
        float current_mag = float.MaxValue;
        int index1 = 0;
        int index2 = 0;
        bool safe = false;

        if (buildings != null && buildings.Length > 0)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                //Cast to get milk information which the parent building does not hold.
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

                //Get index of closest building and eligible mouse.
                for (int j = 0; j < buildings[i].Mouse_occupants.Count; j++)
                {
                    if (container.CURRENT_MILK_AMOUNT >= task.amounts[0] && !buildings[i].Mouse_occupants[j].Moving)
                    {
                        index1 = CheckIfCloser(task.building.GetPosition(), buildings[i].GetPosition(), current_mag, index1, i);
                        index2 = j;
                        safe = true;
                    }
                }
            }

            //If building and mouse is found
            if(safe)
            {
                MouseTemp mouse = buildings[index1].Mouse_occupants[index2];
                //mouse leave home so it can be moved to the task.
                buildings[index1].MouseLeave(mouse);
                return mouse;
            }
        }

        return null;
    }

    //For finding closest mice to building.
    //Not pacificly for find the mouse in the building, but it might happen.
    protected MouseTemp FindClosestMouse(Vector2Int building, MouseTemp[] mice)
    {
        float current_mag = float.MaxValue;
        int index = 0;

        if (mice != null && mice.Length > 0)
        {
            for (int i = 0; i < mice.Length; i++)
            {
                if (mice[i].Moving == false)
                    index = CheckIfCloser(building, mice[i].Position, current_mag, index, i);
            }

            if(mice[index].Home != null)
                mice[index].Home.MouseLeave(mice[index]); 

            return mice[index];
        }

        return null;
    }

    //Only use this funtion with MilkTanks or MilkCollectors.
    //For finding the closest building to the task, which is a building,
    //because mice might have to go to two buildings if no mice can be found in the desired building.
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
                    index = CheckIfCloser(task.building.GetPosition(), buildings[i].GetPosition(), current_mag, index, i);
            }

            return buildings[index];
        }

        return null;
    }

    //Decided if tank or collector is the closest option.
    protected MouseTemp FindMilk(Task task)
    {
        MouseTemp mouse_tank = FindClosestMouseInBuilding(task, tanks, BuildingType.tank);

        MouseTemp mouse_collector = FindClosestMouseInBuilding(task, collectors, BuildingType.collector);

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

    protected void GetRoute(MouseTemp mouse, Vector2Int building_loc)
    {
        mouse.Path = null;
        mouse.Path = pathfinding.FindPath(mouse.Position, building_loc);

        if (mouse.Path == null)
            mouse.Path = pathfinding.CreatePath(mouse.Position, building_loc);

        if (mouse.Path != null)
            mouse.Moving = true;
    }

    //For mice that are already in the correct building and only need to go to one offer.
    protected void MakeSingleRoute()
    {
        foreach (var (key, value) in mice_route)
        {
            GetRoute(value, key.GetPosition());
        }
    }

    //These task only need the mouse to moved to one building making things simplar.
    protected bool PickTaskWithMouseInBuilding()
    {
        //For checking if any task was fulfilled.
        int change = mice_route.Count;

        for (int i = tasks.Count - 1; i >= 0; i--) 
        {
            switch (tasks[i].building_type)
            {
                case BuildingType.factory:
                    //Find mouse inside building.
                    MouseTemp factory_mouse = FindMilk(tasks[i]);

                    if (factory_mouse != null)
                    {
                        if (mice_route.TryAdd(tasks[i].building, factory_mouse)) 
                        {
                            factory_mouse.transform.gameObject.SetActive(true);
                            tasks.RemoveAt(i);
                        }
                    }
                    break;
                case BuildingType.tank:
                    //tanks can only be filled by the collecter.
                    MouseTemp tank_mouse = FindClosestMouseInBuilding(tasks[i], collectors, BuildingType.collector);

                    if (tank_mouse != null)
                    {
                        if(mice_route.TryAdd(tasks[i].building, tank_mouse))
                        {
                            tank_mouse.transform.gameObject.SetActive(true);
                            tasks.RemoveAt(i);
                        }
                    }
                    break;
            }
        }
        //If no task fulfilled, fail.
        if (change == mice_route.Count)
            return false;
        else
        {
            MakeSingleRoute();
            return true;
        }
    }

    //Same as FindMilk(), but return the building because if there are no mice in the right building, we need the building.
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

    //For moving mouse to two buildings because there were no mice in the right building.
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
            //Stop mice entering wrong building whilst LERPing.
            mouse.Collider = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (!success)
                {
                    MoveMouse(mouse, building, next_building);
                }
                else
                {
                    mouse.Home.MouseLeave(mouse);
                    GetRoute(mouse, next_building.GetPosition());
                    MoveMouse(mouse, next_building);
                }
            }));
        }
    }

    //For moving the mouse to one building if it is in the correct building, and save the path,
    //because mouse movement from building to building is more likely to happen again than mouse movement from anywhere.
    protected void MoveMouse(MouseTemp mouse, ParentBuilding building)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = building.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

        //For saving path.
        List<BaseNode> path = mouse.Path;

        //LERP mouse if fail start pathfinding again.
        if (mouse.Path != null)
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

    //This is to pick mouse to go to two building for task.
    protected bool PickMouse()
    {
        //For checking if any task was fulfilled.
        int change = mice_route.Count;

        //Get all mice that are not moving mouse might be in third building.
        List<MouseTemp> mouses = new List<MouseTemp>();
        mouses.AddRange(GameObject.FindObjectsOfType(typeof(MouseTemp), true) as MouseTemp[]);

        for (int i = 0; i < mouses.Count; i++)
        {
            if (mouses[i].Moving)
                mouses.RemoveAt(i);
        }
        
        MouseTemp mouse = new MouseTemp();

        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            switch (tasks[i].building_type)
            {
                case BuildingType.factory:
                    //Finding the correct building to go to first then building that created task.
                    ParentBuilding milk_building = FindContainer(tasks[i]);

                    if (milk_building != null)
                    {
                        mouse = FindClosestMouse(milk_building.GetPosition(), mouses.ToArray());

                        if (mouse != null)
                        {
                            //Mouse might be in third building so needs to leave.
                            if (mouse.Home != null)
                                mouse.Home.MouseLeave(mouse);

                            //mouse can't be picked again in this cycle.
                            mouses.Remove(mouse);

                            //Move mouse to both buildings.
                            mouse.Moving = true;
                            MoveMouse(mouse, milk_building, tasks[i].building);
                        }
                    }
                    break;
                case BuildingType.market:
                        mouse = FindClosestMouse(tasks[i].building.GetPosition(), mouses.ToArray());

                        if (mouse != null)
                        {
                            if (mouse.Home != null)
                                mouse.Home.MouseLeave(mouse);
                            mouses.Remove(mouse);
                            mouse.Moving = true;
                            GetRoute(mouse, tasks[i].building.GetPosition());
                            mice_route.TryAdd(tasks[i].building, mouse);
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
                                if (mouse.Home != null)
                                    mouse.Home.MouseLeave(mouse);
                                mouses.Remove(mouse);
                                mouse.Moving = true;
                                MoveMouse(mouse, colletor_building, tasks[i].building);
                            }
                        }
                    }
                    break;
            }
        }
        //If no task fulfilled, fail.
        if (change == mice_route.Count)
            return false;
        return true;
    }

    //Take all the mice that are ready to complete task and move them.
    protected void Pathfinding()
    {
        foreach (var (key, value) in mice_route) 
        {
            MoveMouse(value, key);
        }
    }

    protected void BehaviourTree()
    {
        //Sequence
        if (FindBuildings())
        {
            if (Tasks())
            {
                //Selector
                if (PickTaskWithMouseInBuilding() == false && PickMouse() == false)
                {

                }
                else
                    Pathfinding();
            }
        }

        Invoke(nameof(BehaviourTree), 15);
    }

    protected void Start()
    {
        if (UImGui.DebugWindow.Instance != null)
        {
            UImGui.DebugWindow.Instance.RegisterExternalCommand("Mouse_AI", " - Allows the AI to be active and control the mice.", args =>
            {
                UImGui.DebugWindow.LogToConsole("Mouse_AI Activated:");
                MouseTemp.Grid_manager = grid_manager;
                pathfinding.Grid_manager = grid_manager;
                BehaviourTree();
            });
        }
    }
}