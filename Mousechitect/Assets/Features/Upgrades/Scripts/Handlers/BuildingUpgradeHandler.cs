using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract Base Class for the upgrade system,  meaning this script should not be attached to building objects. Attach ResidentialUpgradeHandler, CommercialUpgradeHandler etc instead.
/// Handles the 'spending' logic: checking costs against resource availability via the ResourceManager.
/// Also ensures prerequisites in the upgrade tree are met before an upgrade is purchasable.
/// </summary>
public abstract class BuildingUpgradeHandler : MonoBehaviour
{
    [Header("Config")]
    public List<UpgradeDefinition> availableUpgrades;
    protected HashSet<string> unlockedUpgradeIDs = new HashSet<string>();
    protected abstract void ApplyUpgradeEffect(UpgradeDefinition upgrade);

    public bool TryPurchaseUpgrade(UpgradeDefinition upgrade)
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager not present in scene!");
            return false;
        }

        if (!CanUnlock(upgrade))
        {
            Debug.Log("Prerequisite upgrade(s) missing or wanted upgrade already owned.");
            return false;
        }

        if (ResourceManager.Instance.CanAfford(upgrade.scrapCost, upgrade.cheeseCost))
        {
            ResourceManager.Instance.SpendResources(upgrade.scrapCost, upgrade.cheeseCost);
            unlockedUpgradeIDs.Add(upgrade.upgradeID);
            ApplyUpgradeEffect(upgrade);
            Debug.Log($"Purchased: {upgrade.upgradeName}");
            return true;
        }

        Debug.Log("Not enough resources.");
        return false;
    }

    public bool CanUnlock(UpgradeDefinition upgrade)
    {
        if (unlockedUpgradeIDs.Contains(upgrade.upgradeID)) return false;
        if (upgrade.requiredPrerequisite != null)
        {
            if (!unlockedUpgradeIDs.Contains(upgrade.requiredPrerequisite.upgradeID)) return false;
        }
        return true;
    }
}
