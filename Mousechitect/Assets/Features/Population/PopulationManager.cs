using UnityEngine;
using System.Collections;
using ImGuiNET;
using UImGui;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

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

    private float morale;
    private float arrival_chance_modifier;
    private float retention_chance_modifier;

    [SerializeField] private GameObject mouse_prefab;
    [SerializeField] private Transform[] mouse_spawn_points;

    private readonly List<GameObject> spawned_mice = new List<GameObject>();



    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void Update()
    {
        if (MoraleSystem.Instance != null)
        {
            morale = MoraleSystem.Instance.GetGlobalMorale();
            arrival_chance_modifier = MoraleSystem.Instance.GetArrivalChanceModifier();
            retention_chance_modifier = MoraleSystem.Instance.GetRetentionModifier();

            HandlePopulationGrowth();
        }
    }

    private void HandlePopulationGrowth()
    {
        /*
        if (ResidentialUpgradeHandler.instance != null)
        {
            morale = ResidentialUpgradeHandler.instance.current_morale_multiplier * morale;
        }
        */

        if (morale <= 0)
        {
            arrival_timer += Time.deltaTime;
            if (arrival_timer >= 2.0f * retention_chance_modifier)
            {
                if (current_population > 0)
                {
                    current_population--;
                    DebugWindow.LogToConsole($"Mice leaving due to low morale.", true);
                    SyncVisualMiceToPopulation();
                }
                arrival_timer = 0;
            }
            return;
        }

        if (current_population < total_housing_capacity)
        {
            float growth_speed = ((Time.deltaTime / base_arrival_interval) * morale) * arrival_chance_modifier;
            arrival_timer += growth_speed;

            //DebugWindow.LogToConsole($"Morale: {morale:F2}, Arrival Chance Modifier: {arrival_chance_modifier:F2}, Retention Chance Modifier: {retention_chance_modifier:F2}, Growth Speed: {growth_speed:F4}, Arrival Timer: {arrival_timer:F4}", true);

            if (arrival_timer >= 1.0f)
            {
                current_population++;
                arrival_timer = 0;
                DebugWindow.LogToConsole($"A new mouse has arrived! Current population: {current_population}/{total_housing_capacity}", true);
                SyncVisualMiceToPopulation();
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

        SyncVisualMiceToPopulation();
    }

    public void UnregisterHousing(int official_cap, int visual_cap)
    {
        total_housing_capacity -= official_cap;
        current_visual_capacity -= visual_cap;
        SyncVisualMiceToPopulation();
    }

    public void PopulateSaveData(GameData data)
    {
        data.player_data.population = this.current_population;

        data.player_data.spawned_mice_data = new List<mouse_save_data>();

        foreach (GameObject mouse_obj in spawned_mice)
        {
            if (mouse_obj == null) continue;

            MouseTemp mt = mouse_obj.GetComponent<MouseTemp>();
            if (mt != null)
            {
                mouse_save_data m_data = new mouse_save_data();
                m_data.mouse_id = mt.Mouse_id;
                m_data.mouse_position = mouse_obj.transform.position;
                m_data.mouse_morale = mt.MouseMorale;
                m_data.favourite_cheese = mt.FavouriteCheese;
                m_data.least_favourite_cheese = mt.LeastFavouriteCheese;

                data.player_data.spawned_mice_data.Add(m_data);
            }
        }
    }

    public void LoadFromSaveData(GameData data)
    {
        this.current_population = data.player_data.population;

        foreach (GameObject m in spawned_mice)
        {
            if (m != null) Destroy(m);
        }
        spawned_mice.Clear();

        if (data.player_data.spawned_mice_data != null)
        {
            foreach (mouse_save_data m_data in data.player_data.spawned_mice_data)
            {
                GameObject mouse_obj = Instantiate(mouse_prefab, m_data.mouse_position, Quaternion.identity);
                MouseTemp mt = mouse_obj.GetComponent<MouseTemp>();

                if (mt != null) 
                {
                    mt.LoadData(m_data.mouse_id, m_data.mouse_morale, m_data.favourite_cheese, m_data.least_favourite_cheese);
                }

                spawned_mice.Add(mouse_obj);
            }
        }
        SyncVisualMiceToPopulation();
    }

    //Updated by Anthony - 10/2/2026
    // Determines how many mouse GameObjects should be spawned visually.
    private int GetTargetVisualPopulation()
    {
        return Mathf.Min(current_population, current_visual_capacity);
    }

    //Spawns or despawns mice as needed whenever population or visual capacity changes.
    private void SyncVisualMiceToPopulation()
    {
        int target = GetTargetVisualPopulation();

        if (mouse_prefab == null)
        {
            Debug.LogWarning("[Population] mouse_prefab not set, cannot spawn visual mice.");
            return;
        }

        // Spawn mice until the target visual population
        while (spawned_mice.Count < target)
            SpawnOneMouse();

        // Despawn mice if exceed the target visual population
        while (spawned_mice.Count > target)
            DespawnOneMouse();
    }

    // Spawns a single mouse GameObject at a valid spawn location
    private void SpawnOneMouse()
    {
        Vector3 pos = transform.position;

        // Prefer defined spawn points
        if (mouse_spawn_points != null && mouse_spawn_points.Length > 0)
            pos = mouse_spawn_points[Random.Range(0, mouse_spawn_points.Length)].position;

        GameObject mouse = Instantiate(mouse_prefab, pos, Quaternion.identity);
        spawned_mice.Add(mouse);

        MouseTemp mt = mouse.GetComponent<MouseTemp>();
        if (mt != null) mt.InitialisePreferencesIfNeeded();
    }

    // updated by Iain Benner 23/02/2026
    // updated by Jess 27/02/2026
    // Removes the most recently spawned mouse GameObject
    private void DespawnOneMouse()
    {
        spawned_mice.RemoveAll(m => m == null);

        int last = spawned_mice.Count - 1;
        if (last < 0) return;

        GameObject mouse = spawned_mice[last];
        if (mouse != null)
        {
            MouseTemp character = mouse.transform.GetComponentInChildren<MouseTemp>();
            spawned_mice.RemoveAt(last);
            Destroy(mouse);
        }
    }

    public MouseTemp GetMouseById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (GameObject mouse_obj in spawned_mice)
        {
            if (mouse_obj == null) continue;
            MouseTemp mt = mouse_obj.GetComponent<MouseTemp>();
            if (mt != null && mt.Mouse_id == id)
            {
                return mt;
            }
        }
        return null;
    }
}
