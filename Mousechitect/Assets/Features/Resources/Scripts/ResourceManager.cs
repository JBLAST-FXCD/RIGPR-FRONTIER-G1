using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

// Updated by Iain Benner 02/02/2026
// Hani Hailston 13/12/2025

/// <summary>
/// This script handles the resource count (scrap & cheese) as well as purchase logic.
/// </summary>

public class ResourceManager : MonoBehaviour, ISaveable
{
    private const int INITIAL_SCRAP = 100;

    public static ResourceManager instance;

    [Header("Current Resources")]
    protected static int scrap = INITIAL_SCRAP;
    protected static int total_cheese = 1;
    protected static int money = 0; // added temp as forgotten
    protected static Dictionary<CheeseTypes, int> cheeses;

    [Header("UI References")]
    public TextMeshProUGUI scrap_text;
    public TextMeshProUGUI cheese_text;

    // factory instance -> current cheese type
    private readonly Dictionary<int, CheeseTypes> factory_to_type = new Dictionary<int, CheeseTypes>();

    // cheese type -> how many factories currently set to it
    private readonly Dictionary<CheeseTypes, int> active_type_counts = new Dictionary<CheeseTypes, int>();

    public int Scrap { get { return scrap; } }
    public int Total_cheese { get {return total_cheese; } }

    // Checks if player can afford a specific purchase based on cheese type.
    public bool CanAfford(int scrap_cost, CheeseTypes key, int cheese_amount)
    {
        if (scrap >= scrap_cost && cheeses[key] >= cheese_amount)
        {
            return true;
        }

        return false;
    }
    public bool CanAfford(CheeseTypes key, int cheese_amount)
    {
        if (cheeses[key] >= cheese_amount)
        {
            return true;
        }

        return false;
    }
    public bool CanAfford(int scrap_cost)
    {
        if (scrap >= scrap_cost)
        {
            return true;
        }

        return false;
    }

    public void SpendResources(int scrap_cost, CheeseTypes key, int cheese_amount)
    {
        scrap -= scrap_cost;

        cheeses[key] -= cheese_amount;
        total_cheese -= cheese_amount;

        UpdateUI();
    }
    public void SpendResources(int scrap_cost)
    {
        scrap -= scrap_cost;

        UpdateUI();
    }
    public void SpendResources(CheeseTypes key, int cheese_amount)
    {
        cheeses[key] -= cheese_amount;
        total_cheese -= cheese_amount;

        UpdateUI();
    }

    public void AddResources(int scrap_to_add, CheeseTypes key, int cheese_amount)
    {
        scrap += scrap_to_add;

        cheeses[key] += cheese_amount;
        total_cheese += cheese_amount;

        UpdateUI();
    }
    public void AddResources(int scrap_to_add)
    {
        scrap += scrap_to_add;

        UpdateUI();
    }
    public void AddResources(CheeseTypes key, int cheese_amount)
    {
        cheeses[key] += cheese_amount;
        total_cheese += cheese_amount;

        UpdateUI();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;

        if (cheeses == null)
            cheeses = new Dictionary<CheeseTypes, int>();
    }

    private void Start()
    {
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
            cheeses[c] = 0;


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
            cheese_text.text = "Total Cheese: " + total_cheese;
        }
    }

    public void PopulateSaveData(GameData data)
    {
        data.player_data.resources.scrap = scrap;
        data.player_data.resources.total_cheese = total_cheese;
        data.player_data.resources.money = money;

        int i = 0;
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
        {
            data.player_data.resources.cheese_amounts[i] = cheeses[c];
            i++;
        }
    }

    public void LoadFromSaveData(GameData data)
    {
        scrap = data.player_data.resources.scrap;
        total_cheese = data.player_data.resources.total_cheese;
        money = data.player_data.resources.money;

        int i = 0;
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes))) 
        {
            cheeses[c] = data.player_data.resources.cheese_amounts[i];
            i++;
        }
    }

    // Updates by Anthony - 05/02/2026

    // Returns how many cheese types currently have > 0 in storage.
    public int GetCheeseVarietyCount()
    {
        if (cheeses == null) return 0;

        int count = 0;
        foreach (KeyValuePair<CheeseTypes, int> kvp in cheeses)
        {
            if (kvp.Value > 0) count++;
        }
        return count;
    }

    // Increments active factory count for the given cheese type.
    private void IncActive(CheeseTypes t)
    {
        if (!active_type_counts.ContainsKey(t)) active_type_counts[t] = 0;
        active_type_counts[t]++;
    }

    // Decrements active factory count for the given cheese type (and removes the key at 0 to keep Count meaningful).
    private void DecActive(CheeseTypes t)
    {
        if (!active_type_counts.ContainsKey(t)) return;

        active_type_counts[t]--;
        if (active_type_counts[t] <= 0)
            active_type_counts.Remove(t);
    }

    // Registers a factory's current cheese type, or updates it when the factory switches type.
    // This drives ACTIVE variety used by the morale food variety adapter.
    public void RegisterOrUpdateFactoryCheeseType(UnityEngine.Object factory, CheeseTypes new_type)
    {
        if (factory == null) return;

        int id = factory.GetInstanceID();

        // If factory already registered, remove its old type from counts first.
        if (factory_to_type.TryGetValue(id, out CheeseTypes old_type))
        {
            if (old_type == new_type) return;

            DecActive(old_type);
            factory_to_type[id] = new_type;
            IncActive(new_type);
        }
        else
        {
            // First time seeing this factory instance
            factory_to_type.Add(id, new_type);
            IncActive(new_type);
        }

        Debug.Log($"[ResourceManager] ACTIVE variety={active_type_counts.Count} (factory {factory.name}={new_type})");
    }

    // Removes a factory from ACTIVE variety tracking (called when a factory is disabled/destroyed).
    public void UnregisterFactory(UnityEngine.Object factory)
    {
        if (factory == null) return;

        int id = factory.GetInstanceID();
        if (factory_to_type.TryGetValue(id, out CheeseTypes old_type))
        {
            factory_to_type.Remove(id);
            DecActive(old_type);

            Debug.Log($"[ResourceManager] ACTIVE variety={active_type_counts.Count} (factory {factory.name} removed)");
        }
    }

    // Returns number of distinct cheese types currently selected across all factories.
    public int GetActiveCheeseVarietyCount()
    {
        return active_type_counts.Count;
    }
}