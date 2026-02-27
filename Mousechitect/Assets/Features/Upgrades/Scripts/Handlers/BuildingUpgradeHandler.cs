using UnityEngine;
using System.Collections.Generic;

// Updated by Iain Benner 02/02/2026
// Hani Hailston 14/12/2025

/// <summary>
/// Abstract Base Class for the upgrade system, so attach ResidentialUpgradeHandler, CommercialUpgradeHandler etc. to buildings instead.
/// Handles spending logic e.g checking costs against resource availability using the ResourceManager script.
/// Ensures any prior upgrades are applied before a new upgrade is purchasable.
/// </summary>

public abstract class BuildingUpgradeHandler : MonoBehaviour
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

        if (ResourceManager.instance.CanAfford(upgrade.scrap_cost, upgrade.type, upgrade.cheese_amount))
        {
            ResourceManager.instance.SpendResources(upgrade.scrap_cost, upgrade.type, upgrade.cheese_amount);

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

    /*
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

    */
    protected abstract void ApplyUpgradeEffect(UpgradeDefinition upgrade);

    public List<string> GetUnlockedUpgrades()
    {
        return new List<string>(UnlockedUpgradeIDs);
    }

    public void RestoreUnlockedUpgrades(List<string> saved_upgrades)
    {
        if (saved_upgrades != null)
        {
            UnlockedUpgradeIDs = new HashSet<string>(saved_upgrades);
        }
    }
}
