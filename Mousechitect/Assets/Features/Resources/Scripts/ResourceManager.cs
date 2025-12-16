using UnityEngine;
using TMPro;

//// Hani Hailston 13/12/2025

/// <summary>
/// This script handles the resource count (scrap & cheese) as well as purchase logic.
/// </summary>

public class ResourceManager : MonoBehaviour, ISaveable
{
    private const int INITIAL_SCRAP = 100;

    public static ResourceManager instance;

    [Header("Current Resources")]
    public int current_scrap = INITIAL_SCRAP;
    public int current_cheese = 0;

    [Header("UI References")]
    public TextMeshProUGUI scrap_text;
    public TextMeshProUGUI cheese_text;

    // Checks if player can afford a specific purchase.
    public bool CanAfford(int scrap_cost, int cheese_cost)
    {
        if (current_scrap >= scrap_cost && current_cheese >= cheese_cost)
        {
            return true;
        }

        return false;
    }

    public void SpendResources(int scrap_cost, int cheese_cost)
    {
        current_scrap -= scrap_cost;
        current_cheese -= cheese_cost;

        UpdateUI();
    }

    public void AddResources(int scrap_to_add, int cheese_to_add)
    {
        current_scrap += scrap_to_add;
        current_cheese += cheese_to_add;

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
            scrap_text.text = "Scrap: " + current_scrap;
        }

        if (cheese_text != null)
        {
            cheese_text.text = "Cheese: " + current_cheese;
        }
    }

    public void PopulateSaveData(GameData data)
    {
        data.player_data.money = this.currentMoney;
    }

    public void LoadFromSaveData(GameData data)
    {
        this.currentMoney = data.player_data.money;
    }
}
