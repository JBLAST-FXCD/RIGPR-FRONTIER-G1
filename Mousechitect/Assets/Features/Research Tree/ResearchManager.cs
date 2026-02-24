using System.Collections;
using System.Collections.Generic;
using UImGui;
using UnityEngine;

public class ResearchManager : MonoBehaviour, ISaveable
{
    public static ResearchManager instance;

    private HashSet<string> unlocked_research_ids = new HashSet<string>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public bool IsUnlocked(string upgrade_id)
    {
        if (string.IsNullOrEmpty(upgrade_id)) return true;
        return unlocked_research_ids.Contains(upgrade_id);
    }

    public bool AttemptResearch(UpgradeDefinition research_node)
    {
        if (ResourceManager.instance != null) return false;

        if (!IsUnlocked(research_node.required_prerequisite.upgrade_id))
        {
            DebugWindow.LogToConsole($"Prerequisite for research not met", true);
            return false;
        }

        if (ResourceManager.instance.CanAfford(research_node.scrap_cost, research_node.type, research_node.cheese_amount))
        {
            ResourceManager.instance.SpendResources(research_node.scrap_cost, research_node.type, research_node.cheese_amount);

            unlocked_research_ids.Add(research_node.upgrade_id);

            if (BuildingUnlockManager.instance != null)
            {
                BuildingUnlockManager.instance.UnlockBuilding(research_node.upgrade_id);
            }

            DebugWindow.LogToConsole($"Successfully researched: {research_node.upgrade_name}", true);
            return true;
        }

        DebugWindow.LogToConsole($"Cannot afford research", true);
        return false;
    }

    public void PopulateSaveData(GameData data)
    {
        data.research_data.completed_research = new List<string>(unlocked_research_ids);
    }

    public void LoadFromSaveData(GameData data)
    {
        unlocked_research_ids = new HashSet<string>(data.research_data.completed_research);
    }
}
