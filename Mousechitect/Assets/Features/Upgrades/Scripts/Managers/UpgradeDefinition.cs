using UnityEngine;

// Updated by Iain Benner 02/02/2026
// Hani Hailston 10/12/2025

/// <summary>
/// Allows for the creation of new building upgrades from the Unity Editor right-click menu.
/// </summary>

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "NewUpgrade")]

public class UpgradeDefinition : ScriptableObject
{
    [Header("General Info")]
    public string upgrade_id;
    public string upgrade_name;

    [TextArea]
    public string description;

    [Header("Costs")]
    public int scrap_cost = 0;
    public CheeseTypes type = CheeseTypes.AmericanCheese;
    public int cheese_amount = 0;

    [Header("Requirements")]
    public UpgradeDefinition required_prerequisite;

    [Header("Icon")]
    public Sprite icon;
}
