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
    protected bool IsActive;

    //Buildings for creating tasks.
    protected CommercialBuilding[] markets;
    protected List<IMilkContainer> factories;
    protected List<IMilkContainer> tanks;
    protected List<IMilkContainer> containers;
    List<MouseTemp> mouses;

    protected ResourceManager resources;

    protected List<MilkTask> milk_tasks;
    protected List<CheeseTask> cheese_tasks;
    protected Dictionary<MouseTemp, MilkTask> mice_route;


    //struct used to convert all classes into a manageable system
    protected struct MilkTask
    {
        public ParentBuilding building;
        public float weight;
        public int amount;
    }
    protected struct CheeseTask
    {
        public CommercialBuilding market;
        public float weight;
        public List<CheeseTypes> types;
    }

    public Mouse_AI() 
    {
        scrap_weight  = 0.5f;
        cheese_weight = 0.5f;
        milk_weight   = 0.5f;

        overtime_multiple = 1.1f;
        tree_cycle = 15;
        IsActive = true;

        milk_tasks = new List<MilkTask>();
        cheese_tasks = new List<CheeseTask>();
        mice_route = new Dictionary<MouseTemp, MilkTask>();
    }

    //Get all the building that will be used to create tasks.
    //Tanks are not necessary because the collectors produce the milk.
    protected bool FindBuildings()
    {
        markets = FindObjectsOfType(typeof(CommercialBuilding)) as CommercialBuilding[];

        milkmanager.RefreshRankings();
        factories  = milkmanager.ranked_factories;
        tanks      = milkmanager.ranked_tanks;
        containers = milkmanager.ranked_containers;

        if (factories.Any() || markets.Any() || containers.Any())
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
    protected bool TasksContainsBuilding(CommercialBuilding market)
    {
        for (int i = 0; i < cheese_tasks.Count; i++)
        {
            if (cheese_tasks[i].market == market)
            {
                //Update weight per cycle
                CheeseTask temp = cheese_tasks[i];
                temp.weight *= overtime_multiple;
                cheese_tasks[i] = temp;

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
            MilkTask new_taks = new()
            {
                building = factory,
                weight = 1 * cheese_weight,
                amount = factory.MilkToAdd()
            };

            milk_tasks.Add(new_taks);
        }
    }
    protected void CommercialTask(CommercialBuilding market)
    {
        List<CheeseTypes> keys = new();

        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
        {
            int cheese_amount = market.CheeseAmount(c);
            bool enough = resources.CanAfford(c, cheese_amount);
            if (enough == true)
                keys.Add(c);
        }

        if (keys.Count > 0)
        {
            CheeseTask new_taks = new()
            {
                market = market,
                weight = 1 * scrap_weight,
                types = keys
            };

            cheese_tasks.Add(new_taks);
        }
    }
    protected void TankTask(MilkTank tank)
    {
        MilkTask new_taks = new()
        {
            building = tank,
            weight = 1 * milk_weight,
            amount = tank.MilkToAdd()
        };

        milk_tasks.Add(new_taks);
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

        //Checks whether a task needs to be created or updated for all buildings that require a resource added.
        foreach (IMilkContainer c in factories)
        {
            if (!TasksContainsBuilding((FactoryBuilding)c))
                FactoryTask((FactoryBuilding)c);
        }
        foreach (IMilkContainer c in tanks)
        {
            if (!TasksContainsBuilding((MilkTank)c))
                TankTask((MilkTank)c);
        }
        foreach (CommercialBuilding m in markets)
        {
            if (!TasksContainsBuilding(m))
                CommercialTask(m);
        }

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
        }

        if (cheese_tasks.Count > 0)
        {
            //Sort list in wieght order from lowest to hights because we will loop over the list backwords.
            for (int i = 0; i < cheese_tasks.Count - 1; i++)
            {
                for (int j = 0; j < cheese_tasks.Count - i - 1; j++)
                {
                    if (cheese_tasks[j].weight > cheese_tasks[j + 1].weight)
                    {
                        CheeseTask temp = cheese_tasks[j];
                        cheese_tasks[j] = cheese_tasks[j + 1];
                        cheese_tasks[j + 1] = temp;
                    }
                }
            }
        }

        if (milk_tasks.Any() || cheese_tasks.Any())
            return true;
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
                ParentBuilding building = null;
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
                    if (!building.Mouse_occupants[j].Moving && containers[i].CanAfford(task.amount))
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
            containers[index1].SubtractMilk(task.amount);
            ParentBuilding building = (ParentBuilding)containers[index1];
            MouseTemp mouse = building.Mouse_occupants[index2];
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
                ParentBuilding building = null;
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
            GetRoute(key, value.building.GetPosition());
        }
    }

    protected List<IMilkContainer> GetCollectors()
    {
        List<IMilkContainer> rv = new();

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
                        if (mice_route.TryAdd(factory_mouse, milk_tasks[i])) 
                            milk_tasks.RemoveAt(i);
                    }
                    break;
                case BuildingType.tank:
                    //tanks can only be filled by the collecter.
                    List<IMilkContainer> collectors = GetCollectors();
                    MouseTemp tank_mouse = FindClosestMouseInBuilding(milk_tasks[i], collectors);

                    if (tank_mouse != null)
                    {
                        if(mice_route.TryAdd(tank_mouse, milk_tasks[i]))
                            milk_tasks.RemoveAt(i);
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

    //For moving mouse to two buildings because there were no mice in the right building.
    protected void MoveMouse(MouseTemp mouse, ParentBuilding building, MilkTask task)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = building.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

        if (mouse.Home != null)
            mouse.Home.MouseLeave(mouse);

        GetRoute(mouse, building_loc);

        //LERP mouse if fail start pathfinding again.
        if (mouse.Moving)
        {
            //Stop mice entering wrong building whilst LERPing.
            mouse.Collider = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (!success)
                {
                    MoveMouse(mouse, building, task);
                }
                else
                {
                    IMilkContainer container = (IMilkContainer)building;
                    container.SubtractMilk(task.amount);
                    mouse.Home.MouseLeave(mouse);
                    GetRoute(mouse, task.building.GetPosition());
                    MoveMouse(mouse, task);
                }
            }));
        }
    }

    //For moving the mouse to one building if it is in the correct building, and save the path,
    //because mouse movement from building to building is more likely to happen again than mouse movement from anywhere.
    protected void MoveMouse(MouseTemp mouse, MilkTask task)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = task.building.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

        if (mouse.Home != null)
            mouse.Home.MouseLeave(mouse);

        //For saving path.
        List<BaseNode> path = mouse.Path;

        //LERP mouse if fail start pathfinding again.
        if (mouse.Moving)
        {
            mouse.Collider = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (!success)
                {
                    MoveMouse(mouse, task);
                }
                else
                {
                    IMilkContainer container = (IMilkContainer)task.building;
                    container.AddMilk(task.amount);
                    mouse.Moving = false;
                    pathfinding.SavePath(mouse_loc, building_loc, path);
                }
            }));
        }
    }

    protected void MoveMouse(MouseTemp mouse, CheeseTask task)
    {
        //Convert world space to grid coordinates
        Vector2Int building_loc = task.market.GetPosition();
        Vector2Int mouse_loc = mouse.Position;

        if (mouse.Home != null)
            mouse.Home.MouseLeave(mouse);

        //For saving path.
        List<BaseNode> path = mouse.Path;

        //LERP mouse if fail start pathfinding again.
        if (mouse.Moving)
        {
            mouse.Collider = false;
            StartCoroutine(mouse.FollowPath((success) =>
            {
                if (!success)
                {
                    MoveMouse(mouse, task);
                }
                else
                {
                    task.market.AddCheese(task.types);
                    mouse.Moving = false;
                    pathfinding.SavePath(mouse_loc, building_loc, path);
                }
            }));
        }
    }

    protected int MilkDoubleTask(MouseTemp mouse, int index)
    {
        switch (milk_tasks[index].building.Building_type)
        {
            case BuildingType.factory:
                //Finding the correct building to go to first then building that created task.
                ParentBuilding milk_building = FindClosestBuilding(milk_tasks[index], containers);

                if (milk_building != null)
                {
                    mouse = FindClosestMouse(milk_building.GetPosition(), mouses.ToArray());

                    if (mouse != null)
                    {
                        //mouse can't be picked again in this cycle.
                        mouses.Remove(mouse);

                        //Move mouse to both buildings.
                        MoveMouse(mouse, milk_building, milk_tasks[index]);
                    }
                }
                break;
            case BuildingType.tank:
                List<IMilkContainer> collectors = GetCollectors();
                ParentBuilding colletor_building = FindClosestBuilding(milk_tasks[index], collectors);

                if (colletor_building != null)
                {
                    mouse = FindClosestMouse(colletor_building.GetPosition(), mouses.ToArray());

                    if (mouse != null)
                    {
                        mouses.Remove(mouse);
                        MoveMouse(mouse, colletor_building, milk_tasks[index]);
                    }
                }
                break;
        }
        return --index;
    }

    protected int CheeseDoubleTask(MouseTemp mouse, int index)
    {
        mouse = FindClosestMouse(cheese_tasks[index].market.GetPosition(), mouses.ToArray());

        if (mouse != null)
        {
            mouses.Remove(mouse);
            mouse.Moving = true;
            GetRoute(mouse, cheese_tasks[index].market.GetPosition());
            MoveMouse(mouse, cheese_tasks[index]);
        }
        return --index;
    }

    //This is to pick mouse to go to two building for task.
    protected bool PickMouse()
    {
        //For checking if any task was fulfilled.
        int change = mice_route.Count;

        //Get all mice that are not moving mouse might be in third building.
        mouses = PopulationManager.GetMice();

        if (!mouses.Any())
            return false;

        for (int i = 0; i < mouses.Count; i++)
        {
            if (mouses[i].Moving)
                mouses.RemoveAt(i);
        }
        
        MouseTemp mouse = null;

        int milk_indedx = milk_tasks.Count - 1;
        int cheese_index = cheese_tasks.Count - 1;
        while (mouses.Count > 0 && (milk_tasks.Count > 0 || cheese_tasks.Count > 0)) 
        {
            if(milk_tasks.Count == 0)
                CheeseDoubleTask(mouse, cheese_index);
            else if (cheese_tasks.Count == 0)
                MilkDoubleTask(mouse, milk_indedx);
            else
            {
                if (milk_tasks[milk_indedx].weight > cheese_tasks[cheese_index].weight)
                    MilkDoubleTask(mouse, milk_indedx);
                else
                    CheeseDoubleTask(mouse, cheese_index);
            }
        }

        //If no tasks are fulfilled, fail.
        if (change == mice_route.Count)
            return false;
        return true;
    }

    //Take all the mice that are ready to complete task and move them.
    protected void Pathfinding()
    {
        foreach (var (key, value) in mice_route) 
        {
            MoveMouse(key, value);
        }
    }

    protected void BehaviourTree()
    {
        if (IsActive)
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
        }

        Invoke(nameof(BehaviourTree), tree_cycle);
    }

    protected void Start()
    {
        MouseTemp.Grid_manager = grid_manager;
        pathfinding.Grid_manager = grid_manager;
        BehaviourTree();

        if (UImGui.DebugWindow.Instance != null)
        {
            UImGui.DebugWindow.Instance.RegisterExternalCommand("AI.true", " - Allows the AI to be active and control the mice.", args =>
            {
                UImGui.DebugWindow.LogToConsole("Mouse_AI Activated:");
                IsActive = true;
            });
        }

        if (UImGui.DebugWindow.Instance != null)
        {
            UImGui.DebugWindow.Instance.RegisterExternalCommand("AI.false", " - Stops the AI and the control of the mice.", args =>
            {
                UImGui.DebugWindow.LogToConsole("Mouse_AI Deactivated:");
                IsActive = false;
            });
        }
    }

    // jess 27/02/2026
    // saving implementation fixes
    public void ResetAI()
    {
        if (mice_route != null)
        {
            mice_route.Clear();
        }

        StopAllCoroutines();
    }
}