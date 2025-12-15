using UnityEngine;

// Allows for the creation of new building upgrades from the Unity Editor right-click menu.
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "NewUpgrade")]

public class UpgradeDefinition : ScriptableObject
{
    [Header("General Info")]
    public string upgradeID;
    public string upgradeName;
    [TextArea] public string description;

    [Header("Costs")]
    public int scrapCost;
    public int cheeseCost;

    [Header("Requirements")]
    public UpgradeDefinition requiredPrerequisite;

    [Header("Icon")]
    public Sprite icon;
}