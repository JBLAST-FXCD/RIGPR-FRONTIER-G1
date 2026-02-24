using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UImGui;

//// Hani Hailston 15/12/2025

/// <summary>
/// This script will link the UI System to each building upgrade handler.
/// For now, functionality stops at capturing upgrade unlock commands and logging them.
/// </summary>
/// Updated 24/02/2026 - Jess Batey

public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager instance;

    [System.Serializable]
    public struct BuildingUIUnlock
    {
        [Tooltip("The ID used in the research tree)")]
        public string building_id;

        [Tooltip("The UI button Gameobject in Panel")]
        public GameObject build_menu_button;
    }

    [Header("Build Menu bindings")]
    public List<BuildingUIUnlock> unlockable_buildings;

    private HashSet<string> unlocked_building_ids = new HashSet<string>();

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

    private void Start()
    {
        RefreshBuildMenuUI();
    }

    public void UnlockBuilding(string building_id)
    {
        if (!unlocked_building_ids.Contains(building_id))
        {
            unlocked_building_ids.Add(building_id);
            DebugWindow.LogToConsole($"Building Unlocked: {building_id}", true);
            RefreshBuildMenuUI();
        }
    }

    public bool IsBuildingUnlocked(string building_id)
    {
        return unlocked_building_ids.Contains(building_id);
    }

    private void RefreshBuildMenuUI()
    {
        foreach (var  mapping in unlockable_buildings)
        {
            if (mapping.build_menu_button != null)
            {
                bool is_unlocked = unlocked_building_ids.Contains(mapping.building_id);
                mapping.build_menu_button.SetActive(is_unlocked);
            }
        }
    }
}
