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
    protected static int scrap  = INITIAL_SCRAP;
    protected static int total_cheese = 0;
    protected static int money  = 0; // added temp as forgotten
    protected static Dictionary<CheeseTypes, int> cheeses;

    [Header("UI References")]
    public TextMeshProUGUI scrap_text;
    public TextMeshProUGUI cheese_text;

    public ResourceManager()
    {
        cheeses = new Dictionary<CheeseTypes, int>();
    }

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
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
            cheeses.Add(c, 0);

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
}
