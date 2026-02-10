using UnityEngine;
using System.Collections;

//// Hani - Updated 09/02/2026

/// <summary>
/// Manages all upgrades for residential buildings.
/// Handles morale boosting, population caps, and passive cheese generation.
/// </summary>

public class ResidentialUpgradeHandler : BuildingUpgradeHandler
{
    public static ResidentialUpgradeHandler instance;

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

    private const string ID_MORALE_SMALL = "RES_2_1";
    private const string ID_POP_SMALL = "RES_2_2";
    private const string ID_PASSIVE_TELLY = "RES_2_3";
    private const string ID_UNLOCK_MANOR = "RES_3";
    private const string ID_MORALE_LARGE = "RES_4_1";
    private const string ID_POP_LARGE = "RES_4_2";
    private const string ID_PASSIVE_BIG = "RES_4_3";
    private const string ID_RAPID_CONSTRUCTION = "RES_5";
    private const string ID_UNLOCK_FLATS = "RES_6";
    private const float SECONDS_IN_MINUTE = 60.0f;
    private const float DEFAULT_MORALE_SMALL = 0.10f;
    private const float DEFAULT_MORALE_LARGE = 0.15f;
    private const int DEFAULT_NEIGHBOR_SMALL = 1;
    private const int DEFAULT_NEIGHBOR_LARGE = 3;
    private const int DEFAULT_TELLY_AMOUNT = 5;
    private const float DEFAULT_TELLY_INTERVAL = 15.0f;
    private const int DEFAULT_BIG_CHEESE_AMOUNT = 10;
    private const float DEFAULT_BIG_CHEESE_INTERVAL = 10.0f;

    [Header("Base Stats (Only Editable In-Script)")]
    public float current_morale_multiplier = 1.0f;
    public int global_population_cap_bonus = 0;
    public bool is_rapid_construction_active = false;

    [Header("Configurables - RES Building Bonuses")]
    [SerializeField] private float morale_bonus_small = DEFAULT_MORALE_SMALL;
    [SerializeField] private float morale_bonus_large = DEFAULT_MORALE_LARGE;

    [SerializeField] private int neighbor_pop_bonus_small = DEFAULT_NEIGHBOR_SMALL;
    [SerializeField] private int neighbor_pop_bonus_large = DEFAULT_NEIGHBOR_LARGE;

    [Header("Configurables - RES Passive Bonuses")]
    [SerializeField] private int telly_cheese_amount = DEFAULT_TELLY_AMOUNT;
    [SerializeField] private float telly_interval_minutes = DEFAULT_TELLY_INTERVAL;
    [SerializeField] private int big_cheese_amount = DEFAULT_BIG_CHEESE_AMOUNT;
    [SerializeField] private float big_cheese_interval_minutes = DEFAULT_BIG_CHEESE_INTERVAL;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgrade_id)
        {
            case ID_MORALE_SMALL:
                current_morale_multiplier += morale_bonus_small;
                break;

            case ID_POP_SMALL:
                global_population_cap_bonus += neighbor_pop_bonus_small;
                break;

            case ID_PASSIVE_TELLY:
                StartCoroutine(PassiveCheeseRoutine(telly_cheese_amount, telly_interval_minutes));
                break;

            case ID_UNLOCK_MANOR:
                if (BuildingUnlockManager.instance != null)
                {
                    BuildingUnlockManager.instance.UnlockBuilding("MilkCartonManor");
                }
                break;

            case ID_MORALE_LARGE:
                current_morale_multiplier += morale_bonus_large;
                break;

            case ID_POP_LARGE:
                global_population_cap_bonus += neighbor_pop_bonus_large;
                break;

            case ID_PASSIVE_BIG:
                StartCoroutine(PassiveCheeseRoutine(big_cheese_amount, big_cheese_interval_minutes));
                break;

            case ID_RAPID_CONSTRUCTION:
                is_rapid_construction_active = true;
                break;

            case ID_UNLOCK_FLATS:
                if (BuildingUnlockManager.instance != null)
                {
                    BuildingUnlockManager.instance.UnlockBuilding("LunchBoxFlats");
                }
                break;
        }
    }

    private IEnumerator PassiveCheeseRoutine(int amount, float interval_minutes)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval_minutes * SECONDS_IN_MINUTE);

            if (ResourceManager.instance != null)
            {
                ResourceManager.instance.AddResources(0, amount);
                Debug.Log($"Gained {amount} Cheese passively!");
            }
        }
    }
}
