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
    //protected CommercialBuilding[] markets;
    protected List<IMilkContainer> factories;
    protected List<IMilkContainer> tanks;
    protected List<IMilkContainer> containers;

    protected ResourceManager resources;

    protected List<MilkTask> milk_tasks;
    protected Dictionary<ParentBuilding, MouseTemp> mice_route;


    //struct used to convert all classes into a manageable system
    protected struct MilkTask
    {
        public ParentBuilding building;
        public float weight;
        public int amount;
    }

    public Mouse_AI() 
    {
        scrap_weight  = 0.5f;
        cheese_weight = 0.5f;
        milk_weight   = 0.5f;

        overtime_multiple = 1.1f;
        tree_cycle = 15;

        milk_tasks = new List<MilkTask>();
        mice_route = new Dictionary<ParentBuilding, MouseTemp>();
    }

    //Get all the building that will be used to create tasks.
    //Tanks are not necessary because the collectors produce the milk.
    protected bool FindBuildings()
    {
        //markets = FindObjectsOfType(typeof(CommercialBuilding)) as CommercialBuilding[];

        milkmanager.RefreshRankings();
        factories  = milkmanager.ranked_factories;
        tanks      = milkmanager.ranked_tanks;
        containers = milkmanager.ranked_containers;

        if (factories.Any() /*|| markets.Any()*/ || containers.Any())
            return true;
        return false;
    }

    //Weight gets upadted if the task exsists else create task.
    protected bool TasksContainsBuilding(ParentBuilding building)
    {
        for (int i = 0; i < milk_tasks.Count; i++)
        {
            if (milk_tasks[i].building == building)
            {
                //Update weight per cycle
                MilkTask temp = milk_tasks[i];
                temp.weight *= overtime_multiple;
                milk_tasks[i] = temp;

                return true;
            }
        }
        return false;
    }

    //Collector deos not have task because milk does not get put in it.
    //The weights applied all read have all factors combined.
    protected void FactoryTask(FactoryBuilding factory)
    {
        if (factory.IsActive && factory.CURRENT_MILK_AMOUNT < factory.MAX_MILK_CAPACITY)
        {
            MilkTask new_taks = new MilkTask();

            new_taks.building = factory;
            new_taks.weight = 1 * cheese_weight;
            new_taks.amount = factory.MilkToAdd();

            milk_tasks.Add(new_taks);
        }
    }
    //protected void CommercialTask(CommercialBuilding market, ParentBuilding building)
    //{
    //    List<CheeseTypes> keys = new List<CheeseTypes>();
    //    List<int> amounts = new List<int>();

    //    foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
    //    {
    //        int cheese_amount = market.CheeseAmount(c);
    //        bool enough = resources.CanAfford(c, cheese_amount);
    //        if (enough == true)
    //        {
    //            keys.Add(c);
    //            amounts.Add(cheese_amount);
    //        }
    //    }

    //    Task new_taks = new Task();

    //    new_taks.building = building;
    //    new_taks.weight = 1 * scrap_weight;
    //    new_taks.building_type = BuildingType.market;
    //    new_taks.amounts = amounts.ToArray();
    //    new_taks.cheese_types = keys.ToArray();

    //    tasks.Add(new_taks);
    //}
    protected void TankTask(MilkTank tank)
    {
        MilkTask new_taks = new MilkTask();

        new_taks.building = tank;
        new_taks.weight = 1 * milk_weight;
        new_taks.amount = tank.MilkToAdd();

        milk_tasks.Add(new_taks);
    }

    //Checks whether a task needs to be created or updated for all buildings that require a resource added.
    protected void CheckTasks(List<IMilkContainer> containers)
    {
        if (containers != null)
        {
            for (int i = 0; i < containers.Count; i++)
            {
                IMilkContainer container = containers[i];

                //Weight gets upadted if the task exsists else create task
                switch (container.Building_type)
                {
                    case BuildingType.factory:
                        if (!TasksContainsBuilding((FactoryBuilding)container))
                            FactoryTask((FactoryBuilding)container);
                        break;
                    //case BuildingType.market:
                    //    CommercialTask((CommercialBuilding)buildings[i], building);
                    //    break;
                    case BuildingType.tank:
                        if (!TasksContainsBuilding((MilkTank)container))
                            TankTask((MilkTank)container);
                        break;
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

        //Calculate average cheese for each resource and total cheese.
        //resource diveided by average cost to find how many average cheese are there in that recourse.
        float scrap_cheese = resources.Scrap      / (total_scrap_cost / Enum.GetValues(typeof(CheeseTypes)).Length);
        float milk_cheese  = resources.total_milk / (total_milk_cost / Enum.GetValues(typeof(CheeseTypes)).Length);
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
        CheckTasks(factories);
        //CheckTasks(markets);
        CheckTasks(tanks);

        if (milk_tasks.Count > 0) 
        {
            //Sort list in wieght order from lowest to hights because we will loop over the list backwords.
            for (int i = 0; i < milk_tasks.Count - 1; i++)
            {
                for (int j = 0; j < milk_tasks.Count - i -1; j++)
                {
                    if (milk_tasks[j].weight > milk_tasks[j + 1].weight) 
                    {
                        MilkTask temp = milk_tasks[j];
                        milk_tasks[j] = milk_tasks[j + 1];
                        milk_tasks[j + 1] = temp;
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
    protected MouseTemp FindClosestMouseInBuilding(MilkTask task, List<IMilkContainer> containers)
    {
        float current_mag = float.MaxValue;
        int index1 = 0;
        int index2 = 0;
        bool safe = false;

        if (containers.Any())
        {
            for (int i = 0; i < containers.Count; i++)
            {
                ParentBuilding building = new ParentBuilding();
                switch (containers[i].Building_type)
                {
                    case BuildingType.tank:
                        building = (MilkTank)containers[i];
                        break;
                    case BuildingType.collector:
                        building = (MilkCollector)containers[i];
                        break;
                }
                //Get index of closest building and eligible mouse.
                for (int j = 0; j < building.Mouse_occupants.Count; j++)
                {
                    if (!building.Mouse_occupants[j].Moving)
                    {
                        index1 = CheckIfCloser(task.building.GetPosition(), building.GetPosition(), current_mag, index1, i);
                        index2 = j;
                        safe = true;
                    }
                }
            }
        }

        //If building and mouse is found
        if (safe)
        {
            ParentBuilding building = (ParentBuilding)containers[index1];
            MouseTemp mouse = building.Mouse_occupants[index2];
            //mouse leave home so it can be moved to the task.
            building.MouseLeave(mouse);
            return mouse;
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
    protected ParentBuilding FindClosestBuilding(MilkTask task, List<IMilkContainer> containers)
    {
        float current_mag = float.MaxValue;
        int index = 0;

        if (containers.Any())
        {
            for (int i = 0; i < containers.Count; i++)
            {
                ParentBuilding building = new ParentBuilding();
                switch (containers[i].Building_type)
                {
                    case BuildingType.tank:
                        building = (MilkTank)containers[i];
                        break;
                    case BuildingType.collector:
                        building = (MilkCollector)containers[i];
                        break;
                }
                index = CheckIfCloser(task.building.GetPosition(), building.GetPosition(), current_mag, index, i);
            }

            return (ParentBuilding)containers[index];
        }

        return null;
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

    protected List<IMilkContainer> GetCollectors()
    {
        List<IMilkContainer> rv = new List<IMilkContainer>();

        foreach (IMilkContainer c in containers)
        {
            if(c.Building_type == BuildingType.collector)
                rv.Add(c);
        }

        return rv;
    }

    //These task only need the mouse to moved to one building making things simplar.
    protected bool PickTaskWithMouseInBuilding()
    {
        //For checking if any task was fulfilled.
        int change = mice_route.Count;

        for (int i = milk_tasks.Count - 1; i >= 0; i--) 
        {
            switch (milk_tasks[i].building.Building_type)
            {
                case BuildingType.factory:
                    //Find mouse inside building.
                    MouseTemp factory_mouse = FindClosestMouseInBuilding(milk_tasks[i], containers);

                    if (factory_mouse != null)
                    {
                        if (mice_route.TryAdd(milk_tasks[i].building, factory_mouse)) 
                        {
                            factory_mouse.transform.gameObject.SetActive(true);
                            milk_tasks.RemoveAt(i);
                        }
                    }
                    break;
                case BuildingType.tank:
                    //tanks can only be filled by the collecter.
                    List<IMilkContainer> collectors = GetCollectors();
                    MouseTemp tank_mouse = FindClosestMouseInBuilding(milk_tasks[i], collectors);

                    if (tank_mouse != null)
                    {
                        if(mice_route.TryAdd(milk_tasks[i].building, tank_mouse))
                        {
                            tank_mouse.transform.gameObject.SetActive(true);
                            milk_tasks.RemoveAt(i);
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

        for (int i = milk_tasks.Count - 1; i >= 0; i--)
        {
            switch (milk_tasks[i].building.Building_type)
            {
                case BuildingType.factory:
                    //Finding the correct building to go to first then building that created task.
                    ParentBuilding milk_building = FindClosestBuilding(milk_tasks[i], containers);

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
                            MoveMouse(mouse, milk_building, milk_tasks[i].building);
                        }
                    }
                    break;
                //case BuildingType.market:
                //        mouse = FindClosestMouse(milk_tasks[i].building.GetPosition(), mouses.ToArray());

                //        if (mouse != null)
                //        {
                //            if (mouse.Home != null)
                //                mouse.Home.MouseLeave(mouse);
                //            mouses.Remove(mouse);
                //            mouse.Moving = true;
                //            GetRoute(mouse, milk_tasks[i].building.GetPosition());
                //            mice_route.TryAdd(milk_tasks[i].building, mouse);
                //        }
                    //break;
                case BuildingType.tank:
                    List <IMilkContainer> collectors = GetCollectors();
                    ParentBuilding colletor_building = FindClosestBuilding(milk_tasks[i], collectors);

                    if (colletor_building != null)
                    {
                        mouse = FindClosestMouse(colletor_building.GetPosition(), mouses.ToArray());

                        if (mouse != null)
                        {
                            if (mouse.Home != null)
                                mouse.Home.MouseLeave(mouse);
                            mouses.Remove(mouse);
                            mouse.Moving = true;
                            MoveMouse(mouse, colletor_building, milk_tasks[i].building);
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

        Invoke(nameof(BehaviourTree), tree_cycle);
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