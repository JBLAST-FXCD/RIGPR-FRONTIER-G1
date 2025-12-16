using UnityEngine;
using System.Collections;

//// Hani Hailston 14/12/2025

/// <summary>
/// Manages all upgrades for commercial buildings specifically.
/// Handles profits from sales, offer events, and cheese bonuses.
/// </summary>

public class CommercialUpgradeHandler : BuildingUpgradeHandler
{
    private const float SECONDS_IN_MINUTE = 60.0f;

    private const string ID_PROFIT_SMALL_1 = "COM_2_1";
    private const string ID_FREQ_SMALL_1 = "COM_2_2";
    private const string ID_REFUND_CHANCE = "COM_2_3";
    private const string ID_LIMITED_OFFER = "COM_3";
    private const string ID_PROFIT_SMALL_2 = "COM_4_1";
    private const string ID_FREQ_SMALL_2 = "COM_4_2";
    private const string ID_DECENT_REWARD = "COM_4_3";
    private const string ID_HALF_PRICE = "COM_5";
    private const string ID_UNLOCK_DOCK = "COM_6";

    private const float DEFAULT_PROFIT_BONUS = 0.10f;
    private const float DEFAULT_FREQ_BONUS = 0.10f;
    private const float DEFAULT_REFUND_CHANCE = 0.10f;
    private const int DEFAULT_SMALL_REWARD = 5;
    private const int DEFAULT_DECENT_REWARD = 15;
    private const float DEFAULT_EVENT_COOLDOWN = 15.0f;
    private const float DEFAULT_EVENT_DURATION = 2.0f;
    private const float DEFAULT_EVENT_MULT = 2.0f;

    [Header("Base Stats (Editable In-Script)")]
    public float profit_multiplier = 1.0f;
    public float sale_frequency_multiplier = 1.0f;
    public float chance_for_random_cheese = 0.0f;

    public int random_cheese_amount = 0;

    public bool is_half_price_construction = false;

    [Header("Configurables - Stats")]
    [SerializeField]
    private float profit_bonus_small = DEFAULT_PROFIT_BONUS, freq_bonus_small = DEFAULT_FREQ_BONUS;

    [Header("Configurables - Cheese Bonuses")]
    [SerializeField]
    private float refund_chance = DEFAULT_REFUND_CHANCE;

    [SerializeField]
    private int small_cheese_reward = DEFAULT_SMALL_REWARD, decent_cheese_reward = DEFAULT_DECENT_REWARD;

    [Header("Configurables - Limited-Time Offer Event")]
    [SerializeField]
    private float event_cooldown_mins = DEFAULT_EVENT_COOLDOWN, event_duration_mins = DEFAULT_EVENT_DURATION, event_profit_mult = DEFAULT_EVENT_MULT;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgrade_id)
        {
            case ID_PROFIT_SMALL_1:
                profit_multiplier += profit_bonus_small;
                break;

            case ID_FREQ_SMALL_1:
                sale_frequency_multiplier += freq_bonus_small;
                break;

            case ID_REFUND_CHANCE:
                chance_for_random_cheese = refund_chance;
                random_cheese_amount = small_cheese_reward;
                break;

            case ID_LIMITED_OFFER:
                StartCoroutine(LimitedTimeOfferRoutine());
                break;

            case ID_PROFIT_SMALL_2:
                profit_multiplier += profit_bonus_small;
                break;

            case ID_FREQ_SMALL_2:
                sale_frequency_multiplier += freq_bonus_small;
                break;

            case ID_DECENT_REWARD:
                random_cheese_amount = decent_cheese_reward;
                break;

            case ID_HALF_PRICE:
                is_half_price_construction = true;
                break;

            case ID_UNLOCK_DOCK:
                if (BuildingUnlockManager.instance != null)
                {
                    BuildingUnlockManager.instance.UnlockBuilding("DockYard");
                }
                break;
        }
    }

    private IEnumerator LimitedTimeOfferRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(event_cooldown_mins * SECONDS_IN_MINUTE);

            Debug.Log("Limited time offer has begun!");

            float old_mult = profit_multiplier;
            profit_multiplier *= event_profit_mult;

            yield return new WaitForSeconds(event_duration_mins * SECONDS_IN_MINUTE);

            profit_multiplier = old_mult;

            Debug.Log("Limited time offer has ended.");
        }
    }
}