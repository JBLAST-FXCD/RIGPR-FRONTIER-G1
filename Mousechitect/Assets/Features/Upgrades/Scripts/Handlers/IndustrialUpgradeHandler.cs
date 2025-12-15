using UnityEngine;

/// <summary>
/// Manages all upgrades for industrial buildings specifically.
/// Handles milk & cheese production speed and general output multipliers.
/// Specific additions of buildings TBA to the script
/// </summary>
public class IndustrialUpgradeHandler : BuildingUpgradeHandler
{
    [Header("Base Stats (Editable In-Script)")]
    public float milkSpeedMult = 1.0f;
    public float cheeseSpeedMult = 1.0f;
    public float milkOutputMult = 1.0f;
    public float cheeseOutputMult = 1.0f;

    [Header("Configurables: IND Building Bonuses")]
    [SerializeField] private float smallSpeedBoost = 0.02f;
    [SerializeField] private float mediumSpeedBoost = 0.05f;
    [SerializeField] private float hugeSpeedBoost = 0.25f;
    [SerializeField] private float outputBoost = 0.05f;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgradeID)
        {
            case "IND_2_1":
                milkSpeedMult += smallSpeedBoost;
                break;
            case "IND_2_2":
                cheeseSpeedMult += smallSpeedBoost;
                break;
            case "IND_2_3":
                cheeseOutputMult += outputBoost;
                break;
            case "IND_3":
                BuildingUnlockManager.Instance.UnlockBuilding("SoftCheeseFactory");
                break;
            case "IND_4_1":
                milkSpeedMult += mediumSpeedBoost;
                break;
            case "IND_4_2":
                milkOutputMult += outputBoost;
                break;
            case "IND_4_3":
                cheeseSpeedMult += mediumSpeedBoost;
                break;
            case "IND_5":
                milkSpeedMult += hugeSpeedBoost;
                break;
            case "IND_6_1":
                BuildingUnlockManager.Instance.UnlockBuilding("MilkTank");
                break;
            case "IND_6_2":
                BuildingUnlockManager.Instance.UnlockBuilding("SpecialtyCheeseFactory");
                break;
        }
    }
}