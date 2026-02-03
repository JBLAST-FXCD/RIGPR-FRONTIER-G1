using UnityEngine;
using TMPro;
using System.Collections.Generic;

//// Hani Hailston 13/12/2025

//Updated By Anthony 2/2/2026

/// <summary>
/// This script handles the resource count (scrap & cheese) as well as purchase logic.
/// </summary>

public class ResourceManager : MonoBehaviour, ISaveable
{
    private const int INITIAL_SCRAP = 100;

    public static ResourceManager instance;

    [Header("Current Resources")]
    protected static int scrap  = INITIAL_SCRAP;
    protected static int cheese = 0;
    protected static int money  = 0; // added temp as forgotten
/*
    // Tracks cheese amounts per type for morale plus future economy expansion
    private readonly Dictionary<CheeseType, int> cheese_by_type = new Dictionary<CheeseType, int>();

    // Which cheese type each factory is currently set to produce
    private readonly Dictionary<int, CheeseType> factory_to_type = new Dictionary<int, CheeseType>();

    // How many factories are currently set to each cheese type
    private readonly Dictionary<CheeseType, int> active_type_counts = new Dictionary<CheeseType, int>();

*/

    [Header("UI References")]
    public TextMeshProUGUI scrap_text;
    public TextMeshProUGUI cheese_text;

    // Checks if player can afford a specific purchase.
    public bool CanAfford(int scrap_cost, int cheese_cost)
    {
        if (scrap >= scrap_cost && cheese >= cheese_cost)
        {
            return true;
        }

        return false;
    }

    public int Scrap {  get { return scrap; } }
    public int Cheese { get { return cheese; } }

    public void SpendResources(int scrap_cost, int cheese_cost)
    {
        scrap -= scrap_cost;
        cheese -= cheese_cost;

        UpdateUI();
    }

    public void AddResources(int scrap_to_add, int cheese_to_add)
    {
        scrap += scrap_to_add;
        cheese += cheese_to_add;

        UpdateUI();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scrap_text != null)
        {
            scrap_text.text = "Scrap: " + scrap;
        }

        if (cheese_text != null)
        {
            cheese_text.text = "Cheese: " + cheese;
        }
    }

    public void PopulateSaveData(GameData data)
    {
        data.player_data.money = money;
    }

    public void LoadFromSaveData(GameData data)
    {
        money = data.player_data.money;
    }

    /* public void AddCheese(CheeseType type, int amount)
    {
        if (amount <= 0) return;

        if (!cheese_by_type.ContainsKey(type))
            cheese_by_type[type] = 0;

        cheese_by_type[type] += amount;

        // Keep existing total cheese logic/UI intact
        AddResources(0, amount);

        Debug.Log($"[ResourceManager] Added {amount}x {type}. " + $"produced_variety={GetCheeseVarietyCountProduced()} " + $"active_variety={GetActiveCheeseVarietyCount()} " + $"total_cheese={cheese}");
    }

    public int GetCheeseVarietyCountProduced()
    {
        int count = 0;

        foreach (KeyValuePair<CheeseType, int> kv in cheese_by_type)
        {
            if (kv.Value > 0)
                count++;
        }

        return count;
    }

    private void IncrementActiveType(CheeseType type)
    {
        if (!active_type_counts.ContainsKey(type))
            active_type_counts[type] = 0;

        active_type_counts[type]++;
    }

    private void DecrementActiveType(CheeseType type)
    {
        if (!active_type_counts.ContainsKey(type))
            return;

        active_type_counts[type]--;

        if (active_type_counts[type] <= 0)
            active_type_counts.Remove(type);
    }

    public void RegisterOrUpdateFactoryCheeseType(Object factory, CheeseType new_type)
    {
        if (factory == null) return;

        int id = factory.GetInstanceID();

        // If already registered, decrement old type
        if (factory_to_type.TryGetValue(id, out CheeseType old_type))
        {
            if (old_type == new_type)
                return;

            DecrementActiveType(old_type);
            factory_to_type[id] = new_type;
            IncrementActiveType(new_type);
        }
        else
        {
            factory_to_type.Add(id, new_type);
            IncrementActiveType(new_type);
        }
    }

    public void UnregisterFactory(Object factory)
    {
        if (factory == null) return;

        int id = factory.GetInstanceID();

        if (factory_to_type.TryGetValue(id, out CheeseType old_type))
        {
            factory_to_type.Remove(id);
            DecrementActiveType(old_type);
        }
    }

    public int GetActiveCheeseVarietyCount()
    {
        return active_type_counts.Count;
    } */



}
