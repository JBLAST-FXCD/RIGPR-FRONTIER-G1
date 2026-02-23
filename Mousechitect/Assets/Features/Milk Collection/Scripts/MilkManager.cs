using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UImGui;
using UnityEngine;

// jess @ 03/02/2026
// <summary>
// manages all milk containers in the game (collectors and tanks).
// handles ranking of containers inline with GDD requirements.
// provides milk sources to buildings that require milk.
// </summary>
public class MilkManager : MonoBehaviour
{
    public static MilkManager Instance;

    public List<IMilkContainer> all_containers { get; private set; } = new List<IMilkContainer>();
    public List<IMilkContainer> ranked_containers { get; private set; } = new List<IMilkContainer>();
    public List<IMilkContainer> ranked_factories { get; private set; } = new List<IMilkContainer>();
    public List<IMilkContainer> ranked_tanks { get; private set; } = new List<IMilkContainer>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MilkConsoleCommands();
    }

    public void RegisterContainer(IMilkContainer container)
    {
        // adds container to the master list
        all_containers.Add(container);
    }

    public void UnregisterContainer(IMilkContainer container)
    {
        // removes container from the master list
        all_containers.Remove(container);
    }

    public void RefreshRankings()
    {
        List<IMilkContainer> temp = all_containers;
        temp.RemoveAll(c => c.CONTAINER_GAME_OBJECT == null);

        for (int i = 0; i < temp.Count; i++) 
        {
            switch (temp[i].Building_type)
            {
                case BuildingType.factory:
                    //Checking if theres room to add milk
                    if (temp[i].MilkToAdd() != 0)
                        ranked_factories.Add(temp[i]);
                    break;
                case BuildingType.tank:
                    //Checking if theres room to add milk
                    if (temp[i].MilkToAdd() != 0)
                        ranked_tanks.Add(temp[i]);
                    //Checking if theres milk to remove
                    if (temp[i].CanAfford(1))
                        ranked_containers.Add(temp[i]);
                    break;
                case BuildingType.collector:
                    //Checking if theres milk to remove
                    if (temp[i].CanAfford(1))
                        ranked_containers.Add(temp[i]);
                    break;
            }
        }

        //Ranks containers by current milk amount.
        //Tanks have greater capacity, so they are prioritised if they have milk.
        //Containers must have milk to work with Mouse_AI
        ranked_containers.OrderByDescending(c => c.CURRENT_MILK_AMOUNT);


        ranked_factories.OrderByDescending(c => c.MilkToAdd());
    }

    public MilkTank GetAvailableTank()
    {
        // returns the first tank that is not full
        return all_containers.OfType<MilkTank>().FirstOrDefault(t => !t.is_full);
    }

    public int GetTotalMilk()
    {
        // returns the total amount of milk across all containers
        return all_containers.Where(c => c != null && c.CONTAINER_GAME_OBJECT != null).Sum(c => c.CURRENT_MILK_AMOUNT);
    }

    //****************************************************************
    public IMilkContainer RequestMilkSource(int amount_needed)
    {
        // provides a milk container that can fulfill the requested amount
        foreach (var container in ranked_containers)
        {
            if (container.CURRENT_MILK_AMOUNT >= amount_needed)
            {
                return container;
            }
        }
        return null;
    }

    private void MilkConsoleCommands()
    {
        DebugWindow.Instance.RegisterExternalCommand("milk.fill", "Fills all milk containers to max capacity.", args =>
        {
            foreach (var container in all_containers)
            {
                container.CURRENT_MILK_AMOUNT = container.MAX_MILK_CAPACITY;
            }
            DebugWindow.LogToConsole("All milk containers filled to maximum.");
        });

        DebugWindow.Instance.RegisterExternalCommand("milk.empty", "Empties all milk containers to min capacity.", args =>
        {
            foreach (var container in all_containers)
            {
                container.CURRENT_MILK_AMOUNT = 0;
            }
            DebugWindow.LogToConsole("All milk containers emptied.");
        });

        DebugWindow.Instance.RegisterExternalCommand("milk.rate", "Sets production interval for all collectors. Usage: milk_rate [seconds]", args =>
        {
            if (args.Length > 0 && float.TryParse(args[0], out float newRate))
            {
                var collectors = FindObjectsOfType<MilkCollector>();
                foreach (var c in collectors)
                {
                    c.production_interval = newRate;
                }
                DebugWindow.LogToConsole($"Production interval set to {newRate}s for {collectors.Length} collectors.");
            }
        });

        DebugWindow.Instance.RegisterExternalCommand("milk.logistics", "Prints the current ranked priority list.", args =>
        {
            DebugWindow.LogToConsole("--- Priority List (Tanks first, then highest storage) ---");
            for (int i = 0; i < ranked_containers.Count; i++)
            {
                var c = ranked_containers[i];
                DebugWindow.LogToConsole($"{i + 1}: {c.CONTAINER_GAME_OBJECT.name} (Milk: {c.CURRENT_MILK_AMOUNT}/{c.MAX_MILK_CAPACITY})");
            }
        });
    }
}
