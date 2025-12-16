using UnityEngine;
using System.Collections.Generic;

//// Hani Hailston 14/12/2025

/// <summary>
/// Abstract Base Class for the upgrade system, meaning this script should not be attached to building objects. 
/// Attach ResidentialUpgradeHandler, CommercialUpgradeHandler etc instead.
/// Handles the 'spending' logic: checking costs against resource availability via the ResourceManager.
/// Also ensures prerequisites in the upgrade tree are met before an upgrade is purchasable.
/// </summary>

public abstract class BuildingUpgradeHandler : MonoBehaviour, ISaveable
{
    [Header("Save Data")]
    public string unique_id;
    public int prefab_index;

    [Header("Config")]
    public List<UpgradeDefinition> AvailableUpgrades;

    protected HashSet<string> UnlockedUpgradeIDs = new HashSet<string>();

    public bool TryPurchaseUpgrade(UpgradeDefinition upgrade)
    {
        if (ResourceManager.instance == null)
        {
            Debug.LogError("ResourceManager not present in scene!");
            return false;
        }

        if (!CanUnlock(upgrade))
        {
            Debug.Log("Prerequisite upgrade(s) missing or wanted upgrade already owned.");
            return false;
        }

        if (ResourceManager.instance.CanAfford(upgrade.scrap_cost, upgrade.cheese_cost))
        {
            ResourceManager.instance.SpendResources(upgrade.scrap_cost, upgrade.cheese_cost);

            UnlockedUpgradeIDs.Add(upgrade.upgrade_id);

            ApplyUpgradeEffect(upgrade);

            Debug.Log($"Purchased: {upgrade.upgrade_name}");

            return true;
        }

        Debug.Log("Not enough resources.");
        return false;
    }

    public bool CanUnlock(UpgradeDefinition upgrade)
    {
        if (UnlockedUpgradeIDs.Contains(upgrade.upgrade_id))
        {
            return false;
        }

        if (upgrade.required_prerequisite != null)
        {
            if (!UnlockedUpgradeIDs.Contains(upgrade.required_prerequisite.upgrade_id))
            {
                return false;
            }
        }

        return true;
    }

    public void PopulateSaveData(GameData data)
    {
        building_save_data saveData = new building_save_data();

        saveData.unique_id = this.unique_id;
        saveData.prefab_index = this.prefab_index;
        saveData.position = transform.position;
        saveData.rotation = transform.rotation;
        saveData.occupied_cells = new List<Vector2Int>();

        data.building_data.buildings.Add(saveData);
    }

    public void LoadFromSaveData(GameData data)
    {
        foreach (var savedBuilding in data.building_data.buildings)
        {
            if (savedBuilding.unique_id == this.unique_id)
            {
                transform.position = savedBuilding.position;
                transform.rotation = savedBuilding.rotation;

                return;
            }
        }
    }

    protected abstract void ApplyUpgradeEffect(UpgradeDefinition upgrade);
}
