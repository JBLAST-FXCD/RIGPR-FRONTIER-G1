using UnityEngine;
using TMPro;
using UImGui;

//// Hani Hailston 13/12/2025

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
}
