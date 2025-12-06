using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Anthony Grummett - 5/12/25

/// <summary>
/// Shows and hides build-related UI while build mode is active.
/// </summary>
public class BuildModeUI : MonoBehaviour
{
    [SerializeField] private BuildingManager building_manager;
    [SerializeField] private GameObject build_panel;   // Palette of building buttons
    [SerializeField] private GameObject hints_panel;   // Optional: bottom hint bar

    private bool is_last_build_mode_state = false;

    private void Start()
    {
        UpdatePanels(false);
    }

    private void Update()
    {
        if (building_manager == null)
        {
            return;
        }

        bool is_build_mode_active = building_manager.GetIsBuildModeActive();

        if (is_build_mode_active != is_last_build_mode_state)
        {
            UpdatePanels(is_build_mode_active);
            is_last_build_mode_state = is_build_mode_active;
        }
    }

    private void UpdatePanels(bool is_active)
    {
        if (build_panel != null)
        {
            build_panel.SetActive(is_active);
        }

        if (hints_panel != null)
        {
            hints_panel.SetActive(is_active);
        }
    }
}