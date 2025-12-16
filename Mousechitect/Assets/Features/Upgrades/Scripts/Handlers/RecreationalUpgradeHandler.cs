using UnityEngine;
using System.Collections;

//// Hani Hailston 15/12/2025

/// <summary>
/// Manages all upgrades for recreational buildings specifically.
/// Handles morale bonuses, events & deductions as well as passive cheese generation.
/// </summary>

public class RecreationUpgradeHandler : BuildingUpgradeHandler
{
    private const string ID_FUN_TIMES = "REC_2_1";
    private const string ID_VEGAS_MODE = "REC_2_2";
    private const string ID_BRIGHT_SIDE = "REC_2_3";
    private const string ID_GRAND_OPENING = "REC_3";
    private const string ID_DAY_TO_REMEMBER = "REC_4_1";
    private const string ID_TAX_WRITE_OFF = "REC_4_2";
    private const string ID_BRIGHTER_SIDE = "REC_4_3";
    private const string ID_HALF_PRICE = "REC_5";

    private const float SECONDS_IN_MINUTE = 60.0f;
    private const float DEFAULT_FUN_BONUS = 0.01f;
    private const float DEFAULT_DAY_BONUS = 0.03f;
    private const float DEFAULT_BRIGHT_DECAY = 0.10f;
    private const float DEFAULT_BRIGHTER_DECAY = 0.15f;
    private const int DEFAULT_SMALL_GROUP_CHEESE = 5;
    private const float DEFAULT_VEGAS_INTERVAL = 15.0f;
    private const float DEFAULT_TAX_INTERVAL = 10.0f;
    private const float DEFAULT_GRAND_OPENING_DURATION = 3.0f;

    [Header("Base Stats (Editable In-Script)")]
    public float morale_increase_multiplier = 1.0f;
    public float morale_decay_multiplier = 1.0f;

    public bool is_half_price_construction = false;
    public bool is_grand_opening_unlocked = false;
    public bool is_morale_frozen = false;

    [Header("Configurables - Morale Multipliers")]
    [SerializeField]
    private float fun_times_bonus = DEFAULT_FUN_BONUS, day_to_remember_bonus = DEFAULT_DAY_BONUS, bright_side_decay_reduction = DEFAULT_BRIGHT_DECAY, brighter_side_decay_reduction = DEFAULT_BRIGHTER_DECAY;

    [Header("Configurables - Passive Cheese")]
    [SerializeField]
    private int small_group_cheese = DEFAULT_SMALL_GROUP_CHEESE;

    [SerializeField]
    private float vegas_interval_minutes = DEFAULT_VEGAS_INTERVAL, tax_write_off_interval_minutes = DEFAULT_TAX_INTERVAL;

    [Header("Configuration - Special Ability")]
    [SerializeField]
    private float grand_opening_duration_minutes = DEFAULT_GRAND_OPENING_DURATION;

    private Coroutine passive_cheese_routine;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgrade_id)
        {
            case ID_FUN_TIMES:
                morale_increase_multiplier += fun_times_bonus;
                break;

            case ID_VEGAS_MODE:
                if (passive_cheese_routine != null)
                {
                    StopCoroutine(passive_cheese_routine);
                }
                passive_cheese_routine = StartCoroutine(GroupBonusCheeseRoutine(small_group_cheese, vegas_interval_minutes));
                break;

            case ID_BRIGHT_SIDE:
                morale_decay_multiplier -= bright_side_decay_reduction;
                break;

            case ID_GRAND_OPENING:
                is_grand_opening_unlocked = true;
                break;

            case ID_DAY_TO_REMEMBER:
                morale_increase_multiplier += day_to_remember_bonus;
                break;

            case ID_TAX_WRITE_OFF:
                if (passive_cheese_routine != null)
                {
                    StopCoroutine(passive_cheese_routine);
                }
                passive_cheese_routine = StartCoroutine(GroupBonusCheeseRoutine(small_group_cheese, tax_write_off_interval_minutes));
                break;

            case ID_BRIGHTER_SIDE:
                morale_decay_multiplier -= brighter_side_decay_reduction;
                break;

            case ID_HALF_PRICE:
                is_half_price_construction = true;
                break;
        }
    }

    // This function will need to be called from the building system/placement script whenever a new Recreation-Type building is built.
    public void TriggerGrandOpening()
    {
        if (is_grand_opening_unlocked)
        {
            if (!is_morale_frozen)
            {
                StartCoroutine(GrandOpeningRoutine());
            }
        }
    }

    // Grid manager script will need to be linked below so the bonus cheese reward scales with the number of recreational buildings in a given area.
    private IEnumerator GroupBonusCheeseRoutine(int amount, float interval_minutes)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval_minutes * SECONDS_IN_MINUTE);

            if (ResourceManager.instance != null)
            {
                ResourceManager.instance.AddResources(0, amount);
            }
        }
    }

    private IEnumerator GrandOpeningRoutine()
    {
        is_morale_frozen = true;

        yield return new WaitForSeconds(grand_opening_duration_minutes * SECONDS_IN_MINUTE);

        is_morale_frozen = false;
    }
}
