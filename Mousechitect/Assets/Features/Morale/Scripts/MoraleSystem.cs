using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 21/01/26

/// <summary>
/// Central morale system.
/// - Tracks per-mouse morale for objects tagged "MouseTemp"
/// - Computes global morale as the average of individual morale
/// - Calculates a morale state (negative / neutral / positive)
/// - Exposes modifiers for future systems (productivity, retention, arrival)
/// 
/// Current inputs:
/// - Housing quality: reads ResidentialBuilding quality
/// - Recreation, food variety, aesthetics aren't there until systems are implemented
/// </summary>
public class MoraleSystem : MonoBehaviour
{
    public enum MORALE_STATE
    {
        STATE_NEGATIVE,
        STATE_NEUTRAL,
        STATE_POSITIVE
    }

    [SerializeField] private BuildingManager building_manager;

    private const string MOUSE_TAG = "MouseTemp";

    private const float UPDATE_INTERVAL_SECONDS = 1.0f;

    // Morale ranges are kept UI-friendly: 0-1
    private const float MIN_MORALE = 0.0f;
    private const float MAX_MORALE = 1.0f;

    // Thresholds are applied to morale_score (-1 - 1) 
    private const float POSITIVE_THRESHOLD = 0.3f;
    private const float NEGATIVE_THRESHOLD = -0.3f;

    // How fast individual mice drift toward the "target" morale
    private const float MOUSE_MORALE_ADJUST_SPEED = 0.10f;

    // Factor weights (sum to 1.0f)
    private const float HOUSING_WEIGHT = 0.40f;
    private const float RECREATION_WEIGHT = 0.20f;
    private const float FOOD_WEIGHT = 0.20f;
    private const float AESTHETICS_WEIGHT = 0.20f;

    // Modifiers applied when morale state changes
    private const float POSITIVE_PRODUCTIVITY_BONUS = 0.15f;
    private const float NEGATIVE_PRODUCTIVITY_PENALTY = 0.15f;

    private const float POSITIVE_RETENTION_BONUS = 0.10f;
    private const float NEGATIVE_RETENTION_PENALTY = 0.15f;

    private const float POSITIVE_ARRIVAL_BONUS = 0.10f;
    private const float NEGATIVE_ARRIVAL_PENALTY = 0.15f;

    private float global_morale = 0.5f;
    private float morale_score = 0.0f;

    private float productivity_modifier = 1.0f;
    private float retention_modifier = 1.0f;
    private float arrival_modifier = 1.0f;

    private MORALE_STATE morale_state = MORALE_STATE.STATE_NEUTRAL;

    private readonly Dictionary<int, float> mouse_morale = new Dictionary<int, float>();

    private void Start()
    {
        if (building_manager == null)
        {
            building_manager = FindObjectOfType<BuildingManager>();
        }

        if (building_manager != null)
        {
            building_manager.building_placed += OnBuildingPlaced;
            building_manager.building_removed += OnBuildingRemoved;
        }

        StartCoroutine(MoraleUpdateLoop());
    }

    private void OnBuildingPlaced(GameObject placed_object)
    {
        if (placed_object == null)
        {
            return;
        }

        // For now, morale will update next tick anyway.
        // Later: update cached housing/food/recreation/aesthetics here.
    }

    private void OnBuildingRemoved(string unique_id)
    {
        if (string.IsNullOrEmpty(unique_id))
        {
            return;
        }

        // Later: clear cached data by id if you store it.
    }

    private IEnumerator MoraleUpdateLoop()
    {
        while (true)
        {
            UpdateMorale();

            yield return new WaitForSeconds(UPDATE_INTERVAL_SECONDS);
        }
    }

    // Main morale tick that matches the flowchart
    private void UpdateMorale()
    {
        CacheMice();

        float housing_score = CollectHousingQualityScore();
        float recreation_score = CollectRecreationScore();
        float food_score = CollectFoodVarietyScore();
        float aesthetics_score = CollectAestheticsScore();

        float target_global_morale =
            (housing_score * HOUSING_WEIGHT) +
            (recreation_score * RECREATION_WEIGHT) +
            (food_score * FOOD_WEIGHT) +
            (aesthetics_score * AESTHETICS_WEIGHT);

        target_global_morale = Mathf.Clamp01(target_global_morale);

        UpdateMouseMorale(target_global_morale);
        global_morale = CalculateGlobalMoraleFromMice();

        // Convert 0 - 1 into -1 - 1 so thresholds match the GDD flowchart logic
        morale_score = (global_morale * 2.0f) - 1.0f;

        DetermineMoraleState();
        ApplyMoraleModifiers();
    }

    // Registers all mice currently in the scene.
    // Uses instance ID to avoid duplicate keys.
    private void CacheMice()
    {
        GameObject[] mice = GameObject.FindGameObjectsWithTag(MOUSE_TAG);

        int i = 0;

        while (i < mice.Length)
        {
            int id = mice[i].GetInstanceID();

            if (!mouse_morale.ContainsKey(id))
            {
                mouse_morale[id] = global_morale;
            }

            ++i;
        }
    }

    // Housing score: average residential building quality normalised to 0 - 1.
    private float CollectHousingQualityScore()
    {
        ResidentialBuilding[] buildings = FindObjectsOfType<ResidentialBuilding>();

        if (buildings.Length <= 0)
        {
            return 0.0f;
        }

        float total_quality = 0.0f;
        float max_possible = 0.0f;

        // NOTE: ResidentialBuilding currently sets quality from max_quality[tier - 1]
        // with values { 5, 10, 20 }. We treat 20 as the per-building maximum for now.
        // If later you expose max quality per building, replace this constant.
        const float MAX_QUALITY_PER_HOUSE = 20.0f;

        int i = 0;

        while (i < buildings.Length)
        {
            total_quality += buildings[i].GetQuality();
            max_possible += MAX_QUALITY_PER_HOUSE;

            ++i;
        }

        float score = total_quality / max_possible;

        return Mathf.Clamp01(score);
    }

    // Recreation score until recreation buildings exist.
    private float CollectRecreationScore()
    {
        // Early game baseline so morale isn't impossible before recreation exists
        return 0.2f;
    }

    // Food variety until cheese types + mouse preferences exist.
    private float CollectFoodVarietyScore()
    {
        return 0.5f;
    }

    // Aesthetics until decor + synergy tags are implemented.
    private float CollectAestheticsScore()
    {
        return 0.0f;
    }

    // Moves each mouse morale toward the current target morale.
    private void UpdateMouseMorale(float target_morale)
    {
        List<int> keys = new List<int>(mouse_morale.Keys);

        int i = 0;

        while (i < keys.Count)
        {
            int id = keys[i];

            float current = mouse_morale[id];
            float next = Mathf.MoveTowards(current, target_morale, MOUSE_MORALE_ADJUST_SPEED * UPDATE_INTERVAL_SECONDS);

            mouse_morale[id] = Mathf.Clamp(next, MIN_MORALE, MAX_MORALE);

            ++i;
        }
    }

    private float CalculateGlobalMoraleFromMice()
    {
        if (mouse_morale.Count <= 0)
        {
            return global_morale;
        }

        float sum = 0.0f;

        foreach (KeyValuePair<int, float> kvp in mouse_morale)
        {
            sum += kvp.Value;
        }

        float average = sum / mouse_morale.Count;

        return Mathf.Clamp01(average);
    }

    private void DetermineMoraleState()
    {
        if (morale_score > POSITIVE_THRESHOLD)
        {
            morale_state = MORALE_STATE.STATE_POSITIVE;
        }
        else if (morale_score < NEGATIVE_THRESHOLD)
        {
            morale_state = MORALE_STATE.STATE_NEGATIVE;
        }
        else
        {
            morale_state = MORALE_STATE.STATE_NEUTRAL;
        }
    }

    private void ApplyMoraleModifiers()
    {
        productivity_modifier = 1.0f;
        retention_modifier = 1.0f;
        arrival_modifier = 1.0f;

        if (morale_state == MORALE_STATE.STATE_POSITIVE)
        {
            productivity_modifier += POSITIVE_PRODUCTIVITY_BONUS;
            retention_modifier += POSITIVE_RETENTION_BONUS;
            arrival_modifier += POSITIVE_ARRIVAL_BONUS;
        }
        else if (morale_state == MORALE_STATE.STATE_NEGATIVE)
        {
            productivity_modifier -= NEGATIVE_PRODUCTIVITY_PENALTY;
            retention_modifier -= NEGATIVE_RETENTION_PENALTY;
            arrival_modifier -= NEGATIVE_ARRIVAL_PENALTY;
        }

        productivity_modifier = Mathf.Max(0.0f, productivity_modifier);
        retention_modifier = Mathf.Max(0.0f, retention_modifier);
        arrival_modifier = Mathf.Max(0.0f, arrival_modifier);
    }

    // GETTERS

    public float GetGlobalMorale()
    {
        return global_morale;
    }

    public float GetMoraleScore()
    {
        return morale_score;
    }

    public MORALE_STATE GetMoraleState()
    {
        return morale_state;
    }

    public float GetProductivityModifier()
    {
        return productivity_modifier;
    }

    public float GetRetentionModifier()
    {
        return retention_modifier;
    }

    public float GetArrivalModifier()
    {
        return arrival_modifier;
    }
}

