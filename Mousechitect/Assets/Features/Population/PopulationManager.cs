using UnityEngine;
using System.Collections;

//// Hani - 09/02/2026

/// <summary>
/// Manages the population logic as stated in the GDD.
/// </summary>

public class PopulationManager : MonoBehaviour, ISaveable
{
    public static PopulationManager instance;

    [Header("Official Population")]
    public int current_population = 0;
    public int total_housing_capacity = 0;

    [Header("Visual Population")]
    public int current_visual_capacity = 0;

    [Header("Growth Rate Settings")]
    public float base_arrival_interval = 10.0f;
    private float arrival_timer = 0f;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void Update()
    {
        HandlePopulationGrowth();
    }

    private void HandlePopulationGrowth()
    {
        float morale = 1.0f;

        if (ResidentialUpgradeHandler.instance != null)
        {
            morale = ResidentialUpgradeHandler.instance.current_morale_multiplier;
        }

        if (morale <= 0)
        {
            arrival_timer += Time.deltaTime;
            if (arrival_timer >= 2.0f)
            {
                if (current_population > 0)
                {
                    current_population--;
                    Debug.Log("Morale is 0! A mouse has left the city.");
                }
                arrival_timer = 0;
            }
            return;
        }

        if (current_population < total_housing_capacity)
        {
            float growth_speed = (Time.deltaTime / base_arrival_interval) * morale;
            arrival_timer += growth_speed;

            if (arrival_timer >= 1.0f)
            {
                current_population++;
                arrival_timer = 0;
                Debug.Log($"A new mouse moved in! Pop: {current_population}/{total_housing_capacity}");
            }
        }
    }

    public void RegisterHousing(int official_cap, int visual_cap)
    {
        total_housing_capacity += official_cap;
        current_visual_capacity += visual_cap;

        if (ResidentialUpgradeHandler.instance != null)
        {
            total_housing_capacity += ResidentialUpgradeHandler.instance.global_population_cap_bonus;
        }
    }

    public void UnregisterHousing(int official_cap, int visual_cap)
    {
        total_housing_capacity -= official_cap;
        current_visual_capacity -= visual_cap;
    }

    public void PopulateSaveData(GameData data)
    {
        data.player_data.population = this.current_population;
    }

    public void LoadFromSaveData(GameData data)
    {
        this.current_population = data.player_data.population;
    }
}
