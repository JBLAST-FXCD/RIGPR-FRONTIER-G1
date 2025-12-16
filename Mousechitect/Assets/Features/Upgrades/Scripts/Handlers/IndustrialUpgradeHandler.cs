using UnityEngine;

//// Hani Hailston 14/12/2025

/// <summary>
/// Manages all upgrades for industrial buildings specifically.
/// Handles milk & cheese production speed and general output multipliers.
/// Specific additions of buildings TBA to the script
/// </summary>

public class IndustrialUpgradeHandler : BuildingUpgradeHandler
{
    private const string ID_MILK_SPEED_SMALL = "IND_2_1";
    private const string ID_CHEESE_SPEED_SMALL = "IND_2_2";
    private const string ID_CHEESE_OUTPUT_BOOST = "IND_2_3";
    private const string ID_UNLOCK_SOFT_CHEESE = "IND_3";
    private const string ID_MILK_SPEED_MEDIUM = "IND_4_1";
    private const string ID_MILK_OUTPUT_BOOST = "IND_4_2";
    private const string ID_CHEESE_SPEED_MEDIUM = "IND_4_3";
    private const string ID_MILK_SPEED_HUGE = "IND_5";
    private const string ID_UNLOCK_MILK_TANK = "IND_6_1";
    private const string ID_UNLOCK_SPECIALTY = "IND_6_2";

    private const float DEFAULT_SPEED_SMALL = 0.02f;
    private const float DEFAULT_SPEED_MEDIUM = 0.05f;
    private const float DEFAULT_SPEED_HUGE = 0.25f;
    private const float DEFAULT_OUTPUT_BOOST = 0.05f;

    [Header("Base Stats (Editable In-Script)")]
    public float milk_speed_mult = 1.0f;
    public float cheese_speed_mult = 1.0f;
    public float milk_output_mult = 1.0f;
    public float cheese_output_mult = 1.0f;

    [Header("Configurables: IND Building Bonuses")]
    [SerializeField]
    private float small_speed_boost = DEFAULT_SPEED_SMALL, medium_speed_boost = DEFAULT_SPEED_MEDIUM, huge_speed_boost = DEFAULT_SPEED_HUGE;

    [SerializeField]
    private float output_boost = DEFAULT_OUTPUT_BOOST;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgrade_id)
        {
            case ID_MILK_SPEED_SMALL:
                milk_speed_mult += small_speed_boost;
                break;

            case ID_CHEESE_SPEED_SMALL:
                cheese_speed_mult += small_speed_boost;
                break;

            case ID_CHEESE_OUTPUT_BOOST:
                cheese_output_mult += output_boost;
                break;

            case ID_UNLOCK_SOFT_CHEESE:
                if (BuildingUnlockManager.instance != null)
                {
                    BuildingUnlockManager.instance.UnlockBuilding("SoftCheeseFactory");
                }
                break;

            case ID_MILK_SPEED_MEDIUM:
                milk_speed_mult += medium_speed_boost;
                break;

            case ID_MILK_OUTPUT_BOOST:
                milk_output_mult += output_boost;
                break;

            case ID_CHEESE_SPEED_MEDIUM:
                cheese_speed_mult += medium_speed_boost;
                break;

            case ID_MILK_SPEED_HUGE:
                milk_speed_mult += huge_speed_boost;
                break;

            case ID_UNLOCK_MILK_TANK:
                if (BuildingUnlockManager.instance != null)
                {
                    BuildingUnlockManager.instance.UnlockBuilding("MilkTank");
                }
                break;

            case ID_UNLOCK_SPECIALTY:
                if (BuildingUnlockManager.instance != null)
                {
                    BuildingUnlockManager.instance.UnlockBuilding("SpecialtyCheeseFactory");
                }
                break;
        }
    }
}